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
using System.Windows.Shapes;

namespace WPFCootreguaV2.UserControls
{
    /// <summary>
    /// Lógica de interacción para GenericButton.xaml
    /// </summary>
    public partial class GenericButton : UserControl
    {
        public GenericButton()
        {
            InitializeComponent();
        }
        public void GenericButton_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustButtonSizeToFitText();
        }
        public Brush ButtonBackground
        {
            get { return (Brush)GetValue(ButtonBackgroundProperty); }
            set { SetValue(ButtonBackgroundProperty, value); }
        }
        public Brush ForegroundButton
        {
            get { return (Brush)GetValue(ForegroundButtonProperty); }
            set { SetValue(ForegroundButtonProperty, value); }
        }
        public Brush BorderForeground
        {
            get { return (Brush)GetValue(BorderForegroundProperty); }
            set { SetValue(BorderForegroundProperty, value); }
        }
        public Brush IconForeground
        {
            get { return (Brush)GetValue(IconForegroundProperty); }
            set { SetValue(IconForegroundProperty, value); }
        }
        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }
        public PackIconKind IconKind
        {
            get { return (PackIconKind)GetValue(IconKindProperty); }
            set { SetValue(IconKindProperty, value); }
        }



        public static readonly DependencyProperty ButtonBackgroundProperty =
            DependencyProperty.Register("ButtonBackground", typeof(Brush), typeof(GenericButton), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty ForegroundButtonProperty =
            DependencyProperty.Register("ForegroundButton", typeof(Brush), typeof(GenericButton), new PropertyMetadata(Brushes.Transparent));
        
        public static readonly DependencyProperty BorderForegroundProperty =
                   DependencyProperty.Register("BorderForeground", typeof(Brush), typeof(GenericButton), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty IconForegroundProperty =
                  DependencyProperty.Register("IconForeground", typeof(Brush), typeof(GenericButton), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(GenericButton), new PropertyMetadata(string.Empty));
        
        public static readonly DependencyProperty IconKindProperty = 
            DependencyProperty.Register("IconKind", typeof(PackIconKind), typeof(GenericButton), new PropertyMetadata(default(PackIconKind)));


        private void AdjustButtonSizeToFitText()
        {
            // Asegúrate de que el TextBlock y el Border estén cargados
            if (this.IsLoaded)
            {
                TextBlock textBlock = (TextBlock) ButtonMessage; // Asegúrate de asignar el nombre correcto del TextBlock
                ColumnDefinition border = (ColumnDefinition) ParentBorder; // Asegúrate de asignar el nombre correcto del Border

                if (textBlock != null && border != null)
                {
                    textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    double textWidth = textBlock.DesiredSize.Width;

                    if (textWidth > border.ActualWidth)
                    {
                        // Ajusta el ancho del botón basado en el ancho del texto
                        
                        // Puedes agregar un valor extra al ancho para asegurar un margen alrededor del texto
                        var startActualWidht = border.ActualWidth / border.Width.Value;
                        var numberNewStarts = textWidth / startActualWidht;
                        var startQuantities = (int) Math.Ceiling(numberNewStarts)+1;
                        var parentDelParent = (border.Parent as Grid).Parent as Border;
                        parentDelParent.Width =  startActualWidht * (startQuantities + 4);
                        GridLength gridLength = new GridLength(startQuantities, GridUnitType.Star);
                        border.Width = gridLength; // Ajusta este valor según sea necesario

                    }
                }
            }
        }
    }
}
