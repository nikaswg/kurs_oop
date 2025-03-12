using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class Book_Issuance
    {
        public int Id { get; set; }
        public int ISBN { get; set; }   
        public int Issue_Number { get; set; }
        public DateTime? Issue_Date { get; set; }
        public DateTime? Return_Date { get; set; }
        public string Name_Reader { get; set; } = string.Empty;
    }
}
