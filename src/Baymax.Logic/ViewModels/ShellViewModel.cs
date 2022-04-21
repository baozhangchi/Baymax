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

        public void Close()
        {
            CurrentCase = null;
            Project = null;
        }

        public void DeleteCase()
        {
            if (IOC.Get<IWindowManager>().ShowMessageBox($"确定要删除用例【{CurrentCase.Name}】吗？", "提示",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                Project.TestCases.Remove(CurrentCase);
                File.WriteAllText(ProjectFile, Project.ToXml(removeDefaultNamespaces: true));
                File.Delete(CurrentCase.FullSource);
                var screenhotFolder = Path.Combine(Path.GetDirectoryName(CurrentCase.FullSource),
                    "results", CurrentCase.Name);
                if (Directory.Exists(screenhotFolder))
                {
                    Directory.Delete(screenhotFolder, true);
                }
                CurrentCase = Project.TestCases?.FirstOrDefault();
            }
        }

        private void OpenProject(string projectFile)
        {
            ProjectFile = projectFile;
            var xml = File.ReadAllText(ProjectFile);
            Project = File.ReadAllText(ProjectFile).ToObject<BaymaxProjectModel>();
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
            }
        }
    }
}
