﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using System.Web.UI;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using Magix.UX.Widgets.Core;
using Magix.Brix.Publishing.Common;
using Magix.UX.Effects;

namespace Magix.Brix.Components.ActiveModules.MetaView
{
    /**
     * Level2: UI parts for showing a MetaView in 'SingleView Mode'. Basically shows a form, with items
     * dependent upon the look of the view. This is a Publisher Plugin module. This form expects
     * to be given a 'MetaViewName', which will serve as the foundation for raising the
     * 'Magix.MetaView.GetViewData' event, whos default implementation will populate the node
     * structure according to the views content in a Key/Value pair kind of relationship.
     * This will serv as the foundation for the module to know which types of controls it needs to load
     * up [TextBoxes, Buttons etc]
     * 
     * Handles the 'Magix.MetaView.SerializeSingleViewForm' event, which is the foundation for creating
     * new objects upon clicking Save buttons etc.
     * 
     * This is the PublisherPlugin you'd use if you'd like to have the end user being able to 
     * create a new MetaObject
     */
    [ActiveModule]
    [PublisherPlugin]
    public class MetaView_Single : ActiveModule
    {
        protected Panel ctrls;
        protected HiddenField oId;

        bool isFirstLoad;

        public override void InitialLoading(Node node)
        {
            isFirstLoad = true;
            base.InitialLoading(node);

            Load +=
                delegate
                {
                    if (!node.Contains("MetaViewTypeName"))
                    {
                        // Probably in 'production mode' and hence need to get our data ...
                        node["MetaViewName"].Value = ViewName;
                        node["IsFirstLoad"].Value = isFirstLoad;

                        RaiseSafeEvent(
                            "Magix.MetaView.GetViewData",
                            node);
                    }
                };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DataSource != null)
                DataBindCtrls();

            Button b = Selector.SelectFirst<Button>(ctrls);
            if (b != null)
                ctrls.DefaultWidget = b.ID;
        }

        private void DataBindCtrls()
        {
            foreach (Node idx in DataSource["Properties"])
            {
                if (!string.IsNullOrEmpty(idx["Action"].Get<string>()) && 
                    idx["Name"].Get<string>().IndexOf(':') == -1)
                {
                    CreateActionControl(idx);
                }
                else if (idx["ReadOnly"].Get<bool>())
                {
                    CreateReadOnlyControl(idx, true);
                }
                else
                {
                    string name = idx["Name"].Get<string>();
                    if (!name.StartsWith("init-actions:"))
                    {
                        if (name.IndexOf(':') > 0)
                            name = name.Substring(name.LastIndexOf(':') + 1);
                        if (DataSource["Type"]["Properties"][name].Contains("TemplateColumnEvent") &&
                            !string.IsNullOrEmpty(DataSource["Type"]["Properties"][name]["TemplateColumnEvent"].Get<string>()))
                        {
                            string eventName = DataSource["Type"]["Properties"][name]["TemplateColumnEvent"].Get<string>();

                            Node colNode = new Node();
                            colNode["FullTypeName"].Value = DataSource["FullTypeName"].Get<string>();

                            colNode["Name"].Value = name;
                            colNode["Value"].Value = idx.Get<string>();
                            colNode["MetaViewName"].Value = DataSource["MetaViewName"].Get<string>();
                            colNode["ID"].Value = DataSource["ID"].Get<int>();
                            colNode["OriginalWebPartID"].Value = DataSource["OriginalWebPartID"].Value;
                            colNode["IsFirstLoad"].Value = isFirstLoad;

                            RaiseSafeEvent(
                                eventName,
                                colNode);

                            if (colNode.Contains("Control"))
                                ctrls.Controls.Add(colNode["Control"].Get<Control>());
                        }
                        else
                        {
                            CreateReadWriteControl(idx, true);
                        }
                    }
                }
            }
            if (isFirstLoad)
            {
                if (DataSource.Contains("AfterInitializingEvent"))
                {
                    RaiseSafeEvent(
                        DataSource["AfterInitializingEvent"].Get<string>(),
                        DataSource);
                }
            }
        }

        private void CreateActionControl(Node idx)
        {
            Button b = new Button();
            b.Text = idx["Name"].Get<string>();
            b.ID = "b-" + idx["ID"].Get<int>();
            if (idx.Contains("ReadOnly"))
                b.Enabled = !idx["ReadOnly"].Get<bool>();
            b.CssClass = "action-button";
            b.Info = idx["Name"].Get<string>();
            b.ToolTip = idx["Description"].Get<string>();
            b.Click +=
                delegate
                {
                    ExecuteSafely(
                        delegate
                        {
                            Node node = new Node();

                            GetPropertyValues(node["PropertyValues"], false);

                            // TODO: Out-factor into controller
                            foreach (string idxS in idx["Action"].Get<string>().Split('|'))
                            {
                                node["ActionSenderName"].Value = b.Text;
                                node["MetaViewName"].Value = DataSource["MetaViewName"].Value;
                                node["MetaViewTypeName"].Value = DataSource["MetaViewTypeName"].Value;
                                node["ID"].Value = DataSource.Contains("ID") ? DataSource["ID"].Value : (object)0;

                                // Settings Event Specific Features ...
                                node["ActionName"].Value = idxS;
                                node["OriginalWebPartID"].Value = DataSource["OriginalWebPartID"].Value;

                                RaiseEvent(
                                    "Magix.MetaAction.RaiseAction",
                                    node);
                            }
                        }, "Something went wrong while trying to execute Actions associated with Meta View Property");
                };
            ctrls.Controls.Add(b);
        }

        private void GetPropertyValues(Node node, bool flat)
        {
            if (oId.Value != "0" && oId.Value != "-1" && oId.Value != "")
                node["ID"].Value = int.Parse(oId.Value);
            foreach (Control idx in ctrls.Controls)
            {
                BaseWebControl w = idx as BaseWebControl;
                if (w != null)
                {
                    if (w is TextBox)
                    {
                        if (flat)
                        {
                            node[w.Info].Value = (w as TextBox).Text;
                        }
                        else
                        {
                            node[w.Info]["Value"].Value = (w as TextBox).Text;
                            node[w.Info]["Name"].Value = w.Info;
                        }
                    }
                }
            }
        }

        // TODO: Out-factor all of these to the controller ...
        // Make it more similar [hopefully shareable] between the 'MultiView' logic ...!!
        private void CreateReadOnlyControl(Node idx, bool shouldClear)
        {
            Label lbl = new Label();
            lbl.ToolTip = idx["Name"].Get<string>();
            lbl.Info = idx["Name"].Get<string>();
            lbl.Text = idx["Description"].Get<string>();
            lbl.CssClass = "meta-view-form-element meta-view-form-label";

            if (shouldClear)
                lbl.CssClass += " clear-both";

            ctrls.Controls.Add(lbl);
        }

        private void CreateReadWriteControl(Node idx, bool shouldClear)
        {
            TextBox b = new TextBox();
            b.PlaceHolder = idx["Description"].Get<string>();
            b.ToolTip = b.PlaceHolder;
            b.Info = idx["Name"].Get<string>();
            b.CssClass = "meta-view-form-element meta-view-form-textbox";

            if (shouldClear)
                b.CssClass += " clear-both";

            ctrls.Controls.Add(b);
        }

        /**
         * Level2: Will return the Container's ID back to caller [e.g. "content1"] if it's the
         * correct WebPartTemplate Container according to the requested 'PageObjectTemplateID'
         */
        [ActiveEvent(Name = "Magix.MetaView.GetWebPartsContainer")]
        protected void Magix_MetaView_GetWebPartsContainer(object sender, ActiveEventArgs e)
        {
            if (e.Params["OriginalWebPartID"].Get<int>() == DataSource["OriginalWebPartID"].Get<int>())
                e.Params["ID"].Value = this.Parent.ID;
        }

        [ActiveEvent(Name = "Magix.MetaView.LoadObjectIntoForm")]
        protected void Magix_MetaView_LoadObjectIntoForm(object sender, ActiveEventArgs e)
        {
            if (e.Params["MetaViewName"].Get<string>() == DataSource["MetaViewName"].Get<string>())
            {
                foreach (Node idx in e.Params["Properties"])
                {
                    TextBox t = Selector.SelectFirst<TextBox>(ctrls,
                        delegate(Control idxC)
                        {
                            return idxC is BaseWebControl &&
                                (idxC as BaseWebControl).Info == idx.Name;
                        });
                    if (t != null)
                        t.Text = idx.Get<string>();
                }
                oId.Value = e.Params["ID"].Value.ToString();
            }
        }

        /**
         * Level2: Will set Focus to the first TextBox in the form if raised from 'within' the 
         * current MetaView, or explicitly has its 'OriginalWebPartID' overridden to 
         * reflect another WebPart ID on the page
         */
        [ActiveEvent(Name = "Magix.MetaView.SetFocusToFirstTextBox")]
        protected void Magix_MetaView_SetFocusToFirstTextBox(object sender, ActiveEventArgs e)
        {
            if (e.Params["OriginalWebPartID"].Get<int>() == DataSource["OriginalWebPartID"].Get<int>())
            {
                TextBox b = Selector.SelectFirst<TextBox>(ctrls);
                if (b != null)
                {
                    new EffectTimeout(500)
                        .ChainThese(
                            new EffectFocusAndSelect(b))
                        .Render();
                }
            }
        }

        /**
         * Level2: Will serialize the form into a key/value pair back to the caller. Basically the foundation
         * for this control's ability to create MetaObjects. Create an action, encapsulating this event,
         * instantiate it and raise it [somehow] when user is done, by attaching it to e.g. a Save button,
         * and have the form serialized into a brand new MetaObject of the given TypeName
         */
        [ActiveEvent(Name = "Magix.MetaView.SerializeSingleViewForm")]
        protected void Magix_MetaView_SerializeSingleViewForm(object sender, ActiveEventArgs e)
        {
            if (e.Params["OriginalWebPartID"].Get<int>() == DataSource["OriginalWebPartID"].Get<int>())
            {
                GetPropertyValues(e.Params, true);
            }
        }

        /**
         * Level2: Will 'empty' the current form. Useful in combination with Save or Clear button
         */
        [ActiveEvent(Name = "Magix.MetaView.EmptyForm")]
        protected void Magix_Meta_Actions_EmptyForm(object sender, ActiveEventArgs e)
        {
            foreach (Control idx in ctrls.Controls)
            {
                BaseWebControl b = idx as BaseWebControl;
                if (b != null)
                {
                    TextBox t = b as TextBox;
                    if (t != null)
                    {
                        t.Text = "";
                    }
                }
            }
        }

        /**
         * Level2: The name of the MetaView to use as the foundation for this form
         */
        [ModuleSetting(ModuleEditorEventName = "Magix.MetaView.MetaView_Single.GetTemplateColumnSelectView")]
        public string ViewName
        {
            get { return ViewState["ViewName"] as string; }
            set { ViewState["ViewName"] = value; }
        }
    }
}
