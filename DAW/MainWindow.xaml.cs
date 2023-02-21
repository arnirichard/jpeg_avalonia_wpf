using DAW.FilterDesign;

using DAW.Transcription;
using DAW.Utils;
using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Closing += (s, args) =>
            {
                viewModel.Deactivate();
            };
            string? folder = GetLastFolder();
            if(folder != null && Directory.Exists(folder))
            {
                ApplyFolder(folder);
            }
            LoadPhoneModels.Load();

            //var zeros = Polynomial.GetZeros(1, Math.Sqrt(2), 1);
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            var bla = openFileDlg.InitialDirectory;
            var result = openFileDlg.ShowDialog();
            if (result.ToString() != string.Empty && Directory.Exists(openFileDlg.SelectedPath))
            {
                ApplyFolder(openFileDlg.SelectedPath);
                SaveFolder(openFileDlg.SelectedPath);
            }
        }

        const string FolderName = "folder";
        string? GetLastFolder()
        {
            if (File.Exists(FolderName))
                return File.ReadAllText(FolderName);
            return null;
        }

        void SaveFolder(string folderName)
        {
            try
            {
                File.WriteAllText(FolderName, folderName);
            }
            catch { }
        }
        
        void ApplyFolder(string folder)
        {
            viewModel.Files.Clear();
            viewModel.FileFormats.Clear();

            Dictionary<string, List<FileInfo>> formats = new();
            List<FileInfo>? list;
            foreach (var f in Directory.GetFiles(folder))
            {
                FileInfo fi = new FileInfo(f);
                if (fi.Name.EndsWith("wav"))
                {
                    viewModel.Files.Add(fi);
                    WaveFileReader waveFileReader = new WaveFileReader(fi.FullName);
                    if(waveFileReader.WaveFormat.BitsPerSample == 16 && !fi.Name.Contains("-"))
                    { 
                    }
                    string key = waveFileReader.WaveFormat.GetShortString();
                    if (!formats.TryGetValue(key, out list))
                        formats.Add(key, list = new());
                    list.Add(fi);
                }
            }
            foreach (var kvp in formats)
            {
                viewModel.FileFormats.Add(new KeyValuePair<string, List<FileInfo>>(kvp.Key, kvp.Value));
            }
            viewModel.Modules.ForEach(m => m.SetFolder(folder));
            viewModel.Folder = folder;
        }

        private void Files_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (fileList.SelectedItem is FileInfo fi)
                {
                    viewModel.LastFileName = fi.FullName;
                    viewModel.SelectedModule?.SetFile(fi.FullName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            fileList.SelectedItem = null;
        }

        private void Module_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (fileList.SelectedItem is FileInfo fi)
                {
                    viewModel.SelectedModule?.SetFile(fi.FullName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
