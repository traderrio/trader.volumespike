using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using Trader.VolumeSpike.Common.Configuration;

namespace Trader.VolumeSpike.Controllers
{
	[Route("api/[controller]")]
	public class WhoAmiController : Controller
	{
		private readonly IOptions<AppSettings> _appSettings;

		public WhoAmiController(IOptions<AppSettings> appSettings)
		{
			_appSettings = appSettings;
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			Log.Logger.Warning("accessing api/whomi");

			var response = new
			{
				LocalTime = DateTime.UtcNow.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt"),
				UtcTime = DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm tt"),
				DataProcessingSetting = _appSettings.Value.DataProcessing,
			};
			return Ok(response);
		}
	}
}