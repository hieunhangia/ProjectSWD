namespace ProjectSWD.DTOs.Review
{
    public class SubmitFeedbackRequestDto
    {
        public string CustomerId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
    }
}
