using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using JpegLib;
using System.IO;

namespace JpegAvalonia
{
    public partial class BlockAnalysis : UserControl
    {
        WriteableBitmap writeableBitmap;
        const string ImagePath = @".\Assets\bird.bmp";

        public BlockAnalysis()
        {
            InitializeComponent();

            DataContext = new BlockAnalysisViewModel();

            var bitmap = new Bitmap(ImagePath);

            using (var stream = File.Open(ImagePath, FileMode.Open))
            {
                writeableBitmap = WriteableBitmap.Decode(stream);
            }

            birdImage.Source = bitmap;
            birdImage.PointerPressed += BirdImage_PointerPressed;

            quant_luminance.DataContext = Quant.QuantLuminance;
            quant_crominance.DataContext = Quant.QuantChrominance;
            zigzag.DataContext = Zigzag.ZIGZAG;
        }

        private unsafe void BirdImage_PointerPressed(object? sender, PointerEventArgs e)
        {
            var p = e.GetPosition(birdImage);

            var size = writeableBitmap.Size;
            int x = (int)(p.X / birdImage.Bounds.Width * size.Width);
            int y = (int)(p.Y / birdImage.Bounds.Height * size.Height);

            if (x >= 0 && y >= 0)
            {
                int[] values = writeableBitmap.ReadPixels(x, y, 8, 8);
                if (values.Length == 64 && DataContext is BlockAnalysisViewModel vm)
                    vm.SetNewBlock(values);
            }
        }
    }
}
