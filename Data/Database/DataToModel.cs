using System;
using System.Data;
using System.Collections.Generic;

namespace GatheringTimer.Data.Database
{
    class DataToModel
    {
        /// <summary>
        /// Type enum
        /// </summary>
        private enum ModelType
        {
            //Value Type
            Struct,
            Enum,
            //Reference Type
            String,
            Object,
            Else
        }

        /// <summary>
        /// Get model tpye in ModelType enum
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        private static ModelType GetModelType(Type modelType)
        {
            //Enum Tpye
            if (modelType.IsEnum)
            {
                return ModelType.Enum;
            }
            //Value Tpye
            if (modelType.IsValueType)
            {
                return ModelType.Struct;
            }
            //Reference Tpye string
            if (modelType == typeof(string))
            {
                return ModelType.String;
            }
            //Reference Tpye object or else
            return modelType == typeof(object) ? ModelType.Object : ModelType.Else;
        }

        /// <summary>
        /// DataTable to List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T> DataTableToList<T>(DataTable table)
        {
            var list = new List<T>();
            foreach (DataRow dataRow in table.Rows)
            {
                list.Add(DataRowToModel<T>(dataRow));
            }
            return list;
        }

        /// <summary>
        /// Data row to model(Foreach PropertyInfo[])
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataRow"></param>
        /// <returns></returns>
        public static T DataRowToModel<T>(DataRow dataRow)
        {
            T model;
            Type type = typeof(T);
            ModelType modelType = GetModelType(type);
            switch (modelType)
            {
                //Enum Tpye
                case ModelType.Enum:
                    {
                        model = default;
                        if (dataRow[0] != null)
                        {
                            Type findType = dataRow[0].GetType();
                            if (findType == typeof(int)|| findType == typeof(int?) || findType == typeof(long))
                            {
                                model = (T)dataRow[0];
                            }
                            else if (findType == typeof(string))
                            {
                                model = (T)Enum.Parse(typeof(T), dataRow[0].ToString());
                            }
                        }
                    }
                    break;
                //Value Tpye
                case ModelType.Struct:
                    {
                        model = default;
                        if (dataRow[0] != null)
                            model = (T)dataRow[0];
                    }
                    break;

                //Reference Type(string is executed by Value Type In C#)
                case ModelType.String:
                    {
                        model = default;
                        if (dataRow[0] != null)
                            model = (T)dataRow[0];
                    }
                    break;

                //Reference Type(Return frist row)
                case ModelType.Object:
                    {
                        model = default;
                        if (dataRow[0] != null)
                            model = (T)dataRow[0];
                    }
                    break;

                //Reference Type
                case ModelType.Else:
                    {
                        //Reference Type(CreateInstance is necessaey for T)
                        model = Activator.CreateInstance<T>();
                        //Get Properties in model
                        var modelPropertyInfos = type.GetProperties();
                        //Foreach every property in model and assign homologous row
                        foreach (var propertyInfo in modelPropertyInfos)
                        {
                            //Get property name
                            var name = propertyInfo.Name;
                            if (!dataRow.Table.Columns.Contains(name) || dataRow[name] == null) continue;
                            var propertyInfoType = GetModelType(propertyInfo.PropertyType);
                            switch (propertyInfoType)
                            {
                                case ModelType.Struct:
                                    {
                                        switch (dataRow[name].GetType().ToString())
                                        {
                                            case "System.Int64":
                                                {
                                                    var value = Convert.ToInt32(dataRow[name]);
                                                    propertyInfo.SetValue(model, value, null);
                                                    break;
                                                }
                                            case "System.DBNull":
                                                {
                                                    propertyInfo.SetValue(model, null, null);
                                                    break;
                                                }
                                            default:
                                                {
                                                    var value = Convert.ChangeType(dataRow[name], propertyInfo.PropertyType);
                                                    propertyInfo.SetValue(model, value, null);
                                                    break;
                                                }
                                        }
                                    }
                                    break;
                                case ModelType.Enum:
                                    {
                                        var findType = dataRow[0].GetType();
                                        if (findType == typeof(int))
                                        {
                                            propertyInfo.SetValue(model, dataRow[name], null);
                                        }
                                        else if (findType == typeof(string))
                                        {
                                            var value = (T)Enum.Parse(typeof(T), dataRow[name].ToString());
                                            if (value != null)
                                                propertyInfo.SetValue(model, value, null);
                                        }
                                    }
                                    break;
                                case ModelType.String:
                                    {
                                        var value = Convert.ChangeType(dataRow[name], propertyInfo.PropertyType);
                                        propertyInfo.SetValue(model, value, null);
                                    }
                                    break;
                                case ModelType.Object:
                                    {
                                        propertyInfo.SetValue(model, dataRow[name], null);
                                    }
                                    break;
                                case ModelType.Else:
                                    throw new Exception("Type is not supported");
                                default:
                                    throw new Exception("Unknown Type");
                            }
                        }
                    }
                    break;
                default:
                    model = default;
                    break;
            }
            return model;
        }

    }
}
