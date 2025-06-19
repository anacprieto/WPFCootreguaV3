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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace WPFCootreguaV2.UserControls
{
    /// <summary>
    /// Lógica de interacción para ScanInputUC.xaml
    /// </summary>
    public partial class ListProductsUC : AppUserControl
    {
        private DocumentFormat _document = new();
        private bool isSelected = false;

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
        private bool flag = false; // Variable para controlar el estado de la imagen
        public ListProductsUC()
        {
            InitializeComponent();
            try
            {
                _ts = Transaction.Instance;
                _ts.Total = 0;
                MaxAmountAhorroVista = Convert.ToDecimal(AppConfig.Get("MaxAmountAhorroVista"));
                view = new CollectionViewSource();
                lstPager = new ObservableCollection<ProductsState>();


                InitView();

                Utilities.Speak("Selecciona el producto con el que vas a pagar.");
                this.Unloaded += OnUnloaded;
                this.Loaded += Onloaded;
            }
            catch (Exception ex)
            {
                // Log del error
            }
        }
        private void Onloaded(object sender, RoutedEventArgs e)
        {
        }


        private void InitView()
        {
            try
            {
                lstPager.Clear(); // Limpiar la lista antes de agregar nuevos elementos

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
                        Cuota = Utilities.RoundValue(product.Cuota, true),
                        ValorPagar = Utilities.RoundValue(total, true),
                        ColorState = color,
                        RetirarProducto = product.RetirarProducto,
                        IsSelected = false // Agregar esta propiedad si no existe
                    });
                }

                if (lstPager.Count > 0)
                {
                    view.Source = lstPager;
                    lv_Products.DataContext = view;
                }
            }
            catch (Exception ex)
            {
                // Log del error
            }
        }


        #region "Eventos"


        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var service = button?.DataContext as ProductsState;
                if (service != null && service.ValorPagar > 0)
                {
                    HandleProductSelection(service);
                    // No necesitas actualizar itemInCollection aquí porque HandleProductSelection ya lo hace
                }
            }
            catch (Exception ex)
            {
                // Log del error
            }
        }
        private void HandleProductSelection(ProductsState service)
        {
            try
            {
                // Limpiar selección anterior
                if (ProductsSelected != null)
                {
                    ProductsSelected.IsSelected = false;
                }

                // Limpiar todas las selecciones en la lista
                foreach (var item in lstPager)
                {
                    item.IsSelected = false;
                }

                // Seleccionar el nuevo producto
                service.IsSelected = true;
                ProductsSelected = service;

                // Actualizar la vista
                lv_Products.Items.Refresh();

                // Lógica de transacción
                _ts.ProductSelect = service;
                _ts.Total = Utilities.RoundValue(service.ValorPagar, true);

                Dispatcher.BeginInvoke((Action)delegate
                {
                    this.Opacity = 0.3;
                    // Switcher.Timer(false);

                    if (service.TipoProducto == (int)ETypeProductCootregua.AhorrosVista)
                    {
                        MaxAmountAhorroVista = _ts.Type == ETransactionType.Withdrawal ? (service.Saldo - 100) : MaxAmountAhorroVista;

                        if (_ts.Type == ETransactionType.Withdrawal && MaxAmountAhorroVista > Convert.ToDecimal(Utilities.GetConfiguration("MaxAmountAhorroVistaWithdrawal")))
                        {
                            MaxAmountAhorroVista = Convert.ToDecimal(Utilities.GetConfiguration("MaxAmountAhorroVistaWithdrawal"));
                        }

                        ModalAmountWindow modal = new ModalAmountWindow(MaxAmountAhorroVista, service.TipoProducto);
                        modal.ShowDialog();
                        _ts.Total = modal.ValueToPay;
                    }
                    else
                    {
                        ModalAmountWindow modal = new ModalAmountWindow(_ts.Total, service.TipoProducto);
                        modal.ShowDialog();
                        _ts.Total = modal.ValueToPay;
                    }

                    this.Opacity = 1;
                     GoTimer();

                    if (_ts.Total == 0)
                    {
                        _ts.Total = Utilities.RoundValue(service.ValorPagar, true);
                    }
                    else
                    {
                        SaveTransaction();
                    }
                });
                GC.Collect();
            }
            catch (Exception ex)
            {
                // Log del error
            }
        }
        private void BtnCancelar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StopTimer();
            _nav.CloseModal();
            Dispatcher.Invoke(() => GoTo(new ConfigUC()));

        }
        private void BtnPagar_TouchDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (ProductsSelected != null && _ts.Total > 0)
                {
                    SaveTransaction();

                }
                else
                {
                    StopTimer();
                    _nav.ShowModal(string.Format("Estimado {0}, debe de seleccionar un producto para continuar.", _ts.DataPerson.FirstName), new InfoModal());
                    GoTimer();
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"btnPagar_TouchDown {ex.Message}", ex);
            }
        }

        private void txtSaldo_TouchDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                var txt = sender as Run;

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
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
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

        //private void SaveTransaction()
        //{
        //    _nav.CloseModal();
        //    try
        //    {
        //        // Add null check for _ts and its properties
        //        if (_ts == null)
        //        {
        //            _nav.ShowModal("Error: No se pudo obtener la información de la transacción. Por favor intenta de nuevo.", new InfoModal());
        //            EventLogger.SaveLog(EventType.Error, "Error: No se pudo obtener la información de la transacción. Por favor intenta de nuevo.", null);

        //            return;
        //        }
        //        if (_ts.DataPerson == null)
        //        {
        //            EventLogger.SaveLog(EventType.Error, "DataPerson is null in SaveTransaction", null);
        //            _nav.ShowModal("Error: No se pudo obtener la información de la persona. Por favor intenta de nuevo.", new InfoModal());
        //            return;
        //        }

        //        if (_ts.ProductSelect?.TipoProducto == (int)ETypeProductCootregua.Creditos)
        //        {
        //            string ms = string.Format("Estimado {0}, {1} Esta transacción esta siendo realizado a la cuota de su crédito.", _ts.DataPerson.FirstName, Environment.NewLine);
        //            _nav.ShowModal(ms, new InfoModal());
        //        }

        //        _ts.TipoTransaccion = TypeTransaction.Pago;

        //        Task.Run(async () =>
        //        {
        //            try
        //            {
        //                _ts.TipoPago = TypePayment.Efectivo;
        //                _ts.TipoTransaccion = TypeTransaction.Pago;
        //                _ts.EstadoTransaccion = StateTransaction.Iniciada;
        //                _ts.payer = new Payer 
        //                {
        //                    Document = _ts.Documento,
        //                        DocumentType = "CC",
        //                        Name = _ts.DataPerson?.FirstName ?? string.Empty,
        //                        LastName = string.Concat(_ts.DataPerson.FirstLastName, " ", _ts.DataPerson.SecondLastName),
        //                        Email = _ts.DataPerson?.Email ?? string.Empty,
        //                        Phone = _ts.DataPerson?.Phone ?? string.Empty,
        //                        Adress = _ts.DataPerson?.Adress ?? string.Empty,
        //                        IdTransaction = _ts.IdTransaccionApi, // Este valor debe estar disponible después de crear la transacción
        //                        IdPayPad = _ts.IdPaypad,
        //                        IdClient = 27
        //                };

        //                var tsCreated = await Api.CreateTransaction();
        //                if (tsCreated == null)
        //                {
        //                    //throw new Exception("No se pudo enviar la transacción");
        //                    EventLogger.SaveLog(EventType.Error, "No se pudo enviar la transacción");

        //                }; 



        //                _nav.CloseModal();

        //                if (_ts.IdTransaccionApi == 0)
        //                {
        //                    Dispatcher.Invoke(() =>
        //                    {
        //                        _nav.ShowModal("Se presentó un problema en los servicios de consulta, por favor intentalo más tarde.", new InfoModal());
        //                        EventLogger.SaveLog(EventType.Error, "No se pudo enviar la transacción,_ts.IdTransaccionApi==0");

        //                        GoTimer();
        //                    });
        //                }
        //                else
        //                {
                           
        //                   Dispatcher.Invoke(() => GoTo(new PaymentUC()));
        //                    EventLogger.SaveLog(EventType.Error, "Yendo a ventana de pago");


        //                }
        //            }
        //            catch (Exception taskEx)
        //            {
        //                Dispatcher.Invoke(() =>
        //                {
        //                    _nav.CloseModal();
        //                    EventLogger.SaveLog(EventType.Error, $"Error in SaveTransaction task: {taskEx.Message}", taskEx);
        //                    _nav.ShowModal("Error procesando la transacción. Por favor intenta de nuevo.", new InfoModal());
        //                });
        //            }
        //        });

        //        StopTimer();
        //        _nav.ShowModal("Procesando transacción...");
        //       // CloseModal();
        //    }
        //    catch (Exception ex)
        //    {
        //        EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución:SaveTransaction de listProducts {ex.Message}", ex);
        //    }
        //}

        private async Task SaveTransaction()
        {
            try
            {
                // Crear la transacción y esperar su resultado
                if (_ts == null)
                {
                    _nav.ShowModal("Error: No se pudo obtener la información de la transacción. Por favor intenta de nuevo.", new InfoModal());
                    EventLogger.SaveLog(EventType.Error, "Error: No se pudo obtener la información de la transacción. Por favor intenta de nuevo.", null);

                    return;
                }
                if (_ts.DataPerson == null)
                {
                    EventLogger.SaveLog(EventType.Error, "DataPerson is null in SaveTransaction", null);
                    _nav.ShowModal("Error: No se pudo obtener la información de la persona. Por favor intenta de nuevo.", new InfoModal());
                    return;
                }

                if (_ts.ProductSelect?.TipoProducto == (int)ETypeProductCootregua.Creditos)
                {
                    string ms = string.Format("Estimado {0}, {1} Esta transacción esta siendo realizado a la cuota de su crédito.", _ts.DataPerson.FirstName, Environment.NewLine);
                    _nav.ShowModal(ms, new InfoModal());
                }

                _ts.TipoTransaccion = TypeTransaction.Pago;
                // Configurar los datos de la transacción después de que se haya creado exitosamente
                _ts.EstadoTransaccion = StateTransaction.Iniciada;
                _ts.Total = 0;
                _ts.TipoPago = TypePayment.Efectivo;
                _ts.Type= ETransactionType.Payment;
                _ts.payer = new Payer
                {
                    Document = _ts.Documento,
                    DocumentType = "CC",
                    Name = _ts.DataPerson?.FirstName ?? string.Empty,
                    LastName = string.Concat(_ts.DataPerson.FirstLastName, " ", _ts.DataPerson.SecondLastName),
                    Email = _ts.DataPerson?.Email ?? string.Empty,
                    Phone = _ts.DataPerson?.Phone ?? string.Empty,
                    Adress = _ts.DataPerson?.Adress ?? string.Empty,
                    IdTransaction = _ts.IdTransaccionApi, // Este valor debe estar disponible después de crear la transacción
                    IdPayPad = _ts.IdPaypad,
                    IdClient = 27
                };

                // Crear el pagador y esperar su resultado
                var payerCreated = await Api.CreatePayer();
                if (payerCreated == null)
                    throw new Exception("No se pudo enviar el pagador");

                // _nav.CloseModal();

                // Validar si el proceso fue exitoso
                _nav.CloseModal();

                if (_ts.IdTransaccionApi == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        _nav.ShowModal("Se presentó un problema en los servicios de consulta, por favor intentalo más tarde.", new InfoModal());
                        EventLogger.SaveLog(EventType.Error, "No se pudo enviar la transacción,_ts.IdTransaccionApi==0");

                        GoTimer();
                    });
                }
                else
                {

                    Dispatcher.Invoke(() => GoTo(new PaymentUC()));
                    EventLogger.SaveLog(EventType.Error, "Yendo a ventana de pago");


                }

            }
            catch (Exception ex)
            {
                _nav.CloseModal(); // Asegurar que se cierre el modal en caso de error
                EventLogger.SaveLog(EventType.Error, $"Error al guardar transacción: {ex.Message}");

                // Mostrar mensaje de error al usuario
                _nav.ShowModal("No se pudo completar el registro. Por favor intentalo de nuevo.");
                _nav.CloseModal();
            }
        }


        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _nav.CloseModal();
            StopTimer();
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

        private bool _isSelected = false;
        public bool IsSelected
        {
                get { return _isSelected; }
                set
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
        }

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
            #endregion
        }
    }
}