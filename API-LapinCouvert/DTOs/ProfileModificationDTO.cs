namespace API_LapinCouvert.DTOs
{
    public class ProfileModificationDTO
    {
        public string? NewPassword { get; set; }
        public string? OldPassword { get; set; }
        public string? NewFirstName { get; set; }
        public string? NewLastName { get; set; }
    }
}