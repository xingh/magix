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
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;
using System.Web.UI.HtmlControls;
using System.Web.UI;

namespace Magix.Brix.Components.ActiveModules.DBAdmin
{
    /**
     * Level2: Contains the logic for editing and viewing one single ActiveType object, 
     * with all of its properties. Can become initiated in two different states, one
     * of which is 'edit object reference from another object' which will allow for 
     * changing and removing the reference, the other is plain old 'edit the thing' mode.
     * Supports 'ChildCssClass' and several other properties. Most of the common properties from
     * the Database Enterprise Manager is included. Will raise 'DBAdmin.Form.ChangeObject'
     * if user attempts to change the reference. Will raise 'DBAdmin.Data.RemoveObject'
     * when object reference is removed. Changing the reference or removing it is only
     * enabled if 'IsChange' and/or 'IsRemove' is given as true
     */
    [ActiveModule]
    public class ViewSingleObject : Module, IModule
    {
        protected Panel pnl;
        protected Button change;
        protected Button remove;
        protected Button delete;

        public ViewSingleObject()
        {
            _guid = "single-object";
        }

        public override void InitialLoading(Node node)
        {
            base.InitialLoading(node);

            Load +=
                delegate
                {
                    if (node.Contains("ChildCssClass"))
                    {
                        pnl.CssClass = node["ChildCssClass"].Get<string>();
                    }
                };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DataBindObjects();
        }

        protected void change_Click(object sender, EventArgs e)
        {
            Node node = new Node();

            node["FullTypeName"].Value = DataSource["FullTypeName"].Value;
            node["ParentID"].Value = DataSource["ParentID"].Value;
            node["ParentPropertyName"].Value = DataSource["ParentPropertyName"].Value;
            node["ParentFullTypeName"].Value = DataSource["ParentFullTypeName"].Value;

            RaiseSafeEvent(
                "DBAdmin.Form.ChangeObject",
                node);
        }

        protected void remove_Click(object sender, EventArgs e)
        {
            Node node = new Node();

            node["FullTypeName"].Value = DataSource["FullTypeName"].Value;
            node["ParentID"].Value = DataSource["ParentID"].Value;
            node["ParentPropertyName"].Value = DataSource["ParentPropertyName"].Value;
            node["ParentFullTypeName"].Value = DataSource["ParentFullTypeName"].Value;

            RaiseSafeEvent(
                "DBAdmin.Data.RemoveObject",
                node);

            ActiveEvents.Instance.RaiseClearControls("child");
        }

        /**
         * Level2: Simply here to close window upon deletion of instance object
         */
        [ActiveEvent(Name = "DBAdmin.Data.DeleteObject-HelperClearer")]
        protected void DBAdmin_Data_DeleteObject_HelperClearer(object sender, ActiveEventArgs e)
        {
            if (HelperForSeed == e.Params["Event"]["Seed"].Get<Guid>())
            {
                ActiveEvents.Instance.RaiseClearControls("child");

                RaiseEvent(
                    "DBAdmin.Common.ComplexInstanceDeletedConfirmed",
                    e.Params);

                Node n = new Node();

                n["FullTypeName"].Value = DataSource["FullTypeName"].Value;
                n["ID"].Value = DataSource["Object"]["ID"].Value;

                RaiseEvent(
                    "Magix.Core.UpdateGrids",
                    n);
            }
        }

        private Guid HelperForSeed
        {
            get { return (Guid)ViewState["HelperForSeed"]; }
            set { ViewState["HelperForSeed"] = value; }
        }

        protected void delete_Click(object sender, EventArgs e)
        {
            Node node = new Node();

            node["FullTypeName"].Value = DataSource["FullTypeName"].Value;
            node["ID"].Value = DataSource["Object"]["ID"].Value;
            node["OK"]["Event"].Value = "DBAdmin.Data.DeleteObject-HelperClearer";
            HelperForSeed = Guid.NewGuid();
            node["OK"]["Event"]["Seed"].Value = HelperForSeed;

            RaiseSafeEvent(
                "DBAdmin.Data.DeleteObject",
                node);
        }

        protected void DataBindObjects()
        {
            if (DataSource.Contains("Object") && 
                DataSource["Object"]["ID"].Get<int>() != 0)
            {
                Label tb = new Label();
                tb.Tag = "table";
                tb.CssClass = "mux-grid-objects mux-grid-single-edit";

                // Header rows
                tb.Controls.Add(CreateHeaderRow());

                // Creating property rows
                foreach (Node idxProp in DataSource["Type"]["Properties"])
                {
                    if (!DataSource.Contains("WhiteListColumns") ||
                        (DataSource["WhiteListColumns"].Contains(idxProp.Name) &&
                        DataSource["WhiteListColumns"][idxProp.Name].Get<bool>()))
                    {
                        tb.Controls.Add(CreateRow(DataSource["Object"]["Properties"][idxProp.Name]));
                    }
                }
                pnl.Controls.Add(tb);
            }
            DataBindDone();
        }

        // TODO: Rip its heart out, throw it in the garbage, and REFACTOR ...!!
        protected void DataBindDone()
        {
            // TODO: We REALLY need to get 'control over' our Header module somehow here ...
            change.Visible = DataSource["IsChange"].Get<bool>();
            remove.Visible =
                DataSource["IsRemove"].Get<bool>() &&
                DataSource.Contains("Object");
            delete.Visible = false;
                //DataSource["IsDelete"].Get<bool>() &&
                //DataSource.Contains("Object");

            string parentTypeName = 
                !DataSource.Contains("ParentFullTypeName") ? 
                "" : 
                DataSource["ParentFullTypeName"].Get<string>();

            string caption = "";
            if (DataSource.Contains("Caption"))
            {
                caption = DataSource["Caption"].Get<string>();
            }
            else
            {
                if (DataSource["ParentID"].Get<int>() > 0)
                {
                    parentTypeName =
                        parentTypeName.Substring(
                            parentTypeName.LastIndexOf(".") + 1);

                    if (DataSource.Contains("Object"))
                    {
                        caption = string.Format(
                            "{0}[{1}] of {2}[{3}]/{4}",
                            DataSource["TypeName"].Get<string>(),
                            DataSource["Object"]["ID"].Get<int>(),
                            parentTypeName.Substring(parentTypeName.LastIndexOf(".") + 1),
                            DataSource["ParentID"].Value,
                            DataSource["ParentPropertyName"].Value);
                    }
                }
                else
                {
                    if (DataSource.Contains("Object"))
                    {
                        caption = string.Format(
                            "{0}[{1}]",
                            DataSource["TypeName"].Get<string>(),
                            DataSource["Object"]["ID"].Get<int>());
                    }
                    else
                    {
                        caption = "Viewing an object...";
                    }
                }
            }
            if (Parent.Parent.Parent is Window)
            {
                (Parent.Parent.Parent as Window).Caption = caption;
            }
            else
            {
                //Node node = new Node();

                //node["Caption"].Value = caption;

                //RaiseSafeEvent(
                //    "Magix.Core.SetFormCaption",
                //    node);
            }
        }

        private Label CreateRow(Node node)
        {
            Label row = new Label();
            row.Tag = "tr";

            if (!DataSource.Contains("WhiteListProperties") ||
                (DataSource["WhiteListProperties"].Contains("Name") &&
                DataSource["WhiteListProperties"]["Name"].Get<bool>()))
            {
                if (DataSource["Type"]["Properties"][node.Name].Contains("ClickLabelEvent")
                    && !string.IsNullOrEmpty(DataSource["Type"]["Properties"][node.Name]["ClickLabelEvent"].Get<string>()))
                {
                    string evtName = DataSource["Type"]["Properties"][node.Name]["ClickLabelEvent"].Get<string>();

                    Panel c1 = new Panel();
                    if (DataSource.Contains("WhiteListProperties") &&
                        DataSource["WhiteListProperties"]["Name"].Contains("ForcedWidth"))
                    {
                        c1.CssClass += "span-" +
                            DataSource["WhiteListProperties"]["Name"]["ForcedWidth"].Get<int>();
                    }
                    bool bold = DataSource["Type"]["Properties"][node.Name].Contains("Bold") &&
                        DataSource["Type"]["Properties"][node.Name]["Bold"].Get<bool>();
                    if (bold)
                        c1.Style[Styles.fontWeight] = "bold";
                    c1.Tag = "td";
                    c1.CssClass = "mux-column-name";

                    LinkButton clicker = new LinkButton();
                    if (DataSource["Type"]["Properties"][node.Name].Contains("Header"))
                    {
                        clicker.Text = DataSource["Type"]["Properties"][node.Name]["Header"].Get<string>();
                    }
                    else
                    {
                        clicker.Text = node.Name;
                    }
                    clicker.Click +=
                        delegate
                        {
                            RaiseSafeEvent(
                                evtName,
                                DataSource["Type"]["Properties"][node.Name]["ClickLabelEvent"]);
                        };
                    c1.Controls.Add(clicker);
                    row.Controls.Add(c1);
                }
                else
                {
                    if (DataSource["Type"]["Properties"][node.Name].Contains("TemplateColumnHeaderEvent") &&
                        !string.IsNullOrEmpty(
                            DataSource["Type"]["Properties"][node.Name]["TemplateColumnHeaderEvent"].Get<string>()))
                    {
                        Label c1 = new Label();
                        c1.Tag = "td";
                        c1.Info = node.Name;

                        if (DataSource.Contains("WhiteListProperties") &&
                            DataSource["WhiteListProperties"]["Value"].Contains("ForcedWidth"))
                        {
                            c1.CssClass += "span-" +
                                DataSource["WhiteListProperties"]["Name"]["ForcedWidth"].Get<int>();
                        }

                        string eventName =
                            DataSource["Type"]["Properties"][node.Name]["TemplateColumnHeaderEvent"].Get<string>();

                        Node colNode = new Node();

                        colNode["FullTypeName"].Value = DataSource["FullTypeName"].Get<string>(); ;
                        colNode["Name"].Value = node.Name;
                        colNode["Value"].Value = node.Get<string>();
                        colNode["ID"].Value = DataSource["Object"]["ID"].Get<int>();
                        if (DataSource.Contains("Container"))
                            colNode["Container"].Value = DataSource["Container"].Value;

                        RaiseEvent(
                            eventName,
                            colNode);

                        c1.Controls.Add(colNode["Control"].Get<Control>());
                        row.Controls.Add(c1);
                    }
                    else
                    {
                        Label c1 = new Label();
                        if (DataSource.Contains("WhiteListProperties") &&
                            DataSource["WhiteListProperties"]["Name"].Contains("ForcedWidth"))
                        {
                            c1.CssClass += "span-" +
                                DataSource["WhiteListProperties"]["Name"]["ForcedWidth"].Get<int>();
                        }
                        bool bold = DataSource["Type"]["Properties"][node.Name].Contains("Bold") &&
                            DataSource["Type"]["Properties"][node.Name]["Bold"].Get<bool>();
                        if (bold)
                            c1.Style[Styles.fontWeight] = "bold";
                        c1.Tag = "td";
                        c1.CssClass = "mux-column-name";
                        if (DataSource["Type"]["Properties"][node.Name].Contains("Header"))
                        {
                            c1.Text = DataSource["Type"]["Properties"][node.Name]["Header"].Get<string>();
                        }
                        else
                        {
                            c1.Text = node.Name;
                        }
                        row.Controls.Add(c1);
                    }
                }
            }

            if (!DataSource.Contains("WhiteListProperties") ||
                (DataSource["WhiteListProperties"].Contains("Type") &&
                DataSource["WhiteListProperties"]["Type"].Get<bool>()))
            {
                Label c1 = new Label();
                c1.Tag = "td";
                c1.CssClass = "columnType";
                if (DataSource.Contains("WhiteListProperties") &&
                    DataSource["WhiteListProperties"]["Type"].Contains("ForcedWidth"))
                {
                    c1.CssClass += "span-" +
                        DataSource["WhiteListProperties"]["Type"]["ForcedWidth"].Get<int>();
                }
                c1.Text =
                    DataSource["Type"]["Properties"][node.Name]["TypeName"].Get<string>()
                        .Replace("<", "&lt;").Replace(">", "&gt;");
                row.Controls.Add(c1);
            }

            if (!DataSource.Contains("WhiteListProperties") ||
                (DataSource["WhiteListProperties"].Contains("Attributes") &&
                DataSource["WhiteListProperties"]["Attributes"].Get<bool>()))
            {
                Label c1 = new Label();
                c1.Tag = "td";
                c1.CssClass = "columnType";
                if (DataSource.Contains("WhiteListProperties") &&
                    DataSource["WhiteListProperties"]["Attributes"].Contains("ForcedWidth"))
                {
                    c1.CssClass += "span-" +
                        DataSource["WhiteListProperties"]["Attributes"]["ForcedWidth"].Get<int>();
                }
                string text = "";
                if (!DataSource["Type"]["Properties"][node.Name]["IsOwner"].Get<bool>())
                    text += "IsNotOwner ";
                if (DataSource["Type"]["Properties"][node.Name]["BelongsTo"].Get<bool>())
                    text += "BelongsTo ";
                if (!string.IsNullOrEmpty(DataSource["Type"]["Properties"][node.Name]["RelationName"].Get<string>()))
                    text += "'" + DataSource["Type"]["Properties"][node.Name]["RelationName"].Get<string>() + "'";
                c1.Text = text;
                row.Controls.Add(c1);
            }

            if (!DataSource.Contains("WhiteListProperties") ||
                (DataSource["WhiteListProperties"].Contains("Value") &&
                DataSource["WhiteListProperties"]["Value"].Get<bool>()))
            {
                if (DataSource["Type"]["Properties"][node.Name].Contains("TemplateColumnEvent") &&
                    !string.IsNullOrEmpty(
                        DataSource["Type"]["Properties"][node.Name]["TemplateColumnEvent"].Get<string>()))
                {
                    Label c1 = new Label();
                    c1.Tag = "td";
                    c1.Info = node.Name;

                    if (DataSource.Contains("WhiteListProperties") &&
                        DataSource["WhiteListProperties"]["Value"].Contains("ForcedWidth"))
                    {
                        c1.CssClass += "span-" +
                            DataSource["WhiteListProperties"]["Value"]["ForcedWidth"].Get<int>();
                    }

                    string eventName = 
                        DataSource["Type"]["Properties"][node.Name]["TemplateColumnEvent"].Get<string>();

                    Node colNode = new Node();

                    colNode["FullTypeName"].Value = DataSource["FullTypeName"].Get<string>(); ;
                    colNode["MetaViewName"].Value = DataSource["MetaViewName"].Get<string>(); ;
                    colNode["Name"].Value = node.Name;
                    colNode["Value"].Value = node.Get<string>();
                    colNode["ID"].Value = DataSource["Object"]["ID"].Get<int>();
                    colNode["OriginalWebPartID"].Value = DataSource["OriginalWebPartID"].Value;

                    if (DataSource["Type"]["Properties"][node.Name].Contains("ReadOnly"))
                        colNode["ReadOnly"].Value = DataSource["Type"]["Properties"][node.Name]["ReadOnly"].Value;

                    RaiseEvent(
                        eventName,
                        colNode);

                    c1.Controls.Add(colNode["Control"].Get<Control>());
                    row.Controls.Add(c1);
                }
                else
                {
                    bool bold = DataSource["Type"]["Properties"][node.Name].Contains("Bold") &&
                        DataSource["Type"]["Properties"][node.Name]["Bold"].Get<bool>();
                    Label c1 = new Label();
                    if (bold)
                        c1.Style[Styles.fontWeight] = "bold";
                    if (DataSource.Contains("WhiteListProperties") &&
                        DataSource["WhiteListProperties"]["Value"].Contains("ForcedWidth"))
                    {
                        c1.CssClass += "span-" +
                            DataSource["WhiteListProperties"]["Value"]["ForcedWidth"].Get<int>();
                    }
                    c1.Tag = "td";
                    if (DataSource["Type"]["Properties"][node.Name]["IsComplex"].Get<bool>())
                    {
                        if (DataSource["Type"]["Properties"][node.Name].Contains("ReadOnly") &&
                            DataSource["Type"]["Properties"][node.Name]["ReadOnly"].Get<bool>())
                        {
                            string value = (node.Value ?? "").ToString();
                            if (DataSource["Type"]["Properties"][node.Name].Contains("MaxLength"))
                            {
                                if (value.ToString().Length > DataSource["Type"]["Properties"][node.Name]["MaxLength"].Get<int>())
                                {
                                    c1.Text = value.ToString().Substring(0, DataSource["Type"]["Properties"][node.Name]["MaxLength"].Get<int>()) + " ...";
                                }
                                else
                                    c1.Text = value.ToString();
                            }
                            else
                                c1.Text = value.ToString();
                            if (DataSource.Contains("WhiteListProperties") &&
                                DataSource["WhiteListProperties"]["Value"].Contains("ForcedWidth"))
                            {
                                c1.CssClass += " span-" + 
                                    DataSource["WhiteListProperties"]["Value"]["ForcedWidth"].Get<int>();
                            }
                        }
                        else
                        {
                            LinkButton ed = new LinkButton();
                            string value = (node.Value ?? "").ToString();
                            if (DataSource["Type"]["Properties"][node.Name].Contains("MaxLength"))
                            {
                                if (value.ToString().Length > DataSource["Type"]["Properties"][node.Name]["MaxLength"].Get<int>())
                                {
                                    ed.Text = value.ToString().Substring(0, DataSource["Type"]["Properties"][node.Name]["MaxLength"].Get<int>()) + " ...";
                                }
                                else
                                    ed.Text = value.ToString();
                            }
                            else
                                ed.Text = value.ToString();
                            ed.Info = node.Name;
                            if (DataSource["Type"]["Properties"][node.Name]["BelongsTo"].Get<bool>())
                                ed.CssClass = "mux-grid-belongs-to";
                            ed.Click +=
                                delegate(object sender, EventArgs e)
                                {
                                    LinkButton lb = sender as LinkButton;
                                    Label ctrlOld = Magix.UX.Selector.SelectFirst<Label>(lb.Parent.Parent.Parent,
                                        delegate(Control idxCtrl)
                                        {
                                            BaseWebControl ctrl = idxCtrl as BaseWebControl;
                                            if (ctrl != null)
                                                return ctrl.CssClass == "mux-grid-selected";
                                            return false;
                                        });
                                    if (ctrlOld != null)
                                        ctrlOld.CssClass = "";
                                    (lb.Parent.Parent as Label).CssClass = "mux-grid-selected";
                                    int id = DataSource["Object"]["ID"].Get<int>();
                                    string column = lb.Info;

                                    Node n = new Node();

                                    n["ID"].Value = id;
                                    n["PropertyName"].Value = column;
                                    n["IsList"].Value = DataSource["Type"]["Properties"][column]["IsList"].Value;
                                    n["FullTypeName"].Value = DataSource["FullTypeName"].Value;

                                    RaiseSafeEvent(
                                        "DBAdmin.Form.ViewListOrComplexPropertyValue",
                                        n);
                                };
                            c1.Controls.Add(ed);
                        }
                    }
                    else
                    {
                        if (DataSource["Type"]["Properties"][node.Name].Contains("ReadOnly") &&
                            DataSource["Type"]["Properties"][node.Name]["ReadOnly"].Get<bool>())
                        {
                            string value = (node.Value ?? "").ToString();
                            if (DataSource.Contains("WhiteListProperties") &&
                                DataSource["WhiteListProperties"]["Value"].Contains("ForcedWidth"))
                            {
                                c1.CssClass += " span-" +
                                    DataSource["WhiteListProperties"]["Value"]["ForcedWidth"].Get<int>();
                            }
                            if (DataSource["Type"]["Properties"][node.Name].Contains("MaxLength"))
                            {
                                if (value.ToString().Length > DataSource["Type"]["Properties"][node.Name]["MaxLength"].Get<int>())
                                {
                                    c1.Text = value.ToString().Substring(0, DataSource["Type"]["Properties"][node.Name]["MaxLength"].Get<int>()) + " ...";
                                }
                                else
                                    c1.Text = value.ToString();
                            }
                            else
                                c1.Text = value.ToString();
                        }
                        else
                        {
                            switch (DataSource["Type"]["Properties"][node.Name]["FullTypeName"].Get<string>())
                            {
                                default:
                                    {
                                        string ctrlType = typeof(TextAreaEdit).FullName;

                                        if (DataSource["Type"]["Properties"][node.Name].Contains("ControlType"))
                                        {
                                            ctrlType = DataSource["Type"]["Properties"][node.Name]["ControlType"].Get<string>();
                                        }
                                        switch (ctrlType)
                                        {
                                            case "Magix.Brix.Components.TextAreaEdit":
                                                {
                                                    TextAreaEdit ed = new TextAreaEdit();
                                                    ed.TextLength = 500;
                                                    string value = (node.Value ?? "").ToString();
                                                    if (DataSource["Type"]["Properties"][node.Name].Contains("MaxLength"))
                                                    {
                                                        ed.TextLength = DataSource["Type"]["Properties"][node.Name]["MaxLength"].Get<int>();
                                                    }
                                                    ed.Text = value.ToString();
                                                    ed.CssClass += " mux-larger-edit";
                                                    ed.Info = node.Name;
                                                    ed.TextChanged +=
                                                        delegate(object sender, EventArgs e)
                                                        {
                                                            TextAreaEdit edit = sender as TextAreaEdit;
                                                            int id = DataSource["Object"]["ID"].Get<int>();
                                                            string column = edit.Info;

                                                            Node n = new Node();

                                                            n["ID"].Value = id;
                                                            n["PropertyName"].Value = column;
                                                            n["NewValue"].Value = edit.Text;
                                                            n["FullTypeName"].Value = DataSource["FullTypeName"].Value;

                                                            n["_guid"].Value = _guid;

                                                            RaiseSafeEvent(
                                                                DataSource.Contains("ChangeSimplePropertyValue") ?
                                                                    DataSource["ChangeSimplePropertyValue"].Get<string>() :
                                                                    "DBAdmin.Data.ChangeSimplePropertyValue",
                                                                n);
                                                        };
                                                    c1.Controls.Add(ed);
                                                } break;
                                            case "Magix.UX.Widgets.InPlaceEdit":
                                                {
                                                    InPlaceEdit ed = new InPlaceEdit();
                                                    string value = (node.Value ?? "").ToString();
                                                    if (DataSource["Type"]["Properties"][node.Name].Contains("MaxLength"))
                                                    {
                                                        if (value.ToString().Length > DataSource["Type"]["Properties"][node.Name]["MaxLength"].Get<int>())
                                                        {
                                                            ed.Text = value.ToString().Substring(0, DataSource["Type"]["Properties"][node.Name]["MaxLength"].Get<int>()) + " ...";
                                                        }
                                                        else
                                                            ed.Text = value.ToString();
                                                    }
                                                    else
                                                        ed.Text = value.ToString();
                                                    ed.CssClass += " mux-larger-edit";
                                                    ed.Info = node.Name;
                                                    ed.TextChanged +=
                                                        delegate(object sender, EventArgs e)
                                                        {
                                                            InPlaceEdit edit = sender as InPlaceEdit;
                                                            int id = DataSource["Object"]["ID"].Get<int>();
                                                            string column = edit.Info;

                                                            Node n = new Node();

                                                            n["ID"].Value = id;
                                                            n["PropertyName"].Value = column;
                                                            n["NewValue"].Value = edit.Text;
                                                            n["FullTypeName"].Value = DataSource["FullTypeName"].Value;
                                                            
                                                            n["_guid"].Value = _guid;

                                                            RaiseSafeEvent(
                                                                DataSource.Contains("ChangeSimplePropertyValue") ?
                                                                    DataSource["ChangeSimplePropertyValue"].Get<string>() :
                                                                    "DBAdmin.Data.ChangeSimplePropertyValue",
                                                                n);
                                                        };
                                                    c1.Controls.Add(ed);
                                                } break;
                                        }

                                    } break;
                            }
                        }
                    }
                    row.Controls.Add(c1);
                }
            }
            return row;
        }

        // TODO: Refactor ...!
        private HtmlTableRow CreateHeaderRow()
        {
            HtmlTableRow row = new HtmlTableRow();
            row.Attributes.Add("class", "mux-grid-header");

            if (!DataSource.Contains("WhiteListProperties") ||
                (DataSource["WhiteListProperties"].Contains("Name") &&
                DataSource["WhiteListProperties"]["Name"].Get<bool>()))
            {
                HtmlTableCell c1 = new HtmlTableCell();
                if (DataSource.Contains("WhiteListProperties") &&
                    DataSource["WhiteListProperties"].Contains("Name") &&
                    DataSource["WhiteListProperties"]["Name"].Contains("Header"))
                {
                    c1.InnerHtml = 
                        DataSource["WhiteListProperties"]["Name"]["Header"].Get<string>();
                }
                else
                {
                    c1.InnerHtml = "Name";
                }
                if (DataSource.Contains("WhiteListProperties") &&
                    DataSource["WhiteListProperties"]["Name"].Contains("ForcedWidth"))
                    c1.Attributes.Add(
                        "class",
                        "span-" +
                        DataSource["WhiteListProperties"]["Name"]["ForcedWidth"].Get<int>());
                else
                    c1.Attributes.Add("class", "span-4");
                row.Cells.Add(c1);
            }

            if (!DataSource.Contains("WhiteListProperties") ||
                (DataSource["WhiteListProperties"].Contains("Type") &&
                DataSource["WhiteListProperties"]["Type"].Get<bool>()))
            {
                HtmlTableCell c1 = new HtmlTableCell();
                c1.InnerHtml = "Type";
                if (DataSource.Contains("WhiteListProperties") &&
                    DataSource["WhiteListProperties"]["Type"].Contains("ForcedWidth"))
                    c1.Attributes.Add(
                        "class",
                        "span-" +
                        DataSource["WhiteListProperties"]["Type"]["ForcedWidth"].Get<int>());
                else
                    c1.Attributes.Add("class", "span-4");
                row.Cells.Add(c1);
            }

            if (!DataSource.Contains("WhiteListProperties") ||
                (DataSource["WhiteListProperties"].Contains("Attributes") &&
                DataSource["WhiteListProperties"]["Attributes"].Get<bool>()))
            {
                HtmlTableCell c1 = new HtmlTableCell();
                c1.InnerHtml = "Attributes";
                if (DataSource.Contains("WhiteListProperties") &&
                    DataSource["WhiteListProperties"]["Attributes"].Contains("ForcedWidth"))
                    c1.Attributes.Add(
                        "class",
                        "span-" +
                        DataSource["WhiteListProperties"]["Attributes"]["ForcedWidth"].Get<int>());
                else
                    c1.Attributes.Add("class", "span-5");
                row.Cells.Add(c1);
            }

            if (!DataSource.Contains("WhiteListProperties") ||
                (DataSource["WhiteListProperties"].Contains("Value") &&
                DataSource["WhiteListProperties"]["Value"].Get<bool>()))
            {
                HtmlTableCell c1 = new HtmlTableCell();
                if (DataSource.Contains("WhiteListProperties") &&
                    DataSource["WhiteListProperties"].Contains("Value") &&
                    DataSource["WhiteListProperties"]["Value"].Contains("Header"))
                {
                    c1.InnerHtml =
                        DataSource["WhiteListProperties"]["Value"]["Header"].Get<string>();
                }
                else
                {
                    c1.InnerHtml = "Value";
                }
                if (DataSource.Contains("WhiteListProperties") && 
                    DataSource["WhiteListProperties"]["Value"].Contains("ForcedWidth"))
                    c1.Attributes.Add(
                        "class", 
                        "span-" + 
                        DataSource["WhiteListProperties"]["Value"]["ForcedWidth"].Get<int>());
                else
                    c1.Attributes.Add("class", "span-7");
                row.Cells.Add(c1);
            }
            return row;
        }

        /**
         * Level2: Will return 'Yes' == true if the given 'ID' matches the object being edited
         */
        [ActiveEvent(Name = "DBAdmin.Form.CheckIfActiveTypeIsBeingSingleEdited")]
        protected void DBAdmin_Form_CheckIfActiveTypeIsBeingSingleEdited(object sender, ActiveEventArgs e)
        {
            if (e.Params["ID"].Get<int>() == DataSource["ID"].Get<int>())
                e.Params["Yes"].Value = true;
        }

        /**
         * Level2: Will change the CSS class of the editing parts of the module if the 'FullTypeName'
         * and the 'ID' matches. Useful for setting CSS class of specific 'Edit Object Module'
         */
        [ActiveEvent(Name = "DBAdmin.Form.ChangeCssClassOfModule")]
        protected void DBAdmin_Form_ChangeCssClassOfModule(object sender, ActiveEventArgs e)
        {
            if (e.Params["FullTypeName"].Get<string>() == 
                DataSource["FullTypeName"].Get<string>())
            {
                if (!e.Params.Contains("ID") || 
                    e.Params["ID"].Get<int>() == DataSource["ID"].Get<int>())
                {
                    if (e.Params.Contains("Replace"))
                    {
                        pnl.CssClass =
                            pnl.CssClass.Replace(
                                e.Params["Replace"].Get<string>(),
                                e.Params["CssClass"].Get<string>());
                    }
                    else
                    {
                        pnl.CssClass += e.Params["CssClass"].Get<string>();
                    }
                }
            }
        }

        protected override void ReDataBind()
        {
            if (DataSource.Contains("DoNotRebind") &&
                DataSource["DoNotRebind"].Get<bool>())
                return;

            if (DataSource.Contains("ParentID") && 
                DataSource["ParentID"].Get<int>() > 0)
            {
                DataSource["Object"].UnTie();
                
                if (RaiseSafeEvent(
                    "DBAdmin.Data.GetObjectFromParentProperty",
                    DataSource))
                {
                    pnl.Controls.Clear();
                    DataBindObjects();
                    pnl.ReRender();
                }
            }
            else
            {
                if (!DataSource.Contains("Object"))
                    return;

                DataSource["ID"].Value = DataSource["Object"]["ID"].Get<int>();
                DataSource["Object"].UnTie();
                
                if (RaiseSafeEvent(
                    DataSource.Contains("GetObjectEvent") ?
                        DataSource["GetObjectEvent"].Get<string>() :
                        "DBAdmin.Data.GetObject",
                    DataSource))
                {
                    pnl.Controls.Clear();
                    DataBindObjects();
                    pnl.ReRender();
                }

                // Checking to see if our object has 'vanished' ...
                if (!DataSource.Contains("Object"))
                {
                    // We are looking at one object, with no parent 'select logic' included
                    // This one object is NOT EXISTING
                    // Hence we can safely close this particular window ...
                    ActiveEvents.Instance.RaiseClearControls(Parent.ID);
                    return;
                }
            }
            FlashPanel(pnl);
        }
    }
}
