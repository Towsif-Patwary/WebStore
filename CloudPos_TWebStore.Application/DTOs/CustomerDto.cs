namespace CloudPos_WebStore.Application.DTOs;

public class CustomerDto
{
    public string? CustomerId { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime? DateOfEntry { get; set; }
    public string? Type { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal EarnedPoints { get; set; } = 0;
    public decimal RedeemedPoints { get; set; } = 0;
    public decimal BalancePoints { get; set; } = 0;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public string Transfer { get; set; } = "N";
    public DateTime? ExpireDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? EShopCode { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; } = 0;
    public bool IsCreditCustomer { get; set; } = false;
    public bool IsZeroVatSale { get; set; } = false;
    public string BIN { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public DateTime? SpecialDate { get; set; }
    public string? SpecialDateNote { get; set; }
    public string? AgeRange { get; set; }
    public string? MainChannelCode { get; set; }
    public string? ZoneCode { get; set; }
    public string? CategoryCode { get; set; }
    public string? SubCategoryCode { get; set; }
    public string? CustomerGroup { get; set; }
    public bool IsInactive { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string AlternativePhone { get; set; } = string.Empty;
}
