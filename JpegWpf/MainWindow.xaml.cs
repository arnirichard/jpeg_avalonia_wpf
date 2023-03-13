using JpegLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JpegWpf
{
    public partial class MainWindow : Window
    {
        WriteableBitmap writeableBitmap;

        public MainWindow()
        {
            InitializeComponent();

            BitmapImage bitmap = new BitmapImage(new Uri(@".\Assets\bird.bmp", UriKind.Relative));
            writeableBitmap = new WriteableBitmap(bitmap);
            birdImage.Source = bitmap;

            DataContext = new MainWindowViewModel();

            quant_luminance.DataContext = Quant.QuantLuminance;
            quant_crominance.DataContext = Quant.QuantChrominance;
            zigzag.DataContext = Zigzag.ZIGZAG;

            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight * 0.8;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth * 0.8;
        }

        private void birdImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (birdImage.ActualWidth <= 0 || birdImage.ActualHeight <= 0)
                return;

            var p = e.GetPosition(birdImage);

            int x = (int)(p.X / birdImage.ActualWidth * writeableBitmap.PixelWidth);
            int y = (int)(p.Y / birdImage.ActualHeight * writeableBitmap.PixelHeight);

            if (x >= 0 && y >= 0)
            {
                int[] values = writeableBitmap.ReadPixels(x, y, 8, 8);
                if (values.Length == 64 && DataContext is MainWindowViewModel vm)
                    vm.SetNewBlock(values);
            }
        }
    }
}
