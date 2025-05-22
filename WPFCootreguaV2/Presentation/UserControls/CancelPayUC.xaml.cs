using DB;
using Microsoft.AspNet.SignalR.Client.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.ApiService.IntegrationModels;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.ApiService;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Integrations;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.Peripherals.Printer;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.Variables;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.Models;
using WPFCootreguaV2.Presentation.UserControls;

namespace WPFCootreguaV2.UserControls
{
    public partial class CancelPayUC : AppUserControl
    {
        private Transaction _ts;
        private ArduinoController _peripherals;
        private CancelPayViewModel _paymentViewModel;

        private bool _isPayCanceled = false;

        private ModalWindow? _currentLoadModal;

        private StateTransaction _tranStateTemp = StateTransaction.Iniciada;
        private MenuBackground bg;
        private decimal ValueReturn;
        private DocumentFormat _document = new();
        private TimerGeneric _timer;

        public CancelPayUC()
        {
            InitializeComponent();
            bg = new MenuBackground();
            _ts = Transaction.Instance;

            //this.transaction.statePaySuccess = false;
            ValueReturn = 0;

            ChangeBackground(EBackground.Paga);

            _ts = Transaction.Instance;
            _ts.DevueltaCorrecta = false;
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            ReturnMoney();

        }
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            DisableView();
            PrintService.CleanPrintQueue();
            PrintVoucher();
            var currentModal = _nav.ShowModal("Imprimiendo factura...");

            await Task.Delay(TimeSpan.FromSeconds(PrintService.numberOfSecondsToPrint));
            _timer.ControlTimer(pauseOrder: true);
            while (!(PrintService.recentImpressionSuccess ?? false))
            {
                currentModal.Close();
                if (!HandlePrintingError()) break;
                currentModal = _nav.ShowModal("Imprimiendo factura...");
                await Task.Delay(TimeSpan.FromSeconds(PrintService.numberOfSecondsToPrint));
            }
            currentModal.Close();
            _timer.ControlTimer(pauseOrder: false);
            EnableView();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            PrintService.recentImpressionSuccess = false;
        }

        //private void FinishCancelPay()
        //{
        //    try
        //    {
        //        AdminPayPlus.ControlPeripherals.ClearValues();

        //        if (!string.IsNullOrEmpty(transaction.Observation))
        //        {
        //            AdminPayPlus.SaveErrorControl(transaction.Observation, "", EError.Device, ELevelError.Medium);
        //        }

        //        _ts.EstadoTransaccion = StateTransaction.Cancelada;

        //        _ts.StatePay = "Cancelada";

        //        Api.UpdateTransaction();


        //        //AdminPayPlus.UpdateTransaction(transaction);

        //        //Utilities.PrintVoucher(transaction);
        //        PrintService.CleanPrintQueue();
        //        PrintVoucher();
        //        Thread.Sleep(5000);

        //        Switcher.CLose();
        //    }
        //    catch (Exception ex)
        //    {
        //        Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        //    }
        //}
        private void FinishBtn(object sender, MouseButtonEventArgs e)
        {
            try
            {
                FinishTransaction();

            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Error al detener VideoRecorder: {ex.Message}", ex);
                MessageBox.Show($"Error al detener VideoRecorder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void FinishTransaction()
        {
            if (string.IsNullOrEmpty(_ts.Calificacion))
            {
                _ts.Calificacion = "Sin calificación";
            }
            //TODO: Endpoint para calificación de transacción

            if (!_ts.DatosPago._isReturnSuccess)
            {
                var loadModal = _nav.ShowModal(
                    "No se pudo entregar la totalidad del dinero hay un faltante de:" +
                    $" {_ts.DatosPago.RemainingAmount.ToString("C0")} " +
                    ". Por favor comuníquese con un administrador.");
                await Task.Delay(TimeSpan.FromSeconds(20));
                if (loadModal != null)
                {
                    loadModal.Close();
                    loadModal = null;
                }
            }

            Dispatcher.Invoke(() => GoTo(new ConfigUC()));
        }
        private void PrintVoucher()
        {
            //StopVideoRecording();
            try
            {
                if (_ts != null)
                {
                    SolidBrush color = new SolidBrush(System.Drawing.Color.Black);
                    Font fontKey = new Font("Arial", 8, System.Drawing.FontStyle.Bold);
                    Font fontValue = new Font("Arial", 8, System.Drawing.FontStyle.Regular);
                    int y = 0;
                    int sum = 25;
                    int x = 150;
                    int xKey = 15;

                    var header = new Dictionary<string, string?>
                    {
                        {"","Comprobante de pago"},
                        {"","COOTREGUA"},
                        {"","SEDE ADMINISTRATIVA"},
                        {"","Calle 10 20-54, Barrio La Esperanza"},
                        {"","San josé del Guaviare, Guaviare"},
                        {"","CEL. 315 8404476"},
                        {"NIT:","800 155 087-8"},
                        {"", "========================================"},
                    

                    };

                    var body = new Dictionary<string, string?>
                    {
                        //DATOS DE LAS TRANSACCIONES
                        {"Transacción",_ts.IdTransaccionApi.ToString()},
                        {"Código",_ts.IdPaypad.ToString()},
                        {"Fecha:",DateTime.Now.ToString("yyyy/MM/dd")},
                        {"Hora",DateTime.Now.ToString("hh:mm:ss")},
                        {"Estado",_ts.EstadoTransaccion.ToString()},

                        {"Valor a Pagar", String.Format("{0:C0}", _ts.Total)},
                        {"Documento", _ts.Documento},
                        {"Valor Ingresado", String.Format("{0:C0}", _ts.DatosPago.EnteredAmount)},
                        {"Valor Devuelto", String.Format("{0:C0}", _ts.DatosPago.DispensedAmount)},

                        {"Valor a Retirar",String.Format("{0:C0}", _ts.Total)},
                        {"Valor retirado", String.Format("{0:C0}", _ts.DatosPago.ReturnAmount)},
                        {"", "========================================"}
                    };


                    var footer = new Dictionary<string, string?>
                    {
                        {"", "E-city Software"},
                        {"", "Se abonara el dinero faltante"},
                    };


                _document.header = header;
                _document.body = body;
                _document.footer = footer;
                PrintService.BuildPrint(header, body, footer);
                PrintService.Start();
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }
        private bool HandlePrintingError()
        {
            bool result = false;

            BillReportViewModel model = new BillReportViewModel
            {
                Title = "Estimado Cliente: "
            };


            Application.Current.Dispatcher.Invoke(delegate
            {
                var _currentModal = new BillReportWindow(model,_document.header, _document.body, _document.footer);
                _currentModal.ShowDialog();
                if (_currentModal.DialogResult.HasValue)
                {
                    result = _currentModal.DialogResult.Value;
                    if (result) PrintVoucher();
                }
            });
            return result;
        }

        private void ReturnMoney()
        {
            try
            {
                ValueReturn = _ts.DatosPago.EnteredAmount - _ts.DatosPago.DispensedAmount;
                txtValueReturn.Text = string.Format("{0:C0}", ValueReturn);
                _paymentViewModel = new CancelPayViewModel
                {
                    PayAmount = _ts.DatosPago.PayAmount,
                    EnteredAmount = _ts.DatosPago.EnteredAmount,
                    ReturnAmount = ValueReturn,
                    DispensedAmount = _ts.DatosPago.DispensedAmount,
                    Denominations = new List<Denomination>()
                };
                _ts.DevueltaCorrecta = false;
#if NO_PERIPHERALS
                OnCashDispensed(ValueReturn, new Dictionary<int, int>());
#else
            _peripherals.StartDispenser(returnValue);
#endif

            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }
        public void ChangeBackground(EBackground eBackground)
        {

            try
            {
                //if (bg == null)
                //{
                //    bg = new MenuBackground(); // Tipo correcto
                //}

                Dispatcher.Invoke(() =>
                {
                    switch (eBackground)
                    {
                        case EBackground.Identificate:
                            // Usar el recurso estático
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/identificate.jpg";
                            break;
                        case EBackground.Identificate2:
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/identificate2.jpg";
                            break;
                        case EBackground.Autenticate:
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/autenticate.jpg";
                            break;
                        case EBackground.Autenticate2:
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/autenticate2.jpg";
                            break;
                        case EBackground.Productos:
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/elige.jpg";
                            break;
                        case EBackground.Paga:
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/paga.jpg";
                            break;
                        case EBackground.Generico:
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/generic.jpg";
                            break;
                    }

                    this.DataContext = bg;
                });
            }
            catch (Exception ex)
            {
                // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }
      

        #region Responses to Peripheral Events
        private async void OnCashDispensed(decimal totalDispensed, Dictionary<int, int> details)
        {

            _paymentViewModel.DispensedAmount = totalDispensed;

            _paymentViewModel.RemainingAmount = _paymentViewModel.ReturnAmount - _paymentViewModel.DispensedAmount;
            string strValueToReturn = _paymentViewModel.RemainingAmount.ToString("C0");

            SendDispenseDetails(details);

            CloseLoadModal();

            if (_paymentViewModel.DispensedAmount == _paymentViewModel.ReturnAmount)
            {
                _ts.DevueltaCorrecta = true;
                await SavePay();
            }
            else
            {
                _currentLoadModal = _nav.ShowModal("No se pudo entregar la totalidad del dinero hay un faltante de:" + $" {strValueToReturn} " + ".Por favor comunícate con un administrador.");
                await Task.Delay(5000); // Timer para mostrar la modal y que se pueda leer
                _ts.DevueltaCorrecta = false;
                await SavePay();
            }

        }


        private async Task SavePay()
        {
            try
            {
                if (_paymentViewModel == null) throw new InvalidOperationException("PaymentViewModel is null");
                _paymentViewModel.IsPayCompleted = true;
                _ts = Transaction.Instance ?? throw new InvalidOperationException("Transaction instance is null");
                _ts.DatosPago.EnteredAmount = _paymentViewModel.EnteredAmount;
                _ts.TotalDevuelta = _paymentViewModel.DispensedAmount;

                SetTransactionDescription();

                if ((_tranStateTemp == StateTransaction.Aprobada || _tranStateTemp == StateTransaction.Cancelada)
                    && !_ts.DevueltaCorrecta)
                {
                    // Si el estado de transacción es aprobada o cancelada y además hay error de devuelta se cambia a su respectivo estado
                    // CanceladoErrorDevuelta o AprobadaErrorDevuelta
                    _ts.EstadoTransaccion  = (StateTransaction)((int)_tranStateTemp + 2);
                    _ts.DatosPago.RemainingAmount = _paymentViewModel.RemainingAmount;
                }
                else
                {
                    _ts.EstadoTransaccion = _tranStateTemp;
                }

                Api.UpdateTransaction();

                CloseLoadModal();
                Dispatcher.Invoke(() => GoTo(new SuccessUC()));

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
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error fatal intentando reportar los datos del pago. Por favor comuníquese con soporte técnico.");
                _currentLoadModal = _nav.ShowModal("Ocurrió un error fatal intentando reportar los datos del pago. Por favor comuníquese con soporte técnico.");
            }
        }

        private async Task CancelPay()
        {
            try
            {
                _isPayCanceled = true;
                _ts.EstadoTransaccion = StateTransaction.Cancelada;
                _ts.DatosPago.RemainingAmount = _paymentViewModel.RemainingAmount;
                Api.UpdateTransaction();
                CloseLoadModal();
                Dispatcher.Invoke(() => GoTo(new SuccessUC()));
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }


        #endregion

        #region UI control methods

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

    }

    public class CancelPayViewModel : INotifyPropertyChanged
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

}
