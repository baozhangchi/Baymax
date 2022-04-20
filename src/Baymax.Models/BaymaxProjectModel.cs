using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Baymax.Models
{
    [XmlRoot("Project", Namespace = null)]
    public class BaymaxProjectModel : INotifyPropertyChanged
    {
        [XmlElement("TestCase")]
        public ObservableCollection<BaymaxCaseModel> TestCases { get; set; } = new ObservableCollection<BaymaxCaseModel>();

        [XmlAttribute("Name")]
        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BaymaxCaseModel : INotifyPropertyChanged
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Source { get; set; }

        [XmlIgnore]
        public string FullSource { get; set; }

        [XmlIgnore]
        public string ConnectionString => $"data source={FullSource}";

        [XmlElement]
        public string StartUrl { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
