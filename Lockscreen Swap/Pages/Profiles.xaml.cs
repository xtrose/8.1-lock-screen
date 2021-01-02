using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Text.RegularExpressions;
using System.IO;
using System.ComponentModel;
using System.Windows.Media;










namespace Lockscreen_Swap.Pages
{
    public partial class Profiles : PhoneApplicationPage
    {










        //Allgemeine Variabeln
        //---------------------------------------------------------------------------------------------------------
        bool MenuOpen = false;
        IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
        ObservableCollection<ClassFolders> datalist3 = new ObservableCollection<ClassFolders>();
        //---------------------------------------------------------------------------------------------------------









        //Wird beim ersten Aufruf der Seite geladen
        //---------------------------------------------------------------------------------------------------------
        public Profiles()
        {
            //Komponenten laden
            InitializeComponent();

            //Icons ändern
            Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            string temp = Convert.ToString(backgroundColor);
            if (temp != "#FF000000")
            {
                //Icons ändern
                ImgProfile01.Source = new BitmapImage(new Uri("Images/Profile.Light.png", UriKind.Relative));
                ImgProfileMenu.Source = new BitmapImage(new Uri("Images/Profile.Light.png", UriKind.Relative));
                ImgProfileMenu.Opacity = 0.1;
                ImgProfile.Source = new BitmapImage(new Uri("Images/Profile.Light.png", UriKind.Relative));
                ImgProfile.Opacity = 0.1;
                ImgProfileSet.Source = new BitmapImage(new Uri("Images/Profile.Light.png", UriKind.Relative));
                ImgProfileEdit.Source = new BitmapImage(new Uri("Images/Edit.Light.png", UriKind.Relative));
                ImgProfileDelete.Source = new BitmapImage(new Uri("Images/Delete.Light.png", UriKind.Relative));
            }
        }
        //---------------------------------------------------------------------------------------------------------










        //Wird bei jedem Aufruf der Seite geladen
        //---------------------------------------------------------------------------------------------------------
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Menüs schließen
            GRProfileMenu.Margin = new Thickness(-600, 0, 0, 0);
            //Angeben das Menüs geschlossen sind
            MenuOpen = false;

            //Profile laden
            CreateProfiles();
        }
        //---------------------------------------------------------------------------------------------------------










        //Profile erstellen
        //*********************************************************************************************************
        void CreateProfiles()
        {
            //Alte Daten löschen
            datalist3.Clear();

            //Dateien Laden "Styles"
            string[] tempProfiles = file.GetFileNames("/Profile/");
            int tempProfiles_c = tempProfiles.Count();

            //Dateien durchlaufen und in Listbox schreiben
            for (int i = 0; i < tempProfiles_c; i++)
            {
                tempProfiles[i] = Regex.Replace(tempProfiles[i], ".txt", "");
                datalist3.Add(new ClassFolders(tempProfiles[i], "0"));
            }

            //Daten in Listbox schreiben
            LBProfiles.ItemsSource = datalist3;
        }
        //*********************************************************************************************************










        //Profil Buttons
        //*********************************************************************************************************

        //Variabeln
        int SelectedProfil = -1;

        //Button Profil erstellen
        //---------------------------------------------------------------------------------------------------------
        private void BtnCreateProfil(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Create Profil öffnen
            NavigationService.Navigate(new Uri("/Pages/CreateProfil.xaml", UriKind.Relative));
        }
        //---------------------------------------------------------------------------------------------------------


        //Menü öffnen
        //---------------------------------------------------------------------------------------------------------
        private void BtnOpenProfilMenu(object sender, SelectionChangedEventArgs e)
        {
            //Prüfen ob Menü bereits offen
            if (MenuOpen == false)
            {
                //Index auswählen
                SelectedProfil = Convert.ToInt32(LBProfiles.SelectedIndex);
                TBProfilMenuName.Text = (datalist3[SelectedProfil] as ClassFolders).name;

                //Menü öffnen
                GRProfileMenu.Margin = new Thickness(0, 0, 0, 0);
                //Angeben das ein Menü offen ist
                MenuOpen = true;

                //Auswahl aufheben
                try
                {
                    LBProfiles.SelectedIndex = -1;
                }
                catch
                {
                }
            }
        }
        //---------------------------------------------------------------------------------------------------------


        //Profil anwenden
        //---------------------------------------------------------------------------------------------------------
        private void BtnSetProfil(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Warnung ausgeben
            if (MessageBox.Show(Lockscreen_Swap.AppResx.Z02_WarningSetProfil, Lockscreen_Swap.AppResx.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //Settings kopieren
                file.DeleteFile("/Settings/Settings.txt");
                file.CopyFile("/Profile/" + TBProfilMenuName.Text + ".txt", "/Settings/Settings.txt");

                //Create new erstellen
                if (!file.FileExists("CreateNew.txt"))
                {
                    IsolatedStorageFileStream filestream = file.CreateFile("CreateNew.txt");
                    StreamWriter sw = new StreamWriter(filestream);
                    sw.WriteLine(Convert.ToString("1"));
                    sw.Flush();
                    filestream.Close();
                }

                //Zurück zur Startseite
                NavigationService.GoBack();
            }
        }
        //---------------------------------------------------------------------------------------------------------


        //Profil bearbeiten
        //---------------------------------------------------------------------------------------------------------
        private void BtnRenameProfile(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Rename Profile öffnen
            NavigationService.Navigate(new Uri("/Pages/RenameProfil.xaml?profile=" + SelectedProfil, UriKind.Relative));
        }
        //---------------------------------------------------------------------------------------------------------


        //Profil löschen
        //---------------------------------------------------------------------------------------------------------
        private void BtnDeleteProfile(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Warnung ausgeben ausgeben
            if (MessageBox.Show(Lockscreen_Swap.AppResx.Z02_DELETE + " " + TBProfilMenuName.Text + "?", Lockscreen_Swap.AppResx.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                try
                {
                    file.DeleteFile("Profile/" + TBProfilMenuName.Text + ".txt");
                }
                catch
                {
                }

                //Folders neu laden
                CreateProfiles();
                //Menü schließen
                GRProfileMenu.Margin = new Thickness(-600, 0, 0, 0);
                MenuOpen = false;
            }
        }
        //---------------------------------------------------------------------------------------------------------
        //*********************************************************************************************************










        //Back Button
        //---------------------------------------------------------------------------------------------------------------------------------
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            //Prüfen ob Menü offen ist und alle Menüs schließen
            if (MenuOpen == true)
            {
                //Menüs schließen
                GRProfileMenu.Margin = new Thickness(-600, 0, 0, 0);
                //Angeben das Menüs geschlossen sind
                MenuOpen = false;

                //Zurück oder beenden abbrechen
                e.Cancel = true;
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------









    }
}