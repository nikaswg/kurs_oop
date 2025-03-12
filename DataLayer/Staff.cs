namespace DataLayer
{
    public class Staff
    {
        public int Id {  get; set; }    
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Role_Id { get; set; }    
        public string Home_Adres { get; set; } = string.Empty;
        public string Phone_Number { get; set; } = string.Empty;
        public string Birthday_Date { get; set; } = string.Empty; 
        public int Salary { get; set; }
    }
}
