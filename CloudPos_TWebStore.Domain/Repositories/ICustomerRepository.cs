
using CloudPos_TWebStore.Domain.DataModels;
using CloudPos_WebStore.Domain.DataModels;
using MongoDB.Driver;


namespace CloudPos_TWebStore.Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync();
        Task<List<Customer>> GetPagedAsync(int skip, int limit);
        Task<IAsyncCursor<Customer>> GetAllStreamAsync();
        Task<Customer?> GetByIdAsync(string id);
        Task InsertManyAsync(IEnumerable<CustomerInsert> customers);
        Task InsertOneAsync(CustomerInsert customer);
        Task InsertLogAsync(Logs logs);
    }
}
