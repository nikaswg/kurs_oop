using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class Book
    {
        public int Id {  get; set; }    
        public int ISBN { get; set; }
        public string Book_Name { get; set; } = string.Empty;
        public string Book_Author { get; set; } = string.Empty;
        public string Book_Description { get; set; } = string.Empty;
        public DateTime? Publication_Date { get; set; }
        public int Number_Of_Pages { get; set; }
    }
}
