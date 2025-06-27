using Microsoft.AspNet.SignalR.Client.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
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
using WPFCootreguaV2.Models;
using WPFCootreguaV2.Presentation.UserControls;

namespace WPFCootreguaV2.UserControls
{
    public partial class PaymentViejoUC : AppUserControl
    {
        private Transaction _ts;
        private ArduinoController _peripherals;
        private PaymentViejoViewModel _paymentViejoViewModel;

        private bool _isPayCanceled = false;


        private StateTransaction _tranStateTemp = StateTransaction.Iniciada;

        public PaymentViejoUC()
        {
            InitializeComponent();
            EventLogger.SaveLog(EventType.Info, "Comienza proceso de pago, Iniciando PaymentUC.");
            _nav = Navigator.Instance;
            _ts = Transaction.Instance;
            _ts.DevueltaCorrecta = false;
            _ts.statePaySuccess = false;
            Utilities.Speak("Por favor ingresa el dinero.");
            // OrganizeValues();


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
            dynamicButton2.Click += ExecuteScanner;

            void ExecuteScanner(object sender, EventArgs e)
            {

                OnCashIn(80000);
            }

            MainGrid.Children.Add(dynamicButton);

#else
    _peripherals = ArduinoController.Instance;
    _peripherals.CashIn += OnCashIn;
    _peripherals.CashDispensed += OnCashDispensed;
    _peripherals.DispenserReject += OnDispenserReject;
    _peripherals.PeripheralError += OnPeripheralError;
#endif

            // Agregar eventos
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;


        }
        private async void BtnCancel_TouchDown(object sender, MouseButtonEventArgs e)
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            InitViewModel();
#if NO_PERIPHERALS
#else
            _peripherals.StartAcceptance(_paymentViejoViewModel.PayAmount);
#endif
        }
        private void InitViewModel()
        {

            try
            {
                //_paymentViejoViewModel = new PaymentViewModel
                //{
                //    PayAmount = _ts.Total,
                //    RemainingAmount = _ts.Total,
                //    //ImgContinue = Visibility.Hidden,
                //    //ImgCancel = Visibility.Visible,
                //    //ImgCambio = Visibility.Hidden,
                //    ReturnAmount = 0,
                //    EnteredAmount = 0,
                //    Denominations = new List<Denomination>(),
                //    DispensedAmount = 0
                //};
                //this.DataContext = _paymentViejoViewModel;

            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Info, MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex.ToString());
            }
        }
        private async void OnCashDispensed(decimal totalDispensed, Dictionary<int, int> details)
        {
            try
            {
                // Actualizar datos del pago
                _paymentViejoViewModel.DispensedAmount = totalDispensed;
                _paymentViejoViewModel.RemainingAmount = _paymentViejoViewModel.ReturnAmount - _paymentViejoViewModel.DispensedAmount;

                string strValueToReturn = _paymentViejoViewModel.RemainingAmount.ToString("C0");

                // Enviar detalles de la dispensación
                SendDispenseDetails(details);

                // Cerrar cualquier modal activo
                _nav.CloseLoadModal();
                _nav.CloseModal();

                // Verificar si la devolución fue completa
                if (_paymentViejoViewModel.DispensedAmount == _paymentViejoViewModel.ReturnAmount)
                {
                    // Devolución exitosa
                    _ts.DevueltaCorrecta = true;
                    EventLogger.SaveLog(EventType.Info, "Devolución exitosa, guardando pago...");

                    // Mostrar mensaje de éxito brevemente
                    _nav.ShowLoadModal("Devolución completada exitosamente. Procesando...");
                    await Task.Delay(2000);
                    _nav.CloseLoadModal();

                  //  await SavePay();
                }
                else
                {
                    // Devolución incompleta
                    _ts.DevueltaCorrecta = false;

                    string mensaje = $"No se pudo entregar la totalidad del dinero, hay un faltante de {strValueToReturn}. " +
                                   "Por favor comuníquese con un administrador.";

                    EventLogger.SaveLog(EventType.Warning, mensaje);

                    // Mostrar modal informativo (no de carga) por tiempo prolongado
                    _nav.ShowModal(mensaje, new InfoModal());

                    // Esperar que el usuario lea el mensaje
                    await Task.Delay(8000);

                    // Cerrar modal y proceder a guardar
                    _nav.CloseModal();

                 //   await SavePay();
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Error en OnCashDispensed: {ex.Message}", ex);

                // Limpiar modales
                _nav.CloseLoadModal();
                _nav.CloseModal();

                // Marcar como devolución incorrecta
                _ts.DevueltaCorrecta = false;

                // Mostrar error y proceder
                _nav.ShowModal("Error durante la devolución de dinero. Contacte al administrador.", new InfoModal());
                await Task.Delay(5000);
                _nav.CloseModal();

                try
                {
                   // await SavePay();
                }
                catch
                {
                    // Si también falla SavePay, cancelar transacción
                    if (!_isPayCanceled)
                    {
                        CancelTransaction();
                    }
                }
            }
        }
        //private async void OnCashDispensed(decimal totalDispensed, Dictionary<int, int> details)
        //{

        //    _paymentViejoViewModel.DispensedAmount = totalDispensed;

        //    _paymentViejoViewModel.RemainingAmount = _paymentViejoViewModel.ReturnAmount - _paymentViejoViewModel.DispensedAmount;
        //    string strValueToReturn = _paymentViejoViewModel.RemainingAmount.ToString("C0");

        //    SendDispenseDetails(details);

        //    _nav.CloseModal();

        //    if (_paymentViejoViewModel.DispensedAmount == _paymentViejoViewModel.ReturnAmount)
        //    {
        //        _ts.DevueltaCorrecta = true;
        //        EventLogger.SaveLog(EventType.Info, "Devuelta exitosa, Guardando pago..");
        //        await SavePay();
        //    }
        //    else
        //    {
        //        _nav.ShowModal("No se pudo entregar la totalidad del dinero hay un faltante de:" + $" {strValueToReturn} " + ".Por favor comunícate con un administrador.");
        //        await Task.Delay(5000); // Timer para mostrar la modal y que se pueda leer
        //        _ts.DevueltaCorrecta = false;
        //        EventLogger.SaveLog(EventType.Info, "No se pudo entregar la totalidad del dinero hay un faltante de:" + $" {strValueToReturn} " + ".Por favor comunícate con un administrador.");

        //        await SavePay();
        //    }

        //}

        #region
        private void SendDispenseDetails(Dictionary<int, int> details)
        {

            foreach (var denom in details.Keys)
            {
                var quantity = details[denom];
                for (int i = 0; i < quantity; i++)
                {
                    SendTransactionDetail(TypeOperation.DP, Convert.ToDecimal(denom));
                }
            }
        }
        #endregion




        #region Responses to Peripheral Events


   
        private async void OnCashIn(decimal value)
        {
            EventLogger.SaveLog(EventType.Info, "Init view" + _paymentViejoViewModel);

            if (_paymentViejoViewModel?.IsPayCompleted ?? true) return;

            _paymentViejoViewModel.EnteredAmount += value;

            _paymentViejoViewModel.RefreshAmountsList(Convert.ToInt32(value), 1);

            SendTransactionDetail(TypeOperation.AP, value, 1);


            if (_paymentViejoViewModel.EnteredAmount < _paymentViejoViewModel.PayAmount) return;

            //Finaliza pago cantidad completa
            _ = Dispatcher.BeginInvoke(() => BtnCancel.Visibility = Visibility.Collapsed);
#if NO_PERIPHERALS
#else
            await _peripherals.StopAceptance();
#endif

            // _nav.ShowModal("Estamos procesando el pago...", new LoadModal());
            //await Task.Delay(3000);
            //_nav.CloseModal();
            EventLogger.SaveLog(EventType.Info, "Iniciando Proceso de pago...");

            await PaymentProcess(
            _paymentViejoViewModel);
        }
        private async Task PaymentProcess(PaymentViejoViewModel? paymentViewModel)
        {
            bool isPaySuccess;

            try
            {
                await NotifyPay();
                EventLogger.SaveLog(EventType.Info, "Pago Completado en integración");
                _tranStateTemp = StateTransaction.Aprobada;
                isPaySuccess = true;
            }
            catch (ProcedureException ex)
            {
                if (ex.Message == "El pago está en proceso de verificación. Por favor espere la confirmación.")
                {
                    EventLogger.SaveLog(EventType.Error, $"El pago está en proceso de verificación. Por favor espere la confirmación.");
                    _nav.CloseModal();
                    isPaySuccess = true;
                    _tranStateTemp = StateTransaction.AprobadaSinNotificar;
                    _nav.ShowModal(ex.Message, new InfoModal());
                }
                else
                {
                    _nav.CloseModal();
                    isPaySuccess = false;
                    _nav.ShowModal(ex.Message, new InfoModal());
                    await CancelPay();
                    return;
                }
            }
            catch (Exception ex)
            {
                _nav.CloseModal();
            
                isPaySuccess = false;
                _tranStateTemp = StateTransaction.ErrorServicioTercero;
                EventLogger.SaveLog(EventType.Error, $"Error durante procedimiento de notificación {ex.Message}", ex);
                _nav.ShowModal("Se presentó un problema durante el proceso de notificación del pago.", new InfoModal());
                await CancelPay();
                return;
            }
            if (isPaySuccess)
            {
                if (paymentViewModel != null)
                {
                    await FinishSuccessfulPay();
                }
                else
                {
                    // Si paymentViewModel es nulo, se guarda el estado de la transacción como "AprobadaSinNotificar"
                    _ts.DevueltaCorrecta = true;

                   // _ts.paymentProcess.DevueltaCorrecta = true;
                    //await SavePay();
                }
                return;
            }

            if (paymentViewModel != null)
            {
                paymentViewModel.ReturnAmount = paymentViewModel.EnteredAmount;
                ReturnMoney(paymentViewModel.ReturnAmount);
            }
            else
            {
                EventLogger.SaveLog(EventType.Error, "El objeto PaymentViewModel es nulo.");
                // Manejar el caso cuando paymentViewModel es nulo, por ejemplo:
                // - Mostrar un mensaje de error al usuario
                // - Realizar alguna acción de recuperación o cancelación del pago
                // - Registrar el error en el registro de eventos
            }
        }

        private void SendTransactionDetail(TypeOperation op, decimal denom, int quantity)
        {
            try
            {
                EventLogger.SaveLog(EventType.Info, $"Enviando detalle a la api: Op: {op}, Denom: ${denom:N0} COP, Cantidad: {quantity}");
                Api.CreateTransactionDetail1(op, (int)denom, quantity);

            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }
        #endregion

        #region UI control methods
        private async void BtnCancel_TouchDown(object sender, EventArgs e)
        {
            _ = Dispatcher.BeginInvoke(() => BtnCancel.Visibility = Visibility.Collapsed);

            if (!_nav.ShowModal(Messages.CANCEL_TRANSACTION, new ConfirmationModal()))
            {
                _ = Dispatcher.BeginInvoke(() => BtnCancel.Visibility = Visibility.Visible);
                _ = Dispatcher.BeginInvoke(() => BtnCancel.Visibility = Visibility.Visible);
                return;
            }
            EventLogger.SaveLog(EventType.Info, "Pago cancelado por el usuario.");
            Dispatcher.Invoke(() => GoTo(new CancelPayUC()));


           // await CancelPay();
        }

        #endregion

        #region Internal Operation process

        #region PagoFactura

        public async Task NotifyPay()
        {
            try
            {

                bool isPaySuccess = false;

                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {

                    EventLogger.SaveLog(EventType.Info, "Pago Completado en integración");
                    _tranStateTemp = StateTransaction.Aprobada;
                    isPaySuccess = true;
                 
                   
                    if (isPaySuccess)
                    {                   
                        await FinishSuccessfulPay();
                        return;
                    }

                    _paymentViejoViewModel.ReturnAmount = _paymentViejoViewModel.EnteredAmount;
                    ReturnMoney(_paymentViejoViewModel.ReturnAmount);

                });
                Dispatcher.Invoke(() => GoTo(new SuccessUC()));

            }
            catch (Exception ex)
            {

            }
        }

        #endregion




        /*
        private async Task FinishSuccessfulPay()
        {
            if (_paymentViejoViewModel.EnteredAmount > 0 && _paymentViejoViewModel.ReturnAmount > 0)
            {

                _nav.CloseModal();
                _nav.ShowModal("Pago completado con éxito devolución en curso...");
                await Task.Delay(3000);
                EventLogger.SaveLog(EventType.Info, $"Iniciando devuelta de {_paymentViejoViewModel.ReturnAmount}");
                ReturnMoney(_paymentViejoViewModel.ReturnAmount);
            }
            else
            {
                _ts.DevueltaCorrecta = true;
                await SavePay();
            }
        }

        private async Task SavePay()
        {
            try
            {
                _ts.EstadoTransaccion = StateTransaction.Aprobada;
                _ts.EstadoTransaccionVerb = "Aprobada";
                _paymentViejoViewModel.IsPayCompleted = true;
                _ts.DatosPago = _paymentViejoViewModel;
                _ts.TotalIngresado = _paymentViejoViewModel.EnteredAmount;
                _ts.TotalDevuelta = _paymentViejoViewModel.DispensedAmount;

                SetTransactionDescription();
                

                if ( (_tranStateTemp == StateTransaction.Aprobada || _tranStateTemp == StateTransaction.Cancelada)
                    && !_ts.DevueltaCorrecta )
                {
                    // Si el estado de transacción es aprobada o cancelada y además hay error de devuelta se cambia a su respectivo estado
                    // CanceladoErrorDevuelta o AprobadaErrorDevuelta
                    _ts.EstadoTransaccion = (StateTransaction) ((int)_tranStateTemp + 2);
                    EventLogger.SaveLog(EventType.Error, $"" + _ts.EstadoTransaccion);

                }
                else
                {
                    _ts.EstadoTransaccion = _tranStateTemp;
                    EventLogger.SaveLog(EventType.Error, $"" + _ts.EstadoTransaccion);

                }

                Api.UpdateTransaction();

                _nav.CloseModal();
                Dispatcher.Invoke(() => GoTo(new SuccessUC()));

                
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
                if (!_isPayCanceled)
                {
                    EventLogger.SaveLog(EventType.Info, "Pago cancelado por error guardando el pago");
                    CancelTransaction();
                }

                _nav.CloseModal();
                _nav.ShowModal("Ocurrió un error fatal intentando reportar los datos del pago. Por favor comuníquese con soporte técnico.");
            }
        }
        */

        private async Task FinishSuccessfulPay()
        {
            try
            {
                if (_paymentViejoViewModel.EnteredAmount > 0 && _paymentViejoViewModel.ReturnAmount > 0)
                {
                    _nav.CloseModal();
                    _nav.ShowLoadModal("Pago completado con éxito, devolución en curso...");

                    await Task.Delay(3000);

                    EventLogger.SaveLog(EventType.Info, $"Iniciando devuelta de {_paymentViejoViewModel.ReturnAmount}");

                    // Ejecutar devolución de dinero
                    await Task.Run(() => ReturnMoney(_paymentViejoViewModel.ReturnAmount));

                    _nav.CloseLoadModal();
                }
                else
                {
                    _ts.DevueltaCorrecta = true;
                }

                // Siempre guardar el pago al final
                //await SavePay();
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Error en FinishSuccessfulPay: {ex.Message}", ex);
                _nav.CloseLoadModal();
                _nav.CloseModal();

                // En caso de error, intentar guardar el pago de todas formas
                try
                {
                   // await SavePay();
                }
                catch
                {
                    // Si falla completamente, cancelar transacción
                    if (!_isPayCanceled)
                    {
                        CancelTransaction();
                    }
                }
            }
        }

        //private async Task SavePay()
        //{
        //    try
        //    {
        //        // Mostrar modal de procesamiento
        //        _nav.ShowLoadModal("Procesando pago...");

        //        // Configurar datos de la transacción
        //        _ts.EstadoTransaccion = StateTransaction.Aprobada;
        //        _ts.EstadoTransaccionVerb = "Aprobada";
        //        _paymentViejoViewModel.IsPayCompleted = true;
        //        _ts.DatosPago = _paymentViejoViewModel;
        //        _ts.TotalIngresado = _paymentViejoViewModel.EnteredAmount;
        //        _ts.TotalDevuelta = _paymentViejoViewModel.DispensedAmount;

        //        SetTransactionDescription();

        //        // Determinar el estado final de la transacción
        //        if ((_tranStateTemp == StateTransaction.Aprobada || _tranStateTemp == StateTransaction.Cancelada)
        //            && !_ts.DevueltaCorrecta)
        //        {
        //            // Si hay error de devuelta, cambiar al estado correspondiente
        //            _ts.EstadoTransaccion = (StateTransaction)((int)_tranStateTemp + 2);
        //            EventLogger.SaveLog(EventType.Warning, $"Transacción con error de devuelta: {_ts.EstadoTransaccion}");
        //        }
        //        else
        //        {
        //            _ts.EstadoTransaccion = _tranStateTemp;
        //            EventLogger.SaveLog(EventType.Info, $"Transacción completada: {_ts.EstadoTransaccion}");
        //        }

        //        // Actualizar transacción en la API
        //        await Task.Run(() => Api.UpdateTransaction());

        //        // Cerrar modal de carga
        //        _nav.CloseLoadModal();

        //        // Navegar a la pantalla de éxito
        //        Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            GoTo(new SuccessUC());
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        EventLogger.SaveLog(EventType.Error, $"Error al guardar pago: {ex.Message}", ex);

        //        _nav.CloseLoadModal();
        //        _nav.CloseModal();

        //        // Mostrar mensaje de error
        //        _nav.ShowModal("Ocurrió un error al procesar el pago. Por favor comuníquese con soporte técnico.",
        //                      new InfoModal());

        //        // Cancelar transacción si no se ha cancelado ya
        //        if (!_isPayCanceled)
        //        {
        //            EventLogger.SaveLog(EventType.Info, "Cancelando transacción por error al guardar el pago");
        //            CancelTransaction();
        //        }
        //    }
        //}

        private void ReturnMoney(decimal returnValue)
        {
            _ts.DevueltaCorrecta = false;
#if NO_PERIPHERALS
            OnCashDispensed(returnValue, new Dictionary<int, int>());
#else
            _peripherals.StartDispenser(returnValue);
#endif

        }
        private void CancelTransaction()
        {
            try
            {
                var ms=string.Format("Estimado {0}, no se pudo notificar el pago. Se le hará la devolución del dinero ingresado.", _ts.DataPerson.FirstName);
                _nav.ShowModal(ms,new InfoModal());
                _nav.CloseModal();
                Dispatcher.Invoke(() => GoTo(new CancelPayUC()));
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Info, MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex.ToString());
            }
        }

        private async Task CancelPay()
        {
            try
            {
                _paymentViejoViewModel.ImgContinue = Visibility.Hidden;

                _paymentViejoViewModel.ImgCancel = Visibility.Hidden;

                if (_paymentViejoViewModel.IsPayCompleted) return;
#if NO_PERIPHERALS
#else
                await _peripherals.StopAceptance();
#endif
                _isPayCanceled = true;
                _tranStateTemp = StateTransaction.Cancelada;

                //if (_paymentViejoViewModel.EnteredAmount > 0)
                //{
                //        _paymentViejoViewModel.ImgContinue = Visibility.Visible;
                //        _paymentViejoViewModel.ReturnAmount = _paymentViejoViewModel.EnteredAmount;
                //        _nav.ShowModal("Transacción cancelada. Devolución en curso...", new LoadModal());
                //        await Task.Delay(3000);
                //        ReturnMoney(_paymentViejoViewModel.EnteredAmount);
                //}
                //else
                //{
                //        _paymentViejoViewModel.ImgCancel = Visibility.Visible;
                //        _nav.ShowModal("Transacción cancelada", new InfoModal());
                //        _ts.DevueltaCorrecta = true;
                //        await SavePay();
                // }

                Dispatcher.Invoke(() => GoTo(new CancelPayUC()));
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }


        #endregion

        #region HTTP API Consume
       
        private void SendTransactionDetail(TypeOperation op, decimal denom)
        {
            try
            {
                EventLogger.SaveLog(EventType.Info, $"Enviando detalle a la api: Op: {op}, Denom: {denom.ToString("C0")}");
                Api.CreateTransactionDetail2(op, (int)denom);

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
                _ts.Descripcion += $"Ocurrió un error durante la devolución del dinero. Cantidad faltante {_paymentViejoViewModel.RemainingAmount.ToString("C0")}";
        }

        #endregion

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
#if NO_PERIPHERALS
#else
            _peripherals.CashIn -= OnCashIn;
            _peripherals.CashDispensed -= OnCashDispensed;
            _peripherals.DispenserReject -= OnDispenserReject;
            _peripherals.PeripheralError -= OnPeripheralError;
#endif
        }

    }

    public class PaymentViejoViewModel : INotifyPropertyChanged
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

        private Visibility _imgCancel;

        private Visibility _imgContinue;

        private Visibility _imgCambio;

        public bool _statePay;
        public Visibility ImgCancel
        {
            get { return _imgCancel; }
            set
            {
                _imgCancel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImgCancel)));
            }
        }

        public Visibility ImgContinue
        {
            get { return _imgContinue; }
            set
            {
                _imgContinue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImgContinue)));
            }
        }

        public Visibility ImgCambio
        {
            get { return _imgCambio; }
            set
            {
                _imgCambio = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImgCambio)));
            }
        }

        public bool StatePay
        {
            get { return _statePay; }
            set
            {
                if (_statePay != value)
                {
                    _statePay = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatePay)));
                }
            }
        }

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
