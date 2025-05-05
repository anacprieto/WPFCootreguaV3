using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.Variables;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.Presentation.UserControls;

namespace WPFCootreguaV2.UserControls
{

    public partial class ConfigUC : AppUserControl
    {


        private ConfigViewModel _viewModel;
        public ConfigUC()
        {
            InitializeComponent();

            DataContext = new ConfigViewModel();
            _viewModel = (ConfigViewModel)DataContext;

            Transaction.Reset();
            this.Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(200);
            await InitPayPad();
        }

        private async Task InitPayPad()
        {
            EventLogger.SaveLog(EventType.Info, "Inicializando Pay+");
            try
            {
                // TODO: Finalizar cualquier grabación

                _viewModel.StatusMsg = Messages.LOGIN_IN;
                if (!await Api.Login())
                {
                    await Retry(Messages.NO_SERVICE + " No se logró iniciar sesión en los servicios de E-City.");
                    return;
                }

                _viewModel.StatusMsg = Messages.VALIDATING_PAYPLUS;
                if (!await Api.Validate())
                {
                    await Retry(Messages.NO_SERVICE + " No cuenta con suficiente dinero para operar.");
                    return;
                }
#if NO_PERIPHERALS
#else
                // Validación de perifericos
                _viewModel.StatusMsg = Messages.VALIDATING_PERIPHERALS;
                var peripheralController = ArduinoController.Instance;
                if (!await peripheralController.SendStart())
                {
                    await Retry(Messages.NO_SERVICE + " " + Messages.PERIPHERALS_FAILED_VALIDATE);
                    return;
                }

                peripheralController.StartAcceptance(0);
                await Task.Delay(1000);
                await peripheralController.StopAceptance();
#endif
                _viewModel.StatusMsg = "Exitoso";
               
                Dispatcher.Invoke(()=> GoTo(new MainUC()));
            }
            catch(Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución {ex.Message}", ex);
                await Retry(Messages.NO_SERVICE+" Ocurrió un error inesperado"+" Presiona continuar para intentar de nuevo.");
            }

            
           
        }

        private async Task Retry(string msgModal)
        {
            
            _nav.ShowModal(msgModal, new InfoModal());
            await InitPayPad();
        }
    }

    public class ConfigViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _statusMsg = string.Empty;
        public string StatusMsg
        {
            get
            {
                return _statusMsg;
            }
            set
            {
                if (_statusMsg != value)
                {
                    _statusMsg = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMsg)));
                }
                
            }
        }

    }


}
