using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Baymax.Dal;
using Baymax.Dal.Attributes;
using Baymax.Dal.Enums;
using Baymax.Models;
using MaterialDesignThemes.Wpf;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Selenium.Handler;
using Stylet;
using Stylet.Logging;
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

        public BaymaxCaseModel CurrentCase { get; set; }

        public string ProjectFile { get; set; }

        public bool CanNewCase => Project != null;

        public bool CanRun => CurrentCase != null;

        public async void Run()
        {
            var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var driver = WebDriverFactory.CreateDriver(DriverType.Chrome, headLess: false);
            try
            {
                driver.Navigate().GoToUrl(CurrentCase.StartUrl);
                using (var db = new BaymaxDbContext(CurrentCase.ConnectionString))
                {
                    List<TestHistory> testHistories = new List<TestHistory>();
                    foreach (var step in db.TestSteps.OrderBy(x => x.SortIndex).ToList())
                    {
                        if (step.ActionType == ActionType.JumpToAddress)
                        {
                            driver.Navigate().GoToUrl(step.JumpAddress);
                        }
                        else if (step.ActionType == ActionType.GetElement)
                        {
                            ReadOnlyCollection<IWebElement> elements = null;
                            if (step.ElementGetType != null)
                            {
                                var method = typeof(By).GetMethod(
                                    step.ElementGetType.Value.GetAttribute<ActionNameAttribute>(),
                                    BindingFlags.Public | BindingFlags.Static);
                                if (method != null)
                                {
                                    var @by = (By)method.Invoke(null, new object?[] { step.ElementGetValue });

                                    if (step.GetMultipleElements)
                                    {
                                        elements = driver.FindElements(@by);
                                    }
                                    else
                                    {
                                        elements = new ReadOnlyCollection<IWebElement>(new List<IWebElement>()
                                            {driver.FindElement(@by)});
                                    }
                                }
                            }

                            if (elements is { Count: > 0 })
                            {
                                if (step.ElementEvent == ElementEvent.Click)
                                {
                                    foreach (var element in elements)
                                    {
                                        element.Click();
                                    }
                                }
                                else if (step.ElementEvent == ElementEvent.Enter)
                                {
                                    foreach (var element in elements)
                                    {
                                        element.SendKeys(step.EnterValue);
                                    }
                                }
                                else if (step.ElementEvent == ElementEvent.Verification)
                                {
                                    TestHistory history = (TestHistory)step;
                                    history.TimeStamp = timeStamp;
                                    foreach (var element in elements)
                                    {
                                        var verificateResult = true;
                                        var message = string.Empty;
                                        switch (step.VerificationType)
                                        {
                                            case VerificationType.Contain:
                                                verificateResult = element.Text.Contains(step.VerificationValue);
                                                if (!verificateResult)
                                                    message = $"{element.Text}不包含{step.VerificationValue}";
                                                break;
                                            case VerificationType.Equal:
                                                verificateResult = element.Text.Equals(step.VerificationValue);
                                                if (!verificateResult)
                                                    message = $"{element.Text}不等于{step.VerificationValue}";
                                                break;
                                            case VerificationType.NotContain:
                                                verificateResult = !element.Text.Contains(step.VerificationValue);
                                                if (!verificateResult)
                                                    message = $"{element.Text}包含{step.VerificationValue}";
                                                break;
                                            case VerificationType.NotEqual:
                                                verificateResult = !element.Text.Equals(step.VerificationValue);
                                                if (!verificateResult)
                                                    message = $"{element.Text}等于{step.VerificationValue}";
                                                break;

                                        }

                                        TestResult testResult = new TestResult
                                        {
                                            VerificationResult = verificateResult,
                                            HistoryId = step.Id,
                                            Message = message
                                        };
                                        if (!verificateResult)
                                        {
                                            if (step.OutputScreenhot)
                                            {
                                                var screenHotFile =
                                                    Path.Combine(Path.GetDirectoryName(CurrentCase.FullSource),
                                                        "results",
                                                        timeStamp.ToString(),
                                                        $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.png");
                                                if (!Directory.Exists(Path.GetDirectoryName(screenHotFile)))
                                                {
                                                    Directory.CreateDirectory(Path.GetDirectoryName(screenHotFile));
                                                }

                                                driver.ExecuteScript("arguments[0].scrollIntoView();", element);
                                                ((ITakesScreenshot)element).GetScreenshot().SaveAsFile(screenHotFile, ScreenshotImageFormat.Png);
                                                testResult.ScreenshotPath = screenHotFile;
                                            }
                                        }

                                        history.Results.Add(testResult);
                                    }

                                    testHistories.Add(history);
                                }
                            }
                        }
                    }

                    db.TestHistories.AddRange(testHistories);
                    await db.SaveChangesAsync();
                }
            }
            catch (TargetInvocationException targetInvocationException)
            {
                LogManager.GetLogger(typeof(ShellViewModel)).Error(targetInvocationException);
            }
            catch (Exception exception)
            {
                LogManager.GetLogger(typeof(ShellViewModel)).Error(exception);
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }

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

        private void OpenProject(string projectFile)
        {
            ProjectFile = projectFile;
            var xml = File.ReadAllText(projectFile);
            Project = File.ReadAllText(projectFile).ToObject<BaymaxProjectModel>();
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
                    DisplayName = Project == null ? "大白" : $"大白 - {Project.Name}";
                    break;
                case nameof(CurrentCase):
                    NotifyOfPropertyChange(nameof(CanRun));
                    break;
            }
        }
    }
}
