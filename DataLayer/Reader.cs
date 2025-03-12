using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class Reader
    {
        public int Id {  get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Home_Adress { get; set; } = string.Empty;
        public string Phone_Number { get; set; } = string.Empty;   
    }

}
