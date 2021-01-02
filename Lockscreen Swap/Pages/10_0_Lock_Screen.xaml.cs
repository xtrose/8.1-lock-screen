using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;






// Namespace
namespace Lockscreen_Swap.Pages
{





    // 10.0 Sperrbildschirm Werbung
    public partial class _10_0_Lock_Screen : PhoneApplicationPage
    {




        // Klasse erzeugen
        //---------------------------------------------------------------------------------------------------------
        public _10_0_Lock_Screen()
        {
            // UI Komponenten laden
            InitializeComponent();
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Facebook
        //---------------------------------------------------------------------------------------------------------
        private void BtnStore(object sender, RoutedEventArgs e)
        {
            var wb = new WebBrowserTask();
            wb.URL = "http://windowsphone.com/s?appid=9e37cf48-e834-4d56-ae48-e81d1b1cf608";
            wb.Show();
        }
        //---------------------------------------------------------------------------------------------------------





    }





}