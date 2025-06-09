using ControlzEx.Standard;
using DB;
using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MPOST;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.ApiService.IntegrationModels;
using WPFCootreguaV2.ApiService.Models;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Integrations;
using WPFCootreguaV2.Domain.Peripherals;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.UIServices.Integrations;
using WPFCootreguaV2.Modals;
using WPFCootreguaV2.Models;
using WPFCootreguaV2.Presentation.UserControls;

namespace WPFCootreguaV2.UserControls
{
    /// <summary>
    /// Lógica de interacción para ScanInputUC.xaml
    /// </summary>
    public partial class ListProductsUC : AppUserControl
    {
        private DocumentFormat _document = new();

        private MenuBackground bg;

        private const string STR_TIMER = "02:30";
        private TimerGeneric _timer;
        private Transaction _ts;
        private ModalWindow? _currentLoadModal = null;
        private ModalWindow? _currentModal;
        private ListProductsViewModel _listProductsViewModel;
        private ObservableCollection<ProductsState> lstPager;
        private CollectionViewSource view;
        private decimal MaxAmountAhorroVista;

        #region Regex properies

        private string _referencia = string.Empty;

        public TypeTransaction TransactionType { get; private set; }
        #endregion
        private ProductsState ProductsSelected = null;

        private bool _isInitializing = true;
        public ListProductsUC()
        {
            InitializeComponent();
            try
            {
                _isInitializing = true; // Activar bandera

                _ts = Transaction.Instance;
                _ts.Total = 0;
                MaxAmountAhorroVista = Convert.ToDecimal(AppConfig.Get("MaxAmountAhorroVista"));
                view = new CollectionViewSource();
                lstPager = new ObservableCollection<ProductsState>();

                InitView();

                // Limpiar selección después de InitView
                ClearSelection();

                // IMPORTANTE: Desactivar bandera DESPUÉS de todo
                _isInitializing = false;

                Utilities.Speak("Selecciona el producto con el que vas a pagar.");
                this.Unloaded += OnUnloaded;
                this.Loaded += Onloaded;
            }
            catch (Exception ex)
            {
                _isInitializing = false;
                // Log del error
            }
        }
        private void ClearSelection()
        {
            // Usar Dispatcher para asegurar que se ejecute después del binding
            Dispatcher.BeginInvoke(new Action(() =>
            {
                lv_Products.SelectedItem = null;
                lv_Products.SelectedIndex = -1;

                // También limpiar en la fuente de datos
                if (lstPager != null)
                {
                    foreach (var item in lstPager)
                    {
                        item.IsSelected = false;
                    }
                }
            }), DispatcherPriority.DataBind);
        }
        //public ListProductsUC()
        //{
        //    InitializeComponent();
        //    try
        //    {
        //        _isInitializing = true; // Activar bandera

        //        _ts = Transaction.Instance;
        //        _ts.Total = 0;
        //        MaxAmountAhorroVista = Convert.ToDecimal(AppConfig.Get("MaxAmountAhorroVista"));
        //        view = new CollectionViewSource();
        //        lstPager = new ObservableCollection<ProductsState>();
        //        //ProductsSelected = new ProductsState();

        //        InitView();
        //        lv_Products.SelectedItem = null;
        //        lv_Products.SelectedIndex = -1;
        //        Utilities.Speak("Selecciona el producto con el que vas a pagar.");
        //        this.Unloaded += OnUnloaded;
        //        this.Loaded += Onloaded;
        //    }
        //    catch (Exception ex)
        //    {
        //        _isInitializing = false;
        //    }
        //}
        private void ListViewItem_TouchDown(object sender, TouchEventArgs e)
        {

            HandleItemSelection(sender);
        }

        private void ListViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {

            HandleItemSelection(sender);
        }

        private void HandleItemSelection(object sender)
        {
            try
            {
                var listViewItem = sender as ListViewItem;
                var service = listViewItem?.DataContext as ProductsState;

                if (service != null && service.ValorPagar > 0)
                {
                    // Deseleccionar producto anterior
                    if (ProductsSelected != null)
                    {
                        ProductsSelected.IsSelected = false;
                        ProductsSelected.img = GetImage(false);
                    }

                    // Seleccionar nuevo producto
                    service.IsSelected = true;
                    service.img = GetImage(true);
                    ProductsSelected = service;

                    // Actualizar vista
                    lv_Products.Items.Refresh();

                    // Tu lógica existente
                    _ts.ProductSelect = service;
                    _ts.Total = RoundValue(service.ValorPagar, true);

                    Dispatcher.BeginInvoke((Action)delegate
                    {
                        this.Opacity = 0.3;
                        StopTimer();

                        if (service.TipoProducto == (int)ETypeProductCootregua.AhorrosVista)
                        {
                            MaxAmountAhorroVista = TransactionType == TransactionType ? (service.Saldo - 100) : MaxAmountAhorroVista;
                            if (service.TipoProducto == 2)
                            {
                                MaxAmountAhorroVista = service.Saldo - 100;
                            }
                            if (service.TipoProducto == 2 && MaxAmountAhorroVista > Convert.ToDecimal(AppConfig.Get("MaxAmountAhorroVistaWithdrawal")))
                            {
                                MaxAmountAhorroVista = Convert.ToDecimal(AppConfig.Get("MaxAmountAhorroVistaWithdrawal"));
                            }

                            // Tu modal aquí
                            ModalPrueba();
                        }
                        else
                        {
                            // Tu modal aquí para otros tipos de producto
                        }

                        this.Opacity = 1;

                        if (_ts.Total == 0)
                        {
                            _ts.Total = RoundValue(service.ValorPagar, true);
                        }
                        else
                        {
                            SaveTransaction();
                        }
                    });

                    GC.Collect();
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }
        private void InitView()
        {
            try
            {
                //btnPagar.Source = new BitmapImage(new Uri("/Images/Buttons/BtnPagar.png", UriKind.Relative));

                // Limpiar lista anterior
                lstPager.Clear();

                foreach (var product in _ts.DataProducts.OrderByDescending(f => f.ProxDate))
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
                    else if (product.ProxDate <= DateTime.Now.AddDays(30) && product.ProxDate >= DateTime.Now.AddDays(1))
                    {
                        color = "Yellow";
                    }
                    else
                    {
                        color = "Green";
                    }

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
                        Cuota = RoundValue(product.Cuota, true),
                        ValorPagar = RoundValue(total, true),
                        ColorState = color,
                        RetirarProducto = product.RetirarProducto,
                        IsSelected = false, // IMPORTANTE: Siempre false al crear
                        SelectionColor = "LightGray"
                    });
                }

                if (lstPager.Count > 0)
                {
                    view.Source = lstPager;
                    lv_Products.DataContext = view;
                }
                else
                {
                    string ms = string.Format("Estimado {0}, {1} No se encontrarón productos para este tipo de trámite.", _ts.DataPerson.FirstName, Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }
        //private void InitView()
        //{
        //    try
        //    {
        //        btnPagar.Source = new BitmapImage(new Uri("/Images/Buttons/BtnPagar.png", UriKind.Relative));


        //        foreach (var product in _ts.DataProducts.OrderByDescending(f => f.ProxDate))
        //        {
        //            decimal total = product.ValorAPagar == 0 ? product.Cuota : product.ValorAPagar;
        //            string color = string.Empty;

        //            if (product.TipoProducto == (int)ETypeProductCootregua.AhorrosVista)
        //            {
        //                total = 100;
        //            }

        //            if (product.ProxDate <= DateTime.Now.AddDays(1))
        //            {
        //                color = "Red";
        //            }
        //            else if (product.ProxDate <= DateTime.Now.AddDays(30) && product.ProxDate >= DateTime.Now.AddDays(1))
        //            {
        //                color = "Yellow";
        //            }
        //            else
        //            {
        //                color = "Green";
        //            }

        //                lstPager.Add(new ProductsState
        //                {
        //                    CodSession = product.CodSession,
        //                    CreationDate = product.CreationDate,
        //                    Identititfy = product.Identititfy,
        //                    NameLine = product.NameLine.ToString().ToLower(),
        //                    NameProduct = product.NameProduct,
        //                    NumberProduct = product.NumberProduct,
        //                    PaymentMethod = product.PaymentMethod,
        //                    ProxDate = product.ProxDate,
        //                    Saldo = product.Saldo,
        //                    TipoProducto = product.TipoProducto,
        //                    img = GetImage(false),
        //                    Cuota = RoundValue(product.Cuota, true),
        //                    ValorPagar = RoundValue(total, true),
        //                    ColorState = color,
        //                    RetirarProducto = product.RetirarProducto,
        //                    IsSelected = false, // NUEVA PROPIEDAD
        //                    SelectionColor = "LightGray" // NUEVA PROPIEDAD
        //                });
        //            }

        //        if (lstPager.Count > 0)
        //        {
        //            view.Source = lstPager;
        //            lv_Products.DataContext = view;
        //        }
        //        else
        //        {
        //            string ms = string.Format("Estimado {0}, {1} No se encontrarón productos para este tipo de trámite.", _ts.DataPerson.FirstName, Environment.NewLine);
        //            // Tu lógica de mensaje aquí
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
        //    }
        //}


        private void lv_Products_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // CRÍTICO: Salir si estamos inicializando
            if (_isInitializing) return;

            ListView listView = sender as ListView;
            if (listView.SelectedItem != null)
            {
                ProductsSelected = listView.SelectedItem as ProductsState;
                calcular(ProductsSelected);
                Console.WriteLine($"Producto seleccionado: {ProductsSelected?.NumberProduct}");
            }
            else
            {
                ProductsSelected = null;
            }
        }
        //private void lv_Products_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    ListView listView = sender as ListView;
        //    if (listView.SelectedItem != null)
        //    {
        //        // Asume que tu clase de producto se llama Product
        //        ProductsSelected = listView.SelectedItem as ProductsState; // o el tipo que corresponda

        //        calcular(ProductsSelected);
        //        // Si necesitas el índice
        //        // int selectedIndex = listView.SelectedIndex;

        //        Console.WriteLine($"Producto seleccionado: {ProductsSelected?.NumberProduct}");
        //    }
        //    else
        //    {
        //        ProductsSelected = null;
        //    }
        //}

        private void calcular(ProductsState ProductsSelected)
        {
            try
            {


                _ts.ProductSelect = ProductsSelected;

                _ts.Total = Utilities.RoundValue(ProductsSelected.ValorPagar, true);

                Dispatcher.BeginInvoke((Action)delegate
                {
                    this.Opacity = 0.3;
                    //Switcher.Timer(false);

                    if (ProductsSelected.TipoProducto == (int)ETypeProductCootregua.AhorrosVista)
                    {
                        MaxAmountAhorroVista = _ts.Type == ETransactionType.Withdrawal ? (ProductsSelected.Saldo - 100) : MaxAmountAhorroVista;

                        if (_ts.Type == ETransactionType.Withdrawal && MaxAmountAhorroVista > Convert.ToDecimal(Utilities.GetConfiguration("MaxAmountAhorroVistaWithdrawal")))
                        {
                            MaxAmountAhorroVista = Convert.ToDecimal(Utilities.GetConfiguration("MaxAmountAhorroVistaWithdrawal"));
                        }

                        ModalAmountWindow modal = new ModalAmountWindow(MaxAmountAhorroVista, ProductsSelected.TipoProducto);
                        modal.ShowDialog();
                        _ts.Total = modal.ValueToPay;
                    }
                    else
                    {
                        ModalAmountWindow modal = new ModalAmountWindow(_ts.Total, ProductsSelected.TipoProducto);
                        modal.ShowDialog();
                        _ts.Total = modal.ValueToPay;
                    }

                    this.Opacity = 1;
                    // Switcher.Timer(true);

                    if (_ts.Total == 0)
                    {
                        _ts.Total = Utilities.RoundValue(ProductsSelected.ValorPagar, true);
                    }
                    else
                    {
                        //  SaveTransaction();
                    }
                });
                GC.Collect();
            }
            catch (Exception ex)
            {
                // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }

        private void btnPagar_TouchDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                var selectedProduct = lv_Products.SelectedItem as ProductsState;


                if (selectedProduct != null && _ts.Total > 0)
                {
                    ProductsSelected = selectedProduct;
                    SaveTransaction();
                    //_nav.ShowModal("Estamos procesando el pago...", new InfoModal()); // Mostrar modal de procesamiento
                    //_nav.CloseModal();
                }
                else
                {
                    //StopTimer();
                    // Switcher.ModalMS(string.Format("Estimado {0}, debe de seleccionar un producto para continuar.", transaction.DataPerson.FirstName));
                    //GoTimer();
                    _nav.ShowModal(string.Format("Estimado {0}, debe de seleccionar un producto para continuar.", _ts.DataPerson.FirstName), new InfoModal());

                }
                _nav.CloseModal();

            }
            catch (Exception ex)
            {
                // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }

        }

        private string GetImage(bool flag)
        {
            try
            {
                if (!flag)
                {
                    return "/Images/Others/circle.png";
                }

                return "/Images/Others/ok.png";
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
            return string.Empty;
        }

        private void txtSaldo_TouchDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                var txt = sender as Run;

                if (txt.Text.Contains("*"))
                {
                    txt.Text = String.Format("{0:C0}", Convert.ToDecimal(txt.Tag));
                    CloseModal();
                }
                else
                {
                    txt.Text = "********";
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);

                // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }

        private bool ModalPrueba()
        {
            bool result = false;

            BillReportViewModel model = new BillReportViewModel
            {
                Title = "Estimado Cliente: "
            };


            Application.Current.Dispatcher.Invoke(delegate
            {
                var _currentModal = new BillReportWindow(model, _document.header, _document.body, _document.footer);
                _currentModal.ShowDialog();
                if (_currentModal.DialogResult.HasValue)
                {
                    result = _currentModal.DialogResult.Value;
                    // if (result) PrintVoucher();
                }
            });
            return result;
        }
        public static decimal RoundValue(decimal Total, bool arriba)
        {
            try
            {
                decimal roundTotal = 0;

                if (arriba)
                {
                    roundTotal = Math.Ceiling(Total / 100) * 100;
                }
                else
                {
                    roundTotal = Math.Floor(Total / 100) * 100;
                }

                return roundTotal;
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);

                // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, "Utilities", ex);
                return Total;
            }
        }
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CloseModal();
            StopTimer();
            //ScannerController.Stop();
            //ScannerController.ScannerDataReceived -= OnScannerDataReceived;
        }
        private void Onloaded(object sender, RoutedEventArgs e)
        {
            ClearSelection();
            //ScannerController.ScannerDataReceived += OnScannerDataReceived;
            //ScannerController.Start();
            //       _viewModel.HelpMessage = "Ingresa número de cuenta o referente";
        }

        
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

        public bool ShowModal(string msg, ModalType type)
        {
            bool result = false;

            ModalViewModel model = new ModalViewModel
            {
                Title = "Estimado Cliente: ",
                Message = msg,
                TypeModal = type,
            };

            Application.Current.Dispatcher.Invoke(delegate
            {
                _currentLoadModal = new ModalWindow(model);
                _currentLoadModal.ShowDialog();
                if (_currentLoadModal.DialogResult.HasValue)
                {
                    result = _currentLoadModal.DialogResult.Value;
                }
            });
            return result;
        }
        public void CloseModal() => Application.Current.Dispatcher.Invoke(delegate
        {
            if (_currentLoadModal != null)
            {
                _currentLoadModal.Close();
                _currentLoadModal = null;
            }
        });

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

        private void SaveTransaction()
        {
            CloseModal();
            try
            {
                // Add null check for _ts and its properties
                if (_ts == null)
                {
                    EventLogger.SaveLog(EventType.Error, "Transaction object (_ts) is null in SaveTransaction", null);
                    _nav.ShowModal("Error: No se pudo obtener la información de la transacción. Por favor intenta de nuevo.", new InfoModal());
                    return;
                }


                ///DATOS QUEMADOS PARA SIMULAR DATA PERSON 
                ///
                

                if (_ts.DataPerson == null)
                {
                    EventLogger.SaveLog(EventType.Error, "DataPerson is null in SaveTransaction", null);
                    _nav.ShowModal("Error: No se pudo obtener la información de la persona. Por favor intenta de nuevo.", new InfoModal());
                    return;
                }

                if (_ts.ProductSelect?.TipoProducto == (int)ETypeProductCootregua.Creditos)
                {
                    //string ms = string.Format("Estimado {0}, {1} Esta transacción esta siendo realizado a la cuota de su crédito.", _ts.DataPerson.FirstName, Environment.NewLine);
                    // Utilities.ShowModal(ms, EModalType.Error);
                }

               /// _ts.TipoTransaccion = TypeTransaction.Pago;

                Task.Run(async () =>
                {
                    try
                    {
                        _ts.TipoPago = TypePayment.Efectivo;
                        _ts.TipoTransaccion = TypeTransaction.Pago;
                        _ts.EstadoTransaccion = StateTransaction.Iniciada;
                        _ts.payer = new Payer
                        {
                            IDENTIFICATION = _ts.Documento,
                            NAME = _ts.DataPerson?.FirstName ?? string.Empty,
                            EMAIL = _ts.DataPerson?.Email ?? string.Empty
                        };

                        var tsCreated = await Api.CreateTransaction();
                        if (tsCreated == null) throw new Exception("No se pudo enviar la transacción");

                        CloseModal();

                        if (_ts.IdTransaccionApi == 0)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _nav.ShowModal("Se presentó un problema en los servicios de consulta, por favor intentalo más tarde.", new InfoModal());
                                GoTimer();
                            });
                        }
                        else
                        {
                           
                           Dispatcher.Invoke(() => GoTo(new PaymentUC()));
                               
                        }
                    }
                    catch (Exception taskEx)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            CloseModal();
                            EventLogger.SaveLog(EventType.Error, $"Error in SaveTransaction task: {taskEx.Message}", taskEx);
                            _nav.ShowModal("Error procesando la transacción. Por favor intenta de nuevo.", new InfoModal());
                        });
                    }
                });

                StopTimer();
               //_nav.ShowModal("Procesando transacción...");
               // CloseModal();
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución:SaveTransaction de listProducts {ex.Message}", ex);
                CloseModal();
                _nav.ShowModal("Error inesperado. Por favor intenta de nuevo.", new InfoModal());
            }
            _nav.CloseModal();
        }
        private void btnCancelar_TouchDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            StopTimer();
            //Switcher.CLose();
        }


        public class ListProductsViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;


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
    }
}
}