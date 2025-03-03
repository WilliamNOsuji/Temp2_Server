namespace API_LapinCouvert.DTOs
{
    public class SuggestedProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; } = "";
        public DateTime FinishDate { get; set; }
        public string? UserVote { get; set; }
    }
}
