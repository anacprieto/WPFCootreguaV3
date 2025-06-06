using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.Models;
using WPFCootreguaV2.UserControls;

namespace WPFCootreguaV2.Presentation.UserControls
{
    /// <summary>
    /// Lógica de interacción para MenuUC.xaml
    /// </summary>
    public partial class TypeTransUC : AppUserControl
    {
        private const string STR_TIMER = "00:30";
        private Transaction _ts;


        //private TimerGeneric _timer;

        public TypeTransUC()
        {
            InitializeComponent();
            Unloaded += OnUnloaded;
            _ts = Transaction.Instance;

        }
        private async void SelectOption(object sender, EventArgs e)
        {
            string? selectedTag = ((Grid)sender).Tag.ToString();
            if (selectedTag == null) return;

            await Dispatcher.BeginInvoke(() =>
            {
                this.IsEnabled = false;
                this.Opacity = 0.3;
            });

            _ts.TipoRecaudo = selectedTag;
            switch (_ts.TipoRecaudo)
            {
                case "1":
                    _ts.Type = ETransactionType.Withdrawal;
                    _ts.TipoTransaccion = TypeTransaction.Retiro;
                    _ts.TipoTransaccionVerb = "Retiro";
                    //_ts.ProcedureManager = new WithdrawalManager();
                    Dispatcher.Invoke(() => GoTo(new IdentificationRetirosUC()));
                    break;

                case "2":
                    _ts.Type = ETransactionType.Payment;
                    _ts.TipoTransaccion = TypeTransaction.Pago;
                    _ts.TipoTransaccionVerb = "Pago";
                    //_ts.ProcedureManager = new PaymentManager();
                    Dispatcher.Invoke(() => GoTo(new IdentificationUC()));
                    break;

                case "3":
                    _ts.Type = ETransactionType.Registros;
                    _ts.TipoTransaccion = TypeTransaction.Registro;
                    _ts.TipoTransaccionVerb = "Registro";
                    //_ts.ProcedureManager = new PaymentManager();
                    Dispatcher.Invoke(() => GoTo(new IdentificationRegisterUC()));
                    break;


            }

            _ts.TipoPago = TypePayment.Efectivo;
            //ValidateStatus();
        }

        private void typeTrans_TouchDown(object sender, EventArgs e)
        {
            try
            {

                int Type = Convert.ToInt32((sender as Image).Tag);

                if (Type == 1)
                {
                    _ts.Type = ETransactionType.Withdrawal;
                }
                else
                if (Type == 2)
                {
                    _ts.Type = ETransactionType.Payment;
                }
                else
                {
                    _ts.Type = ETransactionType.Registros;
                }

                //ValidateStatus();
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }
        private void ValidateStatus()
        {
            var tsCreated = Api.CreateTransaction();
            if (tsCreated == null) throw new Exception("No se pudo enviar la transacción");

            // Agregar un retraso de 2 segundos antes de iniciar la cámara
            //await Task.Delay(2000, cancellationToken); // 2000 milisegundos = 2 segundos



#if NO_PERIPHERALS

#else
#endif

            //if (loadModal != null)
            //{
            //    loadModal.Close();
            //    loadModal = null;
            //}

            if (_ts.TipoPago == TypePayment.Efectivo)
                Dispatcher.Invoke(() => GoTo(new PaymentUC()));
        }
        private void Redirect(bool state)
        {
            try
            {
                if (state)
                {
                    Dispatcher.Invoke(() => GoTo(new MenuUC()));

                   // Utilities.navigator.Navigate(UserControlView.Menu);
                }
                else
                {
                    if (_ts.Type == ETransactionType.Registros)
                    {
                        Dispatcher.Invoke(() => GoTo(new MenuUC()));

                        //Utilities.navigator.Navigate(UserControlView.Menu);
                    }
                    else
                    {
                        //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, null, AdminPayPlus.DataPayPlus.Message);
                        //Utilities.ShowModal(MessageResource.NoService + " " + MessageResource.NoMoneyKiosco, EModalType.Error, false);
                        //GoTimer();
                    }
                }
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
        }

        private void BtnSalir_MouseDown(object sender, EventArgs e)
        {
            //Dispatcher.Invoke(() => GoTo(new WelcomeUC()));
        }

        private void BtnAtras_MouseDown(object sender, EventArgs e)
        {
           // Dispatcher.Invoke(() => GoTo(new WelcomeUC()));
        }
        


        private void BillPaymentFlow_Touch(object sender, EventArgs e)
        {
            // Detener la grabación antes de navegar
            //  VideoRecorder.Start();
            

           // Dispatcher.Invoke(() => GoTo(new SelectInputUC()));
        }

    }
}
