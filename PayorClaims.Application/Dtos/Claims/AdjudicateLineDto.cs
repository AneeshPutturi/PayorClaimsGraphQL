namespace PayorClaims.Application.Dtos.Claims;

public class AdjudicateLineDto
{
    public int LineNumber { get; set; }
    public string Status { get; set; } = null!; // "Approved" or "Denied"
    public string? DenialReasonCode { get; set; }
    public decimal AllowedAmount { get; set; }
    public decimal PaidAmount { get; set; }
}
