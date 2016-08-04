using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WebCamTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_FormClosed;
        }
        
        private VideoCaptureDevice videoSource;

        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            FilterInfoCollection videoSources = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoSources != null)
            {
                videoSource = new VideoCaptureDevice(videoSources[0].MonikerString);

                try
                {
                    if (videoSource.VideoCapabilities.Length > 0)
                    {
                        string highestResolution = "0;0";

                        for (int i = 0; i < videoSource.VideoCapabilities.Length; i++)
                        {
                            highestResolution = videoSource.VideoCapabilities[i].FrameSize.Width.ToString() + ";" + i.ToString();
                        }

                        videoSource.VideoResolution = videoSource.VideoCapabilities[Convert.ToInt32(highestResolution.Split(';')[1])];
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }

                videoSource.NewFrame += new AForge.Video.NewFrameEventHandler(videoSource_NewFrame);
                videoSource.Start();
            }
        }

        void videoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            ImageSource imgSrc = BitmapToImageSource((Bitmap)eventArgs.Frame.Clone());

            try
            {
                img.Dispatcher.Invoke(new ChangeImageSourceDelegate(this.ChangeImageSource), new object[] { imgSrc });
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private delegate void ChangeImageSourceDelegate(ImageSource imgSrc);

        private void ChangeImageSource(ImageSource imgSrc)
        {
            img.Source = imgSrc;
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private void MainWindow_FormClosed(object sender, EventArgs e)
        {
            videoSource.SignalToStop();
            videoSource = null;
        }
    }
}