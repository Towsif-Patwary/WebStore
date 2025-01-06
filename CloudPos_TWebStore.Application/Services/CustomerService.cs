using AutoMapper;
using CloudPos_TWebStore.Domain.DataModels;
using CloudPos_TWebStore.Domain.Repositories;
using CloudPos_TWebStore.Infrastructure.Repositories;
using CloudPos_WebStore.Application.DTOs;
using CloudPos_WebStore.Application.Interfaces;
using CloudPos_WebStore.Domain.DataModels;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Text;

namespace CloudPos_TWebStore.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly IMapper _mapper;

        public CustomerService(ICustomerRepository repository, IMapper mapper) 
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _repository.GetAllAsync();
            var customerlist = _mapper.Map<List<CustomerDto>>(customers);
            return customerlist;
        }
        //public async Task ExportCustomersToFile(string jobId)
        //{
        //    var filePath = Path.Combine("Exports", $"{jobId}.json");

        //    // Create directory if it doesn't exist
        //    Directory.CreateDirectory("Exports");

        //    // Fetch data and write to file
        //    var customers = await _repository.GetAllAsync();
        //    var json = JsonConvert.SerializeObject(customers);

        //    await File.WriteAllTextAsync(filePath, json);
        //}
        public async Task ExportCustomersAsync(string jobId)
        {
            const int targetFileSizeMB = 25;
            const int targetFileSizeBytes = targetFileSizeMB * 1024 * 1024;

            var filePathPrefix = Path.Combine("Exports", $"{DateTime.Now:yyyyMMdd}_Customers");
            Directory.CreateDirectory("Exports");

            var customers = await _repository.GetAllAsync();
            var customersList = customers.ToList(); // Ensure it's a list to enable indexing
            int partNumber = 1;
            int startIndex = 0;

            while (startIndex < customersList.Count)
            {
                var currentBatch = new List<object>();
                long currentBatchSizeBytes = 0;

                for (int i = startIndex; i < customersList.Count; i++)
                {
                    var customer = customersList[i];
                    var serializedCustomer = JsonConvert.SerializeObject(customer);
                    var customerSizeBytes = Encoding.UTF8.GetByteCount(serializedCustomer) + 2; // Account for list formatting

                    if (currentBatchSizeBytes + customerSizeBytes > targetFileSizeBytes && currentBatch.Count > 0)
                    {
                        break; // Stop adding customers to the current batch
                    }

                    currentBatch.Add(customer);
                    currentBatchSizeBytes += customerSizeBytes;
                    startIndex = i + 1; // Update the next start index
                }

                // Write the current batch to a file
                var fileName = $"{filePathPrefix}_Part{partNumber}.json";
                var json = JsonConvert.SerializeObject(currentBatch, Formatting.Indented); // Pretty-print for readability
                await File.WriteAllTextAsync(fileName, json);

                Console.WriteLine($"Created file: {fileName} with size: {currentBatchSizeBytes / (1024 * 1024)} MB");

                partNumber++;
            }
        }


        public async Task<List<CustomerDto>> GetCustomersPagedAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            var customers = await _repository.GetPagedAsync(skip, pageSize);
            return _mapper.Map<List<CustomerDto>>(customers);
        }
        public async IAsyncEnumerable<CustomerDto> GetAllCustomersStreamAsync()
        {
            var cursor = await _repository.GetAllStreamAsync();
            while (await cursor.MoveNextAsync())
            {
                foreach (var customer in cursor.Current)
                {
                    yield return _mapper.Map<CustomerDto>(customer);
                }
            }
        }


        public async Task<CustomerDto?> GetCustomerByIdAsync(string id)
        {
            var customer = await _repository.GetByIdAsync(id);
            var customerdata = _mapper.Map<CustomerDto>(customer);
            return customerdata;
        }

        public async Task CustomersBulkInsertAsync(List<CustomerInsert> customers)
        {
            var failedRecords = new List<CustomerInsert>();
            const int batchSize = 1000; // Adjust the batch size as needed
            const int maxConcurrentTasks = 5; // Limit concurrent tasks

            var semaphore = new SemaphoreSlim(maxConcurrentTasks);

            var tasks = new List<Task>();
            var totalBatches = (int)Math.Ceiling((double)customers.Count / batchSize);

            for (int i = 0; i < totalBatches; i++)
            {
                var batch = customers.Skip(i * batchSize).Take(batchSize).ToList();

                await semaphore.WaitAsync();
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await _repository.InsertManyAsync(batch);
                    }
                    catch (MongoBulkWriteException<CustomerInsert> ex)
                    {
                        var writeErrors = ex.WriteErrors;
                        foreach (var error in writeErrors)
                        {
                            failedRecords.Add(batch[error.Index]);
                        }

                        if (failedRecords.Any())
                        {
                            await LogFailedRecordsAsync("INSERT", "Customer", failedRecords, ex.Message);
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }
        private async Task LogFailedRecordsAsync(string operationType, string modelName, List<CustomerInsert> failedRecords, string errorMessage)
        {
            var logEntry = new Logs
            {
                OperationType = operationType,
                ModelName = modelName,
                ActionDate = DateTime.UtcNow,
                JsonData = JsonConvert.SerializeObject(failedRecords), // Serialize failed records to JSON
                Message = errorMessage
            };

            await _repository.InsertLogAsync(logEntry);
        }

        public async Task CustomerOneInsertAsync(CustomerInsert customer)
        {
            await _repository.InsertOneAsync(customer);
        }

    }
}
