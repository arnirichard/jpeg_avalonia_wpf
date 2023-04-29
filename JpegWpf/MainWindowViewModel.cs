using JpegLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegWpf
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public BlockAnalysisData Analysis { get; set; } = BlockAnalysisData.CreateFrom(GetInitialBlock());
        public long RefreshTime { get; set; }

        public void SetNewBlock(int[] originalRgb)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Analysis = BlockAnalysisData.CreateFrom(originalRgb);

            OnPropertyChanged(nameof(Analysis));
            
            RefreshTime = stopwatch.ElapsedMilliseconds;

            OnPropertyChanged(nameof(RefreshTime));
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
