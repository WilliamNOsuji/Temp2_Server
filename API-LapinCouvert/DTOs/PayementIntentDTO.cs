using System.ComponentModel.DataAnnotations;

namespace API_LapinCouvert.DTOs
{
    public class PaymentIntentDTO
    {
       
        public string ClientSecret { get; set; }
        public string Customer { get; set; }
    }
}
