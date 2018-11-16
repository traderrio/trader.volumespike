using System;
using Microsoft.Extensions.DependencyInjection;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike
{
	partial class Startup
	{
		private void Bootstrap(IServiceProvider serviceProvider)
		{
			var polygonService = serviceProvider.GetRequiredService<IPolygonService>();
			polygonService.SubscribeToTrades();
		}
	}
}