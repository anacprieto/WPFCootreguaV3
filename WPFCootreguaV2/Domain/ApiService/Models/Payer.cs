using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFCootreguaV2.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;

namespace WPFCootreguaV2.Domain.ApiService.Models
{

    public partial class Payer: DtoCommon
    {
        public string? Document { get; set; }
        public string? DocumentType { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Adress { get; set; }

        public int? IdTransaction { get; set; }
        public int? IdClient { get; set; }
        public int IdPayPad { get; set; }
        public string? PayPad { get; set; }
    }


}
