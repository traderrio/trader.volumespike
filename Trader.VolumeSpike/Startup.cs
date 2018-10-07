using FluentScheduler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.MsgPack;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Infrastructure;
using Trader.VolumeSpike.Infrastructure.Jobs;
using Trader.VolumeSpike.Services;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike
{
    partial class Startup
    {
	    public MongoConnectionString MongoConnectionString { get; set; }
		private readonly ILogger<Startup> _logger;

        public Startup(IHostingEnvironment env, ILogger<Startup> logger)
        {
            _logger = logger;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
	        MongoConnectionString = new MongoConnectionString(Configuration.GetConnectionString("Mongo"));

			services.Configure<AppSettings>(Configuration);
            services.AddSingleton(Configuration);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

	        services.AddSingleton(option =>
	        {
		        var client = new MongoClient(MongoConnectionString.Settings);
		        return client.GetDatabase(MongoConnectionString.Database);
	        });
	        services.AddSingleton<IMongoDbContext, MongoDbContext>();
			services.AddSingleton<MongoPolygonDataSaver>();
			//services.AddScoped<DeleteLastTradesJob>();
            services.AddSingleton<IPolygonService, PolygonService>();
            services.AddSingleton<IPolygonDataSaver, PolygonDataSaver>();

			services.AddSingleton<RedisPolygonDataSaver>();
 

            var redisConfiguration = Configuration.GetSection("Redis:Server").Get<RedisConfiguration>();
            services.AddSingleton(redisConfiguration);
            services.AddSingleton<ICacheClient, StackExchangeRedisCacheClient>();
            services.AddSingleton<ISerializer, MsgPackObjectSerializer>();
            services.AddScoped<ITradingActivityService, TradingActivityService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            Bootstrap(app.ApplicationServices);
        }
    }
}