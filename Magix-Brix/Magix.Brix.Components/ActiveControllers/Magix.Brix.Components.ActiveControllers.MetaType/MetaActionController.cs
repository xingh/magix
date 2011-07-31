﻿/*
 * Magix - A Modular-based Framework for building Web Applications 
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * Magix is licensed as GPLv3.
 */

using System;
using Magix.Brix.Loader;
using Magix.Brix.Types;
using Magix.Brix.Components.ActiveTypes.MetaTypes;
using Magix.Brix.Data;
using Magix.UX.Widgets;
using System.Globalization;

namespace Magix.Brix.Components.ActiveControllers.MetaTypes
{
    [ActiveController]
    public class MetaActionController : ActiveController
    {
        [ActiveEvent(Name = "Magix.Publishing.GetPluginMenuItems")]
        protected void Magix_Publishing_GetPluginMenuItems(object sender, ActiveEventArgs e)
        {
            e.Params["Items"]["MetaType"]["Caption"].Value = "Meta Types";

            e.Params["Items"]["MetaType"]["Items"]["Actions"]["Caption"].Value = "Meta Actions ...";
            e.Params["Items"]["MetaType"]["Items"]["Actions"]["Event"]["Name"].Value = "Magix.MetaType.ViewActions";
        }

        [ActiveEvent(Name = "Magix.Meta.FindAction")]
        protected void Magix_Meta_FindAction(object sender, ActiveEventArgs e)
        {
            Node node = new Node();

            node["FullTypeName"].Value = typeof(Action).FullName;
            node["Container"].Value = "content5";
            node["Width"].Value = 21;
            node["Last"].Value = true;
            node["Padding"].Value = 3;
            node["MarginBottom"].Value = 10;
            node["PullTop"].Value = 8;
            node["IsDelete"].Value = false;

            node["WhiteListColumns"]["Name"].Value = true;
            node["WhiteListColumns"]["Name"]["ForcedWidth"].Value = 4;
            node["WhiteListColumns"]["EventName"].Value = true;
            node["WhiteListColumns"]["EventName"]["ForcedWidth"].Value = 5;
            node["WhiteListColumns"]["Description"].Value = true;
            node["WhiteListColumns"]["Description"]["ForcedWidth"].Value = 8;

            node["FilterOnId"].Value = false;
            node["IDColumnName"].Value = "Select";
            node["IDColumnValue"].Value = "Select";
            node["IDColumnEvent"].Value = "Magix.Meta.SelectAction";
            node["CreateEventName"].Value = "Magix.Meta.CreateAction";
            node["MetaTypeID"].Value = e.Params["ID"].Get<int>();

            node["Type"]["Properties"]["Name"]["ReadOnly"].Value = true;
            node["Type"]["Properties"]["EventName"]["ReadOnly"].Value = true;
            node["Type"]["Properties"]["EventName"]["Header"].Value = "Action";
            node["Type"]["Properties"]["Description"]["ReadOnly"].Value = true;

            ActiveEvents.Instance.RaiseActiveEvent(
                this,
                "DBAdmin.Form.ViewClass",
                node);
        }

        [ActiveEvent(Name = "Magix.MetaType.ViewActions")]
        protected void Magix_MetaType_ViewActions(object sender, ActiveEventArgs e)
        {
            Node node = new Node();

            node["FullTypeName"].Value = typeof(Action).FullName;
            node["Container"].Value = "content3";
            node["CssClass"].Value = "edit-actions";
            node["Width"].Value = 18;
            node["Last"].Value = true;

            node["WhiteListColumns"]["Name"].Value = true;
            node["WhiteListColumns"]["Name"]["ForcedWidth"].Value = 9;
            node["WhiteListColumns"]["Params"].Value = true;
            node["WhiteListColumns"]["Params"]["ForcedWidth"].Value = 2;
            node["WhiteListColumns"]["Copy"].Value = true;
            node["WhiteListColumns"]["Copy"]["ForcedWidth"].Value = 3;

            node["FilterOnId"].Value = false;
            node["IDColumnName"].Value = "Edit";
            node["IDColumnEvent"].Value = "Magix.Meta.EditAction";
            node["CreateEventName"].Value = "Magix.Meta.CreateAction";
            node["DeleteColumnEvent"].Value = "Magix.MetaAction.DeleteMetaAction";


            node["Criteria"]["C1"]["Name"].Value = "Sort";
            node["Criteria"]["C1"]["Value"].Value = "Created";
            node["Criteria"]["C1"]["Ascending"].Value = false;

            node["Type"]["Properties"]["Name"]["ReadOnly"].Value = false;
            node["Type"]["Properties"]["Name"]["MaxLength"].Value = 50;
            node["Type"]["Properties"]["Params"]["ReadOnly"].Value = true;
            node["Type"]["Properties"]["Params"]["NoFilter"].Value = true;
            node["Type"]["Properties"]["Params"]["Header"].Value = "Pars.";
            node["Type"]["Properties"]["Copy"]["NoFilter"].Value = true;
            node["Type"]["Properties"]["Copy"]["TemplateColumnEvent"].Value = "Magix.Meta.GetCopyActionTemplateColumn";

            ActiveEvents.Instance.RaiseActiveEvent(
                this,
                "DBAdmin.Form.ViewClass",
                node);
        }

        [ActiveEvent(Name = "Magix.MetaActions.SearchActions")]
        protected void Magix_MetaActions_SearchActions(object sender, ActiveEventArgs e)
        {
            Node node = new Node();

            node["FullTypeName"].Value = typeof(Action).FullName;
            node["Container"].Value = "content6";
            node["Width"].Value = 18;
            node["Padding"].Value = 6;
            node["PullTop"].Value = 28;
            node["MarginBottom"].Value = 30;
            node["Last"].Value = true;
            if (e.Params.Contains("ParentID"))
                node["ParentID"].Value = e.Params["ParentID"].Value;

            node["WhiteListColumns"]["Name"].Value = true;
            node["WhiteListColumns"]["Name"]["ForcedWidth"].Value = 9;
            node["WhiteListColumns"]["Params"].Value = true;
            node["WhiteListColumns"]["Params"]["ForcedWidth"].Value = 2;

            node["NoIdColumn"].Value = true;
            node["IsCreate"].Value = false;
            node["IsDelete"].Value = false;
            node["IsFind"].Value = true;
            node["IsSelect"].Value = true;
            node["GetContentsEventName"].Value = "Magix.MetaAction.GetContentsEventName";
            node["SetFocus"].Value = true;
            node["SelectEvent"].Value = "Magix.MetaAction.ActionWasSelected";
            node["ParentPropertyName"].Value = e.Params["SelectEvent"].Value;


            node["Criteria"]["C1"]["Name"].Value = "Sort";
            node["Criteria"]["C1"]["Value"].Value = "Created";
            node["Criteria"]["C1"]["Ascending"].Value = false;

            node["Type"]["Properties"]["Name"]["ReadOnly"].Value = false;
            node["Type"]["Properties"]["Name"]["MaxLength"].Value = 50;
            node["Type"]["Properties"]["Params"]["ReadOnly"].Value = true;
            node["Type"]["Properties"]["Params"]["NoFilter"].Value = true;
            node["Type"]["Properties"]["Params"]["Header"].Value = "Pars.";

            ActiveEvents.Instance.RaiseActiveEvent(
                this,
                "DBAdmin.Form.ViewClass",
                node);
        }

        [ActiveEvent(Name = "Magix.MetaAction.ActionWasSelected")]
        protected void Magix_MetaAction_ActionWasSelected(object sender, ActiveEventArgs e)
        {
            Action a = Action.SelectByID(e.Params["ID"].Get<int>());
            e.Params["ActionName"].Value = a.Name;

            RaiseEvent(
                e.Params["ParentPropertyName"].Get<string>(),
                e.Params);
        }

        [ActiveEvent(Name = "Magix.MetaAction.GetContentsEventName")]
        protected void Magix_MetaAction_GetContentsEventName(object sender, ActiveEventArgs e)
        {
            if (e.Params.Contains("Filter"))
            {
                e.Params["Criteria"]["C3"]["Name"].Value = "Like";
                e.Params["Criteria"]["C3"]["Value"].Value = "%" + e.Params["Filter"].Get<string>() + "%";
                e.Params["Criteria"]["C3"]["Column"].Value = "Name";
            }
            else if (e.Params.Contains("Criteria") &&
                e.Params["Criteria"].Contains("C3"))
                e.Params["Criteria"]["C3"].UnTie();
            RaiseEvent(
                "DBAdmin.Data.GetContentsOfClass",
                e.Params);
        }

        [ActiveEvent(Name = "Magix.Meta.GetCopyActionTemplateColumn")]
        protected void Magix_Meta_GetCopyActionTemplateColumn(object sender, ActiveEventArgs e)
        {
            // Extracting necessary variables ...
            string name = e.Params["Name"].Get<string>();
            string fullTypeName = e.Params["FullTypeName"].Get<string>();
            int id = e.Params["ID"].Get<int>();
            string value = e.Params["Value"].Get<string>();

            // Creating our SelectList
            LinkButton ls = new LinkButton();
            ls.Text = "Copy";
            ls.Click +=
                delegate
                {
                    Node node = new Node();
                    node["ID"].Value = id;
                    RaiseEvent(
                        "Magix.Meta.CopyAction",
                        node);
                };

            // Stuffing our newly created control into the return parameters, so
            // our Grid control can put it where it feels for it ... :)
            e.Params["Control"].Value = ls;
        }

        [ActiveEvent(Name = "Magix.Meta.CopyAction")]
        protected void Magix_Meta_CopyAction(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                Action a = Action.SelectByID(e.Params["ID"].Get<int>());
                Action clone = a.Copy();

                clone.Save();

                tr.Commit();

                Node n = new Node();

                n["FullTypeName"].Value = typeof(Action).FullName;
                n["ID"].Value = clone.ID;

                RaiseEvent(
                    "DBAdmin.Grid.SetActiveRow",
                    n);

                n = new Node();
                n["FullTypeName"].Value = typeof(Action).FullName;

                RaiseEvent(
                    "Magix.Core.UpdateGrids",
                    n);

                n = new Node();
                n["ID"].Value = clone.ID;

                RaiseEvent(
                    "Magix.Meta.EditAction",
                    n);
            }
        }

        [ActiveEvent(Name = "Magix.Meta.CreateAction")]
        protected void Magix_Meta_CreateAction(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                Action a = new Action();
                a.Name = "Default, please change";
                a.Save();

                tr.Commit();

                Node node = new Node();

                node["Start"].Value = 0;
                node["End"].Value = 10;
                node["FullTypeName"].Value = typeof(Action).FullName;

                RaiseEvent(
                    "Magix.Core.SetGridPageStart",
                    node);

                Node n = new Node();
                n["ID"].Value = a.ID;

                RaiseEvent(
                    "Magix.Meta.EditAction",
                    n);
            }
        }

        [ActiveEvent(Name = "Magix.Meta.EditAction")]
        protected void Magix_Meta_EditAction(object sender, ActiveEventArgs e)
        {
            Action a = Action.SelectByID(e.Params["ID"].Get<int>());
            
            Node node = new Node();
            node["ID"].Value = e.Params["ID"].Value;
            node["FullTypeName"].Value = typeof(Action).FullName;

            EditActionItemParams(a, node);
            EditActionItem(a, node);

            CreateRunParamButton(a);

            CreateCreateParamButton(a);

            CreateDeleteParamButton(a);

            ActiveEvents.Instance.RaiseClearControls("content6");
        }

        private void CreateRunParamButton(Action a)
        {
            Node node = new Node();

            node["Text"].Value = "Run!";
            node["ButtonCssClass"].Value = "span-4";
            node["Append"].Value = true;
            node["Event"].Value = "Magix.Meta.RaiseEvent";
            node["Event"]["ActionID"].Value = a.ID;

            LoadModule(
                "Magix.Brix.Components.ActiveModules.CommonModules.Clickable",
                "content5",
                node);
        }

        private void CreateCreateParamButton(Action a)
        {
            Node node = new Node();

            node["Text"].Value = "New Param ...";
            node["ButtonCssClass"].Value = "span-5";
            node["Append"].Value = true;
            node["Event"].Value = "Magix.Meta.CreateParameter";
            node["Event"]["ID"].Value = a.ID;

            LoadModule(
                "Magix.Brix.Components.ActiveModules.CommonModules.Clickable",
                "content5",
                node);
        }

        private void CreateDeleteParamButton(Action a)
        {
            Node node = new Node();

            node["Text"].Value = "Delete";
            node["Seed"].Value = "delete-button";
            node["ButtonCssClass"].Value = "span-4 last";
            node["Append"].Value = true;
            node["Enabled"].Value = false;
            node["Event"].Value = "Magix.Meta.DeleteParameter";
            node["Event"]["ID"].Value = a.ID;

            LoadModule(
                "Magix.Brix.Components.ActiveModules.CommonModules.Clickable",
                "content5",
                node);
        }

        [ActiveEvent(Name = "Magix.Meta.DeleteParameter")]
        protected void Magix_Meta_DeleteParameter(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                Node node2 = new Node();

                RaiseEvent(
                    "Magix.Core.GetSelectedTreeItem",
                    node2);

                Action.ActionParams p = Action.ActionParams.SelectByID(node2["ID"].Get<int>());
                p.Delete();

                tr.Commit();
            }

            ActiveEvents.Instance.RaiseClearControls("content6");

            Node node = new Node();
            node["Seed"].Value = "delete-button";
            node["Enabled"].Value = false;
            RaiseEvent(
                "Magix.Core.EnabledClickable",
                node);

            // Signalizing to Grids that they need to update ...
            Node n = new Node();
            n["FullTypeName"].Value = typeof(Action).FullName +
                "|" +
                typeof(Action.ActionParams).FullName;

            RaiseEvent(
                "Magix.Core.UpdateGrids",
                n);
        }

        [ActiveEvent(Name = "Magix.Meta.CreateParameter")]
        protected void Magix_Meta_CreateParameter(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                Action a = Action.SelectByID(e.Params["ID"].Get<int>());

                Node tmp = new Node();

                RaiseEvent(
                    "Magix.Core.GetTreeSelectedID",
                    tmp);

                RaiseEvent(
                    "Magix.Core.ExpandTreeSelectedID");

                Action.ActionParams p = new Action.ActionParams();
                p.Value = "Default, please change ...";
                p.Name = "Default Name";

                if (!tmp.Contains("ID"))
                {
                    a.Params.Add(p);
                    a.Save();
                }
                else
                {
                    Action.ActionParams p2 = Action.ActionParams.SelectByID(tmp["ID"].Get<int>());
                    p2.Children.Add(p);
                    p2.Save();
                }

                tr.Commit();

                Node node = new Node();

                // Signalizing to Grids that they need to update ...
                Node n = new Node();
                n["FullTypeName"].Value = typeof(Action).FullName +
                    "|" +
                    typeof(Action.ActionParams).FullName;

                RaiseEvent(
                    "Magix.Core.UpdateGrids",
                    n);
            }
        }

        private void EditActionItem(Action a, Node node)
        {
            // First filtering OUT columns ...!
            node["WhiteListColumns"]["Name"].Value = true;
            node["WhiteListColumns"]["EventName"].Value = true;
            node["WhiteListColumns"]["StripInput"].Value = true;
            node["WhiteListColumns"]["Description"].Value = true;

            node["WhiteListProperties"]["Name"].Value = true;
            node["WhiteListProperties"]["Value"].Value = true;
            node["WhiteListProperties"]["Value"]["ForcedWidth"].Value = 10;

            node["Type"]["Properties"]["Name"]["ReadOnly"].Value = false;
            node["Type"]["Properties"]["EventName"]["ReadOnly"].Value = false;
            node["Type"]["Properties"]["EventName"]["Header"].Value = "Event Name";
            node["Type"]["Properties"]["EventName"]["Bold"].Value = true;
            node["Type"]["Properties"]["StripInput"]["ReadOnly"].Value = false;
            node["Type"]["Properties"]["StripInput"]["Header"].Value = "Strip Input Node";
            node["Type"]["Properties"]["StripInput"]["TemplateColumnEvent"].Value = "Magix.MetaAction.GetStripInputTemplateColumn";
            node["Type"]["Properties"]["Description"]["ReadOnly"].Value = false;

            node["Width"].Value = 18;
            node["Last"].Value = true;
            node["Container"].Value = "content5";
            node["MarginBottom"].Value = 20;

            RaiseEvent(
                "DBAdmin.Form.ViewComplexObject",
                node);
        }

        private void EditActionItemParams(Action a, Node e)
        {
            Node node = new Node();

            node["CssClass"].Value = "clear-left";
            node["Width"].Value = 6;
            node["MarginBottom"].Value = 10;
            node["Top"].Value = 1;
            node["FullTypeName"].Value = typeof(Action.ActionParams).FullName;
            node["ActionItemID"].Value = a.ID;
            node["ItemSelectedEvent"].Value = "Magix.Meta.EditParam";
            node["NoClose"].Value = true;

            RaiseEvent(
                "Magix.Meta.GetActionItemTree",
                node);

            LoadModule(
                "Magix.Brix.Components.ActiveModules.CommonModules.Tree",
                "content4",
                node);
        }

        [ActiveEvent(Name = "Magix.MetaAction.GetStripInputTemplateColumn")]
        protected void Magix_MetaAction_GetStripInputTemplateColumn(object sender, ActiveEventArgs e)
        {
            // Extracting necessary variables ...
            string name = e.Params["Name"].Get<string>();
            string fullTypeName = e.Params["FullTypeName"].Get<string>();
            int id = e.Params["ID"].Get<int>();
            bool value = bool.Parse(e.Params["Value"].Get<string>());

            // Fetching specific user
            Action a = Action.SelectByID(id);

            Panel p = new Panel();

            // Creating our SelectList
            CheckBox ch = new CheckBox();
            ch.Checked = a.StripInput;
            ch.Style[Styles.floating] = "left";
            ch.CheckedChanged +=
                delegate
                {
                    using (Transaction tr = Adapter.Instance.BeginTransaction())
                    {
                        Action a2 = Action.SelectByID(id);
                        a2.StripInput = ch.Checked;
                        a2.Save();

                        tr.Commit();
                    }
                };

            p.Controls.Add(ch);

            Label lbl = new Label();
            lbl.Text = "&nbsp;";
            lbl.Style[Styles.display] = "block";
            lbl.Style[Styles.floating] = "left";
            lbl.Style[Styles.width] = "375px";
            lbl.Tag = "label";
            lbl.Load +=
                delegate
                {
                    lbl.For = ch.ClientID;
                };
            p.Controls.Add(lbl);

            // Stuffing our newly created control into the return parameters, so
            // our Grid control can put it where it feels for it ... :)
            e.Params["Control"].Value = p;
        }

        [ActiveEvent(Name = "Magix.MetaAction.DeleteMetaAction")]
        protected void Magix_MetaAction_DeleteMetaAction(object sender, ActiveEventArgs e)
        {
            int id = e.Params["ID"].Get<int>();
            string fullTypeName = e.Params["FullTypeName"].Get<string>();
            string typeName = fullTypeName.Substring(fullTypeName.LastIndexOf(".") + 1);
            Node node = e.Params;
            if (node == null)
            {
                node = new Node();
                node["ForcedSize"]["width"].Value = 550;
                node["WindowCssClass"].Value =
                    "mux-shaded mux-rounded push-5 down-2";
            }
            node["Caption"].Value = @"
Please confirm deletion of " + typeName + " with ID of " + id;
            node["Text"].Value = @"
<p>Are you sure you wish to delete this Action? 
This action might be in use in several forms or other parts of your system. 
Deleting it may break these parts.</p>";
            node["OK"]["ID"].Value = id;
            node["OK"]["FullTypeName"].Value = fullTypeName;
            node["OK"]["Event"].Value = "Magix.MetaAction.DeleteMetaAction-Confirmed";
            node["Cancel"]["Event"].Value = "DBAdmin.Common.ComplexInstanceDeletedNotConfirmed";
            node["Cancel"]["FullTypeName"].Value = fullTypeName;
            node["Width"].Value = 15;

            LoadModule(
                "Magix.Brix.Components.ActiveModules.CommonModules.MessageBox",
                "child",
                node);
        }

        [ActiveEvent(Name = "Magix.MetaAction.DeleteMetaAction-Confirmed")]
        protected void Magix_MetaAction_DeleteMetaAction_Confirmed(object sender, ActiveEventArgs e)
        {
            if (e.Params["FullTypeName"].Get<string>() == typeof(Action).FullName)
            {
                // In case it's the one being edited ...
                Node n = new Node();

                n["Position"].Value = "content4";

                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "ClearControls",
                    n);
                
                RaiseEvent(
                    "DBAdmin.Common.ComplexInstanceDeletedConfirmed",
                    e.Params);
            }
        }

        [ActiveEvent(Name = "Magix.Meta.EditParam")]
        private void Magix_Meta_EditParam(object sender, ActiveEventArgs e)
        {
            Action.ActionParams p = 
                Action.ActionParams.SelectByID(e.Params["SelectedItemID"].Get<int>());

            Node ch = new Node();
            ch["ID"].Value = p.ID;

            // Checks to see if Item is already being edited ...
            RaiseEvent(
                "Magix.Core.CheckIfIDIsBeingSingleEdited",
                ch);

            if (!ch.Contains("Yes"))
            {
                Node node = new Node();

                // First filtering OUT columns ...!
                node["WhiteListColumns"]["Name"].Value = true;
                node["WhiteListColumns"]["Value"].Value = true;
                node["WhiteListColumns"]["TypeName"].Value = true;

                node["WhiteListProperties"]["Name"].Value = true;
                node["WhiteListProperties"]["Name"]["ForcedWidth"].Value = 2;
                node["WhiteListProperties"]["Value"].Value = true;
                node["WhiteListProperties"]["Value"]["ForcedWidth"].Value = 4;

                node["Type"]["Properties"]["Name"]["ReadOnly"].Value = false;
                node["Type"]["Properties"]["Value"]["ReadOnly"].Value = false;
                node["Type"]["Properties"]["TypeName"]["TemplateColumnEvent"].Value = "Magix.Publishing.GetMetaTypeName";

                node["Width"].Value = 18;
                node["Last"].Value = true;
                node["Padding"].Value = 6;
                node["Container"].Value = "content6";
                node["IsList"].Value = false;
                node["CssClass"].Value = "small-editer";
                node["PullTop"].Value = 18;
                node["FullTypeName"].Value = typeof(Action.ActionParams).FullName;
                node["ID"].Value = p.ID;

                Node xx = new Node();
                xx["Container"].Value = "content6";
                RaiseEvent(
                    "Magix.Core.GetNumberOfChildrenOfContainer",
                    xx);

                node["Append"].Value = xx["Count"].Get<int>() < 3;

                RaiseEvent(
                    "DBAdmin.Form.ViewComplexObject",
                    node);

                node = new Node();
                node["Seed"].Value = "delete-button";
                node["Enabled"].Value = true;
                RaiseEvent(
                    "Magix.Core.EnabledClickable",
                    node);
            }

            Node cc = new Node();

            cc["FullTypeName"].Value = typeof(Action.ActionParams).FullName;
            cc["CssClass"].Value = "";
            cc["Replace"].Value = " selected-action";

            RaiseEvent(
                "Magix.Core.ChangeCssClassOfModule",
                cc);

            cc = new Node();

            cc["FullTypeName"].Value = typeof(Action.ActionParams).FullName;
            cc["ID"].Value = p.ID;
            cc["CssClass"].Value = " selected-action";

            RaiseEvent(
                "Magix.Core.ChangeCssClassOfModule",
                cc);
        }

        [ActiveEvent(Name = "Magix.Publishing.GetMetaTypeName")]
        private void Magix_Publishing_GetMetaTypeName(object sender, ActiveEventArgs e)
        {
            Action.ActionParams p = Action.ActionParams.SelectByID(e.Params["ID"].Get<int>());

            SelectList ls = new SelectList();
            ls.CssClass = "gridSelect";

            ls.SelectedIndexChanged +=
                delegate
                {
                    using (Transaction tr = Adapter.Instance.BeginTransaction())
                    {
                        p.TypeName = ls.SelectedItem.Value;

                        p.Save();

                        tr.Commit();
                    }
                };


            ls.Items.Add(new ListItem("String", "System.String"));
            ls.Items.Add(new ListItem("Integer", "System.Int32"));
            ls.Items.Add(new ListItem("Decimal", "System.Decimal"));
            ls.Items.Add(new ListItem("DateTime", "System.DateTime"));
            ls.Items.Add(new ListItem("Bool", "System.Boolean"));

            switch (p.TypeName)
            {
                case "System.String":
                    ls.SelectedIndex = 0;
                    break;
                case "System.Int32":
                    ls.SelectedIndex = 1;
                    break;
                case "System.Decimal":
                    ls.SelectedIndex = 2;
                    break;
                case "System.DateTime":
                    ls.SelectedIndex = 3;
                    break;
                case "System.Boolean":
                    ls.SelectedIndex = 4;
                    break;
                default:
                    ls.Enabled = false;
                    ls.ToolTip = "Some Custom type I cannot infer ...";
                    break;
            }

            // Stuffing our newly created control into the return parameters, so
            // our Grid control can put it where it feels for it ... :)
            e.Params["Control"].Value = ls;
        }

        [ActiveEvent(Name = "Magix.Meta.GetActionItemTree")]
        private void Magix_Meta_GetActionItemTree(object sender, ActiveEventArgs e)
        {
            Action a = Action.SelectByID(e.Params["ActionItemID"].Get<int>());
            foreach (Action.ActionParams idx in a.Params)
            {
                AddParamToNode(idx, e.Params);
            }
        }

        private void AddParamToNode(Action.ActionParams par, Node node)
        {
            node["Items"]["i-" + par.ID]["ID"].Value = par.ID;
            node["Items"]["i-" + par.ID]["Name"].Value = par.Name;
            node["Items"]["i-" + par.ID]["ToolTip"].Value = par.Value;

            foreach (Action.ActionParams idx in par.Children)
            {
                AddParamToNode(idx, node["Items"]["i-" + par.ID]);
            }
        }

        [ActiveEvent(Name = "Magix.Publishing.GetDataForAdministratorDashboard")]
        protected void Magix_Publishing_GetDataForAdministratorDashboard(object sender, ActiveEventArgs e)
        {
            e.Params["WhiteListColumns"]["MetaActionCount"].Value = true;
            e.Params["Type"]["Properties"]["MetaActionCount"]["ReadOnly"].Value = true;
            e.Params["Type"]["Properties"]["MetaActionCount"]["Header"].Value = "Actions";
            e.Params["Type"]["Properties"]["MetaActionCount"]["ClickLabelEvent"].Value = "Magix.MetaType.ViewActions";
            e.Params["Object"]["Properties"]["MetaActionCount"].Value = Action.Count.ToString();
        }

        [ActiveEvent(Name = "Magix.Core.EventClickedWhileDebugging")]
        protected void Magix_Core_EventClickedWhileDebugging(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                string eventName = e.Params["EventName"].Get<string>();
                Node eventNode = new Node();

                if(e.Params.Contains("EventNode"))
                    eventNode = e.Params["EventNode"].Value as Node;
                
                Action a = new Action();
                a.EventName = eventName;
                a.Name = "Debug-Copy-" + eventName;

                FillActionParams(eventNode, a.Params);
                
                
                a.Save();

                tr.Commit();

                
                Node node = new Node();
                node["ID"].Value = a.ID;

                RaiseEvent("Magix.MetaType.ViewActions");

                RaiseEvent(
                    "Magix.Meta.EditAction",
                    node);

                node = new Node();
                node["ID"].Value = a.ID;
                node["FullTypeName"].Value = typeof(Action).FullName;

                RaiseEvent(
                    "DBAdmin.Grid.SetActiveRow",
                    node);
            }
        }

        private void FillActionParams(Node eventNode, LazyList<Action.ActionParams> lazyList)
        {
            foreach (Node idx in eventNode)
            {
                Action.ActionParams a = new Action.ActionParams();
                a.Name = idx.Name;
                switch ((idx.Value == null ? "" : idx.Value.GetType().FullName))
                {
                    case "System.Int32":
                        a.TypeName = idx.Value.GetType().FullName;
                        a.Value = idx.Value.ToString();
                        break;
                    case "System.DateTime":
                        a.TypeName = idx.Value.GetType().FullName;
                        a.Value = ((DateTime)idx.Value).ToString("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                        break;
                    case "System.Boolean":
                        a.TypeName = idx.Value.GetType().FullName;
                        a.Value = idx.Value.ToString();
                        break;
                    case "System.Decimal":
                        a.TypeName = idx.Value.GetType().FullName;
                        a.Value = ((decimal)idx.Value).ToString(CultureInfo.InvariantCulture);
                        break;
                    case "System.String":
                        a.TypeName = idx.Value.GetType().FullName;
                        a.Value = idx.Value.ToString();
                        break;
                    default:
                        a.TypeName = "[Anonymous-Coward-Type]";
                        a.Value = (idx.Value ?? "").ToString();
                        break;
                }
                lazyList.Add(a);
                if (eventNode.Contains("Children"))
                    FillActionParams(eventNode["Children"], a.Children);
            }
        }

        [ActiveEvent(Name = "Magix.Meta.RaiseEvent")]
        protected void Magix_Meta_RaiseEvent(object sender, ActiveEventArgs e)
        {
            Action action = null;
            if (e.Params.Contains("ActionName"))
                action = Action.SelectFirst(
                    Criteria.Eq("Name", e.Params["ActionName"].Get<string>()));
            else
                action = Action.SelectByID(e.Params["ActionID"].Get<int>());

            Node node = e.Params;
            if (action.StripInput)
            {
                node = new Node();
            }

            foreach (Action.ActionParams idx in action.Params)
            {
                GetActionParameters(node, idx);
            }

            RaiseEvent(
                action.EventName,
                node);
        }

        private static void GetActionParameters(Node node, Action.ActionParams a)
        {
            switch (a.TypeName)
            {
                case "System.String":
                    node[a.Name].Value = a.Value;
                    break;
                case "System.Int32":
                    node[a.Name].Value = int.Parse(a.Value, CultureInfo.InvariantCulture);
                    break;
                case "System.Decimal":
                    node[a.Name].Value = decimal.Parse(a.Value, CultureInfo.InvariantCulture);
                    break;
                case "System.DateTime":
                    node[a.Name].Value = DateTime.ParseExact(a.Value, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                    break;
                case "System.Boolean":
                    node[a.Name].Value = bool.Parse(a.Value);
                    break;
                default:
                    node[a.Name].Value = a.Value;
                    break;
            }
            foreach (Action.ActionParams idx in a.Children)
            {
                GetActionParameters(node[a.Name], idx);
            }
        }
    }
}





















