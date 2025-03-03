namespace API_LapinCouvert.DTOs
{
    public class ProfileDTO
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImgUrl { get; set; }
        public bool IsDeliveryMan { get; set; }
        public bool IsActiveAsDeliveryMan { get; set; }
    }
}
