using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Toolkit.Mvvm.Input;
using WPF_Mandelbrot.Models;
using System.Windows;
using System.Numerics;
using System.Diagnostics;
using System.Threading;

namespace WPF_Mandelbrot.Presentation
{
    public class MainViewModel : ObservableObject
    {
        public Stopwatch stopwatch;
        public double OffsetX = 0;
        public double OffsetY = 0;
        public Point realCoord;
        private double _tempImagCrdTpL;
        private double _tempImagCrdBtR;
        public double ImageCoordTpL
        {
            get { return _tempImagCrdTpL; }
            set { _tempImagCrdTpL = value; OnPropertyChanged(); }
        }
        public double ImageCoordBtR
        {
            get { return _tempImagCrdBtR; }
            set { _tempImagCrdBtR = value; OnPropertyChanged(); }
        }
        private double _tempMouseX;
        private double _tempMouseY;
        public double MouseCoordinateX
        {
            get { return _tempMouseX; }
            set { _tempMouseX = value; OnPropertyChanged(); }
        }
        public double MouseCoordinateY
        {
            get { return _tempMouseY; }
            set { _tempMouseY = value; OnPropertyChanged(); }
        }
        public IRelayCommand ResetMandelbrot { get; set; }
        public double StepSize { get; set; } = 1;
        private readonly ILogic logic;
        public int[] iterableNumbers = new int[9];
        public WriteableBitmap BitmapDisplay { get; set; }
        public int MaxIterabeleNr { get; set; } = 50;
        public string SelectedColor { get; set; } = "COLOR";
        private const int maxWidth = 400; //Y
        private const int maxHeight = 400; //X
        //Based on https://stackoverflow.com/questions/45531674/how-to-draw-rectangle-by-coordinates-of-click
        public Rect Coordinates { get; set; } = new(new Point(-2.2, -1.5), new Point(1, 1.5));


        public MainViewModel(ILogic logic)
        {
            this.logic = logic;
            CreateBitmap(maxHeight, maxWidth);
            GenerateBitmapAsync(Coordinates, StepSize, 0, 0);
            ResetMandelbrot = new RelayCommand(ResetAllAsync);
        }

        private void CreateBitmap(int width, int height)
        {
            var pixelFormat = PixelFormats.Pbgra32;
            BitmapDisplay = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            OnPropertyChanged(nameof(BitmapDisplay));
        }


        //Based on https://www.i-programmer.info/projects/38/1072-mandelbrot-zoomer-in-wpf.html
        public async Task<WriteableBitmap> GenerateBitmapAsync(Rect crdts, double zoomFactor, double offsetX, double offsetY)
        {
            using (var cancellationSource = new CancellationTokenSource())
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
                var cancellationToken = cancellationSource.Token;
                int bytesPerPxl = BitmapDisplay.Format.BitsPerPixel / 8;
                byte[] byteOfPxls = new byte[maxHeight * maxWidth * bytesPerPxl];
                int bytesPerPxlRow = maxWidth * bytesPerPxl;//scale calculation
                double xscale = (crdts.Right - crdts.Left) / maxWidth;
                double yscale = (crdts.Top - crdts.Bottom) / maxHeight;
                int[] count = await Task.Run(() => logic.GenerateMandelbrot(crdts, bytesPerPxl, byteOfPxls, bytesPerPxlRow, xscale, yscale, offsetX, offsetY, MaxIterabeleNr, zoomFactor), cancellationToken);
                cancellationSource.Cancel();
                for (int i = 0; i < byteOfPxls.Length; i += bytesPerPxl)
                {
                    ColorVariation(i, count[i], byteOfPxls, SelectedColor);
                }
                BitmapDisplay.WritePixels(new Int32Rect(0, 0, maxWidth, maxHeight), byteOfPxls, bytesPerPxlRow, 0);
                stopwatch.Stop();
            }
            return BitmapDisplay;
        }
        

        //Give each pixel in differant position a unique color
        private void ColorVariation(int i, int count, byte[] byteOfPxls, string colorSelection)
        {
            if (colorSelection.Equals("COLOR"))
            {
                Color C = SetRandomColor(count);
                ColorPixelPosition(i, C, byteOfPxls);
            }
            else if (colorSelection.Equals("BANDING"))
            {
                Color C = SetBanding(count);
                ColorPixelPosition(i, C, byteOfPxls);
            }
            else if (colorSelection.Equals("GRAYSCALE"))
            {
                Color C = SetGrayScale(count);
                ColorPixelPosition(i, C, byteOfPxls);
            }
        }

        private static void ColorPixelPosition(int i, Color C, byte [] byteOfPxls)
        {
            byteOfPxls[i + 3] = C.A;
            byteOfPxls[i + 2] = C.R;
            byteOfPxls[i] = C.B;
            byteOfPxls[i + 1] = C.G;
        }

        private static Color SetBanding(int count)
        {
            Color C = new();
            if (count % 2 == 0)
            {
                C.A = 255;
                C.R = 0;
                C.B = 0;
                C.G = 0;
            }
            else {
                C.A = 255;
                C.R = 255;
                C.B = 255;
                C.G = 255;
            }
            return C;
        }

        private static Color SetGrayScale(int count)
        {
            //Based on https://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale
            Color C = new();
            C.A = 255;
            C.R = (byte)(count / 10 * 25);
            C.B = (byte)(count % 100 * 25);
            C.G = (byte)(count / 10 * 25);
            int avg = (C.R + C.G + C.B) / 3;
            return Color.FromArgb(C.A, (byte)avg, (byte)avg, (byte)avg);
        }

        //Iteration count correlates to color
        private static Color SetRandomColor(int count)
        {
            Color C = new ();
            C.A = 255;
            C.R = (byte)(count / 10 * 25);
            C.B = (byte)(count % 100 * 25);
            C.G = (byte)(count / 10 * 25);
            return C;
        }


        public void GetMousePositon(double delta)
        {
            StepSize = delta >= 0 ? StepSize *= 0.9 : StepSize /= 0.9;
            OnPropertyChanged(nameof(StepSize));
        }

        public void CalculateRealCoordinates(double imageWidth, double imageHeight, Point mousePosition)
        {
            //Actual coordinates for fractal
            double differenceX = Coordinates.TopLeft.X - Coordinates.TopRight.X;
            double differenceY = Coordinates.BottomLeft.Y + Coordinates.BottomRight.Y;
            var pixelX = (differenceX * mousePosition.X / imageWidth) - Coordinates.TopLeft.X;//* 2
            var pixelY = (differenceY * mousePosition.Y / imageHeight) - Coordinates.BottomRight.Y;// * 2
            realCoord = new Point(pixelX, pixelY);
            MouseCoordinateX = Math.Round(realCoord.X * -1, 4);
            MouseCoordinateY = Math.Round(realCoord.Y / -1, 4);
        }

        public async void CalculatePanningPositionAsync(double width, double height, Point mousePos)
        {
            CalculateRealCoordinates(width, height, mousePos);
            OffsetX = realCoord.X;
            OffsetY = realCoord.Y;
            double valX = OffsetX * StepSize / 2;
            double valY = OffsetY * StepSize / 2;
            BitmapDisplay = await GenerateBitmapAsync(Coordinates, StepSize, valX, valY);
        }

        public void CalculateMouseCornerCoords(double imageWidth, double imageHeight, Point mousePos)
        {
            double x = Coordinates.TopLeft.X - Coordinates.TopRight.X * mousePos.X / imageWidth;
            double y = Coordinates.BottomLeft.Y - Coordinates.BottomLeft.Y * mousePos.Y / imageHeight;
            Point cornerCoordinate = new Point(x, y);
            ImageCoordTpL = Math.Round(x, 4);//top-left corner
            ImageCoordBtR = Math.Round(y, 4);//bottom-right corner
        }
        private async void ResetAllAsync()
        {
            Coordinates = new(new Point(-2.2, -1.5), new Point(1, 1.5));
            StepSize = 1;//Default scale of 1
            OffsetX = 0;
            OffsetY = 0;
            MaxIterabeleNr = 50;
            BitmapDisplay = await GenerateBitmapAsync(Coordinates, StepSize, OffsetX, OffsetY);
        }
    }
}

