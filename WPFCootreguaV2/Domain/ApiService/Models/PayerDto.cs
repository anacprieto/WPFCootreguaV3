using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFCootreguaV2.ApiService.Models;

namespace WPFCootreguaV2.Domain.ApiService.Models
{
    public class PayerDto : DtoCommon
    {
        [Required(ErrorMessage = "DOCUMENT is required.")]
        public string? DOCUMENT { get; set; }

        [Required(ErrorMessage = "DOCUMENT_TYPE is required.")]
        [StringLength(2, ErrorMessage = "DOCUMENT_TYPE must be 2 characters or less.")]
        public string? DOCUMENTTYPE { get; set; }

        [Required(ErrorMessage = "NAME is required.")]
        public string? NAME { get; set; }

        [Required(ErrorMessage = "LASTNAME is required.")]
        public string? LASTNAME { get; set; }
        public string? PHONE { get; set; }
        public string? EMAIL { get; set; }
        public string? ADRESS { get; set; }
        public int? IDTRANSACTION { get; set; }
        public int? IDCLIENT { get; set; }
        public int? IDPAYPAD { get; set; }
        public string? PAYPAD { get; set; }
    }
}
