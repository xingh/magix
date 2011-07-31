﻿/*
 * Magix - A Modular-based Framework for building Web Applications 
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * Magix is licensed as GPLv3.
 */

using System;
using Magix.Brix.Data;
using Magix.Brix.Types;
using System.Reflection;
using Magix.Brix.Loader;
using System.Collections.Generic;

namespace Magix.Brix.Components.ActiveTypes.MetaTypes
{
    [ActiveType]
    public class MetaObject : ActiveType<MetaObject>
    {
        [ActiveType]
        public class Value : ActiveType<Value>
        {
            [ActiveField]
            public string Name { get; set; }

            [ActiveField]
            public string Val { get; set; }

            [ActiveField(BelongsTo = true)]
            public MetaObject ParentMetaObject { get; set; }

            internal Value Clone()
            {
                Value ret = new Value();
                ret.Name = Name;
                ret.Val = Val;
                return ret;
            }

            public override void Save()
            {
                // Making sure every single property is uniquely named ...
                bool found = true;
                while (found)
                {
                    found = false;
                    Value other = ParentMetaObject.Values.Find(
                        delegate(Value idx)
                        {
                            return idx.Name == Name &&
                                idx != this;
                        });
                    if(other != null)
                    {
                        // Last added must 'yield' ...
                        other.Name += "_";
                        found = true;
                    }
                }
                base.Save();
            }
        }

        public MetaObject()
        {
            Values = new LazyList<Value>();
        }

        [ActiveField]
        public string TypeName { get; set; }

        [ActiveField]
        public string Reference { get; set; }

        [ActiveField]
        public DateTime Created { get; set; }

        [ActiveField]
        public LazyList<Value> Values { get; set; }

        [ActiveField(IsOwner = false)]
        public LazyList<MetaObject> Children { get; set; }

        [ActiveField(BelongsTo = true, RelationName = "Children")]
        public MetaObject ParentMetaObject { get; set; }

        public override void Save()
        {
            if (ID == 0)
                Created = DateTime.Now;

            foreach (Value idx in Values)
            {
                idx.ParentMetaObject = this;
            }

            // Making sure the reference at the very least says; Anonymous Coward ...
            if (string.IsNullOrEmpty(Reference))
                Reference = "[Anonymous-Coward-Reference]";
            base.Save();
        }

        public MetaObject Clone()
        {
            return DeepClone(this);
        }

        private MetaObject DeepClone(MetaObject metaObject)
        {
            MetaObject ret = new MetaObject();
            ret.TypeName = TypeName;
            ret.Reference = "cloned: " + ID;
            foreach (Value idx in Values)
            {
                Value v = idx.Clone();
                ret.Values.Add(v);
            }
            return ret;
        }
    }
}
