﻿/*
 * Magix - A Modular-based Framework for building Web Applications 
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * Magix is licensed as GPLv3.
 */

using System;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.Brix.Data;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using Magix.Brix.Components.ActiveTypes;
using Magix.Brix.Components.ActiveTypes.Publishing;
using Magix.Brix.Components.ActiveTypes.Users;

namespace Magix.Brix.Components.ActiveControllers.Publishing
{
    [ActiveController]
    public class SliderMenuController : ActiveController
    {
        [ActiveEvent(Name = "Magix.Publishing.GetSliderMenuItems")]
        protected void Magix_Publishing_GetSliderMenuItems(object sender, ActiveEventArgs e)
        {
            foreach (PageObject idx in PageObject.Select())
            {
                if (idx.Parent != null)
                    continue; // Looking for Root Page and starting traversal from it ...

                GetOneMenuItem(e.Params, idx);
            }
        }

        private void GetOneMenuItem(Node node, PageObject po)
        {
            bool canAccess = true;
            if (User.Current != null)
            {
                foreach (Role idx in User.Current.Roles)
                {
                    Node xx = new Node();

                    xx["ID"].Value = po.ID;

                    RaiseEvent(
                        "Magix.Publishing.GetRolesListForPage",
                        xx);

                    if (xx.Contains("ActiveRoles") &&
                        !xx["ActiveRoles"]["r-" + idx.ID]["HasAccess"].Get<bool>())
                    {
                        canAccess = false;
                    }
                }
            }
            else
            {
                Node xx = new Node();

                xx["ID"].Value = po.ID;

                RaiseEvent(
                    "Magix.Publishing.GetRolesListForPage",
                    xx);

                if (xx.Contains("ActiveRoles"))
                {
                    canAccess = false;
                }
            }

            if (canAccess)
            {
                node["Items"]["i" + po.ID]["Caption"].Value = po.Name;
                node["Items"]["i" + po.ID]["Event"]["Name"].Value = "Magix.Publishing.SliderMenuItemClicked";
                node["Items"]["i" + po.ID]["Event"]["MenuItemID"].Value = po.URL;
                foreach (PageObject idx in po.Children)
                {
                    GetOneMenuItem(node["Items"]["i" + po.ID], idx);
                }
            }
        }

        [ActiveEvent(Name = "Magix.Publishing.SliderMenuItemClicked")]
        protected void Magix_Publishing_SliderMenuItemClicked(object sender, ActiveEventArgs e)
        {
            PageObject o = PageObject.SelectFirst(Criteria.Eq("URL", e.Params["MenuItemID"].Get<string>()));

            Node node = new Node();

            node["ID"].Value = o.ID;

            RaiseEvent(
                "Magix.Publishing.OpenPage",
                node);
        }
    }
}






















