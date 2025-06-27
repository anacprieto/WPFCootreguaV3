using HantleDispenserAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using WPFCootreguaV2.Domain.ApiService;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Exceptions;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.UIServices.Integrations;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.UserControls;
using ManualInputViewModel = WPFCootreguaV2.UserControls.ManualInputViewModel;


namespace WPFCootreguaV2.Presentation.UserControls
{
    /// <summary>
    /// Lógica de interacción para MenuUC.xaml
    /// </summary>
    public partial class WithdrawalUC : AppUserControl
    {
        private const string STR_TIMER = "02:30";
        private TimerGeneric _timer;
        private Transaction _ts;
        private PaymentViewModel _paymentViewModel;

        private StateTransaction _tranStateTemp = StateTransaction.Iniciada;
        public WithdrawalUC()
        {
            InitializeComponent();
            _ts = Transaction.Instance;

            _ts.DevueltaCorrecta = false;


#if NO_PERIPHERALS
            Button dynamicButton = new Button();

            // Set properties of the button
            dynamicButton.Content = "Add minor value";
            dynamicButton.Width = 100;
            dynamicButton.Height = 50;
            dynamicButton.VerticalAlignment = VerticalAlignment.Top;
            dynamicButton.HorizontalAlignment = HorizontalAlignment.Left;
            // Set background color
            dynamicButton.Background = new SolidColorBrush(Colors.Red); // Change to the desired color
            dynamicButton.Foreground = new SolidColorBrush(Colors.White); // Change to the desired color

            // Set border brush and thickness
            dynamicButton.BorderBrush = new SolidColorBrush(Colors.White); // Change to the desired color
            dynamicButton.BorderThickness = new Thickness(2); // Change thickness as needed
            dynamicButton.Click += ExecuteScanner;

            Button dynamicButton2 = new Button();

            // Set properties of the button
            dynamicButton2.Content = "Add mid value";
            dynamicButton2.Width = 100;
            dynamicButton2.Height = 50;
            dynamicButton2.VerticalAlignment = VerticalAlignment.Top;
            dynamicButton2.HorizontalAlignment = HorizontalAlignment.Center;
            // Set background color
            dynamicButton2.Background = new SolidColorBrush(Colors.Transparent); // Change to the desired color
            dynamicButton2.Foreground = new SolidColorBrush(Colors.White); // Change to the desired color

            // Set border brush and thickness
            dynamicButton2.BorderBrush = new SolidColorBrush(Colors.White); // Change to the desired color
            dynamicButton2.BorderThickness = new Thickness(2); // Change thickness as needed
            dynamicButton2.Click += ExecuteScanner2;

            void ExecuteScanner(object sender, EventArgs e)
            {
                SimulateFullDispense(_paymentViewModel.ReturnAmount);

                //SaveWithdrawal();
                //NotifyPay();
            }

            void ExecuteScanner2(object sender, EventArgs e)
            {
                //OnCashIn(50000);
            }
            //MainGrid.Children.Add(dynamicButton);

#else
      _peripherals = PeripheralController.Instance;
      _peripherals.CashDispensed += OnCashDispensed;
      _peripherals.DispenserReject += OnDispenserReject;
#endif
            // Agregar eventos
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;

        }
        void SimulateFullDispense(decimal amount)
        {
            var dispensedDetails = SimulateCashDispense(amount);
            decimal totalDispensed = dispensedDetails.Sum(d => d.Key * d.Value);
            OnCashDispensed(totalDispensed, dispensedDetails);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _paymentViewModel = new PaymentViewModel
            {
                PayAmount = _ts.Total,
                RemainingAmount = 0,
                ReturnAmount = _ts.Total,
                EnteredAmount = 0,
                Denominations = new List<Denomination>(),
                DispensedAmount = 0
            };

            ReturnMoney(_paymentViewModel.ReturnAmount);

            //#if NO_PERIPHERALS
            //#else
            //            _peripherals.StartAcceptance(_paymentViewModel.PayAmount);
            //#endif
        }
        //private void InitViewModel()
        //{

        //    _paymentViewModel = new PaymentViewModel
        //    {
        //        PayAmount = _ts.Total,
        //        RemainingAmount =0,
        //        ReturnAmount = _ts.Total,
        //        EnteredAmount = 0,
        //        Denominations = new List<Denomination>(),
        //        DispensedAmount = 0
        //    };

        //    ReturnMoney(_paymentViewModel.ReturnAmount);

        //}




        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
#if NO_PERIPHERALS
#else
            _peripherals.CashDispensed -= OnCashDispensed;
            _peripherals.DispenserReject -= OnDispenserReject;
#endif
        }

#if NO_PERIPHERALS
        private Dictionary<int, int> SimulateCashDispense(decimal amount)
        {
            var details = new Dictionary<int, int>();
            const int billValue = 50000;

            // Calcular cuántos billetes de 50,000 se necesitan
            int billCount = (int)(amount / billValue);

            if (billCount > 0)
            {
                details.Add(billValue, billCount);
            }

            return details;
        }
#endif

        private void ReturnMoney(decimal returnValue)
        {
            _ts.DevueltaCorrecta = false;
#if NO_PERIPHERALS
            var dispensedDetails = SimulateCashDispense(returnValue);
            OnCashDispensed(returnValue, dispensedDetails);
            OnCashDispensed(returnValue, new Dictionary<int, int>());
#else
            _peripherals.StartDispenser(returnValue);
#endif

        }
        private async void OnCashDispensed(decimal totalDispensed, Dictionary<int, int> details)
        {
            _paymentViewModel.DispensedAmount = totalDispensed;
            _paymentViewModel.RemainingAmount = _paymentViewModel.ReturnAmount - _paymentViewModel.DispensedAmount;
            string strValueToReturn = _paymentViewModel.RemainingAmount.ToString("C0");
            SendDispenseDetails(details);
            _nav.CloseModal();

            if (_paymentViewModel.DispensedAmount == _paymentViewModel.ReturnAmount)
            {
                _ts.DevueltaCorrecta = true;
                await SaveWithdrawal();
            }
            else
            {
                _nav.ShowModal("No se pudo entregar la totalidad del dinero hay un faltante de:" + $" {strValueToReturn} " + ".Por favor comunícate con un administrador.", new InfoModal());
                await Task.Delay(5000); // Timer para mostrar la modal y que se pueda leer
                _ts.DevueltaCorrecta = false;
                await SaveWithdrawal();
            }
        }

        private void SendDispenseDetails(Dictionary<int, int> details)
        {
            foreach (var denom in details.Keys)
            {
                var quantity = details[denom];
                if (quantity <= 0) continue;
                SendTransactionDetail(TypeOperation.DP, Convert.ToDecimal(denom), quantity);
            }
        }

        //private async void OnCashDispensed(decimal totalDispensed, Dictionary<int, int> details)
        //{

        //    _paymentViewModel.DispensedAmount = totalDispensed;

        //    _paymentViewModel.RemainingAmount = _paymentViewModel.ReturnAmount - _paymentViewModel.DispensedAmount;
        //    string strValueToReturn = _paymentViewModel.RemainingAmount.ToString("C0");

        //    SendDispenseDetails(details);

        //     _nav.CloseModal();

        //    if (_paymentViewModel.DispensedAmount == _paymentViewModel.ReturnAmount)
        //    {
        //        _ts.DevueltaCorrecta = true;
        //        await SaveWithdrawal();

        //    }
        //    else
        //    {
        //        _nav.ShowModal("No se pudo entregar la totalidad del dinero hay un faltante de:" + $" {strValueToReturn} " + ".Por favor comunícate con un administrador.",new InfoModal());
        //        await Task.Delay(5000); // Timer para mostrar la modal y que se pueda leer
        //        _ts.DevueltaCorrecta = false;
        //        await SaveWithdrawal();

        //    }

        //}


        private void SendTransactionDetail(TypeOperation op, decimal denom, int quantity)
        {
            try
            {
                EventLogger.SaveLog(EventType.Info, $"Enviando detalle a la api: Op: {op}, Denom: {denom.ToString("C0")}, Cantidad: {quantity}");
                Api.CreateTransactionDetail1(op, (int)denom, quantity);

            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }
        #region "Eventos"
        private void txtValueReturn_TouchDown(object sender, TouchEventArgs e)
        {
            try
            {
                var txt = sender as TextBlock;

                if (txt.Text.Contains("*"))
                {
                    txt.Text = String.Format("{0:C0}", Convert.ToDecimal(txt.Tag));
                }
                else
                {
                    txt.Text = "********";
                }
            }
            catch (Exception ex)
            {
               // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }
        #endregion

        private async Task Notify()
        {
            var maxTries = 3;
            var nTries = 0;
            while (nTries < maxTries)
            {
                nTries++;
                EventLogger.SaveLog(EventType.Info, $"Intento {nTries} notificar retiro");
                PymentProduct pay = new PymentProduct
                {
                    Identification = _ts.Documento,
                    Description = "Retiro",
                    PayDate = DateTime.Now,
                    ValueToPay = (long)_ts.Total,
                    NumberProduct = long.Parse(_ts.ProductSelect.NumberProduct),
                    TypeProduct = _ts.ProductSelect.TipoProducto,
                    Coduser = _ts.Codigo,
                    codOpe = 0,
                    TipoMovimiento = 1
                };

                var authen = await ApiIntegration.CallApiCootregua("ControllerCootreguaRetirePayments", pay);
                Thread.Sleep(500);

                if (!string.IsNullOrEmpty(authen) && authen != "[]")
                {
                    var data = JsonConvert.DeserializeObject<PymentProduct>(authen);
                    _tranStateTemp = StateTransaction.Aprobada;

                    if (data.codOpe >= 1)
                    {
                        _ts.PayCode = data.codOpe;
                        _ts.statePaySuccess = true;
                        _ts.EstadoTransaccion =StateTransaction.Aprobada;
                        // ReturnMoney();
                    }
                }


            }
            _tranStateTemp = StateTransaction.AprobadaSinNotificar;

        }

        private async Task SaveWithdrawal()
        {
            try
            {
#if NO_PERIPHERALS
                
#else
                await Notify();
#endif


                _paymentViewModel.IsPayCompleted = true;
                _ts.DatosPago = _paymentViewModel;
                _ts.TotalIngresado = _paymentViewModel.EnteredAmount;
                _ts.TotalDevuelta = _paymentViewModel.DispensedAmount;

                SetTransactionDescription();


                if ((_tranStateTemp == StateTransaction.Aprobada || _tranStateTemp == StateTransaction.Cancelada)
                    && !_ts.DevueltaCorrecta)
                {
                    // Si el estado de transacción es aprobada o cancelada y además hay error de devuelta se cambia a su respectivo estado
                    // CanceladoErrorDevuelta o AprobadaErrorDevuelta
                    _ts.EstadoTransaccion = (StateTransaction)((int)_tranStateTemp + 2);
                }
                else
                {
                    _ts.EstadoTransaccion = _tranStateTemp;
                }



                Api.UpdateTransaction();

                Dispatcher.Invoke(() => GoTo(new SuccessUC()));
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
                _nav.CloseModal();
                _nav.ShowModal("Se presentó un problema intentando reportar los datos del retiro. Por favor comuníquese con soporte técnico.",new InfoModal());
            }
        }
        private void SetTransactionDescription()
        {
            switch (_tranStateTemp)
            {
                case StateTransaction.Aprobada:
                    _ts.EstadoTransaccionVerb = "Exitoso";
                    _ts.Descripcion += "Transacción finalizada correctamente. ";
                    break;
                case StateTransaction.AprobadaSinNotificar:
                    _ts.EstadoTransaccionVerb = "Exitoso";
                    _ts.Descripcion += "Transacción aprobada, pero no se ha podido notificar el retiro.";
                    break;

                default:
                    break;
            }

            if (!_ts.DevueltaCorrecta)
                _ts.Descripcion += $"Ocurrió un error durante la devolución del dinero. Cantidad faltante {_paymentViewModel.RemainingAmount.ToString("C0")}";
        }


    }
}
