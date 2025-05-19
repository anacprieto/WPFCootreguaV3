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
using System.Windows.Markup;
using Microsoft.AspNet.SignalR.Client;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Modals;
using System.Diagnostics;
using System.Threading;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.Domain.Peripherals.Datafono;

namespace WPFCootreguaV2.Presentation.UserControls
{
    /// <summary>
    /// Lógica de interacción para CardPaymentUC.xaml
    /// </summary>
    /// 

    public partial class CardPaymentUC : AppUserControl
    {

        private const string STR_TIMER = "02:00";
        private TimerGeneric _timer;
        //    public TEFTransactionManager transactionManager;
        private ModalWindow? _currentLoadModal;
        private Transaction _ts;
        private StateTransaction _tranStateTemp = StateTransaction.Iniciada;
        private RequestDatafonoInfo data;
        private bool isPaySuccess = false;
        private int _intentos = 0;
        private int _IntentosTimer = 0;
        const int maxIntentos = 3;
        const int maxIntentosTimer = 1;
        private bool _isSubscribed = false;

        private IHubProxy hubProxy;
        //      static TEFTransactionManager transactionManager;
        private bool ConexionApi;

        public CardPaymentUC()
        {
            InitializeComponent();

            _ts = Transaction.Instance;

            data = new RequestDatafonoInfo();

            data.Inicial = "01";
            data.Value = _ts.Total.ToString();
            data.PaypadID = AppConfig.Get("paypadId");
            data.IdTransaccion = _ts.IdTransaccionApi.ToString();

#if !NO_DATAFONO

            Button dynamicButton = new Button();

            // Set properties of the button
            dynamicButton.Content = "Simulate Datafono";
            dynamicButton.Width = 100;
            dynamicButton.Height = 50;
            dynamicButton.VerticalAlignment = VerticalAlignment.Top;
            dynamicButton.HorizontalAlignment = HorizontalAlignment.Left;
            // Set background color
            dynamicButton.Background = new SolidColorBrush(Colors.Transparent); // Change to the desired color
            dynamicButton.Foreground = new SolidColorBrush(Colors.White); // Change to the desired color

            // Set border brush and thickness
            dynamicButton.BorderBrush = new SolidColorBrush(Colors.White); // Change to the desired color
            dynamicButton.BorderThickness = new Thickness(2); // Change thickness as needed
            dynamicButton.Click += ExecuteScanner;

            void ExecuteScanner(object sender, EventArgs e)
            {
                ManejarRespuesta("00");
            }

            MainGrid.Children.Add(dynamicButton);

#else
#endif

            this.Unloaded += OnUnloaded;
            this.Loaded += OnLoaded;


            GoTimer();
        }

        #region UI EVENTS

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CloseLoadModal();
            StopTimer();

        }


        public async void OnLoaded(object sender, EventArgs events)
        {

#if !NO_DATAFONO
#else
            await Task.Run(async () => {
               await Start();
            });
#endif


        }

        private void BtnAtras_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new DataPayUC()));
        }

        private void BtnSalir_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new MenuUC()));
        }

        #endregion


        #region ImplementacionDatafono

        public async Task Start()
        {
            var respuesta = await iniciarConexionApi();

            if (respuesta == true)
            {

                string datosIniciales = "";

            
                datosIniciales = $"{data.Inicial},{data.Value},0,{data.PaypadID},{data.IdTransaccion},0,0,{data.PaypadID},";
        

                string rcObtenido = CalcularLRC(datosIniciales);
                //   Console.WriteLine("LRC: " + rcObtenido); // Debería imprimir 56.

                // Concatenar LRC con los datos iniciales
                string dataConLRC = $"{datosIniciales}{rcObtenido}";
                // Console.WriteLine("Datos con LRC: " + dataConLRC);

                // Enviar la petición
                EnviarPeticion(dataConLRC);
            }
            else
            {

                //DescriptionStatusPayPlus = MessageResource.ComunicationServerFail;
                //callbackResult?.Invoke(false);


                //  rutaconsola con su .exe


                var FileName = AppConfig.Get("ConexionDatafono");
                var respouesta = EjecutarAplicacionConsola(FileName);

                if (respouesta == true)
                {

                    var respuesta2 = await iniciarConexionApi();

                    if (respuesta2 == true)
                    {
                        //         transactionManager = new TEFTransactionManager();

                         string datosIniciales = $"{data.Inicial},{data.Value},0,{data.PaypadID},{data.IdTransaccion},0,0,{data.PaypadID},";
                      

                        string rcObtenido = CalcularLRC(datosIniciales);
                        //   Console.WriteLine("LRC: " + rcObtenido); // Debería imprimir 56.

                        // Concatenar LRC con los datos iniciales
                        string dataConLRC = $"{datosIniciales}{rcObtenido}";
                        // Console.WriteLine("Datos con LRC: " + dataConLRC);

                        // Enviar la petición
                        EnviarPeticion(dataConLRC);
                    }
                    else
                    {
                        Console.WriteLine("No fue posible la conexion");

                    }
                }
                else
                {
                    Console.WriteLine("No fue posible la conexion");

                }
            }
        }


        public async Task<bool> iniciarConexionApi()
        {
            try
            {
                // AdminPayPlus.SaveLog("AdminPayPlus", "iniciarConexionApi(), entrando a la ejecucion iniciarConexionApi ", "OK", "", null);
                var respuesta = await SignalRManager.Instance.Initialize();

                if (respuesta == true)
                {
                    // AdminPayPlus.SaveLog("AdminPayPlus", "iniciarConexionApi(), entrando a la ejecucion respuesta True ", "OK", "", null);
                    var signalRManager = SignalRManager.Instance;
                    hubProxy = signalRManager.HubProxy;

                    ConexionApi = true;

                    return ConexionApi;
                }
                else
                {
                    // AdminPayPlus.SaveLog("AdminPayPlus", "iniciarConexionApi(), entrando a la ejecucion respuesta false ", "OK", "", null);

                    ConexionApi = false;

                    return ConexionApi;
                }

            }
            catch (Exception ex)
            {
                // AdminPayPlus.SaveLog("AdminPayPlus", "iniciarConexionApi(), entrando a la ejecucion catch, respuesta false  ", "OK", ex.Message, null);
                ConexionApi = false;
                return ConexionApi;

            }


        }


        private bool EjecutarAplicacionConsola(string rutaConsolaApp)
        {
            // Intenta ejecutar la aplicación de consola
            Process procesoConsola = null;
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = rutaConsolaApp,
                    UseShellExecute = true
                };

                procesoConsola = Process.Start(startInfo);


            }
            catch (Exception ex)
            {

                // AdminPayPlus.SaveLog("Adminpayplus", "EjecutarAplicacionConsola(), entrando al metodo Catch", "OK", ex.Message, null);

                return false;
            }

            // Comprueba si la aplicación se está ejecutando
            if (procesoConsola != null && !procesoConsola.HasExited)
            {

                // AdminPayPlus.SaveLog("Adminpayplus", "EjecutarAplicacionConsola(), entrando al metodo", "OK", "La aplicación de consola se ha iniciado correctamente.", null);// Aquí puedes agregar más lógica si es necesario

                return true;
            }
            else
            {

                // AdminPayPlus.SaveLog("Adminpayplus", "EjecutarAplicacionConsola(), entrando al metodo", "OK", "La aplicación de consola no se pudo iniciar.", null);
                return false;
            }
        }


        public static string CalcularLRC(string pi_sCadena)
        {

            try
            {
                string vl_sLRC = "";

                if (pi_sCadena.Length > 0)
                {
                    int vl_iLRC = (int)pi_sCadena[0]; // Obtener el valor ASCII del primer carácter.

                    for (int i = 1; i < pi_sCadena.Length; i++)
                    {
                        vl_iLRC = vl_iLRC ^ (int)pi_sCadena[i]; // Realizar la operación XOR con el resto de los caracteres.
                    }

                    vl_sLRC = vl_iLRC.ToString("x"); // Convertir el resultado a hexadecimal.
                }

                if (vl_sLRC.Length == 1)
                {
                    vl_sLRC = "0" + vl_sLRC; // Asegurarse de que el LRC tenga dos dígitos.
                }

                return vl_sLRC.ToUpper(); // Convertir a mayúsculas para el formato hexadecimal.
            }
            catch (Exception ex)
            {

            }

            return null;

        }

        public async void EnviarPeticion(string Data)
        {
            try
            {
                hubProxy.On("RespuestaPeticionDatafono", (parametro) =>
                {

                    var respuesta = parametro;

                    EventLogger.SaveLog(EventType.Info, "RespuestaPeticionDatafono" + respuesta);


                    ManejarRespuesta(respuesta);
                });

                //  await hubProxy.Invoke("EnviarPeticionDatafono");

                await hubProxy.Invoke("EnviarPeticionDatafono", Data);
            }
            catch (Exception ex)
            {

            }
        }

        private async void ManejarRespuesta(string codigo)
        {
            try
            {

                switch (codigo)
                {
                    case "00":
                        StopTimer();
                        CloseLoadModal();
                        NotifyPay();
                        break;
                    case "02":
                        StopTimer();
                        CloseLoadModal();
                        _nav.ShowModal("Transacción rechazada y/o cancelada,por favor verificar nuevamente", new InfoModal());

                        isPaySuccess = false;
                        _tranStateTemp = StateTransaction.Cancelada;
                        await SavePay();



                        break;

                    case "03":



                        StopTimer();
                        CloseLoadModal();
                        _currentLoadModal = _nav.ShowModal("Transacción en verificación, un momento por favor mientras se realizan las validaciones pertinentes.");
                        GoTimer();
                        _intentos++;
                        _IntentosTimer++;

                        if (_IntentosTimer <= maxIntentosTimer)
                        {
                            Thread.Sleep(180000);
                        }

                        if (!_isSubscribed)
                        {
                            // Solo suscribir una vez
                            hubProxy.On("RespuestaUltimaTrx", (parametro) =>
                            {
                                var respuesta = parametro;
                                EventLogger.SaveLog(EventType.Info, "RespuestaPeticionDatafono RespuestaUltimaTrx " + respuesta);
                                ManejarRespuesta(respuesta);
                            });

                            _isSubscribed = true; // Marcar como suscrito
                        }

                        if (_intentos > maxIntentos)
                        {
                            isPaySuccess = false;
                            _tranStateTemp = StateTransaction.Cancelada;
                            CloseLoadModal();
                            await SavePay();
                        }
                        else
                        {

                            if (_IntentosTimer <= maxIntentosTimer)
                            {
                                await hubProxy.Invoke("ValidarRespUltimaTransaccion");
                            }
                            else
                            {
                                Thread.Sleep(30000);

                                await hubProxy.Invoke("ValidarRespUltimaTransaccion");
                            }

                            // Invocar la validación de la transacción

                            //await Task.Delay(20000);

                        }

                        break;

                    case "05":
                        StopTimer();
                        CloseLoadModal();
                        _nav.ShowModal("Transacción con errores de apertura de puerto serial, o timeout sin recibir la solicitud inicial del datáfono, por favor verificar nuevamente", new InfoModal());

                        isPaySuccess = false;
                        _tranStateTemp = StateTransaction.Cancelada;
                        await SavePay();


                        break;

                    case "06":
                        StopTimer();
                        CloseLoadModal();
                        _nav.ShowModal("Transacción rechazada por trama inicial incorrecta, por favor verificar nuevamente", new InfoModal());

                        isPaySuccess = false;
                        _tranStateTemp = StateTransaction.Cancelada;
                        await SavePay();

                        break;

                    case "99":
                        StopTimer();
                        CloseLoadModal();
                        _nav.ShowModal("Transacción rechazada por trama inicial incorrecta, por favor verificar nuevamente", new InfoModal());

                        isPaySuccess = false;
                        _tranStateTemp = StateTransaction.Cancelada;
                        await SavePay();

                        break;

                    default:
                        StopTimer();
                        CloseLoadModal();
                        _nav.ShowModal("Error desconocido, por favor verificar nuevamente", new InfoModal());

                        isPaySuccess = false;
                        _tranStateTemp = StateTransaction.Cancelada;
                        await SavePay();

                        break;
                }


            }
            catch (Exception ex)
            {

            }


        }


        public async Task NotifyPay()
        {
            try
            {
       
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {

                    EventLogger.SaveLog(EventType.Info, "Pago Completado en integración");
                    _tranStateTemp = StateTransaction.Aprobada;
                    isPaySuccess = true;


                    if (isPaySuccess)
                    {
                        await SavePay();
                        return;
                    }

                 
                });

            }
            catch (Exception ex)
            {

            }
        }


        private async Task SavePay()
        {
            try
            {

                SetTransactionDescription();

                if (isPaySuccess)
                {
                    SignalRManager.Instance.CerrarConexion();
                    _ts.TotalIngresado = _ts.Total;
                    _ts.EstadoTransaccion = _tranStateTemp;

                    _ts.DevueltaCorrecta = true;

                    Api.UpdateTransaction();


                    Dispatcher.Invoke(() => GoTo(new FinishUC()));
                }
                else
                {
                    SignalRManager.Instance.CerrarConexion();
                    _ts.TotalIngresado = 0;
                    _ts.EstadoTransaccion = _tranStateTemp;
                    Api.UpdateTransaction();

                    if (_ts.TipoTransaccion == TypeTransaction.Pago)
                    {
                        Dispatcher.Invoke(() => GoTo(new DataPayUC()));
                    }

                }


            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);

                EventLogger.SaveLog(EventType.Info, "Pago cancelado por error guardando el pago");
                await SavePay();



                _nav.ShowModal("Ocurrió un error fatal intentando reportar los datos del pago. Por favor comuníquese con soporte técnico.");
            }
        }


        private void SetTransactionDescription()
        {
            switch (_tranStateTemp)
            {
                case StateTransaction.Aprobada:
                    _ts.EstadoTransaccionVerb = "Exitoso";
                    _ts.Descripcion += "Transacción finalizada correctamente. ";
                    break;
                case StateTransaction.Cancelada:
                    _ts.EstadoTransaccionVerb = "Declinada";
                    _ts.Descripcion += "Transacción Cancelada, No se realizó el pago.";
                    break;
                case StateTransaction.AprobadaSinNotificar:
                    _ts.EstadoTransaccionVerb = "Exitoso";
                    _ts.Descripcion += "Transacción aprobada, pero no se ha podido notificar el pago a la entidad correspondiente. ";
                    break;
                case StateTransaction.ErrorServicioTercero:
                    _ts.EstadoTransaccionVerb = "Declinada";
                    _ts.Descripcion += $"Transacción cancelada ocurrió un error en el servicio tercero, No se realizó el pago.";
                    break;
                default:
                    break;
            }


        }

        private RespuestaDatafono ProcesarRespuesta(string respuesta)
        {
            var partes = respuesta.Split(',');

            // Crear objeto para almacenar los datos de la respuesta
            var resultado = new RespuestaDatafono
            {
                Codigo = partes.Length > 0 ? partes[0] : string.Empty,
                TransaccionID = partes.Length > 1 ? partes[1] : string.Empty,
                Parametros = partes
            };

            return resultado;
        }

        public class RespuestaDatafono
        {
            public string Codigo { get; set; }
            public string TransaccionID { get; set; }
            public string[] Parametros { get; set; }
        }



        #endregion


        #region Timer
        public void GoTimer()
        {
            try
            {
                _timer = new TimerGeneric(STR_TIMER);

                Dispatcher.Invoke(() =>
                {
                    TxtTimer.Text = STR_TIMER;
                });

                _timer.CallBackTimeOut = () =>
                {
                    CloseLoadModal();
                    Dispatcher.Invoke(() => GoTo(new MenuUC()));

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

        private void CloseLoadModal()
        {
            if (_currentLoadModal != null)
            {
                Dispatcher.Invoke(() =>
                {
                    _currentLoadModal.Close();
                    _currentLoadModal = null;
                });
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
