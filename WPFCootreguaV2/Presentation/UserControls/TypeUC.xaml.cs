using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.Peripherals.Printer;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Modals;

namespace WPFCootreguaV2.UserControls
{
    public partial class TypeUC : AppUserControl
    {
        private const string STR_TIMER = "00:45";
        
        private Transaction _ts;
        private TimerGeneric _timer;
        private DocumentFormat _document = new();

        private TypeUCViewModel _typeViewModel;


        public TypeUC()
        {
            InitializeComponent();

            
            _ts = Transaction.Instance;

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
           
        }

        public class TypeUCViewModel : INotifyPropertyChanged
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


}
