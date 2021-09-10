using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GatheringTimer.Util
{
    public class Notify
    {
        public static void Info(string str)
        {
            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon("./Resources/Timer.ico");
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(1000, "GatheringTimer", str, ToolTipIcon.Info);
        }

    }
}
