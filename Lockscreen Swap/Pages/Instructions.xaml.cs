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










namespace Lockscreen_Swap.Pages
{
    public partial class Instructions : PhoneApplicationPage
    {










        public Instructions()
        {
            //Komponenten laden
            InitializeComponent();

            //Icons ändern
            Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            string temp = Convert.ToString(backgroundColor);
            if (temp != "#FF000000")
            {
                //Icons ändern
                ImgTop.Source = new BitmapImage(new Uri("Images/Instruction.Light.png", UriKind.Relative));
                ImgTop.Opacity = 0.1;
            }
        }










    }
}