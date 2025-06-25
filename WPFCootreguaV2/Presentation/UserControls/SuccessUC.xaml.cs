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
using WPFCootreguaV2.Domain.Peripherals.Printer;
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

        private const string STR_TIMER = "02:30";
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
            _ts = Transaction.Instance;
            Utilities.Speak("Gracias por utilizar nuestros servicios, esperamos verte nuevamente pronto. No olvides retirar tu recibo.");
            FinishTransaction();

        }

        private void FinishTransaction()
        {
            try
            {
                if (_ts.EstadoTransaccion == StateTransaction.Aprobada)
                {
                    EventLogger.SaveLog(EventType.Info, "Referencia: " + _ts.Referencia + " Descripcion: " + _ts.Documento+ "Estado:" + _ts.EstadoTransaccion);

                }
                else
                {
                    EventLogger.SaveLog(EventType.Info, "Referencia: " + _ts.Referencia + " Descripcion: " + _ts.Documento + "Estado:"  + _ts.EstadoTransaccion);

                }

                GC.Collect();

                Task.Run(() =>
                {
                    if (!string.IsNullOrEmpty(_ts.Descripcion))
                    {
                        EventLogger.SaveLog(EventType.Info, "Referencia: " + _ts.Referencia + " Descripcion: " + _ts.Documento + "Estado:" + _ts.EstadoTransaccion);
                        //AdminPayPlus.SaveErrorControl(transaction.Observation, "", EError.Device, ELevelError.Medium);
                    }
                    Api.UpdateTransaction();

                    _ts.StatePay = "Aprobada";
                    PrintService.CleanPrintQueue();
                    PrintVoucher();
                    Thread.Sleep(6000);

                    Dispatcher.BeginInvoke((Action)delegate
                    {
                        if (_ts.EstadoTransaccion == StateTransaction.CanceladaErrorDevuelta)
                        {
                            RestartApp();
                        }
                        else
                        {
                            if (!_ts.DevueltaCorrecta)
                            {
                                _nav.CloseLoadModal();
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
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Info, MethodBase.GetCurrentMethod().Name, this.GetType().Name,  ex.ToString());
            }
        }

        //private void QuestionUser()
        //{
        //    try
        //    {
        //        string ms = string.Format("Estimado {0},{1} ¿Desea realizar otra transacción?", _ts.DataPerson.FirstName, Environment.NewLine);
        //        bool state = _nav.ShowModal(ms, new ConfirmationModal());
        //        if (state)
        //        {
        //            _nav.ShowLoadModal(string.Format("Estimado {0}, consultando productos...", _ts.DataPerson.FirstName));
        //            Task.Run(async () =>
        //            {
        //                GetProducts();
        //            });
        //        }
        //        else
        //        {
        //            _nav.CloseLoadModal();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _nav.CloseLoadModal();
        //        EventLogger.SaveLog(EventType.Info, MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex.ToString());
        //        //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        //    }
        //}
        private void QuestionUser()
        {
            try
            {
                // Validaciones para identificar el NullReferenceException
                if (_nav == null)
                {
                    EventLogger.SaveLog(EventType.Error, "Navigator (_nav) es null en QuestionUser");
                    return;
                }

                if (_ts == null)
                {
                    EventLogger.SaveLog(EventType.Error, "Transaction (_ts) es null en QuestionUser");
                    _nav.ShowModal("Error: No hay información de transacción disponible.", new InfoModal());
                    return;
                }

                if (_ts.DataPerson == null)
                {
                    EventLogger.SaveLog(EventType.Error, "DataPerson es null en QuestionUser");
                    _nav.ShowModal("Error: No hay información de persona disponible.", new InfoModal());
                    return;
                }

                string firstName = _ts.DataPerson.FirstName ?? "Cliente";
                string ms = string.Format("Estimado {0},{1} ¿Desea realizar otra transacción?", firstName, Environment.NewLine);

                bool state = _nav.ShowModal(ms, new ConfirmationModal());
                if (state)
                {
                    _nav.ShowLoadModal(string.Format("Estimado {0}, consultando productos...", firstName));
                    Task.Run(async () =>
                    {
                        GetProducts();
                    });
                }
                else
                {
                    _nav.CloseLoadModal();
                }
            }
            catch (Exception ex)
            {
                _nav?.CloseLoadModal();
                EventLogger.SaveLog(EventType.Error, MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex.ToString());
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

                _nav.CloseLoadModal();
                if (!string.IsNullOrEmpty(prodct))
                {
                    var data = JsonConvert.DeserializeObject<List<ProductsState>>(prodct);

                    if (data.Count >= 1)
                    {
                        _ts.DataProducts = data;

                        Dispatcher.BeginInvoke((Action)delegate
                        {
                            Dispatcher.Invoke(() => GoTo(new ListProductsUC()));

                        });
                        GC.Collect();
                    }
                    else
                    {
                        _nav.ShowModal(string.Format("Estimado {0}, no se encontrarón productos en el servicio.", _ts.DataPerson.FirstName),new InfoModal());
                        _nav.CloseModal();
                    }
                }
                else
                {
                    _nav.ShowModal(string.Format("Estimado {0}, no se encontrarón productos en el servicio.", _ts.DataPerson.FirstName), new InfoModal());
                    _nav.CloseModal();
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Info, MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex.ToString());
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
