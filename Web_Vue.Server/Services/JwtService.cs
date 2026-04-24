using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Web_Vue.Server.ViewModels.Auth;
using Web_Vue.Server.ViewModels.Base;
using Web_Vue.Server.Tools;

namespace Web_Vue.Server.Services;

/// <summary> 使用 HMAC-SHA256 簽章產生 JWT Token </summary>
public class JwtService
{
    private readonly JwtSettings _jwtSettings;

    public JwtService(IOptions<AppSettings> appOptions)
    {
        _jwtSettings = appOptions.Value.JwtSettings;
    }

    /// <summary> 依據使用者資訊模型產生 JWT Token </summary>
    public string GenerateToken(UserInfoViewModel userInfo)
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("JwtSettings:SecretKey 未設定");
        }

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roleListText = string.Join(",", (userInfo.RoleList ?? []).Distinct());

        var claims = new[]
        {
            new Claim(UserInfoTool.UserIdClaimType,        userInfo.ID.ToString()),
            new Claim(UserInfoTool.AccountClaimType,       userInfo.Account ?? string.Empty),
            new Claim(UserInfoTool.NameClaimType,          userInfo.Name ?? string.Empty),
            new Claim(UserInfoTool.RoleListClaimType,      roleListText),
            new Claim(UserInfoTool.IsAdminClaimType,       userInfo.IsAdmin.ToString()),
            new Claim(UserInfoTool.IsMaintainingClaimType, userInfo.IsMaintaining.ToString()),
            new Claim(ClaimTypes.NameIdentifier,           userInfo.ID.ToString()),
            new Claim(ClaimTypes.Name,                     userInfo.Name ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Sub,         userInfo.ID.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,         Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,         DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                      ClaimValueTypes.Integer64),
        };

        var token = new JwtSecurityToken(
            issuer:             _jwtSettings.Issuer,
            audience:           _jwtSettings.Audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
