using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Documents;

namespace WPFCootreguaV2.Modals
{
    /// <summary>
    /// Lógica de interacción para ModalWindow.xaml
    /// </summary>
    /// 
    public partial class BillReportWindow :Window
    {
        private BillReportViewModel _viewBillReportModel;
        private List<DictionaryData> _billData = new List<DictionaryData>();
        public BillReportWindow(BillReportViewModel model, Dictionary<string, string> header, Dictionary<string, string> body, Dictionary<string, string> footer)
        {
            InitializeComponent();
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.Width = this.Height * 9 / 16;
            this.WindowVB.Height = SystemParameters.PrimaryScreenHeight;
            this.WindowVB.Width = this.Height * 9 / 16;

            //this._viewBillReportModel = modalBill;

            this.DataContext = _viewBillReportModel;
            void InsertData(Dictionary<string, string> toInsert)
            {
                foreach (var entry in toInsert)
                {
                    _billData.Add(new DictionaryData { Key = entry.Key, Value = entry.Value });
                }
            }
            InsertData(new Dictionary<string, string>() { { "=====================", "==============================" } });
            InsertData(header);
            InsertData(new Dictionary<string, string>() { { "=====================", "==============================" } });
            InsertData(body);
            InsertData(new Dictionary<string, string>() { { "=====================", "==============================" } });
            InsertData(footer);
            InsertData(new Dictionary<string, string>() { { "=====================", "==============================" } });


            DataView.ItemsSource = _billData;

            //ConfigureModal();
        }
        private void BtnClose_MouseDown(object sender, EventArgs e)
        {
            this.DialogResult = true;

        }
        public class DictionaryData
        {
            public string Key { get; set; }

            public string Value { get; set; }

        }
        //private void ConfigureModal()
        //{
        //    this.BtnOk.Visibility = _viewModel.TypeModal.BtnOkVisibility;
        //    this.BtnYes.Visibility = _viewModel.TypeModal.BtnYesVisibility;
        //    this.BtnNo.Visibility = _viewModel.TypeModal.BtnNoVisibility;
        //    this.LoadGif.Visibility = _viewModel.TypeModal.LoadGifVisibility;
        //}
    }

    public class BillReportViewModel : INotifyPropertyChanged
    {
        private string _message = string.Empty;

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                OnPropertyRaised(nameof(Message));
            }
        }

        

        private string _title;

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyRaised(nameof(Title));
            }
        }

        private ModalType _typeModal;

        public ModalType TypeModal
        {
            get
            {
                return _typeModal;
            }
            set
            {
                _typeModal = value;
                OnPropertyRaised(nameof(TypeModal));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyRaised(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

        }
    }

}
