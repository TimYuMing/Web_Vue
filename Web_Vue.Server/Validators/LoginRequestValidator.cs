using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Web_Vue.Server.Helpers;
using Web_Vue.Server.Models;
using Web_Vue.Server.Models.Entities;
using Web_Vue.Server.Services;
using Web_Vue.Server.ViewModels.Auth;

namespace Web_Vue.Server.Validators;

/// <summary> 登入請求驗證器 </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(
        IStringLocalizer<SharedResource> resx,
        LoginChallengeService loginChallengeService,
        LoginAttemptService loginAttemptService,
        IHttpContextAccessor httpContextAccessor,
        DbEntityContext db)
    {
        // ── 必填欄位 ────────────────────────────────────────────────────────

        RuleFor(x => x.Account)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(resx["ValidMessage_VM_{0}_Required", resx["Txt_帳號"].Value].Value);

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(resx["ValidMessage_VM_{0}_Required", resx["Txt_密碼"].Value].Value);

        RuleFor(x => x.ChallengeId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(resx["ValidMessage_Login_ChallengeId_Required"].Value);

        RuleFor(x => x.ValidCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(resx["ValidMessage_VM_{0}_Required", resx["Txt_驗證碼"].Value].Value);

        // ── 圖形驗證碼驗證 ──────────────────────────────────────────────────

        RuleFor(x => x)
            .Custom((model, context) =>
            {
                if (string.IsNullOrEmpty(model.ChallengeId) || string.IsNullOrEmpty(model.ValidCode))
                    return;

                if (!loginChallengeService.ValidateAndConsume(model.ChallengeId, model.ValidCode))
                    context.AddFailure(nameof(model.ValidCode),
                        resx["ValidMessage_ValidCode_驗證碼錯誤"].Value);
            });

        // ── 帳號鎖定判斷 ────────────────────────────────────────────────────

        RuleFor(x => x)
            .Custom((model, context) =>
            {
                if (string.IsNullOrEmpty(model.Account)) return;

                var ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                if (loginAttemptService.IsLocked(model.Account, ip))
                    context.AddFailure(nameof(model.Password),
                        resx["ValidMessage_Login_已連續輸入錯誤 5 次，請 15 分鐘後再試"].Value);
            });

        // ── 帳號密碼驗證 + 帳號狀態 ─────────────────────────────────────────

        RuleFor(x => x)
            .CustomAsync(async (model, context, ct) =>
            {
                if (string.IsNullOrEmpty(model.Account) || string.IsNullOrEmpty(model.Password))
                    return;

                var ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                var userAccount = await db.UserAccount
                    .FirstOrDefaultAsync(u => u.Account == model.Account, ct);

                if (userAccount == null || !PasswordHelper.VerifyPassword(model.Password, userAccount.Password))
                {
                    loginAttemptService.RegisterFailure(model.Account, ip);
                    context.AddFailure(nameof(model.Password),
                        resx["ValidMessage_Login_帳號或密碼錯誤，若連續錯誤 5 次，將暫停 15 分鐘禁止登入"].Value);
                    return;
                }

                var profile = await db.UserProfile
                    .FirstOrDefaultAsync(p => p.ID == userAccount.ID, ct);

                if (profile == null)
                {
                    loginAttemptService.RegisterFailure(model.Account, ip);
                    context.AddFailure(nameof(model.Account),
                        resx["ValidMessage_Login_帳號或密碼錯誤，若連續錯誤 5 次，將暫停 15 分鐘禁止登入"].Value);
                    return;
                }

                if (profile.Status == UserProfileStatus.Disable)
                    context.AddFailure(nameof(model.Account),
                        resx["ValidMessage_Login_帳號已停用"].Value);
            });
    }
}
