using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Baymax.Models;
using MaterialDesignThemes.Wpf;
using OpenQA.Selenium.Chrome;
using Stylet;
using StyletIoC;

namespace Baymax.Logic.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly IContainer _container;

        public ShellViewModel(IContainer container)
        {
            _container = container;
            DisplayName = "大白";
        }

        public BaymaxProjectModel Project { get; set; }

        public ObservableCollection<TestCaseViewModel> TestCaseModels { get; set; }

        public string ProjectFile { get; set; }

        public bool CanNewCase => Project != null;

        /// <summary>
        /// 退出
        /// </summary>
        public void Exit()
        {
            RequestClose();
        }

        /// <summary>
        /// 新建项目
        /// </summary>
        public async void New()
        {
            var model = _container.Get<NewProjectViewModel>();
            var dialogResult = await Window.ShowDialog(model);
            if (dialogResult is string projectFile)
            {
                OpenProject(projectFile);
            }
        }

        /// <summary>
        /// 新建用例
        /// </summary>
        public async void NewCase()
        {
            var model = _container.Get<NewCaseViewModel>();
            model.Project = Project;
            model.ProjectFolder = Path.GetDirectoryName(ProjectFile);
            var dialogResult = await Window.ShowDialog(model);
            if (dialogResult is string caseFile)
            {
                await File.WriteAllTextAsync(ProjectFile, Project.ToXml());
                OpenTestCase(caseFile);
            }
        }

        /// <summary>
        /// 打开
        /// </summary>s
        public void Open()
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "项目文件|*.bmpro";
            dialog.Title = "打开项目文件";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OpenProject(dialog.FileName);
            }
        }

        private void OpenProject(string projectFile)
        {
            ProjectFile = projectFile;
            var xml = File.ReadAllText(projectFile);
            if (string.IsNullOrWhiteSpace(xml))
            {
                Project = new BaymaxProjectModel();
            }
            else
            {
                Project = File.ReadAllText(projectFile).ToObject<BaymaxProjectModel>();
            }
        }

        private void OpenTestCase(string caseFile)
        {

        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(Project):
                    NotifyOfPropertyChange(nameof(CanNewCase));
                    break;
            }
        }
    }
}
