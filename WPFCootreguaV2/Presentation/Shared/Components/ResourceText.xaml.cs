using MaterialDesignThemes.Wpf;
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

namespace WPFCootreguaV2.UserControls
{

    public partial class ResourceText : UserControl
    {
        public ResourceText()
        {
            InitializeComponent();
        }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public object Resource
        {
            get { return GetValue(ResourceProperty); }
            set { SetValue(ResourceProperty, value); }
        }
        public Brush ForegroundText
        {
            get { return (Brush)GetValue(ForegroundTextProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ResourceText), new PropertyMetadata(string.Empty));
        
        public static readonly DependencyProperty ResourceProperty =
              DependencyProperty.Register("Resource", typeof(object), typeof(ResourceText), new PropertyMetadata(null));

        public static readonly DependencyProperty ForegroundTextProperty =
             DependencyProperty.Register("ForegroundText", typeof(Brush), typeof(ResourceText), new PropertyMetadata(Brushes.Transparent));

    }
}
