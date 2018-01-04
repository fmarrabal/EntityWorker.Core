﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using EntityWorker.Core.Attributes;
using EntityWorker.Core.Interface;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.Object.Library;
using FastDeepCloner;

namespace EntityWorker.Core.Helper
{
    /// <summary>
    /// UseFull Extension, that work with EntityWorker.Core
    /// </summary>
    public static class Extension
    {
        private static readonly Dictionary<IFastDeepClonerProperty, string> CachedPropertyNames = new Dictionary<IFastDeepClonerProperty, string>();

        private static readonly Dictionary<Type, string> DbMsSqlMapper = new Dictionary<Type, string>()
        {
            {typeof(int), "BIGINT"},
            {typeof(long), "BIGINT"},
            {typeof(string), "NVARCHAR(4000)"},
            {typeof(bool), "BIT"},
            {typeof(DateTime), "DATETIME"},
            {typeof(float), "FLOAT"},
            {typeof(decimal), "DECIMAL(18,5)"},
            {typeof(Guid), "UNIQUEIDENTIFIER"},
            {typeof(byte[]), "varbinary(MAX)"},
        };

        private static readonly Dictionary<Type, string> DbSQLiteMapper = new Dictionary<Type, string>()
        {
            {typeof(int), "BIGINT"},
            {typeof(long), "BIGINT"},
            {typeof(string), "NVARCHAR(4000)"},
            {typeof(bool), "BIT"},
            {typeof(DateTime), "DATETIME"},
            {typeof(float), "FLOAT"},
            {typeof(decimal), "DECIMAL(18,5)"},
            {typeof(Guid), "TEXT"},
            {typeof(byte[]), "BLOB"},
        };

        /// <summary>
        /// Clear all ids
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="includeIndependedData"> Clear all ids of object that contains IndependedData attributes</param>
        public static T ClearAllIdsHierarchy<T>(this T item, bool includeIndependedData = false) where T : IDbEntity
        {
            return (T)ClearAllIdsHierarchy(item as object, includeIndependedData);
        }

        /// <summary>
        /// Clear all ids
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="includeIndependedData"> Clear all ids of object that contains IndependedData attributes</param>
        public static List<T> ClearAllIdsHierarchy<T>(List<T> item, bool includeIndependedData = false) where T : IDbEntity
        {
            return (List<T>)ClearAllIdsHierarchy(item as object, includeIndependedData);
        }

        /// <summary>
        /// Clear all ids
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="includeIndependedData"> Clear all ids of object that contains IndependedData attributes</param>
        public static object ClearAllIdsHierarchy(object item, bool includeIndependedData = false)
        {
            if (item == null)
                return item;
            if (item is IList)
            {
                foreach (var tm in item as IList)
                {
                    if ((tm as IDbEntity) != null)
                        ClearAllIdsHierarchy(tm, includeIndependedData);

                }
            }
            else
            {
                if ((item as IDbEntity) == null)
                    return item;

                var props = DeepCloner.GetFastDeepClonerProperties(item.GetType());
                foreach (var p in props)
                {
                    if (p.ContainAttribute<ExcludeFromAbstract>() || (p.ContainAttribute<IndependentData>() && !includeIndependedData))
                        continue;
                    if (!p.IsInternalType)
                    {
                        ClearAllIdsHierarchy(p.GetValue(item), includeIndependedData);
                        continue;
                    }
                    else if (p.ContainAttribute<PrimaryKey>() || p.ContainAttribute<ForeignKey>())
                        p.SetValue(item, MethodHelper.ConvertValue(null, p.PropertyType)); // Reset to default

                }
            }

            return item;
        }

        /// <summary>
        /// TrimEnd with string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TrimEnd(this string source, string value)
        {
            return !source.EndsWith(value) ? source : source.Remove(source.LastIndexOf(value, StringComparison.Ordinal));
        }

        /// <summary>
        /// Try and insert Last
        /// </summary>
        /// <param name="str">Source string</param>
        /// <param name="text"> string to insert</param>
        /// <param name="identifier"></param>
        /// <param name="insertLastIfNotFound">insert after a specific string, count is from last</param>
        /// <returns></returns>
        public static string InsertAfter(this string str, string text, string identifier, bool insertLastIfNotFound = true)
        {
            var txt = "";
            var found = false;
            for (var j = str.Length - 1; j >= 0; j--)
            {
                txt = str[j] + txt;
                if (txt == identifier)
                {
                    str = str.Insert(j, text);
                    found = true;
                    break;
                }
                else if (txt.Length >= identifier.Length || txt.Last() != identifier.Last() || (txt.Length >= 2 && identifier.Length >= 2 && txt[txt.Length - 2] != identifier[identifier.Length - 2]))
                    txt = "";
            }

            if (!found && insertLastIfNotFound)
                str += text;
            return str;
        }

        public static string InsertBefore(this string str, string text, string identifier, bool insertLastIfNotFound = true)
        {
            var txt = "";
            var found = false;
            for (var j = str.Length - 1; j >= 0; j--)
            {
                txt = str[j] + txt;
                if (txt == identifier)
                {
                    str = str.Insert(j, text);
                    found = true;
                    break;
                }
                else if (txt.Length >= identifier.Length || txt.Last() != identifier.Last() || (txt.Length >= 2 && identifier.Length >= 2 && txt[txt.Length - 2] != identifier[identifier.Length - 2]))
                    txt = "";
            }
            if (!found && insertLastIfNotFound)
                str += text;

            return str;
        }


        public static StringBuilder InsertBefore(this StringBuilder str, string text, string identifier, bool insertLastIfNotFound = true)
        {
            str = new StringBuilder(str.ToString().InsertBefore(text, identifier, insertLastIfNotFound));
            return str;
        }

        /// <summary>
        /// GetDbEntity Identifire. must contain a PrimaryKey
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string GetEntityKey(this IDbEntity entity)
        {
            return string.Format("{0}&{1}", entity.GetType().FullName, entity.Id);
        }

        /// <summary>
        /// Convert System Type to SqlType
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static string GetDbTypeByType(this Type type, DataBaseTypes dbType)
        {
            if (type.GetTypeInfo().IsEnum)
                type = typeof(long);

            if (Nullable.GetUnderlyingType(type) != null)
                type = Nullable.GetUnderlyingType(type);
            if (dbType == DataBaseTypes.Mssql)
                return DbMsSqlMapper.ContainsKey(type) ? DbMsSqlMapper[type] : null;
            else return DbSQLiteMapper.ContainsKey(type) ? DbSQLiteMapper[type] : null;
        }

        /// <summary>
        /// if date is between two dates
        /// </summary>
        /// <param name="input"></param>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static bool Between(DateTime input, DateTime date1, DateTime date2)
        {
            return (input > date1 && input < date2);
        }

        /// <summary>
        /// Get PropertyName from the cashed Properties
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static string GetPropertyName(this IFastDeepClonerProperty prop)
        {
            if (CachedPropertyNames.ContainsKey(prop))
                return CachedPropertyNames[prop];
            CachedPropertyNames.Add(prop, prop.GetCustomAttribute<PropertyName>()?.Name ?? prop.Name);
            return CachedPropertyNames[prop];
        }

        /// <summary>
        /// Get the Primary key from type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IFastDeepClonerProperty GetPrimaryKey(this Type type)
        {
            return DeepCloner.GetFastDeepClonerProperties(type).FirstOrDefault(x => x.ContainAttribute<PrimaryKey>());
        }

        /// <summary>
        /// Create Instance
        /// </summary>
        /// <param name="type"></param>
        /// <param name="uninitializedObject"> true for FormatterServices.GetUninitializedObject and false for Activator.CreateInstance </param>
        /// <returns></returns>

        public static object CreateInstance(this Type type, bool uninitializedObject = true)
        {
            return uninitializedObject ? FormatterServices.GetUninitializedObject(type) : Activator.CreateInstance(type);
        }

        /// <summary>
        /// Get IList Actual Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static readonly Dictionary<Type, Type> CachedActualType = new Dictionary<Type, Type>();

        /// <summary>
        /// Get Internal type of IList
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetActualType(this Type type)
        {
            if (CachedActualType.ContainsKey(type))
                return CachedActualType[type];
            lock (CachedActualType)
            {
                if (type.GetTypeInfo().IsArray)
                    CachedActualType.Add(type, type.GetElementType());
                else if (type.GenericTypeArguments.Any())
                    CachedActualType.Add(type, type.GenericTypeArguments.First());
                else if (type.FullName?.Contains("List`1") ?? false)
                    CachedActualType.Add(type, type.GetRuntimeProperty("Item").PropertyType);
                else
                    CachedActualType.Add(type, type);
            }

            return CachedActualType[type];
        }

        /// <summary>
        /// Check if string is base64
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsBase64String(this string str)
        {
            if ((str.Length % 4) != 0)
            {
                return false;
            }

            // Check that the string matches the base64 layout
            var regex = new System.Text.RegularExpressions.Regex(@"[^-A-Za-z0-9+/=]|=[^=]|={3,}$");
            if (regex.Match(str).Success)
            {
                return false;
            }

            try
            {
                // If no exception is caught, then it is possibly a base64 encoded string
                var stream = new MemoryStream(Convert.FromBase64String(str));
                return true;
            }
            catch
            {
                // If exception is caught, then it is not a base64 encoded string
                return false;
            }
        }

        /// <summary>
        /// Convert From type to another type,
        /// make sure to have the same propertyNames in both or you could also map them by PropertyName Attributes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static T ToType<T>(this object o) where T : class
        {
            object item;
            if (typeof(T) == typeof(IList))
                return (T)(item = new LightDataTable(o).Rows.Select(x => ((IList)x.ToObject<T>())[0]).ToList());
            else return new LightDataTable(o).Rows.First().ToObject<T>();
        }

        /// <summary>
        /// Clone Object, se FastDeepCloner fo more information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static List<T> Clone<T>(this List<T> items, FieldType fieldType = FieldType.PropertyInfo) where T : class, IDbEntity
        {
            return DeepCloner.Clone(items, new FastDeepClonerSettings()
            {
                FieldType = fieldType,
                OnCreateInstance = new Extensions.CreateInstance(FormatterServices.GetUninitializedObject)
            });
        }


        internal static List<string> ConvertExpressionToIncludeList(this Expression[] actions, bool onlyLast = false)
        {
            var result = new List<string>();
            if (actions == null) return result;
            foreach (var exp in actions)
            {
                var tempList = new List<string>();
                var expString = exp.ToString().Split('.');
                var propTree = "";
                foreach (var item in expString)
                {
                    var x = item.Trim().Replace("(", "").Replace(")", "").Replace("&&", "").Replace("||", "").Trim();
                    if (x.Any(char.IsWhiteSpace) || x.Contains("="))
                        continue;
                    propTree += ("." + x);
                    if (propTree.Split('.').Length == 4)
                        propTree = string.Join(".", propTree.Split('.').Skip(2));
                    tempList.Add(propTree.TrimStart('.'));
                }


                if (!onlyLast)
                    result.AddRange(tempList);
                else if (tempList.Any())
                {
                    var str = tempList.Last();
                    str = str?.Split('.').Length >= 2 ? string.Join(".", str.Split('.').Reverse().Take(2).Reverse()) : str;
                    result.Add(str);
                }
            }
            return result;
        }

        internal static Type TypeByTypeAndDbIsNull(this Type propertyType, bool allowDbNull)
        {
            if (propertyType == typeof(int))
                return allowDbNull ? typeof(int?) : typeof(int);

            if (propertyType == typeof(decimal))
                return allowDbNull ? typeof(decimal?) : typeof(decimal);

            if (propertyType == typeof(double))
                return allowDbNull ? typeof(double?) : typeof(double);

            if (propertyType == typeof(float))
                return allowDbNull ? typeof(float?) : typeof(float);

            if (propertyType == typeof(DateTime))
                return allowDbNull ? typeof(DateTime?) : typeof(DateTime);

            if (propertyType == typeof(long))
                return allowDbNull ? typeof(long?) : typeof(long);

            if (propertyType == typeof(TimeSpan))
                return allowDbNull ? typeof(TimeSpan?) : typeof(TimeSpan);

            if (propertyType == typeof(bool))
                return allowDbNull ? typeof(bool?) : typeof(bool);

            return propertyType == typeof(byte[]) ? typeof(byte[]) : typeof(string);
        }

        private static readonly Dictionary<string, Exception> CachedSqlException = new Dictionary<string, Exception>();
        private static readonly Dictionary<string, LightDataTable> CachedGetSchemaTable = new Dictionary<string, LightDataTable>();
        internal static ILightDataTable ReadData(this ILightDataTable data, DataBaseTypes dbType, IDataReader reader, DbCommandExtended command, string primaryKey = null)
        {
            var i = 0;
            if (reader.FieldCount <= 0)
                return data;
            data.TablePrimaryKey = primaryKey;

            if (reader.FieldCount <= 0)
            {
                reader.Close();
                reader.Dispose();
                return data;
            }
            try
            {
                var key = command?.TableType != null ? command.TableType.FullName : command.Command.CommandText;
                if (!CachedSqlException.ContainsKey(command.Command.CommandText))
                {
                    if (!CachedGetSchemaTable.ContainsKey(key))
                    {
                        CachedGetSchemaTable.Add(key, new LightDataTable(reader.GetSchemaTable()));

                    }
                    foreach (var item in CachedGetSchemaTable[key].Rows)
                    {
                        var columnName = item.Value<string>("ColumnName");
                        data.TablePrimaryKey = data.TablePrimaryKey == null && item.Columns.ContainsKey("IsKey") && item.TryValueAndConvert<bool>("IsKey", false) ? columnName : data.TablePrimaryKey;
                        var dataType = TypeByTypeAndDbIsNull(item["DataType"] as Type,
                            item.TryValueAndConvert<bool>("AllowDBNull", true));
                        if (data.Columns.ContainsKey(columnName))
                            columnName = columnName + i;
                        data.AddColumn(columnName, dataType);

                        i++;
                    }
                }
                else
                {
                    for (var col = 0; col < reader.FieldCount; col++)
                    {
                        var columnName = reader.GetName(col);
                        var dataType = TypeByTypeAndDbIsNull(reader.GetFieldType(col) as Type, true);
                        if (data.Columns.ContainsKey(columnName))
                            columnName = columnName + i;
                        data.AddColumn(columnName, dataType);
                        i++;
                    }
                }
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(command.Command.CommandText))
                    CachedSqlException.Add(command.Command.CommandText, e);
                return ReadData(data,dbType, reader, command, primaryKey);
            }

            while (reader.Read())
            {
                var row = data.NewRow();
                reader.GetValues(row._itemArray);
                data.AddRow(row);
            }

            reader.Close();
            reader.Dispose();
            return data;
        }
    }
}