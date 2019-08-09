# Welcome

This is brief technical documentation for `traderr.volumespike` microservice

### Purpose

`traderr.volumespike` is a `.NET Core 2.0/C#` microservice app used to identify trending tickers based on spikes in sell and buy volume activity for around 3000 US stocks.

### Development

> Local development steps & requirements

1. Install latest version of Visual Studio
2. Make sure to have installed latest version of [MongoDB](https://www.mongodb.com/)
3. Pull latest from [GitHub](https://github.com/traderrio/trader.polygon.git)
4. Run & Build
5. Access the service at [http://localhost:6999/api/whoami](http://localhost:6999/api/whoami

### Production Deployment

> Production deployment steps

1. In `Visual Studio` click publish and create a new profile or use existing.
2. Publish to a file system folder of you choice. For Example `C:\Dropbox\IIS\volumespike.traderr.io\microservice\Trader.VolumeSpike.exe`
3. Create a windows service that will run `Trader.VolumeSpike.exe` for example:
 `sc.exe create TraderrVolumeSpikeService binPath="C:\Dropbox\IIS\volumespike.traderr.io\microservice\Trader.VolumeSpike.exe --service" DisplayName= "TraderrVolumeSpikeService" start= "auto""`
4. Access the service at [http://localhost:8005/api/whoami](http://localhost:6999/api/whoami)
5. `Polygon Api` uses the ports below:

"nats://nats1.polygon.io:30401",
"nats://nats2.polygon.io:30402",
"nats://nats3.polygon.io:30403"

so make sure to open them in windows firewall.
