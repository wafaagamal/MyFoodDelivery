namespace OrderingSvc.Application.Contracts.Payments.Dtos;

public class RefundResultDto
{
    public bool Success { get; set; }
    public string? RefundId { get; set; }
    public decimal Amount { get; set; }
    public string? ErrorMessage { get; set; }
}

