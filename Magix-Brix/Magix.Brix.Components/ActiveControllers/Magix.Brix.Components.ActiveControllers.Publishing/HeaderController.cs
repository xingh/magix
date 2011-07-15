﻿/*
 * Magix - A Modular-based Framework for building Web Applications 
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * Magix is licensed as GPLv3.
 */

using System;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using Magix.Brix.Components.ActiveTypes;
using Magix.Brix.Components.ActiveTypes.Publishing;

namespace Magix.Brix.Components.ActiveControllers.Publishing
{
    [ActiveController]
    public class HeaderController : ActiveController
    {
        [ActiveEvent(Name = "Magix.Publishing.LoadHeader")]
        protected void Magix_Publishing_LoadHeader(object sender, ActiveEventArgs e)
        {
            if (!e.Params.Contains("Caption"))
                e.Params["Caption"].Value = "Administrator Dashboard";

            // Checking to see if Header module is loaded, and if not, loading it ...
            DynamicPanel header = Selector.FindControl<DynamicPanel>(Page, "content2");

            if (header.Controls.Count == 0 ||
                header.Controls[0].GetType().FullName.IndexOf("_header") == -1)
            {
                Node node = new Node();

                node["Width"].Value = 18;
                node["Top"].Value = 1;
                node["Last"].Value = true;
                node["CssClass"].Value = "headerModule";

                LoadModule(
                    "Magix.Brix.Components.ActiveModules.CommonModules.Header",
                    "content2",
                    node);
            }

            RaiseEvent(
                "Magix.Core.SetFormCaption",
                e.Params);
        }
    }
}