using System.Windows;
using System.Windows.Controls;

namespace Baymax.Controls.TemplateSelectors
{
    public class DialogHostWindowDialogContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ObjecTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is FrameworkElement)
            {
                return base.SelectTemplate(item, container);
            }

            return ObjecTemplate;
        }
    }
}
