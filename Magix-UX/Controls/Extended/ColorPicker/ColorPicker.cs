/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;
using System.Drawing;


namespace Magix.UX.Widgets
{
    /**
     * ColorPicker widget for allowing the End User to select a Color from a Color Wheel kind of
     * control. Use the ColorWheelImage to override which colors the user is allowed to pick from,
     * if you'd like to restrict him to some sort of palette
     */
    public class ColorPicker : BaseWebControl
    {
        public ColorPicker()
        {
            CssClass = "mux-color-picker";
        }

        /**
         * Sets which Image to use s the foundation for selecting a Color. By
         * chaing this value, you can for instance [kind of] restrict the user 
         * in regards to which colors he is allowed to pick from
         */
        public string ColorWheelImage
        {
            get { return ViewState["ColorWheelImage"] == null ? 
                "media/images/spectrum.png" : 
                (string)ViewState["ColorWheelImage"]; }
            set
            {
                if (value != ColorWheelImage)
                    SetJsonValue("ColorWheelImage", value);
                ViewState["ColorWheelImage"] = value;
            }
        }

        /**
         * The selected value of the control
         */
        public Color Value
        {
            get
            {
                if (!HasValue())
                    return Color.Empty;

                string clr = Page.Request.Params[ClientID + "__value"];
                return Color.FromArgb(
                    255, 
                    int.Parse(clr.Split(',')[0].Split('(')[1]), 
                    int.Parse(clr.Split(',')[1]),
                    int.Parse(clr.Split(',')[2].Split(')')[0]));
            }
            set
            {
                ViewState["Value"] = value;
            }
        }

        /**
         * True if the Widget has any value
         */
        public bool HasValue()
        {
            return !string.IsNullOrEmpty(Page.Request.Params[ClientID + "__value"]);
        }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement("div"))
            {
                AddAttributes(el);
                using (Element ca = builder.CreateElement("canvas"))
                {
                    ca.AddAttribute("id", ClientID + "__cnv");
                }
                using (Element ca = builder.CreateElement("div"))
                {
                    ca.AddAttribute("id", ClientID + "__prev");
                    ca.AddAttribute("class", "mux-color-picker-preview");
                }
                using (Element ca = builder.CreateElement("div"))
                {
                    ca.AddAttribute("id", ClientID + "__selected");
                    ca.AddAttribute("class", "mux-color-picker-preview mux-color-picker-selected");
                    if (ViewState["Value"] != null)
                    {
                        Color c = (Color)ViewState["Value"];
                        ca.AddAttribute("style", "background-color:" + System.Drawing.ColorTranslator.ToHtml(c).ToLowerInvariant() + ";");
                        ca.Write("R:{0}<br/>G:{1}<br/>B:{2}<br/>",
                            c.R,
                            c.G,
                            c.B);
                    }
                }
                using (Element ca = builder.CreateElement("input"))
                {
                    ca.AddAttribute("type", "text");
                    ca.AddAttribute("id", ClientID + "__value");
                    ca.AddAttribute("name", ClientID + "__value");
                    ca.AddAttribute("class", "mux-color-value");
                    if (ViewState["Value"] != null)
                    {
                        Color c = (Color)ViewState["Value"];
                        ca.AddAttribute("value", "rgb(" +
                            c.R +
                            "," +
                            c.G +
                            "," +
                            c.B +
                            ")");
                    }
                }
                using (Element ca = builder.CreateElement("input"))
                {
                    ca.AddAttribute("type", "text");
                    ca.AddAttribute("id", ClientID + "__valueHex");
                    ca.AddAttribute("class", "mux-color-value");
                    if (ViewState["Value"] != null)
                    {
                        Color c = (Color)ViewState["Value"];
                        ca.AddAttribute("value", System.Drawing.ColorTranslator.ToHtml(c).ToLowerInvariant());
                    }
                }
                RenderChildren(builder.Writer);
            }
        }

        protected override void AddAttributes(Element el)
        {
            base.AddAttributes(el);
        }

        protected override void OnPreRender(EventArgs e)
        {
            AjaxManager.Instance.IncludeScriptFromResource(
                typeof(Timer),
                "Magix.UX.Js.ColorPicker.js");
            base.OnPreRender(e);
        }

        protected override string GetClientSideScriptType()
        {
            return "new MUX.ColorPicker";
        }
    }
}
