using log4net.Appender;
using log4net.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GatheringTimer
{
    public class TextBoxAppender : AppenderSkeleton
    {
        public static TextBox _textBox;
        public string FormName { get; set; }
        public string TextBoxName { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (_textBox != null)
            {
                _textBox.AppendText(loggingEvent.RenderedMessage + Environment.NewLine);
            }
            else
            {

                if (String.IsNullOrEmpty(FormName) ||
                    String.IsNullOrEmpty(TextBoxName))
                    return;

                Form form = Application.OpenForms[FormName];
                if (form == null)
                    return;

                _textBox = form.Controls[TextBoxName] as TextBox;
                if (_textBox == null)
                    return;

                form.FormClosing += (s, e) => _textBox = null;
            }


        }
    }

    public class Logger
    {
        public static TextBox textBoxLogger;

        public static void Error(object msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logerror");
            Task.Run(() =>
            {
                log.Error(msg);
            });
        }

        public static void Error(object msg, Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logerror");
            if (ex != null)
            {
                Task.Run(() =>
                {
                    log.Error(msg, ex);
                });
            }
            else
            {
                Task.Run(() =>
                {
                    log.Error(msg);
                });
            }
        }

        public static void Warn(object msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logwarn");
            Task.Run(() =>
            {
                log.Warn(msg);
            });
        }

        public static void Warn(object msg, Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logwarn");
            if (ex != null)
            {
                Task.Run(() =>
                {
                    log.Warn(msg, ex);
                });
            }
            else
            {
                Task.Run(() =>
                {
                    log.Warn(msg);
                });
            }
        }

        public static void Info(object msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("loginfo");
            Task.Run(() =>
            {
                log.Info(msg);
            });
        }

        public static void Debug(object msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logdebug");
            Task.Run(() =>
            {
                log.Debug(msg);
            });
        }

        public static void Debug(Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logdebug");
            Task.Run(() =>
            {
                log.Debug(ex.Message.ToString() + "/r/n" + ex.Source.ToString() + "/r/n" + ex.TargetSite.ToString() + "/r/n" + ex.StackTrace.ToString());
            });
        }

        public static void Debug(object msg, Exception ex)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("logdebug");
            if (ex != null)
            {
                Task.Run(() =>
                {
                    log.Debug(msg, ex);
                });
            }
            else
            {
                Task.Run(() =>
                {
                    log.Debug(msg);
                });
            }
        }
    }

}