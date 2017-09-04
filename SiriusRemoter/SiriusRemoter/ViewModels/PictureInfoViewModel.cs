using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace SiriusRemoter.ViewModels
{
    public class PictureInfoViewModel : INotifyPropertyChanged
    {
        #region INotify things

        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Constructors

        public PictureInfoViewModel(PictureViewerViewModel picViewerViewModel)
        {
            _pictureViewModel = picViewerViewModel;
            _pictureViewModel.PropertyChanged += PictureViewViewModelChange;
        }

        #endregion

        #region Members

        private PictureViewerViewModel _pictureViewModel;
        private string _urlSource;
        private string _fileName;
        private double _dpi;
        private double _width;
        private double _height;
        private int _bitDepth;
        private long _sizeKb;

        #endregion

        #region Properties

        /// <summary>
        /// In kilobytes
        /// </summary>
        public long SizeKB
        {
            get
            {
                return _sizeKb;
            }
            set
            {
                _sizeKb = value;
                OnPropertyChanged(nameof(SizeKB));
            }
        }

        public int BitDepth
        {
            get
            {
                return _bitDepth;
            }
            set
            {
                _bitDepth = value;
                OnPropertyChanged(nameof(BitDepth));
            }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public double Height
        {
            get
            {
                return _height;
            }
            set
            {
                if (_height == value)
                {
                    return;
                }
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }

        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                if (_width == value)
                {
                    return;
                }
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }

        public double DPI
        {
            get
            {
                return _dpi;
            }
            set
            {
                if (_dpi == value)
                {
                    return;
                }
                _dpi = value;
                OnPropertyChanged(nameof(DPI));
            }
        }

        #endregion

        #region Methods

        public static byte[] ImageToByte(BitmapImage imageSource)
        {
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageSource));

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }

        #endregion

        #region Event Handlers

        private void PictureViewViewModelChange(object sender, PropertyChangedEventArgs e)
        {
            var picViewerViewModel = sender as PictureViewerViewModel;
            if (picViewerViewModel == null)
            {
                return;
            }
            var bImage = (picViewerViewModel.SourceImage as BitmapImage);
            if (bImage == null)
            {
                return;
            }
            if (_urlSource ==null || !_urlSource.Equals(bImage.UriSource.ToString()))
            {
                bImage.DownloadCompleted += new EventHandler(bi_DownloadCompleted);
            }
        }

        /// <summary>
        /// When image has been downloaded, stop the progressring. Setting all
        /// properties here since before the download has finished, trying to retieve these
        /// data items returns garbage values.
        /// </summary>
        void bi_DownloadCompleted(object sender, EventArgs e)
        {
            try
            {
                var bImage = sender as BitmapImage;
                if (bImage == null)
                {
                    return;
                }
                _urlSource = bImage.UriSource.ToString();
                FileName = (_pictureViewModel.PlayerController.GetSelectedItem().Name);
                DPI = bImage.DpiX;
                Width = bImage.PixelWidth;
                Height = bImage.PixelHeight;
                BitDepth = bImage.Format.BitsPerPixel;
                var bvytes = ImageToByte(bImage);
                SizeKB = bvytes.Length / 1000;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }            
        }

        #endregion
    }
}
