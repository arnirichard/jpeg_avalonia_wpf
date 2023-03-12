using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection.Metadata;
using JpegLib;
using JpegAvaloniaAsync.ViewModels;

namespace JpegAvaloniaAsync.Views
{
    public partial class MainWindow : Window
    {
        WriteableBitmap writeableBitmap;

        public MainWindow()
        {
            InitializeComponent();

            quant_luminance.DataContext = Quant.QuantLuminance;
            quant_crominance.DataContext = Quant.QuantChrominance;

            var bitmap = new Bitmap(@".\Assets\bird.bmp");

            using (var stream = File.Open(@".\Assets\bird.bmp", FileMode.Open))
            {
                writeableBitmap = WriteableBitmap.Decode(stream);
            }

            birdImage.Source = bitmap;
            birdImage.PointerPressed += BirdImage_PointerPressed;

            Height = Screens.Primary.WorkingArea.Height * 0.8;
            Width = Screens.Primary.WorkingArea.Width * 0.8;
        }

        private unsafe void BirdImage_PointerPressed(object? sender, PointerEventArgs e)
        {
            var p = e.GetPosition(birdImage);

            var size = writeableBitmap.Size;
            int x = (int)(p.X / birdImage.Bounds.Width * size.Width);
            int y = (int)(p.Y / birdImage.Bounds.Height *  size.Height);

            if (x >= 0 && y >= 0)
            {
                int[] values = writeableBitmap.ReadPixels(x, y, 8, 8);
                if(values.Length == 64 && DataContext is MainWindowViewModel vm)
                    vm.SetNewBlock(values);
            }
        }
    }
}
