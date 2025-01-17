﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using Magix.UX.Widgets;
using Magix.UX;
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;

namespace Magix.Brix.Components.ActiveModules.TalkBack
{
    /**
     * Level2: Basically a 'Forum module' which allows for posting and reading and replying to other people's
     * opinions about 'whatever'. Not very good on its own, please use through TalkBack Controller
     * to save head aches
     */
    [ActiveModule]
    public class Forum : ActiveModule
    {
        protected Panel wrp;
        protected System.Web.UI.WebControls.Repeater rep;
        protected TextBox header;
        protected TextArea body;

        public override void InitialLoading(Node node)
        {
            base.InitialLoading(node);

            Load +=
                delegate
                {
                    if (node.Contains("Active"))
                        Current = node["Active"].Get<int>();

                    DataBindRepeater();
                };
        }

        private void DataBindRepeater()
        {
            rep.DataSource = DataSource["Posts"];
            rep.DataBind();

            foreach (Label idx in Selector.Select<Label>(rep,
                delegate(System.Web.UI.Control idxP)
                {
                    return (idxP is Label) && (idxP as Label).CssClass == "header";
                }))
            {
                Panel p = Selector.SelectFirst<Panel>(idx.Parent,
                    delegate(System.Web.UI.Control idxN)
                    {
                        return (idxN is BaseWebControl) &&
                            (idxN as BaseWebControl).CssClass.Contains("one-item-content");
                    });
                idx.ClickEffect = new EffectToggle(p, 250, false);
            }
        }

        protected string GetShorter(object strObj)
        {
            string retVal = strObj as string;
            if (retVal != null)
            {
                if (retVal.Length > 35)
                    retVal = retVal.Substring(0, 35) + "...";
            }
            return retVal;
        }

        protected string GetVisiblePanel(object strID)
        {
            int id = (int)strID;
            if (id == Current)
                return "";
            return "display:none;";
        }

        private int Current
        {
            get { return ViewState["Current"] == null ? -1 : (int)ViewState["Current"]; }
            set { ViewState["Current"] = value; }
        }

        protected void submit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(header.Text.Trim()) ||
                string.IsNullOrEmpty(body.Text.Trim()))
            {
                Node n = new Node();
                n["Message"].Value = 
                    @"You need to supply at least some characters in both the 
header and the body field ...";

                RaiseEvent(
                    "Magix.Core.ShowMessage",
                    n);
            }
            else
            {
                Node node = new Node();
                node["Header"].Value = header.Text;
                node["Body"].Value = body.Text;

                RaiseSafeEvent(
                    "Magix.Talkback.CreatePost",
                    node);

                DataSource["Posts"].UnTie();

                RaiseSafeEvent(
                    "Magix.Talkback.GetPostings",
                    DataSource);

                Current = DataSource["Posts"][0]["ID"].Get<int>();

                DataBindRepeater();

                wrp.ReRender();

                new EffectHighlight(wrp, 500)
                    .Render();

                header.Text = "";
                body.Text = "";
            }
        }

        protected void reply_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            Node node = new Node();

            node["Header"].Value = Selector.SelectFirst<TextBox>(button.Parent).Text;
            node["Body"].Value = Selector.SelectFirst<TextArea>(button.Parent).Text;
            node["Parent"].Value = int.Parse(button.Info);

            if (RaiseSafeEvent(
                "Magix.Talkback.CreatePost",
                node))
            {
                DataSource["Posts"].UnTie();

                RaiseSafeEvent(
                    "Magix.Talkback.GetPostings",
                    DataSource);

                Current = DataSource["Posts"][0]["ID"].Get<int>();

                DataBindRepeater();

                wrp.ReRender();

                new EffectHighlight(wrp, 500)
                    .Render();

                header.Text = "";
                body.Text = "";
            }
        }
    }
}
