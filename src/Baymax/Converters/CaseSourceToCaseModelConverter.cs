using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Baymax.Logic;
using Baymax.Logic.ViewModels;
using Baymax.Models;

namespace Baymax.Converters
{
    public class CaseSourceToCaseModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BaymaxCaseModel caseModel)
            {
                var model = IOC.Get<TestCaseViewModel>();
                model.TestCase = caseModel;
                return model;
            }

            return default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default;
        }
    }
}
