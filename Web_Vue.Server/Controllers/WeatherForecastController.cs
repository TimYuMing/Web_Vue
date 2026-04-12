using Microsoft.AspNetCore.Mvc;
using Web_Vue.Server.Enums;
using Web_Vue.Server.Models;

namespace Web_Vue.Server.Controllers
{
    /// <summary>
    /// 天氣預報控制器
    /// </summary>
    public class WeatherForecastController : BaseController
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="logger">Logger 實例</param>
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 取得天氣預報資料
        /// </summary>
        /// <returns>統一回應模型，資料內容為天氣預報清單</returns>
        [HttpGet(Name = "GetWeatherForecast")]
        public ActionResult<ResponseModel> Get()
        {
            var data = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            return Ok(new ResponseModel
            {
                Status = ResultType.Success,
                Data = data,
                Message = string.Empty
            });
        }
    }
}
