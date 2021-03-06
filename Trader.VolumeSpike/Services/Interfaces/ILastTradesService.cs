﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trader.Domain;

namespace Trader.VolumeSpike.Services.Interfaces
{
	public interface ILastTradesService
	{
        Task<List<StockLastTrade>> GetLastTrades(string ticker, DateTime fromDate, DateTime to, int length, int timeFrame);
	}
}