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
using Newtonsoft.Json;
using System.Threading;
using WPFCootreguaV2.Domain.ApiService;

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
        private MenuBackground bg;
        //private ManualInputViewModel _viewModel;
        //private ModalWindow? _currentLoadModal = null;

        public WithdrawalUC()
        {
            InitializeComponent();



        } 
        
        //private void OrganizeValues()
        //{
        //    try
        //    {
        //        this.paymentViewModel = new PaymentViewModel
        //        {
        //            PayValue = transaction.Amount,
        //            ValorFaltante = 0,
        //            ImgContinue = Visibility.Hidden,
        //            ImgCancel = Visibility.Hidden,
        //            ImgCambio = Visibility.Hidden,
        //            ValorSobrante = 0,
        //            ValorIngresado = 0,
        //            viewList = new CollectionViewSource(),
        //            Denominations = new List<DenominationMoney>(),
        //            ValorDispensado = 0,
        //            StatePay = false
        //        };

        //        transaction.Payment = this.paymentViewModel;

        //        this.DataContext = transaction;

        //        SaveWithdrawal();
        //    }
        //    catch (Exception ex)
        //    {
        //        Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        //    }
        //}

        //private void ReturnMoney()
        //{
        //    try
        //    {
        //        //TODO:pruebas
        //        //transaction.Payment.ValorDispensado = 100;
        //        //transaction.StateReturnMoney = false;
        //        //Finish(true);

        //        Task.Run(() =>
        //        {
        //            AdminPayPlus.ControlPeripherals.callbackTotalOut = totalOut =>
        //            {
        //                transaction.StateReturnMoney = true;

        //                transaction.Payment.ValorDispensado = totalOut;

        //                Finish(true);
        //            };

        //            AdminPayPlus.ControlPeripherals.callbackError = error =>
        //            {
        //                AdminPayPlus.SaveLog(new RequestLogDevice
        //                {
        //                    Code = error.Item1,
        //                    Date = DateTime.Now,
        //                    Description = error.Item2,
        //                    Level = ELevelError.Medium,
        //                    TransactionId = transaction.IdTransactionAPi
        //                }, ELogType.Device);
        //            };

        //            AdminPayPlus.ControlPeripherals.callbackOut = valueOut =>
        //            {
        //                AdminPayPlus.ControlPeripherals.callbackOut = null;

        //                transaction.Payment.ValorDispensado = valueOut;

        //                transaction.StateReturnMoney = false;

        //                if (transaction.Payment.ValorDispensado != transaction.Amount)
        //                {
        //                    transaction.Observation += MessageResource.IncompleteMony + " Devolvio: " + valueOut.ToString();
        //                }
        //                else
        //                {
        //                    transaction.StateReturnMoney = true;
        //                }

        //                Finish(true);
        //            };

        //            AdminPayPlus.ControlPeripherals.callbackLog = log =>
        //            {
        //                AdminPayPlus.SaveDetailsTransaction(transaction.IdTransactionAPi, 0, 0, 0, string.Empty, log);
        //            };

        //            AdminPayPlus.ControlPeripherals.StartDispenser(transaction.Amount);
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        //    }
        //}

        //private async void SaveWithdrawal()
        //{
        //    try
        //    {
        //        Task.Run(async () =>
        //        {
        //            PymentProduct pay = new PymentProduct
        //            {
        //                Identification = transaction.Document,
        //                Description = "Retiro",
        //                PayDate = DateTime.Now,
        //                ValueToPay = (long)transaction.Amount,
        //                NumberProduct = long.Parse(transaction.ProductSelect.NumberProduct),
        //                TypeProduct = transaction.ProductSelect.TipoProducto,
        //                Coduser = transaction.Codigo,
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
        //                    this.transaction.PayCode = data.codOpe;
        //                    this.transaction.statePaySuccess = true;
        //                    transaction.State = ETransactionState.Success;

        //                    ReturnMoney();
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

        //        Switcher.ModalLoad(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        //        Finish(false);
        //    }
        //}

        //private void Finish(bool state)
        //{
        //    try
        //    {
        //        if (!this.paymentViewModel.StatePay)
        //        {
        //            this.paymentViewModel.StatePay = true;

        //            Switcher.ModalLoad(false);

        //            if (state)
        //            {
        //                AdminPayPlus.ControlPeripherals.ClearValues();

        //                if (!transaction.StateReturnMoney)
        //                {
        //                    Switcher.ModalMS(string.Format("Estimado {0}, no se pudo entregar la totalidad del dinero. {1} Falto por devolver {2} {3} Se abonara lo faltante a la cuenta.", transaction.DataPerson.FirstName, Environment.NewLine, String.Format("{0:C0}", (transaction.Amount - transaction.Payment.ValorDispensado)), Environment.NewLine));
        //                    RenotifyTransaction();
        //                }
        //                else
        //                {
        //                    Switcher.Navigate(UserControlView.PaySuccess, transaction);
        //                }
        //            }
        //            else
        //            {
        //                Switcher.ModalMS(string.Format("Estimado {0}, no se pudo notificar el retiro. Por favor vuelve a intentarlo.", transaction.DataPerson.FirstName));

        //                transaction.State = ETransactionState.Cancel;

        //                AdminPayPlus.UpdateTransaction(transaction);

        //                Switcher.CLose();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        //    }
        //}

        //private void RenotifyTransaction()
        //{
        //    try
        //    {
        //        Task.Run(async () =>
        //        {
        //            PymentProduct pay = new PymentProduct
        //            {
        //                Identification = transaction.Document,
        //                Description = "Renotificación-Retiro",
        //                PayDate = DateTime.Now,
        //                ValueToPay = (long)(transaction.Amount - transaction.Payment.ValorDispensado),
        //                NumberProduct = long.Parse(transaction.ProductSelect.NumberProduct),
        //                TypeProduct = transaction.ProductSelect.TipoProducto,
        //                Coduser = transaction.Codigo,
        //                codOpe = 0,
        //                TipoMovimiento = 2
        //            };

        //            var authen = await ApiIntegration.CallApiCootregua("ControllerCootreguaRetirePayments", pay);

        //            Thread.Sleep(500);

        //            Switcher.ModalLoad(false);

        //            if (!string.IsNullOrEmpty(authen) && authen != "[]")
        //            {
        //                var data = JsonConvert.DeserializeObject<PymentProduct>(authen);

        //                if (data.codOpe >= 1)
        //                {
        //                    this.transaction.PayCode = data.codOpe;
        //                    this.transaction.statePaySuccess = true;
        //                    transaction.State = ETransactionState.Success;

        //                    Switcher.Navigate(UserControlView.PaySuccess, transaction);
        //                }
        //                else
        //                {
        //                    AdminPayPlus.SaveWithdrawalExpin(pay);
        //                    Switcher.Navigate(UserControlView.PaySuccess, transaction);
        //                }
        //            }
        //            else
        //            {
        //                AdminPayPlus.SaveWithdrawalExpin(pay);
        //                Switcher.Navigate(UserControlView.PaySuccess, transaction);
        //            }
        //        });

        //        Switcher.ModalLoad(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
        //    }
        //}
        //#endregion

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
    }
}
