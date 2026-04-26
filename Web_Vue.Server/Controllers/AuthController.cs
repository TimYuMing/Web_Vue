using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Web_Vue.Server.Services;
using Web_Vue.Server.Tools;
using Web_Vue.Server.ViewModels.Auth;
using Web_Vue.Server.ViewModels.Base;

namespace Web_Vue.Server.Controllers;

/// <summary> 身份驗證 API — 登入、登出、Session 查詢、Token 刷新 </summary>
public class AuthController(
    UserService _userService,
    AuthService _authService,
    JwtService _jwtService,
    IOptions<AppSettings> _appOptions,
    CurrentUserService _currentUserService,
    IStringLocalizer<SharedResource> _resx) : BaseController
{
    private int ExpireMinutes => _appOptions.Value.JwtSettings.ExpireMinutes;

    // ===================== 驗證碼 =====================

    /// <summary> 取得登入圖形驗證碼 </summary>
    [AllowAnonymous]
    [HttpGet("captcha")]
    public IActionResult GetCaptcha()
    {
        var captcha = _authService.CreateChallenge();
        return Success(data: captcha);
    }

    // ===================== 登入 =====================

    /// <summary> 帳號密碼登入，成功後將 JWT 寫入 HttpOnly Cookie </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestViewModel request,
        [FromServices] IValidator<LoginRequestViewModel> validator)
    {
        // ── 輸入驗證（含必填、圖形驗證碼、鎖定、帳密、帳號狀態） ──
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return Fail(data: new LoginFailViewModel { FailType = LoginFailType.AuthFailed },
                        errorList: ValidationTool.BuildErrorList(validation));
        }

        // ── 載入帳號資料（驗證已通過，帳號必定存在） ──
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userInfo = await _userService.GetUserInfoByAccountAsync(request.Account);

        // ── 暫時密碼 ──
        if (userInfo!.IsTemplatePassword)
        {
            return Fail(_resx["RedirectMessage_ValidCode_請進行重設密碼"].Value,
                        data: new LoginFailViewModel { FailType = LoginFailType.UseTempPassword, RedirectPath = "/change-password" });
        }

        // ── 密碼過期 ──
        if (userInfo.ValidPasswordExpireTime.HasValue && userInfo.ValidPasswordExpireTime.Value < DateTime.Now)
        {
            return Fail(_resx["RedirectMessage_ValidCode_密碼已過期，請進行重設密碼申請"].Value,
                        data: new LoginFailViewModel { FailType = LoginFailType.PasswordExpired, RedirectPath = "/forget-password" });
        }

        // ── 登入成功 ──
        _authService.ResetAttempt(request.Account, clientIp);
        await _userService.RecordLoginAsync(userInfo.ID);

        var token = _jwtService.GenerateToken(userInfo);
        var expires = DateTime.UtcNow.AddMinutes(ExpireMinutes);
        var session = await _userService.BuildSessionAsync(userInfo);
        session.ExpiresAt = expires;

        SetAuthCookies(token, expires);

        return Success(data: session);
    }

    // ===================== 登出 =====================

    /// <summary> 登出 — 清除 Cookie </summary>
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        ClearAuthCookies();
        return Success("已成功登出");
    }

    // ===================== Me =====================

    /// <summary> 取得目前登入者 session（前端重整後恢復狀態用） </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var claims = _currentUserService.UserInfo;
        if (claims == null)
        {
            return Unauthorized(_resx["SysMsg_VerifyError_未登入"].Value);
        }

        var userInfo = await _userService.GetUserInfoByAccountAsync(claims.Account);
        if (userInfo == null)
        {
            return Unauthorized(_resx["SysMsg_VerifyError_未登入"].Value);
        }

        var session = await _userService.BuildSessionAsync(userInfo);

        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
        session.ExpiresAt = long.TryParse(expClaim, out var exp)
            ? DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime
            : null;

        return Success(data: session);
    }

    // ===================== Refresh =====================

    /// <summary> 延長登入 session — 重新簽發 JWT Cookie </summary>
    [Authorize]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var claims = _currentUserService.UserInfo;
        if (claims == null)
        {
            return Unauthorized(_resx["SysMsg_VerifyError_未登入"].Value);
        }

        var userInfo = await _userService.GetUserInfoByAccountAsync(claims.Account);
        if (userInfo == null)
        {
            return Unauthorized(_resx["SysMsg_VerifyError_未登入"].Value);
        }

        var token = _jwtService.GenerateToken(userInfo);
        var expires = DateTime.UtcNow.AddMinutes(ExpireMinutes);
        var session = await _userService.BuildSessionAsync(userInfo);
        session.ExpiresAt = expires;

        SetAuthCookies(token, expires);

        return Success(data: session);
    }

    // ===================== Private Helpers =====================

    /// <summary> 設定 HttpOnly JWT Cookie 及非 HttpOnly CSRF Cookie </summary>
    private void SetAuthCookies(string jwtToken, DateTime expiresUtc)
    {
        var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var isHttps = !env.IsDevelopment();
        var expires = new DateTimeOffset(expiresUtc, TimeSpan.Zero);

        // JWT — HttpOnly，JS 無法讀取，防 XSS
        Response.Cookies.Append(Config.JwtCookieName, jwtToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Strict,
            Expires = expires,
            Path = "/",
        });

        // CSRF Token — 非 HttpOnly，由 JS 讀取後放入請求 Header
        var csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        Response.Cookies.Append(Config.CsrfCookieName, csrfToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = isHttps,
            SameSite = SameSiteMode.Strict,
            Expires = expires,
            Path = "/",
        });
    }

    /// <summary> 清除 JWT Cookie 及 CSRF Cookie </summary>
    private void ClearAuthCookies()
    {
        Response.Cookies.Delete(Config.JwtCookieName,
            new CookieOptions { Path = "/", SameSite = SameSiteMode.Strict });
        Response.Cookies.Delete(Config.CsrfCookieName,
            new CookieOptions { Path = "/", SameSite = SameSiteMode.Strict });
    }
}