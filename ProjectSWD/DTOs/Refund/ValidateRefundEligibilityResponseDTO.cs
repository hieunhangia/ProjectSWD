namespace ProjectSWD.DTOs.Refund
{
    public class ValidateRefundEligibilityResponseDTO
    {
        public bool IsEligible { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
