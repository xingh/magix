﻿/*
 * Magix - A Modular-based Framework for building Web Applications 
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * Magix is licensed as GPLv3.
 */

using System;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using Magix.Brix.Data;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using System.Collections;
using Magix.Brix.Components.ActiveTypes;

namespace Magix.Brix.Components.ActiveControllers.DBAdmin
{
    public sealed class Data
    {
        private static Data _instance;

        public static Data Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(Data))
                    {
                        if (_instance == null)
                            _instance = new Data();
                    }
                }
                return _instance;
            }
        }

        public int GetCount(string typeName, Criteria[] pars)
        {
            Type type = GetType(typeName);
            if (type == null)
            {
                Node node = new Node();
                node["FullTypeName"].Value = typeName;
                node["Criteria"].Value = pars;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.GetCount",
                    node);
                return node["Value"].Get<int>();
            }
            else
            {
                if (pars == null)
                    pars = new Criteria[0];
                return (int)type.GetMethod(
                    "CountWhere",
                    BindingFlags.FlattenHierarchy |
                    BindingFlags.Static |
                    BindingFlags.NonPublic |
                    BindingFlags.Public)
                    .Invoke(null, new object[] { pars });
            }
        }

        public void GetObjectTypeNode(string typeName, Node node)
        {
            Type type = GetType(typeName);
            if (type == null)
            {
                node["FullTypeName"].Value = typeName;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.GetObjectTypeNode",
                    node);
            }
            else
            {
                node["FullTypeName"].Value = type.FullName;
                node["TypeName"].Value = type.Name;
                Dictionary<string, Tuple<MethodInfo, ActiveFieldAttribute>> getters =
                    GetMethodInfos(typeName, node);
                foreach (string idxKey in getters.Keys)
                {
                    Tuple<MethodInfo, ActiveFieldAttribute> idx = getters[idxKey];
                    string propertyName = idx.Left.Name.Replace("get_", "");
                    node["Type"]["Properties"][propertyName]["FullTypeName"].Value =
                        idx.Left.ReturnType.FullName;
                    node["Type"]["Properties"][propertyName]["TypeName"].Value =
                        idx.Left.ReturnType.Name;
                    node["Type"]["Properties"][propertyName]["IsList"].Value = false;
                    node["Type"]["Properties"][propertyName]["IsComplex"].Value = false;
                    node["Type"]["Properties"][propertyName]["BelongsTo"].Value = idx.Right.BelongsTo;
                    node["Type"]["Properties"][propertyName]["IsOwner"].Value = idx.Right.IsOwner;
                    node["Type"]["Properties"][propertyName]["RelationName"].Value = idx.Right.RelationName;
                    if (idx.Left.ReturnType.FullName.IndexOf("Types.LazyList") != -1)
                    {
                        node["Type"]["Properties"][propertyName]["IsComplex"].Value = true;
                        node["Type"]["Properties"][propertyName]["IsList"].Value = true;
                        node["Type"]["Properties"][propertyName]["TypeName"].Value =
                            "LazyList<" + idx.Left.ReturnType.GetGenericArguments()[0].Name + ">";
                    }
                    else if (idx.Left.ReturnType.FullName.IndexOf("GenericList") != -1)
                    {
                        node["Type"]["Properties"][propertyName]["IsComplex"].Value = true;
                        node["Type"]["Properties"][propertyName]["IsList"].Value = true;
                        node["Type"]["Properties"][propertyName]["TypeName"].Value =
                            "List<" + idx.Left.ReturnType.GetGenericArguments()[0].Name + ">";
                    }
                    else if (idx.Left.ReturnType.FullName.IndexOf("System.") == -1)
                    {
                        node["Type"]["Properties"][propertyName]["IsComplex"].Value = true;
                    }

                    string settingsVisibleColumnKey = "DBAdmin.VisibleColumns." + typeName + ":" + propertyName;
                    node["Type"]["Properties"][propertyName]["Visible"].Value = 
                        Settings.Instance.Get(settingsVisibleColumnKey, true);
                }
            }
        }

        public void GetObjectNode(Node node, int id, string typeName, Node root)
        {
            object obj = GetObject(id, typeName);
            GetObjectNode(obj, typeName, node, root);
        }

        public void GetComplexPropertyFromObjectNode(
            string typeName,
            int id,
            string propertyName,
            Node node)
        {
            Type type = GetType(typeName);
            if (type == null)
            {
                node["FullTypeName"].Value = typeName;
                node["ID"].Value = id;
                node["PropertyName"].Value = propertyName;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.GetComplexPropertyFromObjectNode",
                    node);
            }
            else
            {
                object obj = GetObject(id, typeName);
                PropertyInfo prop = type.GetProperty(
                    propertyName,
                    BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.Instance);
                ActiveFieldAttribute[] attrs = 
                    prop.GetCustomAttributes(
                        typeof(ActiveFieldAttribute), true) as ActiveFieldAttribute[];
                node["IsChange"].Value = !attrs[0].BelongsTo;
                object child = prop.GetGetMethod(true).Invoke(obj, null);
                if (child != null)
                {
                    node["IsRemove"].Value = !attrs[0].BelongsTo;
                    int idOfChild = GetID(child, prop.PropertyType);
                    node["Object"]["ID"].Value = idOfChild;
                    GetObjectNode(child, prop.PropertyType.FullName, node["Object"], node);
                }
                GetObjectTypeNode(prop.PropertyType.FullName, node);
            }
        }

        public void GetComplexPropertyFromListObjectNode(
            string typeName,
            int id,
            string propertyName,
            Node node,
            int start,
            int end)
        {
            Type type = GetType(typeName);
            if (type == null)
            {
                node["FullTypeName"].Value = typeName;
                node["ID"].Value = id;
                node["PropertyName"].Value = propertyName;
                node["Start"].Value = start;
                node["End"].Value = end;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.GetComplexPropertyFromListObjectNode",
                    node);
            }
            else
            {
                object obj = GetObject(id, typeName);
                PropertyInfo prop = type.GetProperty(
                    propertyName,
                    BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.Instance);
                ActiveFieldAttribute[] attrs = 
                    prop.GetCustomAttributes(typeof(ActiveFieldAttribute), true) as ActiveFieldAttribute[];
                if (attrs[0].BelongsTo)
                {
                    node["IsRemove"].Value = false;
                    node["IsAppend"].Value = false;
                }
                else
                {
                    node["IsRemove"].Value = true;
                    node["IsAppend"].Value = true;
                }
                object child = prop.GetGetMethod(true).Invoke(obj, null);
                Type childType = prop.PropertyType.GetGenericArguments()[0];
                GetObjectTypeNode(childType.FullName, node);
                int idxNo = 0;
                foreach (object idxObj in child as IEnumerable)
                {
                    if (idxNo >= start && idxNo < end)
                    {
                        int idOfChild = GetID(idxObj, childType);
                        node["Objects"]["O-" + idOfChild]["ID"].Value = idOfChild;
                        GetObjectNode(
                            idxObj,
                            childType.FullName,
                            node["Objects"]["O-" + idOfChild], node);
                    }
                    idxNo += 1;
                }
                node["SetCount"].Value = idxNo;
            }
        }

        private void GetObjectNode(object obj, string typeName, Node node, Node root)
        {
            Type type = GetType(typeName);
            if (type == null)
            {
                node["FullTypeName"].Value = typeName;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.GetObjectNode",
                    node);
            }
            else
            {
                Dictionary<string, Tuple<MethodInfo, ActiveFieldAttribute>> getters =
                    GetMethodInfos(typeName, root);
                node["ID"].Value =
                    (int)type.GetProperty("ID")
                    .GetGetMethod()
                    .Invoke(obj, null);
                foreach (string idxKey in getters.Keys)
                {
                    Tuple<MethodInfo, ActiveFieldAttribute> idx = getters[idxKey];
                    string propertyName = idx.Left.Name.Replace("_get", "");
                    object value = idx.Left.Invoke(obj, null);
                    string idxFullTypeName = idx.Left.ReturnType.FullName;
                    switch (idx.Left.ReturnType.FullName)
                    {
                        case "System.String":
                        case "System.Boolean":
                        case "System.Int32":
                        case "System.Byte[]": // TODO: Special treatment ...?
                            if (value == null)
                                node["Properties"][idxKey].Value = null;
                            else
                                node["Properties"][idxKey].Value = value.ToString();
                            break;
                        case "System.Decimal":
                            node["Properties"][idxKey].Value =
                                ((decimal)value).ToString(CultureInfo.InvariantCulture);
                            break;
                        case "System.DateTime":
                            node["Properties"][idxKey].Value =
                                ((DateTime)value).ToString("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                            break;
                        default:
                            if (idxFullTypeName.IndexOf("Magix.Brix.Types.LazyList") == 0)
                            {
                                if (value == null)
                                {
                                    node["Properties"][idxKey].Value = "[#null]";
                                }
                                else
                                {
                                    int count =
                                        (int)idx.Left.ReturnType.GetProperty(
                                            "Count",
                                            BindingFlags.Instance |
                                            BindingFlags.Public)
                                        .GetGetMethod(true)
                                        .Invoke(value, null);
                                    node["Properties"][idxKey].Value = "[#" + count + "]";
                                }
                            }
                            else if (idxFullTypeName.IndexOf("System.Collections.Generics.List") == 0)
                            {
                                if (value == null)
                                {
                                    node["Properties"][idxKey].Value = "[#null]";
                                }
                                else
                                {
                                    int count =
                                        (int)idx.Left.ReturnType.GetProperty(
                                            "Count",
                                            BindingFlags.Instance |
                                            BindingFlags.Public)
                                        .GetGetMethod(true)
                                        .Invoke(value, null);
                                    node["Properties"][idxKey].Value = "[#" + count + "]";
                                }
                            }
                            else
                            {
                                string valueStr = value == null ? "[null]" : "[@" + value.ToString() + "]";
                                node["Properties"][idxKey].Value = valueStr;
                            }
                            break;
                    }
                }
            }
        }

        public void GetObjectsNode(
            string typeName, 
            Node node, 
            int startAt, 
            int endAt,
            Criteria[] pars)
        {
            Type type = GetType(typeName);
            if (type == null)
            {
                node["FullTypeName"].Value = typeName;
                node["Start"].Value = startAt;
                node["End"].Value = endAt;
                node["Criteria"].Value = pars;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.GetObjectsNode",
                    node);
            }
            else
            {
                MethodInfo ret = type.GetMethod(
                    "Select",
                    BindingFlags.Static |
                    BindingFlags.FlattenHierarchy |
                    BindingFlags.Public);
                List<Criteria> parameters = new List<Criteria>(pars);
                bool containsId = false;
                foreach (Criteria idx in parameters)
                {
                    if (idx is CritID)
                    {
                        containsId = true;
                        break;
                    }
                }
                if (!containsId)
                    parameters.Add(Criteria.Range(startAt, endAt, "ID", true));
                IEnumerable enumer =
                    ret.Invoke(null, new object[] { parameters.ToArray() }) as IEnumerable;
                Dictionary<string, Tuple<MethodInfo, ActiveFieldAttribute>> getters =
                    GetMethodInfos(typeName, node);
                foreach (object idx in enumer)
                {
                    int id = GetID(idx, type);
                    GetObjectNode(idx, typeName, node["Objects"]["O-" + id], node);
                }
            }
        }

        public void UpdatePropertyValue(
            int idOfObject, 
            string typeOfObject,
            string propertyName, 
            object newValue)
        {
            Type type = GetType(typeOfObject);
            if (type == null)
            {
                Node node = new Node();
                node["ID"].Value = idOfObject;
                node["FullTypeName"].Value = typeOfObject;
                node["PropertyName"].Value = propertyName;
                node["NewValue"].Value = newValue;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.UpdatePropertyValue",
                    node);
            }
            else
            {
                object obj = GetObject(idOfObject, typeOfObject);
                MethodInfo setter =
                    type.GetProperty(
                        propertyName,
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic)
                    .GetSetMethod(true);
                object valueChanged =
                    Convert.ChangeType(
                        newValue,
                        setter.GetParameters()[0].ParameterType);
                setter.Invoke(
                    obj,
                    new object[] { valueChanged });
                type.GetMethod(
                    "Save",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)
                    .Invoke(obj, null);
            }
        }

        // TODO: Refactor, doesn't support Dynamic Types, referenced in GetFilterForColumn ...!
        public Type GetType(string typeName)
        {
            return (new List<Type>(PluginLoader.Instance.ActiveTypes))
                .Find(delegate(Type idx)
                {
                    return idx.FullName == typeName;
                });
        }

        public void ChangeValue(
            int id,
            string fullTypeName,
            string propertyName,
            string newValue)
        {
            Type type = GetType(fullTypeName);
            if (type == null)
            {
                Node node = new Node();
                node["ID"].Value = id;
                node["FullTypeName"].Value = fullTypeName;
                node["PropertyName"].Value = propertyName;
                node["NewValue"].Value = newValue;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.ChangeValue",
                    node);
            }
            else
            {
                MethodInfo setter =
                    type.GetProperty(
                        propertyName,
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic)
                    .GetSetMethod(true);
                object obj = GetObject(id, fullTypeName);
                object valueChanged = Convert.ChangeType(
                    newValue,
                    setter.GetParameters()[0].ParameterType);
                setter.Invoke(obj, new object[] { valueChanged });
                type.GetMethod(
                    "Save",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)
                    .Invoke(obj, null);
            }
        }

        public List<Criteria> GetCriteria(string fullTypeName, Node node)
        {
            // Creating Criteria from Settings ...
            Dictionary<string, Tuple<MethodInfo, ActiveFieldAttribute>> getters
                = GetMethodInfos(fullTypeName, node);
            List<Criteria> retVal = new List<Criteria>();


            // Adding up any given criterias .....
            if (node.Contains("Criteria"))
            {
                // Using given Criteria
                foreach (Node idx in node["Criteria"])
                {
                    switch (idx["Name"].Get<string>())
                    {
                        case "ParentId":
                            retVal.Add(
                                Criteria.ParentId(
                                    idx["Value"].Get<int>()));
                            break;
                        case "ExistsIn":
                            retVal.Add(
                                Criteria.ExistsIn(
                                    idx["Value"].Get<int>(),
                                    idx["Reversed"].Get<bool>()));
                            break;
                        case "Sort":
                            retVal.Add(
                                Criteria.Sort(
                                    idx["Value"].Get<string>(), 
                                    idx["Ascending"].Get<bool>()));
                            break;
                        default:
                            throw new ApplicationException("Not impemented that criteria yet ...!");
                    }
                }
            }
            
            
            string idFilter =
                Settings.Instance.Get(
                    "DBAdmin.Filter." + fullTypeName + ":ID",
                    "");
            if (!string.IsNullOrEmpty(idFilter))
            {
                // ID filters overrides any other filters ...
                string[] ids = idFilter.Split(',');
                foreach (string idx in ids)
                {
                    int val = int.Parse(idx);
                    retVal.Add(Criteria.Id(val));
                }
            }
            else
            {
                // Non ID filters in existance ...
                foreach (string propertyName in getters.Keys)
                {
                    string filter =
                        Settings.Instance.Get(
                            "DBAdmin.Filter." + fullTypeName +
                            ":" +
                            propertyName, "");
                    if (string.IsNullOrEmpty(filter))
                        continue;
                    if (filter.IndexOf("|") == 0)
                        continue;
                    string function = filter.Split('|')[0];
                    string value = filter.Split('|')[1];
                    switch (function)
                    {
                        case "Lt":
                            retVal.Add(
                                Criteria.Lt(
                                    propertyName,
                                    Convert.ChangeType(
                                        value, getters[propertyName].Left.ReturnType)));
                            break;
                        case "Gt":
                            retVal.Add(
                                Criteria.Mt(
                                    propertyName,
                                    Convert.ChangeType(
                                        value, getters[propertyName].Left.ReturnType)));
                            break;
                        case "Eq":
                            retVal.Add(
                                Criteria.Eq(
                                    propertyName,
                                    Convert.ChangeType(
                                        value, getters[propertyName].Left.ReturnType)));
                            break;
                        case "Like":
                            retVal.Add(
                                Criteria.Like(
                                    propertyName,
                                    value.Replace("*", "%").Replace("?", "_")));
                            break;
                        case "In":
                            {
                                int nVal = int.Parse(value);
                                Type returnType = getters[propertyName].Left.ReturnType;
                                Type type = GetType(fullTypeName);
                                PropertyInfo info =
                                    type.GetProperty(
                                        propertyName,
                                        BindingFlags.Instance |
                                        BindingFlags.Public |
                                        BindingFlags.NonPublic);
                                ActiveFieldAttribute attr =
                                    info.GetCustomAttributes(
                                        typeof(ActiveFieldAttribute),
                                        true)[0] as ActiveFieldAttribute;
                                if (attr.BelongsTo)
                                {
                                    if (string.IsNullOrEmpty(attr.RelationName))
                                    {
                                        retVal.Add(
                                            Criteria.ParentId(
                                                nVal));
                                    }
                                    else
                                    {
                                        retVal.Add(
                                            Criteria.ExistsIn(
                                                nVal, false));
                                    }
                                }
                                else if (attr.IsOwner)
                                {
                                    retVal.Add(
                                        Criteria.HasChild(
                                            nVal));
                                }
                                else
                                {
                                    retVal.Add(
                                        Criteria.ExistsIn(
                                            nVal, true));
                                }
                            } break;
                    }
                }
            }
            return retVal;
        }

        public void DeleteObject(int id, string fullTypeName)
        {
            Type type = GetType(fullTypeName);
            if (type == null)
            {
                Node node = new Node();
                node["ID"].Value = id;
                node["FullTypeName"].Value = fullTypeName;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.DeleteObject",
                    node);
            }
            else
            {
                object obj = GetObject(id, fullTypeName);
                type.GetMethod(
                    "Delete",
                    BindingFlags.Public |
                    BindingFlags.Instance)
                    .Invoke(obj, null);
            }
        }

        public int CreateObject(string fullTypeName)
        {
            Type type = GetType(fullTypeName);
            if (type == null)
            {
                Node node = new Node();
                node["FullTypeName"].Value = fullTypeName;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.CreateObject",
                    node);
                return node["ID"].Get<int>();
            }
            else
            {
                object o = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                type.GetMethod("Save").Invoke(o, null);
                int id = (int)type.GetProperty("ID").GetGetMethod(true).Invoke(o, null);
                return id;
            }
        }

        public void AppendObjectToParentPropertyList(
            int id,
            string fullTypeName,
            int parentId,
            string parentPropertyName,
            string parentFullTypeName)
        {
            Type parentType = GetType(parentFullTypeName);
            if (parentType == null)
            {
                Node node = new Node();
                node["ID"].Value = id;
                node["FullTypeName"].Value = fullTypeName;
                node["ParentID"].Value = parentId;
                node["ParentPropertyName"].Value = parentPropertyName;
                node["ParentFullTypeName"].Value = parentFullTypeName;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.AppendObjectToParentPropertyList",
                    node);
            }
            else
            {
                object parent = GetObject(parentId, parentFullTypeName);
                object n = GetObject(id, fullTypeName);
                object list =
                    parentType.GetProperty(
                        parentPropertyName,
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic)
                        .GetGetMethod(true)
                        .Invoke(parent, null);
                Type typeOfList = list.GetType();
                typeOfList.GetMethod(
                    "Add",
                    BindingFlags.Public |
                    BindingFlags.Instance)
                    .Invoke(list, new object[] { n });
                parentType.GetMethod(
                    "Save",
                    BindingFlags.Instance |
                    BindingFlags.Public)
                    .Invoke(parent, null);
            }
        }

        public void ChangeObjectReference(
            int id,
            string fullTypeName,
            int parentId,
            string parentPropertyName,
            string parentFullTypeName)
        {
            Type parentType = GetType(parentFullTypeName);
            if (parentType == null)
            {
                Node node = new Node();
                node["ID"].Value = id;
                node["FullTypeName"].Value = fullTypeName;
                node["ParentID"].Value = parentId;
                node["ParentPropertyName"].Value = parentPropertyName;
                node["ParentFullTypeName"].Value = parentFullTypeName;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.ChangeObjectReference",
                    node);
            }
            else
            {
                object parent = GetObject(parentId, parentFullTypeName);
                object n = GetObject(id, fullTypeName);
                parentType.GetProperty(
                    parentPropertyName,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.NonPublic)
                    .GetSetMethod(true)
                    .Invoke(parent, new object[] { n });
                parentType.GetMethod(
                    "Save",
                    BindingFlags.Instance |
                    BindingFlags.Public)
                    .Invoke(parent, null);
            }
        }

        public void RemoveObjectFromParentPropertyList(
            int id,
            string fullTypeName,
            int parentId,
            string parentPropertyName,
            string parentFullTypeName)
        {
            Type parentType = GetType(parentFullTypeName);
            if (parentType == null)
            {
                Node node = new Node();
                node["ID"].Value = id;
                node["FullTypeName"].Value = fullTypeName;
                node["ParentID"].Value = parentId;
                node["ParentPropertyName"].Value = parentPropertyName;
                node["ParentFullTypeName"].Value = parentFullTypeName;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.RemoveObjectFromParentPropertyList",
                    node);
            }
            else
            {
                object parent = GetObject(parentId, parentFullTypeName);
                object n = GetObject(id, fullTypeName);
                object list =
                    parentType.GetProperty(
                        parentPropertyName,
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic)
                        .GetGetMethod(true)
                        .Invoke(parent, null);
                Type typeOfList = list.GetType();
                typeOfList.GetMethod(
                    "Remove",
                    BindingFlags.Public |
                    BindingFlags.Instance)
                    .Invoke(list, new object[] { n });
                parentType.GetMethod(
                    "Save",
                    BindingFlags.Instance |
                    BindingFlags.Public)
                    .Invoke(parent, null);
            }
        }

        public void RemoveObjectFromParentProperty(
            string fullTypeName,
            int parentId,
            string parentPropertyName,
            string parentFullTypeName)
        {
            Type parentType = GetType(parentFullTypeName);
            if (parentType == null)
            {
                Node node = new Node();
                node["FullTypeName"].Value = fullTypeName;
                node["ParentID"].Value = parentId;
                node["ParentPropertyName"].Value = parentPropertyName;
                node["ParentFullTypeName"].Value = parentFullTypeName;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.RemoveObjectFromParentProperty",
                    node);
            }
            else
            {
                object parent = GetObject(parentId, parentFullTypeName);
                parentType.GetProperty(
                    parentPropertyName,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.FlattenHierarchy |
                    BindingFlags.NonPublic)
                    .GetSetMethod(true)
                    .Invoke(parent, new object[] { null });
                parentType.GetMethod(
                    "Save",
                    BindingFlags.Instance |
                    BindingFlags.Public)
                    .Invoke(parent, null);
            }
        }

        private int GetID(object obj, Type type)
        {
            return (int)type.GetProperty("ID").GetGetMethod().Invoke(obj, null);
        }

        private Dictionary<string, Tuple<MethodInfo, ActiveFieldAttribute>> 
            GetMethodInfos(string typeNamey, Node node)
        {
            Type type = GetType(typeNamey);
            Dictionary<string, Tuple<MethodInfo, ActiveFieldAttribute>> retVal =
                new Dictionary<string, Tuple<MethodInfo, ActiveFieldAttribute>>();
            foreach (PropertyInfo idx in type.GetProperties())
            {
                ActiveFieldAttribute[] attrs =
                    idx.GetCustomAttributes(
                        typeof(ActiveFieldAttribute), true) as ActiveFieldAttribute[];
                if (attrs != null && attrs.Length > 0)
                {
                    bool add = true;
                    if (node.Contains("WhiteListColumns"))
                    {
                        add = false;
                        foreach (Node idxN in node["WhiteListColumns"])
                        {
                            if (idx.Name == idxN.Name)
                            {
                                add = true;
                                break;
                            }
                        }
                    }
                    if (add)
                    {
                        // Serializable property...
                        retVal[idx.Name] =
                            new Tuple<MethodInfo, ActiveFieldAttribute>(
                                idx.GetGetMethod(true),
                                attrs[0]);
                    }
                }
            }
            return retVal;
        }

        private object GetObject(int id, string typeName)
        {
            Type type = GetType(typeName);
            if (type == null)
            {
                Node node = new Node();
                node["FullTypeName"].Value = typeName;
                node["ID"].Value = id;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "DBAdmin.DynamicType.GetObject",
                    node);
                return node["Value"].Value;
            }
            else
            {
                return type.GetMethod(
                    "SelectByID",
                    BindingFlags.Public |
                    BindingFlags.FlattenHierarchy |
                    BindingFlags.Static).Invoke(null, new object[] { id });
            }
        }
    }
}