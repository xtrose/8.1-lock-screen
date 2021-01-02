using System;
using System.Collections.ObjectModel;
using System.ComponentModel;





namespace Lockscreen_Swap
{
    class ClassFolders
    {
        public string name { get; set; }
        public string images { get; set; }

        public ClassFolders(string name, string images)
        {
            this.name = name;
            this.images = images;
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