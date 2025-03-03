namespace API_LapinCouvert.DTOs
{
    public class CheckoutSessionRequest
    {
        public double TotalPrice { get; set; }
        public string Currency { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> DeviceTokens { get; set; }
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
    }
}
