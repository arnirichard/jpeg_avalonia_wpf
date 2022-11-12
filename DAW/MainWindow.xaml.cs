using DAW.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DAW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            var modules = ReflectionHelper.CreateAllInstancesOf<IModule>();
            viewModel = new MainWindowViewModel(modules);
            DataContext = viewModel;
            Closing += (s, args) => viewModel.SelectedModule.Deactivate();
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog folderBrowser = new OpenFileDialog();
            // Set validate names and check file exists to false otherwise windows will
            // not let you select "Folder Selection."
            folderBrowser.Filter = "All Supported Files (*.wav;*.mp3;*.pcm)|*.wav;*.mp3;*.pcm|All Files (*.*)|*.*";
            folderBrowser.ValidateNames = false;
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;

            // Always default to Folder Selection.
            //folderBrowser.FileName = "Folder Selection.";
            if (folderBrowser.ShowDialog() == true)
            {
                string? folder = Path.GetDirectoryName(folderBrowser.FileName);

                if(Directory.Exists(folder))
                {
                    viewModel.Files.Clear();

                    foreach (var f in Directory.GetFiles(folder))
                    {
                        FileInfo fi = new FileInfo(f);
                        if(fi.Name.EndsWith("wav"))
                            viewModel.Files.Add(fi);
                    }
                }

                if (File.Exists(folderBrowser.FileName))
                {
                    viewModel.SelectedModule?.SetFile(folderBrowser.FileName);
                }    
            }
        }

        private void Files_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(fileList.SelectedItem is FileInfo fi)
            {
                viewModel.SelectedModule?.SetFile(fi.FullName);
            }
        }
    }
}
