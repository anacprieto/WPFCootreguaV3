using MahApps.Metro.Controls;
using Microsoft.AspNet.SignalR.Client.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.ApiService.IntegrationModels;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Integrations;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.Variables;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.Presentation.UserControls;

namespace WPFCootreguaV2.UserControls
{
    public partial class PaymentUC : AppUserControl
    {
        private Transaction _ts;
        private ArduinoController _peripherals;
        private PaymentViewModel _paymentViewModel;
        private const string STR_TIMER = "07:30";
        private TimerGeneric _timer;
        private DocumentFormat _document = new();


        private bool _isPayCanceled = false;

        private ModalWindow? _currentLoadModal;

        private StateTransaction _tranStateTemp = StateTransaction.Iniciada;
        public PaymentUC()
        {
            InitializeComponent();

            EventLogger.SaveLog(EventType.Info, "Comienza proceso de pago, Iniciando PaymentUC.");

            
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
            dynamicButton.Background = new SolidColorBrush(Colors.Transparent); // Change to the desired color
            dynamicButton.Foreground = new SolidColorBrush(Colors.White); // Change to the desired color

            // Set border brush and thickness
            dynamicButton.BorderBrush = new SolidColorBrush(Colors.White); // Change to the desired color
            dynamicButton.BorderThickness = new Thickness(2); // Change thickness as needed
            dynamicButton.Click += ExecuteScanner;


            void ExecuteScanner(object sender, EventArgs e)
            {
                OnCashIn(20000);

            }

            MainGrid.Children.Add(dynamicButton);
#else
            _peripherals = ArduinoController.Instance;
            _peripherals.CashIn += OnCashIn;
            _peripherals.CashDispensed += OnCashDispensed;
            _peripherals.DispenserReject += OnDispenserReject;
            _peripherals.PeripheralError += OnPeripheralError;
#endif


            this.Unloaded += OnUnloaded;
            this.Loaded += OnLoaded;
            
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            InitViewModel();
#if NO_PERIPHERALS
#else
            _peripherals.StartAcceptance(_paymentViewModel.PayAmount);
#endif
        }

        private void InitViewModel()
        {
            _paymentViewModel = new PaymentViewModel
            {
                PayAmount = _ts.Total,
                RemainingAmount = _ts.Total,
                ReturnAmount = 0,
                EnteredAmount = 0,
                Denominations = new List<Denomination>(),
                DispensedAmount = 0
            };

            // Cambio principal: usar lv_denominations en lugar de DataView
            lv_denominations.DataContext = _paymentViewModel;
            this.DataContext = _paymentViewModel;
            lv_denominations.ItemsSource = _paymentViewModel.Denominations;
        }
        //private void InitViewModel()
        //{

        //    _paymentViewModel = new PaymentViewModel
        //    {
        //        PayAmount = _ts.Total,
        //        RemainingAmount = _ts.Total,
        //        ReturnAmount = 0,
        //        EnteredAmount = 0,
        //        Denominations = new List<Denomination>(),
        //        DispensedAmount = 0
        //    };
        //    DataView.DataContext = _paymentViewModel;
        //    this.DataContext = _paymentViewModel;
        //    DataView.ItemsSource = _paymentViewModel.Denominations;

        //}

        
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _nav.CloseModal();
            _nav.CloseLoadModal();
           
#if NO_PERIPHERALS
#else
            _peripherals.CashIn -= OnCashIn;
            _peripherals.CashDispensed -= OnCashDispensed;
            _peripherals.DispenserReject -= OnDispenserReject;
            _peripherals.PeripheralError -= OnPeripheralError;
#endif
        }


        #region Responses to Peripheral Events
        private async void OnCashIn(decimal value)
        {

            if (_paymentViewModel.IsPayCompleted) return;

            _paymentViewModel.EnteredAmount += value;

            _paymentViewModel.RefreshAmountsList(Convert.ToInt32(value), 1);

            SendTransactionDetail(TypeOperation.AP, value);

            _ = RefreshView(); // Se refresca la vista asincronamente

            if (_paymentViewModel.EnteredAmount < _paymentViewModel.PayAmount) return;
            
            //Finaliza pago cantidad completa
            _ = Dispatcher.BeginInvoke(() => BtnCancel.Visibility = Visibility.Collapsed);
#if NO_PERIPHERALS
#else
            await _peripherals.StopAceptance();
#endif

             _nav.ShowLoadModal("Estamos procesando el pago...");
            await Task.Delay(3000);
            EventLogger.SaveLog(EventType.Info, "Iniciando Proceso de pago...");
            NotifyPay();
            await SavePay();
           bool result= _nav.ShowModal("Desea realizar otra transacción?",new ConfirmationModal());
           if(result)
            {
                _nav.CloseModal();
                _nav.CloseLoadModal();
                Dispatcher.Invoke(() => GoTo(new ListProductsUC()));
                GC.Collect();
            }
            else
            {
                _nav.CloseModal();
                _nav.CloseLoadModal();
                Dispatcher.Invoke(() => GoTo(new SuccessUC()));
                GC.Collect();
             }


            // await PaymentProcess();
        }

        private async void OnCashDispensed(decimal totalDispensed, Dictionary<int, int> details)
        {

            _paymentViewModel.DispensedAmount = totalDispensed;

            _paymentViewModel.RemainingAmount = _paymentViewModel.ReturnAmount - _paymentViewModel.DispensedAmount;
            string strValueToReturn = _paymentViewModel.RemainingAmount.ToString("C0");

            SendDispenseDetails(details);

            _nav.CloseLoadModal();
            if (_paymentViewModel.DispensedAmount == _paymentViewModel.ReturnAmount)
            {
                _ts.DevueltaCorrecta = true;
                await SavePay();
            }
            else
            {
                _nav.ShowLoadModal("No se pudo entregar la totalidad del dinero hay un faltante de:" + $" {strValueToReturn} " + ".Por favor comunícate con un administrador.");
                await Task.Delay(5000); // Timer para mostrar la modal y que se pueda leer
                _ts.DevueltaCorrecta = false;
                await SavePay();
            }

        }

        private void OnDispenserReject(Dictionary<int,int> rejectData)
        {
            // Se registra el reject en la api
            SendRejectDetails(rejectData);

        }

        private void OnPeripheralError(Exception ex)
        {
            //TODO: Evaluar Si es necesario reportar errores de perifericos al Dashboard por que ya los errores de perifericos se reportan internamente
        }
        #endregion

        #region UI control methods

        private async Task RefreshView()
        {
            await Dispatcher.BeginInvoke(() =>
            {
                lv_denominations.Items.Refresh();
            });
        }
        //private async Task RefreshView()
        //{
        //    await Dispatcher.BeginInvoke(() =>
        //    {
        //        DataView.Items.Refresh();
        //    });
        //}
        private void CloseLoadModal()
        {
            if (_currentLoadModal != null)
            {
                Dispatcher.Invoke(() =>
                {
                    _currentLoadModal.Close();
                    _currentLoadModal = null;
                });
            }
        }
        private async void BtnCancel_TouchDown(object sender, EventArgs e)
        {
            _ = Dispatcher.BeginInvoke(() => BtnCancel.Visibility = Visibility.Collapsed);

            if (!_nav.ShowModal(Messages.CANCEL_TRANSACTION, new ConfirmationModal()))
            {
                _ = Dispatcher.BeginInvoke(() => BtnCancel.Visibility = Visibility.Visible);
                return;
            }
            EventLogger.SaveLog(EventType.Info, "Pago cancelado por el usuario.");
            await CancelPay();
        }

        #endregion

        #region Internal Operation process
      

        #region PagoFactura

        public async Task NotifyPay()
        {
            try
            {

                bool isPaySuccess = false;
                _nav.CloseLoadModal();
                EventLogger.SaveLog(EventType.Info, "Pago Completado en integración");
                _tranStateTemp = StateTransaction.Aprobada;
                isPaySuccess = true;


                if (isPaySuccess)
                {
                    await FinishSuccessfulPay();
                    return;
                }

                _paymentViewModel.ReturnAmount = _paymentViewModel.EnteredAmount;
                ReturnMoney(_paymentViewModel.ReturnAmount);
            
              
            }
            catch(Exception ex)
            {

            }
        }

        #endregion

     



        private async Task FinishSuccessfulPay()
        {
            if (_paymentViewModel.EnteredAmount > 0 && _paymentViewModel.ReturnAmount > 0)
            {

                CloseLoadModal();
                _nav.ShowLoadModal("Pago completado con éxito devolución en curso...");
                await Task.Delay(3000);
                EventLogger.SaveLog(EventType.Info, $"Iniciando devuelta de {_paymentViewModel.ReturnAmount}");
                ReturnMoney(_paymentViewModel.ReturnAmount);
            }
            else
            {
              
            }
        }

        //private async Task SavePay()
        //{
        //    try
        //    {
        //        // Ejecutar la lógica de guardado en el thread de UI
        //        await Application.Current.Dispatcher.InvokeAsync(async () =>
        //        {
        //            _ts.EstadoTransaccionVerb = "Aprobada";
        //            _paymentViewModel.IsPayCompleted = true;
        //            _ts.DatosPago = _paymentViewModel;
        //            _ts.TotalIngresado = _paymentViewModel.EnteredAmount;
        //            _ts.TotalDevuelta = _paymentViewModel.DispensedAmount;

        //            SetTransactionDescription();

        //            if ((_tranStateTemp == StateTransaction.Aprobada || _tranStateTemp == StateTransaction.Cancelada)
        //                && !_ts.DevueltaCorrecta)
        //            {
        //                _ts.EstadoTransaccion = (StateTransaction)((int)_tranStateTemp + 2);
        //            }
        //            else
        //            {
        //                _ts.EstadoTransaccion = _tranStateTemp;
        //            }

        //            // Ejecutar la llamada a la API en un thread separado
        //            await Task.Run(() => Api.UpdateTransaction());

        //            _nav.CloseLoadModal();

        //            if (_ts.DevueltaCorrecta == true)
        //            {
        //                EventLogger.SaveLog(EventType.Info, "Navegando a SuccessUC");
        //                try
        //                {
        //                    Dispatcher.Invoke(() => GoTo(new ListProductsUC()));
        //                    GC.Collect();
        //                    EventLogger.SaveLog(EventType.Info, "GoTo ejecutado");
        //                }
        //                catch (Exception navEx)
        //                {
        //                    EventLogger.SaveLog(EventType.Error, $"Error en navegación: {navEx.Message}", navEx);
        //                }
        //            }
        //            else
        //            {
        //                EventLogger.SaveLog(EventType.Info, "Devuelta no correcta, no navegando");
        //                _ts.EstadoTransaccion = _tranStateTemp;
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
        //        if (!_isPayCanceled)
        //        {
        //            EventLogger.SaveLog(EventType.Info, "Pago cancelado por error guardando el pago");
        //        }

        //        // Cerrar modal en el UI thread
        //        await Application.Current.Dispatcher.InvokeAsync(() =>
        //        {
        //            _nav.CloseLoadModal();
        //            _nav.ShowLoadModal("Ocurrió un error fatal intentando reportar los datos del pago. Por favor comuníquese con soporte técnico.");
        //        });
        //    }
        //}

        private async Task SavePay()
        {
            try
            {
                _ts.EstadoTransaccionVerb = "Aprobada";
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

                _nav.CloseLoadModal();

                PrintService.CleanPrintQueue();
                PrintVoucher();
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
                if (!_isPayCanceled)
                {
                            EventLogger.SaveLog(EventType.Info, "Pago cancelado por error guardando el pago");
                            await CancelPay();
                }

                CloseLoadModal();
                _nav.ShowLoadModal("Ocurrió un error fatal intentando reportar los datos del pago. Por favor comuníquese con soporte técnico.");
                    
             }
        }

        private void PrintVoucher()
        {
            try
            {
                var body = new Dictionary<string, string?>();
                var footer = new Dictionary<string, string?>();

                var header = new Dictionary<string, string?>
                {
                     {"NIT: ", "800 155 087-8"},
                     {"Transacción: ", _ts.TipoTransaccionVerb},
                     {"Código: ", _ts.Codigo.ToString()},
                     {"Fecha: ", DateTime.Now.ToString("yyyy/MM/dd")},
                     {"Hora: ", DateTime.Now.ToString("hh:mm:ss")},
                     {"Estado: ", _ts.EstadoTransaccionVerb}
                };

                if (_ts.TipoTransaccionVerb == "Pago")
                {
                    body = new Dictionary<string, string?>
                    {
                        {"Nro de Factura",_ts.IdTransaccionApi.ToString()},
                        {"Documento", _ts.Documento.ToString()},
                        {"Producto", _ts.ProductSelect.NameLine.ToString()},
                        {"Valor a Pagar", String.Format("{0:C0}",_ts.Total)},
                        {"Valor Ingresado",String.Format("{0:C0}",_ts.TotalIngresado)},
                        {"Valor Devuelto", String.Format("{0:C0}",_ts.TotalDevuelta)}
                    };
                }
                else
                {
                    body = new Dictionary<string, string?>
                    {
                        {"Nro de Factura",_ts.IdTransaccionApi.ToString()},
                        {"Documento", _ts.Documento.ToString()},
                        {"Producto", _ts.ProductSelect.NameLine.ToString()},
                        {"Valor a Pagar", String.Format("{0:C0}",_ts.Total)},
                        {"Valor a Retirar", String.Format("{0:C0}",_ts.Total)},
                        {"Valor retirado", String.Format("{0:C0}",_ts.DatosPago.DispensedAmount)}
                    };
                }
                if (_ts.TipoTransaccionVerb == "Pago" &&  !_ts.DevueltaCorrecta)
                {
                    footer = new Dictionary<string, string?>
                    {
                        {"Valor Devuelto", String.Format("{0:C0}",_ts.TotalDevuelta)}
                    };
                }
                else if(_ts.TipoTransaccionVerb == "Retiro" && !_ts.DevueltaCorrecta)
                {
                    footer = new Dictionary<string, string?>
                    {

                        {"Valor faltante", String.Format("{0:C0}",_ts.DatosPago.RemainingAmount)},
                        {"","Se abonara el dinero faltante"}
                    };
                }


                //var footer = new Dictionary<string, string?>
                //{
                //    {"Dirección", "Carrera 11 No. 18 - 132"},
                //    {"Línea de Atención", "(+57) 4 8582024"},
                //};

                _document.header = header;
                _document.body = body;
                _document.footer = footer;
                PrintService.BuildPrint(header, body, footer);
                PrintService.Start();
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }

        private void ReturnMoney(decimal returnValue)
        {
            _ts.DevueltaCorrecta = false;
#if NO_PERIPHERALS
            OnCashDispensed(returnValue, new Dictionary<int, int>());
#else
            _peripherals.StartDispenser(returnValue);
#endif

        }

        private async Task CancelPay()
        {
            try
            {
                if (_paymentViewModel.IsPayCompleted) return;
#if NO_PERIPHERALS
#else
                await _peripherals.StopAceptance();
#endif
                _isPayCanceled = true;
                _tranStateTemp = StateTransaction.Cancelada;

                if (_paymentViewModel.EnteredAmount > 0)
                {
                    _paymentViewModel.ReturnAmount = _paymentViewModel.EnteredAmount;
                    _nav.ShowLoadModal("Transacción cancelada. Devolución en curso...");
                    ReturnMoney(_paymentViewModel.EnteredAmount);
                }
                else
                {
                    _nav.ShowLoadModal("Transacción cancelada");
                    _ts.DevueltaCorrecta = true;
                   await  SavePay();
                }

            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }
        #endregion

        #region HTTP API Consume
        private void SendDispenseDetails (Dictionary<int,int> details)
        {
            
            foreach (var denom in details.Keys)
            {
                var quantity = details[denom];
                for (int i = 0; i<quantity; i++)
                {
                    SendTransactionDetail(TypeOperation.DP, Convert.ToDecimal(denom));
                }   
            }
        }

        private void SendRejectDetails(Dictionary<int, int> details)
        {

            foreach (var denom in details.Keys)
            {
                var quantity = details[denom];
                for (int i = 0; i < quantity; i++)
                {
                    SendTransactionDetail(TypeOperation.Reject, Convert.ToDecimal(denom));
                }
            }
        }

        private void SendTransactionDetail(TypeOperation op, decimal denom)
        {
            try
            {
                EventLogger.SaveLog(EventType.Info, $"Enviando detalle a la api: Op: {op}, Denom: {denom.ToString("C0")}");
                Api.CreateTransactionDetail(op, (int)denom);

            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
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
                case StateTransaction.Cancelada:
                    _ts.EstadoTransaccionVerb = "Declinada";
                    _ts.Descripcion += "Transacción Cancelada, No se realizó el pago.";
                    break;
                case StateTransaction.AprobadaSinNotificar:
                    _ts.EstadoTransaccionVerb = "Exitoso";
                    _ts.Descripcion += "Transacción aprobada, pero no se ha podido notificar el pago a la entidad correspondiente. ";
                    break;
                case StateTransaction.ErrorServicioTercero:
                    _ts.EstadoTransaccionVerb = "Declinada";
                    _ts.Descripcion += $"Transacción cancelada ocurrió un error en el servicio tercero, No se realizó el pago.";
                    break;
                default:
                    break;
            }

            if (!_ts.DevueltaCorrecta)
                _ts.Descripcion += $"Ocurrió un error durante la devolución del dinero. Cantidad faltante {_paymentViewModel.RemainingAmount.ToString("C0")}";
        }

        #endregion

        #region Timer
        public void GoTimer()
        {
            try
            {
                _timer = new TimerGeneric(STR_TIMER);

                TxtTimer.Text = STR_TIMER;

                _timer.CallBackTimeOut = () =>
                {

                    Dispatcher.Invoke(() => GoTo(new ConfigUC()));

                };

                _timer.CallBackTick = stringTimer =>
                {
                    Dispatcher.BeginInvoke((Action)delegate
                    {
                        TxtTimer.Text = stringTimer;

                    });
                };

            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }

        public void StopTimer()
        {
            try
            {
                if (_timer != null)
                {
                    _timer.CallBackTimeOut = null;
                    _timer.CallBackTick = null;
                    _timer.CallBackStop?.Invoke();
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }


        #endregion


    }

    public class PaymentViewModel : INotifyPropertyChanged
    {
       
        public event PropertyChangedEventHandler? PropertyChanged;
        

        #region Attributes
        private decimal _payAmount;

        private decimal _enteredAmount;

        private decimal _remainingAmount;

        private decimal _returnAmount;

        public decimal _dispensedAmount;

        public bool _isReturnSuccess;

        private List<Denomination> _denominations;


        public List<Denomination> Denominations
        {
            get { return _denominations; }
            set
            {
                _denominations = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Denominations)));
            }
        }

        public decimal PayAmount
        {
            get { return _payAmount; }
            set
            {
                if (_payAmount != value)
                {
                    _payAmount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PayAmount)));
                }
            }
        }

        
        public decimal EnteredAmount
        {
            get { return _enteredAmount; }
            set
            {
                if (_enteredAmount != value)
                {
                    _enteredAmount = value;
                    RemainingAmount = (EnteredAmount < PayAmount) ? PayAmount - EnteredAmount : 0;
                    ReturnAmount = (EnteredAmount > PayAmount) ? EnteredAmount - PayAmount : 0;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnteredAmount)));
                }
            }
        }

        public decimal RemainingAmount
        {
            get { return _remainingAmount; }
            set
            {
                if (_remainingAmount != value)
                {
                    _remainingAmount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingAmount)));
                }
            }
        }

        public decimal ReturnAmount
        {
            get { return _returnAmount; }
            set
            {
                if (_returnAmount != value)
                {
                    _returnAmount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReturnAmount)));
                }
            }
        }

        public decimal DispensedAmount
        {
            get { return _dispensedAmount; }
            set
            {
                if (_dispensedAmount != value)
                {
                    _dispensedAmount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DispensedAmount)));
                }
            }
        }

        public bool IsPayCompleted
        {
            get { return _isReturnSuccess; }
            set
            {
                if (_isReturnSuccess != value)
                {
                    _isReturnSuccess = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPayCompleted)));
                }
            }
        }

        #endregion

        #region Methods
        public void RefreshAmountsList(int denomination, int quantity)
        {
            
            var itemDenomination = Denominations.Where(d => d.DenominationValue == denomination).FirstOrDefault();
            if (itemDenomination == null)
            {
                Denominations.Add(new Denomination
                {
                    DenominationValue = denomination,
                    Quantity = quantity,
                    TotalDenomAmount = denomination * quantity,
                });
                return;
            }

            itemDenomination.Quantity += quantity;
            itemDenomination.TotalDenomAmount = denomination * itemDenomination.Quantity;
        }

        #endregion
    }

    public class Denomination
    {
        public decimal DenominationValue { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalDenomAmount { get; set; }
    }
}
