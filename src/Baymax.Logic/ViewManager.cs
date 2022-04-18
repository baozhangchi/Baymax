using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Stylet;

namespace Baymax.Logic
{
    public class ViewManager : Stylet.ViewManager
    {
        private readonly Dictionary<Type, Type> _modelToViewMap;

        public ViewManager(ViewManagerConfig config) : base(config)
        {
            var viewTypes = Assembly.Load("Baymax").GetTypes().Where(x => x.Namespace?.StartsWith("Baymax.Views") ?? false)
                 .ToList();

            var modelTypes = this.GetType().Assembly.GetTypes().Where(x => x.Namespace?.StartsWith("Baymax.Logic.ViewModels") ?? false)
                 .ToList();

            _modelToViewMap = (from v in viewTypes
                               join m in modelTypes on $"{v.Name}Model" equals m.Name
                               select new { v, m }).ToDictionary(x => x.m, x => x.v);
        }

        protected override Type LocateViewForModel(Type modelType)
        {
            if (_modelToViewMap.ContainsKey(modelType))
            {
                return _modelToViewMap[modelType];
            }

            return base.LocateViewForModel(modelType);
        }
    }
}
