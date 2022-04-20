using Baymax.Dal;
using System;

namespace Baymax.Logic.ViewModels
{
    public class TestCaseDetailViewModel : ViewModelBase
    {
        public TestCaseDetailViewModel()
        {
            DisplayName = "步骤详情";
        }

        public TestStep Step { get; set; }

        public TestStep OriginStep { get; set; }

        public void Save()
        {
            foreach (var propertyInfo in typeof(TestStep).GetProperties())
            {
                propertyInfo.SetValue(OriginStep, propertyInfo.GetValue(Step));
            }

            SaveClick?.Invoke(this, EventArgs.Empty);
        }

        public void Cancel()
        {
            Step = DeepCopy.DeepCopier.Copy(OriginStep);
        }

        public event EventHandler SaveClick;

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(OriginStep):
                    Step = DeepCopy.DeepCopier.Copy(OriginStep);
                    break;
            }
        }
    }
}
