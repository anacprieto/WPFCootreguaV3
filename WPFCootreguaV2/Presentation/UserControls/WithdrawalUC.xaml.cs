using HantleDispenserAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.ApiService;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Exceptions;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.UIServices.Integrations;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.UserControls;
using ManualInputViewModel = WPFCootreguaV2.UserControls.ManualInputViewModel;


namespace WPFCootreguaV2.Presentation.UserControls
{
    /// <summary>
    /// Lógica de interacción para MenuUC.xaml
    /// </summary>
    public partial class WithdrawalUC : AppUserControl
    {
        private const string STR_TIMER = "02:30";
        private TimerGeneric _timer;
        private Transaction _ts;
        private PaymentViewModel _paymentViewModel;
        private DocumentFormat _document = new();
        private StateTransaction _tranStateTemp = StateTransaction.Iniciada;
        private ArduinoController _peripherals;
        public WithdrawalUC()
        {
            InitializeComponent();
            _ts = Transaction.Instance;

            _ts.DevueltaCorrecta = false;

            _peripherals = ArduinoController.Instance;
            _peripherals.CashDispensed += OnCashDispensed;
            _peripherals.DispenserReject += OnDispenserReject;
            // Agregar eventos
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;

        }
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
        }
        private void OnDispenserReject(Dictionary<int, int> rejectData)
        {
            // Se registra el reject en la api
            SendRejectDetails(rejectData);

        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _paymentViewModel = new PaymentViewModel
            {
                PayAmount = _ts.Total,
                RemainingAmount = 0,
                ReturnAmount = _ts.Total,
                EnteredAmount = 0,
                Denominations = new List<Denomination>(),
                DispensedAmount = 0
            };
            

            ReturnMoney(_paymentViewModel.ReturnAmount);

        }

        private void ReturnMoney(decimal returnValue)
        {
            _ts.DevueltaCorrecta = false;

#if NO_PERIPHERALS
#else
            _peripherals.StartDispenser(returnValue);
#endif

        }
        private async void OnCashDispensed(decimal totalDispensed, Dictionary<int, int> details)
        {

            _paymentViewModel.DispensedAmount = totalDispensed;

            _paymentViewModel.RemainingAmount = _paymentViewModel.ReturnAmount - _paymentViewModel.DispensedAmount;
            string strValueToReturn = _paymentViewModel.RemainingAmount.ToString("C0", new CultureInfo("en-US"));

            SendDispenseDetails(details);

            if (_paymentViewModel.DispensedAmount == _paymentViewModel.ReturnAmount)
            {
                _ts.DevueltaCorrecta = true;
                await SaveWithdrawal();
            }
            else
            {
                 _nav.ShowLoadModal("No se pudo entregar la totalidad del dinero hay un faltante de:" + $" {strValueToReturn} " + "Este dinero dinero permanecerá en su cuenta.");
                Thread.Sleep(5000); // Timer para mostrar la modal y que se pueda leer
                _ts.DevueltaCorrecta = false;
                await SaveWithdrawal();
            }
        }


        private void SendDispenseDetails(Dictionary<int, int> details)
        {

            foreach (var denom in details.Keys)
            {
                var quantity = details[denom];
                if (quantity <= 0) continue;
                SendTransactionDetail(TypeOperation.DP, Convert.ToDecimal(denom), quantity);

            }
        }

        private void SendRejectDetails(Dictionary<int, int> details)
        {

            foreach (var denom in details.Keys)
            {
                var quantity = details[denom];
                if (quantity <= 0) continue;
                SendTransactionDetail(TypeOperation.Reject, Convert.ToDecimal(denom), quantity);

            }
        }

        private void SendTransactionDetail(TypeOperation op, decimal denom, int quantity)
        {
            try
            {
                EventLogger.SaveLog(EventType.Info, $"Enviando detalle a la api: Op: {op}, Denom: {denom.ToString("C0")}, Cantidad: {quantity}");
                Api.CreateTransactionDetail1(op, (int)denom, quantity);

            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }
        #region "Eventos"
        private void txtValueReturn_TouchDown(object sender, TouchEventArgs e)
        {
            try
            {
                var txt = sender as TextBlock;

                if (txt.Text.Contains("*"))
                {
                    txt.Text = String.Format("{0:C0}", Convert.ToDecimal(txt.Tag));
                }
                else
                {
                    txt.Text = "********";
                }
            }
            catch (Exception ex)
            {
               // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }
        #endregion

        private async Task Notify()
        {
            var maxTries = 3;
            var nTries = 0;
            while (nTries < maxTries)
            {
                nTries++;
                EventLogger.SaveLog(EventType.Info, $"Intento {nTries} notificar retiro");
                PymentProduct pay = new PymentProduct
                {
                    Identification = _ts.Documento,
                    Description = "Retiro",
                    PayDate = DateTime.Now,
                    ValueToPay = (long)_ts.Total,
                    NumberProduct = long.Parse(_ts.ProductSelect.NumberProduct),
                    TypeProduct = _ts.ProductSelect.TipoProducto,
                    Coduser = _ts.Codigo,
                    codOpe = 0,
                    TipoMovimiento = 1
                };

                var authen = await ApiIntegration.CallApiCootregua("ControllerCootreguaRetirePayments", pay);
                Thread.Sleep(500);
                if (!string.IsNullOrEmpty(authen) && authen != "[]")
                {
                    var data = JsonConvert.DeserializeObject<PymentProduct>(authen);
                    _tranStateTemp = StateTransaction.Aprobada;

                    if (data.codOpe >= 1)
                    {
                        _ts.PayCode = data.codOpe;
                        _ts.statePaySuccess = true;
                        _ts.EstadoTransaccion =StateTransaction.Aprobada;
                        // ReturnMoney();
                    }
                }

            }
            _tranStateTemp = StateTransaction.AprobadaSinNotificar;

        }
        private void CancelBeforeReturn(string msj)
        {

            _ts.EstadoTransaccion = StateTransaction.Cancelada;
            _ts.Descripcion += $"Transacción Cancelada por respuesta del servicio \"{msj}\". No se realizó el retiro.";
            _ts.DatosPago = _paymentViewModel;
            _ts.TotalIngresado = _paymentViewModel.EnteredAmount;
            _ts.TotalDevuelta = _paymentViewModel.DispensedAmount;

            Api.UpdateTransaction();

        }
        private async Task SaveWithdrawal()
        {
            try
            {
#if NO_PERIPHERALS
                
#else
                await Notify();
#endif


                _paymentViewModel.IsPayCompleted = true;
                _ts.DatosPago = _paymentViewModel;
                _ts.TotalIngresado = _paymentViewModel.EnteredAmount;
                _ts.TotalDevuelta = _paymentViewModel.DispensedAmount;

                SetTransactionDescription();


                if ((_tranStateTemp == StateTransaction.Aprobada || _tranStateTemp == StateTransaction.Cancelada)
                    && !_ts.DevueltaCorrecta)
                {
                    // Si el estado de transacción es aprobada o cancelada y además hay error de devuelta se cambia a su respectivo estado
                    // CanceladoErrorDevuelta o AprobadaErrorDevuelta
                    _ts.EstadoTransaccion = (StateTransaction)((int)_tranStateTemp + 2);
                }
                else
                {
                    _ts.EstadoTransaccion = _tranStateTemp;
                }



                Api.UpdateTransaction();

                PrintService.CleanPrintQueue();
                PrintVoucher();

                Dispatcher.Invoke(() => GoTo(new SuccessUC()));
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
                _nav.CloseModal();
                _nav.ShowModal("Se presentó un problema intentando reportar los datos del retiro. Por favor comuníquese con soporte técnico.",new InfoModal());
            }
        }


        private void PrintVoucher()
        {
            try
            {
                var body = new Dictionary<string, string?>();
                var footer = new Dictionary<string, string?>();

                var header = new Dictionary<string, string?>
                {
                     {"NIT: ", "800 155 087-8"},
                     {"Transacción: ", _ts.TipoTransaccionVerb},
                     {"Código: ", _ts.Codigo.ToString()},
                     {"Fecha: ", DateTime.Now.ToString("yyyy/MM/dd")},
                     {"Hora: ", DateTime.Now.ToString("hh:mm:ss")},
                     {"Estado: ", _ts.EstadoTransaccionVerb}
                };

                if (_ts.TipoTransaccionVerb == "Pago")
                {
                    body = new Dictionary<string, string?>
                    {
                        {"Nro de Factura",_ts.IdTransaccionApi.ToString()},
                        {"Documento", _ts.Documento.ToString()},
                        {"Producto", _ts.ProductSelect.NameLine.ToString()},
                        {"Valor a Pagar", String.Format("{0:C0}",_ts.Total)},
                        {"Valor Ingresado",String.Format("{0:C0}",_ts.TotalIngresado)},
                        {"Valor Devuelto", String.Format("{0:C0}",_ts.TotalDevuelta)}
                    };
                }
                else
                {
                    body = new Dictionary<string, string?>
                    {
                        {"Nro de Factura",_ts.IdTransaccionApi.ToString()},
                        {"Documento", _ts.Documento.ToString()},
                        {"Producto", _ts.ProductSelect.NameLine.ToString()},
                        {"Valor a Pagar", String.Format("{0:C0}",_ts.Total)},
                        {"Valor a Retirar", String.Format("{0:C0}",_ts.Total)},
                        {"Valor retirado", String.Format("{0:C0}",_ts.DatosPago.DispensedAmount)}
                    };
                }
                if (_ts.TipoTransaccionVerb == "Pago" && !_ts.DevueltaCorrecta)
                {
                    footer = new Dictionary<string, string?>
                    {
                        {"Valor Devuelto", String.Format("{0:C0}",_ts.TotalDevuelta)}
                    };
                }
                else if (_ts.TipoTransaccionVerb == "Retiro" && !_ts.DevueltaCorrecta)
                {
                    footer = new Dictionary<string, string?>
                    {

                        {"Valor faltante", String.Format("{0:C0}",_ts.DatosPago.RemainingAmount)},
                        {"","Se abonara el dinero faltante"}
                    };
                }


                //var footer = new Dictionary<string, string?>
                //{
                //    {"Dirección", "Carrera 11 No. 18 - 132"},
                //    {"Línea de Atención", "(+57) 4 8582024"},
                //};

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
        private void SetTransactionDescription()
        {
            switch (_tranStateTemp)
            {
                case StateTransaction.Aprobada:
                    _ts.EstadoTransaccionVerb = "Exitoso";
                    _ts.Descripcion += "Transacción finalizada correctamente. ";
                    break;
                case StateTransaction.AprobadaSinNotificar:
                    _ts.EstadoTransaccionVerb = "Exitoso";
                    _ts.Descripcion += "Transacción aprobada, pero no se ha podido notificar el retiro.";
                    break;

                default:
                    break;
            }

            if (!_ts.DevueltaCorrecta)
                _ts.Descripcion += $"Ocurrió un error durante la devolución del dinero. Cantidad faltante {_paymentViewModel.RemainingAmount.ToString("C0")}";
        }

    }
}
