using MongoDB.Driver;
using Trader.Domain;
using Trader.VolumeSpike.Infrastructure.DbContext.Interfaces;
using Trader.VolumeSpike.Services.Interfaces;

namespace Trader.VolumeSpike.Services
{
	public class VolumeRecordService : IVolumeRecordService
	{
		private readonly IMongoCollection<VolumeRecord> _collection;
		public VolumeRecordService(IVolumeSpikeDbContext dbContext)
		{
			_collection = dbContext.GetCollection<VolumeRecord>();
		}

		public void SaveVolumeRecord(VolumeRecord r)
		{
			_collection.InsertOne(r);
		}
	}
}