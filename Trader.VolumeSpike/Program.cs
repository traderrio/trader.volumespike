using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Trader.VolumeSpike
{
	public class Program
	{
		public static string PathToContentRoot { get; set; }

		public static int Main(string[] args)
		{
			var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
			PathToContentRoot = Path.GetDirectoryName(pathToExe);

			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			var isDevelopment = env == EnvironmentName.Development;

			if (isDevelopment)
			{
				PathToContentRoot = Directory.GetCurrentDirectory();
			}

            var configuration = new ConfigurationBuilder()
				.SetBasePath(PathToContentRoot)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env ?? "Development"}.json", optional: true)
				.Build();

			var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
			var pathToLogFolder = isLinux
				? $"/var/log/{env}.volumespike.api/"
				: $"{PathToContentRoot}/Logs/{env}/";

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration)
				.Enrich.FromLogContext()
				.WriteTo.Async(
					a => a.RollingFile(pathToLogFolder + "volumespike-{Date}.txt"))
				.CreateLogger();

			try
			{
				Log.Warning("VolumeSpikes is running...");
                Log.Warning($"Is Development? {isDevelopment}");

                CreateWebHostBuilder(args).Build().Run();

				return 0;
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Host terminated unexpectedly");
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			return WebHost.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((context, config) =>
				{
					// Configure the app here.
				})
				.UseContentRoot(PathToContentRoot)
				.UseStartup<Startup>()
				.UseDefaultServiceProvider(options => options.ValidateScopes = false)
				.CaptureStartupErrors(true)
				.UseUrls("http://localhost:8005/")
				.UseSerilog();
		}
	}
}