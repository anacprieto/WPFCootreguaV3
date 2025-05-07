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
            //GoTimer();
           
        }
      
      
    }
}
