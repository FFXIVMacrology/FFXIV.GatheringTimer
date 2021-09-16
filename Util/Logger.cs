using log4net.Appender;
using log4net.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GatheringTimer
{
    public class Logger
    {
        public static TextBox textBoxLogger;

        private static System.Threading.Timer timer;

        public static void InitTimer()
        {
            timer = new System.Threading.Timer(
                    new System.Threading.TimerCallback(ReadLogFile),
                    null,
                    0,
                    1000
                    );
        }

        private static void ReadLogFile(object param) {
            string oldValue = string.Empty, newValue = string.Empty;
            string path = "./Plugins/FFXIV.GatheringTimer/Log/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            //if (File.Exists(path)) {
            //    using (StreamReader read = new StreamReader(path, true))
            //    {
            //        do
            //        {
            //            newValue = read.ReadLine();
            //            oldValue = newValue != null ? newValue : oldValue;
            //        } while (newValue != null);
            //    }
            //    if (textBoxLogger != null)
            //    {
            //        textBoxLogger.AppendText(oldValue);
            //        textBoxLogger.AppendText(Environment.NewLine);
            //        textBoxLogger.Refresh();
            //    }

            //}

        }


        public static void Error(object msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logerror");
            Task.Run(() => {
                log.Error(msg);
            }); 
        }

        public static void Error(object msg, Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logerror");
            if (ex != null)
            {
                Task.Run(() => {
                    log.Error(msg, ex);
                });
            }
            else
            {
                Task.Run(() => {
                    log.Error(msg);
                });
            }
        }

        public static void Warn(object msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logwarn");
            Task.Run(() => {
                log.Warn(msg);
            });
        }

        public static void Warn(object msg, Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logwarn");
            if (ex != null)
            {
                Task.Run(() => {
                    log.Warn(msg, ex);
                });
            }
            else
            {
                Task.Run(() => {
                    log.Warn(msg);
                });
            }
        }

        public static void Info(object msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("loginfo");
            Task.Run(() => {
                log.Info(msg);
            });
        }

        public static void Debug(object msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logdebug");
            Task.Run(() => {
                log.Debug(msg);
            });
        }

        public static void Debug(Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logdebug");
            Task.Run(() => {
                log.Debug(ex.Message.ToString() + "/r/n" + ex.Source.ToString() + "/r/n" + ex.TargetSite.ToString() + "/r/n" + ex.StackTrace.ToString());
            });
        }

        public static void Debug(object msg, Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logdebug");
            if (ex != null)
            {
                Task.Run(() => {
                    log.Debug(msg, ex);
                });
            }
            else
            {
                Task.Run(() => {
                    log.Debug(msg);
                });
            }
        }
    }

}