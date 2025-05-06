using HantleDispenserAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.UIServices;
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

        public MainUC()
        {
            InitializeComponent();
            Transaction.Reset();
            Init();
            this.Unloaded += OnUnLoaded;
            LoadFiles();
            StartSlideshow();
        }

        #region Publish Config

        private void LoadFiles()
        {
            // Cargar imágenes y videos en formato 1920x1080
            var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".mp4", ".avi", ".mov" };
            files = Directory.GetFiles(folderPath)
                .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                .ToArray();
        }

        private async void StartSlideshow()
        {
            if (files == null || files.Length == 0)
            {
                MessageBox.Show("No se encontraron archivos en la carpeta especificada.");
                return;
            }

            while (true)
            {
                string currentFile = files[currentIndex];
                string extension = Path.GetExtension(currentFile).ToLower();

                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                {
                    ShowImage(currentFile);
                    await Task.Delay(5000); // Mostrar la imagen durante 5 segundos
                }
                else if (extension == ".mp4" || extension == ".avi" || extension == ".mov")
                {
                    await PlayVideo(currentFile);
                }

                currentIndex = (currentIndex + 1) % files.Length; // Ir al siguiente archivo
            }
        }

        private void ShowImage(string filePath)
        {
            mediaElement.Visibility = Visibility.Collapsed;
            mediaElement.Stop();
            imageDisplay.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(filePath));
            imageDisplay.Visibility = Visibility.Visible;
        }

        private async Task PlayVideo(string filePath)
        {
            imageDisplay.Visibility = Visibility.Collapsed;
            mediaElement.Source = new Uri(filePath);
            mediaElement.Visibility = Visibility.Visible;
            mediaElement.Play();

            // Esperar a que termine el video
            while (mediaElement.NaturalDuration.HasTimeSpan == false ||
                   mediaElement.Position < mediaElement.NaturalDuration.TimeSpan)
            {
                await Task.Delay(500); // Chequear cada 500ms
            }

            mediaElement.Stop();
        }

        #endregion


        private async void Init()
        {
#if NO_PERIPHERALS
#else
     //       _testingConnection = InternetConnectionManager.StartTestingConnection();
     //       await VideoRecorder.Stop();
#endif
        }

        private async void Continuar_Touch(object sender, EventArgs e)
        {
           Dispatcher.Invoke(() => GoTo(new MenuUC()));
        }

        private async void OnUnLoaded(object sender, RoutedEventArgs e)
        {
            GC.Collect();
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

                    Dispatcher.Invoke(() => GoTo(new MainUC()));
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


    }
}
