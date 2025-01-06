using CloudPos_TWebStore.Domain.DataModels;
using CloudPos_WebStore.Application.DTOs;

namespace CloudPos_WebStore.Application.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllCustomersAsync();
    Task<List<CustomerDto>> GetCustomersPagedAsync(int pageNumber, int pageSize);
    IAsyncEnumerable<CustomerDto> GetAllCustomersStreamAsync();
    Task ExportCustomersAsync(string jobId);
    Task<CustomerDto?> GetCustomerByIdAsync(string id);
    Task CustomersBulkInsertAsync(List<CustomerInsert> customers);
    Task CustomerOneInsertAsync(CustomerInsert customer);

}
