﻿using Easy.MessageHub;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.MsgPack;
using Trader.VolumeSpike.Common.Configuration;
using Trader.VolumeSpike.Infrastructure;
using Trader.VolumeSpike.Infrastructure.DbContext;
using Trader.VolumeSpike.Infrastructure.DbContext.Interfaces;
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

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddSingleton<IVolumeSpikeDbContext>(opt =>
			{
				var client = new MongoClient(MongoConnectionString.Settings);
				var db = client.GetDatabase(MongoConnectionString.Database);
				IOptions<AppSettings> appSettings = opt.GetRequiredService<IOptions<AppSettings>>();
				return new VolumeSpikeDbContext(db, appSettings);
			});

            services.AddSingleton<IMessageHub, MessageHub>();
			services.AddSingleton<ICacheClient, StackExchangeRedisCacheClient>();
			services.AddSingleton<ISerializer, MsgPackObjectSerializer>();
			services.AddSingleton<IVolumeSpikesDetector, VolumeSpikesDetector>();
			services.AddSingleton<IPolygonWsClient, PolygonWsClient>();
			services.AddScoped<ISymbolService, SymbolService>();
			services.AddScoped<ILastTradesService, LastTradesService>();
			services.AddScoped<IVolumeRecordService, VolumeRecordService>();
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