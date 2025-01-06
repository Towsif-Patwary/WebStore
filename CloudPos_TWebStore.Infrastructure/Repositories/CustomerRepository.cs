using CloudPos_TWebStore.Domain.DataModels;
using CloudPos_TWebStore.Domain.Repositories;
using CloudPos_WebStore.Domain.DataModels;
using MongoDB.Driver;

namespace CloudPos_TWebStore.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IMongoCollection<Customer> _customers;
        private readonly IMongoCollection<CustomerInsert> _customerinsert;
        private readonly IMongoCollection<Logs> _logsCollection;

        public CustomerRepository(IMongoClient client, string databaseName)
        {
            var database = client.GetDatabase(databaseName);
            _customers = database.GetCollection<Customer>("Customers");
            _customerinsert = database.GetCollection<CustomerInsert>("Customers");
            _logsCollection = database.GetCollection<Logs>("Logs");
            CreateIndexes();
        }
        private void CreateIndexes()
        {
            var indexKeys = Builders<Customer>.IndexKeys.Ascending(c => c.CustomerId);
            var indexOptions = new CreateIndexOptions { Unique = true }; 
            var indexModel = new CreateIndexModel<Customer>(indexKeys, indexOptions);

            _customers.Indexes.CreateOne(indexModel);
        }

        public async Task<List<Customer>> GetAllAsync() =>
            await _customers.Find(_ => true).ToListAsync();

        public async Task<List<Customer>> GetPagedAsync(int skip, int limit)
        {
            return await _customers.Find(_ => true)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }
        public async Task<IAsyncCursor<Customer>> GetAllStreamAsync()
        {
            return await _customers.Find(_ => true).ToCursorAsync();
        }

        public async Task<Customer?> GetByIdAsync(string id) =>
            await _customers.Find(c => c.CustomerId == id).FirstOrDefaultAsync();

        public async Task InsertManyAsync(IEnumerable<CustomerInsert> customers)
        {
            if (customers == null || !customers.Any())
                throw new ArgumentException("Customer list cannot be null or empty");

            try
            {
                await _customerinsert.InsertManyAsync(customers, new InsertManyOptions { IsOrdered = false });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while inserting customers", ex);
            }
        }

        public async Task InsertOneAsync(CustomerInsert customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer), "Customer cannot be null");

            try
            {
                await _customerinsert.InsertOneAsync(customer);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while inserting the customer", ex);
            }
        }
        public async Task InsertLogAsync(Logs logs)
        {
            try
            {
                await _logsCollection.InsertOneAsync(logs);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while inserting the customer", ex);
            }
        }

    }
}
