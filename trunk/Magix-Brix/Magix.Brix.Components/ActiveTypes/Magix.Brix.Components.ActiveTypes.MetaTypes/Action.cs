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

namespace Magix.Brix.Components.ActiveTypes.MetaTypes
{
    [ActiveType]
    public class Action : ActiveType<Action>
    {
        [ActiveType]
        public class ActionParams : ActiveType<ActionParams>
        {
            public ActionParams()
            {
                Children = new LazyList<ActionParams>();
            }

            [ActiveField]
            public string Name { get; set; }

            [ActiveField]
            public string Value { get; set; }

            [ActiveField]
            public LazyList<ActionParams> Children { get; set; }
        }

        public Action()
        {
            Params = new LazyList<ActionParams>();
        }

        [ActiveField]
        public string Name { get; set; }

        [ActiveField]
        public string Description { get; set; }

        [ActiveField]
        public string EventName { get; set; }

        [ActiveField]
        public LazyList<ActionParams> Params { get; set; }

        [ActiveField]
        public bool StripInput { get; set; }
    }
}
