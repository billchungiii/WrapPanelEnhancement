using BindingLibrary;
using EnancedWrapPanelSample001.Panels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnancedWrapPanelSample001
{
    public class MainViewModel : NotifyPropertyBase
    {
        private ObservableCollection<string> _items;
        public ObservableCollection<string> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

     

        public MainViewModel()
        {
            Items = new ObservableCollection<string>();          
        }

        public ICommand AddCommand
        {
            get
            {
                return new RelayCommand((x) =>
                {
                    Items.Add($" == Item {Items.Count} ==");
                });
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return new RelayCommand((x) =>
                {
                    Items.Clear();
                });
            }
        }
    }
}
