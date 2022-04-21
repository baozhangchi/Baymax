using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Baymax.Dal;
using Baymax.Models;
using Microsoft.EntityFrameworkCore;

namespace Baymax.Logic.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        public HistoryViewModel()
        {
            DisplayName = "测试记录";
        }

        public BaymaxCaseModel TestCase { get; set; }

        public void HistoriesSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> args)
        {
            if (args.NewValue is TestHistory history)
            {
                CurrentHistory = history;
            }
            else
            {
                CurrentHistory = null;
            }
        }

        public TestHistory CurrentHistory { get; set; }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            LoadHistoryData();
        }

        public void Copy(string content)
        {
            Clipboard.SetText(content);
        }

        public void CopyScreenshot(BitmapSource source)
        {
            Clipboard.SetImage(source);
        }

        private async void LoadHistoryData()
        {
            using (var db = new BaymaxDbContext(TestCase.ConnectionString))
            {
                Histories = (await db.TestHistories.Include(x => x.Results).Include(x => x.Step).ToListAsync()).GroupBy(x => x.TimeStamp)
                    .ToDictionary(x => DateTimeOffset.FromUnixTimeMilliseconds(x.Key).LocalDateTime, x => x.ToList());
            }
        }

        public Dictionary<DateTime, List<TestHistory>> Histories { get; set; }
    }
}
