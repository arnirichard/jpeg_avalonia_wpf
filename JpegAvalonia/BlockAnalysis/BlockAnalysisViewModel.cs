using Avalonia.Media;
using JpegLib;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace JpegAvalonia
{
    public class BlockAnalysisViewModel : ViewModelBase
    {
        public BlockAnalysisData Analysis { get; set; } = BlockAnalysisData.CreateFrom(GetInitialBlock());
        public long RefreshTime { get; set; }
        
        public void SetNewBlock(int[] originalRgb)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Analysis = BlockAnalysisData.CreateFrom(originalRgb);

            this.RaisePropertyChanged(nameof(Analysis));

            RefreshTime = stopwatch.ElapsedMilliseconds;

            this.RaisePropertyChanged(nameof(RefreshTime));
        }

        public static int[] GetInitialBlock()
        {
            int[] rgb = new int[64];
            int[] colors = new int[] { GridPlot.Red, GridPlot.Orange, GridPlot.Blue, GridPlot.Black, GridPlot.White };
            for (int i = 0; i < rgb.Length; i++)
            {
                rgb[i] = colors[i % colors.Length];
            }
            return rgb;
        }
    }
}