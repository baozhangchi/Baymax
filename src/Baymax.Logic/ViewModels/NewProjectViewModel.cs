using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Baymax.Models;
using MaterialDesignThemes.Wpf;
using Stylet;
using StyletIoC;

namespace Baymax.Logic.ViewModels
{
    public class NewProjectViewModel : ViewModelBase
    {
        private readonly IContainer _container;

        public NewProjectViewModel(IContainer container)
        {
            _container = container;
            DisplayName = "新建项目";
        }

        public string ProjectName { get; set; }

        public string ProjectFolder { get; set; }

        public bool CanConfirm => !string.IsNullOrWhiteSpace(ProjectName) && !string.IsNullOrWhiteSpace(ProjectFolder);

        public void Confirm()
        {
            if (!Directory.Exists(ProjectFolder))
            {
                Directory.CreateDirectory(ProjectFolder);
            }

            if (Directory.Exists(Path.Combine(ProjectFolder, ProjectName)))
            {
                _container.Get<IWindowManager>().ShowMessageBox($"项目目录{Path.Combine(ProjectFolder, ProjectName)}已存在");
                return;
            }

            Directory.CreateDirectory(Path.Combine(ProjectFolder, ProjectName));

            var projectFile = Path.Combine(ProjectFolder, ProjectName, $"{ProjectName}.bmpro");
            File.WriteAllText(projectFile, new BaymaxProjectModel().ToXml());
            DialogHost.Close(null, projectFile);
        }

        public void Cancel()
        {
            DialogHost.Close(null);
        }

        public void SelectProjectFolder()
        {
            var dialog = new FolderBrowserDialog() { ShowNewFolderButton = true };
            if (!string.IsNullOrWhiteSpace(ProjectFolder) && Directory.Exists(ProjectFolder))
            {
                dialog.SelectedPath = ProjectFolder;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ProjectFolder = dialog.SelectedPath;
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(ProjectFolder):
                case nameof(ProjectName):
                    NotifyOfPropertyChange(nameof(CanConfirm));
                    break;
            }
        }
    }
}
