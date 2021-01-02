using System;
using System.Diagnostics;
//using System.Collections.Generic;
using System.Linq;
//using System.Net;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Navigation;
//using Microsoft.Phone.Controls;
//using Microsoft.Phone.Shell;
//using MyScheduledTaskAgent;
//Zum laden von Qellcodes und zum erstellen und auslesem von Dateien
using System.IO;
//Zum erstellen und auslesem von Dateien
using System.IO.IsolatedStorage;
//Zum erweiterten schneiden von strings
using System.Text.RegularExpressions;
//Zum speichern von Bildern in den Isolated Storage
using System.Windows.Media.Imaging;
//using System.Windows.Resources;
//using Microsoft.Xna.Framework.Media;
//using Microsoft.Phone.Tasks;
//using Microsoft.Phone;
using System.Windows.Media;
//Um Battery Leistung auszulesen
//using Windows.Phone.Devices.Power;

//Um auszulesen ob Handy geladen wird;
//using Microsoft.Phone.Info;
//Background Agent
using Microsoft.Phone.Scheduler;
using System.Threading;
using Microsoft.Phone.Shell;
using System.Collections.Generic;





namespace MyScheduledTaskAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {










        //Wird am Anfang ausgeführt
        //--------------------------------------------------------------------------------------------------------------------
        static ScheduledAgent()
        {


            //Vorbereitung
            //-----------------------------------------------------------------------------------------------------------------

            //Variabeln erstellen
            bool FullVersion = false;
            bool Run = true;
            string Settings;
            string SetFolder = "*";
            string SetBgColor = "#FF000000";
            string SetFrameColor = "NO";
            string SetAlphaColor = "NO";
            int SetFrameSize = 0;
            int SetInfoAlpha = 0;
            int SwapImage = 1;
            string FoldersAll = "/";
            string[] Folders;
            string ImagesAll = "/";
            string DatFile;
            string InfoSize = "4";
            bool InfoTop = false;
            bool SetCycleTile = true;
            bool SetLogoStart = true;
            //Anzahl der Styles
            int cStyles = 4;
            //IsoStore file erstellen
            IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
            //bool IsLockscreenApp = true;
            DateTime dt = DateTime.Now;
            DateTime dt_Now = DateTime.Now;
            //Prüfen ob CycleTile schon erstellt wurde
            ShellTile oTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("flip".ToString()));

            //Prüfen ob momentan Lockscreen App
            bool IsLockscreenApp = true;
            if (Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication)
            {
                //IsLockscreenApp auf true stellen
                IsLockscreenApp = true;
            }
            else
            {
                IsLockscreenApp = false;
            }


            //FullVersion laden
            IsolatedStorageFileStream filestream = file.OpenFile("Settings/FullVersion.txt", FileMode.Open);
            StreamReader sr = new StreamReader(filestream);
            string temp = sr.ReadToEnd();
            filestream.Close();
            int temp2 = Convert.ToInt32(temp);
            if (temp2 == 1)
            {
                FullVersion = true;
            }

            if (FullVersion == false)
            {
                //FirstTime laden //Zeit der Installation
                filestream = file.OpenFile("Settings/FirstTime.txt", FileMode.Open);
                sr = new StreamReader(filestream);
                temp = sr.ReadToEnd();
                filestream.Close();
                dt = Convert.ToDateTime(temp);

                //Prüfen ob Trial Zeit abgelaufen
                TimeSpan diff = dt_Now - dt;
                int MinToGo = 1440 - Convert.ToInt32(diff.TotalMinutes);
                //Wenn Zeit abgelaufen
                if (MinToGo <= 0)
                {
                    //Angeben das Bild auf Leer gestellt wird
                    Run = false;
                }
            }


            //Settings laden //Einstellungen
            filestream = file.OpenFile("Settings/Settings.txt", FileMode.Open);
            sr = new StreamReader(filestream);
            Settings = sr.ReadToEnd();
            filestream.Close();


            //Folders.dat laden //Ordner Dateien
            filestream = file.OpenFile("Folders.dat", FileMode.Open);
            sr = new StreamReader(filestream);
            FoldersAll = sr.ReadToEnd();
            filestream.Close();
            FoldersAll = FoldersAll.TrimEnd(new char[] { '\r', '\n' });


            //Einstellungen umsetzen
            string[] aSettings = Regex.Split(Settings, ";");
            //Ordner
            SetFolder = aSettings[0];
            //Hintergrundfarbe
            SetBgColor = aSettings[1];
            //Rahmenfarbe
            SetFrameColor = aSettings[2];
            //Alphafarbe
            SetAlphaColor = aSettings[3];
            //Rahmen Größe
            SetFrameSize = Convert.ToInt32(aSettings[4]);
            //Info Bereich Alpha
            SetInfoAlpha = Convert.ToInt32(aSettings[5]);
            //Swap Image
            SwapImage = Convert.ToInt32(aSettings[6]);
            //InfoSize
            InfoSize = Convert.ToString(aSettings[7]);
            //InfoTop
            InfoTop = Convert.ToBoolean(aSettings[8]);
            //Logo beim Start
            SetLogoStart = Convert.ToBoolean(aSettings[9]);


            //StyleSettings laden
            filestream = file.OpenFile("Settings/StylesSettings.txt", FileMode.Open);
            sr = new StreamReader(filestream);
            string stylesSettings = sr.ReadToEnd();
            filestream.Close();

            //Variablen erstellen
            string iStyle = "1.txt";
            int iMirrow = 1;
            Random rand = new Random();

            //Prüfen ob Settings vorhanden
            stylesSettings = stylesSettings.Trim(new char[] { '\r', '\n' });
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
            //Dat File
            string datFile = "";





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
                    string[] tFolders = Regex.Split(FoldersAll, "/" + SetFolder + "/");
                    if (tFolders.Count() <= 1)
                    {
                        //Settings neu erstellen
                        SetFolder = "*";
                        Settings = SetFolder + ";" + SetBgColor + ";" + SetFrameColor + ";" + SetAlphaColor + ";" + SetFrameSize + ";" + SetInfoAlpha + ";" + SwapImage + ";" + InfoSize + ";" + Convert.ToString(InfoTop) + ";" + Convert.ToString(SetLogoStart) + ";";
                        //Einstellungen speichern
                        filestream = file.CreateFile("Settings/Settings.txt");
                        StreamWriter sw = new StreamWriter(filestream);
                        sw.WriteLine(Settings);
                        sw.Flush();
                        filestream.Close();
                    }
                    else
                    //Prüfen ob Dateien in Ordner
                    {
                        //Images.dat laden //Bilder Dateien
                        filestream = file.OpenFile("Thumbs/" + SetFolder + ".dat", FileMode.Open);
                        sr = new StreamReader(filestream);
                        ImagesAll = sr.ReadToEnd();
                        filestream.Close();
                        ImagesAll = ImagesAll.TrimEnd(new char[] { '\r', '\n' });

                        string[] tImages = Regex.Split(ImagesAll, "/");
                        if (tImages.Count() < 3)
                        {
                            //Settings neu erstellen
                            SetFolder = "*";
                            Settings = Settings = SetFolder + ";" + SetBgColor + ";" + SetFrameColor + ";" + SetAlphaColor + ";" + SetFrameSize + ";" + SetInfoAlpha + ";" + SwapImage + ";" + InfoSize + ";" + Convert.ToString(InfoTop) + ";" + Convert.ToString(SetLogoStart) + ";";
                            //Einstellungen speichern
                            filestream = file.CreateFile("Settings/Settings.txt");
                            StreamWriter sw = new StreamWriter(filestream);
                            sw.WriteLine(Settings);
                            sw.Flush();
                            filestream.Close();
                        }
                        else
                        {
                            //LoadFolder erstellen
                            loadFolder = "Folders/" + SetFolder + "/";
                            //Images.dat laden //Bilder Dateien
                            filestream = file.OpenFile("Thumbs/" + SetFolder + ".dat", FileMode.Open);
                            sr = new StreamReader(filestream);
                            ImagesAll = sr.ReadToEnd();
                            filestream.Close();
                            ImagesAll = ImagesAll.TrimEnd(new char[] { '\r', '\n' });
                            noImage = false;
                        }
                    }
                }


                //Wenn Loading Folder nicht besteht //Random
                if (loadFolder == "*")
                {
                    //Ordner Liste erstellen
                    string tFolders = "";
                    string[] tf = Regex.Split(FoldersAll, "/");
                    string[] lFolders = new string[(tf.Count() - 2)];
                    for (int i21 = 1; i21 < tf.Count() - 1; i21++)
                    {
                        lFolders[(i21 - 1)] = tf[i21];
                    }

                    //Ordner Anzahl erstellen
                    int cFolders = lFolders.Count();

                    //Ordner durchlaufen und prüfen ob Bilder vorhanden
                    for (int i2 = 0; i2 < cFolders; i2++)
                    {
                        //Images.dat laden //Bilder Dateien
                        filestream = file.OpenFile("/Thumbs/" + lFolders[i2] + ".dat", FileMode.Open);
                        sr = new StreamReader(filestream);
                        ImagesAll = sr.ReadToEnd();
                        filestream.Close();
                        ImagesAll = ImagesAll.TrimEnd(new char[] { '\r', '\n' });

                        //Bilder aufsplitten
                        string[] tImages = Regex.Split(ImagesAll, "/");
                        if (tImages.Count() > 2)
                        {
                            tFolders += i2 + ";";
                        }
                    }

                    if (tFolders.Length > 0)
                    {
                        string[] tlFolders = Regex.Split(tFolders, ";");
                        int ctlFolders = tlFolders.Count() - 1;
                        int iFolder = rand.Next(1, tlFolders.Count());

                        int temp22 = Convert.ToInt32(tlFolders[iFolder - 1]);
                        //loadFolder erstellen
                        loadFolder = "Folders/" + lFolders[temp22] + "/";
                        datFile = "Thumbs/" + lFolders[temp22] + ".dat";
                        //Images.dat laden //Bilder Dateien
                        filestream = file.OpenFile(datFile, FileMode.Open);
                        sr = new StreamReader(filestream);
                        ImagesAll = sr.ReadToEnd();
                        filestream.Close();
                        ImagesAll = ImagesAll.TrimEnd(new char[] { '\r', '\n' });
                        noImage = false;
                    }
                }

                //Wenn Bilder existieren
                if (noImage == false & Run == true)
                {
                    //Alle Bilder aus Ordner laden
                    string[] tI = Regex.Split(ImagesAll, "/");
                    string[] tImages = new string[(tI.Count() - 2)];
                    for (int i27 = 1; i27 < (tI.Count() - 1); i27++)
                    {
                        tImages[(i27 - 1)] = tI[i27];
                    }
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
            //-----------------------------------------------------------------------------------------------------------------





            //Bild erstellen
            //-----------------------------------------------------------------------------------------------------------------
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {

                //Bild erstellen
                var Background = new WriteableBitmap(480, 800);

                //Hintergrundfarbe erstellen
                Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
                if (SetBgColor != "NO")
                {
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

                    //Prüfen welches File geladen wird
                    string LoadFile = lImages[i];
                    if (size == 100)
                    {
                        LoadFile = Regex.Replace("/" + LoadFile, "/Folders/", "/Thumbs/");
                    }

                    //Tile erstellen
                    byte[] tempData;
                    MemoryStream tempMs;
                    var tile = new WriteableBitmap(0, 0);
                    using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (IsolatedStorageFileStream isfs = isf.OpenFile(LoadFile, FileMode.Open, FileAccess.Read))
                        {
                            tempData = new byte[isfs.Length];
                            isfs.Read(tempData, 0, tempData.Length);
                            isfs.Close();
                        }
                    }
                    tempMs = new MemoryStream(tempData);
                    tile = new WriteableBitmap(0, 0);
                    tile.SetSource(tempMs);

                    if (size != 320)
                    {
                        tile = tile.Resize(size, size, WriteableBitmapExtensions.Interpolation.Bilinear);
                    }

                    //Tile in Bild schneiden
                    Background.Blit(new Rect(x1, y1, size, size), tile, new Rect(0, 0, size, size));

                    //Tempomäre Bilder löschen
                    tempData = null;
                    tempMs = null;
                    tile = null;

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
                    tile = null;
                }

                //InfoTop erstellen
                if (InfoTop == true & SetAlphaColor != "NO")
                {
                    //Tile erstellen
                    var tile = new WriteableBitmap(480, 30);
                    tile.Clear(alphaColor);
                    //Tile in Bild schneiden
                    Background.Blit(new Rect(0, 0, 480, 30), tile, new Rect(0, 0, 480, 30));
                    tile = null;
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
                Settings = SetFolder + ";" + SetBgColor + ";" + SetFrameColor + ";" + SetAlphaColor + ";" + SetFrameSize + ";" + SetInfoAlpha + ";" + SwapImage + ";" + InfoSize + ";" + Convert.ToString(InfoTop) + ";" + Convert.ToString(SetLogoStart) + ";";
                //Einstellungen speichern
                filestream = file.CreateFile("Settings/Settings.txt");
                StreamWriter sw2 = new StreamWriter(filestream);
                sw2.WriteLine(Settings);
                sw2.Flush();
                filestream.Close();

                //Datei in Isolated Storage schreiben
                var isolatedStorageFileStream = file.CreateFile(SaveFile);
                Background.SaveJpeg(isolatedStorageFileStream, 480, 800, 0, 100);
                isolatedStorageFileStream.Close();
                Background = null;

                //Lockscreen erstellen
                string filePathOfTheImage = SaveFile;
                var schema = "ms-appdata:///Local/";
                var uri = new Uri(schema + filePathOfTheImage, UriKind.RelativeOrAbsolute);
                //Wenn Lockscreen App, Lockscreen setzen
                if (IsLockscreenApp == true)
                {
                    Windows.Phone.System.UserProfile.LockScreen.SetImageUri(uri);
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

            });
            //-----------------------------------------------------------------------------------------------------------------
        }
        //--------------------------------------------------------------------------------------------------------------------










        //Tasks die ausgeführt werden
        //--------------------------------------------------------------------------------------------------------------------
        protected override void OnInvoke(ScheduledTask task)
        {
            //PeriodicTask, Periodischer Task
            //****************************************************************************************************
            if (task is PeriodicTask)
            {
            }

            //Task erneut ausführen nach //Nur Debug Modus
            //ScheduledActionService.LaunchForTest("PeriodicAgent", TimeSpan.FromSeconds(30));

            //Angeben das Tasks alle ausgeführt wurden
            NotifyComplete();
            //****************************************************************************************************
        }
        //--------------------------------------------------------------------------------------------------------------------










    }
}