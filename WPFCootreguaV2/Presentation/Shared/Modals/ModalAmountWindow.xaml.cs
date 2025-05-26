using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.UIServices;

namespace WPFCootreguaV2.Modals
{
    /// <summary>
    /// Interaction logic for ModalAmountWindow.xaml
    /// </summary>
    public partial class ModalAmountWindow : Window
    {
        #region "Referencias"
        private decimal ValueMax;
        public decimal ValueToPay;
        private ValueModel Value;
        private bool TouchClick;
        private Transaction _ts;

        #endregion

        #region "Constructor"
        public ModalAmountWindow(decimal valueMax, int typeProduct)
        {
            InitializeComponent();

            _ts = Transaction.Instance;

            try
            {
                TouchClick = true;
                ValueMax = valueMax;
                ValueToPay = 0;
                Value = new ValueModel
                {
                    Val = 0
                };

                if (_ts.Type == ETransactionType.Withdrawal)
                {
                    txtMsInformacion.Text = "Ingrese el valor que desea retirar";
                    btnAceptar.Source = new BitmapImage(new Uri("/Images/Buttons/retirar.png", UriKind.Relative));
                }
                else
                {
                    if (typeProduct != (int)ETypeProductCootregua.AhorrosVista)
                    {
                        Value.Val = valueMax;
                    }
                }

                this.DataContext = Value;
            }
            catch (Exception ex)
            {
               // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }
        #endregion

        #region "Eventos"
        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static readonly Regex _regex = new Regex(@"^[0-9]+$"); // Solo números

        private static bool IsTextAllowed(string text)
        {
            return _regex.IsMatch(text);
        }
        private void TxtVal_TouchDown(object sender, TouchEventArgs e)
        {
            //Utilities.OpenKeyboard(true, sender as TextBox, this, 450, 1050);
        }

        private void TxtVal_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TxtError.Text = string.Empty;

                if (TxtVal.Text.Length > 15)
                {
                    TxtVal.Text = TxtVal.Text.Remove(15, 1);
                    return;
                }
            }
            catch (Exception ex)
            {
               // Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }

        private void BtnCancel_TouchDown(object sender, TouchEventArgs e)
        {
            DialogResult = false;
        }

        private void btnAceptar_TouchDown(object sender, TouchEventArgs e)
        {
            Validate();
        }
        #endregion

        #region "Métodos"
        private void Validate()
        {
            try
            {
                if (Value.Val == 0)
                {
                    TxtError.Text = "Debe ingresar un valor mayor que 0(cero).";
                    return;
                }

                if (Value.Val > ValueMax)
                {
                    TxtError.Text = string.Concat("El valor ingresado debe ser menor o igual a ", string.Format("{0:C0}", ValueMax));
                    return;
                }

                if (Value.Val % 100 != 0)
                {
                    TxtError.Text = string.Concat("Esta máquina sólo maneja multiplos de 100",
                    Environment.NewLine, "Ejemplo: $ 100, $ 200, $ 500, etc.");
                    return;
                }

                ValueToPay = Value.Val;
                DialogResult = true;
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, this.GetType().Name, ex, ex.ToString());
            }
        }
        #endregion
    }
}
