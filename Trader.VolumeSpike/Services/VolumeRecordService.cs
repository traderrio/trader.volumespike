using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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

		public async Task<VolumeRecord> LastVolumeRecordAsync()
		{
			return await _collection.AsQueryable().OrderBy(x => x.Date).FirstOrDefaultAsync();
		}
	}
}