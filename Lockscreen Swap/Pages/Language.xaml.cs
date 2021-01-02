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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.IO.IsolatedStorage;
using System.IO;






namespace Lockscreen_Swap.Pages
{
    public partial class Language : PhoneApplicationPage
    {

        /*
        string[] LangCodes = { "ar-SA", "az-Latn-AZ", "be-BY", "bg-BG", "ca-ES", "cs-CZ", "da-DK", "de-DE", "el-GR", "en-GB", "es-ES", "es-MX", "fa-IR", "fi-FI", "fil-PH", "fr-FR", "he-IL", "hi-IN", "hr-HR", "hu-HU", "id-ID", "it-IT", "ja-JP", "ko-KR", "lt-LT", "lv-LV", "mk-MK", "ms-MY", "nb-NO", "nl-NL", "pl-PL", "pt-BR", "pt-PT", "ro-RO", "ru-RU", "sk-SK", "sq-AL", "sr-Latn-CS", "sv-SE", "th-TH", "tr-TR", "uk-UA", "vi-VN", "zh-CN", "zh-TW", "en" };
        string[] LangNames = { "العربية", "Azərbaycan", "Беларуска", "Български", "català", "Čeština", "dansk", "deutsch", "Ελληνικά", "english", "español", "Español (México)", "فارسی", "suomi", "filipino", "Français", "עברית", "हिंदी", "hrvatski", "magyar", "Indonesia", "italiano", "日本語", "한국어", "Lietuvių", "Latviešu", "македонски", "Melayu", "Norsk", "Dutch", "Polski", "Português (Brasil)", "Português (Portugal)", "Română", "русский", "Slovenský", "shqip", "српски", "Svenska", "ไทย", "Türkçe", "Український", "Tiếng Việt", "简体中文", "繁體中文", "english (international)" };
        */

        //Variabeln erstellen
        //---------------------------------------------------------------------------------------------------------
        string[] LangCodes = { "az-Latn-AZ", "id-ID", "ms-MY", "ca-ES", "cs-CZ", "da-DK", "de-DE", "en-GB", "en", "es-ES", "es-MX", "fil-PH", "fr-FR", "hr-HR", "it-IT", "lt-LT", "lv-LV", "hu-HU", "nl-NL", "nb-NO", "pl-PL", "pt-BR", "pt-PT", "ro-RO", "sq-AL", "fi-FI", "sk-SK", "sv-SE", "vi-VN", "tr-TR", "el-GR", "be-BY", "bg-BG", "mk-MK", "ru-RU", "he-IL", "ar-SA", "fa-IR", "hi-IN", "th-TH", "ko-KR", "zh-CN", "zh-TW", "ja-JP", "sr-Latn-CS", "uk-UA" };
        string[] LangNames = { "Azərbaycan", "Bahasa Indonesia", "Behasa Melayu", "català", "Čeština", "dansk", "deutsch", "English (United States)", "English (international)", "español (España)", "Español (México)", "Filipino", "Français", "hrvatski", "italiano", "Lietuvių", "Latviešu", "magyar", "Nederlands", "norsk", "polski", "português (Brasil)", "português (Portugal)", "română", "Shqip", "suomi", "Slovenský", "Svenska", "Tiếng Việt", "Türkçe", "Ελληνικά", "Беларуска", "Български", "македонски", "русский", "עברית", "العربية", "فارسی", "हिंदी", "ไทย", "한국어", "简体中文", "繁體中文", "日本語", "српски", "Український" };
        //Neue Datenliste erstellen //ClassStyles
        ObservableCollection<ClassLanguages> datalist = new ObservableCollection<ClassLanguages>();
        //---------------------------------------------------------------------------------------------------------










        //Wird am Anfang der Seite geladen
        //---------------------------------------------------------------------------------------------------------
        public Language()
        {
            //Komponenten laden
            InitializeComponent();

            //Hintergrundfarbe prüfen
            Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            string temp = Convert.ToString(backgroundColor);

            //Icons ändern
            if (temp != "#FF000000")
            {
                //Icons ändern
                ImgTop.Source = new BitmapImage(new Uri("Images/Globe.Light.png", UriKind.Relative));
                ImgTop.Opacity = 0.1;
            }

            //Sprachen erstellen
            CreateLanguages();
        }
        //---------------------------------------------------------------------------------------------------------










        //Wird am Anfang der Seite geladen
        //---------------------------------------------------------------------------------------------------------
        void CreateLanguages()
        {
            //Prüfen wieviel Sprachen
            int cLang = LangNames.Count();

            //Sprachen durchlaufen
            datalist.Clear();
            for (int i = 0; i < cLang; i++)
            {
                datalist.Add(new ClassLanguages(LangNames[i], LangCodes[i]));
            }

            //Sprachen in Listbox Setzen
            LBLangList.ItemsSource = datalist;
        
        }
        //---------------------------------------------------------------------------------------------------------










        //Neue Sprachdatei erstellen
        //---------------------------------------------------------------------------------------------------------
        //Variabeln
        bool SelectLang = true; 
        //Aktion
        private void LocList_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            //Prüfen ob Aktion ausgefüfrt wird
            if (SelectLang == true)
            {
                //Index ermitteln
                int SI = LBLangList.SelectedIndex;

                //Code aus Array laden
                string cul = (datalist[SI] as ClassLanguages).code;
                string lang = (datalist[SI] as ClassLanguages).name;

                //Abfrage ob Sprache geändert werden soll
                if (MessageBox.Show(Lockscreen_Swap.AppResx.Z03_NoteLanguage + "\n" + lang, Lockscreen_Swap.AppResx.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    //Sprache ändern
                    CultureInfo newCulture = new CultureInfo(cul);
                    Thread.CurrentThread.CurrentUICulture = newCulture;

                    //IsoStore file erstellen
                    IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
                    //Prüfen ob alte Datei vorhanden
                    if (file.FileExists("Cul.dat"))
                    {
                        file.DeleteFile("Cul.dat");
                    }
                    //Neue Datei erstellen
                    IsolatedStorageFileStream filestream = file.CreateFile("Cul.dat");
                    StreamWriter sw = new StreamWriter(filestream);
                    sw.WriteLine(Convert.ToString(cul));
                    sw.Flush();
                    filestream.Close();

                    //Benachrichtigung ausgeben
                    MessageBox.Show(Lockscreen_Swap.AppResx.Z03_NoteLanguage2);

                    //Zurück
                    NavigationService.GoBack();
                }
            }

            //Auswahl zurücksetzen
            SelectLang = false;
            try
            {
                LBLangList.SelectedIndex = -1;
            }
            catch
            {
            }
            SelectLang = true;

        }
        //---------------------------------------------------------------------------------------------------------
    }
}