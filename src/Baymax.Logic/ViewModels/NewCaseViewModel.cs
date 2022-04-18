using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Baymax.Models;
using MaterialDesignThemes.Wpf;
using Stylet;
using StyletIoC;

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

        public string CaseName { get; set; }

        public string ProjectFolder { get; set; }

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

            Project.TestCases.Add(CaseName);

            var caseFile = Path.Combine(caseFolder, $"{CaseName}.bmc");
            using (var db = new Baymax.Dal.BaymaxDbContext($"data source={caseFile}"))
            {
                db.Database.EnsureCreated();
            }

            DialogHost.Close(null, caseFile);
        }

        public void Cancel()
        {
            DialogHost.Close(null);
        }
    }
}
