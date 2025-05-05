using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WPFCootreguaV2.Domain.UIServices
{
    public class ImageSliderViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        private List<string> _images;
        private DispatcherTimer _slideshowTimer;
        private int _indexImage = 0;

        private string _imgSource = string.Empty;
        public string ImgSource
        {
            get
            {
                return _imgSource;
            }
            set
            {
                _imgSource = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImgSource)));
            }
        }

        public ImageSliderViewModel()
        {

            _images = Directory.GetFiles(AppConfig.Get("publishDir")).ToList();
            ImgSource = _images[0];
            // Set up the timer
            _slideshowTimer = new DispatcherTimer();
            _slideshowTimer.Tick += ChangeImage;
            _slideshowTimer.Interval = TimeSpan.FromSeconds(5);
            _slideshowTimer.Start();
        }



        private void ChangeImage(object? sender, EventArgs e)
        {
            if (_images.Count > 0)
            {
                _indexImage = (_indexImage + 1) % _images.Count;
                ImgSource = _images[_indexImage];

            }
        }
    }
}
