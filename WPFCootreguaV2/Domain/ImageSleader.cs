using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;


namespace WPFCootreguaV2.Domain
{

    class ImageSleader : Window
    {

        private List<string> images;

    private string[] validImageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

    private string folder = string.Empty;

    private int CurrentSourceIndex = 0;

    private int CurrentCtrlIndex;

    public int time;

    public bool isRotate = false;

    public ImageStory imageModel;

    private DispatcherTimer dispatcherTimer;

    public Action<string> callbackError;

    public ImageSleader(List<string> images, string folder)
    {
        if (images != null && images.Count > 0)
        {
            this.images = images;
        }
        else if (!string.IsNullOrEmpty(folder))
        {
            this.folder = folder;

            LoadImageFolder();
        }

        dispatcherTimer = new DispatcherTimer();

        dispatcherTimer.Tick += dispatcherTimer_Tick;

        Init();
    }

    public void Start()
    {
        try
        {
            PlaySlideShow(0);
            if (isRotate)
            {
                StarTime();
            }
        }
        catch (Exception ex)
        {
            callbackError?.Invoke(ex.ToString());
        }
    }

    public void Stop()
    {
        StopTime();
    }

    private void Init()
    {
        try
        {
            imageModel = new ImageStory
            {
                sourse = images[0],
                buttonBack = Visibility.Hidden,
                buttonNext = Visibility.Visible,
                buttonFinish = Visibility.Hidden
            };
        }
        catch (Exception ex)
        {
            callbackError?.Invoke(ex.ToString());
        }
    }

    private void LoadImageFolder()
    {
        try
        {
            if (Directory.Exists(folder))
            {
                Random r = new Random();
                var sources = from file in new DirectoryInfo(folder).GetFiles().AsParallel()
                              where validImageExtensions.Contains(file.Extension, StringComparer.InvariantCultureIgnoreCase)
                              orderby r.Next()
                              select file.FullName;

                images = new List<string>();

                images.AddRange(sources);
            }
        }
        catch (Exception ex)
        {
            callbackError?.Invoke(ex.ToString());
        }
    }

    private void PlaySlideShow(int direction)
    {
        try
        {
            if (images.Count == 0 || CurrentSourceIndex >= images.Count || CurrentSourceIndex < 0)
            {
                return;
            }

            if (direction == 1)
            {
                CurrentSourceIndex = (CurrentSourceIndex + 1);
            }
            else if (direction == 2)
            {
                CurrentSourceIndex = (CurrentSourceIndex - 1);
            }

            Dispatcher.Invoke(() =>
            {
                imageModel.sourse = images[CurrentSourceIndex];

                if (!isRotate)
                {
                    if (CurrentSourceIndex < (images.Count - 1) && CurrentSourceIndex > 0)
                    {
                        imageModel.buttonBack = Visibility.Visible;
                        imageModel.buttonNext = Visibility.Visible;
                        imageModel.buttonFinish = Visibility.Hidden;
                    }
                    else if (CurrentSourceIndex == 0)
                    {
                        imageModel.buttonBack = Visibility.Hidden;
                        imageModel.buttonNext = Visibility.Visible;
                        imageModel.buttonFinish = Visibility.Hidden;
                    }
                    else if (CurrentSourceIndex == (images.Count - 1))
                    {
                        imageModel.buttonBack = Visibility.Visible;
                        imageModel.buttonNext = Visibility.Hidden;
                        imageModel.buttonFinish = Visibility.Visible;
                    }
                }
            });
        }
        catch (Exception ex)
        {
            callbackError?.Invoke(ex.ToString());
        }
    }

    public void StarTime()
    {
        dispatcherTimer.Interval = new TimeSpan(0, 0, this.time);
        if (dispatcherTimer.IsEnabled)
        {
            dispatcherTimer.Stop();
        }
        dispatcherTimer.Start();
    }

    public void StopTime()
    {
        dispatcherTimer.Stop();
        dispatcherTimer = null;
    }

    private void dispatcherTimer_Tick(object sender, EventArgs e)
    {
        try
        {
            if (CurrentSourceIndex == images.Count - 1)
            {
                CurrentSourceIndex = 0;
                PlaySlideShow(0);
            }
            else
            {
                PlaySlideShow(1);
            }
        }
        catch (Exception ex)
        {
            callbackError?.Invoke(ex.ToString());
        }
    }

    public void moveBack()
    {
        try
        {
            PlaySlideShow(2);
        }
        catch (Exception ex)
        {
            callbackError?.Invoke(ex.ToString());
        }
    }

    public void moveNext()
    {
        try
        {
            PlaySlideShow(1);
        }
        catch (Exception ex)
        {
            callbackError?.Invoke(ex.ToString());
        }
    }
}

class ImageStory : INotifyPropertyChanged
{
    private string _sourse;

    public string sourse
    {
        get
        {
            return _sourse;
        }
        set
        {
            _sourse = value;
            OnPropertyRaised("sourse");
        }
    }


    private Visibility _buttonBack;

    public Visibility buttonBack
    {
        get
        {
            return _buttonBack;
        }
        set
        {
            _buttonBack = value;
            OnPropertyRaised("buttonBack");
        }
    }

    private Visibility _buttonNext;

    public Visibility buttonNext
    {
        get
        {
            return _buttonNext;
        }
        set
        {
            _buttonNext = value;
            OnPropertyRaised("buttonNext");
        }
    }

    private Visibility _buttonFinish;

    public Visibility buttonFinish
    {
        get
        {
            return _buttonFinish;
        }
        set
        {
            _buttonFinish = value;
            OnPropertyRaised("buttonFinish");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyRaised(string propertyname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
    }
}
}
