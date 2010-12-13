/*
 * MagicUX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * MagicUX is licensed as GPLv3.
 */

using System.Web.UI;
using System.ComponentModel;
using Magic.UX.Builder;
using Magic.UX.Widgets.Core;

namespace Magic.UX.Widgets
{
    /**
     * A container widget for displaying other widgets inside of it. Will render as a
     * &lt;div by default, but the specific tag this widget will render, can easily be
     * overridden by changing the Tag property. You can choose to render your panels as
     * paragraphs (&lt;p...) for instance.
     * If you only need to display text on your page, and you need to use WebControls for
     * this, you should use the Label control and not the Panel control.
     */
    public class Panel : BaseWebControl, INamingContainer
    {
        /**
         * The HTML tag element type used to render your widget. You can set this property
         * to anything you wish, including 'address', 'p' or any other types of HTML tags
         * you wish to use to render your widget. If you need an inline-element,
         * such as a span or something, or you need to render a widget without child widgets, 
         * you should rather use the Label widget.
         */
        virtual public string Tag
        {
            get { return ViewState["Tag"] == null ? "div" : (string)ViewState["Tag"]; }
            set { ViewState["Tag"] = value; }
        }

        /**
         * The default widget which will be mimicked a 'click' when this widget
         * has focus on the user clicks carriage return (Enter/Return key) on
         * his keyboard. Useful for information typing where you don't want to
         * force the user of clicking a specific 'save' button.
         * If you use another widget as the default widget, you must remember to
         * use the ClientID property of that widget as the DefaultWidget value of 
         * this property.
         */
        public string DefaultWidget
        {
            get { return ViewState["DefaultWidget"] == null ? null : (string)ViewState["DefaultWidget"]; }
            set { ViewState["DefaultWidget"] = value; }
        }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement(Tag))
            {
                AddAttributes(el);
                RenderChildren(builder.Writer as System.Web.UI.HtmlTextWriter);
            }
        }

        protected override string GetClientSideScriptOptions()
        {
            string retVal = base.GetClientSideScriptOptions();
            if (!string.IsNullOrEmpty(DefaultWidget))
            {
                if (!string.IsNullOrEmpty(retVal))
                    retVal += ",";
                retVal += 
                    string.Format(
                        "defaultWidget:'{0}'", 
                        Selector.FindControl<Control>(this, DefaultWidget).ClientID);
            }
            return retVal;
        }
	}
}
