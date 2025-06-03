using HantleDispenserAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Models;
using WPFCootreguaV2.UserControls;

namespace WPFCootreguaV2.Presentation.UserControls
{
    /// <summary>
    /// Lógica de interacción para MainUC.xaml
    /// </summary>
    public partial class MainUC : AppUserControl
    {

        
        //private ImageSliderViewModel _sliderViewModel;
        //private Task? _initTask = null;
        private TimerGeneric _timer;
        private const string STR_TIMER = "09:59";
        //private ImageSleader _imageSleader;

        private string folderPath = @$"{AppConfig.Get("PublishDir")}"; // Cambia esto a la carpeta que contiene las imágenes y videos
        private string[] files;
        private int currentIndex = 0;

        private ImageSleader _imageSleader;
        private bool _validatePaypad;

        public MainUC()
        {
            InitializeComponent();
            Transaction.Reset();
            //_validatePaypad = true;
            Init();
           // this.Unloaded += OnUnLoaded;
            //LoadFiles();
            //StartSlideshow();
            _validatePaypad = true;
        }
        private void Grid_TouchDown(object sender, EventArgs e)
        {
            _imageSleader.Stop();
            _validatePaypad = false;
            GC.Collect();
            Dispatcher.Invoke(() => GoTo(new TypeTransUC()));
        }
        private async void Init()
        {
            try
            {

                // AdminPayPlus.NotificateInformation();
#if NO_PERIPHERALS
#else
     //       _testingConnection = InternetConnectionManager.StartTestingConnection();
     //       await VideoRecorder.Stop();
#endif
                ConfiguratePublish();
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución al init: {ex.Message}", ex);

            }

        }

        //private async void Continuar_Touch(object sender, EventArgs e)
        //{
        //   Dispatcher.Invoke(() => GoTo(new MenuUC()));
        //}

        private async void OnUnLoaded(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }

        private void ConfiguratePublish()
        {
            try
            {
                if (_imageSleader == null)
                {
                    string folder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Images", "Publish");

                    List<string> imageFiles = Directory.GetFiles(folder, "*.jpg")
                                         .Concat(Directory.GetFiles(folder, "*.png"))
                                         .ToList();

                    _imageSleader = new ImageSleader(imageFiles, folder);

                    this.DataContext = _imageSleader.imageModel;

                    _imageSleader.time = int.Parse(AppConfig.Get("TimerPublicity"));

                    _imageSleader.isRotate = true;

                    _imageSleader.Start();
                }
            }
            catch (Exception ex)
            {

            }
        }


        //private async void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    GC.Collect();
        //    Dispatcher.Invoke(() => GoTo(new TypeTransUC()));
        //}


    }
}
