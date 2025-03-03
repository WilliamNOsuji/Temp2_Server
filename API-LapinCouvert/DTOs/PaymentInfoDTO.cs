namespace API_LapinCouvert.DTOs
{
    public class PaymentInfoDTO
    {
        public double Amount { get; set; }
        public string CardNumber { get; set; }
        public int CVCNumber { get; set; }
        // The following type might cause some issues
        public DateTime ExpirationDate { get; set; }
    }
}
