using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFCootreguaV2.Domain.Enumerables;

namespace WPFCootreguaV2.Domain.ApiService.Models
{
    
    public partial class Payer
    {
        public int PAYER_ID { get; set; }
        public string IDENTIFICATION { get; set; }
        public string NAME { get; set; }
        public string LAST_NAME { get; set; }
        public string PHONE { get; set; }
        public string EMAIL { get; set; }
        public string ADDRESS { get; set; }
        public bool STATE { get; set; }
        public string TYPE_PAYER { get; set; }
        public string TYPE_IDENTIFICATION { get; set; }
    }


}
