using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Baymax.Logic.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        public AboutViewModel()
        {
            DisplayName = "关于";
        }

        public void SendEmail(RequestNavigateEventArgs e)
        {
            Clipboard.SetText(e.Uri.OriginalString);
            e.Handled = true;
        }

        public List<string> Statement => Application.Current.Resources["Statement"].ToString()?.Split("\\r\\n").ToList();
    }
}
