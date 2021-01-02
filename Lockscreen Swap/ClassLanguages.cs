using System;
using System.ComponentModel;





namespace Lockscreen_Swap
{
    class ClassLanguages
    {
        public string name { get; set; }
        public string code { get; set; }

        public ClassLanguages(string name, string code)
        {
            this.name = name;
            this.code = code;
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
