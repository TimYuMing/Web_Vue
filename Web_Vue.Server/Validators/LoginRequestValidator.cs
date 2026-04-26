using Web_Vue.Server.Models;
using Web_Vue.Server.Services;
using Web_Vue.Server.Tools;
using Web_Vue.Server.ViewModels.Auth;

namespace Web_Vue.Server.Validators;

/// <summary> 登入請求驗證器 </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequestViewModel>
{
    public LoginRequestValidator(
        IStringLocalizer<SharedResource> _resx,
        AuthService _authService,
        IHttpContextAccessor _httpContextAccessor,
        DbEntityContext _dbContext)
    {
        // ── 必填欄位 ────────────────────────────────────────────────────────

        RuleFor(x => x.Account)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(_resx["ValidMessage_VM_{0}_Required", _resx["Txt_帳號"].Value].Value);

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(_resx["ValidMessage_VM_{0}_Required", _resx["Txt_密碼"].Value].Value);

        RuleFor(x => x.ChallengeId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(_resx["ValidMessage_Login_ChallengeId_Required"].Value);

        RuleFor(x => x.ValidCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(_resx["ValidMessage_VM_{0}_Required", _resx["Txt_驗證碼"].Value].Value);

        // ── 圖形驗證碼 + 帳號鎖定 + 帳密驗證 + 帳號狀態 ──────────────────────

        RuleFor(x => x)
            .CustomAsync(async (model, context, _) =>
            {
                // 圖形驗證碼
                if (!string.IsNullOrEmpty(model.ChallengeId) && !string.IsNullOrEmpty(model.ValidCode))
                {
                    if (!_authService.ValidateAndConsume(model.ChallengeId, model.ValidCode))
                    {
                        context.AddFailure(nameof(model.ValidCode), _resx["ValidMessage_ValidCode_驗證碼错誤"].Value);
                    }
                }

                if (string.IsNullOrEmpty(model.Account))
                {
                    return;
                }

                var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                // 帳號鎖定
                if (_authService.IsLocked(model.Account, ip))
                {
                    context.AddFailure(nameof(model.Password), _resx["ValidMessage_Login_已連續輸入錯誤 5 次，請 15 分鐘後再試"].Value);
                }

                if (string.IsNullOrEmpty(model.Password))
                {
                    return;
                }

                // 帳號密碼驗證
                var userAccount = await _dbContext.UserAccount.FirstOrDefaultAsync(u => u.Account == model.Account);

                if (userAccount == null || !BCryptTool.VerifyPassword(model.Password, userAccount.Password))
                {
                    _authService.RegisterFailure(model.Account, ip);
                    context.AddFailure(nameof(model.Password), _resx["ValidMessage_Login_帳號或密碼錯誤，若連續錯誤 5 次，將暫停 15 分鐘禁止登入"].Value);
                    return;
                }

                // 帳號狀態
                var profile = await _dbContext.UserProfile.FirstOrDefaultAsync(p => p.ID == userAccount.ID);

                if (profile == null)
                {
                    _authService.RegisterFailure(model.Account, ip);
                    context.AddFailure(nameof(model.Account), _resx["ValidMessage_Login_帳號或密碼錯誤，若連續錯誤 5 次，將暫停 15 分鐘禁止登入"].Value);
                    return;
                }

                if (profile.Status == UserProfileStatus.Disable)
                {
                    context.AddFailure(nameof(model.Account), _resx["ValidMessage_Login_帳號已停用"].Value);
                }
            });
    }
}
