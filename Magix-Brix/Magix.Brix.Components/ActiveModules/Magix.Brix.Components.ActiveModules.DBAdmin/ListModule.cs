﻿/*
 * Magix-BRIX - A Web Application Framework for ASP.NET
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * Magix-BRIX is licensed as GPLv3.
 */

using System;
using System.Web.UI;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using Magix.UX.Widgets.Core;
using Magix.Brix.Components.ActiveTypes;
using System.Collections.Generic;
using Magix.UX;

namespace Magix.Brix.Components.ActiveModules.DBAdmin
{
    public abstract class ListModule : Module
    {
        protected abstract Control TableParent { get; }
        protected abstract void DataBindDone();
        private LinkButton rc;

        public override void InitialLoading(Node node)
        {
            base.InitialLoading(node);
            Load +=
                delegate
                {
                };
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Remove Columns button
            rc = new LinkButton();
            rc.ID = "rc";
            rc.Click += rc_Click;
            rc.ToolTip = "Configure visible columns...";
            rc.Text = "&nbsp;";
            Controls.Add(rc);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DataSource != null)
                DataBindGrid();
        }

        protected void rc_Click(object sender, EventArgs e)
        {
            Node node = new Node();
            string fullTypeName = DataSource["FullTypeName"].Get<string>();
            node["FullTypeName"].Value = fullTypeName;
            RaiseSafeEvent(
                "DBAdmin.Form.ShowAddRemoveColumns",
                node);
        }

        protected void DataBindGrid()
        {
            Label table = new Label();
            table.Tag = "table";
            table.CssClass = "viewObjects";

            table.Controls.Add(CreateHeaderForTable());

            foreach (Node idx in DataSource["Objects"])
            {
                table.Controls.Add(CreateRow(idx));
            }

            TableParent.Controls.Add(table);
            DataBindDone();
            if (HasFilteredColumns())
            {
                rc.CssClass = "window-left-buttons window-remove-columns";
                rc.ToolTip = "Remove columns";
            }
            else
            {
                rc.CssClass = "window-left-buttons window-restore-columns";
                rc.ToolTip = "Add removed columns, or remove more columns";
            }
        }

        private bool HasFilteredColumns()
        {
            bool green = true;
            foreach (Node idx in DataSource["Type"]["Properties"])
            {
                if (!GetColumnVisibility(idx.Name))
                {
                    green = false;
                    break;
                }
            }
            return green;
        }

        protected void FlashPanel(Panel pnl)
        {
            new EffectHighlight(pnl.Parent.Parent.Parent, 500)
                .Render();
        }

        protected void FilterMethod(object sender, EventArgs e)
        {
            Node node = new Node();
            node["PropertyName"].Value = (sender as LinkButton).Info;
            node["FullTypeName"].Value = DataSource["FullTypeName"].Get<string>();
            RaiseSafeEvent(
                "DBAdmin.Form.GetFilterForColumn",
                node);
        }

        private Control CreateHeaderForTable()
        {
            Label row = new Label();
            row.Tag = "tr";
            row.CssClass = "header";

            if (DataSource["IsSelect"].Get<bool>())
            {
                Label cS = new Label();
                cS.Tag = "td";
                cS.Text = "Select";
                row.Controls.Add(cS);
            }
            if (DataSource["IsRemove"].Get<bool>())
            {
                Label cS = new Label();
                cS.Tag = "td";
                cS.Text = "Remove";
                row.Controls.Add(cS);
            }
            if (DataSource["IsDelete"].Get<bool>())
            {
                Label cS = new Label();
                cS.Tag = "td";
                cS.Text = "Delete";
                row.Controls.Add(cS);
            }
            Label li = new Label();
            li.Tag = "td";
            bool hasIdFilter = false;
            if (!DataSource["IsFilter"].Get<bool>())
            {
                li.Text = "ID";
            }
            else
            {
                LinkButton b = new LinkButton();
                b.Text = "ID";
                b.ToolTip = "Click to filter ";
                string filterString =
                    Settings.Instance.Get(
                        "DBAdmin.Filter." +
                        DataSource["FullTypeName"].Get<string>() + ":ID", "");
                hasIdFilter = !string.IsNullOrEmpty(filterString);
                b.ToolTip += filterString.Replace("|", " on ");
                b.CssClass =
                    string.IsNullOrEmpty(
                        filterString) ?
                        "" :
                        "filtered overridden";
                bool isFilterOnId = !string.IsNullOrEmpty(filterString);
                b.Click += FilterMethod;
                b.Info = "ID";
                li.Controls.Add(b);
            }
            row.Controls.Add(li);

            foreach (Node idx in DataSource["Type"]["Properties"])
            {
                bool columnVisible = GetColumnVisibility(idx.Name);
                if (!columnVisible)
                    continue;
                Label l = new Label();
                l.Tag = "td";
                if (idx["IsComplex"].Get<bool>() || DataSource["IsFilter"].Get<bool>() == false)
                {
                    l.Text = idx.Name;
                    string toolTip = "";
                    if (idx["BelongsTo"].Get<bool>())
                        toolTip += "BelongsTo ";
                    if (!string.IsNullOrEmpty(idx["RelationName"].Get<string>()))
                        toolTip += "'" + idx["RelationName"].Get<string>() + "' ";
                    l.ToolTip = toolTip;
                }
                else
                {
                    LinkButton b = new LinkButton();
                    b.Text = idx.Name;
                    string filterString =
                        Settings.Instance.Get(
                            "DBAdmin.Filter." +
                            DataSource["FullTypeName"].Get<string>() + ":" + idx.Name, "");
                    b.ToolTip = "Click to filter ";
                    if (idx["BelongsTo"].Get<bool>())
                        b.ToolTip += "BelongsTo ";
                    if (!string.IsNullOrEmpty(idx["RelationName"].Get<string>()))
                        b.ToolTip += "'" + idx["RelationName"].Get<string>() + "' ";
                    if (!string.IsNullOrEmpty(filterString))
                        b.ToolTip += filterString.Replace("|", " on ");
                    b.CssClass =
                        string.IsNullOrEmpty(
                            filterString) ?
                            "" :
                            (hasIdFilter ? "filteredOverridden" : "filtered");
                    b.Click += FilterMethod;
                    b.Info = idx.Name;
                    l.Controls.Add(b);
                }
                row.Controls.Add(l);
            }
            return row;
        }

        private bool GetColumnVisibility(string colName)
        {
            if (ViewState["ColVisible" + colName] == null)
            {
                bool value = Settings.Instance.Get(
                    "DBAdmin.VisibleColumns." +
                    DataSource["FullTypeName"].Get<string>() + ":" + colName,
                    true);
                ViewState["ColVisible" + colName] = value;
            }
            return (bool)ViewState["ColVisible" + colName];
        }

        protected void ResetColumnsVisibility()
        {
            List<string> keys = new List<string>();
            foreach (string idxKey in ViewState.Keys)
            {
                if (idxKey.StartsWith("ColVisible"))
                    keys.Add(idxKey);
            }
            foreach (string idxKey in keys)
            {
                ViewState.Remove(idxKey);
            }
        }

        private int SelectedID
        {
            get { return ViewState["SelectedID"] == null ? -1 : (int)ViewState["SelectedID"]; }
            set { ViewState["SelectedID"] = value; }
        }

        private Control CreateRow(Node node)
        {
            Label row = new Label();
            row.Tag = "tr";
            if (node["ID"].Get<int>() == SelectedID)
                row.CssClass = "grid-selected";
            row.Info = node["ID"].Get<int>().ToString();

            if (DataSource["IsSelect"].Get<bool>())
            {
                Label cS = new Label();
                cS.Tag = "td";
                LinkButton lb2 = new LinkButton();
                lb2.Text = "Select";
                lb2.Click +=
                    delegate(object sender, EventArgs e)
                    {
                        LinkButton b2 = sender as LinkButton;
                        Node n = new Node();
                        n["ID"].Value = int.Parse((b2.Parent.Parent as Label).Info);
                        n["FullTypeName"].Value = DataSource["FullTypeName"].Value;
                        n["ParentID"].Value = DataSource["ParentID"].Value;
                        n["ParentPropertyName"].Value = DataSource["ParentPropertyName"].Value;
                        n["ParentFullTypeName"].Value = DataSource["ParentFullTypeName"].Value;
                        if (DataSource["IsList"].Get<bool>())
                        {
                            RaiseSafeEvent(
                                "DBAdmin.Data.AppendObjectToParentPropertyList",
                                n);
                        }
                        else
                        {
                            RaiseSafeEvent(
                                "DBAdmin.Data.ChangeObjectReference",
                                n);
                        }
                    };
                cS.Controls.Add(lb2);
                row.Controls.Add(cS);
            }
            if (DataSource["IsRemove"].Get<bool>())
            {
                Label cS = new Label();
                cS.Tag = "td";
                LinkButton lb2 = new LinkButton();
                lb2.Text = "Remove";
                lb2.Click +=
                    delegate(object sender, EventArgs e)
                    {
                        LinkButton b2 = sender as LinkButton;
                        Node n = new Node();
                        n["ID"].Value = int.Parse((b2.Parent.Parent as Label).Info);
                        n["FullTypeName"].Value = DataSource["FullTypeName"].Value;
                        n["ParentID"].Value = DataSource["ParentID"].Value;
                        n["ParentPropertyName"].Value = DataSource["ParentPropertyName"].Value;
                        n["ParentFullTypeName"].Value = DataSource["ParentFullTypeName"].Value;
                        RaiseSafeEvent("DBAdmin.Form.RemoveObjectFromParentPropertyList", n);
                    };
                cS.Controls.Add(lb2);
                row.Controls.Add(cS);
            }
            if (DataSource["IsDelete"].Get<bool>())
            {
                Label cS = new Label();
                cS.Tag = "td";
                LinkButton lb2 = new LinkButton();
                lb2.Text = "Delete";
                lb2.Click +=
                    delegate(object sender, EventArgs e)
                    {
                        LinkButton b = sender as LinkButton;
                        Node n = new Node();
                        n["ID"].Value = int.Parse((b.Parent.Parent as Label).Info);
                        n["FullTypeName"].Value = DataSource["FullTypeName"].Value;
                        if (RaiseSafeEvent("DBAdmin.Data.DeleteObject",n))
                            ReDataBind();
                    };
                cS.Controls.Add(lb2);
                row.Controls.Add(cS);
            }
            Label li = new Label();
            li.Tag = "td";
            LinkButton lb = new LinkButton();
            lb.Text = node["ID"].Value.ToString();
            lb.Click +=
                delegate(object sender, EventArgs e)
                {
                    LinkButton b = sender as LinkButton;
                    Node n = new Node();
                    int id = int.Parse((b.Parent.Parent as Label).Info);
                    (b.Parent.Parent as Label).CssClass = "grid-selected";
                    if (SelectedID != -1)
                    {
                        if (id != SelectedID)
                        {
                            Selector.SelectFirst<Label>(this,
                                delegate(Control idxCtrl)
                                {
                                    return idxCtrl is Label && (idxCtrl as Label).Info == SelectedID.ToString();
                                }).CssClass = "";
                        }
                    }
                    SelectedID = id;
                    n["ID"].Value = id;
                    n["FullTypeName"].Value = DataSource["FullTypeName"].Value;
                    RaiseSafeEvent(
                        "DBAdmin.Form.ViewComplexObject",
                        n);
                };
            li.Controls.Add(lb);
            row.Controls.Add(li);
            
            foreach (Node idx in node["Properties"])
            {
                bool columnVisible = GetColumnVisibility(idx.Name);
                if (!columnVisible)
                    continue;
                Label l = new Label();
                l.Tag = "td";
                l.Info = idx.Name;
                if (DataSource["Type"]["Properties"][idx.Name]["IsComplex"].Get<bool>())
                {
                    LinkButton btn = new LinkButton();
                    btn.Text = idx.Get<string>();
                    if (DataSource["Type"]["Properties"][idx.Name]["BelongsTo"].Get<bool>())
                        btn.CssClass = "belongsTo";
                    btn.Info
                        = DataSource["Type"]["Properties"][idx.Name]["IsList"].Get<bool>().ToString();
                    btn.Click +=
                        delegate(object sender, EventArgs e)
                        {
                            LinkButton ed = sender as LinkButton;
                            (ed.Parent.Parent as Label).CssClass = "grid-selected";
                            int id = int.Parse((ed.Parent.Parent as Label).Info);
                            if (SelectedID != -1)
                            {
                                if (id != SelectedID)
                                {
                                    Selector.SelectFirst<Label>(this,
                                        delegate(Control idxCtrl)
                                        {
                                            return idxCtrl is Label && (idxCtrl as Label).Info == SelectedID.ToString();
                                        }).CssClass = "";
                                }
                            }
                            SelectedID = id;
                            string column = (ed.Parent as Label).Info;
                            Node n = new Node();
                            n["ID"].Value = id;
                            n["PropertyName"].Value = column;
                            n["IsList"].Value = bool.Parse(ed.Info);
                            n["FullTypeName"].Value = DataSource["FullTypeName"].Value;
                            RaiseSafeEvent(
                                "DBAdmin.Form.ViewListOrComplexPropertyValue",
                                n);
                        };
                    l.Controls.Add(btn);
                }
                else
                {
                    TextAreaEdit edit = new TextAreaEdit();
                    edit.TextLength = 25;
                    edit.TextChanged +=
                        delegate(object sender, EventArgs e)
                        {
                            TextAreaEdit ed = sender as TextAreaEdit;
                            int id = int.Parse((ed.Parent.Parent as Label).Info);
                            string column = (ed.Parent as Label).Info;
                            Node n = new Node();
                            n["ID"].Value = id;
                            n["PropertyName"].Value = column;
                            n["NewValue"].Value = ed.Text;
                            n["FullTypeName"].Value = DataSource["FullTypeName"].Value;
                            RaiseSafeEvent(
                                "DBAdmin.Data.ChangeSimplePropertyValue",
                                n);
                        };
                    edit.Text = idx.Get<string>();
                    l.Controls.Add(edit);
                }
                row.Controls.Add(l);
            }
            return row;
        }
    }
}
