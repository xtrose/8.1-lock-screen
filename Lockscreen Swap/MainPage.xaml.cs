using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Lockscreen_Swap.Resources;
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
using System.Windows.Threading;
using Windows.Phone.System.UserProfile;
using System.Globalization;
using System.Threading;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using ImageTools;  
using ImageTools.IO;  
using ImageTools.IO.Png;  







namespace Lockscreen_Swap
{
    public partial class MainPage : PhoneApplicationPage
    {










        // Für ScheduledTaskAgent
        //-------------------------------------------------------------------------------------------------------------------------------------------
        //ScheduledTaskAgent Variabeln
        //*********************************************************************************************************
        PeriodicTask periodicTask;
        string periodicTaskName = "PeriodicAgent";
        public bool agentsAreEnabled = true;
        //*********************************************************************************************************



        //ScheduledTaskAgent Starten
        //*********************************************************************************************************
        private void StartPeriodicAgent()
        {
            //Variable ob Task aktiv ist
            agentsAreEnabled = true;

            //Prüfen ob Task aktiv ist
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;

            //Wenn Task aktiv ist, dann Task stoppen um einen neuen zu starten
            if (periodicTask != null)
            {
                RemoveAgent(periodicTaskName);
            }

            //Neuen Task erstellen
            periodicTask = new PeriodicTask(periodicTaskName);

            //Beschreibung des Tasks, wird in der Hintergundaufgaben bei den Einstellungen angezeigt
            periodicTask.Description = Lockscreen_Swap.AppResx.TaskInfo;

            //Versuchen Task zu starten
            try
            {
                //Task hinzufügen
                ScheduledActionService.Add(periodicTask);
                //ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(30));

                //Prüfen ob LockscreenApp und Anzeige auf on Stellen
                if (IsLockscreenApp == true)
                {
                    TBLockscreenChanger.Content = Lockscreen_Swap.AppResx.On;
                }
            }

            //Benachrichtigungen ausgeben, falls Task nicht gestartet werden kann
            catch (InvalidOperationException exception)
            {
                //Wenn Task nicht aktiv
                if (exception.Message.Contains("Error: The action is disabled"))
                {
                    //Benachrichtigung ausgeben
                    MessageBox.Show(Lockscreen_Swap.AppResx.ExError);
                }
                //Wenn maximum der Anwendungen erreicht
                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    //Keine Aktion erförderlich, da eine Systemnachricht ausgegeben wird
                }

                //Variable ob Task aktiv ist
                agentsAreEnabled = false;

                //Button des Tasks deaktivieren
                TBLockscreenChanger.Content = Lockscreen_Swap.AppResx.Off;
            }

            //Bei Fehler im Scheduler Service
            catch (SchedulerServiceException)
            {
                //Variable ob Task aktiv ist
                agentsAreEnabled = false;

                //Button des Tasks deaktivieren
                TBLockscreenChanger.Content = Lockscreen_Swap.AppResx.Off;
            }

        }
        //*********************************************************************************************************



        //ScheduledTaskAgent entfernen
        //*********************************************************************************************************
        private void RemoveAgent(string name)
        {
            try
            {
                //Agent erntfernen/Background/Background.jpg
                ScheduledActionService.Remove(name);

                //Lockscreenchanger auf Off stellen
                TBLockscreenChanger.Content = Lockscreen_Swap.AppResx.Off;
            }
            catch (Exception)
            {
            }
        }
        //*********************************************************************************************************



        //Lockhelper Button on (Button Lockscreen TSLockscreen in den Settings)
        //*********************************************************************************************************
        private void BtnLockscreen()
        {
            //Wenn noch keine Lockscreen App
            if (!Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication)
            {
                //Lockhelper aktivieren
                string filePathOfTheImage = "/Background/Background.jpg";
                bool isAppResource = true;
                LockHelper(filePathOfTheImage, isAppResource);

                //ScheduledTaskAgent Agent starten
                StartPeriodicAgent();

                //Lockscreen auf on stellen
                IsLockscreenApp = true;

                //Lockscreen Bild wechseln
                CreateImage();
            }

            //Wenn bereits lockscreen App
            else
            {
                //ScheduledTaskAgent Agent starten
                StartPeriodicAgent();

                //Lockscreen Bild wechseln
                CreateImage();
            }

            //Prüfen ob ScheduledTaskAgent läuft
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;

            //Prüfen ob momentan Lockscreen App
            if (Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication)
            {
                //IsLockscreenApp auf true stellen
                IsLockscreenApp = true;
            }

            //Prüfen ob Task läuft und App Lockscreen App ist
            if (periodicTask != null & IsLockscreenApp == true)
            {
                //agentsAreEnabled auf true
                agentsAreEnabled = true;

                //Button umstellen
                TBLockscreenChanger.Content = Lockscreen_Swap.AppResx.On;

                //Lockscreen Bild wechseln
                CreateImage();
            }

            //Wenn nicht, Button auf false stellen
            else
            {
                //agentsAreEnabled auf false
                agentsAreEnabled = false;

                //Button umstellen
                TBLockscreenChanger.Content = Lockscreen_Swap.AppResx.Off;
            }
        }
        //*********************************************************************************************************



        //Lockhelper ausführen
        //*********************************************************************************************************
        private async void LockHelper(string filePathOfTheImage, bool isAppResource)
        {
            try
            {
                var isProvider = Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication;
                if (!isProvider)
                {
                    // If you're not the provider, this call will prompt the user for permission.
                    // Calling RequestAccessAsync from a background agent is not allowed.
                    var op = await Windows.Phone.System.UserProfile.LockScreenManager.RequestAccessAsync();

                    // Only do further work if the access was granted.
                    isProvider = op == Windows.Phone.System.UserProfile.LockScreenRequestResult.Granted;
                }

                if (isProvider)
                {
                    // At this stage, the app is the active lock screen background provider.

                    // The following code example shows the new URI schema.
                    // ms-appdata points to the root of the local app data folder.
                    // ms-appx points to the Local app install folder, to reference resources bundled in the XAP package.
                    var schema = isAppResource ? "ms-appx:///" : "ms-appdata:///Local/";
                    var uri = new Uri(schema + filePathOfTheImage, UriKind.Absolute);

                    // Set the lock screen background image.
                    Windows.Phone.System.UserProfile.LockScreen.SetImageUri(uri);

                    // Get the URI of the lock screen background image.
                    var currentImage = Windows.Phone.System.UserProfile.LockScreen.GetImageUri();
                    System.Diagnostics.Debug.WriteLine("The new lock screen background image is set to {0}", currentImage.ToString());

                    //ScheduledTaskAgent Agent starten
                    StartPeriodicAgent();
                }
                else
                {
                    MessageBox.Show(Lockscreen_Swap.AppResx.LockhelperInfo);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        //*********************************************************************************************************










        //Start Animation
        //---------------------------------------------------------------------------------------------------------------------------------
        int UhrzeitMS;
        int StartMs;
        string Animation = "PauseStart";
        void dt_Tick(object sender, object e)
        {
            //uhrzeitms neu erstellen
            DateTime Uhrzeit = DateTime.Now;
            //Aktuelle Uhrzeit Millisekunden erstellen
            UhrzeitMS = (Uhrzeit.Hour * 3600000) + (Uhrzeit.Minute * 60000) + (Uhrzeit.Second * 1000) + Uhrzeit.Millisecond;

            //Animation Pause1
            if (Animation == "PauseStart")
            {
                //Prüfen ob Animation schon gestartet
                if (StartMs == 0)
                {
                    StartMs = UhrzeitMS;
                }
                //Wenn Animation schon gestartet
                else
                {
                    //Wenn Animation beendet
                    if ((UhrzeitMS - StartMs) > 300)
                    {
                        //Nächste Animation Starten
                        Animation = "Img1";
                        StartMs = 0;
                    }
                }
            }

            //Animation Img1
            if (Animation == "Img1")
            {
                //Prüfen ob Animation schon gestartet
                if (StartMs == 0)
                {
                    StartMs = UhrzeitMS;
                }
                //Wenn Animation schon gestartet
                else
                {
                    //Wenn Animation beendet
                    if ((UhrzeitMS - StartMs) > 400)
                    {
                        //Bild endgültig ausrichten
                        Img1.Width = 221;
                        Img1.Opacity = 1.0;
                        //Nächste Animation Starten
                        Animation = "Pause1";
                        StartMs = 0;
                    }
                    //Wenn Animation noch läuft
                    else
                    {
                        //Prozent errechnen
                        int Prozent = 100 * 100000 / 400 * (UhrzeitMS - StartMs) / 100000;
                        //Bild Größe berechnen
                        int NewSize = 1800 * 100000 / 100 * Prozent / 100000;
                        //Bild Transparenz berechnen
                        double opa = 0.1;
                        //string temp = Convert.ToString(Prozent);
                        //opa = Convert.ToDouble("0," + temp);
                        if (Prozent >= 100)
                        {
                            opa = 1.0;
                        }
                        else if (Prozent >= 90)
                        {
                            opa = 0.9;
                        }
                        else if (Prozent >= 80)
                        {
                            opa = 0.8;
                        }
                        else if (Prozent >= 70)
                        {
                            opa = 0.7;
                        }
                        else if (Prozent >= 60)
                        {
                            opa = 0.6;
                        }
                        else if (Prozent >= 50)
                        {
                            opa = 0.5;
                        }
                        else if (Prozent >= 40)
                        {
                            opa = 0.4;
                        }
                        else if (Prozent >= 30)
                        {
                            opa = 0.3;
                        }
                        else if (Prozent >= 20)
                        {
                            opa = 0.2;
                        }
                        else if (Prozent >= 10)
                        {
                            opa = 0.1;
                        }
                        //Bild neu erstellen
                        Img1.Width = 2021 - NewSize;
                        Img1.Opacity = opa;
                    }
                }
            }

            //Animation Pause1
            if (Animation == "Pause1")
            {
                //Prüfen ob Animation schon gestartet
                if (StartMs == 0)
                {
                    StartMs = UhrzeitMS;
                }
                //Wenn Animation schon gestartet
                else
                {
                    //Wenn Animation beendet
                    if ((UhrzeitMS - StartMs) > 200)
                    {
                        //Nächste Animation Starten
                        Animation = "Schein";
                        StartMs = 0;
                    }
                }
            }

            //Animation Img1
            if (Animation == "Schein")
            {
                //Prüfen ob Animation schon gestartet
                if (StartMs == 0)
                {
                    //Bilder sichtbar machen
                    ImgSchein.Visibility = System.Windows.Visibility.Visible;
                    Img2.Visibility = System.Windows.Visibility.Visible;
                    //Zeit neu erstellen
                    StartMs = UhrzeitMS;
                }
                //Wenn Animation schon gestartet
                else
                {
                    //Wenn Animation beendet
                    if ((UhrzeitMS - StartMs) > 400)
                    {
                        //Bild unsichtbar machen
                        ImgSchein.Visibility = System.Windows.Visibility.Collapsed;
                        //Animation umstellen
                        Animation = "Pause2";
                    }
                    //Wenn Animation noch läuft
                    else
                    {
                        //Prozent errechnen
                        int Prozent = 100 * 100000 / 400 * (UhrzeitMS - StartMs) / 100000;
                        //Margin neu berechnen
                        int NewMargin = 2000 * 100000 / 100 * Prozent / 100000;
                        ImgSchein.Margin = new Thickness((NewMargin - 150), 159, 0, 159);
                    }
                }
            }

            //Animation Pause2
            if (Animation == "Pause2")
            {
                //Prüfen ob Animation schon gestartet
                if (StartMs == 0)
                {
                    StartMs = UhrzeitMS;
                }
                //Wenn Animation schon gestartet
                else
                {
                    //Wenn Animation beendet
                    if ((UhrzeitMS - StartMs) > 800)
                    {
                        //Nächste Animation Starten
                        Animation = "Ausblenden";
                        StartMs = 0;
                    }
                }
            }

            //Animation Img1
            if (Animation == "Ausblenden")
            {
                //Prüfen ob Animation schon gestartet
                if (StartMs == 0)
                {
                    //Bilder sichtbar machen
                    ImgSchein.Visibility = System.Windows.Visibility.Visible;
                    Img2.Visibility = System.Windows.Visibility.Visible;
                    //Zeit neu erstellen
                    StartMs = UhrzeitMS;
                }
                //Wenn Animation schon gestartet
                else
                {
                    //Wenn Animation beendet
                    if ((UhrzeitMS - StartMs) > 400)
                    {
                        //Bilder entfernen
                        Img1.Visibility = System.Windows.Visibility.Collapsed;
                        Img2.Visibility = System.Windows.Visibility.Collapsed;
                        //Animation umstellen
                        Animation = "PauseEnde";
                    }
                    //Wenn Animation noch läuft
                    else
                    {
                        //Prozent errechnen
                        int Prozent = 100 * 100000 / 400 * (UhrzeitMS - StartMs) / 100000;
                        Prozent = 100 - Prozent;
                        //Bild Transparenz berechnen
                        double opa = 0.0;
                        if (Prozent >= 100)
                        {
                            opa = 1.0;
                        }
                        else if (Prozent >= 90)
                        {
                            opa = 0.9;
                        }
                        else if (Prozent >= 80)
                        {
                            opa = 0.8;
                        }
                        else if (Prozent >= 70)
                        {
                            opa = 0.7;
                        }
                        else if (Prozent >= 60)
                        {
                            opa = 0.6;
                        }
                        else if (Prozent >= 50)
                        {
                            opa = 0.5;
                        }
                        else if (Prozent >= 40)
                        {
                            opa = 0.4;
                        }
                        else if (Prozent >= 30)
                        {
                            opa = 0.3;
                        }
                        else if (Prozent >= 20)
                        {
                            opa = 0.2;
                        }
                        else if (Prozent >= 10)
                        {
                            opa = 0.1;
                        }
                        //Transparenz auf Bilder anwenden
                        Img1.Opacity = opa;
                        Img2.Opacity = opa;
                    }
                }
            }

            //Animation PauseEnde
            if (Animation == "PauseEnde")
            {
                //Prüfen ob Animation schon gestartet
                if (StartMs == 0)
                {
                    StartMs = UhrzeitMS;
                }
                //Wenn Animation schon gestartet
                else
                {
                    //Wenn Animation beendet
                    if ((UhrzeitMS - StartMs) > 300)
                    {
                        //Seite wechseln
                        dtt.Stop();
                        GRAnimation.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
        }


        //Animation abbrechen
        private void AnimationStop(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Seite wechseln
            dtt.Stop();
            GRAnimation.Visibility = System.Windows.Visibility.Collapsed;
        }
        //---------------------------------------------------------------------------------------------------------------------------------










        // Allgemeine Variabeln
        //---------------------------------------------------------------------------------------------------------
        //Variabeln
        int Count = 0;
        bool FullVersion = false;
        int Version = 0;
        bool Run = true; //Gibt an ob Bild im Trial erstellt wird
        bool TrialMSG = false; //Gibt an ob Trial Message ausgegeben wurde
        int ImageCount = 0;
        string Settings;
        string SetFolder = "*";
        string SetBgColor = "#FF000000"; //BC (Handy Hintergrund), AC (Akzent Farbe), Farbcode
        string SetFrameColor = "NO"; //NO (kein Hintergrund), BC (Handy Hintergrund), AC (Akzent Farbe), Farbcode
        string SetAlphaColor = "NO"; //NO (kein Hintergrund), BC (Handy Hintergrund), AC (Akzent Farbe), Farbcode
        int SetFrameSize = 0;
        int SetInfoAlpha = 0;
        bool SetCycleTile = true;
        bool SetLogoStart = true;
        int SwapImage = 1;
        string InfoSize = "4";
        bool InfoTop = false;
        //IsoStore file erstellen
        IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
        IsolatedStorageFileStream filestream;
        StreamReader sr;
        StreamWriter sw;
        //Neue Datenliste erstellen //ClassStyles
        ObservableCollection<ClassFolders> datalist = new ObservableCollection<ClassFolders>();
        ObservableCollection<ClassFolders> datalist2 = new ObservableCollection<ClassFolders>();
        //Prüfen ob Lockscreenapp
        bool IsLockscreenApp = false;
        //ColorPicker Farbe
        SolidColorBrush CPColorBrush = new SolidColorBrush();
        Color CPColor = new Color();
        SolidColorBrush CP2ColorBrush = new SolidColorBrush();
        Color CP2Color = new Color();
        SolidColorBrush CP3ColorBrush = new SolidColorBrush();
        Color CP3Color = new Color();
        //BackgroundTask
        bool IsSetBGTask = false;
        //Startzeit
        DateTime dt = DateTime.Now;
        //Aktuelle Zeit
        DateTime dt_Now = DateTime.Now;
        //Neues Dateiensystem
        string FoldersAll = "/";
        string[] Folders;
        string ImagesAll = "/";
        string[] Images;
        //Daten auf Startseite
        int CFolders = 0;
        int CPictures = 0;
        //Eingestellte Sprache
        string cul;
        //Prüfen ob CycleTile schon erstellt wurde
        ShellTile oTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("flip".ToString()));
        //---------------------------------------------------------------------------------------------------------





        //Timer erstellen
        //---------------------------------------------------------------------------------------------------------------------------------
        DispatcherTimer dtt = new DispatcherTimer();
        //---------------------------------------------------------------------------------------------------------------------------------





        // Wird am Anfang der Seite geladen
        //---------------------------------------------------------------------------------------------------------
        public MainPage()
        {
            //Prüfen ob eine Sprachdatei besteht
            if (file.FileExists("Cul.dat"))
            {
                //Spachdatei laden
                filestream = file.OpenFile("Cul.dat", FileMode.Open);
                sr = new StreamReader(filestream);
                cul = sr.ReadToEnd();
                cul = cul.TrimEnd(new char[] { '\r', '\n' });
                filestream.Close();
                //Sprache einstellen
                CultureInfo newCulture = new CultureInfo(cul);
                Thread.CurrentThread.CurrentUICulture = newCulture;

            }

            //Komponenten laden
            InitializeComponent();
            
            //Animation vorbereiten
            Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            string temp = Convert.ToString(backgroundColor);
            if (temp == "#FF000000")
            {
                ImgSchein.Source = new BitmapImage(new Uri("Images/StartUp/Schein_black.png", UriKind.Relative));
                Img2.Source = new BitmapImage(new Uri("Images/StartUp/Logo800_2_black.png", UriKind.Relative));
                StartUpInfo.Foreground = new SolidColorBrush(Colors.White);
                Greetings.Foreground = new SolidColorBrush(Colors.White);
            }

            //Animation sichtbar machen
            GRAnimation.Visibility = System.Windows.Visibility.Visible;

            //Timer erstellen
            dtt.Stop();
            dtt.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dtt.Tick += dt_Tick;
            dtt.Start();

            //Icons ändern
            if (temp != "#FF000000")
            {
                //Icons ändern
                ImgPictures01.Source = new BitmapImage(new Uri("Images/Folder.Add.Light.png", UriKind.Relative));
                ImgProfile01.Source = new BitmapImage(new Uri("Images/Profile.Light.png", UriKind.Relative));
                ImgFolderOpen.Source = new BitmapImage(new Uri("Images/Folder.Light.png", UriKind.Relative));
                ImgFolderCopy.Source = new BitmapImage(new Uri("Images/Copy.Light.png", UriKind.Relative));
                ImgFolderEdit.Source = new BitmapImage(new Uri("Images/Edit.Light.png", UriKind.Relative));
                ImgFolderDelete.Source = new BitmapImage(new Uri("Images/Delete.Light.png", UriKind.Relative));
                ImgLogo.Source = new BitmapImage(new Uri("Images/Logo.Light.png", UriKind.Relative));
                ImgLogo.Opacity = 0.1;
                LogoRate.Source = new BitmapImage(new Uri("Images/Logo.Light.png", UriKind.Relative));
                LogoRate.Opacity = 0.1;
                ImgFolderMenu.Source = new BitmapImage(new Uri("Images/Folder.Light.png", UriKind.Relative));
                ImgFolderMenu.Opacity = 0.1;
                ImgFolderMenu2.Source = new BitmapImage(new Uri("Images/Folder.Light.png", UriKind.Relative));
                ImgFolderMenu2.Opacity = 0.1;
                ImgCreateNew.Source = new BitmapImage(new Uri("Images/Logo.Light.png", UriKind.Relative));
                ImgGlobe.Source = new BitmapImage(new Uri("Images/Globe.Light.png", UriKind.Relative));
                ImgInstructions.Source = new BitmapImage(new Uri("Images/Instruction.Light.png", UriKind.Relative));
                ImgSupport.Source = new BitmapImage(new Uri("Images/Support.Light.png", UriKind.Relative));
                ImgShare.Source = new BitmapImage(new Uri("Images/Share.Light.png", UriKind.Relative));
                ImgSet1.Source = new BitmapImage(new Uri("Images/Settings.Light.png", UriKind.Relative));
                ImgSet1.Opacity = 0.08;
                ImgSet2.Source = new BitmapImage(new Uri("Images/Profil.Light.png", UriKind.Relative));
                ImgSet2.Opacity = 0.08;
                ImgSet3.Source = new BitmapImage(new Uri("Images/Settings.Light.png", UriKind.Relative));
                ImgSet3.Opacity = 0.08;
            }
        }
        //---------------------------------------------------------------------------------------------------------










        // Wird bei jedem Aufruf der Seite geladen
        //---------------------------------------------------------------------------------------------------------
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Variabeln erstellen
            string temp = "";

            //Wenn noch keine Einstellungen vorhanden
            if (!file.DirectoryExists("Settings"))
            {
                //Ordner erstellen
                file.CreateDirectory("Settings");
                file.CreateDirectory("Folders");
                file.CreateDirectory("Thumbs");
                file.CreateDirectory("Styles");

                //Startzeit erstellen //Anzahl erstellter Wallpapers
                filestream = file.CreateFile("Settings/FirstTime.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine(Convert.ToString(dt));
                sw.Flush();
                filestream.Close();

                //Image Count erstellen //Anzahl erstellter Bilder
                filestream = file.CreateFile("Settings/ImageCount.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine("0");
                sw.Flush();
                filestream.Close();

                //FullVersion erstellen
                filestream = file.CreateFile("Settings/FullVersion.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine("0");
                sw.Flush();
                filestream.Close();

                //Settings erstellen
                CreateSettings();


                //Styles in Storage laden
                int tempCStyles = 4;
                for (int i = 1; i <= tempCStyles; i++)
                {
                    using (Stream input = Application.GetResourceStream(new Uri("Styles/" + i + ".txt", UriKind.Relative)).Stream)
                    {
                        // Create a stream for the new file in the local folder.
                        using (filestream = file.CreateFile("Styles/" + i + ".txt"))
                        {
                            // Initialize the buffer.
                            byte[] readBuffer = new byte[4096];
                            int bytesRead = -1;

                            // Copy the file from the installation folder to the local folder. 
                            while ((bytesRead = input.Read(readBuffer, 0, readBuffer.Length)) > 0)
                            {
                                filestream.Write(readBuffer, 0, bytesRead);
                            }
                        }
                    }
                }

                //NoImage.jpg in Storage laden
                using (Stream input = Application.GetResourceStream(new Uri("NoImage.jpg", UriKind.Relative)).Stream)
                {
                    // Create a stream for the new file in the local folder.
                    using (filestream = file.CreateFile("NoImage.jpg"))
                    {
                        // Initialize the buffer.
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the file from the installation folder to the local folder. 
                        while ((bytesRead = input.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            filestream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }
            }

            //Wenn Einstellungen bereits vorhanden
            else
            {
                //Count laden //Anzahl gewechselter Wallpapers
                filestream = file.OpenFile("Settings/FirstTime.txt", FileMode.Open);
                sr = new StreamReader(filestream);
                temp = sr.ReadToEnd();
                filestream.Close();
                dt = Convert.ToDateTime(temp);

                //ImageCount laden //Anzahl erstellter Bilder
                filestream = file.OpenFile("Settings/ImageCount.txt", FileMode.Open);
                sr = new StreamReader(filestream);
                temp = sr.ReadToEnd();
                filestream.Close();
                ImageCount = Convert.ToInt32(temp);

                //FullVersion laden
                filestream = file.OpenFile("Settings/FullVersion.txt", FileMode.Open);
                sr = new StreamReader(filestream);
                temp = sr.ReadToEnd();
                filestream.Close();
                int temp2 = Convert.ToInt32(temp);
                if (temp2 == 1)
                {
                    FullVersion = true;
                }

                //Settings laden //Einstellungen
                filestream = file.OpenFile("Settings/Settings.txt", FileMode.Open);
                sr = new StreamReader(filestream);
                Settings = sr.ReadToEnd();
                filestream.Close();
            }



            //Auf Version prüfen und updaten
            //********************************************** Update auf 1.0.0.1 **********************************************
            if (!file.DirectoryExists("Version"))
            {
                //Neue Style Dateien hinzufügen
                for (int i = 5; i <= 6; i++)
                {
                    using (Stream input = Application.GetResourceStream(new Uri("Styles/" + i + ".txt", UriKind.Relative)).Stream)
                    {
                        // Create a stream for the new file in the local folder.
                        using (filestream = file.CreateFile("Styles/" + i + ".txt"))
                        {
                            // Initialize the buffer.
                            byte[] readBuffer = new byte[4096];
                            int bytesRead = -1;

                            // Copy the file from the installation folder to the local folder. 
                            while ((bytesRead = input.Read(readBuffer, 0, readBuffer.Length)) > 0)
                            {
                                filestream.Write(readBuffer, 0, bytesRead);
                            }
                        }
                    }
                }
                //Ordner erstellen
                file.CreateDirectory("Version");
                //Versions Datei erstellen
                filestream = file.CreateFile("Version/Version.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine("1.0.0.1");
                sw.Flush();
                filestream.Close();
            }
            //********************************************** Update auf 1.0.0.1 **********************************************



            //Auf Version prüfen und updaten
            //********************************************** Update auf 1.3.0.0 **********************************************
            if (!file.FileExists("/Version/VersionNew.txt"))
            {
                //Neues Dateiensystem erstellen
                FoldersAll = "/";
                string[] Folders = file.GetDirectoryNames("/Thumbs/");
                int FoldersC = Folders.Count();
                for (int i = 0; i < FoldersC; i++)
                {
                    FoldersAll += Folders[i] + "/";
                }
                //Neue Ordner Datei Erstellen
                filestream = file.CreateFile("Folders.dat");
                sw = new StreamWriter(filestream);
                sw.WriteLine(FoldersAll);
                sw.Flush();
                filestream.Close();

                //Ordner durchlaufen und Bilddateien erstellen
                for (int i = 0; i < FoldersC; i++)
                {
                    ImagesAll = "/";
                    string[] Images = file.GetFileNames("/Thumbs/" + Folders[i] + "/");
                    int ImagesC = Images.Count();
                    for (int i2 = 0; i2 < ImagesC; i2++)
                    {
                        ImagesAll += Images[i2] + "/";
                    }
                    //Neue Images Datei Erstellen
                    filestream = file.CreateFile("/Thumbs/" + Folders[i] + ".dat");
                    sw = new StreamWriter(filestream);
                    sw.WriteLine(ImagesAll);
                    sw.Flush();
                    filestream.Close();
                }

                //Versions Datei erstellen
                filestream = file.CreateFile("Version/VersionNew.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine("1300");
                sw.Flush();
                filestream.Close();
                
                //Version umstellen
                Version = 1300;
            }
            else
            {
                //Version laden
                filestream = file.OpenFile("/Version/VersionNew.txt", FileMode.Open);
                sr = new StreamReader(filestream);
                temp = sr.ReadToEnd();
                filestream.Close();
                Version = Convert.ToInt32(temp);
            }
            //********************************************** Update auf 1.3.0.0 **********************************************



            //********************************************** Update auf 1.4.47.0 **********************************************
            if (Version == 1300)
            {
                //Versions Datei erstellen
                filestream = file.CreateFile("Version/VersionNew.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine("14470");
                sw.Flush();
                filestream.Close();

                //Version umstellen
                Version = 14470;

                //Profil Ordner erstellen
                if (!file.DirectoryExists("Profile"))
                {
                    file.CreateDirectory("Profile");
                }

                //Settings neu erstellen
                CreateSettings();
            }
            //********************************************** Update auf 1.4.47.0 **********************************************


            
            //********************************************** Update auf 1.8.47.0 **********************************************
            if (Version == 14470)
            {
                //Versions Datei erstellen
                filestream = file.CreateFile("Version/VersionNew.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine("18470");
                sw.Flush();
                filestream.Close();

                //Version umstellen
                Version = 18470;

                //Test Version zurücksetzen
                dt = DateTime.Now;
                filestream = file.CreateFile("Settings/FirstTime.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine(Convert.ToString(dt));
                sw.Flush();
                filestream.Close();

                //Settings neu erstellen
                CreateSettings();
            }
            //********************************************** Update auf 1.8.47.0 **********************************************



            //********************************************** Update auf 1.10.47.0 **********************************************
            if (Version == 18470)
            {
                //Versions Datei erstellen
                filestream = file.CreateFile("Version/VersionNew.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine("01104700");
                sw.Flush();
                filestream.Close();

                //Version umstellen
                Version = 01104700;

                //Alte Styles umbenennen
                for (int i = 1; i <= 6; i++)
                {
                    file.MoveFile("Styles/" + i + ".txt", "Styles/000000" + i + ".txt");
                }

                //Neue Styles kopieren
                for (int i = 7; i <= 9; i++)
                {
                    using (Stream input = Application.GetResourceStream(new Uri("Styles/" + i + ".txt", UriKind.Relative)).Stream)
                    {
                        // Create a stream for the new file in the local folder.
                        using (filestream = file.CreateFile("Styles/000000" + i + ".txt"))
                        {
                            // Initialize the buffer.
                            byte[] readBuffer = new byte[4096];
                            int bytesRead = -1;

                            // Copy the file from the installation folder to the local folder. 
                            while ((bytesRead = input.Read(readBuffer, 0, readBuffer.Length)) > 0)
                            {
                                filestream.Write(readBuffer, 0, bytesRead);
                            }
                        }
                    }
                }

                //Neue No Image Datei erstellen
                if (file.FileExists("NoImage.jpg"))
                {
                    //Datei löschen
                    file.DeleteFile("NoImage.Jpg");
                }
                using (Stream input = Application.GetResourceStream(new Uri("NoImage.jpg", UriKind.Relative)).Stream)
                {
                    // Create a stream for the new file in the local folder.
                    using (filestream = file.CreateFile("NoImage.jpg"))
                    {
                        // Initialize the buffer.
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the file from the installation folder to the local folder. 
                        while ((bytesRead = input.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            filestream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }

                //Rate Reminder erstellen
                DateTime datetime = DateTime.Now;
                datetime = datetime.AddDays(4);
                filestream = file.CreateFile("Settings/RateReminder.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine(datetime.ToString());
                sw.Flush();
                filestream.Close();

                //Styles Images Ordner ersellen
                if (!file.DirectoryExists("StylesImages"))
                {
                    file.CreateDirectory("StylesImages");
                }
                //Bilder in Styles Images kppieren
                var tempImage = new WriteableBitmap(1, 1);
                for (int i = 1; i <= 9; i++)
                {
                    //Bild kopieren
                    using (Stream input = Application.GetResourceStream(new Uri("Images/StylesImages/" + i + ".png", UriKind.Relative)).Stream)
                    {
                        // Create a stream for the new file in the local folder.
                        using (filestream = file.CreateFile("StylesImages/000000" + i + ".1.png"))
                        {
                            // Initialize the buffer.
                            byte[] readBuffer = new byte[4096];
                            int bytesRead = -1;

                            // Copy the file from the installation folder to the local folder. 
                            while ((bytesRead = input.Read(readBuffer, 0, readBuffer.Length)) > 0)
                            {
                                filestream.Write(readBuffer, 0, bytesRead);
                            }
                        }
                    }
                    //Bild laden
                    byte[] tempData;
                    MemoryStream tempMs;
                    using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (IsolatedStorageFileStream isfs = isf.OpenFile("StylesImages/000000" + i + ".1.png", FileMode.Open, FileAccess.Read))
                        {
                            tempData = new byte[isfs.Length];
                            isfs.Read(tempData, 0, tempData.Length);
                            isfs.Close();
                        }
                    }
                    tempMs = new MemoryStream(tempData);
                    tempImage.SetSource(tempMs);

                    //Bild Spiegeln
                    tempImage = tempImage.Flip(WriteableBitmapExtensions.FlipMode.Vertical);

                    //Bild speichern
                    Decoders.AddDecoder<PngDecoder>();
                    var img = tempImage.ToImage();
                    var encoder = new PngEncoder();
                    using (var stream = new IsolatedStorageFileStream("StylesImages/000000" + i + ".2.png", FileMode.Create, file))
                    {
                        encoder.Encode(img, stream);
                        stream.Close();
                    }

                    //Bild Spiegeln
                    tempImage = tempImage.Flip(WriteableBitmapExtensions.FlipMode.Horizontal);

                    //Bild speichern
                    Decoders.AddDecoder<PngDecoder>();
                    img = tempImage.ToImage();
                    encoder = new PngEncoder();
                    using (var stream = new IsolatedStorageFileStream("StylesImages/000000" + i + ".3.png", FileMode.Create, file))
                    {
                        encoder.Encode(img, stream);
                        stream.Close();
                    }
                    //Bild Spiegeln
                    tempImage = tempImage.Flip(WriteableBitmapExtensions.FlipMode.Vertical);

                    //Bild speichern
                    Decoders.AddDecoder<PngDecoder>();
                    img = tempImage.ToImage();
                    encoder = new PngEncoder();
                    using (var stream = new IsolatedStorageFileStream("StylesImages/000000" + i + ".4.png", FileMode.Create, file))
                    {
                        encoder.Encode(img, stream);
                        stream.Close();
                    }
                }

                //Neue StyleSettings Datei erstellen
                ResetStyleSettings();

                //Settings neu erstellen
                CreateSettings();
            }
            //********************************************** Update auf 1.10.47.0 **********************************************


                        
            //Prüfen ob Reminder noch vorhanden und wenn ja, laden
            if (file.FileExists("Settings/RateReminder.txt"))
            {
                //Daten laden
                filestream = file.OpenFile("Settings/RateReminder.txt", FileMode.Open);
                sr = new StreamReader(filestream);
                temp = sr.ReadToEnd();
                filestream.Close();
                temp = temp.TrimEnd(new char[] { '\r', '\n' });

                //Prüfen of Benachrichtigung ausgegeben wird
                DateTime DT_Reminder = Convert.ToDateTime(temp);
                DateTime DT_Now = DateTime.Now;
                int result = DateTime.Compare(DT_Reminder, DT_Now);
                if (result < 0)
                {
                    //Bewertung öffnen
                    GRRate.Margin = new Thickness(0, 0, 0, 0);
                    MenuOpen = true;
                }
            }   


            //Farbfelder der Einstellungen zurücksetzen
            ClearInfoSizeBGs();
            ClearFrameSizeBGs();
            ClearInfoTransBGs();


            //Einstellungen umsetzen
            string[] aSettings = Regex.Split(Settings, ";");
            //Ordner
            SetFolder = aSettings[0];
            if (SetFolder == "*")
            {
                TBLockscreenFolder.Content = Lockscreen_Swap.AppResx.Z02_RandomFolder;
            }
            else if (SetFolder == "**")
            {
                TBLockscreenFolder.Content = Lockscreen_Swap.AppResx.Z02_RandomPicture;
            }
            else
            {
                TBLockscreenFolder.Content = SetFolder;
            }
            //Hintergrundfarbe
            SetBgColor = aSettings[1];
            if (SetBgColor == "BC")
            {
                TBBGColor.Content = Lockscreen_Swap.AppResx.PhoneBackground;
            }
            else if (SetBgColor == "AC")
            {
                TBBGColor.Content = Lockscreen_Swap.AppResx.AccentColor;
            }
            else
            {
                //Farbe in Button schreiben
                TBBGColor.Content = SetBgColor;
                //Farbe in Colorpicker einstellen
                byte A = Convert.ToByte(SetBgColor.Substring(1, 2), 16);
                byte R = Convert.ToByte(SetBgColor.Substring(3, 2), 16);
                byte G = Convert.ToByte(SetBgColor.Substring(5, 2), 16);
                byte B = Convert.ToByte(SetBgColor.Substring(7, 2), 16);
                SolidColorBrush sb = new SolidColorBrush(Color.FromArgb(A, R, G, B));
                CP.Color = sb.Color;
            }
            //Frame Color
            SetFrameColor = aSettings[2];
            if (SetFrameColor == "NO")
            {
                TBFrameColor.Content = Lockscreen_Swap.AppResx.Transparent;
            }
            else if (SetFrameColor == "BC")
            {
                TBFrameColor.Content = Lockscreen_Swap.AppResx.PhoneBackground;
            }
            else if (SetFrameColor == "AC")
            {
                TBFrameColor.Content = Lockscreen_Swap.AppResx.AccentColor;
            }
            else
            {
                //Farbe in Button schreiben
                TBFrameColor.Content = SetFrameColor;
                //Farbe in Colorpicker einstellen
                byte A = Convert.ToByte(SetFrameColor.Substring(1, 2), 16);
                byte R = Convert.ToByte(SetFrameColor.Substring(3, 2), 16);
                byte G = Convert.ToByte(SetFrameColor.Substring(5, 2), 16);
                byte B = Convert.ToByte(SetFrameColor.Substring(7, 2), 16);
                SolidColorBrush sb = new SolidColorBrush(Color.FromArgb(A, R, G, B));
                CP2.Color = sb.Color;
            }
            //Alpha Color
            SetAlphaColor = aSettings[3];
            if (SetAlphaColor == "NO")
            {
                TBInfoColor.Content = Lockscreen_Swap.AppResx.Transparent;
            }
            else if (SetAlphaColor == "BC")
            {
                TBInfoColor.Content = Lockscreen_Swap.AppResx.PhoneBackground;
            }
            else if (SetAlphaColor == "AC")
            {
                TBInfoColor.Content = Lockscreen_Swap.AppResx.AccentColor;
            }
            else
            {
                //Farbe in Button schreiben
                TBInfoColor.Content = SetAlphaColor;
                //Farbe in Colorpicker einstellen
                byte A = Convert.ToByte(SetAlphaColor.Substring(1, 2), 16);
                byte R = Convert.ToByte(SetAlphaColor.Substring(3, 2), 16);
                byte G = Convert.ToByte(SetAlphaColor.Substring(5, 2), 16);
                byte B = Convert.ToByte(SetAlphaColor.Substring(7, 2), 16);
                SolidColorBrush sb = new SolidColorBrush(Color.FromArgb(A, R, G, B));
                CP3.Color = sb.Color;
            }
            //Rahmen Größe
            SetFrameSize = Convert.ToInt32(aSettings[4]);
            if (SetFrameSize == 0)
            {
                GRFrameSize0.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetFrameSize == 1)
            {
                GRFrameSize1.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetFrameSize == 2)
            {
                GRFrameSize2.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetFrameSize == 3)
            {
                GRFrameSize3.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetFrameSize == 4)
            {
                GRFrameSize4.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetFrameSize == 5)
            {
                GRFrameSize5.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetFrameSize == 6)
            {
                GRFrameSize6.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetFrameSize == 7)
            {
                GRFrameSize7.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetFrameSize == 8)
            {
                GRFrameSize8.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetFrameSize == 9)
            {
                GRFrameSize9.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            //Info Bereich Alpha
            SetInfoAlpha = Convert.ToInt32(aSettings[5]);
            if (SetInfoAlpha == 0)
            {
                GRInfoTrans0.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetInfoAlpha == 1)
            {
                GRInfoTrans1.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetInfoAlpha == 2)
            {
                GRInfoTrans2.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetInfoAlpha == 3)
            {
                GRInfoTrans3.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetInfoAlpha == 4)
            {
                GRInfoTrans4.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetInfoAlpha == 5)
            {
                GRInfoTrans5.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetInfoAlpha == 6)
            {
                GRInfoTrans6.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetInfoAlpha == 7)
            {
                GRInfoTrans7.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetInfoAlpha == 8)
            {
                GRInfoTrans8.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            if (SetInfoAlpha == 9)
            {
                GRInfoTrans9.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }

            //Swap Image
            SwapImage = Convert.ToInt32(aSettings[6]);
            //TBGeneratedStart.Text = SwapImage + " " + Lockscreen_Swap.AppResx.Z02_Generated;

            //InfoSize umstellen
            InfoSize = aSettings[7];
            if (InfoSize == "0")
            {
                GRInfoSize0.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            else if (InfoSize == "1")
            {
                GRInfoSize1.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            else if (InfoSize == "2")
            {
                GRInfoSize2.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            else if (InfoSize == "3")
            {
                GRInfoSize3.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            else
            {
                GRInfoSize4.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            }
            
            //InfoTop erstellen
            InfoTop = Convert.ToBoolean(aSettings[8]);
            if (InfoTop == true)
            {
                TBInfoTop.Content = Lockscreen_Swap.AppResx.On;
            }
            else
            {
                TBInfoTop.Content = Lockscreen_Swap.AppResx.Off;
            }

            //Logo Start
            SetLogoStart = Convert.ToBoolean(aSettings[9]);
            if (SetLogoStart == true)
            {
                TBSetLogo.Content = Lockscreen_Swap.AppResx.On;
            }
            else
            {
                TBSetLogo.Content = Lockscreen_Swap.AppResx.Off;
                dtt.Stop();
                GRAnimation.Visibility = System.Windows.Visibility.Collapsed;
            }


            //Prüfen ob Cycle Tile bereits ersellt wurde
            oTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("flip".ToString()));
            if (oTile != null && oTile.NavigationUri.ToString().Contains("flip"))
            {
                TBCycleTileHeader.Visibility = System.Windows.Visibility.Collapsed;
                TBCycleTile.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                TBCycleTileHeader.Visibility = System.Windows.Visibility.Visible;
                TBCycleTile.Visibility = System.Windows.Visibility.Visible;
            }


            //Menüs schließen
            GRFolderMenu.Margin = new Thickness(-600, 0, 0, 0);
            GRBGColor.Margin = new Thickness(-600, 0, 0, 0);
            GRFolder.Margin = new Thickness(-600, 0, 0, 0);
            GRFRColor.Margin = new Thickness(-600, 0, 0, 0);
            GRALColor.Margin = new Thickness(-600, 0, 0, 0);
            GRImage.Margin = new Thickness(-600, 0, 0, 0);
            //Angeben das Menüs geschlossen sind
            MenuOpen = false;


            //Prüfen ob App gerade gekauft wurde und in Einstellungen speichern
            if ((Application.Current as App).IsTrial)
            {
            }
            //Bei Kaufversion
            else
            {
                if (FullVersion == false)
                {
                    //Settings neu erstellen
                    filestream = file.CreateFile("Settings/FullVersion.txt");
                    sw = new StreamWriter(filestream);
                    sw.WriteLine("1");
                    sw.Flush();
                    filestream.Close();
                    //FullVersion umstellen
                    FullVersion = true;
                    //Benachrichtigung ausgeben
                    MessageBox.Show(Lockscreen_Swap.AppResx.PurchaseNote);
                }
            }



            //Bei Vollversion
            if (FullVersion == true)
            {
                SPTrial.Visibility = System.Windows.Visibility.Collapsed;
            }
            //Bei Demoversion
            else
            {
                //Prüfen ob Trial Zeit abgelaufen
                TimeSpan diff = dt_Now - dt;
                int MinToGo = 1440 - Convert.ToInt32(diff.TotalMinutes);
                //Wenn Zeit abgelaufen
                if (MinToGo <= 0)
                {
                    //Angeben das Bild auf Leer gestellt wird
                    Run = false;
                    if (TrialMSG == false)
                    {
                        MessageBox.Show(Lockscreen_Swap.AppResx.TrialNote);
                        TrialMSG = true;
                        TBTrial.Text = Lockscreen_Swap.AppResx.TrialExpired;
                        TBTialTime.Text = "";
                    }
                }
                //Wenn Zeit nicht abgelaufen
                else
                {
                    //Restliche Zeit erstellen
                    string tTime = "";
                    int tH = MinToGo / 60;
                    if (tH == 1)
                    {
                        tTime += tH + " " + Lockscreen_Swap.AppResx.Hour + "   ";
                    }
                    else
                    {
                        tTime += tH + " " + Lockscreen_Swap.AppResx.Hours + "   ";
                    }
                    int TM = MinToGo - (tH * 60);
                    if (TM == 1)
                    {
                        tTime += TM + " " + Lockscreen_Swap.AppResx.Minute;
                    }
                    else
                    {
                        tTime += TM + " " + Lockscreen_Swap.AppResx.Minutes;
                    }
                    //Zeit ausgeben
                    TBTialTime.Text = tTime;
                }
            }



            //Prüfen ob ScheduledTaskAgent läuft
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;

            //Prüfen ob momentan Lockscreen App
            if (Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication)
            {
                //IsLockscreenApp auf true stellen
                IsLockscreenApp = true;
            }

            //Prüfen ob Task läuft und App Lockscreen App ist
            if (periodicTask != null & IsLockscreenApp == true)
            {
                //agentsAreEnabled auf true
                agentsAreEnabled = true;

                //Button umstellen
                TBLockscreenChanger.Content = Lockscreen_Swap.AppResx.On;
            }

            //Wenn nicht, Button auf false stellen
            else
            {
                //agentsAreEnabled auf false
                agentsAreEnabled = false;

                //Button umstellen
                TBLockscreenChanger.Content = Lockscreen_Swap.AppResx.Off;
            }

            //Abfragen ob App als Lockscreen App gesetzt werden soll
            if ((IsLockscreenApp == false | agentsAreEnabled == false) & IsSetBGTask == false)
            {
                //Lockhelper aktivieren
                string filePathOfTheImage = "/Images/LockscreenImage.jpg";
                bool isAppResource = true;
                LockHelper(filePathOfTheImage, isAppResource);

                //Button umstellen
                TBLockscreenChanger.Content = Lockscreen_Swap.AppResx.On;
            }

            //Hintergrund Task einmalig neu aktivieren //Und Lockscreen einmalig neu erstellen
            if (IsSetBGTask == false)
            {
                //Task neu Starten
                StartPeriodicAgent();
                //Angeben das Task gestartet wurde
                IsSetBGTask = true;

                //Lockscreen Bild wechseln
                if (IsLockscreenApp == true)
                {
                    CreateImage();
                }
            }

            //Nach auswahl von Profil Bild neu erstellen
            if (file.FileExists("CreateNew.txt"))
            {
                //CreateNew löschen
                file.DeleteFile("CreateNew.txt");
                //Bild neu erstellen
                CreateImage();
                //Bild ausgeben
                GRImage.Margin = new Thickness(0, 0, 0, 0);
                MenuOpen = true;
            }

            //Ordner laden
            CreateFolders();
           

            // Prüfen ob Werbung bereits angezeigt wurde
            if (!file.FileExists("Settings/ShowAd.txt"))
            {
                // Angeben das Werbung beriets angezeigt wurde
                filestream = file.CreateFile("Settings/ShowAd.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine("1");
                sw.Flush();
                filestream.Close();

                // 10.0 Lock Screen Seite anzeigen
                NavigationService.Navigate(new Uri("/Pages/10_0_Lock_Screen.xaml", UriKind.Relative));
            }
        }
        //---------------------------------------------------------------------------------------------------------










        //Neues Bild erstellen
        //*********************************************************************************************************
        void CreateImage()
        {
            //StyleSettings laden
            filestream = file.OpenFile("Settings/StylesSettings.txt", FileMode.Open);
            sr = new StreamReader(filestream);
            string stylesSettings = sr.ReadToEnd();
            filestream.Close();
            stylesSettings = stylesSettings.Trim(new char[] { '\r', '\n' });

            //Installierte styles
            filestream = file.OpenFile("Settings/InstalledStyles.txt", FileMode.Open);
            sr = new StreamReader(filestream);
            string InstalledStyles = sr.ReadToEnd();
            filestream.Close();
            InstalledStyles = InstalledStyles.Trim(new char[] { '\r', '\n' });

            //Variablen erstellen
            string iStyle = "1.txt";
            int iMirrow = 1;
            Random rand = new Random();
            bool StyleOk = false;

            //Prüfen ob Settings vorhanden
            if (stylesSettings.Length > 1)
            {
                //StyleSettings zerlegen
                string[] splitStylesSettings = Regex.Split(stylesSettings, ";");
                int selectedStyle = rand.Next(1, splitStylesSettings.Count());
                if (selectedStyle == splitStylesSettings.Count())
                {
                    selectedStyle--;
                }
                //Nach hintern verschobenen Style zurücksetzen
                selectedStyle--;
                //Auswahl zerlegen
                splitStylesSettings[selectedStyle] = splitStylesSettings[selectedStyle].Trim(new char[] { '\r', '\n' });
                string[] splitTemp = Regex.Split(splitStylesSettings[selectedStyle], ",");
                //Auswahl erstellen
                iStyle = splitTemp[0];
                iMirrow = Convert.ToInt32(splitTemp[1]);
                //Auswahl überprüfen
                string[] tempIS = Regex.Split(InstalledStyles, iStyle + ";");
                if (tempIS.Count() > 1)
                {
                    StyleOk = true;
                }
            }

            //Wenn keine Einstellungen vorhanden
            if (StyleOk == false)
            {
                //Installierte Styles zerlegen und den ersten laden
                string[] tempIS = Regex.Split(InstalledStyles, ";");
                iStyle = tempIS[0];
                iMirrow = 1;
                //StyleSettings zurücksetzen
                ResetStyleSettings();
            }

            //Set Spiegelung erstellen
            int MH = 1;
            int MV = 1;
            if (iMirrow == 2)
            {
                MH = 2;
            }
            if (iMirrow == 3)
            {
                MH = 2;
                MV = 2;
            }
            if (iMirrow == 4)
            {
                MV = 2;
            }


            //Style laden
            filestream = file.OpenFile("Styles/" + iStyle, FileMode.Open);
            sr = new StreamReader(filestream);
            string style = sr.ReadToEnd();
            filestream.Close();

            //Style Datei zerlegen
            string[] images = Regex.Split(style, ";");
            //Anzahl der Bilder erstellen
            int cImages = images.Count() - 1;


            //Bilderliste erstellen
            bool noImage = true;
            string[] lImages = new string[cImages];

            //Ordner auswählen
            string loadFolder = "*";


            
            //Prüfen ob Random Bilder ausgewählt ist
            if (SetFolder == "**")
            {

                //Ordner Datei laden
                filestream = file.OpenFile("Folders.dat", FileMode.Open);
                sr = new StreamReader(filestream);
                string FoldersDat = sr.ReadToEnd();
                FoldersDat = FoldersDat.TrimEnd(new char[] { '\r', '\n' });
                filestream.Close();

                //Ordnerdatei aufteilen
                string[] FoldersSplit = Regex.Split(FoldersDat, "/");
                int cFoldersSplit = FoldersSplit.Count();
                string AllPictures = "";

                //Unterordner Dateien laden
                for (int i = 1; i < (cFoldersSplit - 1); i++)
                {
                    //Pictures.Dat laden
                    filestream = file.OpenFile("Thumbs/" + FoldersSplit[i] + ".dat", FileMode.Open);
                    sr = new StreamReader(filestream);
                    string ImagesDat = sr.ReadToEnd();
                    ImagesDat = ImagesDat.TrimEnd(new char[] { '\r', '\n' });
                    ImagesDat = ImagesDat.Trim(new char[] { '/' });
                    filestream.Close();

                    //Prüfen ob Bilder vorhanden
                    if (ImagesDat.Length >= 1)
                    {
                        //Pictures.Dat trennen
                        string[] ImagesSplit = Regex.Split(ImagesDat, "/");
                        int cImagesSplit = ImagesSplit.Count();
                        //Bilder durchlaufen und All Pictures erstellen
                        for (int i2 = 0; i2 < cImagesSplit; i2++)
                        {
                            AllPictures += "Folders/" + FoldersSplit[i] + "/" + ImagesSplit[i2] + "///";
                        }
                    }
                }
                
                //Doppelte Trennzeichen entfernen
                AllPictures = AllPictures.Trim(new char[] { '/' });

                //Wenn Bilder vorhanden
                if (AllPictures.Length >= 1)
                {
                    //Alle Bilder aus Ordner laden
                    string[] tImages = Regex.Split(AllPictures, "///");
                    int ctImages = tImages.Count();
                    
                    //Bilder verarbeiten
                    string[] sImages = new string[ctImages];
                    for (int i = 0; i < ctImages; i++)
                    {
                        sImages[i] = tImages[i];
                    }
                    //Bilder aus Bilderliste wählen und Bilder liste dabei verkleinern
                    for (int i = 0; i < cImages; i++)
                    {
                        //Bild auswählen
                        int csImages = sImages.Count();
                        int c = rand.Next(1, (csImages + 1));
                        if (c == (csImages + 1))
                        {
                            c--;
                        }
                        c--;

                        lImages[i] = sImages[c];
                        //Bilderliste neu erstellen
                        if (sImages.Count() == 1)
                        {
                            sImages = new string[ctImages];
                            for (int i2 = 0; i2 < ctImages; i2++)
                            {
                                sImages[i2] = tImages[i2];
                            }
                        }
                        else
                        {
                            int cs = sImages.Count();
                            int x = 0;
                            string[] nsImages = new string[(cs - 1)];
                            for (int i3 = 0; i3 < cs; i3++)
                            {
                                if (i3 != c)
                                {
                                    nsImages[x] = sImages[i3];
                                    x++;
                                }
                            }
                            sImages = new string[(cs - 1)];
                            for (int i3 = 0; i3 < sImages.Count(); i3++)
                            {
                                sImages[i3] = nsImages[i3];
                            }
                        }
                    }
                }
                //Wenn keine Bilder existieren
                else
                {
                    //Immer no Image laden
                    for (int i = 0; i < cImages; i++)
                    {
                        lImages[i] = "NoImage.jpg";
                    }
                }
            }



            //Wenn Ordner oder Random Ordner ausgewählt wurde
            else
            {
                //Prüfen ob Settings Folder besteht
                if (SetFolder != "*")
                {
                    if (!file.DirectoryExists("Folders/" + SetFolder))
                    {
                        //Settings neu erstellen
                        SetFolder = "*";
                        CreateSettings();
                    }
                    else
                    //Prüfen ob Dateien in Ordner
                    {
                        string[] tImages = file.GetFileNames("Folders/" + SetFolder + "/");
                        if (tImages.Count() < 1)
                        {
                            //Settings neu erstellen
                            SetFolder = "*";
                            CreateSettings();
                        }
                        else
                        {
                            loadFolder = "Folders/" + SetFolder + "/";
                            noImage = false;
                        }
                    }
                }



                //Wenn Loading Folder nicht besteht //Random
                if (loadFolder == "*")
                {
                    //Ordner Liste laden
                    string tFolders = "";
                    string[] lFolders = file.GetDirectoryNames("Folders/");

                    //Ordner Anzahl erstellen
                    int cFolders = lFolders.Count();

                    //Ordner durchlaufen und prüfen ob Bilder vorhanden
                    for (int i2 = 0; i2 < cFolders; i2++)
                    {
                        string[] tImages = file.GetFileNames("Folders/" + lFolders[i2] + "/");
                        if (tImages.Count() > 0)
                        {
                            tFolders += i2 + ";";
                        }
                    }

                    if (tFolders.Length > 0)
                    {
                        string[] tlFolders = Regex.Split(tFolders, ";");
                        int ctlFolders = tlFolders.Count() - 1;
                        int iFolder = rand.Next(1, tlFolders.Count());
                        int temp = Convert.ToInt32(tlFolders[iFolder - 1]);
                        loadFolder = "Folders/" + lFolders[temp] + "/";
                        noImage = false;
                    }
                }

                //Wenn Bilder existieren
                if (noImage == false & Run == true)
                {
                    //Alle Bilder aus Ordner laden
                    string[] tImages = file.GetFileNames(loadFolder);
                    int ctImages = tImages.Count();
                    string[] sImages = new string[ctImages];
                    for (int i = 0; i < ctImages; i++)
                    {
                        sImages[i] = tImages[i];
                    }
                    //Bilder aus Bilderliste wählen und Bilder liste dabei verkleinern
                    for (int i = 0; i < cImages; i++)
                    {
                        //Bild auswählen
                        int csImages = sImages.Count();
                        int c = rand.Next(1, (csImages + 1));
                        if (c == (csImages + 1))
                        {
                            c--;
                        }
                        c--;

                        lImages[i] = loadFolder + sImages[c];
                        //Bilderliste neu erstellen
                        if (sImages.Count() == 1)
                        {
                            sImages = new string[ctImages];
                            for (int i2 = 0; i2 < ctImages; i2++)
                            {
                                sImages[i2] = tImages[i2];
                            }
                        }
                        else
                        {
                            int cs = sImages.Count();
                            int x = 0;
                            string[] nsImages = new string[(cs - 1)];
                            for (int i3 = 0; i3 < cs; i3++)
                            {
                                if (i3 != c)
                                {
                                    nsImages[x] = sImages[i3];
                                    x++;
                                }
                            }
                            sImages = new string[(cs - 1)];
                            for (int i3 = 0; i3 < sImages.Count(); i3++)
                            {
                                sImages[i3] = nsImages[i3];
                            }
                        }
                    }
                }

                //Wenn keine Bilder existieren
                else
                {
                    //Immer no Image laden
                    for (int i = 0; i < cImages; i++)
                    {
                        lImages[i] = "NoImage.jpg";
                    }
                }
            }



            //Bild erstellen
            var Background = new WriteableBitmap(480, 800);
            

            //Hintergrundfarbe erstellen
            Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            if (SetBgColor == "AC")
            {
                backgroundColor = (Color)Application.Current.Resources["PhoneAccentColor"];
            }
            else if (SetBgColor == "BC")
            {
                backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            }
            else
            {
                if (SetBgColor.Length == 9)
                {
                    byte A = Convert.ToByte(SetBgColor.Substring(1, 2), 16);
                    byte R = Convert.ToByte(SetBgColor.Substring(3, 2), 16);
                    byte G = Convert.ToByte(SetBgColor.Substring(5, 2), 16);
                    byte B = Convert.ToByte(SetBgColor.Substring(7, 2), 16);
                    SolidColorBrush sb = new SolidColorBrush(Color.FromArgb(A, R, G, B));
                    backgroundColor = sb.Color;
                }
            }
            Background.Clear(backgroundColor);

            //Rahmenfarbe erstellen
            Color frameColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            if (SetFrameColor != "NO")
            {
                if (SetFrameColor == "AC")
                {
                    frameColor = (Color)Application.Current.Resources["PhoneAccentColor"];
                }
                else if (SetFrameColor == "BC")
                {
                    frameColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
                }
                else
                {
                    if (SetFrameColor.Length == 9)
                    {
                        byte A = Convert.ToByte(SetFrameColor.Substring(1, 2), 16);
                        byte R = Convert.ToByte(SetFrameColor.Substring(3, 2), 16);
                        byte G = Convert.ToByte(SetFrameColor.Substring(5, 2), 16);
                        byte B = Convert.ToByte(SetFrameColor.Substring(7, 2), 16);
                        SolidColorBrush sb = new SolidColorBrush(Color.FromArgb(A, R, G, B));
                        frameColor = sb.Color;
                    }
                }
            }

            //Tiles durchlaufen und erstellen
            for (int i = 0; i < lImages.Count(); i++)
            {
                //Daten aus Style verarbeiten
                string[] splitImage = Regex.Split(images[i], ",");
                int x1 = Convert.ToInt32(splitImage[0]);
                int y1 = Convert.ToInt32(splitImage[1]);
                int x2 = Convert.ToInt32(splitImage[2]);
                int y2 = Convert.ToInt32(splitImage[3]);
                int size = x2 - x1;
                
                //Spiegelung errechnen
                if (MH == 2)
                {
                    x1 = 480 - x1 - size;
                    x2 = x1 + size;
                }
                if (MV == 2)
                {
                    y1 = 800 - y1 - size;
                    y2 = y1 + size;
                }

                //Tile erstellen
                byte[] tempData;
                MemoryStream tempMs;
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream isfs = isf.OpenFile(lImages[i], FileMode.Open, FileAccess.Read))
                    {
                        tempData = new byte[isfs.Length];
                        isfs.Read(tempData, 0, tempData.Length);
                        isfs.Close();
                    }
                }
                tempMs = new MemoryStream(tempData);
                var tile = new WriteableBitmap(0, 0);
                tile.SetSource(tempMs);
                if (size != 320)
                {
                    tile = tile.Resize(size, size, WriteableBitmapExtensions.Interpolation.Bilinear);
                }

                //Tile in Bild schneiden
                Background.Blit(new Rect(x1, y1, size, size), tile, new Rect(0, 0, size, size));

                //Rahmen erstellen, wenn eingestellt
                if (SetFrameColor != "NO" & SetFrameSize != 0)
                {
                    for (int i5 = 0; i5 < SetFrameSize; i5++)
                    {
                        Background.DrawRectangle((x1 + i5), (y1 + i5), (x2 - i5), (y2 - i5), frameColor);
                    }
                }
            }


            //Infofarbe erstellen
            Color alphaColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            if (SetAlphaColor != "NO")
            {
                if (SetAlphaColor == "AC")
                {
                    alphaColor = (Color)Application.Current.Resources["PhoneAccentColor"];
                }
                else if (SetAlphaColor == "BC")
                {
                    alphaColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
                }
                else
                {
                    if (SetAlphaColor.Length == 9)
                    {
                        byte A = Convert.ToByte(SetAlphaColor.Substring(1, 2), 16);
                        byte R = Convert.ToByte(SetAlphaColor.Substring(3, 2), 16);
                        byte G = Convert.ToByte(SetAlphaColor.Substring(5, 2), 16);
                        byte B = Convert.ToByte(SetAlphaColor.Substring(7, 2), 16);
                        SolidColorBrush sb = new SolidColorBrush(Color.FromArgb(A, R, G, B));
                        alphaColor = sb.Color;
                    }
                }
            }
            //Farbe mit Alpha erstellen
            byte a = Convert.ToByte(0);
            if (SetInfoAlpha != 9)
            {
                a = Convert.ToByte(255 - (28 * SetInfoAlpha));
            }
            byte r = alphaColor.R;
            byte g = alphaColor.G;
            byte b = alphaColor.B;
            SolidColorBrush sb2 = new SolidColorBrush(Color.FromArgb(a, r, g, b));
            alphaColor = sb2.Color;

            
            //Infofarbe in Bild schneiden
            if (SetAlphaColor != "NO" & InfoSize != "0")
            {
                //Info Größe bestimmen
                int tSize = 455;
                if (InfoSize == "3")
                {
                    tSize = 345;
                }
                else if (InfoSize == "2")
                {
                    tSize = 235;
                }
                else if (InfoSize == "1")
                {
                    tSize = 125;
                }
                var tile = new WriteableBitmap(480, tSize);
                tile.Clear(alphaColor);
                //Tile in Bild schneiden
                Background.Blit(new Rect(0, (800 - tSize), 480, tSize), tile, new Rect(0, 0, 480, tSize));
            }


            //InfoTop erstellen
            if (InfoTop == true & SetAlphaColor != "NO")
            {
                //Tile erstellen
                var tile = new WriteableBitmap(480, 30);
                tile.Clear(alphaColor);
                //Tile in Bild schneiden
                Background.Blit(new Rect(0, 0, 480, 30), tile, new Rect(0, 0, 480, 30));
            }


            //Prüfen ob Ordner schon vorhanden
            if (!file.DirectoryExists("Background"))
            {
                file.CreateDirectory("Background");
            }
            //Prüfen ob Bild vorhanden
            string SaveFile;
            if (file.FileExists("Background/" + (SwapImage - 1) + ".jpg"))
            {
                file.DeleteFile("Background/" + (SwapImage - 1) + ".jpg");
            }
            if (file.FileExists("Background/" + (SwapImage - 2) + ".jpg"))
            {
                file.DeleteFile("Background/" + (SwapImage - 2) + ".jpg");
            }
            //Swap Image erhöhen
            SwapImage++;
            //SaveFile neu erstellen
            SaveFile = "Background/" + SwapImage + ".jpg";

            //Einstellungen neu erstellen
            CreateSettings();

            //Datei in Isolated Storage schreiben
            var isolatedStorageFileStream = file.CreateFile(SaveFile);
            Background.SaveJpeg(isolatedStorageFileStream, 480, 800, 0, 100);
            isolatedStorageFileStream.Close();


            //Lockscreen erstellen
            string filePathOfTheImage = SaveFile;
            var schema = "ms-appdata:///Local/";
            var uri = new Uri(schema + filePathOfTheImage, UriKind.RelativeOrAbsolute);
            //Wenn LockscreenApp Lockscreen setzen
            if (IsLockscreenApp == true)
            {
                Windows.Phone.System.UserProfile.LockScreen.SetImageUri(uri);
            }

            //Bild in ShowImage laden
            try
            {
                ShowImage.Source = Background;
                DemoImage.Source = Background;
            }
            catch
            {
            }

            //Cycle Tyle updaten
            if (oTile != null && oTile.NavigationUri.ToString().Contains("flip"))
            {
                //Cycle Bilder erstellen
                List<Uri> list = new List<Uri>();
                for (int i = 0; i < 9; i++)
                {
                    if (lImages[i] != "false")
                    {
                        if (file.FileExists("Shared/ShellContent/myImage" + i + ".jpg"))
                        {
                            file.DeleteFile("Shared/ShellContent/myImage" + i + ".jpg");
                        }
                        file.CopyFile(lImages[i], "Shared/ShellContent/myImage" + i + ".jpg");
                        list.Add(new Uri("isostore:/Shared/ShellContent/myImage" + i + ".jpg", UriKind.Absolute));
                    }
                }


                //CycleTile erstellen
                CycleTileData tileData = new CycleTileData()
                {
                    Title = "",
                    Count = null,
                    SmallBackgroundImage = new Uri("Icon.png", UriKind.RelativeOrAbsolute),
                    CycleImages = list
                };

                oTile.Update(tileData);
            }

            //Generated Info ändern
            //TBGeneratedStart.Text = SwapImage + " " + Lockscreen_Swap.AppResx.Z02_Generated;
        }
        //*********************************************************************************************************










        //Button Bild neu erstellen
        //*********************************************************************************************************
        private void BtnGenerateNew(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CreateImage();
        }
        //*********************************************************************************************************










        //Settings neu erstellen
        //*********************************************************************************************************
        void CreateSettings()
        {
            //Einstellungen neu erstellen
            Settings = SetFolder + ";" + SetBgColor + ";" + SetFrameColor + ";" + SetAlphaColor + ";" + SetFrameSize + ";" + SetInfoAlpha + ";" + SwapImage + ";" + InfoSize + ";" + Convert.ToString(InfoTop) + ";" + Convert.ToString(SetLogoStart) + ";";
            //Einstellungen speichern
            filestream = file.CreateFile("Settings/Settings.txt");
            sw = new StreamWriter(filestream);
            sw.WriteLine(Settings);
            sw.Flush();
            filestream.Close();
        }
        //*********************************************************************************************************









        //Settings neu erstellen
        //*********************************************************************************************************
        void ResetStyleSettings()
        {
            //Alle Style Dateien laden
            string StyleSettings = "";
            string[] AllStyles = file.GetFileNames("Styles/");
            int cAllStyles = AllStyles.Count();

            //Datei mit instellierten Styles erstellen
            string InstalledStyles = "";
            for (int i = 0; i < cAllStyles; i++)
            {
                InstalledStyles += AllStyles[i] + ";";
            }
            filestream = file.CreateFile("Settings/InstalledStyles.txt");
            sw = new StreamWriter(filestream);
            sw.WriteLine(InstalledStyles);
            sw.Flush();
            filestream.Close();

            //Alles Styles durchlaufen und neue Style Datei erstellen
            for (int i = 0; i < cAllStyles; i++)
            {
                //Style laden
                filestream = file.OpenFile("Styles/" + AllStyles[i], FileMode.Open);
                sr = new StreamReader(filestream);
                string tempStyle = sr.ReadToEnd();
                filestream.Close();

                //Temponäre Style Dateien erstellen
                string tempStyle1 = "";
                string tempStyle2 = "";
                string tempStyle3 = "";
                string tempStyle4 = "";

                //Style zerlegen und neu schreiben
                string[] SplitStyle = Regex.Split(tempStyle, ";");
                int cSplitStyle = SplitStyle.Count();

                //Style Datei durchlaufen und temponäre Styles erstellen
                for (int i2 = 0; i2 < (cSplitStyle-1); i2++)
                {
                    //Style trim
                    SplitStyle[i2] = SplitStyle[i2].Trim(new char[] { '\r', '\n' });
                    //Kachel Zerlegen
                    string[] SplitTile = Regex.Split(SplitStyle[i2], ",");
                    //Koordinaten erstellen
                    int x1 = Convert.ToInt32(SplitTile[0]);
                    int y1 = Convert.ToInt32(SplitTile[1]);
                    int x2 = Convert.ToInt32(SplitTile[2]);
                    int y2 = Convert.ToInt32(SplitTile[3]);
                    int size = x2 - x1;

                    //Ersten TempStyle erstellen
                    string tCoordinate = x1.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle1 += "0" + x1.ToString();
                    }
                    else
                    {
                        tempStyle1 += x1.ToString();
                    }
                    tCoordinate = y1.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle1 += "0" + y1.ToString();
                    }
                    else
                    {
                        tempStyle1 += y1.ToString();
                    }
                    tCoordinate = x2.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle1 += "0" + x2.ToString();
                    }
                    else
                    {
                        tempStyle1 += x2.ToString();
                    }
                    tCoordinate = y2.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle1 += "0" + y2.ToString() + ";";
                    }
                    else
                    {
                        tempStyle1 += y2.ToString() + ";";
                    }
                    //Zweiten TempStyle erstellen
                    x1 = 480 - x1 - size;
                    x2 = x1 + size;
                    tCoordinate = x1.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle2 += "0" + x1.ToString();
                    }
                    else
                    {
                        tempStyle2 += x1.ToString();
                    }
                    tCoordinate = y1.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle2 += "0" + y1.ToString();
                    }
                    else
                    {
                        tempStyle2 += y1.ToString();
                    }
                    tCoordinate = x2.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle2 += "0" + x2.ToString();
                    }
                    else
                    {
                        tempStyle2 += x2.ToString();
                    }
                    tCoordinate = y2.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle2 += "0" + y2.ToString() + ";";
                    }
                    else
                    {
                        tempStyle2 += y2.ToString() + ";";
                    }
                    //Driten Temp Style erstellen
                    y1 = 800 - y1 - size;
                    y2 = y1 + size;
                    tCoordinate = x1.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle3 += "0" + x1.ToString();
                    }
                    else
                    {
                        tempStyle3 += x1.ToString();
                    }
                    tCoordinate = y1.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle3 += "0" + y1.ToString();
                    }
                    else
                    {
                        tempStyle3 += y1.ToString();
                    }
                    tCoordinate = x2.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle3 += "0" + x2.ToString();
                    }
                    else
                    {
                        tempStyle3 += x2.ToString();
                    }
                    tCoordinate = y2.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle3 += "0" + y2.ToString() + ";";
                    }
                    else
                    {
                        tempStyle3 += y2.ToString() + ";";
                    }
                    //Vierten Temp Style erstellen
                    x1 = 480 - x1 - size;
                    x2 = x1 + size;
                    tCoordinate = x1.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle4 += "0" + x1.ToString();
                    }
                    else
                    {
                        tempStyle4 += x1.ToString();
                    }
                    tCoordinate = y1.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle4 += "0" + y1.ToString();
                    }
                    else
                    {
                        tempStyle4 += y1.ToString();
                    }
                    tCoordinate = x2.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle4 += "0" + x2.ToString();
                    }
                    else
                    {
                        tempStyle4 += x2.ToString();
                    }
                    tCoordinate = y2.ToString();
                    if (tCoordinate.Length == 2)
                    {
                        tempStyle4 += "0" + y2.ToString() + ";";
                    }
                    else
                    {
                        tempStyle4 += y2.ToString() + ";";
                    }
                }
                
                //Temp Styles 1 zerlegen und sortieren
                SplitStyle = Regex.Split(tempStyle1, ";");
                Array.Sort(SplitStyle);
                tempStyle1 = "";
                for (int i3 = 0; i3 < SplitStyle.Count(); i3++)
                {
                    tempStyle1 += SplitStyle[i3];
                }
                //Temp Styles 2 zerlegen und sortieren
                SplitStyle = Regex.Split(tempStyle2, ";");
                Array.Sort(SplitStyle);
                tempStyle2 = "";
                for (int i3 = 0; i3 < SplitStyle.Count(); i3++)
                {
                    tempStyle2 += SplitStyle[i3];
                }
                //Temp Styles 3 zerlegen und sortieren
                SplitStyle = Regex.Split(tempStyle3, ";");
                Array.Sort(SplitStyle);
                tempStyle3 = "";
                for (int i3 = 0; i3 < SplitStyle.Count(); i3++)
                {
                    tempStyle3 += SplitStyle[i3];
                }
                //Temp Styles 4 zerlegen und sortieren
                SplitStyle = Regex.Split(tempStyle4, ";");
                Array.Sort(SplitStyle);
                tempStyle3 = "";
                for (int i3 = 0; i3 < SplitStyle.Count(); i3++)
                {
                    tempStyle4 += SplitStyle[i3];
                }
                
                //Erste Style Datei erstellen
                StyleSettings += AllStyles[i] + ",1;";
                //Zweite Style Datei erstellen
                if (tempStyle1 != tempStyle2)
                {
                    StyleSettings += AllStyles[i] + ",2;";
                }
                //Dritte Style Datei erstellen
                if (tempStyle1 != tempStyle3 & tempStyle2 != tempStyle3)
                {
                    StyleSettings += AllStyles[i] + ",3;";
                }
                //Vierte Style Datei erstellen
                if (tempStyle1 != tempStyle4 & tempStyle2 != tempStyle4 & tempStyle3 != tempStyle4 & tempStyle1 != tempStyle2)
                {
                    StyleSettings += AllStyles[i] + ",4;";
                }
                StyleSettings += "\n";
            }

            //StylesSettings speichern
            filestream = file.CreateFile("Settings/StylesSettings.txt");
            sw = new StreamWriter(filestream);
            sw.WriteLine(StyleSettings);
            sw.Flush();
            filestream.Close();
        }
        //*********************************************************************************************************










        //Ordner erstellen
        //*********************************************************************************************************
        void CreateFolders()
        {
            //Alte Daten löschen
            datalist.Clear();
            datalist2.Clear();
            datalist2.Add(new ClassFolders("(" + Lockscreen_Swap.AppResx.Z02_RandomFolder + ")", "0"));
            datalist2.Add(new ClassFolders("(" + Lockscreen_Swap.AppResx.Z02_RandomPicture +")", "0"));

            //Dateien Laden "Styles"
            string[] tempFolders = file.GetDirectoryNames("/Folders/");
            int tempFolders_c = tempFolders.Count();

            //Anzahl Ordner ausgeben
            CFolders = tempFolders_c;
            // TBFoldersStart.Text = CFolders + " " + Lockscreen_Swap.AppResx.Z02_Folders;

            //Anzahl Bilder löschen
            CPictures = 0;

            //Ordner durchlaufen
            for (int i = 0; i < tempFolders_c; i++)
            {
                //Anzahl der Bilder laden
                filestream = file.OpenFile("Thumbs/"+ tempFolders[i] +".dat", FileMode.Open);
                sr = new StreamReader(filestream);
                string temp = sr.ReadToEnd();
                filestream.Close();
                string[] images = Regex.Split(temp, "/");
                string cImages = images.Count() - 2 + " " + Lockscreen_Swap.AppResx.Pictures;

                //Bilder gesamt zählen
                CPictures += (images.Count() - 2);

                //Daten in Klasse schreiben
                datalist.Add(new ClassFolders(tempFolders[i],cImages));
                datalist2.Add(new ClassFolders(tempFolders[i], cImages));
            }

            //Anzahl Bilder ausgeben
            // TBPicturesStart.Text = CPictures + " " + Lockscreen_Swap.AppResx.Pictures;

            //Daten in Listbox schreiben
            LBFolders.ItemsSource = datalist;
            LBFolder.ItemsSource = datalist2;
        }
        //*********************************************************************************************************










        //Settings Buttons
        //*********************************************************************************************************
        //Lockscreen on off
        private void BtnLockscreenOnOff(object sender, RoutedEventArgs e)
        {
            if (Convert.ToString(TBLockscreenChanger.Content) == Lockscreen_Swap.AppResx.On)
            {
                RemoveAgent(periodicTaskName);
            }
            else
            {
                BtnLockscreen();
            }
        }


        //Ordner Menü öffnen
        private void BtnChangeFolder(object sender, RoutedEventArgs e)
        {
            if (datalist2.Count >= 3)
            {
                if (MenuOpen == false)
                {
                    GRFolder.Margin = new Thickness(0, 0, 0, 0);
                    MenuOpen = true;
                }
            }
            else
            {
                MessageBox.Show(Lockscreen_Swap.AppResx.MsgNoFolder);
            }
        }
        //Ordner aus Listbox wählen
        bool folderAction = true;
        private void FolderChange(object sender, SelectionChangedEventArgs e)
        {
            if (folderAction == true)
            {
                //Frequenz setzen
                int SI = LBFolder.SelectedIndex;

                if (SI == 0)
                {
                    //Einstellung erstellen
                    SetFolder = "*";
                    CreateSettings();
                    //Button umstellen
                    TBLockscreenFolder.Content = Lockscreen_Swap.AppResx.Z02_RandomFolder;
                    //Angeben das Menü geschlossen ist
                    MenuOpen = false;
                    //Listbox schließen
                    GRFolder.Margin = new Thickness(-600, 0, 0, 0);
                    //Lockscreenbild wechseln
                    CreateImage();
                    //Neues LockscreenBild anzeigen
                    GRImage.Margin = new Thickness(0, 0, 0, 0);
                    MenuOpen = true;
                }
                else if (SI == 1)
                {
                    //Einstellung erstellen
                    SetFolder = "**";
                    CreateSettings();
                    //Button Umstellen
                    TBLockscreenFolder.Content = Lockscreen_Swap.AppResx.Z02_RandomPicture;
                    //Angeben das Menü geschlossen ist
                    MenuOpen = false;
                    //Listbox schließen
                    GRFolder.Margin = new Thickness(-600, 0, 0, 0);
                    //Lockscreenbild wechseln
                    CreateImage();
                    //Neues LockscreenBild anzeigen
                    GRImage.Margin = new Thickness(0, 0, 0, 0);
                    MenuOpen = true;
                }
                else
                {
                    //Prüfen ob Bilder in Ordner vorhanden
                    IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
                    string[] temp = file.GetFileNames("/Folders/" + (datalist2[SI] as ClassFolders).name + "/");
                    //Wenn Bilder in Ornder vorhanden
                    if (temp.Count() >= 1)
                    {
                        SetFolder = (datalist2[SI] as ClassFolders).name;
                        TBLockscreenFolder.Content = SetFolder;
                        //Settings neu erstellen
                        CreateSettings();
                        //Angeben das Menü geschlossen ist
                        MenuOpen = false;
                        //Listbox schließen
                        GRFolder.Margin = new Thickness(-600, 0, 0, 0);
                        //Lockscreenbild wechseln
                        CreateImage();
                        //Neues LockscreenBild anzeigen
                        GRImage.Margin = new Thickness(0, 0, 0, 0);
                        MenuOpen = true;
                    }
                    //Wenn keine Bilder in Ordner vorhanden
                    else
                    {
                        MessageBox.Show(Lockscreen_Swap.AppResx.MsgNoPicture);
                    }
                }

                //Listbox deselectieren
                folderAction = false;
                try
                {
                    LBFolder.SelectedIndex = -1;
                }
                catch
                {
                }
                folderAction = true;
            }
        }


        //Hintergrundfarbe, Menü öffnen
        private void BtnSetBGColor(object sender, RoutedEventArgs e)
        {
            //Hintergrundfarbe Menü reinfahren
            GRBGColor.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Hintergrundfarbe auf Handy Hintergrund
        private void BtnDeleteColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Hintergrundfarbe zurücksetzten
            SetBgColor = "BC";
            TBBGColor.Content = Lockscreen_Swap.AppResx.PhoneBackground;
            //Speichern
            CreateSettings();
            //Hintergrund neu erstellen
            CreateImage();
            //Menü schließen
            GRBGColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Hintergrundfarbe auf Accent Color
        private void BtnAccentColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Hintergrundfarbe zurücksetzten
            SetBgColor = "AC";
            TBBGColor.Content = Lockscreen_Swap.AppResx.AccentColor;
            //Speichern
            CreateSettings();
            //Hintergrund neu erstellen
            CreateImage();
            //Menü schließen
            GRBGColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Hintergundfarbe, auswahl abbrechen
        private void BtnBackColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Menü schließen
            GRBGColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
        }
        //Hintergundfarbe, auswahl
        private void BtnSetColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Hintergrundfarbe setzen
            SetBgColor = Convert.ToString(CP.Color);
            TBBGColor.Content = Convert.ToString(CP.Color);
            //Speichern
            CreateSettings();
            //Hintergrund neu erstellen
            CreateImage();
            //Menü schließen
            GRBGColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Farbauswahl
        private void ColorChanged(object sender, Color color)
        {
            CPColorBrush = new SolidColorBrush(color);
            CPColor = CPColorBrush.Color;
            TBCPColor.Text = CPColor.ToString();
        }


        //Rahmen, Menü öffnen
        private void BtnSetFRColor(object sender, RoutedEventArgs e)
        {
            //Rahmen Menü reinfahren
            GRFRColor.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Rahmen auf Handy Hintergrund
        private void BtnFRDeleteColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Rahmen zurücksetzten
            SetFrameColor = "BC";
            TBFrameColor.Content = Lockscreen_Swap.AppResx.PhoneBackground;
            //Speichern
            CreateSettings();
            //Rahmen neu erstellen
            CreateImage();
            //Menü schließen
            GRFRColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Rahmen auf Accent Color
        private void BtnFRAccentColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Rahmen zurücksetzten
            SetFrameColor = "AC";
            TBFrameColor.Content = Lockscreen_Swap.AppResx.AccentColor;
            //Speichern
            CreateSettings();
            //Hintergrund neu erstellen
            CreateImage();
            //Menü schließen
            GRFRColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Rahmen auf Transparent
        private void BtnFRTransColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Rahmen zurücksetzten
            SetFrameColor = "NO";
            TBFrameColor.Content = Lockscreen_Swap.AppResx.Transparent;
            //Speichern
            CreateSettings();
            //Hintergrund neu erstellen
            CreateImage();
            //Menü schließen
            GRFRColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Rahmen, auswahl abbrechen
        private void BtnFRBackColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Menü schließen
            GRFRColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
        }
        //Rahmen, auswahl
        private void BtnFRSetColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Rahmen setzen
            SetFrameColor = Convert.ToString(CP2.Color);
            TBFrameColor.Content = Convert.ToString(CP2.Color);
            //Speichern
            CreateSettings();
            //Hintergrund neu erstellen
            CreateImage();
            //Menü schließen
            GRFRColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Farbauswahl
        private void FRColorChanged(object sender, Color color)
        {
            CP2ColorBrush = new SolidColorBrush(color);
            CP2Color = CP2ColorBrush.Color;
            TBCP2Color.Text = CP2Color.ToString();
        }


        //Infofarbe, Menü öffnen
        private void BtnSetALColor(object sender, RoutedEventArgs e)
        {
            //InfoFarbe Menü reinfahren
            GRALColor.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Infofarbe auf Handy Hintergrund
        private void BtnALDeleteColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Infofarbe zurücksetzten
            SetAlphaColor = "BC";
            TBInfoColor.Content = Lockscreen_Swap.AppResx.PhoneBackground;
            //Speichern
            CreateSettings();
            //Hintergrund neu erstellen
            CreateImage();
            //Menü schließen
            GRALColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Hintergrundfarbe auf Accent Color
        private void BtnALAccentColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Hintergrundfarbe zurücksetzten
            SetAlphaColor = "AC";
            TBInfoColor.Content = Lockscreen_Swap.AppResx.AccentColor;
            //Speichern
            CreateSettings();
            //Hintergrund neu erstellen
            CreateImage();
            //Menü schließen
            GRALColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Hintergrundfarbe auf Transparent
        private void BtnALTransColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Hintergrundfarbe zurücksetzten
            SetAlphaColor = "NO";
            TBInfoColor.Content = Lockscreen_Swap.AppResx.Transparent;
            //Speichern
            CreateSettings();
            //Hintergrund neu erstellen
            CreateImage();
            //Menü schließen
            GRALColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Hintergundfarbe, auswahl abbrechen
        private void BtnALBackColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Menü schließen
            GRALColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
        }
        //Hintergundfarbe, auswahl
        private void BtnALSetColor(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Hintergrundfarbe setzen
            SetAlphaColor = Convert.ToString(CP3.Color);
            TBInfoColor.Content = Convert.ToString(CP3.Color);
            //Speichern
            CreateSettings();
            //Hintergrund neu erstellen
            CreateImage();
            //Menü schließen
            GRALColor.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Farbauswahl
        private void ALColorChanged(object sender, Color color)
        {
            CP3ColorBrush = new SolidColorBrush(color);
            CP3Color = CP3ColorBrush.Color;
            TBCP3Color.Text = CP3Color.ToString();
        }


        //Rahmen Größe erstellen
        private void BtnFrameSize0(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetFrameSize = 0;
            CreateSettings();
            ClearFrameSizeBGs();
            GRFrameSize0.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnFrameSize1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetFrameSize = 1;
            CreateSettings();
            ClearFrameSizeBGs();
            GRFrameSize1.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnFrameSize2(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetFrameSize = 2;
            CreateSettings();
            ClearFrameSizeBGs();
            GRFrameSize2.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnFrameSize3(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetFrameSize = 3;
            CreateSettings();
            ClearFrameSizeBGs();
            GRFrameSize3.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnFrameSize4(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetFrameSize = 4;
            CreateSettings();
            ClearFrameSizeBGs();
            GRFrameSize4.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnFrameSize5(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetFrameSize = 5;
            CreateSettings();
            ClearFrameSizeBGs();
            GRFrameSize5.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnFrameSize6(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetFrameSize = 6;
            CreateSettings();
            ClearFrameSizeBGs();
            GRFrameSize6.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnFrameSize7(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetFrameSize = 7;
            CreateSettings();
            ClearFrameSizeBGs();
            GRFrameSize7.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnFrameSize8(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetFrameSize = 8;
            CreateSettings();
            ClearFrameSizeBGs();
            GRFrameSize8.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnFrameSize9(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetFrameSize = 9;
            CreateSettings();
            ClearFrameSizeBGs();
            GRFrameSize9.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Rahmen Größe hintergründe zurücksetzen
        void ClearFrameSizeBGs()
        {
            GRFrameSize0.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRFrameSize1.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRFrameSize2.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRFrameSize3.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRFrameSize4.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRFrameSize5.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRFrameSize6.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRFrameSize7.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRFrameSize8.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRFrameSize9.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
        }


        //Info Bereich Transparenz erstellen
        private void BtnInfoTrans0(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetInfoAlpha = 0;
            CreateSettings();
            ClearInfoTransBGs();
            GRInfoTrans0.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoTrans1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetInfoAlpha = 1;
            CreateSettings();
            ClearInfoTransBGs();
            GRInfoTrans1.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoTrans2(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetInfoAlpha = 2;
            CreateSettings();
            ClearInfoTransBGs();
            GRInfoTrans2.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoTrans3(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetInfoAlpha = 3;
            CreateSettings();
            ClearInfoTransBGs();
            GRInfoTrans3.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoTrans4(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetInfoAlpha = 4;
            CreateSettings();
            ClearInfoTransBGs();
            GRInfoTrans4.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoTrans5(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetInfoAlpha = 5;
            CreateSettings();
            ClearInfoTransBGs();
            GRInfoTrans5.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoTrans6(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetInfoAlpha = 6;
            CreateSettings();
            ClearInfoTransBGs();
            GRInfoTrans6.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoTrans7(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetInfoAlpha = 7;
            CreateSettings();
            ClearInfoTransBGs();
            GRInfoTrans7.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoTrans8(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetInfoAlpha = 8;
            CreateSettings();
            ClearInfoTransBGs();
            GRInfoTrans8.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoTrans9(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetInfoAlpha = 9;
            CreateSettings();
            ClearInfoTransBGs();
            GRInfoTrans9.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Rahmen Größe hintergründe zurücksetzen
        void ClearInfoTransBGs()
        {
            GRInfoTrans0.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoTrans1.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoTrans2.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoTrans3.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoTrans4.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoTrans5.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoTrans6.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoTrans7.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoTrans8.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoTrans9.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
        }


        //Info Bereich Größe erstellen
        private void BtnInfoSize0(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InfoSize = "0";
            CreateSettings();
            ClearInfoSizeBGs();
            GRInfoSize0.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoSize1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InfoSize = "1";
            CreateSettings();
            ClearInfoSizeBGs();
            GRInfoSize1.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoSize2(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InfoSize = "2";
            CreateSettings();
            ClearInfoSizeBGs();
            GRInfoSize2.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoSize3(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InfoSize = "3";
            CreateSettings();
            ClearInfoSizeBGs();
            GRInfoSize3.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        private void BtnInfoSize4(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InfoSize = "4";
            CreateSettings();
            ClearInfoSizeBGs();
            GRInfoSize4.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            //Lockscreenbild wechseln
            CreateImage();
            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }
        //Rahmen Größe hintergründe zurücksetzen
        void ClearInfoSizeBGs()
        {
            GRInfoSize0.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoSize1.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoSize2.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoSize3.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            GRInfoSize4.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
        }


        //InfoTop umstellen
        private void BtnInfoTop(object sender, RoutedEventArgs e)
        {
            if (InfoTop == true)
            {
                InfoTop = false;
                CreateSettings();
                TBInfoTop.Content = Lockscreen_Swap.AppResx.Off;
            }
            else
            {
                InfoTop = true;
                CreateSettings();
                TBInfoTop.Content = Lockscreen_Swap.AppResx.On;
            }

            //Neues LockscreenBild anzeigen
            GRImage.Margin = new Thickness(0, 0, 0, 0);
            MenuOpen = true;
        }


        //Image Menü verbergen
        private void HideImage(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GRImage.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
        }
        //*********************************************************************************************************









        //Menü Buttons
        //*********************************************************************************************************
        //Menü Variabeln
        //-----------------------------------------------------------------------------------------------------------------
        //Gibt an ob Menü offen ist
        bool MenuOpen = false;

        //Folder Menü Variabeln
        int SelectedFolder = -1;
        private byte tbyte;
        //-----------------------------------------------------------------------------------------------------------------


        //Menü öffnen
        //-----------------------------------------------------------------------------------------------------------------
        private void BtnOpenFolderMenu(object sender, SelectionChangedEventArgs e)
        {
            //Prüfen ob Menü bereits offen
            if (MenuOpen == false)
            {
                //Index auswählen
                SelectedFolder = Convert.ToInt32(LBFolders.SelectedIndex);
                TBFolderMenuName.Text = (datalist[SelectedFolder] as ClassFolders).name;
                TBFolderMenuPictures.Text = (datalist[SelectedFolder] as ClassFolders).images;

                //Menü öffnen
                GRFolderMenu.Margin = new Thickness(0, 0, 0, 0);
                //Angeben das ein Menü offen ist
                MenuOpen = true;

                //Auswahl aufheben
                try
                {
                    LBFolders.SelectedIndex = -1;
                }
                catch
                {
                }
            }
        }
        //-----------------------------------------------------------------------------------------------------------------


        //Folder bearbeiten
        //-----------------------------------------------------------------------------------------------------------------
        private void BtnEditFolder(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Style auswählen
            //string tempFolder = (datalist[SelectedFolder] as ClassFolders).name;

            //Editor öffnen
            NavigationService.Navigate(new Uri("/Pages/EditFolder.xaml?folder=" + SelectedFolder, UriKind.Relative));
        }
        //-----------------------------------------------------------------------------------------------------------------


        //Folder umbenennen
        //-----------------------------------------------------------------------------------------------------------------
        private void BtnRemaneFolder(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Style auswählen
            //string tempFolder = (datalist[SelectedFolder] as ClassFolders).name;

            //Editor öffnen
            NavigationService.Navigate(new Uri("/Pages/RenameFolder.xaml?folder=" + SelectedFolder, UriKind.Relative));
        }
        //-----------------------------------------------------------------------------------------------------------------


        //Folder kopieren
        //-----------------------------------------------------------------------------------------------------------------
        private void BtnCopyFolder(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Style auswählen
            //string tempFolder = (datalist[SelectedFolder] as ClassFolders).name;
            //Editor öffnen
            NavigationService.Navigate(new Uri("/Pages/CopyFolder.xaml?folder=" + SelectedFolder, UriKind.Relative));
        }
        //-----------------------------------------------------------------------------------------------------------------


        //Folder löschen
        //-----------------------------------------------------------------------------------------------------------------
        private void BtnDeleteFolder(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Style auswählen
            string tempFolder = (datalist[SelectedFolder] as ClassFolders).name;
            if (MessageBox.Show(Lockscreen_Swap.AppResx.Z02_DELETE + " " + tempFolder + "?", Lockscreen_Swap.AppResx.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {

                //Ordner löschen
                DeleteDirectory("Folders/" + tempFolder + "/");
                //Thumbnail Ordner löschen
                DeleteDirectory("Thumbs/" + tempFolder + "/");
                //Image Datei löschen
                file.DeleteFile("Thumbs/" + tempFolder + ".dat");
                //FolderDatei neu erstellen
                FoldersAll = "/";
                string[] Folders = file.GetDirectoryNames("/Thumbs/");
                int FoldersC = Folders.Count();
                for (int i = 0; i < FoldersC; i++)
                {
                    FoldersAll += Folders[i] + "/";
                }
                //Neue Ordner Datei Erstellen
                filestream = file.CreateFile("Folders.dat");
                sw = new StreamWriter(filestream);
                sw.WriteLine(FoldersAll);
                sw.Flush();
                filestream.Close();

                //Folders neu laden
                CreateFolders();
                //Menü schließen
                GRFolderMenu.Margin = new Thickness(-600, 0, 0, 0);
                MenuOpen = false;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------


        //Folder erstellen
        //-----------------------------------------------------------------------------------------------------------------
        private void BtnCreateFolder(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Editor öffnen
            NavigationService.Navigate(new Uri("/Pages/CreateFolder.xaml", UriKind.Relative));
        }
        //-----------------------------------------------------------------------------------------------------------------


        //Ordner mit gesamten Inhalt löschen
        //---------------------------------------------------------------------------------------------------------
        public void DeleteDirectory(string target_dir)
        {
            try
            {
                //Ordner löschen
                IsolatedStorageFile file2 = IsolatedStorageFile.GetUserStoreForApplication();
                string[] files = file2.GetFileNames(target_dir);
                //string[] dirs = file11.GetDirectoryNames(target_dir);
                foreach (string file in files)
                {
                    file2.DeleteFile(target_dir + file);
                }
                file2.DeleteDirectory(target_dir);
            }
            catch
            {
            }
        }
        //---------------------------------------------------------------------------------------------------------
        //*********************************************************************************************************










        //Button Share
        //---------------------------------------------------------------------------------------------------------
        private void BtnShareFacebook(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Prüfen ob Bild vorhanden
            if (MessageBox.Show(Lockscreen_Swap.AppResx.Z03_NoteShare, Lockscreen_Swap.AppResx.Notification, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //Bild verkleinern, speichern und sharen
                var bmp = new WriteableBitmap(this.ShowImage, null);
                var width = (int)bmp.PixelWidth;
                var height = (int)bmp.PixelHeight;
                using (var ms = new MemoryStream(width * height * 4))
                {
                    bmp.SaveJpeg(ms, width, height, 0, 100);
                    ms.Seek(0, SeekOrigin.Begin);
                    var lib = new MediaLibrary();
                    var picture = lib.SavePicture(string.Format("test.jpg"), ms);
                    var task = new ShareMediaTask();
                    task.FilePath = picture.GetPath();
                    task.Show();
                }
            }
        }
        //---------------------------------------------------------------------------------------------------------










        //Button Profil
        //---------------------------------------------------------------------------------------------------------
        private void BtnProfil(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Rename Profile öffnen
            NavigationService.Navigate(new Uri("/Pages/Profiles.xaml", UriKind.Relative));
        }
        //---------------------------------------------------------------------------------------------------------









        // Button Lock Screen 10
        //---------------------------------------------------------------------------------------------------------
        private void BtnLS10(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 10.0 Lock Screen Seite anzeigen
            NavigationService.Navigate(new Uri("/Pages/10_0_Lock_Screen.xaml", UriKind.Relative));
        }
        //---------------------------------------------------------------------------------------------------------









        //Button Erweiterte Einstellungen
        //---------------------------------------------------------------------------------------------------------
        private void BtnExtended(object sender, RoutedEventArgs e)
        {
            //Zu erweiterten Einstellungen gehen
            NavigationService.Navigate(new Uri("/Pages/Components.xaml", UriKind.Relative));
        }
        //---------------------------------------------------------------------------------------------------------


        //Button Cycle Tile erstellen
        //---------------------------------------------------------------------------------------------------------
        private void BtnCycleTile(object sender, RoutedEventArgs e)
        {
            //Tile erstellen
            if (oTile != null && oTile.NavigationUri.ToString().Contains("flip"))
            {
            }
            //Wenn Flip Tile noch nicht erstellt wurde //Flip Tile erstellen
            else
            {
                // once it is created flip tile
                Uri tileUri = new Uri("/MainPage.xaml?tile=flip", UriKind.Relative);
                ShellTileData tileData = this.CreateCycleTileData();
                ShellTile.Create(tileUri, tileData, true);
            }
            TBCycleTileHeader.Visibility = System.Windows.Visibility.Collapsed;
            TBCycleTile.Visibility = System.Windows.Visibility.Collapsed;
        }

        private ShellTileData CreateCycleTileData()
        {
            return new CycleTileData()
                {
                    Title = "",
                    SmallBackgroundImage = new Uri("Icon.png", UriKind.Relative),
                };
        }
        //---------------------------------------------------------------------------------------------------------


        //Button Logo Start
        //---------------------------------------------------------------------------------------------------------
        private void BtnSetLogo(object sender, RoutedEventArgs e)
        {
            //CycleTile Einstellungen erstellen
            if (SetLogoStart == true)
            {
                SetLogoStart = false;
                TBSetLogo.Content = Lockscreen_Swap.AppResx.Off;
            }
            else
            {
                SetLogoStart = true;
                TBSetLogo.Content = Lockscreen_Swap.AppResx.On;
            }
            //Einstellungen speichern
            CreateSettings();
        }
        //---------------------------------------------------------------------------------------------------------
        









        //About Buttons
        //*********************************************************************************************************

        //Button Buy
        //---------------------------------------------------------------------------------------------------------
        private void BtnBuy(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MarketplaceDetailTask _marketPlaceDetailTask = new MarketplaceDetailTask();
            _marketPlaceDetailTask.Show();
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Rate
        //---------------------------------------------------------------------------------------------------------
        private void BtnRate(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MarketplaceReviewTask review = new MarketplaceReviewTask();
            review.Show();
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Other Apps
        //---------------------------------------------------------------------------------------------------------
        private void BtnOther(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MarketplaceSearchTask marketplaceSearchTask = new MarketplaceSearchTask();
            marketplaceSearchTask.SearchTerms = "xtrose";
            marketplaceSearchTask.Show();
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Facebook
        //---------------------------------------------------------------------------------------------------------
        private void BtnFacebook(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var wb = new WebBrowserTask();
            wb.URL = "http://www.facebook.com/xtrose.xtrose";
            wb.Show();
        }
        //---------------------------------------------------------------------------------------------------------





        //Bewertungsaufruf
        //---------------------------------------------------------------------------------------------------------
        //Bewerten
        private void BtnRateRate_click(object sender, RoutedEventArgs e)
        {
            //Zu bewertungen gehen
            MarketplaceReviewTask review = new MarketplaceReviewTask();
            review.Show();
            //Rate File löschen
            file.DeleteFile("Settings/RateReminder.txt");
            //Rate verbergen
            GRRate.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
        }
        //Später
        private void BtnRateLater_click(object sender, RoutedEventArgs e)
        {
            //Rate File neu erstellen
            DateTime datetime = DateTime.Now;
            datetime = datetime.AddDays(4);
            filestream = file.CreateFile("Settings/RateReminder.txt");
            sw = new StreamWriter(filestream);
            sw.WriteLine(datetime.ToString());
            sw.Flush();
            filestream.Close();
            //Rate verbergen
            GRRate.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
        }
        //Nie
        private void BtnRateNever_click(object sender, RoutedEventArgs e)
        {
            //Rate File löschen
            file.DeleteFile("Settings/RateReminder.txt");
            //Rate verbergen
            GRRate.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Support
        //---------------------------------------------------------------------------------------------------------
        private void BtnSupport(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Support.xaml", UriKind.Relative));
            /*
            EmailComposeTask emailcomposer = new EmailComposeTask();
            emailcomposer.To = "xtrose@hotmail.com";
            emailcomposer.Subject = "8.1 Lockscreen Support";
            emailcomposer.Body = "";
            emailcomposer.Show();
            */
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Sprache
        //---------------------------------------------------------------------------------------------------------
        private void BtnLanguage(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Rename Profile öffnen
            NavigationService.Navigate(new Uri("/Pages/Language.xaml", UriKind.Relative));
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Instructions
        //---------------------------------------------------------------------------------------------------------
        private void BtnInstructions(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Instructions.xaml", UriKind.Relative));
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
                GRFolderMenu.Margin = new Thickness(-600, 0, 0, 0);
                GRBGColor.Margin = new Thickness(-600, 0, 0, 0);
                GRFolder.Margin = new Thickness(-600, 0, 0, 0);
                GRFRColor.Margin = new Thickness(-600, 0, 0, 0);
                GRALColor.Margin = new Thickness(-600, 0, 0, 0);
                GRImage.Margin = new Thickness(-600, 0, 0, 0);
                GRRate.Margin = new Thickness(-600, 0, 0, 0);
                //Angeben das Menüs geschlossen sind
                MenuOpen = false;

                //Zurück oder beenden abbrechen
                e.Cancel = true;
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------
    }
}