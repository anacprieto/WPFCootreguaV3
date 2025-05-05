using System;
using System.Collections.Generic;
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
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.UserControls;
using ManualInputViewModel = WPFCootreguaV2.UserControls.ManualInputViewModel;

namespace WPFCootreguaV2.Presentation.UserControls
{
    /// <summary>
    /// Lógica de interacción para SelectOptionUC.xaml
    /// </summary>
    public partial class SelectOptionUC : AppUserControl
    {

        private const string STR_TIMER = "02:30";
        private TimerGeneric _timer;
        private Transaction _ts;
        private ManualInputViewModel _viewModel;
        private ModalWindow? _currentLoadModal = null;

        public SelectOptionUC()
        {
            InitializeComponent();

            _ts = Transaction.Instance;

            _viewModel = new ManualInputViewModel();
            this.DataContext = _viewModel;
            this.Unloaded += OnUnloaded;
            this.Loaded += Onloaded;
            GoTimer();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CloseLoadModal();
            StopTimer();
        }
        private void Onloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.HelpMessage = "Ingresa número de cuenta o referente";
        }

        private void BtnAtras_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new MenuUC()));

        }

        private void BtnSalir_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new MainUC()));

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

        private void Btn_ReferenciaMouseDown(object sender, MouseButtonEventArgs e)
        {
            _ts.TipoConsulta = "Referencia ";
            Dispatcher.Invoke(() => GoTo(new ScanInputUC()));
        }

        private void Btn_EscaneaMouseDown(object sender, MouseButtonEventArgs e)
        {
            _ts.TipoConsulta = "Referencia";
            Dispatcher.Invoke(() => GoTo(new ScanInputUC()));
        }
    }
}
