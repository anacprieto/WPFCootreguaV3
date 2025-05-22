using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using WPFCootreguaV2.Domain.ApiService;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.UIServices.Integrations;
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
        private MenuBackground bg;
        //private ManualInputViewModel _viewModel;
        //private ModalWindow? _currentLoadModal = null;

        public SuccessUC()
        {
            InitializeComponent();
            //this.Unloaded += OnUnloaded;
            _ts = Transaction.Instance;
            bg = new MenuBackground();
            ChangeBackground(EBackground.Generico);
            Utilities.Speak("Gracias por utilizar nuestros servicios, esperamos verte nuevamente pronto. No olvides retirar tu recibo.");

            FinishTransaction();
            //GoTimer();

        }
        private async void FinishTransaction()
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

                var loadModal = _nav.ShowModal(mensaje);

                await Task.Delay(TimeSpan.FromSeconds(20));

                if (loadModal != null)
                {
                    loadModal.Close();
                    loadModal = null;
                }
            }

            QuestionUser();
            Dispatcher.Invoke(() => GoTo(new ConfigUC()));
        }
        private void QuestionUser()
        {
            try
            {

                string ms = string.Format("Estimado {0},{1} ¿Desea realizar otra transacción?", _ts.DataPerson.FirstName, Environment.NewLine);
                bool state = _nav.ShowModal(ms, new ConfirmationModal());


                if (state)
                {
                    Task.Run(async () =>
                    {
                        GetProducts();
                    });

                    _nav.ShowModal("realizar otra transacción");
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
        }
        public void ChangeBackground(EBackground eBackground)
        {

            try
            {
                //if (bg == null)
                //{
                //    bg = new MenuBackground(); // Tipo correcto
                //}

                Dispatcher.Invoke(() =>
                {
                    switch (eBackground)
                    {
                        case EBackground.Identificate:
                            // Usar el recurso estático
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/identificate.jpg";
                            break;
                        case EBackground.Identificate2:
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/identificate2.jpg";
                            break;
                        case EBackground.Autenticate:
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/autenticate.jpg";
                            break;
                        case EBackground.Autenticate2:
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/autenticate2.jpg";
                            break;
                        case EBackground.Productos:
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/elige.jpg";
                            break;
                        case EBackground.Paga:
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/paga.jpg";
                            break;
                        case EBackground.Generico:
                            bg.Background = "C:/Users/Ana Prieto/Desktop/PROYECTOS 2025/WPFCootreguaV2/WPFCootreguaV2/bin/Debug/net6.0-windows/Images/Backgrounds/generic.jpg";
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


    }
}
