using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFCootreguaV2.ApiService.IntegrationModels;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.Domain.Integrations;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.Domain.ApiService;
using System.Windows;
using WPFCootreguaV2.UserControls;
using WPFCootreguaV2.Presentation.UserControls;

namespace WPFCootreguaV2.Domain.UIServices.Integrations
{
    internal class SIGAMPayInvoiceManager : IProcedureManager
    {
        private readonly Transaction _ts;
        private readonly Navigator _nav;

        public SIGAMPayInvoiceManager()
        {
            _ts = Transaction.Instance;
            _nav = Navigator.Instance;
        }

        public bool IsRoundTotal()
        {
            decimal diff = _ts.Total - _ts.TotalSinRedondear;
            bool continuar = _nav.ShowModal(
                $"El valor del pago será redondeado a {_ts.Total.ToString("C0")} " +
                $"por lo cuál pagará un excedente de: {diff.ToString("C0")}. " +
                $"Este valor se verá reflejado como un saldo a favor en sus próximas facturas." +
                $"Presione continuar para Continuar con el pago, de lo contrario presione Cancelar.", new ConfirmationModal());
            return continuar;
        }

        public async Task GetDataPay()
        {
            try
            {

//                var modal = _nav.ShowLoadModal("Consultando Coincidencias...");

  //              _nav.CloseModal();

                Application.Current.Dispatcher.Invoke(() => _nav.NavigateTo(new DataPayUC()));

            }
            catch (Exception ex)
            {

                return;
            }
        }

        public async Task NotifyPay(RequestNotifyPay req)
        {
            try
            {

                var modal = _nav.ShowLoadModal("Consultando Coincidencias...");

                Application.Current.Dispatcher.Invoke(() => _nav.NavigateTo(new DataPayUC()));

            }
            catch (Exception ex)
            {

                return;
            }
        }





    }
}
