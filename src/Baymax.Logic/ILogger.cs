using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using ILogger = Stylet.Logging.ILogger;
using LogManager = NLog.LogManager;

namespace Baymax.Logic
{
    public class NLogger : ILogger
    {
        private readonly Logger _logger;
        private static NLogger _instance;
        private static readonly object Lock = new object();

        private NLogger(string name)
        {
            _logger = LogManager.GetLogger(name);
        }

        public static NLogger GetLogger(string name)
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new NLogger(name);
                    }
                }
            }

            return _instance;
        }

        public void Info(string format, params object[] args)
        {
            _logger?.Info(string.Format(format, args));
        }

        public void Warn(string format, params object[] args)
        {
            _logger?.Warn(string.Format(format, args));
        }

        public void Error(Exception exception, string message = null)
        {
            _logger?.Error(exception, message);
        }
    }
}
