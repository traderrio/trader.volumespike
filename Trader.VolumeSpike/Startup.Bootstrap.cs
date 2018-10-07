using System;
using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trader.VolumeSpike.Infrastructure.Jobs;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike
{
    partial class Startup
    {
        private void Bootstrap(IServiceProvider serviceProvider)
        {
            var polygonService = serviceProvider.GetRequiredService<IPolygonService>();
            polygonService.SubscribeToTrades();
            
            //var registry = new Registry();
            //registry.Schedule<DeleteLastTradesJob>().NonReentrant().ToRunNow().AndEvery(1).Minutes();
            //JobManager.JobFactory = new ScheduleJobFactory(serviceProvider);
            //JobManager.JobException += JobManagerOnJobException;
            //JobManager.Initialize(registry);
        }

        private void JobManagerOnJobException(JobExceptionInfo exceptionInfo)
        {
            _logger.LogError(exceptionInfo.Exception, exceptionInfo.Name);
        }
    }
}