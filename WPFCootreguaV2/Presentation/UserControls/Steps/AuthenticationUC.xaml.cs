using ControlzEx.Standard;
using DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.ApiService.IntegrationModels;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Integrations;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.UIServices.Integrations;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.Presentation.UserControls;

namespace WPFCootreguaV2.UserControls
{
    /// <summary>
    /// Lógica de interacción para ScanInputUC.xaml
    /// </summary>
    public partial class AuthenticationUC : AppUserControl
    {
        private const string STR_TIMER = "02:30";
        private TimerGeneric _timer;
        private Transaction _ts;
        private AuthenticationUCViewModel _viewModel;
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

        public AuthenticationUC()
        {
            InitializeComponent();
            _ts = Transaction.Instance;

            _viewModel = new AuthenticationUCViewModel();
            this.DataContext = _viewModel;
            this.Unloaded += OnUnloaded;
            this.Loaded += Onloaded;
            Keyboard.KeyboardPressed += OnKeyboardPressed;
            GoTimer();

            IcoReferencia.Visibility = Visibility.Visible;

        }
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CloseLoadModal();
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
        private async void OnKeyboardPressed(object? sender, string keyPressed)
        {
            if (string.IsNullOrEmpty(keyPressed)) return;
            if (keyPressed == "Remove")
            {
                string text = InputInvoice.Text;
                InputInvoice.Text = (text.Length > 1) ? text.Remove(text.Length - 1) : "";
                if (text.Length > 1)
                {
                    InputInvoice.Text = text.Remove(text.Length - 1);
                    return;
                }
                InputInvoice.Text = "";
                return;
            }

            if (keyPressed == "Clear")
            {
                InputInvoice.Text = "";
                return;
            }

            InputInvoice.Text += keyPressed;
            await Task.Delay(100);
        }

        private void TxtInvoice_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.StatusMsg = "";
            if (InputInvoice.Text.Length > 26)
            {
                InputInvoice.Text = InputInvoice.Text.Substring(0, InputInvoice.Text.Length - 1);
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
            TxtStatusMsg.Visibility = Visibility.Visible;
            string doc = InputInvoice.Text;
            await RequestDocData(doc);
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
                _nav.ShowModal($"Hubo un error inesperado al realizar la consulta intenta nuevamente", new InfoModal());
                await Application.Current.Dispatcher.InvokeAsync(() => GoTo(new MainUC()));
            }
            EnableView();

        }
        #endregion


        private async Task RequestDocData(string document)
        {
            try
            {
                if (document.Length < 6)
                {
                    _nav.ShowModal("Por favor ingrese un número de referencia valido.", new InfoModal());
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

            CloseLoadModal();
        }

        private void CloseLoadModal()
        {
            if (_currentLoadModal != null)
            {
                _currentLoadModal.Close();
                _currentLoadModal = null;
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
                InputInvoice.Text = "";
                return;
            }

        }
    }

    public class AuthenticationUCViewModel : INotifyPropertyChanged
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
