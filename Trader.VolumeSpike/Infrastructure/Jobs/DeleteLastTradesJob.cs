using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using StackExchange.Redis.Extensions.Core;
using Trader.VolumeSpike.Common.Configuration;

namespace Trader.VolumeSpike.Infrastructure.Jobs
{
    public class DeleteLastTradesJob : AsyncJob
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ICacheClient _cacheClient;

        public DeleteLastTradesJob(IOptions<AppSettings> appSettings, ICacheClient cacheClient)
        {
            _appSettings = appSettings;
            _cacheClient = cacheClient;
        }

        protected override async Task ExecuteAsync()
        {
            var from = DateTime.MinValue.Ticks;
            var to = DateTime.Now.AddHours(-_appSettings.Value.Redis.DeleteLastTradesAfter).Ticks;
            
            var keys =_cacheClient.SearchKeys($"{RedisKeyBuilder.LastTradesPrefix}:*").ToList();

            foreach (var key in keys)
            {
                await _cacheClient.Database.SortedSetRemoveRangeByScoreAsync(key, from, to);
            }
        }
    }
}