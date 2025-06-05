using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Data;
using WPFCootreguaV2.ApiService;

//using WPFCootreguaV2.ApiService.IntegrationsModels;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.ApiService;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Exceptions;

//using WPFCootreguaV2.Domain.Exceptions;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.Models;
using WPFCootreguaV2.Presentation.UserControls;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WPFCootreguaV2.UserControls
{

    public partial class AwaitWithdrawalUC : AppUserControl
    {
        private ArduinoController _peripherals;
        private PaymentViewModel _paymentViewModel;
        private ModalWindow? _loadModal = null;
        private Domain.UIServices.Transaction _ts;
        private StateTransaction _tranStateTemp = StateTransaction.Iniciada;
        public AwaitWithdrawalUC()
        {
            InitializeComponent();
            _ts = Domain.UIServices.Transaction.Instance;

            _ts.DevueltaCorrecta = false;

            _peripherals = ArduinoController.Instance;
            _peripherals.CashDispensed += OnCashDispensed;
            _peripherals.DispenserReject += OnDispenserReject;

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;

        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _paymentViewModel = new PaymentViewModel
            {
                PayAmount = _ts.TotalSinRedondear,
                RemainingAmount = 0,
                ReturnAmount = _ts.TotalSinRedondear,
                EnteredAmount = 0,
                Denominations = new List<Denomination>(),
                DispensedAmount = 0
            };


            bool isOkToReturn = await IsWithdrawalValid();

            if (!isOkToReturn)
            {
                Dispatcher.Invoke(() => GoTo(new ConfigUC()));
                return;
            }
            ReturnMoney(_paymentViewModel.ReturnAmount);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _peripherals.CashDispensed -= OnCashDispensed;
            _peripherals.DispenserReject -= OnDispenserReject;
        }

        #region Events
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
                _loadModal = _nav.ShowLoadModal("No se pudo entregar la totalidad del dinero hay un faltante de:" + $" {strValueToReturn} " + "Este dinero dinero permanecerá en su cuenta.");
                Thread.Sleep(5000); // Timer para mostrar la modal y que se pueda leer
                _ts.DevueltaCorrecta = false;
                await SaveWithdrawal();
            }
        }

        private void OnDispenserReject(Dictionary<int, int> rejectData)
        {
            // Se registra el reject en la api
            SendRejectDetails(rejectData);

        }
        #endregion

        private async Task<bool> IsWithdrawalValid()
        {
            try
            {
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

                // Reemplazar Thread.Sleep con await Task.Delay para operaciones asíncronas
                await Task.Delay(500);

                if (!string.IsNullOrEmpty(authen) && authen != "[]")
                {
                    var data = JsonConvert.DeserializeObject<PymentProduct>(authen);
                    if (data.codOpe >= 1)
                    {
                        _ts.PayCode = data.codOpe;
                        _ts.statePaySuccess = true;
                        _ts.EstadoTransaccion = StateTransaction.Aprobada;
                        ReturnMoney(_ts.Total);
                        return true;
                    }
                    else
                    {
                        Finish(false);
                        return false;
                    }
                }
                else
                {
                    Finish(false);
                    return false;
                }
            }
            catch ( ShowableException  ex)
    {
                EventLogger.SaveLog(EventType.Warning, $"No se pudo notificar al servicio {ex.Message}", ex);
                _nav.ShowModal(ex.Message, new InfoModal());
                CancelBeforeReturn(ex.Message);
                return false;
            }
    catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Respuesta error en petición del servicio. No se pudo notificar al servicio {ex.Message}", ex);
                _nav.ShowModal("Se presentó un problema, por favor inténtalo nuevamente más tarde.", new InfoModal());
                CancelBeforeReturn("Error desconocido en tiempo de ejecución");
                return false;
            }
        }
        //private async Task<bool> IsWithdrawalValid()
        //{

        //    try
        //    {
        //        Task.Run(async () =>
        //        {
        //            PymentProduct pay = new PymentProduct
        //            {
        //                Identification = _ts.Documento,
        //                Description = "Retiro",
        //                PayDate = DateTime.Now,
        //                ValueToPay = (long)_ts.Total,
        //                NumberProduct = long.Parse(_ts.ProductSelect.NumberProduct),
        //                TypeProduct = _ts.ProductSelect.TipoProducto,
        //                Coduser = _ts.Codigo,
        //                codOpe = 0,
        //                TipoMovimiento = 1
        //            };

        //            var authen = await ApiIntegration.CallApiCootregua("ControllerCootreguaRetirePayments", pay);

        //            Thread.Sleep(500);

        //            if (!string.IsNullOrEmpty(authen) && authen != "[]")
        //            {
        //                var data = JsonConvert.DeserializeObject<PymentProduct>(authen);

        //                if (data.codOpe >= 1)
        //                {
        //                    _ts.PayCode = data.codOpe;
        //                    _ts.statePaySuccess = true;
        //                    _ts.EstadoTransaccion = StateTransaction.Aprobada;

        //                    ReturnMoney(_ts.Total);
        //                }
        //                else
        //                {
        //                    Finish(false);
        //                }
        //            }
        //            else
        //            {
        //                Finish(false);
        //            }
        //        });
        //        //wd = await ApiIntegration.GetWithdrawalR(reqData);
        //    }
        //    catch (ShowableException ex)
        //    {
        //        EventLogger.SaveLog(EventType.Warning, $"Respuesta insatisfactoria del servicio GetWithdrawal {ex.Message}",ex);
        //        _nav.ShowModal(ex.Message, new InfoModal());
        //        CancelBeforeReturn(ex.Message);
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        EventLogger.SaveLog(EventType.Error, $"Respuesta error en petición del servicio GetWithdrawal {ex.Message}", ex);
        //        _nav.ShowModal("Se presentó un problema, por favor inténtalo nuevamente más tarde.", new InfoModal());
        //        CancelBeforeReturn("Error desconocido en tiempo de ejecución");
        //        return false;
        //    }


        //    return true;
        //}
        private void Finish(bool state)
        {
            try
            {
                if (!this._paymentViewModel.IsPayCompleted)
                {
                    this._paymentViewModel.IsPayCompleted = true;

                    //Switcher.ModalLoad(false);

                    if (state)
                    {
                        Dispatcher.Invoke(() => GoTo(new SuccessUC()));

                    }
                    else
                    {
                        _nav.ShowModal(string.Format("Estimado {0}, no se pudo notificar el retiro. Por favor vuelve a intentarlo.", _ts.DataPerson.FirstName));

                        _ts.EstadoTransaccion = StateTransaction.Cancelada;

                        Api.UpdateTransaction();

                        _nav.CloseModal();
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Respuesta error en petición Finish(bool state) {ex.Message}", ex); 
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }

        private void ReturnMoney(decimal returnValue)
        {
            _ts.DevueltaCorrecta = false;
            _peripherals.StartDispenser(returnValue);

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

        private void SendTransactionDetail(TypeOperation op, decimal denom,int quantity)
        {
            try
            {
                EventLogger.SaveLog(EventType.Info, $"Enviando detalle a la api: Op: {op}, Denom: {denom.ToString("C0")}, Cantidad: {quantity}");
                Api.CreateTransactionDetail(_ts.IdTransaccionApi, op, (int)denom, quantity);


            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }
        private void CloseLoadModal()
        {
            if (_loadModal != null)
            {
                Dispatcher.Invoke(() =>
                {
                    _loadModal.Close();
                    _loadModal = null;
                });
            }
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
                await Notify();

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


                CloseLoadModal();

                //Si el usuario escoje mostrar en pantalla se devuelve true
                bool isInboxScreen = Dispatcher.Invoke(() => _nav.ShowModalOf(new InboxOptionModal()));
                _ts.FacturaEnPantalla = isInboxScreen;
                if (isInboxScreen)
                {
                    var data = _ts.ProcedureManager.GetInboxData(_ts);
                    Dispatcher.Invoke(() => _nav.ShowModalOf(new InboxViewModal(data)));
                }

                Dispatcher.Invoke(() => GoTo(new FinishUC()));
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
                CloseLoadModal();
                _loadModal = _nav.ShowLoadModal("Se presentó un problema intentando reportar los datos del retiro. Por favor comuníquese con soporte técnico.");
            }
        }

        private async Task Notify()
        {
            var maxTries = 3;
            var nTries = 0;
            while (nTries < maxTries)
            {
                nTries++;
                EventLogger.SaveLog(EventType.Info, $"Intento {nTries} notificar retiro");
                var reqData = new CFA_ReqWithdrawalNotification
                {
                    Document = _ts.DocumentoCliente ?? string.Empty,
                    TypeDocument = _ts.TipoDocumento ?? string.Empty,
                    MontoTransaccion = ((int)_ts.TotalSinRedondear).ToString(),
                    AcountTransaccion = _ts.CuentaSeleccionada,
                    AmountDelivered = _paymentViewModel.DispensedAmount.ToString(),
                };

                bool notificationSuccess = await ApiIntegration.NotifyWithdrawalR(reqData);

                if (notificationSuccess)
                {
                    _tranStateTemp = StateTransaction.Aprobada;
                    return;
                }
                
            }
            _tranStateTemp = StateTransaction.AprobadaSinNotificar;

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
