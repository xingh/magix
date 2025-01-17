﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.Configuration;
using System.Collections.Generic;
using Magix.Brix.Data;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using System.Diagnostics;

namespace Magix.Brix.Loader
{
    /**
     * Level4: Helps load UserControls embedded in resources. Relies on that Magix.Brix.Loader.AssemblyResourceProvider
     * is registered as a Virtual Path Provider in e.g. your Global.asax file. Use the Instance method
     * to access the singleton object, then use the LoadControl to load UserControls embedded as resources.
     * Kind of like the Magix' version of Page.LoadControl. Can be used directly by you, if you really
     * know what you're doing though. In general, I'd say DON'T ...!!
     */
    public sealed class PluginLoader
    {
        private static PluginLoader _instance;
        private readonly Dictionary<string, Tuple<string, Type>> _loadedPlugins = new Dictionary<string, Tuple<string, Type>>();
        private readonly List<Type> _controllerTypes = new List<Type>();
        private static List<Assembly> _assemblies;

        private delegate void TypeDelegate(Type type);

        private PluginLoader()
        {
            // Making sure all DLLs are loaded
            MakeSureAllDLLsAreLoaded();

            // Initializing all Active Modules
            FindAllTypesWithAttribute<ActiveModuleAttribute>(
                delegate(Type type)
                {
                    Adapter.ActiveModules.Add(type);
                    string userControlFile = type.FullName + ".ascx";
                    _loadedPlugins[type.FullName] = new Tuple<string, Type>(userControlFile, type);
                    InitializeEventHandlers(null, type);
                });

            // Initializing all Active Controllers
            FindAllTypesWithAttribute<ActiveControllerAttribute>(
                delegate(Type type)
                {
                    _controllerTypes.Add(type);
                    InitializeEventHandlers(null, type);
                });

            // Initializing all Active Records
            FindAllTypesWithAttribute<ActiveTypeAttribute>(
                delegate(Type type)
                {
                    Adapter.ActiveTypes.Add(type);
                    InitializeEventHandlers(null, type);
                });
        }

        /**
         * Level4: A list of all your ActiveControllers in the system
         */
        public List<Type> ActiveControllers
        {
            get
            {
                return _controllerTypes;
            }
        }

        // Helper method that loops through every type in AppDomain and 
        // looks for an attribute of a given type and passes it into a delegate 
        // submitted by the caller...
        private static void FindAllTypesWithAttribute<TAttrType>(TypeDelegate functor)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Consciously skipping Aall GAC assemblies since these are 
                // expected to be .Net Framework assemblies...
                if(assembly.GlobalAssemblyCache)
                    continue;
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        TAttrType[] attributes = type.GetCustomAttributes(
                            typeof(TAttrType),
                            true) as TAttrType[];
                        if (attributes != null && attributes.Length > 0)
                        {
                            // Calling our given delegate with the type...
                            functor(type);
                        }
                    }
                }
                catch
                {
                    ; // Intentionally do nothing...
                }
            }
        }

        /**
         * Level4: Singleton accessor. Allows access to the 'one and only' PluginLoader
         */
        public static PluginLoader Instance
        {
            [DebuggerStepThrough]
            get
            {
                if (_instance == null)
                {
                    lock (typeof(PluginLoader))
                    {
                        if (_instance == null)
                        {
                            _instance = new PluginLoader();

                            // Fire the "Application Startup" event. This one will only trigger
                            // ONCE in comparison to the "Magix.Core.InitialLoading" event which will fire
                            // every time the page reloads...
                            ActiveEvents.Instance.RaiseActiveEvent(
                                null, 
                                "Magix.Core.ApplicationStartup");
                        }
                    }
                }
                return _instance;
            }
        }

        /**
         * Level3: Dynamically load a Control with the given FullName (namespace + type name). This
         * is the method which is internally used in Magix-Brix to load UserControls from 
         * embedded resources and also other controls. Since ActiveEvents might be mapped and
         * overridden, you actually have no guarantee of that the event you wish to raise
         * is the one who will become raised
         */
        public Control LoadActiveModule(string fullTypeName)
        {
            Page page = (HttpContext.Current.Handler as Page);

            // Checking to see if we've got our UnLoad event handlers event here...
            if (page != null && page.Items["__Ra.Brix.Loader.PluginLoader.hasInstantiatedControllers"] == null)
            {
                page.Items["__Ra.Brix.Loader.PluginLoader.hasInstantiatedControllers"] = true;
                InstantiateAllControllers();
            }

            // Looking through configuration mappings to see if Module Key is overloaded...
            string mapping = ConfigurationManager.AppSettings["mapping-" + fullTypeName];

            if (!string.IsNullOrEmpty(mapping))
                fullTypeName = mapping;

            fullTypeName = ActiveEvents.Instance.GetModuleMappingValue(fullTypeName);

            if (!_loadedPlugins.ContainsKey(fullTypeName))
            {
                throw new ArgumentException(
                    "Couldn't find the plugin with the name of; '" + fullTypeName + "'");
            }
            Tuple<string, Type> pluginType = _loadedPlugins[fullTypeName];
            if (string.IsNullOrEmpty(pluginType.Left))
            {
                // Non-UserControl plugin...
                ConstructorInfo constructorInfo = pluginType.Right.GetConstructor(new Type[] { });
                Control retVal = constructorInfo.Invoke(new object[] { }) as Control;
                InitializeEventHandlers(retVal, pluginType.Right);
                return retVal;
            }
            if (page != null)
            {
                // UserControl plugin type...
                Control retVal =
                    page.LoadControl(
                        "~/Magix.Brix.Module/" +
                        pluginType.Right.Assembly.ManifestModule.ScopeName +
                        "/" +
                        pluginType.Left);
                InitializeEventHandlers(retVal, pluginType.Right);
                return retVal;
            }
            return null;
        }

        private void InstantiateAllControllers()
        {
            foreach (Type idxType in _controllerTypes)
            {
                string mapped = ConfigurationManager.AppSettings["exclude-" + idxType.FullName];
                if (mapped == "true")
                    continue;
                string mapped2 = ConfigurationManager.AppSettings["exclude-" + idxType.FullName.Replace(idxType.Name, "*")];
                if (mapped2 == "true")
                    continue;
                object controllerObject = idxType.GetConstructor(System.Type.EmptyTypes).Invoke(null);
                InitializeEventHandlers(controllerObject, idxType);
            }
        }

        private static void InitializeEventHandlers(object objectInstance, Type pluginType)
        {
            // If the context passed is null, then what we're trying to retrieve
            // are the stat event handlers...
            BindingFlags flags = objectInstance == null ?
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static :
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            foreach (MethodInfo idx in pluginType.GetMethods(flags))
            {
                ActiveEventAttribute[] attr =
                    idx.GetCustomAttributes(
                        typeof(ActiveEventAttribute),
                        true) as ActiveEventAttribute[];
                if (attr == null || attr.Length <= 0)
                    continue;
                foreach (ActiveEventAttribute idxAttr in attr)
                {
                    ActiveEvents.Instance.AddListener(objectInstance, idx, idxAttr.Name, idxAttr.Async);
                }
            }
        }

        private static void MakeSureAllDLLsAreLoaded()
        {
            // Only doing this on WEB...!
            if (HttpContext.Current == null)
                return;

            // Sometimes not all DLLs in the bin folder will be included in the
            // current AppDomain. This often happens due to that no types from
            // the DLLs are actually references within the website itself
            // This logic runs through all DLLs in the bin folder of the
            // website to check if they're inside the current AppDomain, and
            // if not loads them up
            List<Assembly> initialAssemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
            DirectoryInfo di = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/bin"));
            LoadDLLsFromDirectory(di, initialAssemblies);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        /**
         * Level4: Will return all assemblies within your Application Pool, minus 'system assemblies'.
         * Useful for 'meta stuff'
         */
        public static List<Assembly> PluginAssemblies
        {
            get
            {
                if (_assemblies != null)
                    return _assemblies;

                _assemblies = new List<Assembly>();

                foreach (Assembly idx in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (idx.GlobalAssemblyCache)
                        continue;
                    try
                    {
                        // This line will throw an exception if this is one of those stupid 
                        // autogenerated assemblies created by ASP.NET upon compilation
                        // first time after change in any module...
                        string mumbo = idx.CodeBase;
                        _assemblies.Add(idx);
                    }
                    catch (Exception)
                    {
                        ; // Intentionally do nothing...!
                    }
                }
                return _assemblies;
            }
        }

        private static void LoadDLLsFromDirectory(DirectoryInfo di, List<Assembly> initialAssemblies)
        {
            foreach (FileInfo idxFile in di.GetFiles("*.dll"))
            {
                try
                {
                    FileInfo info = idxFile;
                    if (initialAssemblies.Exists(
                        delegate(Assembly idx)
                            {
                                return idx.ManifestModule.Name.ToLower() == info.Name.ToLower();
                            }))
                        continue;
                    Assembly.LoadFrom(idxFile.FullName);
                }
                catch (Exception)
                {
                    ; // Intentionally do nothing in case assembly loading throws...!
                    // Especially true for C++ compiled assemblies...
                    // Sample here is the MySQL DLL...
                }
            }
            foreach (DirectoryInfo idxChild in di.GetDirectories())
            {
                LoadDLLsFromDirectory(idxChild, initialAssemblies);
            }
        }

        static private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
        {
            string name = e.Name;
            if (name.Contains(","))
            {
                name = name.Substring(0, name.IndexOf(","));
            }
            foreach (Assembly idx in PluginAssemblies)
            {
                if (idx.CodeBase.Substring(idx.CodeBase.LastIndexOf("/") + 1).Replace(".dll", "").Replace(".DLL", "") == name)
                    return idx;
            }
            return null;
        }
    }
}
