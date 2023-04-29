using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using JpegLib;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JpegAvalonia
{
    public class ProgressiveStepViewModel
    {
        public string ScanInfo { get; }
        public WriteableBitmap WriteableBitmap { get; }

        public ProgressiveStepViewModel(string scanData, WriteableBitmap writeableBitmap)
        {
            ScanInfo = scanData;
            WriteableBitmap = writeableBitmap;
        }
    }

    public class ProgressiveAnalysisViewModel : ViewModelBase
    {
        public List<ProgressiveStepViewModel> Steps { get; private set; } = new();

        internal void SetSteps(List<ProgressiveStep> steps)
        {
            Steps = new();

            foreach (var step in steps)
            {
                Steps.Add(new ProgressiveStepViewModel(
                    step.Scan.ToString(),
                    FromBitmapData(step.Bmp)
                //RgbBlocks.ConvertRgbBlocksToIntArray(step.Bmp.RgbBlocks, step.Bmp.Width, step.Bmp.Height),
                //step.Bmp.Width
                ));
            }

            this.RaisePropertyChanged(nameof(Steps));
        }

        WriteableBitmap FromBitmapData(BmpData bmpData)
        {
            Vector dpi = new Vector(96, 96);

            var bitmap = new WriteableBitmap(
                new PixelSize(bmpData.Width, bmpData.Height),
                dpi,
                PixelFormat.Bgra8888,
                AlphaFormat.Unpremul);

            var rgb = RgbBlocks.ConvertRgbBlocksToByteArray(bmpData.RgbBlocks, bmpData.Width, bmpData.Height);

            using (var frameBuffer = bitmap.Lock())
            {
                Marshal.Copy(rgb, 0, frameBuffer.Address, rgb.Length);
            }

            return bitmap;
        }

    }
    }
