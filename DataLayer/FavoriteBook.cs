namespace DataLayer
{
    public class FavoriteBook
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; } = DateTime.Now;

    }
}