using CloudPos_TWebStore.Domain.DataModels;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace CloudPos_WebStore.Infrastructure.Models.DbContext
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDbSettings:ConnectionString"]);
            _database = client.GetDatabase(configuration["MongoDbSettings:DatabaseName"]);
        }

        public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("Customers");
    }
}
