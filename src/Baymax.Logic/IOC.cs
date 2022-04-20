using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baymax.Logic
{
    public static class IOC
    {
        public static Func<Type, string, object> GetInstance { get; set; } =
            (type, key) => throw new NotImplementedException();

        public static Func<Type, string, IEnumerable<object>> GetAllInstances { get; set; } = (type, key) => throw new NotImplementedException();

        public static T Get<T>(string key = null)
        {
            return (T)GetInstance(typeof(T), key);
        }

        public static IEnumerable<T> GetAll<T>(string key = null)
        {
            return GetAllInstances(typeof(T), key).Cast<T>();
        }
    }
}
