using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.UserControls;
using WPFCootreguaV2.Domain.Peripherals;
using System.Threading.Tasks;

namespace WPFCootreguaV2.Presentation.UserControls
{
    /// <summary>
    /// Lógica de interacción para MainPublicityUC.xaml
    /// </summary>
    public partial class MainPublicityUC : AppUserControl
    {

        private ImageSleader _imageSleader;
        public MainPublicityUC()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            try
            {
                ConfiguratePublish();

            }
            catch (Exception ex)
            {

            }
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

  
        private async void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GC.Collect();


            Dispatcher.Invoke(() => GoTo(new IdentificationUC()));
        }

    }
}
