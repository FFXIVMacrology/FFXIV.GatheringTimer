using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GatheringTimer.Data.ThirdParty
{
    /// <summary>
    /// 第三方数据更新器基类
    /// </summary>
    public abstract class BaseUpdater
    {
        public abstract Task DataUpdate(CancellationToken cancellationToken);

        public abstract void ClearCache();

        public abstract Task Sync(CancellationToken cancellationToken);

        private T ConvertTo<T, Q>(Q q)
        {
            T t = Activator.CreateInstance<T>();
            Type tType = typeof(T);
            PropertyInfo[] tPropertyInfos = tType.GetProperties();

            Type qType = typeof(Q);
            PropertyInfo[] qPropertyInfos = qType.GetProperties();
            foreach (PropertyInfo qPropertyInfo in qPropertyInfos)
            {
                foreach (PropertyInfo tPropertyInfo in tPropertyInfos)
                {
                    if (tPropertyInfo.Name.Equals(qPropertyInfo.Name) && (tPropertyInfo.GetType() == qPropertyInfo.GetType()))
                    {
                        tType.GetProperty(tPropertyInfo.Name).SetValue(t, qType.GetProperty(qPropertyInfo.Name).GetValue(q));
                    }
                }

            }
            return t;

        }

        private List<T> ConvertTo<T, Q>(List<Q> qList)
        {
            List<T> tList = new List<T>();
            foreach (Q q in qList)
            {
                T t = ConvertTo<T, Q>(q);
                tList.Add(t);
            }
            return tList;
        }

        public void IntoCache<T>(Object updater, List<T> tlist)
        {

            updater.GetType().GetProperty(typeof(T).Name + "Cache").SetValue(updater, tlist);

        }

        public void IntoStaticCache<T>(Type updaterType, List<T> tlist)
        {
            updaterType.GetProperty(typeof(T).Name + "Cache").SetValue(updaterType, tlist);
        }

        /// <summary>
        /// List<Object>的属性名必须为"Object类名+Cache"
        /// </summary>
        public async Task Sync<T, Q>(Object updater, CancellationToken cancellationToken)
        {
            List<T> entities = default;
            await Task.Run(()=> {
                List<Q> cacheEntities = (List<Q>)updater.GetType().GetProperty(typeof(T).Name + "Cache").GetValue(updater);
                entities = ConvertTo<T, Q>(cacheEntities);
                IntoStaticCache(typeof(GatheringTimerResource), entities);
            },cancellationToken);
            await GatheringTimerResource.GetSQLiteDatabase().InsertRowByRow<T>(entities, cancellationToken);
        }

    }

}
