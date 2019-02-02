using System.Threading.Tasks;
using Trader.Domain;

namespace Trader.VolumeSpike.Services.Interfaces
{
	public interface IVolumeRecordService
	{
		void SaveVolumeRecord(VolumeRecord r);
		Task<VolumeRecord> LastVolumeRecordAsync();
	}
}