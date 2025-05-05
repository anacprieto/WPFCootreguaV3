using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.Variables;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.UserControls;
using static WPFCootreguaV2.Presentation.UserControls.ListOfObligationsViewModel;

namespace WPFCootreguaV2.Presentation.UserControls
{
    /// <summary>
    /// Lógica de interacción para DataPayUC.xaml
    /// </summary>
    public partial class DataPayUC : AppUserControl
    {

        private const string STR_TIMER = "02:30";
        private TimerGeneric _timer;
        private Transaction _ts;
        private ListOfObligationsViewModel _viewModel;
        private ModalWindow? _currentLoadModal = null;
        private Border? _borderSelected = null;
        private List<MunicipalDetails> _listFacture = new();

        public DataPayUC()
        {
            InitializeComponent();
            _ts = Transaction.Instance;

            _viewModel = new ListOfObligationsViewModel();
            this.DataContext = _viewModel;
            this.Unloaded += OnUnloaded;
            this.Loaded += Onloaded;
          //  InitData();
            GoTimer();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CloseLoadModal();
            StopTimer();
        }
        private void Onloaded(object sender, RoutedEventArgs e)
        {



            foreach (var facture in _ts.ListFacture)
            {

                DateTime Fecha = DateTime.ParseExact(facture.FechaPago,"yyyyMMdd",null);

                _listFacture.Add(new MunicipalDetails
                {
                    Referencia = facture.Referencia.ToString(),
                    FechaPago = Fecha.ToString("yyyy-MM-dd"),
                    Total = Convert.ToDecimal(facture.Total),
                }); ;
            }

            DataView.ItemsSource = _listFacture;
            //   _viewModel.HelpMessage = "Digita tu cédula";
        }

        private void BtnAtras_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new SelectOptionUC()));
        }

        private void BtnSalir_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new MainUC()));
        }

        private async void BtnContinuar_TouchDown(object sender, EventArgs e)
        {
            await createTrx();
        }

        private async Task createTrx()
        {
            _ts.DetailsPago = _listFacture.First(facture => facture.IsSelected);

            //foreach(var select in _ts.ListFacture)
            //{
            //    if (_ts.DetailsPago.Referencia == select.Referencia)
            //    {
            //        _ts.DetailsPago = select;
            //    }

            //}

            //if (_ts.DetailsPago.Total >= 1000000)
            //{
            //    _nav.ShowModal($"En estos momentos la maquina no cuenta con cargue suficiente para realizar esta transaccion.", new InfoModal());
            //    return;
            //};
            //if (!await HasroundAsync())
            //{
            //    _nav.NavigateTo(new MainUC());
            //    return;
            //};

            _ts.EstadoTransaccion = StateTransaction.Iniciada;
            _ts.TipoTransaccion = TypeTransaction.Pago;
            _ts.TipoPago = TypePayment.TarjetaCredito;
            _ts.TotalSinRedondear = _ts.DetailsPago.Total;
            _ts.Total = _ts.DetailsPago.Total;
            _ts.Referencia = _ts.DetailsPago.Referencia;
            await SendData();
        }

        private async Task<bool> HasroundAsync()
        {
            bool modal = false;
            decimal originalTotal = _ts.DetailsPago.Total;
            _ts.Total = (originalTotal % 100 == 0) ? originalTotal : Math.Ceiling(originalTotal / 100) * 100;

            // Verificar si hay una diferencia debido al redondeo
            if (_ts.Total - originalTotal > 0)
            {
                // Mostrar el modal y actualizar la variable modal basada en la respuesta del usuario
                modal = _nav.ShowModal($"El valor a pagar cuenta con denominaciones no retornables. Para su facilidad, fue redondeado 100 hacia arriba. ¿Está dispuesto a asumir el redondeo por ${_ts.Total - originalTotal}?", new ConfirmationModal());
            }

            // Esperar un breve periodo de tiempo (10 ms)
            await Task.Delay(10);

            // Si no hay redondeo necesario, o el usuario acepta el redondeo, retorna true
            return (_ts.Total - originalTotal == 0) || modal;

        }

        private void SelectItem(object sender, EventArgs e)
        {
            if (!(sender is FrameworkElement element)) return;

            var fila = element.DataContext as MunicipalDetails;
            if (fila == null) return;
            foreach (var facture in _listFacture)
            {
                if (facture.Referencia == fila.Referencia)
                {
                    facture.IsSelected = !facture.IsSelected;
                    _viewModel.TxtTotal = !facture.IsSelected ? 0 : facture.Total;
                    continue;
                }

                facture.IsSelected = false;
            }
            var BorderElement = sender as Border;
            if (BorderElement == null) return;

            // Si ya hay un elemento seleccionado
            if (_borderSelected != null)
            {
                _borderSelected.BorderThickness = new Thickness(4);
                _borderSelected.BorderBrush = (Brush)Application.Current.Resources["TRANSPARENTCOLOR"];
                if (_borderSelected == BorderElement)
                {
                    _borderSelected.Background = (Brush)Application.Current.Resources["QUATERNARYCOLOR"];
                    _borderSelected.Child.Visibility = Visibility.Collapsed;
                    _borderSelected = null;
                    return;
                }
                _borderSelected.Background = (Brush)Application.Current.Resources["QUATERNARYCOLOR"];
                _borderSelected.Child.Visibility = Visibility.Collapsed;
            }

            // Seleccionar el nuevo elemento
            BorderElement.Background = (Brush)Application.Current.Resources["PRIMARYCOLOR"];
            BorderElement.BorderThickness = new Thickness(4);
            BorderElement.BorderBrush = (Brush)Application.Current.Resources["LIGTHCOLOR"];

            BorderElement.Child.Visibility = Visibility.Visible;
            _borderSelected = BorderElement;
        }


     

        private void CloseLoadModal()
        {
            if (_currentLoadModal != null)
            {
                _currentLoadModal.Close();
                _currentLoadModal = null;
            }
        }

        #region Internal Operation Methods
        private async Task SendData()
        {
            ModalWindow? loadModal = null;
            try
            {
                loadModal = _nav.ShowLoadModal(Messages.VALIDATING_INFO);

                var tsCreated = await Api.CreateTransaction();
                if (tsCreated == null) throw new Exception("No se pudo enviar la transacción");

#if NO_PERIPHERALS
#else
                // Cada camara es una source incremental
              //  await VideoRecorder.Start(source: 0);
#endif

                if (loadModal != null)
                {
                    loadModal.Close();
                    loadModal = null;
                }

                  Dispatcher.Invoke(() => _nav.NavigateTo(new CardPaymentUC()));



            }
            catch (Exception ex)
            {
                if (loadModal != null)
                {
                    loadModal.Close();
                    loadModal = null;

                    EnableView();
                }

                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
                _nav.ShowModal("Ocurrió un error validando la información. Por favor intente nuevamente.", new InfoModal());
            }
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

    public class ListOfObligationsViewModel : INotifyPropertyChanged
    {

        private decimal _txtTotal = 0;

        public decimal TxtTotal
        {
            get
            {
                return _txtTotal;
            }
            set
            {
                _txtTotal = value;
                OnPropertyRaised(nameof(TxtTotal));
            }
        }


        public class MunicipalDetails
        {
            public string Referencia { get; set; } = string.Empty;
            public string FechaPago { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public bool IsSelected { get; set; } = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyRaised(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

        }
    }

}
