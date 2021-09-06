using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace GatheringTimer
{
    public class Logger
    {
        private static LogType LOG_LEVEL = LogType.Info;

        private static TextBox TEXT_BOX;

        public static void SetTextBox(TextBox textBox)
        {
            TEXT_BOX = textBox;
        }

        public enum LogType
        {
            Error, Warning, Info, Debug

        }

        private static string InitLogStr(LogType logType, string str, Exception ex)
        {
            StackTrace st = new StackTrace();
            string namespaceName = st.GetFrame(2).GetMethod().ReflectedType.Namespace;
            string className = st.GetFrame(2).GetMethod().ReflectedType.Name;
            string methodName = st.GetFrame(2).GetMethod().Name;
            switch (logType)
            {
                case LogType.Debug:
                    return DateTime.Now.ToString() + "-[Debug][" + namespaceName + "][" + className + "][" + methodName + "]:" + str + (ex is null ?"" : "\r\nException:" + ex.Message);
                case LogType.Info:
                    return DateTime.Now.ToString() + "-[Info][" + namespaceName + "][" + className + "][" + methodName + "]:" + str + (ex is null ? "" : "\r\nException:" + ex.Message);
                case LogType.Warning:
                    return DateTime.Now.ToString() + "-[Warning][" + namespaceName + "][" + className + "][" + methodName + "]:" + str + (ex is null ? "" : "\r\nException:" + ex.Message);
                case LogType.Error:
                    return DateTime.Now.ToString() + "-[Error][" + namespaceName + "][" + className + "][" + methodName + "]:" + str + (ex is null ? "" : "\r\nException:" + ex.Message);
                default: return "";
            }

        }

        public static void Error(string str, Exception exception)
        {
            string log = InitLogStr(LogType.Error, str, exception);
            Console.WriteLine(log);
            TextBoxShowLog(log);
        }

        public static void Warning(string str, Exception exception)
        {
            string log = InitLogStr(LogType.Warning, str, exception);
            if (LOG_LEVEL != LogType.Error)
            {
                Console.WriteLine(log);
                TextBoxShowLog(log);
            }
        }

        public static void Info(string str, Exception exception)
        {
            string log = InitLogStr(LogType.Info, str, exception);
            if (LOG_LEVEL != LogType.Error && LOG_LEVEL != LogType.Warning)
            {
                Console.WriteLine(log);
                TextBoxShowLog(log);
            }
        }

        public static void Debug(string str, Exception exception)
        {
            string log = InitLogStr(LogType.Debug, str, exception);
            if (LOG_LEVEL != LogType.Error && LOG_LEVEL != LogType.Warning && LOG_LEVEL != LogType.Info)
            {
                Console.WriteLine(log);
                TextBoxShowLog(log);
            }
        }

        public static void Error(string str)
        {
            Error(str, null);
        }

        public static void Info(string str)
        {
            Info(str, null);
        }

        public static void Warning(string str)
        {
            Warning(str, null);
        }

        public static void Debug(string str)
        {
            Debug(str, null);
        }

        public static void TextBoxShowLog(string Info)
        {
            if (null != TEXT_BOX)
            {

                TEXT_BOX.AppendText(Info);
                TEXT_BOX.AppendText(Environment.NewLine);
                TEXT_BOX.ScrollToCaret();

            }

        }

    }
}
