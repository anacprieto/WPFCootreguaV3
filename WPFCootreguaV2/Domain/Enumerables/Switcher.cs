using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFCootreguaV2.Presentation.UserControls;
using WPFCootreguaV2.Domain.UIServices;


namespace WPFCootreguaV2.Domain.Enumerables
{
    public class Switcher
    {
        public static MenuUC Navigator { get; set; }

        public static void ChangeBackg(EBackground eBackground)
        {
            Navigator.ChangeBackground(eBackground);
        }



    }
}
