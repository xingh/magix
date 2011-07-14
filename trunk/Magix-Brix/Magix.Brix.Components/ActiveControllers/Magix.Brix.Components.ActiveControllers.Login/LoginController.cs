﻿/*
 * Magix - A Modular-based Framework for building Web Applications 
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * Magix is licensed as GPLv3.
 */

using System;
using Magix.Brix.Loader;
using Magix.Brix.Types;
using Magix.Brix.Components.ActiveTypes;

namespace Magix.Brix.Components.ActiveControllers.Login
{
    [ActiveController]
    public class LoginController : ActiveController
    {
        [ActiveEvent(Name = "Magix.Core.LoadLoginModule")]
        protected void Magix_Core_LoadLoginModule(object sender, ActiveEventArgs e)
        {
            LoadModule(
                "Magix.Brix.Components.ActiveModules.Users.Login",
                (e.Params.Contains("Container") ? 
                    e.Params["Container"].Get<string>() : 
                    null),
                e.Params);
        }
    }
}