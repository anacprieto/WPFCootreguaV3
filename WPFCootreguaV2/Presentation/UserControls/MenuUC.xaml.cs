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
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.UserControls;
using WPFCootreguaV2.Modals;
using ManualInputViewModel = WPFCootreguaV2.UserControls.ManualInputViewModel;
using WPFCootreguaV2.Domain.UIServices.Integrations;
using WPFCootreguaV2.Domain.Peripherals;

namespace WPFCootreguaV2.Presentation.UserControls
{
    /// <summary>
    /// Lógica de interacción para MenuUC.xaml
    /// </summary>
    public partial class MenuUC : AppUserControl
    {
        private const string STR_TIMER = "02:30";
        private TimerGeneric _timer;
        private Transaction _ts;
        private ManualInputViewModel _viewModel;
        private ModalWindow? _currentLoadModal = null;

        public MenuUC()
        {
            InitializeComponent();
            this.Unloaded += OnUnloaded;
            _ts = Transaction.Instance;
            GoTimer();

       //     printData();
        }

        public void printData()
        {
            #region DataVoucher Pago

            //PrinterServicesCommands.StartBuilder();
            ////               PrintService.RegisterInstruction(() => PrintCommands.SetAlignment(1));
            ////               PrintService.RegisterInstruction(() => PrintCommands.PrintDiskbmpfile(PrintCommands.ConfigureStringToSend("C:\\PrinterAssets\\Voucher.bmp")));

            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.SetAlignment(1));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.SetSizetext(1, 1));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.SetBold(1));

            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.SetBold(1));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Transaccion:", $"29100", 40)), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Fecha:", $"{DateTime.Now.ToString("yyyy/MM/dd")}", 40)), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Hora:", $"{DateTime.Now.ToString("HH:mm:ss")}", 40)), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Estado:", $"Aprobada", 40)), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Referencia:", $"0100000002870010000000000", 40)), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Pago sin Redondear:", $"176,101", 40)), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Pago redondeado:", $"176,200", 40)), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Valor Ingresado:", $"177,000", 40)), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Valor Devuelto:", $"$800", 40)), 0));

            //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintMarkcutpaper(0));
            //PrinterServicesCommands.ExecuteBuilder();

            #endregion
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopTimer();
        }

        private void BtnPay_Predial(object sender, EventArgs e)
        {
            _ts.TipoRecaudo = "Predial";
            _ts.ProcedureManager = new SIGAMPayInvoiceManager();
            Dispatcher.Invoke(() => GoTo(new SelectOptionUC()));
        }

        private void BtnPay_ICA(object sender, EventArgs e)
        {
            _ts.TipoRecaudo = "ICA";
            _ts.ProcedureManager = new SIGAMPayInvoiceManager();
        }

        private void BtnAtras_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new MainUC()));
        }

        private void BtnSalir_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new MainUC()));
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

      
    }
}
