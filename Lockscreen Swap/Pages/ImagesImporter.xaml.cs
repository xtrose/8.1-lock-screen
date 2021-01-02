using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using System.Windows.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.Windows.Navigation;
using System.IO.IsolatedStorage;





namespace Lockscreen_Swap.Pages
{
    public partial class ImagesImporter : PhoneApplicationPage
    {





        //Wird am Start der Seite geladen
        //---------------------------------------------------------------------------------------------------------
        //Allgemeine Variabeln
        string Folder;
        int ImageCount = 0;
        string ImagesAll = "/";
        //Bild Area Variabeln
        int imgGes = 0;
        int imgStart = 0;
        int imgEnd = 0;
        int imgArea = 0;
        int imgAreaGes = 0;
        string imgPictures = "all";
        //Neue Datenliste erstellen
        ObservableCollection<ClassMediaImage> datalist = new ObservableCollection<ClassMediaImage>();
        //---------------------------------------------------------------------------------------------------------





        //Wird am Start der Seite geladen
        //---------------------------------------------------------------------------------------------------------
        public ImagesImporter()
        {
            //Komponenten laden
            InitializeComponent();

            //Bilder ändern
            Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            string temp = Convert.ToString(backgroundColor);
            if (temp != "#FF000000")
            {
                ImgTop.Source = new BitmapImage(new Uri("Images/Cut.Light.png", UriKind.Relative));
                ImgTop.Opacity = 0.1;
            }
        }
        //---------------------------------------------------------------------------------------------------------





        //Wird bei jedem Aufruf der Seite ausgeführt
        //---------------------------------------------------------------------------------------------------------------------------------
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //File erstellen
            IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();

            //Variable für Ordner ermitteln
            string[] AllFolders = file.GetDirectoryNames("/Folders/");
            int FolderID = Convert.ToInt32(NavigationContext.QueryString["folder"]);
            Folder = AllFolders[FolderID];
            base.OnNavigatedTo(e);

            //ImageCount laden //Anzahl erstellter Bilder
            FileStream filestream = file.OpenFile("Settings/ImageCount.txt", FileMode.Open);
            StreamReader sr = new StreamReader(filestream);
            String tempSr = sr.ReadToEnd();
            filestream.Close();
            ImageCount = Convert.ToInt32(tempSr);

            //Image.dat laden
            filestream = file.OpenFile("Thumbs/" + Folder + ".dat", FileMode.Open);
            sr = new StreamReader(filestream);
            ImagesAll = sr.ReadToEnd();
            filestream.Close();
            ImagesAll = ImagesAll.TrimEnd(new char[] { '\r', '\n' });

            //Bilder laden
            GetImages("first");
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Bilder in Listbox laden
        //---------------------------------------------------------------------------------------------------------
        void GetImages(string action)
        {
            //Listbox leeren
            datalist.Clear();

            //Wenn alle Bilder ausgewählt werden
            if (action == "allPictures")
            {
                imgPictures = "all";
                action = "first";
            }

            //Wenn gespeicherte Bilder ausgewählt werden
            if (action == "savedPictures")
            {
                imgPictures = "saved";
                action = "first";
            }

            //Bei allen Bildern
            if (imgPictures == "all")
            {
                //Bei allen Bildern
                MediaLibrary mediaLibrary = new MediaLibrary();
                var pictures = mediaLibrary.Pictures;

                //Beim ersten laden
                if (action == "first")
                {
                    //Variabeln erstellen
                    imgGes = pictures.Count;
                    imgStart = 0;
                    if (imgGes >= (imgStart + 199))
                    {
                        imgEnd = imgStart + 199;
                    }
                    else
                    {
                        imgEnd = (pictures.Count - 1);
                    }
                    imgArea = 1;
                    imgAreaGes = (imgGes / 200) + 1;
                }

                //Wenn nächste
                if (action == "next")
                {
                    //Prüfen ob möglich
                    if (imgArea < imgAreaGes)
                    {
                        imgArea++;
                        imgStart = imgStart + 200;
                        if (imgGes >= (imgStart + 199))
                        {
                            imgEnd = imgStart + 199;
                        }
                        else
                        {
                            imgEnd = (pictures.Count - 1);
                        }
                    }
                }

                //Wenn vorherige
                if (action == "previous")
                {
                    //Prüfen ob möglich
                    if (imgArea > 1)
                    {
                        imgArea--;
                        imgStart = imgStart - 200;
                        if (imgGes >= (imgStart + 199))
                        {
                            imgEnd = imgStart + 199;
                        }
                        else
                        {
                            imgEnd = (pictures.Count - 1);
                        }
                    }
                }

                //Bilder auslesen und in ListBox schreiben
                for (int i = imgStart; i <= imgEnd; i++)
                {
                    try
                    {
                        BitmapImage image = new BitmapImage();
                        image.SetSource(pictures[i].GetThumbnail());
                        datalist.Add(new ClassMediaImage((i), image));
                    }
                    catch
                    {
                    }
                }
            }

            //Bei gespeicherten Bildern
            else
            {
                //Bei saved Pictures
                MediaLibrary mediaLibrary = new MediaLibrary();
                var pictures = mediaLibrary.SavedPictures;

                //Beim ersten laden
                if (action == "first")
                {
                    //Variabeln erstellen
                    imgGes = pictures.Count;
                    imgStart = 0;
                    if (imgGes >= (imgStart + 199))
                    {
                        imgEnd = imgStart + 199;
                    }
                    else
                    {
                        imgEnd = (pictures.Count - 1);
                    }
                    imgArea = 1;
                    imgAreaGes = (imgGes / 200) + 1;
                }

                //Wenn nächste
                if (action == "next")
                {
                    //Prüfen ob möglich
                    if (imgArea < imgAreaGes)
                    {
                        imgArea++;
                        imgStart = imgStart + 200;
                        if (imgGes >= (imgStart + 199))
                        {
                            imgEnd = imgStart + 199;
                        }
                        else
                        {
                            imgEnd = (pictures.Count - 1);
                        }
                    }
                }

                //Wenn vorherige
                if (action == "previous")
                {
                    //Prüfen ob möglich
                    if (imgArea > 1)
                    {
                        imgArea--;
                        imgStart = imgStart - 200;
                        if (imgGes >= (imgStart + 199))
                        {
                            imgEnd = imgStart + 199;
                        }
                        else
                        {
                            imgEnd = (pictures.Count - 1);
                        }
                    }
                }
                
                //Bilder auslesen und in ListBox schreiben
                for (int i = imgStart; i <= imgEnd; i++)
                {
                    try
                    {
                        BitmapImage image = new BitmapImage();
                        image.SetSource(pictures[i].GetThumbnail());
                        datalist.Add(new ClassMediaImage((i), image));
                    }
                    catch
                    {
                    }
                }
            }

            //Daten in Listbox schreiben
            LBImages.ItemsSource = datalist;

            //AppBar erstellen
            CreateAppBar();
        }
        //---------------------------------------------------------------------------------------------------------





        //AppBar erstellen //Haupt Panel
        //---------------------------------------------------------------------------------------------------------
        void CreateAppBar()
        {
            //neue AppBar anlegen
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = true;
            ApplicationBar.IsVisible = true;

            //IDs zum festlegen der Aktionen erstellen
            int buttonID = 0;
            int itemID = 0;

            //AppBar //Items //Select erstellen
            ApplicationBarMenuItem item0 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_Select);
            ApplicationBar.MenuItems.Add(item0);
            (ApplicationBar.MenuItems[itemID] as ApplicationBarMenuItem).Click += BtnSelect;
            itemID++;


            //Button zurück anlegen
            if (imgArea > 1)
            {
                ApplicationBarIconButton button1 = new ApplicationBarIconButton(new Uri("/Images/appbar.arrow.left.png", UriKind.Relative));
                button1.Text = Lockscreen_Swap.AppResx.Z01_Previous;
                ApplicationBar.Buttons.Add(button1);
                (ApplicationBar.Buttons[buttonID] as ApplicationBarIconButton).Click += BtnPrevious;
                buttonID++;

                ApplicationBarMenuItem item1 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_Previous);
                ApplicationBar.MenuItems.Add(item1);
                (ApplicationBar.MenuItems[itemID] as ApplicationBarMenuItem).Click += BtnPrevious;
                itemID++;
            }

            //Button select anlegen
            ApplicationBarIconButton button2 = new ApplicationBarIconButton(new Uri("/Images/appbar.add.multiple.png", UriKind.Relative));
            button2.Text = Lockscreen_Swap.AppResx.Z01_Select;
            ApplicationBar.Buttons.Add(button2);
            (ApplicationBar.Buttons[buttonID] as ApplicationBarIconButton).Click += BtnSelect;
            buttonID++;

            //Button zurück anlegen
            if (imgArea < imgAreaGes)
            {
                ApplicationBarIconButton button3 = new ApplicationBarIconButton(new Uri("/Images/appbar.arrow.right.png", UriKind.Relative));
                button3.Text = Lockscreen_Swap.AppResx.Z01_Next;
                ApplicationBar.Buttons.Add(button3);
                (ApplicationBar.Buttons[buttonID] as ApplicationBarIconButton).Click += BtnNext;
                buttonID++;

                ApplicationBarMenuItem item3 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_Next);
                ApplicationBar.MenuItems.Add(item3);
                (ApplicationBar.MenuItems[itemID] as ApplicationBarMenuItem).Click += BtnNext;
                itemID++;
            }

            //Item Alle Bilder
            ApplicationBarMenuItem item4 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_AllPictures);
            ApplicationBar.MenuItems.Add(item4);
            (ApplicationBar.MenuItems[itemID] as ApplicationBarMenuItem).Click += BtnAllPictures;
            itemID++;

            //Item Gespeicherte Bilder
            ApplicationBarMenuItem item5 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_SavedPictures);
            ApplicationBar.MenuItems.Add(item5);
            (ApplicationBar.MenuItems[itemID] as ApplicationBarMenuItem).Click += BtnSavedPictures;
            itemID++;
        }
        //---------------------------------------------------------------------------------------------------------





        //AppBar erstellen //Cut Panel
        //---------------------------------------------------------------------------------------------------------
        void CreateAppBar2()
        {
            //neue AppBar anlegen
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = true;
            ApplicationBar.IsVisible = true;

            //IDs zum festlegen der Aktionen erstellen
            int buttonID = 0;
            int itemID = 0;

            //Button cut anlegen
            ApplicationBarIconButton button1 = new ApplicationBarIconButton(new Uri("/Images/appbar.cut.png", UriKind.Relative));
            button1.Text = Lockscreen_Swap.AppResx.Z01_Cut;
            ApplicationBar.Buttons.Add(button1);
            (ApplicationBar.Buttons[buttonID] as ApplicationBarIconButton).Click += BtnCut;
            buttonID++;

            ApplicationBarMenuItem item1 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_Cut);
            ApplicationBar.MenuItems.Add(item1);
            (ApplicationBar.MenuItems[itemID] as ApplicationBarMenuItem).Click += BtnCut;
            itemID++;

            if (ImagesToAdd_c > 1)
            {
                //Button cut anlegen
                ApplicationBarIconButton button2 = new ApplicationBarIconButton(new Uri("/Images/appbar.cut.auto.png", UriKind.Relative));
                button2.Text = Lockscreen_Swap.AppResx.Z01_CutAuto;
                ApplicationBar.Buttons.Add(button2);
                (ApplicationBar.Buttons[buttonID] as ApplicationBarIconButton).Click += BtnAutoCut;
                buttonID++;

                ApplicationBarMenuItem item2 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_CutAuto);
                ApplicationBar.MenuItems.Add(item2);
                (ApplicationBar.MenuItems[itemID] as ApplicationBarMenuItem).Click += BtnAutoCut;
                itemID++;
            }

            //AppBar Button drehen links
            ApplicationBarIconButton button3 = new ApplicationBarIconButton(new Uri("/Images/appbar.rotate.counterclockwise.png", UriKind.Relative));
            button3.Text = Lockscreen_Swap.AppResx.Z03_Left;
            ApplicationBar.Buttons.Add(button3);
            ApplicationBarMenuItem item3 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z03_RotateLeft);
            ApplicationBar.MenuItems.Add(item3);
            (ApplicationBar.Buttons[buttonID] as ApplicationBarIconButton).Click += BtnRotateLeft;
            (ApplicationBar.MenuItems[itemID] as ApplicationBarMenuItem).Click += BtnRotateLeft;
            buttonID++;
            itemID++;

            //AppBar Button drehen rechts
            ApplicationBarIconButton button4 = new ApplicationBarIconButton(new Uri("/Images/appbar.rotate.clockwise.png", UriKind.Relative));
            button4.Text = Lockscreen_Swap.AppResx.Z03_Right;
            ApplicationBar.Buttons.Add(button4);
            ApplicationBarMenuItem item4 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z03_RotateRight);
            ApplicationBar.MenuItems.Add(item4);
            (ApplicationBar.Buttons[buttonID] as ApplicationBarIconButton).Click += BtnRotateRight;
            (ApplicationBar.MenuItems[itemID] as ApplicationBarMenuItem).Click += BtnRotateRight;
            buttonID++;
            itemID++;
        }
        //---------------------------------------------------------------------------------------------------------





        //AppBar entfernen
        //---------------------------------------------------------------------------------------------------------
        //neue AppBar anlegen
        void DeleteAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = false;
            ApplicationBar.IsVisible = false;
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Select
        //---------------------------------------------------------------------------------------------------------
        //Variablen erstellen
        bool MenuOpen = false;
        int ImagesToAdd_c;
        int[] ImagesToAdd;
        int ImageNow = 0;
        //Aktion ausführen
        private void BtnSelect(object sender, EventArgs e)
        {
            //Errechnen wie viele Bilder ausgewählt sind
            ImagesToAdd_c = LBImages.SelectedItems.Count;

            //Prüfen ob ein oder mehrere Bilder ausgewählt sind
            if (ImagesToAdd_c >= 1)
            {
                //Momenatane Bild zurücksetzen
                ImageNow = 0;

                //BilderListe erstellen
                ImagesToAdd = new int[ImagesToAdd_c];
                for (int i = 0; i < ImagesToAdd_c; i++)
                {
                    ImagesToAdd[i] = (LBImages.SelectedItems[i] as ClassMediaImage).imgID;
                }

                //Cut Panel aufrufen
                CutRoot.Visibility = System.Windows.Visibility.Visible;
                CutRoot.Margin = new Thickness(0, 0, 0, 0);
                MenuOpen = true;

                //Bild in Cut Panel laden
                LoadImageToPanel();

                //AppBar neu erstellen
                CreateAppBar2();
            }

            //Wenn keiner Bilder ausgewählt sind
            else
            {
                MessageBox.Show(Lockscreen_Swap.AppResx.Z01_MsgSelectImage);
            }
        }
        //---------------------------------------------------------------------------------------------------------





        //Nächstes Bild in Cut Panel laden
        //---------------------------------------------------------------------------------------------------------
        //Variabeln erstellen
        WriteableBitmap tempBitmap = new WriteableBitmap(0, 0);
        int nWidth;
        int nHeight;
        int imgSize = 320;
        //Aktion ausführen
        void LoadImageToPanel()
        {
            //Verschiebung zurücksetzen
            transform.TranslateX = 0;
            transform.TranslateY = 0;

            //Bei allen Bildern
            if (imgPictures == "all")
            {
                //Bei allen Bildern
                MediaLibrary mediaLibrary = new MediaLibrary();
                var pictures = mediaLibrary.Pictures;

                //Bilder auslesen und in Cut Panel schreiben
                try
                {
                    //Bild in Temp Bitmap laden
                    tempBitmap.SetSource(pictures[ImagesToAdd[ImageNow]].GetImage());

                    //Bild Maße neu berechnen
                    if (tempBitmap.PixelWidth < tempBitmap.PixelHeight)
                    {
                        nWidth = imgSize;
                        int perc = 100 * 100000 / tempBitmap.PixelWidth * imgSize / 100000;
                        nHeight = tempBitmap.PixelHeight * 100000 / 100 * perc / 100000;
                    }
                    else if (tempBitmap.PixelWidth > tempBitmap.PixelHeight)
                    {
                        nHeight = imgSize;
                        int perc = 100 * 100000 / tempBitmap.PixelHeight * imgSize / 100000;
                        nWidth = tempBitmap.PixelWidth * 100000 / 100 * perc / 100000;
                    }
                    else
                    {
                        nWidth = imgSize;
                        nHeight = imgSize;
                    }

                    //Bild Maße neu erstellen
                    tempBitmap = tempBitmap.Resize(nWidth, nHeight, WriteableBitmapExtensions.Interpolation.Bilinear);

                    //Bild in Cut Bild ausgeben
                    CutImage.Height = nHeight;
                    CutImage.Width = nWidth;
                    CutImage.Source = tempBitmap;
                }
                catch
                {
                }
            }

            //Bei gespeicherten Bildern
            else
            {
                //Bei saved Pictures
                MediaLibrary mediaLibrary = new MediaLibrary();
                var pictures = mediaLibrary.SavedPictures;

                //Bilder auslesen und in Cut Panel schreiben
                try
                {
                    //Bild in Temp Bitmap laden
                    tempBitmap.SetSource(pictures[ImagesToAdd[ImageNow]].GetImage());

                    //Bild Maße neu berechnen
                    if (tempBitmap.PixelWidth < tempBitmap.PixelHeight)
                    {
                        nWidth = imgSize;
                        int perc = 100 * 100000 / tempBitmap.PixelWidth * imgSize / 100000;
                        nHeight = tempBitmap.PixelHeight * 100000 / 100 * perc / 100000;
                    }
                    else if (tempBitmap.PixelWidth > tempBitmap.PixelHeight)
                    {
                        nHeight = imgSize;
                        int perc = 100 * 100000 / tempBitmap.PixelHeight * imgSize / 100000;
                        nWidth = tempBitmap.PixelWidth * 100000 / 100 * perc / 100000;
                    }
                    else
                    {
                        nWidth = imgSize;
                        nHeight = imgSize;
                    }

                    //Bild Maße neu erstellen
                    tempBitmap = tempBitmap.Resize(nWidth, nHeight, WriteableBitmapExtensions.Interpolation.Bilinear);

                    //Bild in Cut Bild ausgeben
                    CutImage.Height = nHeight;
                    CutImage.Width = nWidth;
                    CutImage.Source = tempBitmap;
                }
                catch
                {
                }
            }
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Zurück
        //---------------------------------------------------------------------------------------------------------
        private void BtnPrevious(object sender, EventArgs e)
        {
            GetImages("previous");
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Next
        //---------------------------------------------------------------------------------------------------------
        private void BtnNext(object sender, EventArgs e)
        {
            GetImages("next");
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Alle Bilder
        //---------------------------------------------------------------------------------------------------------
        private void BtnAllPictures(object sender, EventArgs e)
        {
            GetImages("allPictures");
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Gespeicherte Bilder
        //---------------------------------------------------------------------------------------------------------
        private void BtnSavedPictures(object sender, EventArgs e)
        {
            GetImages("savedPictures");
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Schneiden
        //---------------------------------------------------------------------------------------------------------
        private void BtnCut(object sender, EventArgs e)
        {
            //neues Bild erstellen
            var CutBitmap = new WriteableBitmap(imgSize, imgSize);

            //Geschnittenes Bild hinzufügen
            if (CutImage.Width > CutImage.Height)
            {
                //Schnitt errechnen
                int cut = ((tempBitmap.PixelWidth - imgSize) / 2) + (0 - Convert.ToInt16(transform.TranslateX));
                //Bild schneiden
                CutBitmap.Blit(new Rect(0, 0, imgSize, imgSize), tempBitmap, new Rect(cut, 0, imgSize, imgSize));
            }
            else if (CutImage.Width < CutImage.Height)
            {
                //Schnitt errechnen
                int cut = ((tempBitmap.PixelHeight - imgSize) / 2) + (0 - Convert.ToInt16(transform.TranslateY));
                //Bild schneiden
                CutBitmap.Blit(new Rect(0, 0, imgSize, imgSize), tempBitmap, new Rect(0, cut, imgSize, imgSize));
            }
            else
            {
                //Bild schneiden
                CutBitmap.Blit(new Rect(0, 0, imgSize, imgSize), tempBitmap, new Rect(0, 0, imgSize, imgSize));
            }


            //Image Count erhöhen
            ImageCount++;
            string ImageName = Convert.ToString(ImageCount);
            while (ImageName.Length < 8)
            {
                ImageName = "0" + ImageName;
            }
            //Image Count speichern
            IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
            FileStream filestream = file.CreateFile("Settings/ImageCount.txt");
            StreamWriter sw = new StreamWriter(filestream);
            sw.WriteLine(Convert.ToString(ImageCount));
            sw.Flush();
            filestream.Close();


            //Datei in Isolated Storage schreiben
            var userStoreForApplication = IsolatedStorageFile.GetUserStoreForApplication();
            var isolatedStorageFileStream = userStoreForApplication.CreateFile("/Folders/" + Folder + "/" + ImageName + ".jpg");
            int ImgHeight = CutBitmap.PixelHeight;
            int ImgWidth = CutBitmap.PixelWidth;
            CutBitmap.SaveJpeg(isolatedStorageFileStream, CutBitmap.PixelWidth, CutBitmap.PixelHeight, 0, 80);
            isolatedStorageFileStream.Close();

            //Thumbnail erstellen
            int percent;
            int newWidth;
            int newHeight;
            //Wenn breiter als hoch
            if (CutBitmap.PixelWidth > CutBitmap.PixelHeight)
            {
                percent = 100 * 1000 / CutBitmap.PixelWidth * 150 / 1000;
                newHeight = CutBitmap.PixelHeight * 1000 / 100 * percent / 1000;
                newWidth = 150;
            }
            //Wenn höher als breit
            else
            {
                percent = 100 * 1000 / CutBitmap.PixelHeight * 150 / 1000;
                newWidth = CutBitmap.PixelWidth * 1000 / 100 * percent / 1000;
                newHeight = 150;
            }
            //Bild verkleinern
            CutBitmap = CutBitmap.Resize(newWidth, newHeight, WriteableBitmapExtensions.Interpolation.Bilinear);

            //Thumbnail speichern
            isolatedStorageFileStream = userStoreForApplication.CreateFile("/Thumbs/" + Folder + "/" + ImageName + ".jpg");
            CutBitmap.SaveJpeg(isolatedStorageFileStream, CutBitmap.PixelWidth, CutBitmap.PixelHeight, 0, 80);
            isolatedStorageFileStream.Close();

            //ImagesAll bearbeiten
            ImagesAll += ImageName + ".jpg/";
            //Images.dat speichern
            filestream = file.CreateFile("/Thumbs/" + Folder + ".dat");
            sw = new StreamWriter(filestream);
            sw.WriteLine(ImagesAll);
            sw.Flush();
            filestream.Close();

            //Aktuelles Bild erhöhen
            ImageNow++;
            //Prüfen ob noch ein Bild vorhanden
            if (ImageNow < ImagesToAdd_c)
            {
                LoadImageToPanel();
            }
            else
            {
                //Cut zurücksetzen
                CutRoot.Margin = new Thickness(-600, 0, 0, 0);
                CutRoot.Visibility = System.Windows.Visibility.Collapsed;
                //AppBar zurückstellen
                CreateAppBar();
                //MenuOpene zurücksetzen
                MenuOpen = false;
            }

            
        }
        //---------------------------------------------------------------------------------------------------------





        //Button Automatisch schneiden
        //---------------------------------------------------------------------------------------------------------
        private void BtnAutoCut(object sender, EventArgs e)
        {
            //Schleife erstellen um Bilder automatisch zu schneiden
            for (int i = ImageNow; i < ImagesToAdd_c; i++)
            {

                //Bei allen Bildern
                if (imgPictures == "all")
                {
                    //Bei allen Bildern
                    MediaLibrary mediaLibrary = new MediaLibrary();
                    var pictures = mediaLibrary.Pictures;

                    //Bilder auslesen und in Cut Panel schreiben
                    try
                    {
                        //Bild in Temp Bitmap laden
                        tempBitmap.SetSource(pictures[ImagesToAdd[ImageNow]].GetImage());

                        //Bild Maße neu berechnen
                        if (tempBitmap.PixelWidth < tempBitmap.PixelHeight)
                        {
                            //Neue Größer errechnen
                            nWidth = imgSize;
                            int perc = 100 * 100000 / tempBitmap.PixelWidth * imgSize / 100000;
                            nHeight = tempBitmap.PixelHeight * 100000 / 100 * perc / 100000;
                            //Bild Maße neu erstellen
                            tempBitmap = tempBitmap.Resize(nWidth, nHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
                            //Bild schneiden
                            tempBitmap = tempBitmap.Crop(0, ((nHeight - imgSize) / 2), imgSize, imgSize);
                        }
                        else if (tempBitmap.PixelWidth > tempBitmap.PixelHeight)
                        {
                            //Neue Größer errechnen
                            nHeight = imgSize;
                            int perc = 100 * 100000 / tempBitmap.PixelHeight * imgSize / 100000;
                            nWidth = tempBitmap.PixelWidth * 100000 / 100 * perc / 100000;
                            //Bild Maße neu erstellen
                            tempBitmap = tempBitmap.Resize(nWidth, nHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
                            //Bild schneiden
                            tempBitmap = tempBitmap.Crop(((nWidth - imgSize) / 2), 0, imgSize, imgSize);
                        }
                        else
                        {
                            //Neue Größer errechnen
                            nWidth = imgSize;
                            nHeight = imgSize;
                            //Bild Maße neu erstellen
                            tempBitmap = tempBitmap.Resize(nWidth, nHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
                        }

                        //Image Count erhöhen
                        ImageCount++;
                        string ImageName = Convert.ToString(ImageCount);
                        while (ImageName.Length < 8)
                        {
                            ImageName = "0" + ImageName;
                        }
                        //Image Count speichern
                        IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
                        FileStream filestream = file.CreateFile("Settings/ImageCount.txt");
                        StreamWriter sw = new StreamWriter(filestream);
                        sw.WriteLine(Convert.ToString(ImageCount));
                        sw.Flush();
                        filestream.Close();

                        //Datei in Isolated Storage schreiben
                        var userStoreForApplication = IsolatedStorageFile.GetUserStoreForApplication();
                        var isolatedStorageFileStream = userStoreForApplication.CreateFile("/Folders/" + Folder + "/" + ImageName + ".jpg");
                        int ImgHeight = tempBitmap.PixelHeight;
                        int ImgWidth = tempBitmap.PixelWidth;
                        tempBitmap.SaveJpeg(isolatedStorageFileStream, tempBitmap.PixelWidth, tempBitmap.PixelHeight, 0, 80);
                        isolatedStorageFileStream.Close();

                        //Thumbnail erstellen
                        int percent;
                        int newWidth;
                        int newHeight;
                        //Wenn breiter als hoch
                        if (tempBitmap.PixelWidth > tempBitmap.PixelHeight)
                        {
                            percent = 100 * 1000 / tempBitmap.PixelWidth * 150 / 1000;
                            newHeight = tempBitmap.PixelHeight * 1000 / 100 * percent / 1000;
                            newWidth = 150;
                        }
                        //Wenn höher als breit
                        else
                        {
                            percent = 100 * 1000 / tempBitmap.PixelHeight * 150 / 1000;
                            newWidth = tempBitmap.PixelWidth * 1000 / 100 * percent / 1000;
                            newHeight = 150;
                        }
                        //Bild verkleinern
                        tempBitmap = tempBitmap.Resize(newWidth, newHeight, WriteableBitmapExtensions.Interpolation.Bilinear);

                        //Thumbnail speichern
                        isolatedStorageFileStream = userStoreForApplication.CreateFile("/Thumbs/" + Folder + "/" + ImageName + ".jpg");
                        tempBitmap.SaveJpeg(isolatedStorageFileStream, tempBitmap.PixelWidth, tempBitmap.PixelHeight, 0, 80);
                        isolatedStorageFileStream.Close();

                        //ImagesAll bearbeiten
                        ImagesAll += ImageName + ".jpg/";
                        //Images.dat speichern
                        filestream = file.CreateFile("/Thumbs/" + Folder + ".dat");
                        sw = new StreamWriter(filestream);
                        sw.WriteLine(ImagesAll);
                        sw.Flush();
                        filestream.Close();

                        //ImgNow erhöhen
                        ImageNow++;
                    }
                    catch
                    {
                    }
                }

                //Bei gespeicherten Bildern
                else
                {
                    //Bei saved Pictures
                    MediaLibrary mediaLibrary = new MediaLibrary();
                    var pictures = mediaLibrary.SavedPictures;

                    //Bilder auslesen und in Cut Panel schreiben
                    try
                    {
                        //Bild in Temp Bitmap laden
                        tempBitmap.SetSource(pictures[ImagesToAdd[ImageNow]].GetImage());

                        //Bild Maße neu berechnen
                        if (tempBitmap.PixelWidth < tempBitmap.PixelHeight)
                        {
                            //Neue Größer errechnen
                            nWidth = imgSize;
                            int perc = 100 * 100000 / tempBitmap.PixelWidth * imgSize / 100000;
                            nHeight = tempBitmap.PixelHeight * 100000 / 100 * perc / 100000;
                            //Bild Maße neu erstellen
                            tempBitmap = tempBitmap.Resize(nWidth, nHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
                            //Bild schneiden
                            tempBitmap = tempBitmap.Crop(0, ((nHeight - imgSize) / 2), imgSize, imgSize);
                        }
                        else if (tempBitmap.PixelWidth > tempBitmap.PixelHeight)
                        {
                            //Neue Größer errechnen
                            nHeight = imgSize;
                            int perc = 100 * 100000 / tempBitmap.PixelHeight * imgSize / 100000;
                            nWidth = tempBitmap.PixelWidth * 100000 / 100 * perc / 100000;
                            //Bild Maße neu erstellen
                            tempBitmap = tempBitmap.Resize(nWidth, nHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
                            //Bild schneiden
                            tempBitmap = tempBitmap.Crop(((nWidth - imgSize) / 2), 0, imgSize, imgSize);
                        }
                        else
                        {
                            //Neue Größer errechnen
                            nWidth = imgSize;
                            nHeight = imgSize;
                            //Bild Maße neu erstellen
                            tempBitmap = tempBitmap.Resize(nWidth, nHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
                        }

                        //Image Count erhöhen
                        ImageCount++;
                        string ImageName = Convert.ToString(ImageCount);
                        while (ImageName.Length < 8)
                        {
                            ImageName = "0" + ImageName;
                        }
                        //Image Count speichern
                        IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
                        FileStream filestream = file.CreateFile("Settings/ImageCount.txt");
                        StreamWriter sw = new StreamWriter(filestream);
                        sw.WriteLine(Convert.ToString(ImageCount));
                        sw.Flush();
                        filestream.Close();

                        //Datei in Isolated Storage schreiben
                        var userStoreForApplication = IsolatedStorageFile.GetUserStoreForApplication();
                        var isolatedStorageFileStream = userStoreForApplication.CreateFile("/Folders/" + Folder + "/" + ImageName + ".jpg");
                        int ImgHeight = tempBitmap.PixelHeight;
                        int ImgWidth = tempBitmap.PixelWidth;
                        tempBitmap.SaveJpeg(isolatedStorageFileStream, tempBitmap.PixelWidth, tempBitmap.PixelHeight, 0, 80);
                        isolatedStorageFileStream.Close();

                        //Thumbnail erstellen
                        int percent;
                        int newWidth;
                        int newHeight;
                        //Wenn breiter als hoch
                        if (tempBitmap.PixelWidth > tempBitmap.PixelHeight)
                        {
                            percent = 100 * 1000 / tempBitmap.PixelWidth * 150 / 1000;
                            newHeight = tempBitmap.PixelHeight * 1000 / 100 * percent / 1000;
                            newWidth = 150;
                        }
                        //Wenn höher als breit
                        else
                        {
                            percent = 100 * 1000 / tempBitmap.PixelHeight * 150 / 1000;
                            newWidth = tempBitmap.PixelWidth * 1000 / 100 * percent / 1000;
                            newHeight = 150;
                        }
                        //Bild verkleinern
                        tempBitmap = tempBitmap.Resize(newWidth, newHeight, WriteableBitmapExtensions.Interpolation.Bilinear);

                        //Thumbnail speichern
                        isolatedStorageFileStream = userStoreForApplication.CreateFile("/Thumbs/" + Folder + "/" + ImageName + ".jpg");
                        tempBitmap.SaveJpeg(isolatedStorageFileStream, tempBitmap.PixelWidth, tempBitmap.PixelHeight, 0, 80);
                        isolatedStorageFileStream.Close();

                        //ImagesAll bearbeiten
                        ImagesAll += ImageName + ".jpg/";
                        //Images.dat speichern
                        filestream = file.CreateFile("/Thumbs/" + Folder + ".dat");
                        sw = new StreamWriter(filestream);
                        sw.WriteLine(ImagesAll);
                        sw.Flush();
                        filestream.Close();

                        //ImgNow erhöhen
                        ImageNow++;
                    }
                    catch
                    {
                    }
                }
            }

            //Cut zurücksetzen
            CutRoot.Margin = new Thickness(-600, 0, 0, 0);
            CutRoot.Visibility = System.Windows.Visibility.Collapsed;
            //AppBar zurückstellen
            CreateAppBar();
            //MenuOpene zurücksetzen
            MenuOpen = false;

        }
        //---------------------------------------------------------------------------------------------------------





        //Bild ausrichten
        //---------------------------------------------------------------------------------------------------------------------------------
        //Variabeln
        double maxX;
        double minX;
        double maxY;
        double minY;

        //Funktion
        private void OnDoubleTap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            transform.ScaleX = transform.ScaleY = 1;
        }

        private void DoubleTap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            transform.TranslateX = transform.TranslateY = 0;
            transform.ScaleX = transform.ScaleY = 1;
            transform.Rotation = 0;
        }

        private void OnDragStarted(object sender, DragStartedGestureEventArgs e)
        {
            CutImage.Opacity = 0.3;
        }

        private void OnDragDelta(object sender, DragDeltaGestureEventArgs e)
        {

            transform.TranslateX += e.HorizontalChange;
            transform.TranslateY += e.VerticalChange;

            maxX = Convert.ToDouble((nWidth - imgSize) / 2);
            minX = 0 - maxX;
            maxY = Convert.ToDouble((nHeight - imgSize) / 2);
            minY = 0 - maxY;

            if (maxX < transform.TranslateX)
            {
                transform.TranslateX = maxX;
            }
            if (minX > transform.TranslateX)
            {
                transform.TranslateX = minX;
            }


            if (maxY < transform.TranslateY)
            {
                transform.TranslateY = maxY;
            }
            if (minY > transform.TranslateY)
            {
                transform.TranslateY = minY;
            }
        }

        private void OnDragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            CutImage.Opacity = 1.0;
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Bild drehen links
        //---------------------------------------------------------------------------------------------------------------------------------
        private void BtnRotateLeft(object sender, EventArgs e)
        {
            //Bild drehen
            tempBitmap = tempBitmap.Rotate(270);

            //Verschiebung zurücksetzen
            transform.TranslateX = 0;
            transform.TranslateY = 0;

            //Breite und Höhe neu erstellen
            nHeight = tempBitmap.PixelHeight;
            nWidth = tempBitmap.PixelWidth;

            //Bild in Cut Bild ausgeben
            CutImage.Height = nHeight;
            CutImage.Width = nWidth;
            CutImage.Source = tempBitmap;
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Bild drehen rechts
        //---------------------------------------------------------------------------------------------------------------------------------
        private void BtnRotateRight(object sender, EventArgs e)
        {
            //Bild drehen
            tempBitmap = tempBitmap.Rotate(90);

            //Verschiebung zurücksetzen
            transform.TranslateX = 0;
            transform.TranslateY = 0;

            //Breite und Höhe neu erstellen
            nHeight = tempBitmap.PixelHeight;
            nWidth = tempBitmap.PixelWidth;

            //Bild in Cut Bild ausgeben
            CutImage.Height = nHeight;
            CutImage.Width = nWidth;
            CutImage.Source = tempBitmap;
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Back Button
        //---------------------------------------------------------------------------------------------------------------------------------
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (MenuOpen == true)
            {
                //Cut zurücksetzen
                CutRoot.Margin = new Thickness(-600, 0, 0, 0);
                CutRoot.Visibility = System.Windows.Visibility.Collapsed;
                //AppBar zurückstellen
                CreateAppBar();
                //MenuOpene zurücksetzen
                MenuOpen = false;
                //Zurück abbrechen
                e.Cancel = true;
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------       





    }
}
