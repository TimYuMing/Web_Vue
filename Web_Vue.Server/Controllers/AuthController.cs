using Web_Vue.Server.Entities;
using Web_Vue.Server.Helpers;
using Web_Vue.Server.Models.Auth;
using Web_Vue.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace Web_Vue.Server.Controllers;

/// <summary>
/// 身份驗證控制器 — 登入、取得 JWT Token
/// </summary>
public class AuthController : BaseController
{
    private readonly DbEntityContext _db;
    private readonly IJwtService  _jwtService;
    private readonly IConfiguration _configuration;

    public AuthController(DbEntityContext db, IJwtService jwtService, IConfiguration configuration)
    {
        _db            = db;
        _jwtService    = jwtService;
        _configuration = configuration;
    }

    /// <summary>
    /// 使用者登入，成功後回傳 JWT Token
    /// </summary>
    /// <param name="request">帳號密碼</param>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Account) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "帳號與密碼不可空白" });

        var userAccount = await _db.UserAccounts
            .FirstOrDefaultAsync(u => u.Account == request.Account);

        if (userAccount is null || !PasswordHelper.VerifyPassword(request.Password, userAccount.Password))
            return Unauthorized(new { message = "帳號或密碼錯誤" });

        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(p => p.ID == userAccount.ID);

        if (profile is null)
            return Unauthorized(new { message = "帳號或密碼錯誤" });

        var roleIds = await _db.RoleInUserAccounts
            .Where(r => r.UserAccountID == userAccount.ID)
            .Select(r => r.RoleID)
            .ToListAsync();

        var roles = await _db.Roles
            .Where(r => roleIds.Contains(r.ID) && r.IsEnable)
            .ToListAsync();

        var userInfo = new UserInfoModel
        {
            ID                    = userAccount.ID,
            Account               = userAccount.Account,
            Name                  = profile.Name,
            Status                = profile.Status,
            LatestLoginDate       = userAccount.LatestLoginDate,
            IsTemplatePassword    = userAccount.IsTemplatePassword,
            ValidPasswordExpireTime = userAccount.ValidPasswordExpireTime,
            RoleList              = roleIds,
            IsAdmin               = roles.Any(r => r.IsAdmin),
            IsMaintaining         = roles.Any(r => r.IsMaintaining),
        };

        var token = _jwtService.GenerateToken(userInfo);
        var expireMinutes = int.TryParse(_configuration["JwtSettings:ExpireMinutes"], out var m) ? m : 60;

        return Ok(new LoginResponse
        {
            Token         = token,
            ExpireMinutes = expireMinutes,
            Account       = userInfo.Account,
            Name          = userInfo.Name,
            IsAdmin       = userInfo.IsAdmin,
            RoleList      = userInfo.RoleList,
        });
    }

    /// <summary>
    /// 取得當前登入使用者資訊 (需攜帶 JWT)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me([FromServices] ICurrentUserService currentUser)
    {
        var info = currentUser.UserInfo;
        if (info is null)
            return Unauthorized();

        return Ok(info);
    }
}
