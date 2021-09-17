using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Data.Common;

using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using static GatheringTimer.Logger;

namespace GatheringTimer.Data.Database
{
    public class SQLiteDatabase
    {

        private String dataSource;

        private volatile Dictionary<string, ReaderWriterLock> tableLock = new Dictionary<string, ReaderWriterLock>();

        private ReaderWriterLock GetReaderWriterLock<T>()
        {
            Type type = typeof(T);
            string name = type.Name;
            if (tableLock.ContainsKey(name))
            {
                return tableLock[name];
            }
            else
            {
                ReaderWriterLock readerWriterLock = new ReaderWriterLock();
                tableLock.Add(name, readerWriterLock);
                return readerWriterLock;
            }
        }

        private bool IsContains<T>(List<T> listA, List<T> listB)
        {
            bool isContains = true;
            foreach (T tB in listB)
            {
                bool isContain = false;
                foreach (T tA in listA)
                {
                    if (tB.Equals(tA)) { isContain = true; }
                }
                isContains = isContains && isContain;
            }
            return isContains;
        }

        /// <summary>
        /// Enum for parse T List to SQLite Database type
        /// </summary>
        private enum ParseListKey
        {
            TableName, Column, Value, ValueCount
        }

        /// <summary>
        /// Parse T List to SQLite Database type without null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns>(String)TableName,(List<List<String>>)Column,(List<List<String>>)Value,(int)ValueCount</returns>
        private static Dictionary<ParseListKey, Object> ParseListForSQLWithoutNull<T>(List<T> list)
        {
            Dictionary<ParseListKey, Object> dic = new Dictionary<ParseListKey, Object>();
            Type type = typeof(T);
            //Get TableName
            {
                String tableName = type.Name;
                dic.Add(ParseListKey.TableName, tableName);
            }
            //Get Columns, Values and ValueCount
            List<List<String>> columns = new List<List<String>>();
            List<List<String>> values = new List<List<String>>();
            PropertyInfo[] propertyInfos = type.GetProperties();
            int valuesCount = 0;
            foreach (T t in list)
            {
                List<String> column = new List<String>();
                List<String> value = new List<String>();
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    object columnValue = propertyInfo.GetValue(t);
                    if (!(columnValue is null))
                    {
                        String columnValueString = columnValue.ToString();
                        if (columnValue is String || columnValue is string)
                        {
                            if (columnValueString.IndexOf("'") != -1)
                            {
                                columnValueString = columnValueString.Replace("'", "''");
                            }
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("'");
                            stringBuilder.Append(columnValueString);
                            stringBuilder.Append("'");
                            columnValueString = stringBuilder.ToString();

                        }
                        column.Add(propertyInfo.Name);
                        value.Add(columnValueString);
                    }
                    else
                    {
                        continue;
                    }
                }
                columns.Add(column);
                values.Add(value);
                valuesCount++;
            }
            dic.Add(ParseListKey.Column, columns);
            dic.Add(ParseListKey.Value, values);
            dic.Add(ParseListKey.ValueCount, valuesCount);
            return dic;
        }

        /// <summary>
        /// Parse T List to SQLite Database type with null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns>(String)TableName,(List<String>)Column,(List<List<String>>)Value,(int)ValueCount</returns>
        private static Dictionary<ParseListKey, Object> ParseListForSQLWithNull<T>(List<T> list)
        {
            Dictionary<ParseListKey, Object> dic = new Dictionary<ParseListKey, Object>();
            Type type = typeof(T);
            //Get TableName
            {
                String tableName = type.Name;
                dic.Add(ParseListKey.TableName, tableName);
            }
            //Get Column
            List<String> propertyNameList = GetPropertyNameList<T>();
            //Get Values and ValueCount
            List<List<String>> values = new List<List<String>>();
            PropertyInfo[] propertyInfos = type.GetProperties();
            int valuesCount = 0;
            foreach (T t in list)
            {
                List<String> value = new List<String>();
                int propertyCount = 0;
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    object columnValue = propertyInfo.GetValue(t);
                    if (!(columnValue is null))
                    {
                        String columnValueString = columnValue.ToString();
                        if (columnValue is String || columnValue is string)
                        {
                            if (columnValueString.IndexOf("'") != -1)
                            {
                                columnValueString = columnValueString.Replace("'", "''");
                            }
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("'");
                            stringBuilder.Append(columnValueString);
                            stringBuilder.Append("'");
                            columnValueString = stringBuilder.ToString();

                        }
                        value.Add(columnValueString);
                    }
                    else
                    {
                        value.Add("NULL");
                        continue;
                    }
                    propertyCount++;
                }
                values.Add(value);
                valuesCount++;
            }
            dic.Add(ParseListKey.Column, propertyNameList);
            dic.Add(ParseListKey.Value, values);
            dic.Add(ParseListKey.ValueCount, valuesCount);
            return dic;
        }

        /// <summary>
        /// Get T Property name List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static List<String> GetPropertyNameList<T>()
        {
            Type type = typeof(T);
            //Get Properties in model
            var modelPropertyInfos = type.GetProperties();
            //Foreach every property in model and assign homologous row
            List<String> propertyNameList = new List<String>();
            foreach (var propertyInfo in modelPropertyInfos)
            {
                //Get property name
                String name = propertyInfo.Name;
                propertyNameList.Add(name);
            }
            return propertyNameList;
        }

        /// <summary>
        /// Get T Property name List and type List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static Dictionary<String, Type> GetPropertyList<T>()
        {
            Type type = typeof(T);
            //Get Properties in model
            var propertyInfos = type.GetProperties();
            //Foreach every property in model and assign homologous row
            Dictionary<String, Type> dic = new Dictionary<String, Type>();
            foreach (var propertyInfo in propertyInfos)
            {
                //Get property name
                String propertyName = propertyInfo.Name;
                //Get property type
                Type propertyType = propertyInfo.PropertyType;
                dic.Add(propertyName, propertyType);
            }
            return dic;
        }

        public void SetDataSource(String dataSource)
        {
            this.dataSource = dataSource;
        }

        public bool CheckDataSource()
        {
            return null != this.dataSource;
        }

        /// <summary>
        /// Create database with dataSource
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public bool CreateDatabase()
        {
            try
            {
                if (!Directory.Exists(this.dataSource.Substring(0, this.dataSource.LastIndexOf("/"))))
                {
                    Directory.CreateDirectory(this.dataSource.Substring(0, this.dataSource.LastIndexOf("/")));
                }

                if (!File.Exists(this.dataSource))
                {
                    SQLiteConnection.CreateFile(dataSource);
                    return true;
                }
                else
                {
                    throw new Exception("Create Database [" + dataSource + "] fail:" + "File is already Exists");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Create Database [" + dataSource + "] fail:" + e.Message);
            }
        }

        /// <summary>
        /// Delete database with dataSource
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public bool DeleteDatabase()
        {
            try
            {
                if (File.Exists(this.dataSource))
                {
                    File.Delete(this.dataSource);
                }
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("Delete Database [" + dataSource + "] fail:" + e.Message);
            }

        }

        /// <summary>
        /// Execute SQL List
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="sqlList"></param>
        /// <returns></returns>
        public bool ExecuteSQL(List<String> sqlList)
        {
            using (SQLiteConnection connection = new SQLiteConnection("data source=" + this.dataSource))
            {
                if (ConnectionState.Open != connection.State) connection.Open();
                using (SQLiteCommand sqliteCommand = new SQLiteCommand())
                {
                    sqliteCommand.Connection = connection;
                    try
                    {
                        SQLiteTransaction sqliteTransaction = connection.BeginTransaction();
                        try
                        {
                            foreach (String sql in sqlList)
                            {
                                Logger.Debug("\n" + sql);
                                sqliteCommand.CommandText = sql;
                                sqliteCommand.ExecuteNonQuery();
                            }
                            sqliteTransaction.Commit();
                            return true;
                        }
                        catch (Exception e)
                        {
                            Logger.Error("ExecuteSQL Error", e);
                            sqliteTransaction.Rollback();
                            return false;
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Transaction Error", ex);
                        return false;
                    }

                }
            }
        }

        /// <summary>
        /// Create table in database with T and dataSource
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public async Task<bool> CreateTable<T>(CancellationToken cancellationToken)
        {
            Type type = typeof(T);
            //Get TableName
            String tableName = type.Name;
            //Get Column
            Dictionary<String, Type> dic = GetPropertyList<T>();
            String columns = "";
            foreach (String key in dic.Keys)
            {
                if (columns.Length != 0) columns += ",";
                columns += key;
                if (dic[key] == typeof(int)) { columns += " INTEGER"; }
                if (dic[key] == typeof(String) || dic[key] == typeof(string)) { columns += " VARCHAR"; }
                if (key.ToLowerInvariant().Equals("id")) columns += " PRIMARY KEY";

            }
            if (!(columns.Length > 0))
            {
                throw new Exception("Table " + tableName + " column is empty!");
            }
            List<String> sqlList = new List<String>();
            String sql = "CREATE TABLE IF NOT EXISTS " + tableName + "(" + columns + ")";
            sqlList.Add(sql);
            return await Task.Run(() => ExecuteSQL(sqlList), cancellationToken);
        }

        /// <summary>
        /// Dreate table in database with T and dataSource
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public async Task<bool> DeleteTable<T>(CancellationToken cancellationToken)
        {
            Type type = typeof(T);
            //Get TableName
            String tableName = type.Name;
            List<String> sqlList = new List<String>();
            String sql = "DROP TABLE IF EXISTS " + tableName;
            sqlList.Add(sql);
            return await Task.Run(() => ExecuteSQL(sqlList),cancellationToken);
        }

        /// <summary>
        /// AddColumn with T,dataSource and column(column example:"name varcahr")
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public async Task<bool> AddColumn<T>(String column, CancellationToken cancellationToken)
        {
            Type type = typeof(T);
            String tableName = type.Name;
            List<String> sqlList = new List<String>();
            String sql = "ALTER TABLE " + tableName + " ADD COLUMN " + column;
            sqlList.Add(sql);
            return await Task.Run(() => ExecuteSQL(sqlList),cancellationToken);
        }

        /// <summary>
        /// Insert row by row with T list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<bool> InsertRowByRow<T>(List<T> list, CancellationToken cancellationToken)
        {
            Dictionary<ParseListKey, Object> parseListForSQLWithoutNull = ParseListForSQLWithoutNull(list);
            String tableName = (String)parseListForSQLWithoutNull[ParseListKey.TableName];
            List<List<String>> columnLists = (List<List<String>>)parseListForSQLWithoutNull[ParseListKey.Column];
            List<List<String>> valueLists = (List<List<String>>)parseListForSQLWithoutNull[ParseListKey.Value];
            int valueCount = (int)parseListForSQLWithoutNull[ParseListKey.ValueCount];

            List<String> sqlList = new List<String>();
            for (int i = 0; i < valueCount; i++)
            {
                List<String> columnList = columnLists[i];
                List<String> valueList = valueLists[i];
                StringBuilder columnString = new StringBuilder();
                StringBuilder valueString = new StringBuilder();
                int count = 0;
                foreach (String value in valueList)
                {
                    if (columnString.Length != 0) columnString.Append(",");
                    if (valueString.Length != 0) valueString.Append(",");
                    columnString.Append(columnList[count]);
                    valueString.Append(valueList[count]);
                    count++;
                }
                String sql = "INSERT INTO " + tableName + "(" + columnString.ToString() + ") VALUES(" + valueString.ToString() + ");";
                sqlList.Add(sql);
            }
            return await Task.Run(() =>
            {
                ReaderWriterLock writerLock = GetReaderWriterLock<T>();
                try
                {
                    writerLock.AcquireWriterLock(-1);
                    return ExecuteSQL(sqlList);
                }
                finally
                {
                    writerLock.ReleaseWriterLock();
                }
            },cancellationToken);

        }

        /// <summary>
        /// Insert row by union all with T list and union total limit
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <param name="list"></param>
        /// <param name="unionLimit"></param>
        /// <returns></returns>
        public async Task<bool> InsertRowUnion<T>(List<T> list, int unionLimit, CancellationToken cancellationToken)
        {
            Dictionary<ParseListKey, Object> parseListForSQLWithNull = ParseListForSQLWithNull(list);
            String tableName = (String)parseListForSQLWithNull[ParseListKey.TableName];
            List<String> columnList = (List<String>)parseListForSQLWithNull[ParseListKey.Column];
            List<List<String>> valueLists = (List<List<String>>)parseListForSQLWithNull[ParseListKey.Value];

            String columnString = "";
            foreach (String column in columnList)
            {
                if (!String.IsNullOrEmpty(columnString)) columnString += ",";
                columnString = String.Format(columnString + "{0}", column);
            }

            List<String> sqlList = new List<String>();

            int times = 0;
            StringBuilder sql = default;
            foreach (List<String> valueList in valueLists)
            {
                StringBuilder valueString = new StringBuilder();
                foreach (String value in valueList)
                {
                    if (valueString.Length != 0) valueString.Append(",");
                    valueString.Append(value);
                }
                if (times == 0)
                {
                    sql = new StringBuilder();
                    sql.Append("INSERT INTO " + tableName + "(" + columnString + ")");
                }
                else
                {
                    sql.Append(" UNION ALL");
                }
                sql.Append(" SELECT " + valueString);
                times++;
                if (times >= unionLimit)
                {
                    times = 0;
                    sql.Append(";");
                    sqlList.Add("" + sql);
                }
            }
            if (times >= unionLimit)
            {
                sql.Append(";");
                sqlList.Add("" + sql);
            }
            return await Task.Run(() =>
            {
                ReaderWriterLock writerLock = GetReaderWriterLock<T>();
                try
                {
                    writerLock.AcquireWriterLock(-1);
                    return ExecuteSQL(sqlList);
                }
                finally
                {
                    writerLock.ReleaseWriterLock();
                }
            },cancellationToken);
        }

        /// <summary>
        /// #TODO method refactor,
        /// Delete rows with dataSource and conditionT(condition example:"ID=2 AND NAME='nameTest'")
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <param name="conditionT"></param>
        /// <returns></returns>
        public async Task<bool> Delete<T>(String condition, CancellationToken cancellationToken)
        {
            Type type = typeof(T);
            String tableName = type.Name;
            List<String> sqlList = new List<String>();
            String sql = "DELETE FROM " + tableName + " WHERE " + condition;
            sqlList.Add(sql);
            return await Task.Run(() =>
            {
                ReaderWriterLock writerLock = GetReaderWriterLock<T>();
                try
                {
                    writerLock.AcquireWriterLock(-1);
                    return ExecuteSQL(sqlList);
                }
                finally
                {
                    writerLock.ReleaseWriterLock();
                }
            },cancellationToken);

        }

        /// <summary>
        /// #TODO method refactor,
        /// Update one row with T,setValues and condition(setValues example:"ID=1,NAME='name'";condition example:"ID=2 AND NAME='nameTest'")
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <param name="setValues"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public async Task<bool> Update<T>(String setValues, String condition, CancellationToken cancellationToken)
        {
            Type type = typeof(T);
            String tableName = type.Name;
            List<String> sqlList = new List<String>();
            String sql = "UPDATE " + tableName + " SET " + setValues + " WHERE " + condition;
            sqlList.Add(sql);
            return await Task.Run(() =>
            {
                ReaderWriterLock writerLock = GetReaderWriterLock<T>();
                try
                {
                    writerLock.AcquireWriterLock(-1);
                    return ExecuteSQL(sqlList);
                }
                finally
                {
                    writerLock.ReleaseWriterLock();
                }
            },cancellationToken);
        }

        public async Task<bool> UpdateRowByRow<T>(List<T> list, List<String> setColumnList, List<String> conditionColumnList, CancellationToken cancellationToken)
        {
            Dictionary<ParseListKey, Object> parseListForSQLWithoutNull = ParseListForSQLWithoutNull(list);
            String tableName = (String)parseListForSQLWithoutNull[ParseListKey.TableName];
            List<List<String>> columnLists = (List<List<String>>)parseListForSQLWithoutNull[ParseListKey.Column];
            List<List<String>> valueLists = (List<List<String>>)parseListForSQLWithoutNull[ParseListKey.Value];
            int valueCount = (int)parseListForSQLWithoutNull[ParseListKey.ValueCount];

            List<String> sqlList = new List<String>();
            for (int i = 0; i < valueCount; i++)
            {
                List<String> columnList = columnLists[i];
                List<String> valueList = valueLists[i];
                StringBuilder setString = new StringBuilder();
                StringBuilder conditionString = new StringBuilder();
                if (!IsContains(columnList, setColumnList) || !IsContains(columnList, conditionColumnList))
                {
                    Logger.Error("setColumnList or conditionColumnList is not contained with list column", null);
                    return false;
                }
                int count = 0;
                foreach (String value in valueList)
                {
                    String columnString = columnList[count];
                    foreach (String setColumnString in setColumnList)
                    {
                        if (value is null || value.Equals("''")) continue;
                        if (columnString.Equals(setColumnString))
                        {
                            if (setString.Length != 0) setString.Append(",");
                            setString.Append(columnString + "=" + value);
                        }
                    }
                    foreach (String conditionColumnString in conditionColumnList)
                    {
                        if (value is null || value.Equals("''")) continue;
                        if (columnString.Equals(conditionColumnString))
                        {
                            if (conditionString.Length != 0) conditionString.Append(",");
                            conditionString.Append(columnString + "=" + value);
                        }
                    }
                    count++;
                }
                String sql = "UPDATE " + tableName + " SET " + setString.ToString() + " WHERE " + conditionString.ToString() + ";";
                if (setString.Length != 0 && conditionString.Length != 0) sqlList.Add(sql);
            }
            return await Task.Run(() =>
            {
                ReaderWriterLock writerLock = GetReaderWriterLock<T>();
                try
                {
                    writerLock.AcquireWriterLock(-1);
                    return ExecuteSQL(sqlList);
                }
                finally
                {
                    writerLock.ReleaseWriterLock();
                }
            },cancellationToken);
        }

        /// <summary>
        /// Select with sql and return DataTable
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable Select(string sql)
        {
            using (SQLiteConnection connection = new SQLiteConnection("data source=" + this.dataSource))
            {
                if (ConnectionState.Open != connection.State) connection.Open();
                using (SQLiteCommand sqliteCommand = new SQLiteCommand())
                {
                    try
                    {
                        Logger.Debug("\n"+sql);
                        sqliteCommand.Connection = connection;
                        sqliteCommand.CommandText = sql;
                        sqliteCommand.CommandTimeout = 120;
                        sqliteCommand.ExecuteNonQuery();
                        SQLiteDataReader reader = sqliteCommand.ExecuteReader();
                        DataTable dataTable = new DataTable();
                        if (reader != null)
                        {
                            dataTable.Load(reader, LoadOption.PreserveChanges, null);
                        }
                        return dataTable;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Select SQL Error,SQL Query:" + sql, ex);
                        return null;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public async Task<List<T>> Select<T>(List<string> conditionColumnList, List<string> conditionList, T t, List<string> conditionKeywordList, int? limit, CancellationToken cancellationToken)
        {

            List<string> columns = new List<string>();
            List<string> values = new List<string>();
            PropertyInfo[] propertyInfos = t.GetType().GetProperties();
            List<string> propertyInfoList = new List<string>();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                propertyInfoList.Add(propertyInfo.Name);
                object valueObj = t.GetType().GetProperty(propertyInfo.Name).GetValue(t);
                if (!(valueObj is null))
                {
                    string name = propertyInfo.Name;
                    Type columnType = valueObj.GetType();
                    if (columnType.IsValueType || columnType == typeof(string))
                    {
                        if (!(propertyInfo.GetType() == typeof(int) && int.Parse(t.GetType().GetProperty(propertyInfo.Name).GetValue(t).ToString()) == 0 && "ID".Equals(propertyInfo.Name)))
                        {
                            columns.Add(name);
                            values.Add(t.GetType().GetProperty(name).GetValue(t).ToString());
                        }
                    }
                }
            }

            if (!IsContains(propertyInfoList, conditionColumnList))
            {
                Logger.Error("ConditionColumnList is not contained with list column", null);
                return null;
            }
            else if (!IsContains(columns, conditionColumnList))
            {
                Logger.Error("Columns of T is not contained with conditionColumnList", null);
                return null;
            }
            else if (conditionColumnList.Count != conditionList.Count)
            {
                Logger.Error("ConditionColumnList is not matching conditionList", null);
                return null;

            }
            StringBuilder conditionString = new StringBuilder();

            for (int i = 0; i < conditionColumnList.Count; i++)
            {
                if (0 == i)
                {
                    conditionString.Append(" WHERE ");
                }
                conditionString.Append(conditionColumnList[i]);
                conditionString.Append(" ");
                conditionString.Append(conditionList[i]);
                conditionString.Append(" ");
                if (conditionList[i].ToLowerInvariant().Equals("like"))
                {
                    conditionString.Append("\"%" + values[i] + "%\"");
                }
                else
                {
                    conditionString.Append(values[i]);

                }

                if (conditionColumnList.Count - 1 != i)
                {
                    conditionString.Append(" ");
                    if (null != conditionKeywordList)
                    {
                        conditionString.Append(conditionKeywordList[i]);
                    }
                    else
                    {
                        conditionString.Append("AND");
                    }
                    conditionString.Append(" ");
                }
            }

            if (limit != null)
            {
                conditionString.Append(" Limit " + limit);
            }

            String sql = "Select * From " + t.GetType().Name + conditionString.ToString() + ";";

            return await Task.Run(() =>
            {
                ReaderWriterLock writerLock = GetReaderWriterLock<T>();
                try
                {
                    writerLock.AcquireReaderLock(-1);
                    return DataToModel.DataTableToList<T>(Select(sql));
                }
                finally
                {
                    writerLock.ReleaseReaderLock();
                }
            },cancellationToken);

        }

        public async Task<List<T>> Select<T>(List<string> conditionColumnList, List<string> conditionList, T t, CancellationToken cancellationToken)
        {
            return await Select<T>(conditionColumnList, conditionList, t, null, null,cancellationToken);
        }

        public async Task<List<T>> Select<T>(List<string> conditionList, T t, CancellationToken cancellationToken)
        {
            List<string> conditionColumnList = new List<string>();
            foreach (PropertyInfo propertyInfo in t.GetType().GetProperties())
            {
                if (!(propertyInfo.GetType() == typeof(int) && int.Parse(t.GetType().GetProperty(propertyInfo.Name).GetValue(t).ToString()) == 0 && "ID".Equals(propertyInfo.Name)))
                {
                    object obj = t.GetType().GetProperty(propertyInfo.Name).GetValue(t);
                    if (null != obj && !"".Equals(obj.ToString().Trim()))
                    {
                        conditionColumnList.Add(propertyInfo.Name);
                    }
                }
            }
            return await Select<T>(conditionColumnList, conditionList, t, null, null,cancellationToken);
        }
    }
}
