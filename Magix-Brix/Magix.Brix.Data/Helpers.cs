﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using System.Reflection;

namespace Magix.Brix.Data.Internal
{
    /**
      * Level3: Static helper class for data-storage Adapter developers. If yo're fiddling around here,
     * you'd better know what you're doing ...!! ;)
      */
    public static class Helpers
    {
        public static string TypeName(Type type)
        {
            ActiveTypeAttribute[] attr = 
                type.GetCustomAttributes(typeof(ActiveTypeAttribute), true)
                as ActiveTypeAttribute[];

            if (attr != null &&
                attr.Length > 0 &&
                !string.IsNullOrEmpty(attr[0].TableName))
                return attr[0].TableName;

            return "doc" + type.FullName;
        }

        public static string PropertyName(PropertyInfo prop)
        {
            return "prop" + prop.Name;
        }

        public static string PropertyName(string propName)
        {
            return "prop" + propName;
        }
    }

    // Static helper class for data-storage Adapter developers.
    public static class CopyOfHelpers
    {
        public static string TypeName(Type type)
        {
            return "doc" + type.FullName;
        }

        public static string PropertyName(PropertyInfo prop)
        {
            return "prop" + prop.Name;
        }

        public static string PropertyName(string propName)
        {
            return "prop" + propName;
        }
    }
}
