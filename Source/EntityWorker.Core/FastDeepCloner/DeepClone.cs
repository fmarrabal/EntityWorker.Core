﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityWorker.Core.FastDeepCloner
{
    public static class DeepCloner
    {

        public static void CleanCachedItems()
        {
            FastDeepClonerCachedItems.CleanCachedItems();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectToBeCloned">Desire object to cloned</param>
        /// <param name="fieldType">Clone Method</param>
        /// <returns></returns>
        public static T Clone<T>(T objectToBeCloned, FastDeepClonerSettings settings) where T : class
        {
            return (T)new ClonerShared(settings).Clone(objectToBeCloned);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectToBeCloned">Desire object to cloned</param>
        /// <param name="fieldType">Clone Method</param>
        /// <returns></returns>
        public static object Clone(object objectToBeCloned, FieldType fieldType = FieldType.PropertyInfo)
        {
            return new ClonerShared(fieldType).Clone(objectToBeCloned);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectToBeCloned">Desire object to cloned</param>
        /// <param name="fieldType">Clone Method</param>
        /// <returns></returns>
        public static T Clone<T>(T objectToBeCloned, FieldType fieldType = FieldType.PropertyInfo) where T : class
        {
            return (T)new ClonerShared(fieldType).Clone(objectToBeCloned);
        }

        /// <summary>
        /// Create CreateInstance()
        /// this will use expression to create new object from the cached expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateInstance<T>() where T : class
        {
            return (T)typeof(T).Creator();
        }

        /// <summary>
        /// Create CreateInstance()
        /// this will use expression to create new object from the cached expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object CreateInstance(Type type)
        {
            return type.Creator();
        }

        /// <summary>
        /// will return fieldInfo from the cached fieldinfo. Get and set value is much faster.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<IFastDeepClonerProperty> GetFastDeepClonerFields(Type type)
        {
            return type.GetFastDeepClonerFields().Values.ToList();
        }

        /// <summary>
        /// will return propertyInfo from the cached propertyInfo. Get and set value is much faster.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<IFastDeepClonerProperty> GetFastDeepClonerProperties(Type type)
        {
            return type.GetFastDeepClonerProperties().Values.ToList();
        }

        public static IFastDeepClonerProperty GetField(this Type type, string name)
        {
            return type.GetFastDeepClonerFields().ContainsKey(name)
                ? type.GetFastDeepClonerFields()[name]
                : null;
        }

        public static IFastDeepClonerProperty GetProperty(this Type type, string name)
        {
            return type.GetFastDeepClonerProperties().ContainsKey(name)
                ? type.GetFastDeepClonerProperties()[name]
                : null;
        }
    }
}
