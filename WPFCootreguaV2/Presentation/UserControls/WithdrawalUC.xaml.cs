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
        private Transaction transaction;
        private PaymentViewModel _paymentViewModel;
        public WithdrawalUC()
        {
            InitializeComponent();
            bg = new MenuBackground();
            _ts = Transaction.Instance;
            // Cambiar el fondo
            ChangeBackground(EBackground.Paga);
            // Inicializar el ViewModel AQUÍ
            OrganizeValues();

            

#if NO_PERIPHERALS
            Button dynamicButton = new Button();

            // Set properties of the button
            dynamicButton.Content = "Add minor value";
            dynamicButton.Width = 100;
            dynamicButton.Height = 50;
            dynamicButton.VerticalAlignment = VerticalAlignment.Top;
            dynamicButton.HorizontalAlignment = HorizontalAlignment.Left;
            // Set background color
            dynamicButton.Background = new SolidColorBrush(Colors.Red); // Change to the desired color
            dynamicButton.Foreground = new SolidColorBrush(Colors.White); // Change to the desired color

            // Set border brush and thickness
            dynamicButton.BorderBrush = new SolidColorBrush(Colors.White); // Change to the desired color
            dynamicButton.BorderThickness = new Thickness(2); // Change thickness as needed
            dynamicButton.Click += ExecuteScanner;

            Button dynamicButton2 = new Button();

            // Set properties of the button
            dynamicButton2.Content = "Add mid value";
            dynamicButton2.Width = 100;
            dynamicButton2.Height = 50;
            dynamicButton2.VerticalAlignment = VerticalAlignment.Top;
            dynamicButton2.HorizontalAlignment = HorizontalAlignment.Center;
            // Set background color
            dynamicButton2.Background = new SolidColorBrush(Colors.Transparent); // Change to the desired color
            dynamicButton2.Foreground = new SolidColorBrush(Colors.White); // Change to the desired color

            // Set border brush and thickness
            dynamicButton2.BorderBrush = new SolidColorBrush(Colors.White); // Change to the desired color
            dynamicButton2.BorderThickness = new Thickness(2); // Change thickness as needed
            dynamicButton2.Click += ExecuteScanner2;

            void ExecuteScanner(object sender, EventArgs e)
            {

                //OnCashIn(20000);
                //NotifyPay();
            }

            void ExecuteScanner2(object sender, EventArgs e)
            {
                //OnCashIn(50000);
            }
            //MainGrid.Children.Add(dynamicButton);

#else
    _peripherals = ArduinoController.Instance;
    _peripherals.CashIn += OnCashIn;
    _peripherals.CashDispensed += OnCashDispensed;
    _peripherals.DispenserReject += OnDispenserReject;
    _peripherals.PeripheralError += OnPeripheralError;
#endif

            // Agregar eventos
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            InitViewModel();
#if NO_PERIPHERALS
#else
            _peripherals.StartAcceptance(_paymentViewModel.PayAmount);
#endif
        }
        private void InitViewModel()
        {

            _paymentViewModel = new PaymentViewModel
            {
                PayAmount = _ts.Total,
                RemainingAmount = _ts.Total,
                ReturnAmount = 0,
                EnteredAmount = 0,
                Denominations = new List<Denomination>(),
                DispensedAmount = 0
            };
            this.DataContext = _paymentViewModel;

        }


        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
#if NO_PERIPHERALS
#else
            _peripherals.CashIn -= OnCashIn;
            _peripherals.CashDispensed -= OnCashDispensed;
            _peripherals.DispenserReject -= OnDispenserReject;
            _peripherals.PeripheralError -= OnPeripheralError;
#endif
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
        private void OrganizeValues()
        {
            try
            {
                _paymentViewModel = new PaymentViewModel
                {
                    PayAmount = _ts.Total,
                    RemainingAmount = _ts.Total,
                    ReturnAmount = 0,
                    EnteredAmount = 0,
                    Denominations = new List<Denomination>(),
                    DispensedAmount = 0
                };
                this.DataContext = _paymentViewModel;

                this.DataContext = _ts;

                SaveWithdrawal();
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }

        private async void SaveWithdrawal()
        {
            try
            {
                Task.Run(async () =>
                {
                    PymentProduct pay = new PymentProduct
                    {
                        Identification = _ts.Documento,
                        Description = "Retiro",
                        PayDate = DateTime.Now,
                        ValueToPay = (long)_ts.Total,
                        NumberProduct = long.Parse(transaction.ProductSelect.NumberProduct),
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

                        if (data.codOpe >= 1)
                        {
                            _ts.PayCode = data.codOpe;
                            _ts.statePaySuccess = true;
                            _ts.EstadoTransaccion = StateTransaction.Aprobada;

                            ReturnMoney(_ts.Total);
                        }
                        else
                        {
                            //Finish(false);
                        }
                    }
                    else
                    {
                        //Finish(false);
                    }
                });

               // Switcher.ModalLoad(true);
            }
            catch (Exception ex)
            {
                //    Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
                //Finish(false);
            }
        }
        private void ReturnMoney(decimal returnValue)
        {
            _ts.DevueltaCorrecta = false;
#if NO_PERIPHERALS
            OnCashDispensed(returnValue, new Dictionary<int, int>());
#else
            _peripherals.StartDispenser(returnValue);
#endif

        }
        private async void OnCashDispensed(decimal totalDispensed, Dictionary<int, int> details)
        {

            //_paymentViewModel.DispensedAmount = totalDispensed;

            //_paymentViewModel.RemainingAmount = _paymentViewModel.ReturnAmount - _paymentViewModel.DispensedAmount;
            //string strValueToReturn = _paymentViewModel.RemainingAmount.ToString("C0");

            //SendDispenseDetails(details);

            //CloseLoadModal();

            //if (_paymentViewModel.DispensedAmount == _paymentViewModel.ReturnAmount)
            //{
            //    _ts.DevueltaCorrecta = true;
            //    await SavePay();
            //}
            //else
            //{
            //    _currentLoadModal = _nav.ShowModal("No se pudo entregar la totalidad del dinero hay un faltante de:" + $" {strValueToReturn} " + ".Por favor comunícate con un administrador.");
            //    await Task.Delay(5000); // Timer para mostrar la modal y que se pueda leer
            //    _ts.DevueltaCorrecta = false;
            //    await SavePay();
            //}

        }

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
