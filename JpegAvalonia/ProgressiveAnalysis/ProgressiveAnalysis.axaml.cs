using Avalonia.Controls;
using System.Threading.Tasks;
using JpegLib;
using System;
using Avalonia.Platform;
using Avalonia;

namespace JpegAvalonia
{
    public partial class ProgressiveAnalysis : UserControl
    {
        const string ImagePath = @".\Assets\progressive.jpg";

        public ProgressiveAnalysis()
        {
            InitializeComponent();

            DataContext = new ProgressiveAnalysisViewModel();

            _ = CreateProgressiveImages();
        }

        async Task CreateProgressiveImages()
        {
            try
            {
                if (DataContext is ProgressiveAnalysisViewModel vm)
                {
                    var steps = await DecodeProgressive.GetProgresiveSteps(ImagePath);
                    vm.SetSteps(steps);
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
