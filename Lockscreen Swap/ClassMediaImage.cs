using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Lockscreen_Swap
{
    class ClassMediaImage
    {
        public int imgID { get; set; }
        public BitmapImage imgImage { get; set; }

        public ClassMediaImage(int imgID, BitmapImage imgImage)
        {
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
