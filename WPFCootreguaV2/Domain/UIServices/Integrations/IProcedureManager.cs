using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFCootreguaV2.ApiService.IntegrationModels;

namespace WPFCootreguaV2.Domain.Integrations
{
    public interface IProcedureManager
    {            
        public bool IsRoundTotal();

        public Task GetDataPay();

        public Task NotifyPay(RequestNotifyPay req);

    }

    public class ProcedureException: Exception
    {
        public ProcedureException() { }

        public ProcedureException(string? message) : base(message)
        {

        }

        public ProcedureException(string? message, Exception? innerException) : base(message, innerException)
        {

        }
    }
}
