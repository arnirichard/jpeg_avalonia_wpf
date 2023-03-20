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

namespace JpegAvalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Height = Screens.Primary.WorkingArea.Height * 0.8;
            Width = Screens.Primary.WorkingArea.Width * 0.8;
        }
    }
}
