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
//Für Timer
using System.Windows.Threading;
//Zum laden des Qellcodes und zum erstellen und auslesem von Dateien
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
//Um die Hintergrundfarbe der Listbox zu ändern
using System.Windows.Media;
//Für wechsel der Farben, der Textboxen
using System.Windows.Shapes;
//Für Benachrichtigung vor beenden
using System.ComponentModel;
//Für Listbox Update
using System.Collections.ObjectModel;
//Für MVVM Commands
using System.Windows.Input;
//Um pngs zu speichern
using ImageTools;
using ImageTools.IO.Png;





namespace Lockscreen_Swap.Pages
{
    public partial class EditFolder : PhoneApplicationPage
    {





        //Variabeln erstellen
        //---------------------------------------------------------------------------------------------------------
        //Ordner
        string Folder;
        int FolderID = -1;
        //Bilder in Ordner
        string[] Images;
        //Alle Bilder
        string ImagesAll;
        //Anzahl erstellter Bilder
        int ImageCount;
        //Neue Datenliste erstellen
        ObservableCollection<ClassPictures> datalist = new ObservableCollection<ClassPictures>();
        ObservableCollection<ClassFolders> datalist2 = new ObservableCollection<ClassFolders>();
        //PhotoCooserTask erstellen
        PhotoChooserTask photoChooserTask;
        //---------------------------------------------------------------------------------------------------------


        

        //Wird am Anfang ausgeführt
        //---------------------------------------------------------------------------------------------------------
        public EditFolder()
        {
            //Komponenten laden
            InitializeComponent();

            //AppBar erstellen
            AppBar01();

            //PhotoCooser Task
            photoChooserTask = new PhotoChooserTask();
            //Angeben was PhotoCooserTask ausführt wenn Bild ausgewählt
            photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);

            //png Encoder undpng Decoder erstellen
            ImageTools.IO.Decoders.AddDecoder<PngDecoder>();
            ImageTools.IO.Encoders.AddEncoder<PngEncoder>();

            //Bilder ändern
            Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            string temp = Convert.ToString(backgroundColor);
            if (temp != "#FF000000")
            {
                ImgTop.Source = new BitmapImage(new Uri("Images/Folder.Light.png", UriKind.Relative));
                ImgTop.Opacity = 0.1;
                ImgTop2.Source = new BitmapImage(new Uri("Images/Copy.Light.png", UriKind.Relative));
                ImgTop2.Opacity = 0.1;
                ImgTop3.Source = new BitmapImage(new Uri("Images/Cut.Light.png", UriKind.Relative));
                ImgTop3.Opacity = 0.1;
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
            FolderID = Convert.ToInt32(NavigationContext.QueryString["folder"]);
            Folder = AllFolders[FolderID];
            base.OnNavigatedTo(e);


            //ImageCount laden //Anzahl erstellter Bilder
            FileStream filestream = file.OpenFile("Settings/ImageCount.txt", FileMode.Open);
            StreamReader sr = new StreamReader(filestream);
            String tempSr = sr.ReadToEnd();
            filestream.Close();
            ImageCount = Convert.ToInt32(tempSr);

            //Images.dat laden
            filestream = file.OpenFile("Thumbs/" + Folder + ".dat", FileMode.Open);
            sr = new StreamReader(filestream);
            ImagesAll = sr.ReadToEnd();
            filestream.Close();
            ImagesAll = ImagesAll.TrimEnd(new char[] { '\r', '\n' });

            //Bilder erstellen
            CreateImages();

            //Ordner Liste erstellen
            datalist2.Clear();
            //Ordner Daten laden
            string[] temp4 = file.GetDirectoryNames("/Folders/");
            int temp4_c = temp4.Count();
            //Prüfen ob Ordner vorhanden
            for (int i = 0; i < temp4_c; i++)
            {
                datalist2.Add(new ClassFolders(temp4[i],""));
            }
            //Daten in Listbox schreiben
            LBFolders.ItemsSource = datalist2;

        }
        //---------------------------------------------------------------------------------------------------------------------------------










        //ApplicationBar hinzufügen, kopieren, löschen erstellen
        //---------------------------------------------------------------------------------------------------------------------------------
        void AppBar01()
        {
            //neue AppBar anlegen
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = true;
            ApplicationBar.IsVisible = true;

            //AppBar Buttons anlegen
            ApplicationBarIconButton button0 = new ApplicationBarIconButton(new Uri("/Images/appbar.add.png", UriKind.Relative));
            button0.Text = Lockscreen_Swap.AppResx.Z01_SingleAdd;
            ApplicationBar.Buttons.Add(button0);

            ApplicationBarIconButton button1 = new ApplicationBarIconButton(new Uri("/Images/appbar.add.multiple.png", UriKind.Relative));
            button1.Text = Lockscreen_Swap.AppResx.Z01_MultipleAdd;
            ApplicationBar.Buttons.Add(button1);

            ApplicationBarIconButton button2 = new ApplicationBarIconButton(new Uri("/Images/appbar.copy.png", UriKind.Relative));
            button2.Text = Lockscreen_Swap.AppResx.Z01_Copy;
            ApplicationBar.Buttons.Add(button2);

            ApplicationBarIconButton button3 = new ApplicationBarIconButton(new Uri("/Images/appbar.delete.png", UriKind.Relative));
            button3.Text = Lockscreen_Swap.AppResx.Z01_Delete;
            ApplicationBar.Buttons.Add(button3);

            //AppBar Menü Items anlegen
            ApplicationBarMenuItem item0 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_SingleAdd);
            ApplicationBar.MenuItems.Add(item0);

            ApplicationBarMenuItem item1 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_MultipleAdd);
            ApplicationBar.MenuItems.Add(item1);

            ApplicationBarMenuItem item2 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_Copy);
            ApplicationBar.MenuItems.Add(item2);

            ApplicationBarMenuItem item3 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_Delete);
            ApplicationBar.MenuItems.Add(item3);

            ApplicationBarMenuItem item4 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_SelectAll);
            ApplicationBar.MenuItems.Add(item4);

            ApplicationBarMenuItem item5 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_UnselectAll);
            ApplicationBar.MenuItems.Add(item5);

            //AppBar Funktionen festlegen
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Click += BtnAddSingle;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).Click += BtnAddMultiple;
            (ApplicationBar.Buttons[2] as ApplicationBarIconButton).Click += BtnCopy;
            (ApplicationBar.Buttons[3] as ApplicationBarIconButton).Click += BtnDelete;

            //AppMenü Funktionen festlegen
            (ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).Click += BtnAddSingle;
            (ApplicationBar.MenuItems[1] as ApplicationBarMenuItem).Click += BtnAddMultiple;
            (ApplicationBar.MenuItems[2] as ApplicationBarMenuItem).Click += BtnCopy;
            (ApplicationBar.MenuItems[3] as ApplicationBarMenuItem).Click += BtnDelete;
            (ApplicationBar.MenuItems[4] as ApplicationBarMenuItem).Click += BtnSelect;
            (ApplicationBar.MenuItems[5] as ApplicationBarMenuItem).Click += BtnUnselect;
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //ApplicationBar schneiden erstellen
        //---------------------------------------------------------------------------------------------------------------------------------
        void AppBar02()
        {
            //neue AppBar anlegen
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = true;
            ApplicationBar.IsVisible = true;

            //AppBar Buttons anlegen
            ApplicationBarIconButton button0 = new ApplicationBarIconButton(new Uri("/Images/appbar.cut.png", UriKind.Relative));
            button0.Text = Lockscreen_Swap.AppResx.Z01_Cut;
            ApplicationBar.Buttons.Add(button0);

            //AppBar Button drehen links
            ApplicationBarIconButton button1 = new ApplicationBarIconButton(new Uri("/Images/appbar.rotate.counterclockwise.png", UriKind.Relative));
            button1.Text = Lockscreen_Swap.AppResx.Z03_Left;
            ApplicationBar.Buttons.Add(button1);

            //AppBar Button drehen rechts
            ApplicationBarIconButton button2 = new ApplicationBarIconButton(new Uri("/Images/appbar.rotate.clockwise.png", UriKind.Relative));
            button2.Text = Lockscreen_Swap.AppResx.Z03_Right;
            ApplicationBar.Buttons.Add(button2);

            //AppBar Menü Items anlegen
            ApplicationBarMenuItem item0 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z01_Cut);
            ApplicationBar.MenuItems.Add(item0);

            //AppBar Menü Items drehen links
            ApplicationBarMenuItem item1 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z03_RotateLeft);
            ApplicationBar.MenuItems.Add(item1);

            //AppBar Menü Items drehen rechts
            ApplicationBarMenuItem item2 = new ApplicationBarMenuItem(Lockscreen_Swap.AppResx.Z03_RotateRight);
            ApplicationBar.MenuItems.Add(item2);

            //AppBar Funktionen festlegen
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).Click += BtnCut;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).Click += BtnRotateLeft;
            (ApplicationBar.Buttons[2] as ApplicationBarIconButton).Click += BtnRotateRight;

            //AppMenü Funktionen festlegen
            (ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).Click += BtnCut;
            (ApplicationBar.MenuItems[1] as ApplicationBarMenuItem).Click += BtnRotateLeft;
            (ApplicationBar.MenuItems[2] as ApplicationBarMenuItem).Click += BtnRotateRight;
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Bilderliste erstellen
        //---------------------------------------------------------------------------------------------------------------------------------
        void CreateImages()
        {
            //Image Liste leeren
            datalist.Clear();
            //File erstellen
            IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
            string[] temp = file.GetFileNames("/Folders/" + Folder + "/");
            int temp_c = temp.Count();

            //Bilder in Überschrift schreiben
            TBTopImages.Text = temp_c + " " + Lockscreen_Swap.AppResx.Pictures;

            Images = new string[temp_c];
            //Bilder durchlaufen
            for (int i = 0; i < temp_c; i++)
            {
                //Bilder in Images schreiben
                Images[i] = temp[i];

                try
                {
                    //Bilder laden
                    byte[] data1;
                    using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (IsolatedStorageFileStream isfs = isf.OpenFile("/Thumbs/" + Folder + "/" + temp[i], FileMode.Open, FileAccess.Read))
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
                    datalist.Add(new ClassPictures("/Folders/" + Folder + "/" + temp[i], "/Thumbs/" + Folder + "/" + temp[i], temp[i], i, bi));
                }
                catch
                {

                }
            }
            //Daten in Listbox schreiben
            LBImages.ItemsSource = datalist;

            //TextBox Pfad angeben
            TBTop.Text = Folder;
            TBTop2.Text = Folder;
            TBTop3.Text = Folder;

            //Felder sichtbar machen
            CopyRoot.Visibility = System.Windows.Visibility.Visible;
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Button Delete
        //---------------------------------------------------------------------------------------------------------------------------------
        //Variablen erstellen
        int ImagesToDelete_c;
        string[] ImagesToDelete;
        //Bilder löschen
        private void BtnDelete(object sender, EventArgs e)
        {
            //Prüfen ob Multiselect aktiviert ist
            if (LBImages.SelectionMode == SelectionMode.Multiple)
            {
                //Prüfen wieviele Bilder ausgewählt sind
                ImagesToDelete_c = LBImages.SelectedItems.Count;
                //Prüfen ob Bilder ausgewählt sind und löschen
                if (ImagesToDelete_c > 0)
                {
                    //Warnung ausgeben
                    if (MessageBox.Show(Lockscreen_Swap.AppResx.DeleteImages, Lockscreen_Swap.AppResx.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        //ImagesToDelete Liste erstellen
                        ImagesToDelete = new string[ImagesToDelete_c];
                        //IO File erstellen
                        IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
                        //Ausgewählte Bilder durchlaufen
                        for (int i = 0; i < ImagesToDelete_c; i++)
                        {
                            //Bilder löschen
                            if (file.FileExists((LBImages.SelectedItems[i] as ClassPictures).imgPath))
                            {
                                file.DeleteFile((LBImages.SelectedItems[i] as ClassPictures).imgPath);
                            }
                            //Thumbnail löschen
                            if (file.FileExists((LBImages.SelectedItems[i] as ClassPictures).imgThumbPath))
                            {
                                file.DeleteFile((LBImages.SelectedItems[i] as ClassPictures).imgThumbPath);
                            }

                            //ImagesAll bearbeiten
                            ImagesAll = ImagesAll.Replace("/" + (LBImages.SelectedItems[i] as ClassPictures).imgFile + "/", "/");
                            //Image.dat neu erstellen
                            IsolatedStorageFileStream filestream = file.CreateFile("/Thumbs/" + Folder + ".dat");
                            StreamWriter sw = new StreamWriter(filestream);
                            sw.WriteLine(ImagesAll);
                            sw.Flush();
                            filestream.Close();
                        }

                        //ImgSelectSave auf false stellen
                        ImgSelectSave = false;
                        //Listbox auswahl aufheben
                        try
                        {
                            LBImages.SelectedIndex = -1;
                        }
                        catch
                        {
                        }
                        
                        //Bilderliste neu erstellen
                        CreateImages();
                        
                        //ImgSelectSave auf true stellen
                        ImgSelectSave = true;
                    }
                }
                //Wenn keine Bilder ausgewählt sind, Warnung ausgeben
                else
                {
                    //Warnung ausgeben
                    MessageBox.Show(Lockscreen_Swap.AppResx.SelectDelete);
                }
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Button Copy
        //---------------------------------------------------------------------------------------------------------------------------------
        //Variablen erstellen
        int ImagesToCopy_c;
        string[] ImagesToCopy;
        bool MenuOpen = false;
        //Ordnerliste öffnen
        private void BtnCopy(object sender, EventArgs e)
        {
            //Prüfen ob Multiselect aktiviert ist
            if (LBImages.SelectionMode == SelectionMode.Multiple)
            {
                //Prüfen wieviele Bilder ausgewählt sind
                ImagesToCopy_c = LBImages.SelectedItems.Count;
                //Wenn kein Bilder ausgewählt sind, Ordnerliste erstellen
                if (ImagesToCopy_c > 0)
                {
                    //Ordner Panel einblenden
                    CopyRoot.Margin = new Thickness(0, 0, 0, 0);
                    //Ausgeben das Kopierpanel aktiv ist
                    MenuOpen = true;
                }
                //Wenn keine Bilder ausgegeben sind, Warnung ausgeben
                else
                {
                    //Warnung ausgeben
                    MessageBox.Show(Lockscreen_Swap.AppResx.SelectCopy);
                }

            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Button Add Single
        //---------------------------------------------------------------------------------------------------------------------------------
        //Ordnerliste öffnen
        private void BtnAddSingle(object sender, EventArgs e)
        {
            try
            {
                photoChooserTask.Show();
            }
            catch (System.InvalidOperationException ex)
            {
                // Catch the exception, but no handling is necessary.
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Button Add Multiple
        //---------------------------------------------------------------------------------------------------------------------------------
        //Ordnerliste öffnen
        private void BtnAddMultiple(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/ImagesImporter.xaml?folder=" + FolderID, UriKind.Relative));
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Ausgewähltes Bild verarbeiten
        //-----------------------------------------------------------------------------------------------------------------
        //Bild Variabeln
        WriteableBitmap tempBitmap = new WriteableBitmap(0, 0);
        int nWidth;
        int nHeight;
        int imgSize = 320;
        //Wenn Bild ausgewählt wurde
        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            //Ausgewähltes Bild verarbeiten
            if (e.TaskResult == TaskResult.OK)
            {
                //Verschiebung zurücksetzen
                transform.TranslateX = 0;
                transform.TranslateY = 0;

                //Bild in Writeable Bitmap speichern
                tempBitmap.SetSource(e.ChosenPhoto);

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

                //Cut Root herein fahren
                CutRoot.Visibility = System.Windows.Visibility.Visible;
                CutRoot.Margin = new Thickness(0, 0, 0, 0);
                MenuOpen = true;

                //Menü umstellen
                AppBar02();

                
            }
        }
        //-----------------------------------------------------------------------------------------------------------------





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





        //Bild schneiden
        //---------------------------------------------------------------------------------------------------------------------------------
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

            //Images.dat erstellen
            ImagesAll += ImageName + ".jpg/";
            //Neue Image Datei erstellen
            filestream = file.CreateFile("/Thumbs/" + Folder + ".dat");
            sw = new StreamWriter(filestream);
            sw.WriteLine(ImagesAll);
            sw.Flush();
            filestream.Close();

            //Bild neu laden
            CreateImages();

            //Cut Root raus fahren
            CutRoot.Visibility = System.Windows.Visibility.Collapsed;
            CutRoot.Margin = new Thickness(-600, 0, 0, 0);
            MenuOpen = false;

            //AppBar zurückstellen
            AppBar01();
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Ordner auswählen //Kopieren
        //---------------------------------------------------------------------------------------------------------------------------------
        private void SelectFolder(object sender, SelectionChangedEventArgs e)
        {
            //Ordner Pfad laden
            string TargetPath = "/Folders/" + (datalist2[LBFolders.SelectedIndex] as ClassFolders).name + "/";
            string TargetThumbPath = "/Thumbs/" + (datalist2[LBFolders.SelectedIndex] as ClassFolders).name + "/";
            
            //Wenn Quelle anders als ziel
            if ((datalist2[LBFolders.SelectedIndex] as ClassFolders).name != Folder)
            {
                if (MessageBox.Show(Lockscreen_Swap.AppResx.CopyImagesTo + "\n" + (datalist2[LBFolders.SelectedIndex] as ClassFolders).name, Lockscreen_Swap.AppResx.Notification, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    //Images.dat laden //Zu kopierender Ordner
                    IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
                    IsolatedStorageFileStream filestream = file.OpenFile("Thumbs/" + (datalist2[LBFolders.SelectedIndex] as ClassFolders).name + ".dat", FileMode.Open);
                    StreamReader sr = new StreamReader(filestream);
                    string ImagesAll2 = sr.ReadToEnd();
                    filestream.Close();
                    ImagesAll2 = ImagesAll2.TrimEnd(new char[] { '\r', '\n' });

                    //ImagesToCopy Liste erstellen
                    ImagesToCopy = new string[ImagesToCopy_c];
                    //Ausgewählte Bilder durchlaufen
                    for (int i = 0; i < ImagesToCopy_c; i++)
                    {
                        //Bilder kopieren
                        IsolatedStorageFile file02 = IsolatedStorageFile.GetUserStoreForApplication();
                        if (!file02.FileExists(TargetPath + (LBImages.SelectedItems[i] as ClassPictures).imgFile))
                        {
                            file02.CopyFile(((LBImages.SelectedItems[i] as ClassPictures).imgPath), TargetPath + (LBImages.SelectedItems[i] as ClassPictures).imgFile);
                        }
                        if (!file02.FileExists(TargetThumbPath + (LBImages.SelectedItems[i] as ClassPictures).imgFile))
                        {
                            file02.CopyFile(((LBImages.SelectedItems[i] as ClassPictures).imgThumbPath), TargetThumbPath + (LBImages.SelectedItems[i] as ClassPictures).imgFile);
                        }

                        //ImagesAll Zu kopierender Ordner ändern
                        ImagesAll2 += (LBImages.SelectedItems[i] as ClassPictures).imgFile + "/";
                        //Images.dat //Zu kopierender ordner
                        filestream = file.CreateFile("Thumbs/" + (datalist2[LBFolders.SelectedIndex] as ClassFolders).name + ".dat");
                        StreamWriter sw = new StreamWriter(filestream);
                        sw.WriteLine(ImagesAll2);
                        sw.Flush();
                        filestream.Close();
                    }

                    //Auswahl aufheben
                    try
                    {
                        LBFolders.SelectedIndex = -1;
                    }
                    catch
                    {
                    }
                    //Copy zurücksetzen
                    CopyRoot.Margin = new Thickness(-600, 0, 0, 0);
                    //MenuOpene zurücksetzen
                    MenuOpen = false;
                }
                //Bei Abbruch
                else
                {
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
            //Wenn Quelle gleich Ziel
            else
            {
                //Auswahl aufheben
                try
                {
                    LBFolders.SelectedIndex = -1;
                }
                catch
                {
                }
                //Nachricht ausgeben
                MessageBox.Show(Lockscreen_Swap.AppResx.SourceTarget);
            }
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





        //Button Select, alle auswählen
        //---------------------------------------------------------------------------------------------------------------------------------
        //Ordnerliste öffnen
        private void BtnSelect(object sender, EventArgs e)
        {
            //Prüfen ob Multiselect aktiviert ist
            if (LBImages.SelectionMode == SelectionMode.Multiple)
            {
                LBImages.SelectAll();
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Button Unselect, alle abwählen
        //---------------------------------------------------------------------------------------------------------------------------------
        //ImgSelectSave erstellen
        bool ImgSelectSave = true;
        //Alle Bilder seselectieren
        private void BtnUnselect(object sender, EventArgs e)
        {
            //Prüfen ob Multiselect aktiviert ist
            if (LBImages.SelectionMode == SelectionMode.Multiple)
            {
                //ImgSelectSave auf false stellen
                ImgSelectSave = false;
                //Selection Mode umstellen
                LBImages.SelectionMode = SelectionMode.Single;
                //Selection entfernen
                try
                {
                    LBImages.SelectedIndex = -1;
                }
                catch
                {
                }
                //Selection Mode zurückstellen
                LBImages.SelectionMode = SelectionMode.Multiple;
                //ImgSelectSave auf true stellen
                ImgSelectSave = true;
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------





        //Back Button
        //---------------------------------------------------------------------------------------------------------------------------------
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (MenuOpen == true)
            {
                //Copy zurücksetzen
                CopyRoot.Margin = new Thickness(-600, 0, 0, 0);
                //Cut zurücksetzen
                CutRoot.Margin = new Thickness(-600, 0, 0, 0);
                CutRoot.Visibility = System.Windows.Visibility.Collapsed;
                //MenuOpene zurücksetzen
                MenuOpen = false;
                //AppBar ändern
                AppBar01();
                //Zurück abbrechen
                e.Cancel = true;
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------

        



    }
}