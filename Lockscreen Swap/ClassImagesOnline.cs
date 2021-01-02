using System;
using System.ComponentModel;





namespace Lockscreen_Swap
{
    class ClassImagesOnline
    {
        public string name { get; set; }
        public string autor { get; set; }
        public string directory { get; set; }

        public ClassImagesOnline(string name, string autor, string directory)
        {
            this.name = name;
            this.autor = autor;
            this.directory = directory;
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
