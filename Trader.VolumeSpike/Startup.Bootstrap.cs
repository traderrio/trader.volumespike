using System;
using Microsoft.Extensions.DependencyInjection;
using Trader.VolumeSpike.Services;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike
{
	partial class Startup
	{
		private void Bootstrap(IServiceProvider serviceProvider)
		{
			serviceProvider.GetRequiredService<IVolumeSpikesDetector>();
			var polygonWsClient = serviceProvider.GetRequiredService<IPolygonWsClient>();
			polygonWsClient.RegisterAll();
		}
	}
}