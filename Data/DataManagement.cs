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

namespace GatheringTimer.Data
{

    public static class DataManagement
    {
        public static async Task<bool> Sync()
        {
            return await ThirdParty.Updater.SyncRaw();
        }
    }

}