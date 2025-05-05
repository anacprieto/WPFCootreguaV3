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
    /// <summary>
    /// Lógica de interacción para Teclado.xaml
    /// </summary>
    public partial class NumericKeyboard : UserControl
    {
        public event EventHandler<string> KeyboardPressed;
        public NumericKeyboard()
        {
            InitializeComponent();
        }

        private void Keyboard_MouseDown(object sender, EventArgs e)
        {
            Image key = (Image)sender;
            string tag = key.Tag.ToString() ?? "";
            KeyboardPressed?.Invoke(this, tag);
        }
    
    }
}
