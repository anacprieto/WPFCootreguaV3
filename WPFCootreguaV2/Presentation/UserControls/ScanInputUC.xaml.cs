using DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
using WPFCootreguaV2.ApiService.IntegrationModels;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Integrations;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.UserControls;
using static WPFCootreguaV2.Presentation.UserControls.ListOfObligationsViewModel;

namespace WPFCootreguaV2.Presentation.UserControls
{
    /// <summary>
    /// Lógica de interacción para ScanInputUC.xaml
    /// </summary>
    public partial class ScanInputUC : AppUserControl
    {

        private const string STR_TIMER = "01:30";
        private TimerGeneric _timer;
        private Transaction _ts;
        private ScanInputViewModel _viewModel;


        #region Regex properies
        private string _regexReferencia = @"8020(0*[1-9]\d*)\u001d3900";
        private string _regexValorPago = @"\u001d3900(0*[1-9]\d*)\u001d96";
        private string _regexFechaVencimiento = @"\u001d96(\d*)";

        private string _referencia = string.Empty;
        private string _valorPagar = string.Empty;
        private string _fechaPago = string.Empty;
        private string _NoConvenio = string.Empty;
        #endregion

        public ScanInputUC()
        {
            InitializeComponent();

            _ts = Transaction.Instance;
            _viewModel = new ScanInputViewModel();

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            GoTimer();

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            ScannerController.ScannerDataReceived += OnScannerDataReceived;
            ScannerController.ScannerError += OnScannerError;
            ScannerController.Start();
          //  SetTitle();
        }
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopTimer();
            ScannerController.Stop();
            ScannerController.ScannerDataReceived -= OnScannerDataReceived;
            ScannerController.ScannerError -= OnScannerError;

        }

        #region ScannerEvents
        private async void OnScannerDataReceived(string data)
        {
            DisableView();
            try
            {

                EventLogger.SaveLog(EventType.Info, "Data readed from scanner 1", data);

                string scannerRead = data.Replace("\u001d", "").Replace("\r", "");

                scannerRead = "4157709998937215802002303161390000000001009620250331";

                EventLogger.SaveLog(EventType.Info, "Data readed from scanner 2", data);

                _NoConvenio = scannerRead.Substring(3, 13);
                _referencia = scannerRead.Substring(20, 8);
                _valorPagar = scannerRead.Substring(32, 10);
                _fechaPago = scannerRead.Substring(44, 8);

                _ts.Referencia = _referencia;

                MunicipalDetails? obj = new MunicipalDetails
                {
                    Referencia = _referencia,
                    Total = decimal.Parse(_valorPagar),
                    FechaPago = _fechaPago,
                };
                await Filter(obj);
                if (obj == null) throw new Exception("no se logró scannear su factura, intente más tarde");
                if (obj.Referencia == "-1") throw new Exception("La factura ya expiró");
                if (obj.Referencia == "-2") throw new Exception("La factura ya fue recaudada");

                _ts.ListFacture = new List<MunicipalDetails>();

                _ts.ListFacture.Add(obj);

                await _ts.ProcedureManager.GetDataPay();
            }
            catch (ProcedureException ex)
            {
                OnScannerError(ex.Message);
            }
            catch (Exception ex)
            {
                OnScannerError(ex.Message);
            }
    //        EnableView();
            ScannerController.Stop();
            await Task.Delay(150);
            ScannerController.Start();

        }

        private async Task Filter(MunicipalDetails? details)
        {
            EventLogger.SaveLog(EventType.Info, "Objeto scanner", details);

            if (string.IsNullOrEmpty(details.Referencia) || string.IsNullOrEmpty(details.FechaPago) || details.Total == 0)
            {
                details = null;
                return;
            }

            //if (DateTime.ParseExact(details.FechaPago, "yyyyMMdd", CultureInfo.InvariantCulture) < DateTime.Today)
            //{
            //    details.Referencia = "-1";
            //    return;
            //}

            //var existingRefs = (await DB_TransactionService.GetAll()).Where(trx =>
            //      trx.IdStateTransaction == (int)StateTransaction.Aprobada ||
            //      trx.IdStateTransaction == (int)StateTransaction.AprobadaErrorDevuelta ||
            //      trx.IdStateTransaction == (int)StateTransaction.AprobadaSinNotificar)
            //     .Select(trx => trx.Reference).ToList();

            //if (existingRefs.Contains(details.Referencia))
            //{
            //    details.Referencia = "-2";
            //    return;
            //}
        }

        private void OnScannerError(string errMsg)
        {
            _nav.ShowModal($"{errMsg}", new InfoModal());
            Dispatcher.Invoke(() => GoTo(new MainUC()));
            return;
        }

        #endregion

        #region Buttons
        private void BtnAtras_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new MainUC()));
        }
        private void BtnSalir_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new MainUC()));
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

                    Dispatcher.Invoke(() => GoTo(new MainUC()));

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

    public class ScanInputViewModel : INotifyPropertyChanged
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


        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyRaised(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

        }
    }

}
