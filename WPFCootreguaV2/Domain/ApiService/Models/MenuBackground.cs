using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCootreguaV2.Domain.ApiService.Models
{
    public class MenuBackground: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyRaised(string propertyname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
    }

    private string _Background;

    public string Background
    {
        get
        {
            return _Background;
        }
        set
        {
            _Background = value;
            OnPropertyRaised("Background");
        }
    }
}
}
