using Baymax.Dal;
using Baymax.Dal.Attributes;
using Baymax.Dal.Enums;
using Baymax.Models;
using MaterialDesignThemes.Wpf;
using OpenQA.Selenium;
using Selenium.Handler;
using Stylet;
using Stylet.Logging;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Xml;

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

        public BaymaxCaseModel CurrentCase { get; set; }

        public string ProjectFile { get; set; }

        public bool CanNewCase => Project != null;

        public bool CanClose => Project != null;

        public bool CanDeleteCase => CurrentCase != null;

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
                await File.WriteAllTextAsync(ProjectFile, Project.ToXml(removeDefaultNamespaces: true));
                foreach (var testCase in Project.TestCases)
                {
                    testCase.FullSource = Path.Combine(Path.GetDirectoryName(ProjectFile) ?? string.Empty, testCase.Source);
                }

                CurrentCase = Project.TestCases.FirstOrDefault(x => x.FullSource == caseFile);
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

        /// <summary>
        /// 打开关于窗口
        /// </summary>
        public async void ShowAbout()
        {
            await Window.ShowDialog(IOC.Get<AboutViewModel>());
        }

        /// <summary>
        /// 关闭当前项目
        /// </summary>
        public void Close()
        {
            CurrentCase = null;
            Project = null;
        }

        /// <summary>
        /// 删除用例
        /// </summary>
        public void DeleteCase()
        {
            if (IOC.Get<IWindowManager>().ShowMessageBox($"确定要删除用例【{CurrentCase.Name}】吗？", "提示",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                File.Delete(CurrentCase.FullSource);
                var screenshotsFolder = Path.Combine(Path.GetDirectoryName(CurrentCase.FullSource) ?? string.Empty,
                    "results", CurrentCase.Name);
                if (Directory.Exists(screenshotsFolder))
                {
                    Directory.Delete(screenshotsFolder, true);
                }
                Project.TestCases.Remove(CurrentCase);
                File.WriteAllText(ProjectFile, Project.ToXml(removeDefaultNamespaces: true));
                CurrentCase = Project.TestCases?.FirstOrDefault();
            }
        }

        /// <summary>
        /// 打开项目
        /// </summary>
        /// <param name="projectFile"></param>
        private void OpenProject(string projectFile)
        {
            ProjectFile = projectFile;
            var xml = File.ReadAllText(ProjectFile);
            Project = xml.ToObject<BaymaxProjectModel>();
            if (Project.TestCases?.Count > 0)
            {
                foreach (var testCase in Project.TestCases)
                {
                    testCase.FullSource = Path.Combine(Path.GetDirectoryName(ProjectFile) ?? string.Empty, testCase.Source);
                }
            }

            CurrentCase = Project.TestCases?.FirstOrDefault();
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(Project):
                    NotifyOfPropertyChange(nameof(CanNewCase));
                    NotifyOfPropertyChange(nameof(CanClose));
                    DisplayName = Project == null ? "大白" : $"大白 - {Project.Name}";
                    break;
                case nameof(CurrentCase):
                    NotifyOfPropertyChange(nameof(CanDeleteCase));
                    break;
            }
        }
    }
}
