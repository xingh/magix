﻿/*
 * MagicBRIX - A Web Application Framework for ASP.NET
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * MagicBRIX is licensed as GPLv3.
 */

using System;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;
using System.Web.UI.HtmlControls;

namespace Magix.Brix.Components.ActiveModules.DBAdmin
{
    [ActiveModule]
    public class ConfigureColumns : Module, IModule
    {
        protected System.Web.UI.WebControls.Repeater rep;

        public override void InitialLoading(Node node)
        {
            base.InitialLoading(node);
            Load +=
                delegate
                {
                    Node data = new Node();
                    if (DataSource.Contains("WhiteListColumns"))
                    {
                        foreach (Node idx in DataSource["Type"]["Properties"])
                        {
                            string name = idx.Name;
                            if (DataSource["WhiteListColumns"].Contains(name) &&
                                DataSource["WhiteListColumns"][name].Get<bool>())
                            {
                                data.Add(idx);
                            }
                        }
                    }
                    else
                        data = DataSource["Type"]["Properties"];
                    rep.DataSource = data;
                    rep.DataBind();
                    CheckBox ch = Selector.SelectFirst<CheckBox>(rep);
                    new EffectTimeout(500)
                        .ChainThese(
                            new EffectFocusAndSelect(ch))
                        .Render();
                };
        }

        protected void CheckedChange(object sender, EventArgs e)
        {
            CheckBox b = sender as CheckBox;
            string columnName = b.Info;
            Node node = new Node();
            node["ColumnName"].Value = columnName;
            node["FullTypeName"].Value = DataSource["FullTypeName"].Value;
            node["Visible"].Value = b.Checked;
            RaiseSafeEvent(
                "DBAdmin.Data.ChangeVisibilityOfColumn",
                node);
        }

        protected void CheckedChangeFromLabel(object sender, EventArgs e)
        {
            Label l = sender as Label;
            string columnName = l.Info;
            CheckBox b = Selector.SelectFirst<CheckBox>(l.Parent);
            b.Checked = !b.Checked;
            b.Focus();
            Node node = new Node();
            node["ColumnName"].Value = columnName;
            node["FullTypeName"].Value = DataSource["FullTypeName"].Value;
            node["Visible"].Value = b.Checked;
            RaiseSafeEvent(
                "DBAdmin.Data.ChangeVisibilityOfColumn",
                node);
        }

        protected void EscKey(object sender, EventArgs e)
        {
            ActiveEvents.Instance.RaiseClearControls("child");
        }

        protected override void ReDataBind()
        {
        }
    }
}



















