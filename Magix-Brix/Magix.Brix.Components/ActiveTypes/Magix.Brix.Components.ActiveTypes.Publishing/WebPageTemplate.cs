﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Magix.Brix.Data;
using Magix.Brix.Types;

namespace Magix.Brix.Components.ActiveTypes.Publishing
{
    /**
     * Serves as a 'recipe' for a WebPage. Every WebPage is built from one of these.
     * Contains the definition of which module to instantiate for instance, but not which
     * parameters to send into it. Also contains the positioning of the WebParts and other
     * common features, such as CSS classes and such. A Template is just that, a 'template'
     * for your WebPages
     */
    [ActiveType]
    public class WebPageTemplate : ActiveType<WebPageTemplate>
    {
        public WebPageTemplate()
        {
            Containers = new LazyList<WebPartTemplate>();
        }

        /**
         * Friendly name of template. Serves no logical meaning. It is probably smart
         * though to create a coding convention of some sort here
         */
        [ActiveField]
        public string Name { get; set; }

        /**
         * Our 'WebPart-recipes'. Contains the recipe for every webpart that
         * should be added to our page. The number of WebPartTemplates, and their settings,
         * will define how the WebPage's WebParts will look like. This property 'controls'
         * how the WebParts property of the associated WebPage is being built
         */
        [ActiveField]
        public LazyList<WebPartTemplate> Containers { get; set; }

        /**
         * Automatically kept track of. Keeps the 'created date' of the object
         */
        [ActiveField]
        public DateTime Created { get; private set; }

        public WebPageTemplate Clone()
        {
            WebPageTemplate ret = new WebPageTemplate();
            ret.Name = "Copy - " + Name;
            foreach (WebPartTemplate idx in Containers)
            {
                WebPartTemplate t = new WebPartTemplate();
                t.CssClass = idx.CssClass;
                t.Overflow = idx.Overflow;
                t.Height = idx.Height;
                t.Last = idx.Last;
                t.MarginBottom = idx.MarginBottom;
                t.MarginLeft = idx.MarginLeft;
                t.MarginRight = idx.MarginRight;
                t.MarginTop = idx.MarginTop;
                t.ModuleName = idx.ModuleName;
                t.Name = idx.Name;
                t.PageTemplate = idx.PageTemplate;
                t.ViewportContainer = idx.ViewportContainer;
                t.Width = idx.Width;
                ret.Containers.Add(t);
            }
            return ret;
        }

        public override void Delete()
        {
            foreach (WebPage idx in 
                WebPage.Select(
                    Criteria.ExistsIn(this.ID, true)))
            {
                idx.Delete(); // Will force a retrigger of its template container
            }
            base.Delete();
        }

        public override void Save()
        {
            if (ID == 0)
            {
                if (string.IsNullOrEmpty(Name))
                    Name = "Default";
                Created = DateTime.Now;
                if (Containers.Count == 0)
                {
                    // Making sure it has one default container at least ...
                    WebPartTemplate t = new WebPartTemplate();
                    t.Name = "Default Name";
                    t.CssClass = "mux-webpart-template";
                    t.Width = 22;
                    t.Overflow = true;
                    t.MarginTop = 1;
                    t.MarginLeft = 1;
                    Containers.Add(t);
                }
            }

            string name = Name;

            int idxNo = 2;
            while (CountWhere(
                Criteria.Eq("Name", name),
                Criteria.NotId(ID)) > 0)
            {
                name = Name + "-" + idxNo.ToString();
                idxNo += 1;
            }
            Name = name;

            idxNo = 1;
            foreach (WebPartTemplate idx in Containers)
            {
                idx.ViewportContainer = "content" + idxNo;
                idxNo += 1;
            }
            base.Save();
        }
    }
}
