using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
//Zum laden von Qellcodes und zum erstellen und auslesem von Dateien
using System.IO;
//Zum erstellen und auslesem von Dateien
using System.IO.IsolatedStorage;
//Zum erweiterten schneiden von strings
using System.Text.RegularExpressions;
//Zum speichern von Bildern in den Isolated Storage
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using Microsoft.Xna.Framework.Media;
using Microsoft.Phone.Tasks;
using Microsoft.Phone;
using System.Windows.Media;
//Um Battery Leistung auszulesen
using Windows.Phone.Devices.Power;
//Um auszulesen ob Handy geladen wird;
using Microsoft.Phone.Info;
//Für Listbox Update
using System.Collections.ObjectModel;
//Background Agent
using Microsoft.Phone.Scheduler;
//Für Benachrichtigung vor beenden
using System.ComponentModel;






namespace Lockscreen_Swap.Pages
{
    public partial class CopyFolder : PhoneApplicationPage
    {





        // Wird zum Start der Seite geladen
        //-----------------------------------------------------------------------------------------------------------------
        public CopyFolder()
        {
            //Komponenten laden
            InitializeComponent();

            //Bild ändern
            Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            string temp = Convert.ToString(backgroundColor);
            if (temp != "#FF000000")
            {
                ImgTop.Source = new BitmapImage(new Uri("Images/Copy.Light.png", UriKind.Relative));
                ImgTop.Opacity = 0.1;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------





        //Allgemeine Variabeln
        //-----------------------------------------------------------------------------------------------------------------
        //IsoStore file erstellen
        IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
        //Style Name
        string FolderName;
        //-----------------------------------------------------------------------------------------------------------------





        // Wird bei jedem Aufruf der Seite geladen
        //-----------------------------------------------------------------------------------------------------------------
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Variable für Ordner ermitteln
            string[] AllFolders = file.GetDirectoryNames("/Folders/");
            int FolderID = Convert.ToInt32(NavigationContext.QueryString["folder"]);
            FolderName = AllFolders[FolderID];
            base.OnNavigatedTo(e);

            //Style Name
            TBFolderName.Text = FolderName;
        }
        //-----------------------------------------------------------------------------------------------------------------





        //Prüfen ob Return gedrückt wurde
        //-----------------------------------------------------------------------------------------------------------------
        private void TBFolderName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //Prüfen ob Return gedrückt wurde
            string tempkey = Convert.ToString(e.Key);
            if (tempkey == "Enter")
            {
                //";" Zeichen herauslöschen
                TBFolderName.Text = Regex.Replace(TBFolderName.Text, ";", "");
                //Prüfen ob noch Zeichen vorhanden
                if (TBFolderName.Text.Length > 0)
                {
                    //Prüfen ob leere Eingabe und zurücksetzen
                    bool temp = Regex.IsMatch(TBFolderName.Text, @"^[a-zA-Z0-9 ]+$");
                    temp = true;
                    if (temp == false)
                    {
                        MessageBox.Show(Lockscreen_Swap.AppResx.ErrorName);
                        TBFolderName.Text = FolderName;
                    }
                    else
                    {
                        //Prüfen ob Ordner bereits besteht
                        if (!file.DirectoryExists("/Folders/" + TBFolderName.Text))
                        {
                            try
                            {
                                //Ordnerdatei laden
                                IsolatedStorageFileStream filestream = file.OpenFile("Folders.dat", FileMode.Open);
                                StreamReader sr = new StreamReader(filestream);
                                string FoldersAll = sr.ReadToEnd();
                                filestream.Close();
                                FoldersAll = FoldersAll.TrimEnd(new char[] { '\r', '\n' });
                                //Ordner kopieren
                                file.CreateDirectory("/Folders/" + TBFolderName.Text);
                                string[] files = file.GetFileNames("/Folders/" + FolderName + "/");
                                foreach (string file2 in files)
                                {
                                    file.CopyFile("/Folders/" + FolderName + "/" + file2, "Folders/" + TBFolderName.Text + "/" + file2);
                                }
                                file.CreateDirectory("/Thumbs/" + TBFolderName.Text);
                                string[] files2 = file.GetFileNames("/Thumbs/" + FolderName + "/");
                                foreach (string file2 in files2)
                                {
                                    file.CopyFile("/Thumbs/" + FolderName + "/" + file2, "Thumbs/" + TBFolderName.Text + "/" + file2);
                                }
                                //Imagedatei kopieren
                                file.CopyFile("Thumbs/" + FolderName + ".dat", "Thumbs/" + TBFolderName.Text + ".dat");
                                //Ordnerdatei ändern
                                FoldersAll += TBFolderName.Text + "/";
                                //Neue Ordner Datei erstellen
                                filestream = file.CreateFile("Folders.dat");
                                StreamWriter sw = new StreamWriter(filestream);
                                sw.WriteLine(FoldersAll);
                                sw.Flush();
                                filestream.Close();
                                //Navigation zurück
                                NavigationService.GoBack();
                            }
                            catch
                            {
                                MessageBox.Show(Lockscreen_Swap.AppResx.ErrorName);
                                TBFolderName.Text = FolderName;
                            }
                        }
                        //Wenn Ordner bereits besteht
                        else
                        {
                            MessageBox.Show(Lockscreen_Swap.AppResx.ErrorName);
                            TBFolderName.Text = FolderName;
                        }

                    }
                }
                else
                {
                    MessageBox.Show(Lockscreen_Swap.AppResx.ErrorEnterName);
                    TBFolderName.Text = FolderName;
                }

                //Focus zurücksetzen
                //Focus();
            }
        }
        //---------------------------------------------------------------------------------------------------------  





        //Back Button
        //---------------------------------------------------------------------------------------------------------------------------------
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            //Prüfen ob Name anders
            if (FolderName != TBFolderName.Text)
            {
                //Speichern
                if (MessageBox.Show("", Lockscreen_Swap.AppResx.CopyFolder + "?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    //";" Zeichen herauslöschen
                    TBFolderName.Text = Regex.Replace(TBFolderName.Text, ";", "");
                    //Prüfen ob noch Zeichen vorhanden
                    if (TBFolderName.Text.Length > 0)
                    {
                        //Prüfen ob leere Eingabe und zurücksetzen
                        bool temp = Regex.IsMatch(TBFolderName.Text, @"^[a-zA-Z0-9 ]+$");
                        temp = true;
                        if (temp == false)
                        {
                            MessageBox.Show(Lockscreen_Swap.AppResx.ErrorName);
                            TBFolderName.Text = FolderName;
                            //Zurück oder beenden abbrechen
                            e.Cancel = true;
                        }
                        else
                        {
                            //Prüfen ob Ordner bereits besteht
                            if (!file.DirectoryExists("/Folders/" + TBFolderName.Text))
                            {
                                try
                                {
                                    //Ordnerdatei laden
                                    IsolatedStorageFileStream filestream = file.OpenFile("Folders.dat", FileMode.Open);
                                    StreamReader sr = new StreamReader(filestream);
                                    string FoldersAll = sr.ReadToEnd();
                                    filestream.Close();
                                    FoldersAll = FoldersAll.TrimEnd(new char[] { '\r', '\n' });
                                    //Ordner kopieren
                                    file.CreateDirectory("/Folders/" + TBFolderName.Text);
                                    string[] files = file.GetFileNames("/Folders/" + FolderName + "/");
                                    foreach (string file2 in files)
                                    {
                                        file.CopyFile("/Folders/" + FolderName + "/" + file2, "Folders/" + TBFolderName.Text + "/" + file2);
                                    }
                                    file.CreateDirectory("/Thumbs/" + TBFolderName.Text);
                                    string[] files2 = file.GetFileNames("/Thumbs/" + FolderName + "/");
                                    foreach (string file2 in files2)
                                    {
                                        file.CopyFile("/Thumbs/" + FolderName + "/" + file2, "Thumbs/" + TBFolderName.Text + "/" + file2);
                                    }
                                    //Imagedatei kopieren
                                    file.CopyFile("Thumbs/" + FolderName + ".dat", "Thumbs/" + TBFolderName.Text + ".dat");
                                    //Ordnerdatei ändern
                                    FoldersAll += TBFolderName.Text + "/";
                                    //Neue Ordner Datei erstellen
                                    filestream = file.CreateFile("Folders.dat");
                                    StreamWriter sw = new StreamWriter(filestream);
                                    sw.WriteLine(FoldersAll);
                                    sw.Flush();
                                    filestream.Close();
                                    //Navigation zurück
                                    NavigationService.GoBack();
                                }
                                catch
                                {
                                    MessageBox.Show(Lockscreen_Swap.AppResx.ErrorName);
                                    TBFolderName.Text = FolderName;
                                    //Zurück oder beenden abbrechen
                                    e.Cancel = true;
                                }
                            }
                            //Wenn Ordner bereits besteht
                            else
                            {
                                MessageBox.Show(Lockscreen_Swap.AppResx.ErrorName);
                                TBFolderName.Text = FolderName;
                                //Zurück oder beenden abbrechen
                                e.Cancel = true;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(Lockscreen_Swap.AppResx.ErrorEnterName);
                        TBFolderName.Text = FolderName;
                        //Zurück oder beenden abbrechen
                        e.Cancel = true;
                    }
                }
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------




    }
}