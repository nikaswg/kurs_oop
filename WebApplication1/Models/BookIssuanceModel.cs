namespace WebApplication1.Models
{
    public class BookIssuanceModel
    {
        public string ISBN { get; set; }
        public int Issue_Number { get; set; }
        public DateTime? Issue_Date { get; set; }
        public DateTime? Return_Date { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
