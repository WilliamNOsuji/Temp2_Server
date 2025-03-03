using LapinCouvert.Models;

namespace API_LapinCouvert.DTOs
{
    public class CommandDTO
    {
        public string Address { get; set; }
        public string Currency { get; set; }
        public double TotalPrice { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> DeviceTokens { get; set; }
    }
}
