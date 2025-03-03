namespace API_LapinCouvert.DTOs
{
    public class CommandSuccess
    {
        public CommandSuccess(){}
        public string Address { get; set; }
        public double TotalPrice { get; set; }
        public string Currency { get; set; }
        public string CommandNumber { get; set; }

        public string PhoneNumber { get; set; }
        public int? DeliveryId { get; set; }
    }
}
