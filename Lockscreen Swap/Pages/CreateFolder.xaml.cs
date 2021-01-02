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
    public partial class CreateFolder : PhoneApplicationPage
    {





        //Wir zum Start der SEite geladen
        //-----------------------------------------------------------------------------------------------------------------
        public CreateFolder()
        {
            //Komponenten laden
            InitializeComponent();

            //Bild ändern
            Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            string temp = Convert.ToString(backgroundColor);
            if (temp != "#FF000000")
            {
                ImgTop.Source = new BitmapImage(new Uri("Images/Folder.Add.Light.png", UriKind.Relative));
                ImgTop.Opacity = 0.1;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------





        //Allgemeine Variabeln
        //-----------------------------------------------------------------------------------------------------------------
        //IsoStore file erstellen
        IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
        //-----------------------------------------------------------------------------------------------------------------





        //Prüfen ob Return gedrückt wurde
        //-----------------------------------------------------------------------------------------------------------------
        private void TBFolderName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //Prüfen ob Name vorhanden
            if (TBFolderName.Text.Length > 0)
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
                            TBFolderName.Text = "";
                        }
                        else
                        {
                            //Prüfen ob Ordner bereits besteht
                            if (!file.DirectoryExists("/Folders/" + TBFolderName.Text))
                            {

                                //Debugmode erstsellen
                                if (TBFolderName.Text == "DEBUGMODE")
                                {
                                    if (file.FileExists("DEBUGMODE.txt"))
                                    {
                                        //Debugmode löschen
                                        file.DeleteFile("DEBUGMODE.txt");
                                        MessageBox.Show("Debug mode is off");
                                        //Zurück gehen
                                        NavigationService.GoBack();
                                    }
                                    else
                                    {
                                        //Debugmode erstellen
                                        file.CreateFile("DEBUGMODE.txt");
                                        MessageBox.Show("Debug mode is on");
                                        //Zurück gehen
                                        NavigationService.GoBack();
                                    }
                                }
                                else
                                {

                                    try
                                    {
                                        //Ordnerdatei laden
                                        IsolatedStorageFileStream filestream = file.OpenFile("Folders.dat", FileMode.Open);
                                        StreamReader sr = new StreamReader(filestream);
                                        string FoldersAll = sr.ReadToEnd();
                                        filestream.Close();
                                        FoldersAll = FoldersAll.TrimEnd(new char[] { '\r', '\n' });
                                        //Ordner erstellen
                                        file.CreateDirectory("/Folders/" + TBFolderName.Text);
                                        file.CreateDirectory("/Thumbs/" + TBFolderName.Text);
                                        //Ordnerdatei ändern
                                        FoldersAll += TBFolderName.Text + "/";
                                        //Neue Ordner Datei erstellen
                                        filestream = file.CreateFile("Folders.dat");
                                        StreamWriter sw = new StreamWriter(filestream);
                                        sw.WriteLine(FoldersAll);
                                        sw.Flush();
                                        filestream.Close();
                                        //Neue Image Datei erstellen
                                        filestream = file.CreateFile("/Thumbs/" + TBFolderName.Text + ".dat");
                                        sw = new StreamWriter(filestream);
                                        sw.WriteLine("/");
                                        sw.Flush();
                                        filestream.Close();
                                        //Zurück gehen
                                        NavigationService.GoBack();
                                    }
                                    catch
                                    {
                                        MessageBox.Show(Lockscreen_Swap.AppResx.ErrorName);
                                        TBFolderName.Text = "";
                                    }

                                }
                            }
                            //Wenn Ordner bereits besteht
                            else
                            {
                                MessageBox.Show(Lockscreen_Swap.AppResx.ErrorName);
                                TBFolderName.Text = "";
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(Lockscreen_Swap.AppResx.ErrorEnterName);
                        TBFolderName.Text = "";
                    }

                    //Focus zurücksetzen
                    //Focus();
                }
            }
        }
        //---------------------------------------------------------------------------------------------------------  





        //Back Button
        //---------------------------------------------------------------------------------------------------------------------------------
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            //Speichern
            if (TBFolderName.Text.Length > 0)
            {
                //Speichern
                if (MessageBox.Show("", Lockscreen_Swap.AppResx.NewFolder + "?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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
                            TBFolderName.Text = "";
                            //Zurück oder beenden abbrechen
                            e.Cancel = true;
                        }
                        else
                        {
                            //Prüfen ob Ordner bereits besteht
                            if (!file.DirectoryExists("/Folders/" + TBFolderName.Text))
                            {
                                //Debugmode erstsellen
                                if (TBFolderName.Text == "DEBUGMODE")
                                {
                                    if (file.FileExists("DEBUGMODE.txt"))
                                    {
                                        //Debugmode löschen
                                        file.DeleteFile("DEBUGMODE.txt");
                                        MessageBox.Show("Debug mode is off");
                                        //Zurück gehen
                                        NavigationService.GoBack();
                                    }
                                    else
                                    {
                                        //Debugmode erstellen
                                        file.CreateFile("DEBUGMODE.txt");
                                        MessageBox.Show("Debug mode is on");
                                        //Zurück gehen
                                        NavigationService.GoBack();
                                    }
                                }
                                else
                                {

                                    try
                                    {
                                        //Ordnerdatei laden
                                        IsolatedStorageFileStream filestream = file.OpenFile("Folders.dat", FileMode.Open);
                                        StreamReader sr = new StreamReader(filestream);
                                        string FoldersAll = sr.ReadToEnd();
                                        filestream.Close();
                                        FoldersAll = FoldersAll.TrimEnd(new char[] { '\r', '\n' });
                                        //Ordner erstellen
                                        file.CreateDirectory("/Folders/" + TBFolderName.Text);
                                        file.CreateDirectory("/Thumbs/" + TBFolderName.Text);
                                        //Ordnerdatei ändern
                                        FoldersAll += TBFolderName.Text + "/";
                                        //Neue Ordner Datei erstellen
                                        filestream = file.CreateFile("Folders.dat");
                                        StreamWriter sw = new StreamWriter(filestream);
                                        sw.WriteLine(FoldersAll);
                                        sw.Flush();
                                        filestream.Close();
                                        //Neue Image Datei erstellen
                                        filestream = file.CreateFile("/Thumbs/" + TBFolderName.Text + ".dat");
                                        sw = new StreamWriter(filestream);
                                        sw.WriteLine("/");
                                        sw.Flush();
                                        filestream.Close();
                                        //Zurück gehen
                                        NavigationService.GoBack();
                                    }
                                    catch
                                    {
                                        MessageBox.Show(Lockscreen_Swap.AppResx.ErrorName);
                                        TBFolderName.Text = "";
                                        //Zurück oder beenden abbrechen
                                        e.Cancel = true;
                                    }
                                }

                            }
                            //Wenn Ordner bereits besteht
                            else
                            {
                                MessageBox.Show(Lockscreen_Swap.AppResx.ErrorName);
                                TBFolderName.Text = "";
                                //Zurück oder beenden abbrechen
                                e.Cancel = true;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(Lockscreen_Swap.AppResx.ErrorEnterName);
                        TBFolderName.Text = "";
                        //Zurück oder beenden abbrechen
                        e.Cancel = true;
                    }
                }
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------




    }
}