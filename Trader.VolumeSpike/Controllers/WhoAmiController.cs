using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike.Controllers
{
	[Route("api/[controller]")]
	public class WhoAmiController : Controller
	{
		private readonly IOptions<AppSettings> _appSettings;
		private readonly IVolumeRecordService _volumeRecordService;

		public WhoAmiController(IOptions<AppSettings> appSettings, IVolumeRecordService volumeRecordService)
		{
			_appSettings = appSettings;
			_volumeRecordService = volumeRecordService;
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			Log.Logger.Warning("accessing api/whomi");
			var lastVolumeRecord = await _volumeRecordService.LastVolumeRecordAsync();
			var response = new
			{
				LocalTime = DateTime.UtcNow.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt"),
				UtcTime = DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm tt"),
				DataProcessingSetting = _appSettings.Value.DataProcessing,
				LastVolumeRecord = lastVolumeRecord,
				LastVolumeRecordDate = lastVolumeRecord?.Date.ToString(CultureInfo.InvariantCulture)
			};
			return Ok(response);
		}
	}
}