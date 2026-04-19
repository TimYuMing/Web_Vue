using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Web_Vue.Server._Middleware;
using Web_Vue.Server.Models;
using Web_Vue.Server.Models.Entities;
using Web_Vue.Server.Services;
using Web_Vue.Server.ViewModels.Auth;

namespace Web_Vue.Server.Controllers;

/// <summary> 身份驗證 API — 登入、登出、Session 查詢、Token 刷新 </summary>
public class AuthController(
    DbEntityContext db,
    IJwtService jwtService,
    IConfiguration configuration,
    ICurrentUserService currentUserService,
    LoginChallengeService loginChallengeService,
    LoginAttemptService loginAttemptService,
    IStringLocalizer<SharedResource> resx) : BaseController
{
    private const string JwtCookieName = "jwt";

    private int ExpireMinutes =>
        int.TryParse(configuration["JwtSettings:ExpireMinutes"], out var m) ? m : 60;

    // ===================== 驗證碼 =====================

    /// <summary> 取得登入圖形驗證碼 </summary>
    [AllowAnonymous]
    [HttpGet("captcha")]
    public IActionResult GetCaptcha()
    {
        var captcha = loginChallengeService.Create();
        return Ok(new ResponseViewModel { Status = ResultType.Success, Data = captcha });
    }

    // ===================== 登入 =====================

    /// <summary> 帳號密碼登入，成功後將 JWT 寫入 HttpOnly Cookie </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IValidator<LoginRequest> validator)
    {
        // ── 輸入驗證（含必填、圖形驗證碼、鎖定、帳密、帳號狀態） ──
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errorList = validation.Errors
                .GroupBy(e => e.PropertyName)
                .Select(g => new ResponseErrorDataViewModel
                {
                    Key           = g.Key,
                    ErrorTextList = g.Select(e => e.ErrorMessage).ToList()
                })
                .ToList();
            return Ok(LoginFail(LoginFailType.驗證失敗, errorList: errorList));
        }

        // ── 載入帳號資料（驗證已通過，帳號必定存在） ──
        var clientIp    = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAccount = await db.UserAccounts.FirstOrDefaultAsync(u => u.Account == request.Account);
        var profile     = await db.UserProfiles.FirstOrDefaultAsync(p => p.ID == userAccount!.ID);

        // ── 暫時密碼 ──
        if (userAccount!.IsTemplatePassword)
        {
            return Ok(LoginFail(LoginFailType.使用暫時密碼,
                message: resx["RedirectMessage_ValidCode_請進行重設密碼"].Value,
                redirectPath: "/change-password"));
        }

        // ── 密碼過期 ──
        if (userAccount.ValidPasswordExpireTime.HasValue
            && userAccount.ValidPasswordExpireTime.Value < DateTime.Now)
        {
            return Ok(LoginFail(LoginFailType.密碼過期,
                message: resx["RedirectMessage_ValidCode_密碼已過期，請進行重設密碼申請"].Value,
                redirectPath: "/forget-password"));
        }

        // ── 登入成功 ──
        loginAttemptService.Reset(request.Account, clientIp);
        userAccount.LatestLoginDate = DateTime.Now;
        await db.SaveChangesAsync();

        var userInfo = await BuildUserInfoAsync(userAccount, profile!);
        var token    = jwtService.GenerateToken(userInfo);
        var expires  = DateTime.UtcNow.AddMinutes(ExpireMinutes);
        var session  = await BuildSessionAsync(userInfo);
        session.ExpiresAt = expires;

        SetAuthCookies(token, expires);

        return Ok(new ResponseViewModel { Status = ResultType.Success, Data = session });
    }

    // ===================== 登出 =====================

    /// <summary> 登出 — 清除 Cookie </summary>
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        ClearAuthCookies();
        return Ok(new ResponseViewModel { Status = ResultType.Success, Message = "已成功登出" });
    }

    // ===================== Me =====================

    /// <summary> 取得目前登入者 session（前端重整後恢復狀態用） </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var claims = currentUserService.UserInfo;
        if (claims == null)
            return Unauthorized(new ResponseViewModel
                { Status = ResultType.Fail, Message = resx["SysMsg_VerifyError_未登入"].Value });

        var (userAccount, profile) = await GetUserByAccountAsync(claims.Account);
        if (userAccount == null || profile == null)
            return Unauthorized(new ResponseViewModel
                { Status = ResultType.Fail, Message = resx["SysMsg_VerifyError_未登入"].Value });

        var userInfo = await BuildUserInfoAsync(userAccount, profile);
        var session  = await BuildSessionAsync(userInfo);

        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
        session.ExpiresAt = long.TryParse(expClaim, out var exp)
            ? DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime
            : null;

        return Ok(new ResponseViewModel { Status = ResultType.Success, Data = session });
    }

    // ===================== Refresh =====================

    /// <summary> 延長登入 session — 重新簽發 JWT Cookie </summary>
    [Authorize]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var claims = currentUserService.UserInfo;
        if (claims == null)
            return Unauthorized(new ResponseViewModel
                { Status = ResultType.Fail, Message = resx["SysMsg_VerifyError_未登入"].Value });

        var (userAccount, profile) = await GetUserByAccountAsync(claims.Account);
        if (userAccount == null || profile == null)
            return Unauthorized(new ResponseViewModel
                { Status = ResultType.Fail, Message = resx["SysMsg_VerifyError_未登入"].Value });

        var userInfo = await BuildUserInfoAsync(userAccount, profile);
        var token    = jwtService.GenerateToken(userInfo);
        var expires  = DateTime.UtcNow.AddMinutes(ExpireMinutes);
        var session  = await BuildSessionAsync(userInfo);
        session.ExpiresAt = expires;

        SetAuthCookies(token, expires);

        return Ok(new ResponseViewModel { Status = ResultType.Success, Data = session });
    }

    // ===================== Private Helpers =====================

    private async Task<(UserAccount? account, UserProfile? profile)> GetUserByAccountAsync(string account)
    {
        var ua = await db.UserAccounts.FirstOrDefaultAsync(u => u.Account == account);
        var profile = ua != null
            ? await db.UserProfiles.FirstOrDefaultAsync(p => p.ID == ua.ID)
            : null;
        return (ua, profile);
    }

    private async Task<UserInfoViewModel> BuildUserInfoAsync(UserAccount userAccount, UserProfile profile)
    {
        var roleIds = await db.RoleInUserAccounts
            .Where(r => r.UserAccountID == userAccount.ID)
            .Select(r => r.RoleID)
            .ToListAsync();

        var roles = await db.Roles
            .Where(r => roleIds.Contains(r.ID) && r.IsEnable)
            .ToListAsync();

        return new UserInfoViewModel
        {
            ID                      = userAccount.ID,
            Account                 = userAccount.Account,
            Name                    = profile.Name,
            Status                  = profile.Status,
            LatestLoginDate         = userAccount.LatestLoginDate,
            IsTemplatePassword      = userAccount.IsTemplatePassword,
            ValidPasswordExpireTime = userAccount.ValidPasswordExpireTime,
            RoleList                = roleIds,
            IsAdmin                 = roles.Any(r => r.IsAdmin),
            IsMaintaining           = roles.Any(r => r.IsMaintaining),
        };
    }

    private async Task<AuthSessionViewModel> BuildSessionAsync(UserInfoViewModel userInfo)
    {
        List<string> permissions;

        if (userInfo.IsAdmin)
        {
            permissions = await db.Permissions
                .Where(p => p.IsEnable)
                .Select(p => p.Code)
                .Distinct()
                .ToListAsync();
        }
        else if (userInfo.RoleList.Count > 0)
        {
            permissions = await (
                from pir in db.PermissionInRoles
                join p in db.Permissions on pir.PermissionID equals p.ID
                join r in db.Roles       on pir.RoleID         equals r.ID
                where userInfo.RoleList.Contains(pir.RoleID) && p.IsEnable && r.IsEnable
                select p.Code
            ).Distinct().ToListAsync();
        }
        else
        {
            permissions = [];
        }

        return new AuthSessionViewModel
        {
            User = new AuthUserViewModel
            {
                Id            = userInfo.ID,
                Account       = userInfo.Account,
                Name          = userInfo.Name,
                IsAdmin       = userInfo.IsAdmin,
                IsMaintaining = userInfo.IsMaintaining,
            },
            Permissions = permissions,
        };
    }

    /// <summary> 設定 HttpOnly JWT Cookie 及非 HttpOnly CSRF Cookie </summary>
    private void SetAuthCookies(string jwtToken, DateTime expiresUtc)
    {
        var env     = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var isHttps = !env.IsDevelopment();
        var expires = new DateTimeOffset(expiresUtc, TimeSpan.Zero);

        // JWT — HttpOnly，JS 無法讀取，防 XSS
        Response.Cookies.Append(JwtCookieName, jwtToken, new CookieOptions
        {
            HttpOnly = true,
            Secure   = isHttps,
            SameSite = SameSiteMode.Strict,
            Expires  = expires,
            Path     = "/",
        });

        // CSRF Token — 非 HttpOnly，由 JS 讀取後放入請求 Header
        var csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        Response.Cookies.Append(CsrfValidationMiddleware.CookieName, csrfToken, new CookieOptions
        {
            HttpOnly = false,
            Secure   = isHttps,
            SameSite = SameSiteMode.Strict,
            Expires  = expires,
            Path     = "/",
        });
    }

    /// <summary> 清除 JWT Cookie 及 CSRF Cookie </summary>
    private void ClearAuthCookies()
    {
        Response.Cookies.Delete(JwtCookieName,
            new CookieOptions { Path = "/", SameSite = SameSiteMode.Strict });
        Response.Cookies.Delete(CsrfValidationMiddleware.CookieName,
            new CookieOptions { Path = "/", SameSite = SameSiteMode.Strict });
    }

    private List<ResponseErrorDataViewModel> GetValidateErrorList()
        => ModelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .Select(kv => new ResponseErrorDataViewModel
            {
                Key = kv.Key,
                ErrorTextList = kv.Value!.Errors.Select(e => e.ErrorMessage).ToList()
            })
            .ToList();

    private static ResponseViewModel LoginFail(
        LoginFailType failType,
        string? message = null,
        List<ResponseErrorDataViewModel>? errorList = null,
        string? redirectPath = null)
        => new()
        {
            Status    = ResultType.Fail,
            Message   = message ?? string.Empty,
            ErrorList = errorList ?? [],
            Data      = new LoginFailData { FailType = failType, RedirectPath = redirectPath }
        };
}
