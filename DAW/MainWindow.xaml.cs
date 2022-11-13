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

            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = openFileDlg.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                if (Directory.Exists(openFileDlg.SelectedPath))
                {
                    viewModel.Files.Clear();

                    foreach (var f in Directory.GetFiles(openFileDlg.SelectedPath))
                    {
                        FileInfo fi = new FileInfo(f);
                        if (fi.Name.EndsWith("wav"))
                            viewModel.Files.Add(fi);
                    }

                    viewModel.Modules.ForEach(m => m.SetFolder(openFileDlg.SelectedPath));
                    viewModel.Folder = openFileDlg.SelectedPath;
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

        private void Module_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (fileList.SelectedItem is FileInfo fi)
            {
                viewModel.SelectedModule?.SetFile(fi.FullName);
            }
        }
    }
}
