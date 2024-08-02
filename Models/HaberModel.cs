namespace HaberOtesi.Models
{
    public class Haber
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Writing { get; set; }
        public IFormFile? Img { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Slug { get; set; }
    }
}
