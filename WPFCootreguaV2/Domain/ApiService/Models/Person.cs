using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFCootreguaV2.Domain.Enumerables;

namespace WPFCootreguaV2.Domain.ApiService.Models
{
   
    public  class Person
    {
        public long CodPerson { get; set; }
        public string Identification { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string FirstLastName { get; set; }
        public string SecondLastName { get; set; }
        public string Adress { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string CellPhone { get; set; }
        public int CodOficine { get; set; }
    }

  
}
