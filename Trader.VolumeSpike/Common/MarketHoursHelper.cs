using System;
using Trader.Common.Extensions;

namespace Trader.VolumeSpike.Common
{
    public static class MarketHoursHelper
    {
	    public static DateTime Now()
	    {
		    return DateTime.Now;
	    }
		public static DateTime MarketOpenDateTime()
	    {
			return DateTime.Now.StartOfDay().AddHours(9).AddMinutes(30);
		}
	    public static DateTime MarketCloseDateTime()
	    {
		    return DateTime.Now.StartOfDay().AddHours(16);
		}
	}
}
