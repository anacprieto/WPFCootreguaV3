using ControlzEx.Standard;
using DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.ApiService.IntegrationModels;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Integrations;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.UIServices.Integrations;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.Presentation.UserControls;
using WPFCootreguaV2.ApiService.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static WPFCootreguaV2.ApiService.Models.ApiResponse<T>;
using System.Windows.Media.Imaging;

namespace WPFCootreguaV2.UserControls
{
    /// <summary>
    /// Lógica de interacción para ScanInputUC.xaml
    /// </summary>
    public partial class ListProductsUC : AppUserControl
    {
        private const string STR_TIMER = "02:30";
        private TimerGeneric _timer;
        private Transaction _ts;
        private ModalWindow? _currentLoadModal = null;
        private ListProductsViewModel _listProductsViewModel;

        #region Regex properies
        private string _regexReferencia = @"8020(0*[1-9]\d*)\u001d3900";
        private string _regexValorPago = @"\u001d3900(0*[1-9]\d*)\u001d96";
        private string _regexFechaVencimiento = @"\u001d96(\d*)";

        private string _referencia = string.Empty;
        private string _valorPagar = string.Empty;
        private string _fechaPago = string.Empty;
        private string _NoConvenio = string.Empty;
        #endregion

        public ListProductsUC()
        {
            InitializeComponent();
            _ts = Transaction.Instance;

            //_viewModel = new ListProductsViewModel();
            //this.DataContext = _viewModel;
            this.Unloaded += OnUnloaded;
            this.Loaded += Onloaded;
            GoTimer();
            InitViewModel();

        }
        private void InitView()
        {
            try
            {
                if (_ts.TipoTransaccion == TypeTransaction.Retiro)
                {
                    btnPagar.Source = new BitmapImage(new Uri("/Images/Buttons/retirar.png", UriKind.Relative));
                }

                foreach (var product in transaction.DataProducts.OrderByDescending(f => f.ProxDate))
                {
                    decimal total = product.ValorAPagar == 0 ? product.Cuota : product.ValorAPagar;
                    string color = string.Empty;

                    if (product.TipoProducto == (int)ETypeProductCootregua.AhorrosVista)
                    {
                        total = 100;
                    }

                    if (product.ProxDate <= DateTime.Now.AddDays(1))
                    {
                        color = "Red";
                    }
                    else
                    if (product.ProxDate <= DateTime.Now.AddDays(30) && product.ProxDate >= DateTime.Now.AddDays(1))
                    {
                        color = "Yellow";
                    }
                    else
                    {
                        color = "Green";
                    }

                    if (Utilities.TransactionType == ETransactionType.Withdrawal)
                    {
                        if (product.RetirarProducto == 1)
                        {
                            lstPager.Add(new ProductsState
                            {
                                CodSession = product.CodSession,
                                CreationDate = product.CreationDate,
                                Identititfy = product.Identititfy,
                                NameLine = product.NameLine.ToString().ToLower(),
                                NameProduct = product.NameProduct,
                                NumberProduct = product.NumberProduct,
                                PaymentMethod = product.PaymentMethod,
                                ProxDate = product.ProxDate,
                                Saldo = product.Saldo,
                                TipoProducto = product.TipoProducto,
                                img = GetImage(false),
                                Cuota = Utilities.RoundValue(product.Cuota, true),
                                ValorPagar = Utilities.RoundValue(total, true),
                                ColorState = color,
                                RetirarProducto = product.RetirarProducto
                            });
                        }
                    }
                    else
                    {
                        lstPager.Add(new ProductsState
                        {
                            CodSession = product.CodSession,
                            CreationDate = product.CreationDate,
                            Identititfy = product.Identititfy,
                            NameLine = product.NameLine.ToString().ToLower(),
                            NameProduct = product.NameProduct,
                            NumberProduct = product.NumberProduct,
                            PaymentMethod = product.PaymentMethod,
                            ProxDate = product.ProxDate,
                            Saldo = product.Saldo,
                            TipoProducto = product.TipoProducto,
                            img = GetImage(false),
                            Cuota = Utilities.RoundValue(product.Cuota, true),
                            ValorPagar = Utilities.RoundValue(total, true),
                            ColorState = color,
                            RetirarProducto = product.RetirarProducto
                        });
                    }
                }

                if (lstPager.Count > 0)
                {
                    view.Source = lstPager;
                    lv_Products.DataContext = view;
                }
                //else
                //{
                //    string ms = string.Format("Estimado {0}, {1} No se encontrarón productos para este tipo de trámite.", transaction.DataPerson.FirstName, Environment.NewLine);
                //    Switcher.ModalMS(ms);
                //    Switcher.CLose();
                //}
            }
            catch (Exception ex)
            {
                Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }
        private void InitViewModel()
        {
            _listProductsViewModel = new ListProductsViewModel
            {
                CodSession = 0,
                CreationDate = 0,
                Identititfy = 0,
                NameLine =0,
                NameProduct ="",
                NumberProduct =0,
                PaymentMethod = 0,
                ProxDate =0,
                Saldo =_ts.Total,
                TipoProducto =0,
                img ="",
                Cuota =0,
                ValorPagar =_ts.Total,
                ColorState = 0,
                RetirarProducto =0
                /* PayAmount = _ts.Total,
                 RemainingAmount = _ts.Total,
                 ReturnAmount = 0,
                 EnteredAmount = 0,
                 Denominations = new List<Denomination>(),
                 DispensedAmount = 0*/
            };
            DataView.DataContext = _listProductsViewModel;
            this.DataContext = _listProductsViewModel;
            //DataView.ItemsSource = _listProductsViewModel.Denominations;

        }
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CloseLoadModal();
            StopTimer();
            ScannerController.Stop();
            ScannerController.ScannerDataReceived -= OnScannerDataReceived;
        }
        private void Onloaded(object sender, RoutedEventArgs e)
        {
            ScannerController.ScannerDataReceived += OnScannerDataReceived;
            ScannerController.Start();
            //       _viewModel.HelpMessage = "Ingresa número de cuenta o referente";
        }

        #region UI EVENTS

        private void ListViewItem_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            try {
                //var service = (Products)(sender as ListViewItem).Content;

                //if (service.ValorPagar > 0)
                //{
                //    ProductsSelected.img = GetImage(false);

                //    service.img = GetImage(true);

                //    lv_Products.Items.Refresh();

                //    ProductsSelected = service;

                //    transaction.ProductSelect = service;

                //    transaction.Amount = Utilities.RoundValue(service.ValorPagar, true);

                //    Dispatcher.BeginInvoke((Action)delegate
                //    {
                //        this.Opacity = 0.3;
                //        Switcher.Timer(false);

                //        if (service.TipoProducto == (int)ETypeProductCootregua.AhorrosVista)
                //        {
                //            MaxAmountAhorroVista = Utilities.TransactionType == ETransactionType.Withdrawal ? (service.Saldo - 100) : MaxAmountAhorroVista;

                //            ModalAmountWindow modal = new ModalAmountWindow(MaxAmountAhorroVista);
                //            modal.ShowDialog();
                //            transaction.Amount = modal.ValueToPay;
                //        }
                //        else
                //        {
                //            ModalAmountWindow modal = new ModalAmountWindow(transaction.Amount);
                //            modal.ShowDialog();
                //            transaction.Amount = modal.ValueToPay;
                //        }

                //        this.Opacity = 1;
                //        Switcher.Timer(true);

                //        if (transaction.Amount == 0)
                //        {
                //            transaction.Amount = Utilities.RoundValue(service.ValorPagar, true);
                //        }
                //        else
                //        {
                //            SaveTransaction();
                //        }
                //    });
                //    GC.Collect();
                //}
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }

        //private async void OnKeyboardPressed(object? sender, string keyPressed)
        //{
        //    if (string.IsNullOrEmpty(keyPressed)) return;
        //    if (keyPressed == "Remove")
        //    {
        //        string text = InputInvoice.Text;
        //        InputInvoice.Text = (text.Length > 1) ? text.Remove(text.Length - 1) : "";
        //        if (text.Length > 1)
        //        {
        //            InputInvoice.Text = text.Remove(text.Length - 1);
        //            return;
        //        }
        //        InputInvoice.Text = "";
        //        return;
        //    }

        //    if (keyPressed == "Clear")
        //    {
        //        InputInvoice.Text = "";
        //        return;
        //    }

        //    InputInvoice.Text += keyPressed;
        //    await Task.Delay(100);
        //}

        //private void TxtInvoice_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    _viewModel.StatusMsg = "";
        //    if (InputInvoice.Text.Length > 26)
        //    {
        //        InputInvoice.Text = InputInvoice.Text.Substring(0, InputInvoice.Text.Length - 1);
        //        return;
        //    }
        //}
        private void BtnAtras_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new SelectOptionUC()));

        }

        private void BtnSalir_MouseDown(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => GoTo(new MainUC()));

        //}
        //private async void BtnConsultar_Touch(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    TxtStatusMsg.Visibility = Visibility.Visible;
        //    string doc = InputInvoice.Text;
        //    await RequestDocData(doc);
        }
        #endregion

        #region ScannerEvents
        private async void OnScannerDataReceived(string data)
        {
            DisableView();
            try
            {

                string scannerRead = data.Replace("\u001d", "").Replace("\r", "");

                EventLogger.SaveLog(EventType.Info, "Data readed from scanner", scannerRead);

                _NoConvenio = scannerRead.Substring(3, 13);
                _referencia = scannerRead.Substring(20, 24);
                _valorPagar = scannerRead.Substring(48, 12);
                _fechaPago = scannerRead.Substring(62, 8);

                RequestConsultData request = new RequestConsultData();

                request.idComercio = AppConfig.Get("idComercioAlcaldia");
                request.password = AppConfig.Get("passwordAlcaldia");
                request.idCliente = _referencia;
                request.claveConsulta = "01";

                await _ts.ProcedureManager.GetDataPay();
            }
            catch (Exception ex)
            {
                _nav.ShowModal($"Hubo un error inesperado al realizar la consulta intenta nuevamente", new InfoModal());
                await Application.Current.Dispatcher.InvokeAsync(() => GoTo(new MainUC()));
            }
            EnableView();

        }
        #endregion



        private void CloseLoadModal()
        {
            if (_currentLoadModal != null)
            {
                _currentLoadModal.Close();
                _currentLoadModal = null;
            }
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

                    //         Dispatcher.Invoke(() => GoTo(new SelectInputUC()));


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


        private void BtnLimpiar_Touch(object sender, EventArgs e)
        {
            Image key = (Image)sender;
            string tag = key.Tag.ToString() ?? "";

            if (tag == "Clear")
            {
                // InputInvoice.Text = "";
                return;
            }

        }
    }


    public class ListProductsViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        #region Attributes

        private int _CodSession;

        private int _CreationDate;

        private int _Identititfy;

        private int _NameLine;

        private string _NameProduct;

        private int _NumberProduct;

        private int _PaymentMethod;

        private int _ProxDate;

        private decimal _Saldo;

        private int _TipoProducto;

        private string _img;

        private decimal _Cuota;

        private decimal _ValorPagar;


        private int _ColorState;

        private int _RetirarProducto;

        public int CodSession
        {
            get { return _CodSession; }
            set
            {
                if (_CodSession != value)
                {
                    _CodSession = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CodSession)));
                }
            }
        }

        public int CreationDate
        {
            get { return _CreationDate; }
            set
            {
                if (_CreationDate != value)
                {
                    _CreationDate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreationDate)));
                }
            }
        }

        public int Identititfy
        {
            get { return _Identititfy; }
            set
            {
                if (_Identititfy != value)
                {
                    _Identititfy = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Identititfy)));
                }
            }
        }

        public int NameLine
        {
            get { return _NameLine; }
            set
            {
                if (_NameLine != value)
                {
                    _NameLine = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameLine)));
                }
            }
        }

        public string NameProduct
        {
            get { return _NameProduct; }
            set
            {
                if (_NameProduct != value)
                {
                    _NameProduct = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameProduct)));
                }
            }
        }
        public int NumberProduct
        {
            get { return _NumberProduct; }
            set
            {
                if (_NumberProduct != value)
                {
                    _NumberProduct = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumberProduct)));
                }
            }
        }

        public int PaymentMethod
        {
            get { return _PaymentMethod; }
            set
            {
                if (_PaymentMethod != value)
                {
                    _PaymentMethod = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PaymentMethod)));
                }
            }
        }

        public int ProxDate
        {
            get { return _ProxDate; }
            set
            {
                if (_ProxDate != value)
                {
                    _ProxDate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProxDate)));
                }
            }
        }

        public decimal Saldo
        {
            get { return _Saldo; }
            set
            {
                if (_Saldo != value)
                {
                    _Saldo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Saldo)));
                }
            }
        }
        


        public int TipoProducto
        {
            get { return _TipoProducto; }
            set
            {
                if (_TipoProducto != value)
                {
                    _TipoProducto = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TipoProducto)));
                }
            }
        }
        public string img
        {
            get { return _img; }
            set
            {
                if (_img != value)
                {
                    _img = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(img)));
                }
            }
        }


        public decimal Cuota
        {
            get { return _Cuota; }
            set
            {
                if (_Cuota != value)
                {
                    _Cuota = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cuota)));
                }
            }
        }
        public decimal ValorPagar
        {
            get { return _ValorPagar; }
            set
            {
                if (_ValorPagar != value)
                {
                    _ValorPagar = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValorPagar)));
                }
            }
        }

        


        public int ColorState
        {
            get { return _ColorState; }
            set
            {
                if (_ColorState != value)
                {
                    _ColorState = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorState)));
                }
            }
        }

        public int RetirarProducto
        {
            get { return _RetirarProducto; }
            set
            {
                if (_RetirarProducto != value)
                {
                    _RetirarProducto = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RetirarProducto)));
                }
            }
        }



        
        //public decimal RemainingAmount
        //{
        //    get { return _remainingAmount; }
        //    set
        //    {
        //        if (_remainingAmount != value)
        //        {
        //            _remainingAmount = value;
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingAmount)));
        //        }
        //    }
        //}

        //public decimal ReturnAmount
        //{
        //    get { return _returnAmount; }
        //    set
        //    {
        //        if (_returnAmount != value)
        //        {
        //            _returnAmount = value;
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReturnAmount)));
        //        }
        //    }
        //}

        //public decimal DispensedAmount
        //{
        //    get { return _dispensedAmount; }
        //    set
        //    {
        //        if (_dispensedAmount != value)
        //        {
        //            _dispensedAmount = value;
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DispensedAmount)));
        //        }
        //    }
        //}

        //public bool IsPayCompleted
        //{
        //    get { return _isReturnSuccess; }
        //    set
        //    {
        //        if (_isReturnSuccess != value)
        //        {
        //            _isReturnSuccess = value;
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPayCompleted)));
        //        }
        //    }
        //}

        //#endregion

        //#region Methods
        //public void RefreshAmountsList(int denomination, int quantity)
        //{

        //    var itemDenomination = Denominations.Where(d => d.DenominationValue == denomination).FirstOrDefault();
        //    if (itemDenomination == null)
        //    {
        //        Denominations.Add(new Denomination
        //        {
        //            DenominationValue = denomination,
        //            Quantity = quantity,
        //            TotalDenomAmount = denomination * quantity,
        //        });
        //        return;
        //    }

        //    itemDenomination.Quantity += quantity;
        //    itemDenomination.TotalDenomAmount = denomination * itemDenomination.Quantity;
        //}
      #endregion
    }
}