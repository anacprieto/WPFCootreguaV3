using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.ApiService;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.UIServices.Integrations;
using WPFCootreguaV2.Domain.Variables;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.Models;
using WPFCootreguaV2.UserControls;
using ManualInputViewModel = WPFCootreguaV2.UserControls.ManualInputViewModel;


namespace WPFCootreguaV2.Presentation.UserControls
{
    /// <summary>
    /// Lógica de interacción para MenuUC.xaml
    /// </summary>
    public partial class SuccessUC : AppUserControl
    {

        private const string STR_TIMER = "00:45";
        private TimerGeneric _timer;
        private Transaction _ts;
        private DocumentFormat _document = new();
        private readonly Navigator _nav;



        public SuccessUC()
        {
            //InitializeComponent();
            //_ts = Transaction.Instance;
            //Utilities.Speak("Gracias por utilizar nuestros servicios, esperamos verte nuevamente pronto. No olvides retirar tu recibo.");
            //FinishTransaction();
           
            InitializeComponent();
            Utilities.Speak("Gracias por utilizar nuestros servicios, esperamos verte nuevamente pronto. No olvides retirar tu recibo.");

            _ts = Transaction.Instance;
            _nav = Navigator.Instance;

            FinishTransaction();
            GoTimer();
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;

        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            //DisableView();
            //PrintService.CleanPrintQueue();
            //PrintVoucher();
            //_nav.ShowLoadModal("Imprimiendo factura...");

            //await Task.Delay(TimeSpan.FromSeconds(PrintService.NumberOfSecondsToPrint));
            //_timer.ControlTimer(pauseOrder: true);
            //while (!(PrintService.RecentImpressionSuccess ?? false))
            //{
            //    _nav.CloseLoadModal();
            //    _nav.CloseModal();
            //   // if (!HandlePrintingError()) break;
            //     _nav.ShowLoadModal("Imprimiendo factura...");
            //    await Task.Delay(TimeSpan.FromSeconds(PrintService.NumberOfSecondsToPrint));
            //}
            //_nav.CloseLoadModal();
            //_nav.CloseModal();

            //_timer.ControlTimer(pauseOrder: false);
            //EnableView();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
           // StopTimer();
            PrintService.RecentImpressionSuccess = false;
        }
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
        private async void FinishTransaction()
        {
            StopTimer();

            if (string.IsNullOrEmpty(_ts.Calificacion))
            {
                _ts.Calificacion = "Sin calificación";
            }
            if (!_ts.DevueltaCorrecta)
            {
                string mensaje;

                // Verificar que DatosPago y RemainingAmount existan
                if (_ts?.DatosPago?.RemainingAmount != null)
                {
                    mensaje = "No se pudo entregar la totalidad del dinero hay un faltante de:" +
                             $" {_ts.DatosPago.RemainingAmount.ToString("C0")} " +
                             ". Por favor comuníquese con un administrador.";
                }
                else
                {
                    // Mensaje alternativo cuando no hay datos específicos del faltante
                    mensaje = "No se pudo entregar la totalidad del dinero correctamente. " +
                             "Por favor comuníquese con un administrador.";
                }
            }
            // Api.UpdateTransaction();
            StopTimer();
            Dispatcher.Invoke(() => GoTo(new ConfigUC()));
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

                    Dispatcher.Invoke(() => GoTo(new ConfigUC()));

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
        //private bool IsRoundTotal()
        //{
        //    string firstName = _ts.DataPerson.FirstName ?? "Cliente";
        //    string ms = string.Format("Estimado {0},{1} ¿Desea realizar otra transacción?", firstName, Environment.NewLine);
        //    bool continuar = _nav.ShowModal(ms, new ConfirmationModal());
        //    return continuar;
        //}

        //private void QuestionUser()
        //{
        //    try
        //    {
        //        bool state = IsRoundTotal();

        //        if (state)
        //        {
        //            string firstName = _ts.DataPerson.FirstName ?? "Cliente";
        //            _nav.ShowLoadModal(string.Format("Estimado {0}, consultando productos...", firstName));
        //            GetProducts();
        //        }
        //        else
        //        {
        //            _nav.CloseLoadModal();
        //            Dispatcher.Invoke(() => GoTo(new ConfigUC()));

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _nav?.CloseLoadModal();
        //        EventLogger.SaveLog(EventType.Error, MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex.ToString());
        //    }
        //}

        private bool AskForAnotherTransaction(string firstName)
        {
            string ms = string.Format("Estimado {0},{1} ¿Desea realizar otra transacción?", firstName, Environment.NewLine);
            bool resultado=_nav.ShowModal(ms, new ConfirmationModal());
            return resultado;
        }

        private void QuestionUser()
        {
            try
            {
                if (_ts == null)
                {
                    EventLogger.SaveLog(EventType.Error, "Transaction (_ts) es null en QuestionUser");
                    _nav?.ShowModal("Error: No hay información de transacción disponible.", new InfoModal());
                    return;
                }
                if (_ts.DataPerson == null)
                {
                    EventLogger.SaveLog(EventType.Error, "DataPerson es null en QuestionUser");
                    _nav?.ShowModal("Error: No hay información de persona disponible.", new InfoModal());
                    return;
                }

                string firstName = _ts.DataPerson.FirstName ?? "Cliente";
                bool state = AskForAnotherTransaction(firstName);
                if (state)
                {
                    _nav?.ShowLoadModal(string.Format("Estimado {0}, consultando productos...", firstName));
                    GetProducts();
                }
                else
                {
                    _nav?.CloseLoadModal();
                    Dispatcher.Invoke(() => GoTo(new ConfigUC()));
                }
            }
            catch (Exception ex)
            {
                _nav?.CloseLoadModal();
                EventLogger.SaveLog(EventType.Error, nameof(QuestionUser), ex.ToString());
            }
        }
        //private async Task GetProducts()
        //{
        //    try
        //    {
        //        ProductsState products = new ProductsState
        //        {
        //            CodSession = _ts.Codigo,
        //            Identititfy = _ts.Documento,
        //        };

        //        var prodct = await ApiIntegration.CallApiCootregua("ControllerCootreguaGetStateProduct", products);

        //        //_nav.CloseLoadModal();
        //        if (!string.IsNullOrEmpty(prodct))
        //        {
        //            var data = JsonConvert.DeserializeObject<List<ProductsState>>(prodct);

        //            if (data.Count >= 1)
        //            {
        //                _ts.DataProducts = data;
        //                Dispatcher.Invoke(() => GoTo(new ListProductsUC()));
        //            }
        //            else
        //            {
        //                _nav.ShowModal(string.Format("Estimado {0}, no se encontrarón productos en el servicio.", _ts.DataPerson.FirstName),new InfoModal());
        //                _nav.CloseModal();
        //            }
        //        }
        //        else
        //        {
        //            _nav.ShowModal(string.Format("Estimado {0}, no se encontrarón productos en el servicio.", _ts.DataPerson.FirstName), new InfoModal());
        //            _nav.CloseModal();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        EventLogger.SaveLog(EventType.Info, MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex.ToString());
        //    }
        //}
        private async Task GetProducts()
        {
            try
            {
                _nav.CloseLoadModal();
                ProductsState products = new ProductsState
                {
                    CodSession = _ts.Codigo,
                    Identititfy = _ts.Documento,
                };

                //var prodct = await ApiIntegration.CallApiCootregua("ControllerCootreguaGetStateProduct", products);
                // _nav.ShowModal("Consultando productos para {0}...");


                //var desencrypted = EncryptorEcity.Decrypt(prodct);
                //var respuesta1120557056 = "UlmKdX4+uzXax9XsKwFwqkgxt6FMJoVaVcQpR4ambziDsTfenMMfOjwUzrGFBNEtauGKXdJKOMTd/bFOMHjxkw==";
                var product = "3WNkRgO/cTqlfKg08SmuYkwcjgbtYEJfdzqtooSCeGY1fpfGALyNKoifqqpamsGjSeE25UFeKUM8snwB7ODcBeJYoTdJp/V8NmPJKnJ+5AiGqCZpc3AgXun/Ahe52WX4qdo+O4LVFHp8LRSGjHzXJg2VLEu2uBwgidsHc8DGvlL5e9H+hF+yvnwPTp6BC64+xK6QAy2BawvJPtza+WsUah7BeNacMetafWtS/LjFgNhCddOTSxKZd7DjTQ/xPr5kkZ9BEq5iWNfmlg/PS90HswE1MPkZ5cTQCKIRd7AFU28awiWrYpYmOov7vGA5jBypYvXCpBzbUhhMNvrOpukNBaOxRzwuKA3c5OMkr0Fitzw=";
                //var desencrypted1120 = EncryptorEcity.Decrypt(respuesta1120557056);
                //_nav.ShowModal(string.Format("Estimado {0}, La huella capturada no coincide con la registrada, por favor intentalo de nuevo.", new InfoModal()));
                var desncrypted = EncryptorEcity.Decrypt(product);
                //Switcher.ModalLoad(false);

                if (!string.IsNullOrEmpty(desncrypted))
                {
                    var settings = new JsonSerializerSettings
                    {
                        DateFormatString = "dd/MM/yyyy",
                        DateParseHandling = DateParseHandling.DateTime
                    };

                    var data = JsonConvert.DeserializeObject<List<ProductsState>>(desncrypted, settings);
                    // var data = JsonConvert.DeserializeObject<List<ProductsState>>(desncrypted);

                    if (data.Count >= 1)
                    {

                        _ts.DataProducts = data;

                        //Dispatcher.Invoke(() => GoTo(new ListProductsUC()));
                        Dispatcher.Invoke(() => GoTo(new ListProductsUC()));
                        GC.Collect();
                        //
                    }
                    else
                    {
                        _nav.ShowModal(string.Format("Estimado {0}, no se encontraron productos en el servicio.", _ts.DataPerson.FirstName), new InfoModal());

                        _nav.CloseModal();
                    }
                }
                else
                {
                    _nav.ShowModal(string.Format("Estimado {0}, no se encontraron productos en el servicio.", _ts.DataPerson.FirstName), new InfoModal());
                    _nav.CloseModal();
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Info, MethodBase.GetCurrentMethod().Name);

            }
        }

        //private async Task FinishTransaction1()
        //{
        //    if (string.IsNullOrEmpty(_ts.Calificacion))
        //    {
        //        _ts.Calificacion = "Sin calificación";
        //    }

        //    // Si no hay devolución correcta, mostrar mensaje
        //    if (!_ts.DevueltaCorrecta)
        //    {
        //        string mensaje;

        //        // Verificar que DatosPago y RemainingAmount existan
        //        if (_ts?.DatosPago?.RemainingAmount != null)
        //        {
        //            mensaje = "No se pudo entregar la totalidad del dinero hay un faltante de:" +
        //                     $" {_ts.DatosPago.RemainingAmount.ToString("C0")} " +
        //                     ". Por favor comuníquese con un administrador.";
        //        }
        //        else
        //        {
        //            // Mensaje alternativo cuando no hay datos específicos del faltante
        //            mensaje = "No se pudo entregar la totalidad del dinero correctamente. " +
        //                     "Por favor comuníquese con un administrador.";
        //        }

        //        // Mostrar modal en el hilo de la UI
        //        await Application.Current.Dispatcher.InvokeAsync(() =>
        //        {
        //            _nav.ShowLoadModal(mensaje);
        //        });

        //        // Esperar 20 segundos
        //        await Task.Delay(TimeSpan.FromSeconds(20));

        //        // Cerrar modal en el hilo de la UI
        //        await Application.Current.Dispatcher.InvokeAsync(() =>
        //        {
        //            _nav.CloseLoadModal();
        //        });
        //    }
        //}
        private void PrintVoucher()
        {
            try
            {
                var header = new Dictionary<string, string?>
                {
                    {"Recaudo", _ts.TipoRecaudo},
                };

                var body = new Dictionary<string, string?>
                {
                    {"Transacción", _ts.ApiDto.Id.ToString()},
                    {"Fecha", DateTime.Now.ToString("yyyy/MM/dd")},
                    {"Hora", DateTime.Now.ToString("HH:mm:ss")},
                    {"Estado", _ts.EstadoTransaccionVerb},
                    {"Nro. Factura", _ts.paymentProcess.Referencia},
                    {"Documento", _ts.paymentProcess.Documento},
                    {"Pago sin redondear", _ts.paymentProcess.TotalSinRedondear.ToString("C0")},
                    {"Pago redondeado", _ts.paymentProcess.Total.ToString("C0")},
                    {"Valor Ingresado", _ts.paymentProcess.TotalIngresado.ToString("C0")},
                    {"Valor Devuelto", _ts.paymentProcess.TotalDevuelta.ToString("C0")}
                };

                var footer = new Dictionary<string, string?>
                {
                    {"Dirección", "Carrera 11 No. 18 - 132"},
                    {"Línea de Atención", "(+57) 4 8582024"},
                };

                _document.header = header;
                _document.body = body;
                _document.footer = footer;
                PrintService.BuildPrint(header, body, footer);
                PrintService.Start();
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }

        
        /*
        private void FinishTransaction()
        {
            try
            {
                FinishTransaction1();

                if (_ts.EstadoTransaccion == StateTransaction.Aprobada)
                {
                    EventLogger.SaveLog(EventType.Info, "Referencia: "+ _ts.Referencia+ "Descripcion" + _ts.Documento);

                }
                else
                {
                    EventLogger.SaveLog(EventType.Error, "Referencia: " + _ts.Referencia + "Descripcion" + _ts.EstadoTransaccionVerb);

                }

                GC.Collect();

                Task.Run(() =>
                {

                    Api.UpdateTransaction();

                    _ts.StatePay = "Aprobada";

                    PrintService.CleanPrintQueue();
                    PrintVoucher();

                    Thread.Sleep(6000);

                    Dispatcher.BeginInvoke((Action)delegate
                    {
                        if (_ts.EstadoTransaccion == StateTransaction.AprobadaErrorDevuelta)
                        {
                            RestartApp();
                        }
                        else
                        {
                            if (!_ts.DevueltaCorrecta)
                            {
                                _nav.CloseModal();
                            }
                            else
                            {
                                QuestionUser();
                            }
                        }
                    });
                    GC.Collect();
                });
                QuestionUser();

                Dispatcher.Invoke(() => GoTo(new ConfigUC()));

            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, "Error al FinishTransaction(), restablecer app");

               // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }

        private async void FinishTransaction1()
        {

            if (string.IsNullOrEmpty(_ts.Calificacion))
            {
                _ts.Calificacion = "Sin calificación";
            }

            //TODO: Endpoint para calificación de transacción
            if (!_ts.DevueltaCorrecta)
            {
                string mensaje;

                // Verificar que DatosPago y RemainingAmount existan
                if (_ts?.DatosPago?.RemainingAmount != null)
                {
                    mensaje = "No se pudo entregar la totalidad del dinero hay un faltante de:" +
                             $" {_ts.DatosPago.RemainingAmount.ToString("C0")} " +
                             ". Por favor comuníquese con un administrador.";
                }
                else
                {
                    // Mensaje alternativo cuando no hay datos específicos del faltante
                    mensaje = "No se pudo entregar la totalidad del dinero correctamente. " +
                             "Por favor comuníquese con un administrador.";
                }

                var loadModal = _nav.ShowLoadModal(mensaje);

                await Task.Delay(TimeSpan.FromSeconds(20));

                if (loadModal != null)
                {
                    loadModal.Close();
                    loadModal = null;
                }
            }


        }

        public static void RestartApp()
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    Process pc = new Process();
                    Process pn = new Process();
                    ProcessStartInfo si = new ProcessStartInfo();
                    //si.FileName = Path.Combine(Directory.GetCurrentDirectory(), "Archivos", AppConfig.Get("NAME_APLICATION"));
                    si.FileName = Directory.GetCurrentDirectory() + "\\" + AppConfig.Get("NAME_APLICATION");
                    pn.StartInfo = si;
                    pn.Start();
                    pc = Process.GetCurrentProcess();
                    pc.Kill();
                }));
                GC.Collect();
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, "Utilities", ex, ex.ToString());
            }
        }


        private async Task PrintVoucher()
        {
            try
            {


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
                         {"Dirección","Carrera 11 No. 18 - 132"},
                        {"Línea de Atención", "(+57) 4 8582024"},

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
        }*/
        /*

        private void QuestionUser()
        {
            try
            {

                string ms = string.Format("Estimado {0},{1} ¿Desea realizar otra transacción?", _ts.DataPerson.FirstName, Environment.NewLine);
                var modal = _nav.ShowLoadModal("Consultando datos de cuenta...");
                modal.Close();
                bool state = _nav.ShowModal(ms,new ConfirmationModal());


                if (state)
                {
                    Task.Run(async () =>
                    {
                        _nav.CloseModal();
                        GetProducts();
                    });

                   // _nav.ShowModal("realizar otra transacción");
                }
                else
                {
                    _nav.CloseModal();

                }
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }

        private async Task GetProducts()
        {
            try
            {
                ProductsState products = new ProductsState
                {
                    CodSession = _ts.Codigo,
                    Identititfy = _ts.Documento,
                };

                var prodct = await ApiIntegration.CallApiCootregua("ControllerCootreguaGetStateProduct", products);
                _nav.CloseModal();
                //Switcher.ModalLoad(false);

                if (!string.IsNullOrEmpty(prodct))
                {
                    var data = JsonConvert.DeserializeObject<List<ProductsState>>(prodct);

                    if (data.Count >= 1)
                    {
                        _ts.DataProducts = data;

                        Dispatcher.BeginInvoke((Action)delegate
                        {
                            Dispatcher.Invoke(() => GoTo(new ListProductsUC()));
                            //Switcher.Navigate(UserControlView.Products, transaction);
                        });
                        GC.Collect();
                    }
                    else
                    {
                        _nav.ShowModal(string.Format("Estimado {0}, no se encontrarón productos en el servicio.", _ts.DataPerson.FirstName));
                        _nav.CloseModal();
                    }
                }
                else
                {
                    _nav.ShowModal(string.Format("Estimado {0}, no se encontrarón productos en el servicio.", _ts.DataPerson.FirstName));
                    _nav.CloseModal();
                }
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }*/


        /*
        private async void QuestionUser()
        {
            try
            {
                string ms = string.Format("Estimado {0},{1} ¿Desea realizar otra transacción?",
                    _ts.DataPerson.FirstName, Environment.NewLine);

                bool state = _nav.ShowModal(ms, new ConfirmationModal());

                if (state)
                {
                    await GetProducts();
                }
                else
                {
                    _nav.CloseModal();
                }
            }
            catch (Exception ex)
            {
                _nav.CloseLoadModal();
                _nav.CloseModal();
                // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }

        private async Task GetProducts()
        {
            try
            {
                // Mostrar modal de carga
                _nav.ShowLoadModal("Consultando datos de cuenta...");

                ProductsState products = new ProductsState
                {
                    CodSession = _ts.Codigo,
                    Identititfy = _ts.Documento,
                };

                var prodct = await ApiIntegration.CallApiCootregua("ControllerCootreguaGetStateProduct", products);

                // Cerrar modal de carga
                _nav.CloseLoadModal();

                if (!string.IsNullOrEmpty(prodct))
                {
                    var data = JsonConvert.DeserializeObject<List<ProductsState>>(prodct);
                    if (data.Count >= 1)
                    {
                        _ts.DataProducts = data;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            GoTo(new ListProductsUC());
                        });

                        GC.Collect();
                    }
                    else
                    {
                        _nav.ShowModal(string.Format("Estimado {0}, no se encontraron productos en el servicio.",
                            _ts.DataPerson.FirstName), new InfoModal());
                    }
                }
                else
                {
                    _nav.ShowModal(string.Format("Estimado {0}, no se encontraron productos en el servicio.",
                        _ts.DataPerson.FirstName), new InfoModal());
                }
            }
            catch (Exception ex)
            {
                _nav.CloseLoadModal();
                _nav.ShowModal("Error al consultar productos. Intente nuevamente.", new InfoModal());
                // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }*/
        public class DocumentFormat
        {
            public Dictionary<string, string> header;
            public Dictionary<string, string> body;
            public Dictionary<string, string> footer;
        }

    }
}
