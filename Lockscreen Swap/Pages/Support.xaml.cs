using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;





namespace Lockscreen_Swap.Pages
{





    public partial class Support : PhoneApplicationPage
    {





        //Wird am Anfang der Seite geladen
        //---------------------------------------------------------------------------------------------------------
        public Support()
        {
            //Komponenten laden
            InitializeComponent();

            //Animation vorbereiten
            Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            string temp = Convert.ToString(backgroundColor);
            if (temp != "#FF000000")
            {
                Logo2.Source = new BitmapImage(new Uri("Images/StartUp/Logo800_2.png", UriKind.Relative));
                ImgTop.Source = new BitmapImage(new Uri("Images/Logo.Light.png", UriKind.Relative));
                ImgTop.Opacity = 0.1;
                ImgSupport.Source = new BitmapImage(new Uri("Images/Support.Light.png", UriKind.Relative));
            }
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Support
        //---------------------------------------------------------------------------------------------------------
        private void LinkXtrose(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var wb = new WebBrowserTask();
            wb.URL = "http://www.xtrose.com";
            wb.Show();
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Support
        //---------------------------------------------------------------------------------------------------------
        private void BtnSupport(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EmailComposeTask emailcomposer = new EmailComposeTask();
            emailcomposer.To = "xtrose@hotmail.com";
            emailcomposer.Subject = "8.1 Lock Screen Support";
            emailcomposer.Body = "";
            emailcomposer.Show();
        }
        //---------------------------------------------------------------------------------------------------------
    }
}