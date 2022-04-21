using Baymax.Logic.ViewModels;
using Stylet;
using StyletIoC;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Stylet.Logging;
using MessageBoxViewModel = Baymax.Logic.ViewModels.MessageBoxViewModel;

namespace Baymax.Logic
{
    public class Bootstrapper : Stylet.Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            builder.Bind<IViewManager>().To<ViewManager>();
            builder.Bind<IWindowManager>().To<WindowManager>();
            builder.Bind<IMessageBoxViewModel>().To<MessageBoxViewModel>();
            //var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).ToList();
            var modelBaseType = typeof(ViewModelBase);
            var viewBaseType = typeof(FrameworkElement);
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(x => x.Namespace is "Baymax.Views").ToList().ForEach(x =>
            {
                builder.Bind(x).ToSelf();
            });
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(x => modelBaseType.IsAssignableFrom(x) && x != modelBaseType && !x.IsAbstract && !x.IsInterface).ToList().ForEach(x =>
            {
                if (x == typeof(ShellViewModel))
                {
                    builder.Bind(x).To<ShellViewModel>().InSingletonScope();
                }
                else
                {
                    builder.Bind(x).ToSelf();
                }
            });
            LogManager.LoggerFactory = NLogger.GetLogger;
            base.ConfigureIoC(builder);
        }

        protected override void Configure()
        {
            base.Configure();
            var config = Container.Get<ViewManagerConfig>();
            config.ViewAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            IOC.GetInstance = Container.Get;
            IOC.GetAllInstances = Container.GetAll;
        }
    }
}
