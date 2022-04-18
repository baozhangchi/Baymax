using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Baymax.Models
{
    [Serializable]
    public class BaymaxProjectModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> TestCases { get; set; } = new ObservableCollection<string>();

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
