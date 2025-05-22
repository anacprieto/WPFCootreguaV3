using ControlzEx.Standard;
using DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Threading;
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
    public partial class TypeTransUC : AppUserControl
    {
        private const string STR_TIMER = "02:30";
        private TimerGeneric _timer;
        private Transaction _ts;
        private TypeTransViewModel _viewModel;
        private ModalWindow? _currentLoadModal = null;

        #region Regex properies

        private string _referencia = string.Empty;
        #endregion

        public TypeTransUC()
        {
            InitializeComponent();
            _ts = Transaction.Instance;
            Utilities.Speak("Bienvenido, selecciona la operación a realizar.");

            _viewModel = new TypeTransViewModel();
            this.DataContext = _viewModel;
            this.Unloaded += OnUnloaded;
            this.Loaded += Onloaded;

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

                ValidateStatus();
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
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CloseLoadModal();
            ScannerController.Stop();
            ScannerController.ScannerDataReceived -= OnScannerDataReceived;
        }
        private void Onloaded(object sender, RoutedEventArgs e)
        {
            ScannerController.ScannerDataReceived += OnScannerDataReceived;
            ScannerController.Start();
            //       _viewModel.HelpMessage = "Ingresa número de cuenta o referente";
        }

        private void BtnAtras_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new SelectOptionUC()));

        }

        private void BtnSalir_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new MainUC()));

        }
      
        #region ScannerEvents
        private async void OnScannerDataReceived(string data)
        {
            DisableView();
            try
            {

                string scannerRead = data.Replace("\u001d", "").Replace("\r", "");

                EventLogger.SaveLog(EventType.Info, "Data readed from scanner", scannerRead);

                //_NoConvenio = scannerRead.Substring(3, 13);
                _referencia = scannerRead.Substring(20, 24);
               // _valorPagar = scannerRead.Substring(48, 12);
                //_fechaPago = scannerRead.Substring(62, 8);

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

        
    }

    public class TypeTransViewModel : INotifyPropertyChanged
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
