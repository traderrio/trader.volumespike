﻿using System;
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
		private readonly IPolygonDataSaver _polygonDataSaver;

		public WhoAmiController(IOptions<AppSettings> appSettings, IPolygonDataSaver polygonDataSaver)
		{
			_appSettings = appSettings;
			_polygonDataSaver = polygonDataSaver;
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			Log.Logger.Warning("accessing api/whomi");
			var lastTrade = await _polygonDataSaver.LastTradeRecord("SPY");

			var response = new
			{
				LocalTime = DateTime.UtcNow.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt"),
				UtcTime = DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm tt"),
				DataProcessingSetting = _appSettings.Value.DataProcessing,
				SPYLastTrade = lastTrade,
				SPYLastSavedTradeRecord = lastTrade != null ? lastTrade.DateTime.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt") : "N/A"
			};
			return Ok(response);
		}
	}
}