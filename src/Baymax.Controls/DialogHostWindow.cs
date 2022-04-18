using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;

namespace Baymax.Controls
{
    public class DialogHostWindow : Window
    {
        static DialogHostWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogHostWindow), new FrameworkPropertyMetadata(typeof(DialogHostWindow)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (s, e) => { SystemCommands.MinimizeWindow(this); }));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (s, e) => { SystemCommands.MaximizeWindow(this); }));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (s, e) => { SystemCommands.RestoreWindow(this); }));
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (s, e) => { SystemCommands.CloseWindow(this); }));
        }

        public Brush CaptionBackground
        {
            get { return (Brush)GetValue(CaptionBackgroundProperty); }
            set { SetValue(CaptionBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CaptionBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionBackgroundProperty =
            DependencyProperty.Register("CaptionBackground", typeof(Brush), typeof(DialogHostWindow), new PropertyMetadata(default(Brush)));


    }
}
