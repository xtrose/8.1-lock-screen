using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Media.Imaging;





namespace Lockscreen_Swap
{
    class ClassPictures
    {
        public string imgPath { get; set; }
        public string imgThumbPath { get; set; }
        public string imgFile { get; set; }
        public int imgID { get; set; }
        public BitmapImage imgImage { get; set; } 

        public ClassPictures(string imgPath, string imgThumbPath, string imgFile, int imgID, BitmapImage imgImage)
        {
            this.imgFile = imgFile;
            this.imgPath = imgPath;
            this.imgThumbPath = imgThumbPath;
            this.imgID = imgID;
            this.imgImage = imgImage;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}