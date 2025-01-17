/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX.Widgets;
using Magix.UX.Aspects;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    /**
     * Window control. Basically an "advanced panel" with support for moving. 
     * It also features borders which can be skinned. The equivalent of a 
     * normal desktop window.
     */
    public class Window : CompositeControl
    {
        readonly Label _caption = new Label();
        readonly LinkButton _close = new LinkButton();
        readonly AspectDraggable _dragger = new AspectDraggable();

        public Window()
        {
            CssClass = "mux-window";
        }

        /**
         * Raised when window is being closed.
         */
        public event EventHandler Closed;

        /**
         * Raised when window is moved in browser.
         */
        public event EventHandler Dragged;

        /**
         * This is the caption of the header parts of the Window.
         */
        public string Caption
        {
            get { return _caption.Text; }
            set { _caption.Text = value; }
        }

        /**
         * If this property is true, then the window can be closed by clicking a closing icon.
         * The default value is true.
         */
        public bool Closable
        {
            get { return _close.Visible; }
            set { _close.Visible = value; }
        }

        /**
         * If this property is true, then the window can be moved around on the viewport.
         * The default value is true.
         */
        public bool Draggable
        {
            get { return _dragger.Enabled; }
            set
            {
                _dragger.Enabled = value;
            }
        }

        /**
         * Returns the AspectDraggable. Be VERY careful with this one, useful for 
         * setting boundaries and such, but shouldn't very often need directly 
         * editing.
         */
        public AspectDraggable Dragger
        {
            get { return _dragger; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            CreateWindowControls();
            base.CreateChildControls();
        }

        private void CreateWindowControls()
        {
            Content.CssClass = "mux-window-content";

            _caption.ID = "capt";
            _caption.CssClass = "mux-window-caption";
            Controls.Add(_caption);

            _close.ID = "close";
            _close.CssClass = "mux-window-close";
            _close.Text = "&nbsp;";
            _close.ToolTip = "Click to close";
            _close.Click += CloseClick;
            Controls.Add(_close);

            _dragger.ID = "dragger";
            if (Dragged != null)
                _dragger.Dragged += DraggerDropped;
            _dragger.Handle = _caption.ClientID;
            Controls.Add(_dragger);
        }

        private void DraggerDropped(object sender, EventArgs e)
        {
            if (Dragged != null)
                Dragged(this, new EventArgs());
        }

        public void CloseWindow()
        {
            Visible = false;
            if (Closed != null)
                Closed(this, new EventArgs());
        }

        private void CloseClick(object sender, EventArgs e)
        {
            Visible = false;
            if (Closed != null)
                Closed(this, new EventArgs());
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (Draggable)
            {
                _caption.Style[Styles.cursor] = "move";
            }
            else
            {
                _caption.Style[Styles.cursor] = "default";
            }
            base.OnPreRender(e);
        }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement(Tag))
            {
                AddAttributes(el);
                RenderChildren(builder.Writer);
            }
        }
    }
}
