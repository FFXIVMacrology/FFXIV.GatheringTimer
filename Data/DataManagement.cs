using System;
using System.Collections.Generic;
using GatheringTimer.Data.Database;
using GatheringTimer.Data.Model;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Reflection;
using GatheringTimer.Data.Model.Entity;
using System.Threading;

namespace GatheringTimer.Data
{

    public static class DataManagement
    {
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public static async Task<bool> Sync()
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource = new CancellationTokenSource();
                GC.Collect();
            }
            return await ThirdParty.Updater.SyncRaw(cancellationTokenSource.Token);
        }

        public static void SyncCancel()
        {
            cancellationTokenSource.Cancel();
        }
    }

}