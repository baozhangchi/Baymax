using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Baymax.Controls;
using Stylet;

namespace Baymax.Logic
{
    public abstract class ViewModelBase : Screen
    {
        public DialogHostWindow Window
        {
            get
            {
                if (View == null)
                {
                    return null;
                }

                if (View is DialogHostWindow window)
                {
                    return window;
                }

                window = View.ParentOfType<DialogHostWindow>();
                return window;
            }
        }
    }
}
