using Baymax.Models;
using MaterialDesignThemes.Wpf;
using StyletIoC;
using System.IO;
using System.Windows;
using Stylet;

namespace Baymax.Logic.ViewModels
{
    public class NewCaseViewModel : ViewModelBase
    {
        private readonly IContainer _container;

        public NewCaseViewModel(IContainer container)
        {
            _container = container;
            DisplayName = "新建用例";
        }

        /// <summary>
        /// 用例名称
        /// </summary>
        public string CaseName { get; set; }

        /// <summary>
        /// 项目所在目录
        /// </summary>
        public string ProjectFolder { get; set; }

        /// <summary>
        /// 启动地址
        /// </summary>
        public string StartUrl { get; set; }

        public bool CanConfirm => !string.IsNullOrWhiteSpace(CaseName) && !string.IsNullOrWhiteSpace(StartUrl);

        public BaymaxProjectModel Project { get; set; }

        public void Confirm()
        {
            if (!Directory.Exists(ProjectFolder))
            {
                Directory.CreateDirectory(ProjectFolder);
            }

            var caseFolder = Path.Combine(ProjectFolder, "cases");

            if (!Directory.Exists(caseFolder))
            {
                Directory.CreateDirectory(caseFolder);
            }

            var caseFile = Path.Combine(caseFolder, $"{CaseName}.bmc");

            if (File.Exists(caseFile))
            {
                IOC.Get<IWindowManager>().ShowMessageBox($"已存在名称为【{CaseName}】的用例", "提示", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var caseModel = new BaymaxCaseModel()
            {
                Name = CaseName,
                StartUrl = StartUrl,
                Source = Path.GetRelativePath(ProjectFolder, caseFile),
                FullSource = caseFile
            };
            Project.TestCases.Add(caseModel);

            using (var db = new Baymax.Dal.BaymaxDbContext(caseModel.ConnectionString))
            {
                db.Database.EnsureCreated();
            }

            DialogHost.Close(null, caseFile);
        }

        public void Cancel()
        {
            DialogHost.Close(null);
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(CaseName):
                case nameof(StartUrl):
                    NotifyOfPropertyChange(nameof(CanConfirm));
                    break;
            }
        }
    }
}
