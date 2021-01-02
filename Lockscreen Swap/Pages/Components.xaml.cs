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
using System.IO.IsolatedStorage;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.ComponentModel;
using ImageTools;
using ImageTools.IO;
using ImageTools.IO.Png;
using System.Text;





namespace Lockscreen_Swap.Pages
{
    public partial class Components : PhoneApplicationPage
    {





        //Timer erstellen
        //---------------------------------------------------------------------------------------------------------
        DispatcherTimer dt = new DispatcherTimer();
        //---------------------------------------------------------------------------------------------------------





        //Variabeln erstellen
        //-----------------------------------------------------------------------------------------------------------------
        //Neue Datenliste erstellen
        ObservableCollection<ClassPictures> datalist = new ObservableCollection<ClassPictures>();
        //List mit installierten Styles
        ObservableCollection<ClassPictures> datalist2 = new ObservableCollection<ClassPictures>();
        //Online Liste erstellen
        ObservableCollection<ClassImagesOnline> datalist3 = new ObservableCollection<ClassImagesOnline>();
        //Filestream erstellen
        IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
        IsolatedStorageFileStream filestream;
        StreamReader sr;
        StreamWriter sw;
        //String der StylesSettings
        string stylesSettings;
        //Alle Ausgewählten Indexe
        string SelectedIndexes = "";
        //Ob Aktion nach Auswahl ausgeführt wird
        bool ActionIfSelect = true;
        //Ob Menü offen ist
        bool MenuOpen = false;
        //String Installed Styles
        string InstalledStyles;

        //Debug Mode
        bool DEBUGMODE = false;

        // String für die Post Variablen
        Dictionary<string, string> post_parameters = new Dictionary<string, string>();
        //-----------------------------------------------------------------------------------------------------------------





        //Wird am Anfang geladen
        //-----------------------------------------------------------------------------------------------------------------
        public Components()
        {
            //Komponenten laden
            InitializeComponent();

            //Timer erstellen
            dt.Stop();
            dt.Interval = new TimeSpan(0, 0, 0, 0, 200);
            dt.Tick += new EventHandler(dt_Tick);

            //Logos wechseln bei weißem Hintergrund
            Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            string temp = Convert.ToString(backgroundColor);
            if (temp != "#FF000000")
            {
                ImgLogo.Source = new BitmapImage(new Uri("Images/Edit.Light.png", UriKind.Relative));
                ImgLogo.Opacity = 0.1;
                ImgImgLoad.Source = new BitmapImage(new Uri("Images/Globe.Light.png", UriKind.Relative));
            }
        }
        //-----------------------------------------------------------------------------------------------------------------





        //Wird bei jedem Aufruf der Seite ausgeführt
        //---------------------------------------------------------------------------------------------------------------------------------
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Prüfen ob Degugg Modus an ist
            if (file.FileExists("DEBUGMODE.txt"))
            {
                DEBUGMODE = true;
            }

            //Bilderliste erstellen
            CreateImages();
            
            //Bilder auswählen
            SelectImages();
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Bilderliste erstellen
        //---------------------------------------------------------------------------------------------------------------------------------
        //Image Liste leeren
        void CreateImages()
        {
            //StyleSettings laden
            filestream = file.OpenFile("Settings/StylesSettings.txt", FileMode.Open);
            sr = new StreamReader(filestream);
            stylesSettings = sr.ReadToEnd();
            filestream.Close();

            //Installierte styles
            filestream = file.OpenFile("Settings/InstalledStyles.txt", FileMode.Open);
            sr = new StreamReader(filestream);
            InstalledStyles = sr.ReadToEnd();
            filestream.Close();
            InstalledStyles = InstalledStyles.Trim(new char[] { '\r', '\n' });

            //Liste installierter Styles erstellen
            string[] sInstalledStyles = Regex.Split(InstalledStyles, ";");
            datalist2.Clear();
            for (int i = 0;i < sInstalledStyles.Count() - 1;i++)
            {
                //Stylename splitten
                string[] sStyle = Regex.Split(sInstalledStyles[i], ".txt");

                //Wenn Bild besteht
                if (file.FileExists("StylesImages/" + sStyle[0] + ".1.png") & file.FileExists("StylesImages/" + sStyle[0] + ".2.png") & file.FileExists("StylesImages/" + sStyle[0] + ".3.png") & file.FileExists("StylesImages/" + sStyle[0] + ".4.png")  & file.FileExists("Styles/" + sStyle[0] + ".txt"))
                {
                    //Bilder laden
                    byte[] data1;
                    using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (IsolatedStorageFileStream isfs = isf.OpenFile("StylesImages/" + sStyle[0] + ".1.png", FileMode.Open, FileAccess.Read))
                        {
                            data1 = new byte[isfs.Length];
                            isfs.Read(data1, 0, data1.Length);
                            isfs.Close();
                        }
                    }
                    MemoryStream ms = new MemoryStream(data1);
                    BitmapImage bi = new BitmapImage();
                    bi.SetSource(ms);
                    //Bilder in Klasse schreiben
                    datalist2.Add(new ClassPictures(sStyle[0], "", sInstalledStyles[i], i, bi));
                }
                //Wenn Bild nicht besteht
                else
                {
                    //Versuchen Bilder und Verzeichnis zu löschen
                    try
                    {
                        file.DeleteFile("StylesImages/" + sStyle[0] + ".1.png");
                    }
                    catch
                    {
                    }
                    try
                    {
                        file.DeleteFile("StylesImages/" + sStyle[0] + ".2.png");
                    }
                    catch
                    {
                    }
                    try
                    {
                        file.DeleteFile("StylesImages/" + sStyle[0] + ".3.png");
                    }
                    catch
                    {
                    }
                    try
                    {
                        file.DeleteFile("StylesImages/" + sStyle[0] + ".4.png");
                    }
                    catch
                    {
                    }
                    //Style Datei löschen
                    try
                    {
                        file.DeleteFile("Styles/" + sStyle[0] + ".txt");
                    }
                    catch
                    {
                    }
                    //Style aus Liste mit installierten Styles löschen
                    InstalledStyles = Regex.Replace(InstalledStyles, sStyle[0] + ".txt;", "");
                    //Installierte Styles neu erstellen
                    filestream = file.CreateFile("Settings/InstalledStyles.txt");
                    sw = new StreamWriter(filestream);
                    sw.WriteLine(InstalledStyles);
                    sw.Flush();
                    filestream.Close();
                }
            }

            //Erstelte Liste binden
            LBInstalled.ItemsSource = datalist2;

            //Selected Styles zurücksetzen
            SelectedIndexes = "";

            //Dataliste leeren
            datalist.Clear();
            
            //Daten aus Ordner wählen            
            string[]images = file.GetFileNames("StylesImages/");

            //Bilder durchlaufen und Auswahl erstellen
            for (int i = 0; i < images.Count(); i++)
            {
                //Name cutten und Prüfvariable erstellen
                string[] sImgName = Regex.Split(images[i], ".png");
                string name = sImgName[0];
                int lname = name.Length;
                string proof = name.Substring(0, (lname - 2));
                string proof2 = name[lname - 1].ToString();
                proof += ".txt," + proof2 + ";";

                //Prüfen ob ausgewählt
                string[] sStylesSettings = Regex.Split(stylesSettings, proof);
                if (sStylesSettings.Count() > 1)
                {
                    //Ausgewählte in SelectedIndexes schreiben
                    SelectedIndexes += i + ";";
                }

                try
                {
                    //Bilder laden
                    byte[] data1;
                    using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (IsolatedStorageFileStream isfs = isf.OpenFile("StylesImages/" + images[i], FileMode.Open, FileAccess.Read))
                        {
                            data1 = new byte[isfs.Length];
                            isfs.Read(data1, 0, data1.Length);
                            isfs.Close();
                        }
                    }
                    MemoryStream ms = new MemoryStream(data1);
                    BitmapImage bi = new BitmapImage();
                    bi.SetSource(ms);

                    //Bilder in Klasse schreiben
                    datalist.Add(new ClassPictures(name, proof, images[i], i, bi));
                }
                catch
                {

                }
            }
            //Daten in Listbox schreiben
            LBImages.ItemsSource = datalist;
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Bilder auswahl erstellen
        //---------------------------------------------------------------------------------------------------------------------------------
        void SelectImages()
        {
            //Angeben das keine Aktion ausgefüht wird beim Selectieren
            ActionIfSelect = false;

            //Split Selected Indexes erstellen
            string[] sSelectedIndexes;

            //Prüfen ob eine Auswahl besteht
            if (SelectedIndexes.Length > 0)
            {
                
                //Selected Indexes zerlegen
                sSelectedIndexes = Regex.Split(SelectedIndexes, ";");


                //Settings durchlaufen und bool erstellen und Bild auswählen
                for (int i = 0; i < (sSelectedIndexes.Count() - 1); i++)
                {
                    LBImages.SelectedItems.Add(LBImages.Items[Convert.ToInt32(sSelectedIndexes[i])]);
                }
            }

            //Angeben das keine Aktion ausgefüht wird beim Selectieren zurücksetzen
            ActionIfSelect = true;
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Bilder auswahl erstellen
        //---------------------------------------------------------------------------------------------------------------------------------
        private void LBImages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Prüfen ob ausgeführt wird
            if (ActionIfSelect == true)
            {
                //Count Ausgewählte Bilder
                int cSelectedItems = LBImages.SelectedItems.Count;

                stylesSettings = "";

                //Schleife erstellen um Dateien aus Datenliste zu wählen
                for (int i = 0; i < cSelectedItems; i++)
                {
                    string Path = 
                    stylesSettings += (LBImages.SelectedItems[i] as ClassPictures).imgThumbPath;
                }

                //StylesSettings erstellen
                filestream = file.CreateFile("Settings/StylesSettings.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine(Convert.ToString(stylesSettings));
                sw.Flush();
                filestream.Close();
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Online Componenten laden
        //---------------------------------------------------------------------------------------------------------------------------------
        //Variabeln
        string SourceWebsite = "http://www.xtrose.com";
        string SourceUrl = "http://www.xtrose.com/xtrose/apps/8_1_lockscreen/files/";
        string StatusTimer = "none";
        string Source = "";
        string CheckSum = "";
        private void BtnConnect(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (StatusTimer == "none")
            {
                //Button umstellen
                TxtConnect.Text = Lockscreen_Swap.AppResx.Z04_ConnectingNow;
                // Post Variablen erstellen
                post_parameters = new Dictionary<string, string>();
                post_parameters.Add("api", "1");
                post_parameters.Add("id", "12");
                post_parameters.Add("s", "1d292lgGiD7bMj0lNblW");
                //Timer starten
                dt.Start();
                //Timer Status angeben
                StatusTimer = "LoadWebsite";
                //Seite versuchen zu erreichen
                GetSourceCode();
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Aktion versuchen Seite zu erreichen und Quelltext in String laden
        //---------------------------------------------------------------------------------------------------------
        //Webseite versuchen zu erreichen
        public void GetSourceCode()
        {
            try
            {
                // Abfrage erstellen
                HttpWebRequest request = HttpWebRequest.CreateHttp(SourceWebsite);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                // POST Method Abfrage starten
                request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
            }
            catch
            {
                //Wenn Webseite nicht erreichbar, Fehlermeldung ausgeben
                MessageBox.Show(Lockscreen_Swap.AppResx.Z04_ConnectionFailed);
                //Zurück zum Start
                //NavigationService.GoBack();
            }
        }



        // POST Method Abfrage
        public void GetRequestStreamCallback(IAsyncResult callbackResult)
        {
            string post_data = "";
            foreach (string key in post_parameters.Keys)
            {
                post_data += HttpUtility.UrlEncode(key) + "="
                      + HttpUtility.UrlEncode(post_parameters[key]) + "&";
            }

            HttpWebRequest request = (HttpWebRequest)callbackResult.AsyncState;
            // End the stream request operation
            Stream postStream = request.EndGetRequestStream(callbackResult);

            // Create the post data
            byte[] byteArray = Encoding.UTF8.GetBytes(post_data);

            // Add the post data to the web request
            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            // Start the web request
            request.BeginGetResponse(new AsyncCallback(handle_response), request);
        }





        //Quelltext in String speichern
        public void handle_response(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;

            try
            {
                if (request != null)
                {
                    using (WebResponse response = request.EndGetResponse(result))
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            //Quelltext laden
                            Source = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
            }
        }
        //---------------------------------------------------------------------------------------------------------





        //Timer, Ablauf aller aktionen
        //---------------------------------------------------------------------------------------------------------
        //Variabeln
        int StartMS = 0;
        int UhrzeiMS = 0;
        //Aktion
        void dt_Tick(object sender, EventArgs e)
        {
            //Styles laden
            if (StatusTimer == "LoadWebsite")
            {
                //Prüfen ob Timeout
                bool TimeOut = false;


                //Aktuelle Uhrzeit Millisekunden erstellen
                DateTime Uhrzeit = DateTime.Now;
                UhrzeiMS = (Uhrzeit.Hour * 3600000) + (Uhrzeit.Minute * 60000) + (Uhrzeit.Second * 1000) + Uhrzeit.Millisecond;


                //Prüfen ob StartMS vorhanden
                if (StartMS == 0)
                {
                    StartMS = UhrzeiMS;
                }
                else
                {
                    if ((StartMS + 10000) < UhrzeiMS)
                    {
                        TimeOut = true;
                    }
                }


                //Preüfen ob Time out
                if (TimeOut == false)
                {
                    //Prüfen ob Quelle geladen
                    if (Source != "")
                    {
                        //wenn feedtemp = feed, Quelltext komplett geladen
                        if (Source == CheckSum)
                        {
                            //Timer Status löschen
                            StatusTimer = "none";
                            //feedtemp löschen
                            CheckSum = "";
                            //StartMS löschen
                            StartMS = 0;
                            //StackPanel verstecken
                            StpConnect.Visibility = System.Windows.Visibility.Collapsed;
                            //Timer Stoppen
                            dt.Stop();
                            //Listbox erstellen
                            CreateOnlineImages();
                        }
                        //wenn feedtemp != feed, Quelltext noch nicht komplett geladen
                        else
                        {
                            //feedtemp zu aktuellem Feed machen
                            CheckSum = Source;
                        }
                    }
                }
                //Bei TimeOut
                else
                {
                    MessageBox.Show(Lockscreen_Swap.AppResx.Z04_ConnectionTimeOut);
                    //Timer Status löschen
                    StatusTimer = "none";
                    //feedtemp löschen
                    CheckSum = "";
                    //StartMS löschen
                    StartMS = 0;
                    //Button umstellen
                    TxtConnect.Text = Lockscreen_Swap.AppResx.Z04_Connecting;
                    //Timer Stoppen
                    dt.Stop();
                }
            }

            //Bilder downloaden
            if (StatusTimer == "DownloadImages")
            {
                //Datei Herunterladen wenn nicht bereits eine andere runtergeladen wird
                if (DownloadID != TempID & DownloadID < 2)
                {
                    //TempID auf DownloadID stellen
                    TempID = DownloadID;
                    //url aus ausgwälten Bild laden
                    string url = "";
                    if (DownloadID == 0)
                    {
                        url = SourceUrl + OnlineDirectory + ".txt";
                    }
                    else
                    {
                        url = SourceUrl + OnlineDirectory + ".png";
                    }
                    //Downloadstatus ändern
                    TBDownloadStatus.Text = Lockscreen_Swap.AppResx.Z04_Downloading;

                    //Datei öffnen
                    WebClient client = new WebClient();
                    client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadCompleted);
                    client.OpenReadAsync(new Uri(url));

                }

                //Wenn alle Dateien Heruntergeladen sind
                else if (DownloadID == 2)
                {
                    //Download Status verbergen
                    TBDownloadStatus.Visibility = System.Windows.Visibility.Collapsed;
                    //Save Eingabe Hinzufügen
                    SPSaveImages.Visibility = System.Windows.Visibility.Visible;
                    //Timer Status ändern
                    StatusTimer = "none";
                    //Timer stoppen
                    dt.Stop();
                }
            }
        }
        //---------------------------------------------------------------------------------------------------------





        //Liste der Styles online erstellen
        //---------------------------------------------------------------------------------------------------------
        //Variabekn
        bool ComponentsOnline = false;

        //Aktion
        void CreateOnlineImages()
        {
            //Quelle verarbeiten
            Source = Source.TrimEnd(new char[] { '\r', '\n' });

            //Wenn Inhalte verfügbar
            if (Source != "none")
            {
                //Dateienliste leeren
                datalist3.Clear();

                //Angeben das Komponenten online sind
                ComponentsOnline = true;
                //Komponenten aufteilen
                string[] SourceSplit = Regex.Split(Source, ";;;");
                int cSourceSplit = SourceSplit.Count();
                for (int i = 0; i < (cSourceSplit - 1); i++)
                {
                    //Datensatz zerlegen
                    SourceSplit[i] = SourceSplit[i].Trim(new char[] { '\r', '\n' });
                    string[] SourceSplitSplit = Regex.Split(SourceSplit[i], ";");
                    //Prüfen ob bereits vorhander
                    string[] tempIS = Regex.Split(InstalledStyles, SourceSplitSplit[2] + ".txt;");
                    if (tempIS.Count() <= 1)
                    {
                        //Prüfen ob Style zum debuggen hochgeladen
                        string[] TempSP = Regex.Split(SourceSplitSplit[2], "DEBUG");
                        if (TempSP.Count() <= 1 | DEBUGMODE == true)
                        {
                            //Prüfen ob bereits installiert
                            datalist3.Add(new ClassImagesOnline(SourceSplitSplit[0], SourceSplitSplit[1], SourceSplitSplit[2]));
                        }
                    }
                }
                //Listbox neu erstellen
                LBOnlineImages.ItemsSource = datalist3;
            }
            //Wenn keine Inhalte verfügbar
            else
            {
                //Dateienliste leeren
                datalist3.Clear();
                //Überschrift erstellen
                datalist3.Add(new ClassImagesOnline(Lockscreen_Swap.AppResx.Z04_NoComponentsOnline, "", ""));
                //Listbox neu erstellen
                LBOnlineImages.ItemsSource = datalist3;
            }
        }
        //---------------------------------------------------------------------------------------------------------





        //Online Komponenten laden
        //---------------------------------------------------------------------------------------------------------
        //Variabeln
        bool OpenComponent = true;
        string OnlineName = "";
        string OnlineDirectory = "";
        int DownloadID = 0;

        //Aktionen
        private void BtnOpenImageOnline(object sender, SelectionChangedEventArgs e)
        {
            //Prüfen Komponenten online sind
            if (ComponentsOnline == true)
            {
                //Prüfen ob Komponenten Fenster geöffnet wird
                if (OpenComponent == true)
                {
                    //Index laden
                    int SI = LBOnlineImages.SelectedIndex;

                    //Bilderset öffnen
                    OnlineName = (datalist3[SI] as ClassImagesOnline).name;
                    TBImagesOnlineName.Text = OnlineName;
                    OnlineDirectory = (datalist3[SI] as ClassImagesOnline).directory;

                    //Alte Bilder zurücksetzen
                    ImagesOnline1.Source = null;
                    ImagesOnline2.Source = null;
                    ImagesOnline3.Source = null;
                    ImagesOnline4.Source = null;

                    //Download Status sichtbar machen
                    TBDownloadStatus.Visibility = System.Windows.Visibility.Visible;
                    //Save Eingabe verbergen
                    SPSaveImages.Visibility = System.Windows.Visibility.Collapsed;

                    //Timer starten
                    DownloadID = 0;
                    TempID = 5000;
                    StatusTimer = "DownloadImages";
                    dt.Start();

                    //Menü öffnen
                    GRImagesOnline.Margin = new Thickness(0, 0, 0, 0);
                    //Angeben das ein Menü offen ist
                    MenuOpen = true;

                    //Auswahl aufheben
                    OpenComponent = false;
                    try
                    {
                        LBOnlineImages.SelectedIndex = -1;
                    }
                    catch
                    {
                    }
                    OpenComponent = true;
                }
            }
        }
        //---------------------------------------------------------------------------------------------------------





        //Geladenes Teile vom Internet in den Isolated Storage speichern
        //---------------------------------------------------------------------------------------------------------
        //Variabeln
        int TempID = 5000;
        string FileName = "none";
        BitmapImage bi = new BitmapImage();

        //Aktion
        void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            //Bild vom Internet in den Isolated Storage speichern
            try
            {
                //Dateiname anhand der DownloadID erstellen
                if (DownloadID == 0)
                {
                    //Dateiname erstellen
                    FileName = "txt.txt";
                    //Alte Datei löschen
                    if (file.FileExists("TempStyles/txt.txt"))
                    {
                        file.DeleteFile("TempStyles/txt.txt");
                    }
                }
                else
                {
                    //Dateiname erstellen
                    FileName = "1.png";
                    //Alte Datei löschen
                    if (file.FileExists("TempStyles/1.png"))
                    {
                        file.DeleteFile("TempStyles/1.png");
                    }
                }
                

                //Prüfen ob Downloadordner existiert
                if (file.DirectoryExists("TempStyles"))
                {
                }
                else
                {
                    file.CreateDirectory("TempStyles");
                }

                //Prüfen ob Datei bereits vorhanden
                if (file.FileExists("TempStyles/" + FileName))
                {
                    file.DeleteFile("TempStyles/" + FileName);
                }

                //Datei in Isolated Storage laden
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("TempStyles/" + FileName, System.IO.FileMode.Create, file))
                {
                    byte[] buffer = new byte[1024];
                    while (e.Result.Read(buffer, 0, buffer.Length) > 0)
                    {
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }

                //Nach dem Das Bild heruntergeladen wurde, Bild laden und in Storage schreiben
                if (DownloadID == 1)
                {
                    //Writeable Bitmap erstellen
                    var tempImage = new WriteableBitmap(1, 1);
                    byte[] data1;
                    {
                        using (IsolatedStorageFileStream isfs = file.OpenFile("TempStyles/1.png", FileMode.Open, FileAccess.Read))
                        {
                            data1 = new byte[isfs.Length];
                            isfs.Read(data1, 0, data1.Length);
                            isfs.Close();
                        }
                    }
                    MemoryStream ms = new MemoryStream(data1);

                    //Bild in WriteableBitmap
                    tempImage.SetSource(ms);
                    ImagesOnline1.Source = tempImage;

                    //Bild Spiegeln
                    tempImage = tempImage.Flip(WriteableBitmapExtensions.FlipMode.Vertical);
                    //Altes Bild löschen
                    if (file.FileExists("TempStyles/2.png"))
                    {
                        file.DeleteFile("TempStyles/2.png");
                    }
                    //Bild speichern
                    Decoders.AddDecoder<PngDecoder>();
                    var img = tempImage.ToImage();
                    var encoder = new PngEncoder();
                    using (var stream = new IsolatedStorageFileStream("TempStyles/2.png", FileMode.Create, file))
                    {
                        encoder.Encode(img, stream);
                        stream.Close();
                    }
                    //Bild ausgeben
                    ImagesOnline2.Source = tempImage;


                    //Bild Spiegeln
                    tempImage = tempImage.Flip(WriteableBitmapExtensions.FlipMode.Horizontal);
                    //Altes Bild löschen
                    if (file.FileExists("TempStyles/3.png"))
                    {
                        file.DeleteFile("TempStyles/3.png");
                    }
                    //Bild speichern
                    Decoders.AddDecoder<PngDecoder>();
                    img = tempImage.ToImage();
                    encoder = new PngEncoder();
                    using (var stream = new IsolatedStorageFileStream("TempStyles/3.png", FileMode.Create, file))
                    {
                        encoder.Encode(img, stream);
                        stream.Close();
                    }
                    //Bild ausgeben
                    ImagesOnline3.Source = tempImage;


                    //Bild Spiegeln
                    tempImage = tempImage.Flip(WriteableBitmapExtensions.FlipMode.Vertical);
                    //Altes Bild löschen
                    if (file.FileExists("TempStyles/4.png"))
                    {
                        file.DeleteFile("TempStyles/4.png");
                    }
                    //Bild speichern
                    Decoders.AddDecoder<PngDecoder>();
                    img = tempImage.ToImage();
                    encoder = new PngEncoder();
                    using (var stream = new IsolatedStorageFileStream("TempStyles/4.png", FileMode.Create, file))
                    {
                        encoder.Encode(img, stream);
                        stream.Close();
                    }
                    //Bild ausgeben
                    ImagesOnline4.Source = tempImage;
                }
            }
            catch
            {
            }
            //DownloadID erhöhen um nächstes Bild aus aus dem Netz herunterzuladen
            DownloadID++;
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Style speichern
        //---------------------------------------------------------------------------------------------------------
        private void BtnSaveImagesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //Bilder in neuen Style kopieren
                for (int i = 1; i <= 4; i++)
                {
                    if (file.FileExists("StylesImages/" + OnlineDirectory + "." + i + ".png"))
                    {
                        file.DeleteFile("StylesImages/" + OnlineDirectory + "." + i + ".png");
                    }
                    file.CopyFile("TempStyles/" + i + ".png", "StylesImages/" + OnlineDirectory + "." + i + ".png");
                }
                //Style Datei kopieren
                if (file.FileExists("Styles/" + OnlineDirectory + ".txt"))
                {
                    file.DeleteFile("Styles/" + OnlineDirectory + ".txt");
                }
                file.CopyFile("TempStyles/txt.txt", "Styles/" + OnlineDirectory + ".txt");

                //InastalledStyles ändern und speichern
                InstalledStyles += OnlineDirectory + ".txt;";
                filestream = file.CreateFile("Settings/InstalledStyles.txt");
                sw = new StreamWriter(filestream);
                sw.WriteLine(InstalledStyles);
                sw.Flush();
                filestream.Close();
                
                //Bilderliste neu erstellen
                ActionIfSelect = false;
                CreateImages();
                ActionIfSelect = true;
                SelectImages();

                //Online Styles neu erstellen
                if (Source != "")
                {
                    CreateOnlineImages();
                }

                //Menü verbergen
                SPSaveImages.Visibility = System.Windows.Visibility.Collapsed;
                GRImagesOnline.Margin = new Thickness(-600, 0, 0, 0);
                MenuOpen = false;
            }
            catch
            {
                MessageBox.Show(Lockscreen_Swap.AppResx.Z04_Exists);
            }
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Style löschen
        //---------------------------------------------------------------------------------------------------------
        private void LBInstalled_Uninstall(object sender, SelectionChangedEventArgs e)
        {
            //Prüfen ob ausgeführt wird
            if (ActionIfSelect == true)
            {
                //Prüfen ob noch mehr als ein Style vorhanden ist
                string[] sStyles = Regex.Split(InstalledStyles, ";");
                if (sStyles.Count() > 2)
                {
                    //Abfrage ob Rahmen gelöscht werden soll
                    if (MessageBox.Show(Lockscreen_Swap.AppResx.Z04_DeleteFrame, Lockscreen_Swap.AppResx.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        //Index auswählen
                        int SI = LBInstalled.SelectedIndex;

                        //Pfad auswählen
                        string path = (datalist2[SI] as ClassPictures).imgPath;

                        //Bilder löschen
                        for (int i = 1; i <= 4; i++)
                        {
                            //Bilder löschen
                            file.DeleteFile("StylesImages/" + path + "." + i + ".png");
                            //Style aus StylesSettings löschen
                            stylesSettings = Regex.Replace(stylesSettings, path + ".txt," + i + ";", "");
                            stylesSettings = stylesSettings.TrimEnd(new char[] { '\r', '\n' });
                        }
                        //StylesSettings erstellen
                        filestream = file.CreateFile("Settings/StylesSettings.txt");
                        sw = new StreamWriter(filestream);
                        sw.WriteLine(Convert.ToString(stylesSettings));
                        sw.Flush();
                        filestream.Close();

                        //Style Datei löschen
                        file.DeleteFile("Styles/" + path + ".txt");

                        //InastalledStyles ändern und speichern
                        InstalledStyles = Regex.Replace(InstalledStyles, path + ".txt;", "");
                        InstalledStyles = InstalledStyles.TrimEnd(new char[] { '\r', '\n' });
                        filestream = file.CreateFile("Settings/InstalledStyles.txt");
                        sw = new StreamWriter(filestream);
                        sw.WriteLine(InstalledStyles);
                        sw.Flush();
                        filestream.Close();

                        //Bilderliste neu erstellen
                        ActionIfSelect = false;
                        try
                        {
                            LBInstalled.SelectedIndex = -1;
                        }
                        catch
                        {
                        }
                        CreateImages();
                        ActionIfSelect = true;
                        SelectImages();

                        //Online Styles neu erstellen
                        if (Source != "")
                        {
                            CreateOnlineImages();
                        }
                    }
                }

                //Wenn nur noch ein Style vorhanden
                else
                {
                    //Benachrichtigung ausgeben
                    MessageBox.Show(Lockscreen_Swap.AppResx.Z04_LastFrame);
                }
            }
        }
        //---------------------------------------------------------------------------------------------------------




        //Alle vorinstallierten Styles wiederherstellen
        //---------------------------------------------------------------------------------------------------------
        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            //Abfrage ob wiederhergestellt werden soll
            if (MessageBox.Show(Lockscreen_Swap.AppResx.Z04_RestoreNote, Lockscreen_Swap.AppResx.Notification, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //Styles wiederherstellen
                for (int i = 1; i <= 9; i++)
                {
                    //Prüfen ob Style vorhanden ist
                    if (!file.FileExists("Styles/000000" + i + ".txt"))
                    {
                        //Style kopieren
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

                        //Bilder in Styles Images kppieren
                        var tempImage = new WriteableBitmap(1, 1);

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

                    //Style in InstalledStyles schreiben
                    InstalledStyles += "000000" + i + ".txt;";
                    InstalledStyles = InstalledStyles.Trim(new char[] { '\r', '\n' });
                    //Installierte Styles speichern
                    filestream = file.CreateFile("Settings/InstalledStyles.txt");
                    sw = new StreamWriter(filestream);
                    sw.WriteLine(InstalledStyles);
                    sw.Flush();
                    filestream.Close();
                }

                //Bilderliste neu erstellen
                ActionIfSelect = false;
                try
                {
                    LBInstalled.SelectedIndex = -1;
                }
                catch
                {
                }
                CreateImages();
                ActionIfSelect = true;
                SelectImages();

                //Online Styles neu erstellen
                if (Source != "")
                {
                    CreateOnlineImages();
                }

            }
        }
        //---------------------------------------------------------------------------------------------------------





        //Back Button
        //---------------------------------------------------------------------------------------------------------------------------------
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            //Prüfen ob Menü offen ist und alle Menüs schließen
            if (MenuOpen == true)
            {
                //Menüs schließen
                GRImagesOnline.Margin = new Thickness(-600, 0, 0, 0);

                //Angeben das Menüs geschlossen sind
                MenuOpen = false;

                //Zurück oder beenden abbrechen
                e.Cancel = true;
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------




        
    }
}