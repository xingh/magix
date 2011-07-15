﻿/*
 * MagicBRIX - A Web Application Framework for ASP.NET
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * MagicBRIX is licensed as GPLv3.
 */

using System;
using System.Web.UI;
using Magix.UX.Widgets;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using Magix.UX;
using Magix.UX.Widgets.Core;

namespace Magix.Brix.Components.ActiveModules.MetaTypes
{
    [ActiveModule]
    public class EditType : ActiveModule
    {
        protected Panel values;

        public override void InitialLoading(Node node)
        {
            base.InitialLoading(node);
            Load +=
                delegate
                {
                };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DataSource != null)
                DataBindValues();
        }

        private void DataBindValues()
        {
            foreach (Node idx in DataSource["Values"])
            {
                int id = idx["ID"].Get<int>();
                string name = idx["Name"].Get<string>();
                string val = idx["Val"].Get<string>();

                LinkButton btn = new LinkButton();
                btn.Text = "Delete";
                btn.CssClass = "span-2 delete clear-left";
                btn.Click +=
                    delegate
                    {
                        Node node = new Node();
                        node["ID"].Value = id;
                        node["ParentID"].Value = DataSource["ID"].Get<int>();

                        RaiseSafeEvent(
                            "Magix.MetaType.DeleteValue",
                            node);
                    };
                values.Controls.Add(btn);

                InPlaceTextAreaEdit lbl = new InPlaceTextAreaEdit();
                lbl.CssClass += " type-editor span-5";
                lbl.Text = name;
                lbl.TextChanged +=
                    delegate
                    {
                        Node node = new Node();
                        node["ID"].Value = id;
                        node["Name"].Value = lbl.Text;

                        RaiseSafeEvent(
                            "Magix.MetaType.ChangeName",
                            node);
                    };
                values.Controls.Add(lbl);

                InPlaceTextAreaEdit txt = new InPlaceTextAreaEdit();
                txt.Text = val;
                txt.CssClass += " type-editor span-5 last";
                txt.TextChanged +=
                    delegate
                    {
                        Node node = new Node();
                        node["ID"].Value = id;
                        node["Value"].Value = txt.Text;

                        RaiseSafeEvent(
                            "Magix.MetaType.ChangeValue",
                            node);
                    };
                values.Controls.Add(txt);
            }
        }

        protected void create_Click(object sender, EventArgs e)
        {
            Node node = new Node();
            node["ID"].Value = DataSource["ID"].Value;

            RaiseSafeEvent(
                "Magix.MetaType.CreateValue",
                node);

            ReDataBind();
        }

        private void ReDataBind()
        {
            Node node = new Node();
            node["ID"].Value = DataSource["ID"].Get<int>();

            RaiseEvent(
                "Magix.MetaType.EditType",
                node);
        }
    }
}
















