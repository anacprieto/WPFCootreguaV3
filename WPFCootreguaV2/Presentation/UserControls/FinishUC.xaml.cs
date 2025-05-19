using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.Peripherals.Printer;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Modals;

namespace WPFCootreguaV2.UserControls
{
    public partial class FinishUC : AppUserControl
    {
        private const string STR_TIMER = "00:45";
        
        private Transaction _ts;
        private TimerGeneric _timer;
        private DocumentFormat _document = new();


        public FinishUC()
        {
            InitializeComponent();

            
            _ts = Transaction.Instance;

            GoTimer();
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PrintVoucher();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopTimer();
        }


        #region UI control methods
        private void BtnRating(object sender, EventArgs e)
        {
            var index = sender as Image;
            if (index == null) return;
            foreach (Image start in StartContainer.Children)
            {
                //aca vamos a comparar si el indice de la estrella dentro del contenedor padre es menor al indice de la estrella selecionada 
                if (int.Parse(start.Tag.ToString()!) <= int.Parse(index.Tag.ToString()!))
                {
                    start.Source = (ImageSource)App.Current.FindResource("ICO_STARS");
                    continue;
                }
                start.Source = (ImageSource)App.Current.FindResource("ICO_STAR");
            }

            _ts.Calificacion = index.Tag.ToString()!;
            BtnContinuar.Visibility = Visibility.Visible;
            
        }

        private void FinishBtn(object sender, EventArgs e)
        {
            FinishTransaction();
        }
        #endregion

        #region Internal Operation process
        private async Task PrintVoucher()
        {
            try
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
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Transaccion:", $"{_ts.IdTransaccionApi}", 40)), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Fecha:", $"{DateTime.Now.ToString("yyyy/MM/dd")}", 40)), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Hora:", $"{DateTime.Now.ToString("HH:mm:ss")}", 40)), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Estado:", $"{_ts.EstadoTransaccionVerb}", 40)), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Referencia:", $"{_ts.Referencia}", 40)), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Pago sin Redondear:", $"{_ts.TotalSinRedondear.ToString("C0")}", 40)), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Pago redondeado:", $"{_ts.Total.ToString("C0")}", 40)), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Valor Ingresado:", $"{_ts.TotalIngresado.ToString("C0")}", 40)), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(" "), 0));
                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintString(PrintCommands.ConfigureStringToSend(PrintService.ConfigurePairValues("Valor Devuelto:", $"{_ts.TotalDevuelta.ToString("C0")}", 40)), 0));

                //PrinterServicesCommands.RegisterInstruction(() => PrintCommands.PrintMarkcutpaper(0));
                //PrinterServicesCommands.ExecuteBuilder();

                #endregion

                var header = new Dictionary<string, string?>
                {
                    {"Recaudo",_ts.TipoRecaudo},
                };


                var body = new Dictionary<string, string?>
                {
                    {"Transacción:",_ts.IdTransaccionApi.ToString()},
                    {"Medio Pago:","Tarjeta"},
                    {"Fecha:", DateTime.Now.ToString("yyyy/MM/dd")},
                    {"Hora:", DateTime.Now.ToString("HH:mm:ss")},
                    {"Estado:", _ts.EstadoTransaccionVerb },
                    {"Referencia:", _ts.Referencia },
                    {"Pago Total:", _ts.TotalSinRedondear.ToString("C0") },
                    {"Valor Pagado:", _ts.TotalIngresado.ToString("C0") },
                };


                var footer = new Dictionary<string, string?>
                {
               //     {"Dirección","Carrera 11 No. 18 - 132"},
                //    {"Línea de Atención", "(+57) 4 8582024"},

                };

                _document.header = header;
                _document.body = body;
                _document.footer = footer;

                PrintService.BuildPrint(header, body, footer);
                await PrintService.Start();


            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }   

        private async void FinishTransaction()
        {
            StopTimer();

            if (string.IsNullOrEmpty(_ts.Calificacion))
            {
                _ts.Calificacion = "Sin calificación";
            }
            //TODO: Endpoint para calificación de transacción

            if (!_ts.DevueltaCorrecta)
            {
                var loadModal = _nav.ShowModal(
                    "No se pudo entregar la totalidad del dinero hay un faltante de:" +
                    $" {_ts.DatosPago.RemainingAmount.ToString("C0")} " +
                    ". Por favor comunícate con un administrador.");
                await Task.Delay(TimeSpan.FromMinutes(1));
                if (loadModal != null)
                {
                    loadModal.Close();
                    loadModal = null;
                }
            }

            Dispatcher.Invoke(() => GoTo(new ConfigUC()));
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

                    Dispatcher.Invoke(() => FinishTransaction());

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

    public class DocumentFormat
    {
        public Dictionary<string, string> header;
        public Dictionary<string, string> body;
        public Dictionary<string, string> footer;
    }

}
