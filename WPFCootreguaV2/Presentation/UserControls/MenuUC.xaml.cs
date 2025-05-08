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
using System.Reflection;
using WPFCootreguaV2.Domain.ApiService.Models;
using System.Diagnostics;
using WPFCootreguaV2.Domain.Enumerables;
using static WPFCootreguaV2.Domain.Enumerables.Switcher;
using WPFCootreguaV2.Models;

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
        private MenuBackground bg;
        //private ManualInputViewModel _viewModel;
        //private ModalWindow? _currentLoadModal = null;

        public MenuUC()
        {
            InitializeComponent();
            this.Unloaded += OnUnloaded;
            _ts = Transaction.Instance;
            GoTimer();

            try
            {
                bg = new MenuBackground();

                // Cambiamos el fondo a Identificación
                ChangeBackground(EBackground.Identificate);

                // Navegamos a la vista de identificación
                Navegar(new IdentificationUC());
                //Navegar(new AuthenticationUC());
                //Navegar(new ListProductsUC());
            }
            catch (Exception ex)
            {
                // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }

        /*
        InitializeComponent();
        this.Unloaded += OnUnloaded;
        _ts = Transaction.Instance;
        GoTimer();

        try
        {
            bg = new MenuBackground();

            //GoTo(new IdentificationUC());

            ChangeBackground(EBackground.Identificate);

            Navegar(UserControlView.Identification, null); 

            ChangeBackground(EBackground.Identificate);

        }
        catch (Exception ex)
        {
          //  Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        }

        //     printData();
    }
    public void Navegar(UserControlView newPage, Transaction ts)
    {
        try
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                this.cc_View.Content = null;

                switch (newPage)
                {
                    case UserControlView.Identification:
                        this.cc_View.Content = new IdentificationUC();
                        break;
                    case UserControlView.Authentication:
                        this.cc_View.Content = new AuthenticationUC();
                        break;
                    case UserControlView.Products:
                        this.cc_View.Content = new ListProductsUC();
                        break;
                    case UserControlView.Pay:
                        this.cc_View.Content = new PaymentUC();
                        break;
                    case UserControlView.PaySuccess:
                        this.cc_View.Content = new SuccessUC();
                        break;
                    case UserControlView.Withdrawal:
                        this.cc_View.Content = new WithdrawalUC();
                        break;
                    case UserControlView.CancelPay:
                        this.cc_View.Content = new CancelUC();
                        break;
                }
            });
            GC.Collect();
        }
        catch (Exception ex)
        {
           // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        }
    }
    public void ChangeBackground(EBackground eBackground)
    {
        try
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                switch (eBackground)
                {
                    case EBackground.Identificate:
                        bg.Background = "pack://application:,,,/Images/Backgrounds/identificate.jpg";

                        break;
                    case EBackground.Identificate2:
                        bg.Background = "/Images/Backgrounds/identificate2.jpg";
                        break;
                    case EBackground.Autenticate:
                        bg.Background = "/Images/Backgrounds/autenticate.jpg";
                        break;
                    case EBackground.Autenticate2:
                        bg.Background = "/Images/Backgrounds/autenticate2.jpg";
                        break;
                    case EBackground.Productos:
                        bg.Background = "/Images/Backgrounds/elige.jpg";
                        break;
                    case EBackground.Paga:
                        bg.Background = "/Images/Backgrounds/paga.jpg";
                        break;
                    case EBackground.Generico:
                        bg.Background = "/Images/Backgrounds/generic.jpg";
                        break;
                }

                this.DataContext = bg;
            });
            GC.Collect();
        }
        catch (Exception ex)
        {
            //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        }
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

    private void Btn_Identification(object sender, EventArgs e)
    {
        //_ts.TipoRecaudo = "Predial";
       // _ts.ProcedureManager = new SIGAMPayInvoiceManager();
        Dispatcher.Invoke(() => GoTo(new IdentificationUC()));
    }

    private void Btn_Authentication(object sender, EventArgs e)
    {
        //_ts.TipoRecaudo = "ICA";
        Dispatcher.Invoke(() => GoTo(new AuthenticationUC()));

    }

    private void Btn_Products(object sender, EventArgs e)
    {
        //_ts.TipoRecaudo = "Predial";
        // _ts.ProcedureManager = new SIGAMPayInvoiceManager();
        Dispatcher.Invoke(() => GoTo(new ListProductsUC()));
    }

    private void Btn_Withdrawal(object sender, EventArgs e)
    {
        //_ts.TipoRecaudo = "ICA";
        Dispatcher.Invoke(() => GoTo(new PaymentUC()));

        //AQUI LLAMA A LA VENTANA Withdrawal

    }

    private void Btn_CancelPay(object sender, EventArgs e)
    {
        //_ts.TipoRecaudo = "ICA";
        Dispatcher.Invoke(() => GoTo(new PaymentUC()));

        //AQUI LLAMA A LA VENTANA CancelPay

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


    #endregion*/



        // Ya no necesitamos este método porque usamos GoTo heredado de AppUserControl
        // public void Navegar(UserControlView newPage, Transaction ts) { ... }
        
        public void ChangeBackground(EBackground eBackground)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    switch (eBackground)
                    {
                        case EBackground.Identificate:
                            // Usar el recurso estático
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/identificate.jpg";
                            break;
                        case EBackground.Identificate2:
                            bg.Background = "/Images/Backgrounds/identificate2.jpg";
                            break;
                        case EBackground.Autenticate:
                            bg.Background = "/Images/Backgrounds/autenticate.jpg";
                            break;
                        case EBackground.Autenticate2:
                            bg.Background = "/Images/Backgrounds/autenticate2.jpg";
                            break;
                        case EBackground.Productos:
                            bg.Background = "/Images/Backgrounds/elige.jpg";
                            break;
                        case EBackground.Paga:
                            bg.Background = "/Images/Backgrounds/paga.jpg";
                            break;
                        case EBackground.Generico:
                            bg.Background = "/Images/Backgrounds/generic.jpg";
                            break;
                    }

                    this.DataContext = bg;
                });
            }
            catch (Exception ex)
            {
                // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }
        // Método auxiliar para navegar y cambiar el fondo al mismo tiempo
        protected void NavigateWithBackground(UserControl view, EBackground background)
        {
            ChangeBackground(background);
            GoTo(view);
        }
        public void Navegar(AppUserControl newPage)
        {
            try
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    this.cc_View.Content = null;
                    this.cc_View.Content = newPage;
                });
                GC.Collect();
            }
            catch (Exception ex)
            {
                // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }
        //public void Navegar(UserControlView newPage, Transaction ts)
        //{
        //    try
        //    {
        //        Dispatcher.BeginInvoke((Action)delegate
        //        {
        //            this.cc_View.Content = null;

        //            switch (newPage)
        //            {
        //                case UserControlView.Identification:
        //                    this.cc_View.Content = new IdentificationUC();
        //                    break;
        //                case UserControlView.Authentication:
        //                    this.cc_View.Content = new AuthenticationUC();
        //                    break;
        //                case UserControlView.Products:
        //                    this.cc_View.Content = new ListProductsUC();
        //                    break;
        //                case UserControlView.Pay:
        //                    this.cc_View.Content = new PaymentUC();
        //                    break;
        //                case UserControlView.PaySuccess:
        //                    this.cc_View.Content = new SuccessUC();
        //                    break;
        //                case UserControlView.Withdrawal:
        //                    this.cc_View.Content = new WithdrawalUC();
        //                    break;
        //                case UserControlView.CancelPay:
        //                    this.cc_View.Content = new CancelUC();
        //                    break;
        //            }
        //        });
        //        GC.Collect();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        //    }
        //}
        private void GoTimer()
        {
            // Implementación del método GoTimer
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Limpieza al descargar el control
            //_timer?.StopTimer();
        }
    }


}
