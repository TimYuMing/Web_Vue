using System.Text;

namespace Web_Vue.Server.Tools;

/// <summary> 圖形驗證碼工具 — 以 SVG 實作，無需外部套件 </summary>
public static class CaptchaTool
{
    private const string Digits = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ"; // 排除易混淆字元

    /// <summary> 產生隨機驗證碼字串 </summary>
    public static string GenerateCode(int length = 4)
    {
        var rng = Random.Shared;
        return new string(Enumerable.Range(0, length)
            .Select(_ => Digits[rng.Next(Digits.Length)])
            .ToArray());
    }

    /// <summary>
    /// 產生 SVG 驗證碼圖片並回傳 data URI（base64 encoded SVG）
    /// </summary>
    public static string GenerateSvgBase64(string code)
    {
        var rng = new Random();
        var sb  = new StringBuilder(512);

        sb.Append("<svg xmlns='http://www.w3.org/2000/svg' width='120' height='40'>");
        sb.Append("<rect width='120' height='40' rx='4' fill='#f5f5f5'/>");

        // 背景干擾線
        for (var i = 0; i < 6; i++)
        {
            var (r, g, b) = (rng.Next(180, 230), rng.Next(180, 230), rng.Next(180, 230));
            sb.Append($"<line x1='{rng.Next(0, 120)}' y1='{rng.Next(0, 40)}' " +
                      $"x2='{rng.Next(0, 120)}' y2='{rng.Next(0, 40)}' " +
                      $"stroke='rgb({r},{g},{b})' stroke-width='1.5'/>");
        }

        // 干擾點
        for (var i = 0; i < 20; i++)
        {
            var (r, g, b) = (rng.Next(100, 200), rng.Next(100, 200), rng.Next(100, 200));
            sb.Append($"<circle cx='{rng.Next(0, 120)}' cy='{rng.Next(0, 40)}' r='1' " +
                      $"fill='rgb({r},{g},{b})'/>");
        }

        // 文字
        for (var i = 0; i < code.Length; i++)
        {
            var x        = 16 + i * 24 + rng.Next(-2, 2);
            var y        = 27 + rng.Next(-3, 3);
            var rotation = rng.Next(-18, 18);
            var (r, g, b) = (rng.Next(20, 100), rng.Next(20, 100), rng.Next(20, 100));
            sb.Append($"<text x='{x}' y='{y}' transform='rotate({rotation},{x},{y})' " +
                      $"font-size='21' font-family='monospace' font-weight='bold' " +
                      $"fill='rgb({r},{g},{b})'>{code[i]}</text>");
        }

        sb.Append("</svg>");

        var bytes  = Encoding.UTF8.GetBytes(sb.ToString());
        var base64 = Convert.ToBase64String(bytes);
        return $"data:image/svg+xml;base64,{base64}";
    }
}
