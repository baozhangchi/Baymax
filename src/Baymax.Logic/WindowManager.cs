using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Stylet;

namespace Baymax.Logic
{
    public class WindowManager : Stylet.WindowManager
    {
        public WindowManager(IViewManager viewManager, Func<IMessageBoxViewModel> messageBoxViewModelFactory, IWindowManagerConfig config) : base(viewManager, messageBoxViewModelFactory, config)
        {
        }

        protected override Window CreateWindow(object viewModel, bool isDialog, IViewAware ownerViewModel)
        {
            var window = base.CreateWindow(viewModel, isDialog, ownerViewModel);
            window.ShowInTaskbar = !isDialog;
            window.WindowStartupLocation = window.Owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
            return window;
        }
    }
}
