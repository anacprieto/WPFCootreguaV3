using ControlzEx.Standard;
using DB;
using Ecity.DigitalPersona.ReaderUareU;
using Microsoft.EntityFrameworkCore;
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
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Integrations;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.UIServices.Integrations;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.Models;
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
        private int CantIntentos;

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

            //try
            //{
            //    _ts = Transaction.Instance;
            //    CantIntentos = 0;
            //    if (_ts.Type == ETransactionType.Registros)
            //    {
            //        ChangeBackground(EBackground.Autenticate2);
            //    }
            //    else
            //    {
            //        ChangeBackground(EBackground.Autenticate);
            //    }

            //    LoadReader();

            //    //Switcher.Timer(true);

            //    //Utilities.Speak("Ubica tu dedo en el lector biometrico.");
            //}
            //catch (Exception ex)
            //{
            //    Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }



        //}
        //private void LoadReader()
        //{
        //    try
        //    {
        //        EcityReader.callbackError = Error =>
        //        {
        //            EcityReader.callbackTemplate = null;
        //            EcityReader.callbackError = null;

        //            AdminPayPlus.SaveErrorControl("ERROR DEL HUELLERO: " + Error, "", EError.Aplication, ELevelError.Medium);
        //        };

        //        if (EcityReader.OpenReader())
        //        {
        //            EcityReader.callbackTemplate = Template =>
        //            {
        //                EcityReader.callbackTemplate = null;
        //                EcityReader.callbackError = null;

        //                EcityReader.CancelCaptureAndCloseReader(EcityReader.OnCaptured);

        //                if (!string.IsNullOrEmpty(Template))
        //                {
        //                    ValidateUser(Template);
        //                }
        //                else
        //                {
        //                    Switcher.Timer(false);
        //                    Switcher.ModalMS("No se pudo capturar la huella, por favor intentalo de nuevo.");
        //                    Switcher.Timer(true);
        //                    LoadReader();
        //                }
        //            };

        //            EcityReader.StartCaptureAsync(EcityReader.OnCaptured);
        //        }
        //        else
        //        {
        //            Switcher.Timer(false);
        //            Switcher.ModalMS("El huellero no se pudo habilitar, por favor intentalo de nuevo.");
        //            Switcher.Navigate(UserControlView.Identification, null);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        //    }
        //}
        //public void ChangeBackground(EBackground eBackground)
        //{
        //    try
        //    {
        //        Dispatcher.BeginInvoke((Action)delegate
        //        {
        //            switch (eBackground)
        //            {
        //                case EBackground.Identificate:
        //                    bg.Background = "/Images/Backgrounds/identificate.jpg";
        //                    break;
        //                case EBackground.Identificate2:
        //                    bg.Background = "/Images/Backgrounds/identificate2.jpg";
        //                    break;
        //                case EBackground.Autenticate:
        //                    bg.Background = "/Images/Backgrounds/autenticate.jpg";
        //                    break;
        //                case EBackground.Autenticate2:
        //                    bg.Background = "/Images/Backgrounds/autenticate2.jpg";
        //                    break;
        //                case EBackground.Productos:
        //                    bg.Background = "/Images/Backgrounds/elige.jpg";
        //                    break;
        //                case EBackground.Paga:
        //                    bg.Background = "/Images/Backgrounds/paga.jpg";
        //                    break;
        //                case EBackground.Generico:
        //                    bg.Background = "/Images/Backgrounds/generic.jpg";
        //                    break;
        //            }

        //            this.DataContext = bg;
        //        });
        //        GC.Collect();
        //    }
        //    catch (Exception ex)
        //    {
        //        Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        //    }
        //}
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


        private async void OpcionButton_Click(object sender, EventArgs e)
        {

            var control = sender as FrameworkElement;

            // Validar que el control tenga un Tag definido
            if (control == null || control.Tag == null)
            {
                return;
            }

            string tag = control.Tag.ToString();

            //if (tag == "Gimnasio")
            //{
            //    _ts.EspacioReservar = tag;
            //    Requestconsultarinscripciongimnasio request = new Requestconsultarinscripciongimnasio();
            //    request.id_formulario = _ts.IdRegistroFormularioInder.ToString();
            //    await _ts.ProcedureManagerInder.Getconsultarinscripciongimnasio(request);
            //}
            //else if (tag.Contains("Piscina"))
            //{
            //    _ts.EspacioReservar = tag;
            //    Dispatcher.Invoke(() => GoTo(new SelectTipoRegistroPiscinaUC()));
            //}
            //else if (tag.Contains("Turcos"))
            //{
            //    _ts.EspacioReservar = tag;
            //    // Ir primero a ManualInput para verificar si el usuario existe
            //    Dispatcher.Invoke(() => GoTo(new ManualInputUC("Turcos")));
            //}
            //else
            //{
            //    // Verificar si es día de mantenimiento (lunes normal)
            //    bool isDayOfMaintenance = AppConfig.Get("DiaMantenimiento") == diaActual;

            //    // Si es lunes, verificar si es festivo
            //    bool isMondayHoliday = false;
            //    if (diaActual == "lunes")
            //    {
            //        // Verificar si el lunes actual es festivo
            //        isMondayHoliday = ColombianHolidayHelper.IsColombianHoliday(DateTime.Today);
            //        EventLogger.SaveLog(EventType.Info, $"Verificando si hoy ({DateTime.Today:yyyy-MM-dd}) es festivo: {isMondayHoliday}", null);
            //    }

            //    // Mostrar mensaje de mantenimiento solo si es día de mantenimiento y no es un lunes festivo
            //    if (isDayOfMaintenance && !isMondayHoliday)
            //    {
            //        _nav.ShowModal($"Los días {diaActual} se realiza proceso de mantenimiento a este espacio.", new InfoModal());
            //    }
            //    else
            //    {
            //        _ts.EspacioReservar = tag;
            //        Dispatcher.Invoke(() => GoTo(new ManualInputUC(tag)));
            //    }
            //}
        }

        #region UI EVENTS

        private void BtnCancelar_TouchDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new SelectOptionUC()));

        }

        private void BtnSalir_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new MainUC()));

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
            //    InputInvoice.Text = "";
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
