﻿using System;
using System.Collections.Generic;
using GatheringTimer.Data.Database;
using GatheringTimer.Data.Model;
using GatheringTimer.Data.Update;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Reflection;
using GatheringTimer.Data.Model.Entity;

namespace GatheringTimer.Data
{
    interface IDataManagement
    {
        bool DataInitialization();

        bool DataUpdate();

        bool CreateFavouriteItem(long itemId);

        List<long> ReadFavouriteItem();

        bool DeleteFavouriteItem(long itemId);

        bool CreateFavouritePlace(long placeId);
         
        List<long> ReadFavouritePlace();

        bool DeleteFavouritePlace(long placeId);

    }

    public static class DataManagement
    {
        public static async Task<bool> Sync()
        {
            return await Updater.SyncRaw();
        }

        public static async Task<List<Item>> Search(String searchStr) {
            return await Service.Search(searchStr);
        
        }

        public static async Task<List<Model.Vo.DisplayVo.Item>> SearchDetail(String searchStr)
        {
            try
            {
                int.Parse(searchStr);
            }
            catch (Exception ex)
            {
                return null;
            }
            return await Service.SearchDetail(int.Parse(searchStr));

        }
    }
}