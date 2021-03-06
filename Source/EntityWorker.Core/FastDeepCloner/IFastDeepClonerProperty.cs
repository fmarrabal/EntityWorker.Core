﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace EntityWorker.Core.FastDeepCloner
{
    public interface IFastDeepClonerProperty
    {
        string Name { get; }

        string FullName { get; }

        void SetValue(object o, object value);

        object GetValue(object o);

        // able to process or clone
        bool CanRead { get; }

        // Have attr FastDeepClonerIgnore
        bool FastDeepClonerIgnore { get; }

        // All available attributes
        AttributesCollections Attributes { get; set; }

        Type PropertyType { get; }

        bool? IsVirtual { get; }

        /// <summary>
        /// Is a reference type eg not GetTypeInfo().IsClass
        /// </summary>
        bool IsInternalType { get; }

        bool ContainAttribute<T>() where T : Attribute;

        bool ContainAttribute(Type type);

        T GetCustomAttribute<T>() where T : Attribute;

        Attribute GetCustomAttribute(Type type);

        MethodInfo PropertyGetValue { get; set; }

        MethodInfo PropertySetValue { get; set; }

        void Add(Attribute attr);

    }
}
