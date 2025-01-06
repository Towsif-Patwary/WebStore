using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CloudPos_WebStore.Domain.DataModels
{
    public class Logs
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? OperationType { get; set; }// GET, INSERT
        public string? ModelName { get; set; } // Customer
        public DateTime? ActionDate { get; set; } // Now
        public string? JsonData { get; set; } // Data
        public string? Message { get; set; } // Error Msg
    }
}
