using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DAW.Utils;

namespace DAW
{
    class MainWindowViewModel : ViewModelBase
    {
        private IModule? selectedModule;

        public MainWindowViewModel(IEnumerable<IModule> modules)
        {
            Modules = modules.OrderBy(m => m.Name).ToList();
            if (Modules.Count > 0)
            {
                SelectedModule = Modules[0];
            }
        }

        public List<IModule> Modules { get; }
        public ObservableCollection<FileInfo> Files { get; } = new ObservableCollection<FileInfo>();

        public IModule? SelectedModule
        {
            get => selectedModule;
            set
            {
                if (value != selectedModule)
                {
                    selectedModule?.Deactivate();
                    selectedModule = value;
                    OnPropertyChanged("SelectedModule");
                    OnPropertyChanged("UserInterface");
                }
            }
        }

        public UserControl? UserInterface => SelectedModule?.UserInterface;
    }
}
