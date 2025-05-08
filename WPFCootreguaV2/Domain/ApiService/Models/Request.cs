using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFCootreguaV2.Domain.Enumerables;

namespace WPFCootreguaV2.Domain.ApiService.Models
{
    public class RequestGlobal
    {
        public string Data { get; set; }
        public DateTime CallDate { get; set; }
    }
    public class ResponseCootregua
    {
        public EResponseCode ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public object ResponseData { get; set; }
    }


}
