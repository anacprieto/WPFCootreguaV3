using ControlzEx.Standard;
using DB;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.ApiService.IntegrationModels;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.ApiService;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Integrations;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.UIServices.Integrations;
using WPFCootreguaV2.Domain.Variables;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.Presentation.UserControls;

namespace WPFCootreguaV2.UserControls
{
    /// <summary>
    /// Lógica de interacción para ScanInputUC.xaml
    /// </summary>
    public partial class IdentificationUC : AppUserControl
    {
        private const string STR_TIMER = "02:30";
        private TimerGeneric _timer;
        private Transaction _ts;
        private IdentificationViewModel _viewModel;
        private ModalWindow? _currentLoadModal = null;

        #region Regex properies
        private string _regexReferencia = @"8020(0*[1-9]\d*)\u001d3900";
        private string _regexValorPago = @"\u001d3900(0*[1-9]\d*)\u001d96";
        private string _regexFechaVencimiento = @"\u001d96(\d*)";

        private string _referencia = string.Empty;
        private string _valorPagar = string.Empty;
        private string _fechaPago = string.Empty;
        private string _NoConvenio = string.Empty;
        #endregion

        public IdentificationUC()
        {
            InitializeComponent();
            _ts = Transaction.Instance;

            _viewModel = new IdentificationViewModel();
            this.DataContext = _viewModel;
            this.Unloaded += OnUnloaded;
            this.Loaded += Onloaded;
            Keyboard.KeyboardPressed += OnKeyboardPressed;
            GoTimer();

            //IcoReferencia.Visibility = Visibility.Visible;

        }

        // Fix for CS0121: The issue arises because there are two identical method signatures for OnKeyboardPressed in the same class.
        // To resolve this, one of the duplicate methods must be removed.

        private async void OnKeyboardPressed(object? sender, string keyPressed)
        {
            if (string.IsNullOrEmpty(keyPressed)) return;
            if (keyPressed == "Remove")
            {
                string text = TxtIdentification.Text;
                TxtIdentification.Text = (text.Length > 1) ? text.Remove(text.Length - 1) : "";
                if (text.Length > 1)
                {
                    TxtIdentification.Text = text.Remove(text.Length - 1);
                    return;
                }
                TxtIdentification.Text = "";
                return;
            }

            if (keyPressed == "Clear")
            {
                TxtIdentification.Text = "";
                return;
            }

            TxtIdentification.Text += keyPressed;
            await Task.Delay(100);
        }

        private void Btn_Cancelar_Touch(object sender, EventArgs e)
        {
        }
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            //CloseLoadModal();
            StopTimer();
            ScannerController.Stop();
            ScannerController.ScannerDataReceived -= OnScannerDataReceived;
        }
        private void Onloaded(object sender, RoutedEventArgs e)
        {
            ScannerController.ScannerDataReceived += OnScannerDataReceived;
            ScannerController.Start();
            //       _viewModel.HelpMessage = "Ingresa número de cuenta o referente";
        }

        #region UI EVENTS

        private void TxtIdentification_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox text = (TextBox)sender;
                int length = text.Text.Length;
                if (length > 10)
                {
                    text.Text = text.Text.Remove(text.Text.Length - 1);
                }
                _viewModel.StatusMsg = "";

                if (length >= 5)
                {
                    BtnContinuar.IsEnabled = true;
                    BtnContinuar.Opacity = 1;
                }
                else
                {
                    BtnContinuar.IsEnabled = false;
                    BtnContinuar.Opacity = 0.3;
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Info, "Se consulto la cc ", this.GetType().Name, ex.ToString());

            }
        }
        //BtnContinuar_TouchDown

        private async void BtnCancel_TouchDown(object sender, EventArgs e)
        {
            _ = Dispatcher.BeginInvoke(() => BtnCancelar.Visibility = Visibility.Collapsed);

            if (!_nav.ShowModal(Messages.CANCEL_TRANSACTION, new ConfirmationModal()))
            {
                _ = Dispatcher.BeginInvoke(() => BtnCancelar.Visibility = Visibility.Visible);
                return;
            }
            EventLogger.SaveLog(EventType.Info, "Transacción cancelada por el usuario.");
            await CancelPay();
        }

        private async Task CancelPay()
        {
            try
            {
//                if (_paymentViewModel.IsPayCompleted) return;
//#if NO_PERIPHERALS
//#else
//                await _peripherals.StopAceptance();
//#endif
//                _isPayCanceled = true;
//                _tranStateTemp = StateTransaction.Cancelada;

//                if (_paymentViewModel.EnteredAmount > 0)
//                {
//                    _paymentViewModel.ReturnAmount = _paymentViewModel.EnteredAmount;
//                    _currentLoadModal = _nav.ShowLoadModal("Transacción cancelada. Devolución en curso...");
//                    ReturnMoney(_paymentViewModel.EnteredAmount);
//                }
//                else
//                {
//                    _currentLoadModal = _nav.ShowLoadModal("Transacción cancelada");
//                    _ts.DevueltaCorrecta = true;
//                    await SavePay();
//                }

            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }
        private void TxtInvoice_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.StatusMsg = "";
            if (TxtIdentification.Text.Length > 26)
            {
                TxtIdentification.Text = TxtIdentification.Text.Substring(0, TxtIdentification.Text.Length - 1);
                return;
            }
        }
        private void BtnAtras_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new SelectOptionUC()));

        }

        private void BtnSalir_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new MainUC()));

        }
        private async void BtnConsultar_Touch(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //TxtStatusMsg.Visibility = Visibility.Visible;
            //string doc = TxtIdentification.Text;
            //await RequestDocData(doc);
            ValidateData();

        }

        private void ValidateData()
        {
            try
            {
                if (TxtIdentification.Text.Length < 5)
                {
                    Dispatcher.Invoke(() => { _viewModel.StatusMsg = "Debe ingresar un número de cédula valido."; });

                    return;
                }

                _ts.Documento = TxtIdentification.Text;

                Authentication();
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }

        private void Authentication()
        {
            try
            {
                Task.Run(async () =>
                {
                    Authentication authentication = new Authentication
                    {
                        Identification = AppConfig.Get("AuthenticationUser"),
                        Password = "texto ejmplo"
                        //EncryptorEcity.Encrypt(AppConfig.Get("AuthenticationPass"), AppConfig.Get("KeyCootregua")),
                    };

                    var authen = await ApiIntegration.CallApiCootregua("ControllerCootreguaValidateUsers", authentication);

                    if (!string.IsNullOrEmpty(authen))
                    {
                        var data = JsonConvert.DeserializeObject<Authentication>(authen);

                        if (data.Validate == 1 && data.CodUsuario > 0)
                        {
                            _ts.Codigo = Convert.ToInt32(data.CodUsuario);

                            GetPerson();
                        }
                        else
                        {
                            CloseLoadModal();
                            _nav.ShowLoadModal("Por favor ingrese un número de documento válido.");

                            //CloseLoadModal Switcher.ModalMS("No se pudo autenticar el kiosco con el servicio, por favor intenta de nuevo.");
                            GoTimer();                        }
                    }
                    else
                    {
                        CloseLoadModal();
                        _nav.ShowLoadModal("No hay comunicación con el servicio, por favor intenta de nuevo.");
                        GoTimer();
                    }
                });


                StopTimer();
                CloseLoadModal();
            }
            catch (Exception ex)
            {
              //  Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }


        private async Task GetPerson()
        {
            try
            {
                Person person = new Person
                {
                    CodPerson = _ts.Codigo,
                    Identification = _ts.Documento,
                };

                var pers = await ApiIntegration.CallApiCootregua("ControllerCootreguaGetPerson", person);
                CloseLoadModal();


                if (!string.IsNullOrEmpty(pers))
                {
                    var data = JsonConvert.DeserializeObject<Person>(pers);

                    _ts.DataPerson = data;
                    Dispatcher.Invoke(() => GoTo(new AuthenticationUC()));


                }
                else
                {
                    //CloseLoadModal();
                    _nav.ShowLoadModal("No se encontrarón registros con este número de documento, por favor intenta de nuevo.");
                    StopTimer();
                }
            }
            catch (Exception ex)
            {
               // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }

        private void CloseLoadModal()
        {
            if (_currentLoadModal != null)
            {
                _currentLoadModal.Close();
                _currentLoadModal = null;
            }
        }

        #endregion

        #region ScannerEvents
        private async void OnScannerDataReceived(string data)
        {
            DisableView();
            try
            {

                string scannerRead = data.Replace("\u001d", "").Replace("\r", "");

                EventLogger.SaveLog(EventType.Info, "Data readed from scanner", scannerRead);

                _NoConvenio = scannerRead.Substring(3, 13);
                _referencia = scannerRead.Substring(20, 24);
                _valorPagar = scannerRead.Substring(48, 12);
                _fechaPago = scannerRead.Substring(62, 8);

                RequestConsultData request = new RequestConsultData();

                request.idComercio = AppConfig.Get("idComercioAlcaldia");
                request.password = AppConfig.Get("passwordAlcaldia");
                request.idCliente = _referencia;
                request.claveConsulta = "01";

                await _ts.ProcedureManager.GetDataPay();
            }
            catch (Exception ex)
            {
                _nav.ShowModal($"Hubo un error inesperado al realizar la consulta intenta nuevamente", new Modals.InfoModal());
                await Application.Current.Dispatcher.InvokeAsync(() => GoTo(new MainUC()));
            }
            EnableView();

        }
        #endregion


        private async Task RequestDocData(string document)
        {
            ModalWindow? loadModal = null;

            try
            {
                if (document.Length < 6)
                {
                    loadModal = _nav.ShowLoadModal("Por favor ingrese un número de referencia valido.");

                    return;
                }

                if (_ts.TipoConsulta == "Documento")
                {
                    _ts.Documento = document;
                }

                _ts.Referencia = document;


                RequestConsultData request = new RequestConsultData();

                request.idComercio = AppConfig.Get("idComercioAlcaldia");
                request.password = AppConfig.Get("passwordAlcaldia");
                request.idCliente = _ts.Referencia;
                request.claveConsulta = "01";

                await _ts.ProcedureManager.GetDataPay();



            }
            catch (ProcedureException ex)
            {

                Dispatcher.Invoke(() => { _viewModel.StatusMsg = ex.Message; });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => { _viewModel.StatusMsg = "Ocurrió un error durante la consulta de los datos. Por favor intenta de nuevo."; });

            }

            if (loadModal != null)
            {
                loadModal.Close();
                loadModal = null;
            }

        }

        #region Timer
        public void GoTimer()
        {
            try
            {
                _timer = new TimerGeneric(STR_TIMER);

                TxtTimer.Text = STR_TIMER;

                _timer.CallBackTimeOut = () =>
                {

                    //         Dispatcher.Invoke(() => GoTo(new SelectInputUC()));


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


        private void BtnLimpiar_Touch(object sender, EventArgs e)
        {
            Image key = (Image)sender;
            string tag = key.Tag.ToString() ?? "";

            if (tag == "Clear")
            {
                TxtIdentification.Text = "";
                return;
            }

        }
    }

    public class IdentificationViewModel : INotifyPropertyChanged
    {
        private string _statusMsg = string.Empty;

        public string StatusMsg
        {
            get
            {
                return _statusMsg;
            }
            set
            {
                _statusMsg = value;
                OnPropertyRaised(nameof(StatusMsg));
            }
        }
        private string _title = string.Empty;
        public string HelpMessage
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyRaised(nameof(HelpMessage));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyRaised(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

        }
    }
}
