using Microsoft.AspNetCore.Mvc;
using CloudPos_WebStore.Application.DTOs;
using CloudPos_WebStore.Application.Interfaces;
using CloudPos_TWebStore.Domain.DataModels;
using Microsoft.AspNetCore.RateLimiting;
using AutoMapper;
using MongoDB.Driver;
using Hangfire;
using CloudPos_TWebStore.Application.Services;
using System.IO.Compression;

namespace CloudPos_TWebStore.Controllers;

public class CustomersController : BaseController
{
    private readonly ICustomerService _customerService;
    private readonly IMapper _mapper;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public CustomersController(ICustomerService service, IMapper mapper, IBackgroundJobClient backgroundJobClient)
    {
        _customerService = service;
        _mapper = mapper;
        _backgroundJobClient = backgroundJobClient;
    }

    [HttpGet("get-all")]
    [EnableRateLimiting("TowsifPolicy")]
    public async Task<ActionResult<List<CustomerDto>>> Get() =>
        await _customerService.GetAllCustomersAsync();

    [HttpGet("get-all-paged")]
    public async Task<ActionResult<List<CustomerDto>>> GetAllCustomers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("Page number and page size must be greater than 0.");

        var customers = await _customerService.GetCustomersPagedAsync(pageNumber, pageSize);
        return Ok(customers);
    }

    [HttpGet("get-all-stream")]
    [EnableRateLimiting("TowsifPolicy")]
    public async IAsyncEnumerable<CustomerDto> GetAllCustomersStream()
    {
        await foreach (var customer in _customerService.GetAllCustomersStreamAsync())
        {
            yield return customer;
        }
    }

    #region background job
    [HttpPost("get-all-export-to-file")]
    public IActionResult ExportCustomers()
    {
        var jobId = Guid.NewGuid().ToString();

        _backgroundJobClient.Enqueue<CustomerService>(service => service.ExportCustomersAsync(jobId)); 

        return Ok(new { JobId = jobId, Message = "Export started. Check status later." });
    }

    [HttpGet("get-all-export-status/{jobId}")]
    public IActionResult GetExportStatus(string jobId)
    {
        var filePathPrefix = Path.Combine("Exports", $"{jobId}_Part");

        // Get all parts for the job
        var fileParts = Directory.GetFiles("Exports", $"{jobId}_Part*.json");

        // Check if the number of files matches the expected count based on parts
        var expectedParts = int.Parse(fileParts.LastOrDefault()?.Split('_').Last().Replace(".json", "") ?? "0");
        if (fileParts.Length == expectedParts)
        {
            return Ok(new { Status = "Completed", DownloadUrl = $"/api/export/download/{jobId}" });
        }

        return Ok(new { Status = "In Progress" });
    }
    [HttpGet("download/{jobId}")]
    public IActionResult DownloadExport(string jobId)
    {
        var filePathPrefix = Path.Combine("Exports", $"{jobId}_Part");

        // Get all parts for the job
        var fileParts = Directory.GetFiles("Exports", $"{jobId}_Part*.json");

        if (fileParts.Length == 0)
        {
            return NotFound("No files found for this job.");
        }

        // You can either zip all files together or download them individually.
        // For simplicity, we'll send back the first file part.
        var fileBytes = System.IO.File.ReadAllBytes(fileParts[0]);
        var fileName = Path.GetFileName(fileParts[0]);

        return File(fileBytes, "application/json", fileName);
    }
    [HttpGet("download-zip/{jobId}")]
    public IActionResult DownloadZipExport(string jobId)
    {
        var filePathPrefix = Path.Combine("Exports", $"{jobId}_Part");

        // Get all parts for the job
        var fileParts = Directory.GetFiles("Exports", $"{jobId}_Part*.json");

        if (fileParts.Length == 0)
        {
            return NotFound("No files found for this job.");
        }

        var zipFilePath = Path.Combine("Exports", $"{jobId}.zip");

        // Create a ZIP file containing all parts
        using (var zip = new ZipArchive(System.IO.File.Create(zipFilePath), ZipArchiveMode.Create))
        {
            foreach (var file in fileParts)
            {
                var zipEntry = zip.CreateEntry(Path.GetFileName(file));
                using (var entryStream = zipEntry.Open())
                using (var fileStream = System.IO.File.OpenRead(file))
                {
                    fileStream.CopyToAsync(entryStream);
                }
            }
        }

        var zipFileBytes = System.IO.File.ReadAllBytes(zipFilePath);
        return File(zipFileBytes, "application/zip", $"{jobId}.zip");
    }


    #endregion

    [HttpGet("get-by-id/{id}")]
    public async Task<ActionResult<CustomerDto>> Get(string id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer is null) return NotFound();
        return customer;
    }

    [HttpPost("bulk-insert")]
    [EnableRateLimiting("TowsifPolicy")]
    public async Task<IActionResult> BulkInsertCustomers([FromBody] List<CustomerInsert> customers)
    {
        if (customers == null || !customers.Any())
            return BadRequest("Customer list cannot be null or empty");

        try
        {
            await _customerService.CustomersBulkInsertAsync(customers);
            return Ok("Customer Bulk insert completed successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    #region Create bulk data
    [HttpPost("test-bulk-insert")]
    [EnableRateLimiting("TowsifPolicy")]
    public async Task<IActionResult> TestBulkInsertCustomers([FromBody] CustomerInsert customer)
    {
        try
        {
            var customers = new List<CustomerInsert> { customer };
            
            string numericPart = customer.CustomerId.Substring(4);
            if (!int.TryParse(numericPart, out int number))
                return BadRequest("Invalid CustomerId format");


            for (int i=1; i < 1000000; i++) 
            {
                number++;
                var newCustomer = new CustomerInsert
                {
                    FirstName = customer.FirstName,
                    MiddleName = customer.MiddleName,
                    LastName = customer.LastName,
                    Address = customer.Address,
                    City = customer.City,
                    PostalCode = customer.PostalCode,
                    Country = customer.Country,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    DateOfEntry = customer.DateOfEntry,
                    Type = customer.Type,
                    DiscountPercent = customer.DiscountPercent,
                    EarnedPoints = customer.EarnedPoints,
                    RedeemedPoints = customer.RedeemedPoints,
                    BalancePoints = customer.BalancePoints,
                    UpdatedDate = customer.UpdatedDate,
                    Transfer = customer.Transfer,
                    ExpireDate = customer.ExpireDate,
                    DateOfBirth = customer.DateOfBirth,
                    EShopCode = customer.EShopCode,
                    CardNumber = customer.CardNumber,
                    CreditLimit = customer.CreditLimit,
                    IsCreditCustomer = customer.IsCreditCustomer,
                    IsZeroVatSale = customer.IsZeroVatSale,
                    BIN = customer.BIN,
                    Gender = customer.Gender,
                    SpecialDate = customer.SpecialDate,
                    SpecialDateNote = customer.SpecialDateNote,
                    AgeRange = customer.AgeRange,
                    MainChannelCode = customer.MainChannelCode,
                    ZoneCode = customer.ZoneCode,
                    CategoryCode = customer.CategoryCode,
                    SubCategoryCode = customer.SubCategoryCode,
                    CustomerGroup = customer.CustomerGroup,
                    IsInactive = customer.IsInactive,
                    ShippingAddress = customer.ShippingAddress,
                    AlternativePhone = customer.AlternativePhone,
                    CustomerId = $"CUST{number:D7}" 
                };

                customers.Add(newCustomer);
            }

            await _customerService.CustomersBulkInsertAsync(customers);
            return Ok("Customer Bulk insert completed successfully.");
        }
        catch (MongoBulkWriteException ex)
        {
            return StatusCode(500, $"MongoDB Write Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    #endregion

    [HttpPost("single-insert")]
    public async Task<IActionResult> SingleInsertCustomer([FromBody] CustomerInsert customer)
    {
        if (customer == null || customer.CustomerId == null)
            return BadRequest("Customer cannot be null");

        try
        {
            await _customerService.CustomerOneInsertAsync(customer);
            return Ok("Customer insert completed successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

}
