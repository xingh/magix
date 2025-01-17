﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using System.Web;
using System.Web.UI;
using System.Threading;
using System.Reflection;
using System.Configuration;
using System.Collections.Generic;
using Magix.Brix.Types;
using System.Diagnostics;

namespace Magix.Brix.Loader
{
    /**
     * Level3: Class contains methods for raising events and other helpers, like for instance helpers
     * to load controls and such. Though often you'll not use this directly, but rather use
     * it through helper methods on your ActiveControllers and ActiveModules
     */
    public sealed class ActiveEvents
    {
        private readonly Dictionary<string, List<Tuple<MethodInfo, Tuple<object, bool>>>> _methods =
            new Dictionary<string, List<Tuple<MethodInfo, Tuple<object, bool>>>>();

        private static ActiveEvents _instance;

        private Dictionary<string, List<Tuple<MethodInfo, Tuple<object, bool>>>> _nonWeb = 
            new Dictionary<string, List<Tuple<MethodInfo, Tuple<object, bool>>>>();

        private static Dictionary<string, string> _eventMappers = new Dictionary<string, string>();

        private static Dictionary<string, string> _moduleMappers = new Dictionary<string, string>();

        private delegate void AsyncDelegate(object sender, ActiveEventArgs e);

        private ActiveEvents()
        { }

        /**
         * Level3: This is our Singleton to access our only ActiveEvents object. This is
         * the property you'd use to gain access to the only existing ActiveEvents
         * object in your application pool
         */
        public static ActiveEvents Instance
        {
            [DebuggerStepThrough]
            get
            {
                if (_instance == null)
                {
                    lock (typeof(ActiveEvents))
                    {
                        if (_instance == null)
                            _instance = new ActiveEvents();
                    }
                }
                return _instance;
            }
        }

        /**
         * Level3: Loads a control with the given name (class name) into the given position (name of Magix.UX.Dynamic in
         * the Viewport currently used). Use this method to load Modules. Notice
         * that there exists an overload of this method which takes an object parameter that will be 
         * passed into the InitialLoading method when control is loaded.
         */
        [DebuggerStepThrough]
        public void RaiseLoadControl(string name, string position)
        {
            RaiseLoadControl(name, position, null);
        }

        /**
         * Level3: Loads a control with the given name (class name) into the given position (name of Magix.UX.Dynamic in
         * the Viewport currently used). Use this method to load Modules. This overload of the method
         * will pass the "initializingArgument" parameter into the InitialLoading method when control 
         * is loaded.
         */
        [DebuggerStepThrough]
        public void RaiseLoadControl(string name, string position, Node parameters)
        {
            Node tmpNode = new Node("Magix.Core.LoadActiveModule");

            tmpNode["Name"].Value = name;
            tmpNode["Position"].Value = position;

            if (parameters == null)
                tmpNode["Parameters"].Value = null;
            else
                tmpNode["Parameters"].AddRange(parameters);

            RaiseActiveEvent(this, "Magix.Core.LoadActiveModule", tmpNode);
        }

        /**
         * Level3: Clear all controls out of the position (Magix-Dynamic) of your Viewport.
         */
        [DebuggerStepThrough]
        public void RaiseClearControls(string position)
        {
            Node tmp = new Node("Magix.Core.ClearViewportContainer");
            tmp["Position"].Value = position;
            RaiseActiveEvent(this, "Magix.Core.ClearViewportContainer", tmp);
        }

        /**
         * Level3: Raises an event with null as the initialization parameter.
         * This will dispatch control to all the ActiveEvent that are marked with
         * the Name attribute matching the name parameter of this method call.
         */
        [DebuggerStepThrough]
        public void RaiseActiveEvent(object sender, string name)
        {
            RaiseActiveEvent(sender, name, null);
        }

        [DebuggerStepThrough]
        private List<Tuple<MethodInfo, Tuple<object, bool>>> SlurpAllEventHandlers(string eventName)
        {
            List<Tuple<MethodInfo, Tuple<object, bool>>> retVal = 
                new List<Tuple<MethodInfo, Tuple<object, bool>>>();

            // Adding static methods (if any)
            if (_methods.ContainsKey(eventName))
            {
                foreach (Tuple<MethodInfo, Tuple<object, bool>> idx in _methods[eventName])
                {
                    retVal.Add(idx);
                }
            }

            // Adding instance methods (if any)
            if (InstanceMethod.ContainsKey(eventName))
            {
                foreach (Tuple<MethodInfo, Tuple<object, bool>> idx in InstanceMethod[eventName])
                {
                    retVal.Add(idx);
                }
            }
            return retVal;
        }

        [DebuggerStepThrough]
        private void ExecuteEventMethod(
            MethodInfo method, 
            object context, 
            bool async, 
            object sender, 
            ActiveEventArgs e)
        {
            if (async)
            {
                ThreadPool.QueueUserWorkItem(
                    delegate
                    {
                        try
                        {
                            method.Invoke(context, new[] { context, e });
                        }
                        catch (Exception)
                        {
                            // TODO: I have no idea what to do here...
                        }
                    });
            }
            else
            {
                method.Invoke(context, new[] { sender, e });
            }
        }

        /**
         * Level3: Raises an event. This will dispatch control to all the ActiveEvent that are marked with
         * the Name attribute matching the name parameter of this method call.
         */
        [DebuggerStepThrough]
        public void RaiseActiveEvent(
            object sender, 
            string name, 
            Node pars)
        {
            name = RaiseEventImplementation(sender, name, pars, name);
        }

        [DebuggerStepThrough]
        private string RaiseEventImplementation(object sender, string name, Node pars, string actualName)
        {
            // Dummy dereferencing of PluginLoader to make sure we've 
            // loaded all our assemblies and types first ...!
            PluginLoader typesMumboJumbo = PluginLoader.Instance;

            name = GetEventName(name);
            ActiveEventArgs e = new ActiveEventArgs(actualName, pars);
            if (name != "")
            {
                if (pars != null &&
                    (!pars.Contains("Handled") ||
                    !pars["Handled"].Get<bool>()))
                {
                    RaiseEventImplementation(sender, "", pars, actualName);
                    return name;
                }
            }
            if (pars != null && pars.Contains("Handled"))
                pars["Handled"].UnTie();

            if (_methods.ContainsKey(name) || InstanceMethod.ContainsKey(name))
            {
                // We must run this in two operations since events clear controls out
                // and hence make "dead references" to Event Handlers and such...
                // Therefor we first iterate and find all event handlers interested in
                // this event before we start calling them one by one. But every time in
                // between calling the next one, we must verify that it still exists within
                // the collection...
                List<Tuple<MethodInfo, Tuple<object, bool>>> tmp = SlurpAllEventHandlers(name);

                // Looping through all methods...
                foreach (Tuple<MethodInfo, Tuple<object, bool>> idx in tmp)
                {
                    // Since events might load and clear controls we need to check if the event 
                    // handler still exists after *every* event handler we dispatch control to...
                    List<Tuple<MethodInfo, Tuple<object, bool>>> recheck = SlurpAllEventHandlers(name);

                    foreach (Tuple<MethodInfo, Tuple<object, bool>> idx2 in recheck)
                    {
                        if (idx.Equals(idx2))
                        {
                            ExecuteEventMethod(idx.Left, idx.Right.Left, idx.Right.Right, sender, e);
                            break;
                        }
                    }
                }
            }
            return name;
        }

        /**
         * Level3: Will override the given 'from' ActiveEvent name and make it so that every time
         * anyone tries to raise the 'from' event, then the 'to' event will be raised instead.
         * Useful for 'overriding functionality' in Magix. This can also be accomplished through
         * doing the mapping in the system's web.config file
         */
        public void CreateEventMapping(string from, string to)
        {
            _eventMappers[from] = to;
        }

        /**
         * Level3: Will create a mapping from the 'from' module to the 'to' module
         */
        public void CreateModuleMapping(string from, string to)
        {
            _moduleMappers[from] = to;
        }

        /**
         * Level3: Will destroy the given [key] Active Event Mapping
         */
        public void RemoveMapping(string key)
        {
            _eventMappers.Remove(key);
        }

        /**
         * Level3: Will destroy the given [key] Active Module Mapping
         */
        public void RemoveModuleMapping(string key)
        {
            _moduleMappers.Remove(key);
        }

        /**
         * Level2: Will return the given Value for the given Key Active Event Override
         */
        public string GetEventMappingValue(string key)
        {
            return _eventMappers[key];
        }

        /**
         * Level2: Will return the given Value for the given Key Active Module Override
         */
        public string GetModuleMappingValue(string key)
        {
            if (_moduleMappers.ContainsKey(key))
                return _moduleMappers[key];
            return key;
        }

        /**
         * Level3: Returns an Enumerable of all the Keys in the Event Mapping Collection. Basically
         * which Active Events are overridden
         */
        public IEnumerable<string> EventMappingKeys
        {
            get
            {
                foreach (string idx in _eventMappers.Keys)
                {
                    yield return idx;
                }
            }
        }

        /**
         * Level3: Returns an Enumerable of all the Values in the Event Mapping Collection. Basically
         * where he Overridden Active Events are 'pointing'
         */
        public IEnumerable<string> EventMappingValues
        {
            get
            {
                foreach (string idx in _eventMappers.Values)
                {
                    yield return idx;
                }
            }
        }

        [DebuggerStepThrough]
        private string GetEventName(string name)
        {
            if (_eventMappers.ContainsKey(name))
                return _eventMappers[name];
            else
            {
                string mapped = ConfigurationManager.AppSettings["mapped-" + name];
                if (!string.IsNullOrEmpty(mapped))
                {
                    string evtName = mapped.Replace("mapped-", "");
                    _eventMappers[name] = evtName;
                }
                else
                {
                    // No mapping, defaulting to the default event name ...
                    _eventMappers[name] = name;
                }
                return _eventMappers[name];
            }
        }

        // TODO: Try to remove or make internal somehow...?
        public void RemoveListener(object context)
        {
            // Removing all event handler with the given context (object instance)
            foreach (string idx in InstanceMethod.Keys)
            {
                List<Tuple<MethodInfo, Tuple<object, bool>>> idxCur = InstanceMethod[idx];
                List<Tuple<MethodInfo, Tuple<object, bool>>> toRemove = 
                    new List<Tuple<MethodInfo, Tuple<object, bool>>>();
                foreach (Tuple<MethodInfo, Tuple<object, bool>> idxObj in idxCur)
                {
                    if (idxObj.Right.Left == context)
                        toRemove.Add(idxObj);
                }
                foreach (Tuple<MethodInfo, Tuple<object, bool>> idxObj in toRemove)
                {
                    idxCur.Remove(idxObj);
                }
            }
        }

        private Dictionary<string, List<Tuple<MethodInfo, Tuple<object, bool>>>> InstanceMethod
        {
            [DebuggerStepThrough]
            get
            {
                // NON-web scenario...
                if (HttpContext.Current == null)
                    return _nonWeb;

                Page page = (Page)HttpContext.Current.Handler;
                if (!page.Items.Contains("__Ra.Brix.Loader.ActiveEvents._requestEventHandlers"))
                {
                    page.Items["__Ra.Brix.Loader.ActiveEvents._requestEventHandlers"] =
                        new Dictionary<string, List<Tuple<MethodInfo, Tuple<object, bool>>>>();
                }
                return (Dictionary<string, List<Tuple<MethodInfo, Tuple<object, bool>>>>)
                    page.Items["__Ra.Brix.Loader.ActiveEvents._requestEventHandlers"];
            }
        }

        internal void AddListener(object context, MethodInfo method, string name, bool async)
        {
            if (name == null)
            {
                name = "";
            }
            if (context == null)
            {
                // Static event handler, will *NEVER* be cleared until application
                // itself is restarted
                if (!_methods.ContainsKey(name))
                    _methods[name] = new List<Tuple<MethodInfo, Tuple<object, bool>>>();
                _methods[name].Add(new Tuple<MethodInfo, Tuple<object, bool>>(method, new Tuple<object, bool>(context, async)));
            }
            else
            {
                // Request "instance" event handler, will be tossed away when
                // request is over
                if (!InstanceMethod.ContainsKey(name))
                    InstanceMethod[name] = new List<Tuple<MethodInfo, Tuple<object, bool>>>();
                InstanceMethod[name].Add(new Tuple<MethodInfo, Tuple<object, bool>>(method, new Tuple<object, bool>(context, async)));
            }
        }
    }
}
