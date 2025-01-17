﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using Magix.Brix.Components.ActiveTypes;
using Magix.Brix.Components.ActiveTypes.Publishing;
using Magix.Brix.Data;
using System.Reflection;
using Magix.Brix.Publishing.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security;

namespace Magix.Brix.Components.ActiveControllers.Publishing
{
    /**
     * Level2: Initializes WebParts and such while being injected onto WebPage
     */
    [ActiveController]
    public class WebPart_Controller : ActiveController
    {
        /**
         * Level2: Initializes the Plugin with its values according to what's in its settings
         */
        [ActiveEvent(Name = "Magix.Publishing.InitializePublishingPlugin")]
        protected void Magix_Publishing_InitializePublishingPlugin(object sender, ActiveEventArgs e)
        {
            WebPart t = WebPart.SelectByID(e.Params["ID"].Get<int>());

            Type moduleType = Adapter.ActiveModules.Find(
                delegate(Type idx)
                {
                    return idx.FullName == t.Container.ModuleName;
                });

            foreach (PropertyInfo idx in 
                moduleType.GetProperties(
                    BindingFlags.Public | 
                    BindingFlags.NonPublic | 
                    BindingFlags.Instance))
            {
                ModuleSettingAttribute[] atrs =
                    idx.GetCustomAttributes(typeof(ModuleSettingAttribute), true)
                    as ModuleSettingAttribute[];

                if (atrs != null && atrs.Length > 0)
                {
                    string propName = idx.Name;
                    foreach (WebPart.WebPartSetting idxSet in t.Settings)
                    {
                        if (idxSet.Name == moduleType.FullName + idx.Name)
                        {
                            object nValue = Convert.ChangeType(idxSet.Value, idx.PropertyType, CultureInfo.InvariantCulture);
                            idx.GetSetMethod(true).Invoke(e.Params["_ctrl"].Value, new object[] { nValue });
                            break;
                        }
                    }
                }
            }
        }

        /**
         * Level2: Returns the Value of a specific setting of a specific WebPart
         */
        [ActiveEvent(Name = "Magix.Publishing.GetWebPartSettingValue")]
        private void Magix_Publishing_GetWebPartSettingValue(object sender, ActiveEventArgs e)
        {
            WebPart part = WebPart.SelectByID(e.Params["WebPartID"].Get<int>());

            foreach (WebPart.WebPartSetting idx in part.Settings)
            {
                if (idx.Name == part.Container.ModuleName + e.Params["Name"].Get<string>())
                {
                    e.Params["Value"].Value = idx.Value;
                    break;
                }
            }
        }

        /**
         * Level2: Injects one plugin into the given container, unless anything says the
         * Plugin doesn't need to be reloaded for some reasons [sliding menu for instance]
         */
        [ActiveEvent(Name = "Magix.Publishing.InjectPlugin")]
        private void Magix_Publishing_InjectPlugin(object sender, ActiveEventArgs e)
        {
            WebPart page = WebPart.SelectByID(e.Params["ID"].Get<int>());

            Node ch = new Node();

            ch["ModuleName"].Value = page.Container.ModuleName;
            ch["Container"].Value = page.Container.ViewportContainer;
            ch["WebPartID"].Value = page.ID;

            RaiseEvent(
                "Magix.Publishing.ShouldReloadWebPart",
                ch);

            if (!ch.Contains("Stop") || !ch["Stop"].Get<bool>())
            {
                Node node = new Node();

                if (page.Container.MarginBottom != 0)
                    node["BottomMargin"].Value = page.Container.MarginBottom;
                if (!string.IsNullOrEmpty(page.Container.CssClass))
                    node["CssClass"].Value = page.Container.CssClass;
                if (page.Container.Height != 0)
                    node["Height"].Value = page.Container.Height;
                if (page.Container.Last)
                    node["Last"].Value = true;
                if (page.Container.MarginRight != 0)
                    node["PushRight"].Value = page.Container.MarginRight;
                if (page.Container.MarginLeft != 0)
                    node["PushLeft"].Value = page.Container.MarginLeft;
                if (page.Container.MarginBottom != 0)
                    node["SpcBottom"].Value = page.Container.MarginBottom;
                if (page.Container.MarginTop != 0)
                    node["Top"].Value = page.Container.MarginTop;
                if (page.Container.Width != 0)
                    node["Width"].Value = page.Container.Width;
                node["ID"].Value = page.ID;
                node["ModuleInitializationEvent"].Value = "Magix.Publishing.InitializePublishingPlugin";
                node["OriginalWebPartID"].Value = page.ID;

                node["CssClass"].Value = "mux-web-part" + " " + page.Container.CssClass;

                if (page.Container.Overflow)
                    node["OverflowWebPart"].Value = true;

                LoadModule(
                    page.Container.ModuleName,
                    page.Container.ViewportContainer,
                    node);
            }
            else
            {
                // Don't need to Inject Module for some reasons. It might be a Sliding Menu for instance ...
                // Though we DO need to UPDATE SETTINGS for module, since it might still be a different template ...
                Node node = new Node();

                node["MarginBottom"].Value = page.Container.MarginBottom;
                node["CssClass"].Value = page.Container.CssClass;
                node["Height"].Value = page.Container.Height;
                node["Last"].Value = page.Container.Last;
                node["PushRight"].Value = page.Container.MarginRight;
                node["PushLeft"].Value = page.Container.MarginLeft;
                node["SpcBottom"].Value = page.Container.MarginBottom;
                node["Top"].Value = page.Container.MarginTop;
                node["Width"].Value = page.Container.Width;

                string cssClass = "mux-web-part";
                if (page.Container.Overflow)
                    cssClass += " mux-web-part-overflow";
                cssClass += " " + page.Container.CssClass;
                node["CssClass"].Value = cssClass;

                node["Container"].Value = page.Container.ViewportContainer;

                RaiseEvent(
                    "Magix.Core.SetViewPortContainerSettings",
                    node);
            }
        }

        /**
         * Level2: Reloads the Original WebPart content
         */
        [ActiveEvent(Name = "Magix.Publishing.ReloadWebPart")]
        protected void Magix_Publishing_ReloadWebPart(object sender, ActiveEventArgs e)
        {
            WebPart t = WebPart.SelectByID(e.Params["OriginalWebPartID"].Get<int>());

            Node node = new Node();

            node["Container"].Value = e.Params["Parameters"]["Container"].Value;
            node["FreezeContainer"].Value = true;
            node["ID"].Value = t.ID;

            RaiseEvent(
                "Magix.Publishing.InjectPlugin",
                node);
        }
    }
}
