using System.Net;
using Web_Vue.Server.ViewModels.Base;

namespace Web_Vue.Server._Middleware;

/// <summary>
/// CSRF 防護中介層 — Double Submit Cookie Pattern
///
/// 除安全方法 (GET/HEAD/OPTIONS) 及豁免路徑外，所有請求均需同時攜帶：
///   Cookie : <see cref="Config.CsrfCookieName"/>  (非 HttpOnly，由 JS 讀取)
///   Header : <see cref="Config.CsrfHeaderName"/>  (由 JS 在每次請求時帶入)
/// 兩值必須完全相同，否則回傳 403。
///
/// 豁免路徑（使用者尚未登入，無法取得 CSRF Token）：
///   POST /api/auth/login
///   GET  /api/auth/captcha
/// </summary>
public class CsrfValidationMiddleware(RequestDelegate next)
{

    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
        { "GET", "HEAD", "OPTIONS" };

    private static readonly HashSet<string> ExemptPaths = new(StringComparer.OrdinalIgnoreCase)
        { "/api/auth/login", "/api/auth/captcha" };

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path   = context.Request.Path.Value ?? string.Empty;

        if (!SafeMethods.Contains(method) && !ExemptPaths.Contains(path))
        {
            var tokenFromHeader = context.Request.Headers[Config.CsrfHeaderName].FirstOrDefault();
            var tokenFromCookie = context.Request.Cookies[Config.CsrfCookieName];

            if (string.IsNullOrWhiteSpace(tokenFromHeader)
                || string.IsNullOrWhiteSpace(tokenFromCookie)
                || !string.Equals(tokenFromHeader, tokenFromCookie, StringComparison.Ordinal))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsJsonAsync(new ResponseViewModel
                {
                    Status  = ResultType.Fail,
                    Message = "CSRF 驗證失敗，請重新整理頁面後再試"
                });
                return;
            }
        }

        await next(context);
    }
}
