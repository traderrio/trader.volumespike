using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Trader.VolumeSpike.Domain
{

	public interface IEntity
	{
		[BsonId]
		[BsonIgnoreIfDefault]
		[BsonRepresentation(BsonType.ObjectId)]
		string Id { get; set; }
		[BsonIgnore]
		string CollectionName { get; }
	}
	public abstract class Entity : IEntity
	{
		[BsonId]
		[BsonIgnoreIfDefault]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }
		[BsonIgnore]
		public abstract string CollectionName { get; }
	}
}
