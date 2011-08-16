﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;

namespace Magix.Brix.Loader
{
    /**
     * Mark your Active Modules with this attribute. If you mark your Modules with this attribute
     * you can load them using the PluginLoader.LoadControl method.
     */
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ActiveModuleAttribute : Attribute
    {
    }
}
