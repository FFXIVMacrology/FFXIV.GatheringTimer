using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GatheringTimer.Data.Model;
using System.Net;


namespace GatheringTimer.Util
{
    public static class RequestUtil
    {

        /// <summary>
        /// Use RestClient for new response data with async
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static async Task<String> GetResponseDataAsync(String url, String param)
        {
            String html = "";
            var restClient = new RestClient(url);
            try
            {
                html = await restClient.MakeRequestAsync(param);
            }
            catch (Exception e)
            {
                Logger.Warn("No response", e);
            }
            return html;
        }

        /// <summary>
        /// Parse json of List to T of List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonList"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<List<T>> ParseResultList<T>(List<String> jsonList, String key)
        {
            List<T> list = new List<T>();
            Queue<Task> tasks = new Queue<Task>();
            foreach (String json in jsonList)
            {
                Task task = JsonToObjectAsync<List<T>>(json, key);
                tasks.Enqueue(task);
            }
            await Task.WhenAll(tasks);
            foreach (Task<Object> task in tasks) {
                List<T> listPart = (List<T>)task.Result;
                foreach (T t in listPart)
                {
                    list.Add(t);
                }
            }
            return list;
        }

        /// <summary>
        /// Parase Json Value With Key to Object T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<Object> JsonToObjectAsync<T>(String json, String key)
        {
            try
            {

                JObject jsonObject = JObject.Parse(json);
                if (!jsonObject.ContainsKey(key))
                {
                    return null;
                }
                String value = jsonObject.GetValue(key).ToString();
                if (IsJsonArray(value) || IsJsonObject(value))
                {
                    return await Task.Run(() => (T)JsonConvert.DeserializeObject<T>(value));
                }
                return value;

            }
            catch(Exception ex)
            {
                Logger.Error("Json To Object Error,Exception:"+ex.Message);
                return null;
            }


        }

        /// <summary>
        /// JsonArray check
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static Boolean IsJsonArray(String str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return false;
            }
            try
            {
                JArray jsonArray = JArray.Parse(str);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// JsonObject check
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static Boolean IsJsonObject(String str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return false;
            }
            try
            {
                JObject jsonObject = JObject.Parse(str);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
