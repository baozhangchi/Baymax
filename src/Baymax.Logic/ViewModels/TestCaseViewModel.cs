using System;
using System.Collections.Generic;
using Baymax.Dal;
using Microsoft.EntityFrameworkCore;
using StyletIoC;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using System.Xml;
using Baymax.Dal.Attributes;
using Baymax.Dal.Enums;
using Baymax.Models;
using OpenQA.Selenium;
using Selenium.Handler;
using Stylet;
using Stylet.Logging;

namespace Baymax.Logic.ViewModels
{
    public class TestCaseViewModel : ViewModelBase
    {
        private readonly IContainer _container;

        public TestCaseViewModel(IContainer container)
        {
            _container = container;
            DetailViewModel = new TestCaseDetailViewModel();
            DetailViewModel.SaveClick += async (s, e) =>
            {
                await SaveTestStep();
            };
        }

        public ObservableCollection<TestStep> Data { get; set; }

        public TestStep SelectedStep { get; set; }

        public TestCaseDetailViewModel DetailViewModel { get; set; }

        public bool CanDeleteStep => SelectedStep != null;

        public bool CanMoveUp => SelectedStep is { SortIndex: > 1 };

        public bool CanMoveDown => SelectedStep != null && SelectedStep.SortIndex < Data.Count;

        public bool CanShowHistory { get; private set; }

        public BaymaxCaseModel TestCase { get; set; }


        public void AddStep()
        {
            Data.Add(new TestStep());
            for (int i = 0; i < Data.Count; i++)
            {
                Data[i].SortIndex = i + 1;
            }

            SelectedStep = Data.LastOrDefault();
        }

        public async void Run()
        {
            var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var driver = WebDriverFactory.CreateDriver(DriverType.Chrome, headLess: false);
            driver.Manage().Window.Maximize();
            try
            {
                driver.Navigate().GoToUrl(TestCase.StartUrl);
                using (var db = new BaymaxDbContext(TestCase.ConnectionString))
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
                                        if (step.ClearValue)
                                        {
                                            element.Clear();
                                        }

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

                                        if (step.OutputScreenhot)
                                        {
                                            var screenHotFile =
                                                Path.Combine(Path.GetDirectoryName(TestCase.FullSource),
                                                    "results", TestCase.Name,
                                                    timeStamp.ToString(),
                                                    $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.png");
                                            if (!Directory.Exists(Path.GetDirectoryName(screenHotFile)))
                                            {
                                                Directory.CreateDirectory(Path.GetDirectoryName(screenHotFile));
                                            }

                                            driver.ExecuteScript("arguments[0].scrollIntoView();", element);

                                            if (step.ParentLevel > 0)
                                            {
                                                var level = step.ParentLevel;
                                                IWebElement parent = null;
                                                do
                                                {

                                                    parent = (parent ?? element).FindElement(By.XPath(".."));
                                                    level--;
                                                } while (level > 0);
                                                ((ITakesScreenshot)parent).GetScreenshot()
                                                    .SaveAsFile(screenHotFile, ScreenshotImageFormat.Png);

                                            }
                                            else
                                            {
                                                ((ITakesScreenshot)element).GetScreenshot()
                                                    .SaveAsFile(screenHotFile, ScreenshotImageFormat.Png);
                                            }

                                            testResult.ScreenshotPath = screenHotFile;
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
                    CanShowHistory = await db.TestHistories.CountAsync() > 0;
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

        public async void MoveUp()
        {
            using (var db = new BaymaxDbContext(TestCase.ConnectionString))
            {
                var previousItem = Data.First(x => x.SortIndex == SelectedStep.SortIndex - 1);
                previousItem.SortIndex += 1;
                SelectedStep.SortIndex -= 1;
                db.Entry(previousItem).State = EntityState.Modified;
                db.Entry(SelectedStep).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            LoadData();
        }

        public async void MoveDown()
        {
            using (var db = new BaymaxDbContext(TestCase.ConnectionString))
            {
                var nextItem = Data.First(x => x.SortIndex == SelectedStep.SortIndex + 1);
                nextItem.SortIndex -= 1;
                SelectedStep.SortIndex += 1;
                db.Entry(nextItem).State = EntityState.Modified;
                db.Entry(SelectedStep).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            LoadData();
        }

        public async void DeleteStep()
        {
            if (SelectedStep.Id > 0)
            {
                using (var db = new BaymaxDbContext(TestCase.ConnectionString))
                {
                    db.Remove(SelectedStep);
                    Data.Remove(SelectedStep);
                    for (int i = 0; i < Data.Count; i++)
                    {
                        Data[i].SortIndex = i + 1;
                    }

                    db.UpdateRange(Data);
                    await db.SaveChangesAsync();
                }
            }

            Data.Remove(SelectedStep);
        }

        public void Save()
        {
            var model = _container.Get<ShellViewModel>();
            File.WriteAllText(model.ProjectFile, model.Project.ToXml(removeDefaultNamespaces: true));
        }

        public async void ShowHistory()
        {
            var model = IOC.Get<HistoryViewModel>();
            model.TestCase = TestCase;
            await Window.ShowDialog(model);
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            LoadData();
        }

        private async Task SaveTestStep()
        {
            using (var db = new BaymaxDbContext(TestCase.ConnectionString))
            {
                db.Entry(SelectedStep).State = SelectedStep.Id == 0 ? EntityState.Added : EntityState.Modified;
                await db.SaveChangesAsync();
            }

            LoadData();
        }

        private async void LoadData()
        {
            using (var db = new BaymaxDbContext(TestCase.ConnectionString))
            {
                var data = await db.TestSteps.AsNoTracking()
                    .OrderBy(x => x.SortIndex).ToListAsync();
                var canShowHistoty = await db.TestHistories.CountAsync() > 0;
                Execute.OnUIThread(() =>
                {
                    Data = new ObservableCollection<TestStep>(data);
                    CanShowHistory = canShowHistoty;
                });
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(SelectedStep):
                    NotifyOfPropertyChange(nameof(CanDeleteStep));
                    DetailViewModel.OriginStep = SelectedStep;
                    break;
            }
        }
    }
}
