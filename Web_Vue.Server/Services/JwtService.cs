using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Web_Vue.Server.ViewModels.Auth;
using Web_Vue.Server.Tools;

namespace Web_Vue.Server.Services;

/// <summary> 使用 HMAC-SHA256 簽章產生 JWT Token </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc/>
    public string GenerateToken(UserInfoViewModel userInfo)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey 未設定");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
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

        var expireMinutes = int.TryParse(_configuration["JwtSettings:ExpireMinutes"], out var m) ? m : 60;

        var token = new JwtSecurityToken(
            issuer:             _configuration["JwtSettings:Issuer"],
            audience:           _configuration["JwtSettings:Audience"],
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
