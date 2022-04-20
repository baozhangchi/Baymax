using Baymax.Dal;
using Microsoft.EntityFrameworkCore;
using StyletIoC;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Baymax.Models;
using Stylet;

namespace Baymax.Logic.ViewModels
{
    public class TestCaseViewModel : ViewModelBase
    {
        private readonly IContainer _container;

        public TestCaseViewModel(IContainer container)
        {
            _container = container;
        }

        public ObservableCollection<TestStep> Data { get; set; }

        public TestStep SelectedStep { get; set; }

        public TestCaseDetailViewModel DetailViewModel { get; set; }

        public bool CanDeleteStep => SelectedStep != null;

        public bool CanMoveUp => SelectedStep is { SortIndex: > 1 };

        public bool CanMoveDown => SelectedStep != null && SelectedStep.SortIndex < Data.Count;
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
            //var projectFile = model.ProjectFile;
            //foreach (var testCase in model.Project.TestCases)
            //{
            //    if (testCase.FullSource == Source)
            //    {
            //        testCase.StartUrl = StartUrl;
            //        break;
            //    }
            //}

            File.WriteAllText(model.ProjectFile, model.Project.ToXml(removeDefaultNamespaces: true));
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            DetailViewModel = new TestCaseDetailViewModel();
            DetailViewModel.SaveClick += async (s, e) =>
            {
                await SaveTestStep();
            };
            LoadData();
        }

        private async Task SaveTestStep()
        {
            using (var db = new BaymaxDbContext(TestCase.ConnectionString))
            {
                db.Entry(SelectedStep).State = SelectedStep.Id == 0 ? EntityState.Added : EntityState.Modified;
                await db.SaveChangesAsync();
            }
        }

        private void LoadData()
        {
            Execute.OnUIThread(async () =>
            {
                using (var db = new BaymaxDbContext(TestCase.ConnectionString))
                {
                    Data = new ObservableCollection<TestStep>(await db.TestSteps.AsNoTracking()
                        .OrderBy(x => x.SortIndex).ToListAsync());
                }
            });
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
