using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Play.Catalog.Service.BsonFormat
{
    /// <summary>
    /// Static constructor to implement Bson serializer 
    /// </summary>
    public class BsonSerializerRegisterer
    {
        static BsonSerializerRegisterer()
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
        }

        /// <summary>
        /// BsonSerializerRegisters method 
        /// </summary>
        public static void BsonSerializerRegisters()
        {
        
        }
    }
    
}