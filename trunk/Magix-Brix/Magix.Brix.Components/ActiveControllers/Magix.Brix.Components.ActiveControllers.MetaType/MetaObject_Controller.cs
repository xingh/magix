﻿/*
 * Magix - A Modular-based Framework for building Web Applications 
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using Magix.Brix.Loader;
using Magix.Brix.Types;
using Magix.Brix.Components.ActiveTypes.MetaTypes;
using Magix.Brix.Data;
using Magix.UX.Widgets;

namespace Magix.Brix.Components.ActiveControllers.MetaTypes
{
    /**
     * Contains logic for editing, maintaining and viewing MetaObjects. MetaObjects are at the 
     * heart of the Meta Application System since they serve as the 'storage' for everything a
     * view updates or creates through interaction with the end user
     */
    [ActiveController]
    public class MetaObject_Controller : ActiveController
    {
        /**
         * Returns menu event handlers for viewing MetaObjects
         */
        [ActiveEvent(Name = "Magix.Publishing.GetPluginMenuItems")]
        protected void Magix_Publishing_GetPluginMenuItems(object sender, ActiveEventArgs e)
        {
            e.Params["Items"]["MetaType"]["Caption"].Value = "Meta Types";
            e.Params["Items"]["MetaType"]["Items"]["Types"]["Caption"].Value = "Meta Objects ...";
            e.Params["Items"]["MetaType"]["Items"]["Types"]["Event"]["Name"].Value = "Magix.MetaType.EditMetaObjects_UnFiltered";
        }

        /**
         * Will open up editing of the MetaObject directly, without any 'views' or other interferences.
         * Mostly meant for administrators to edit objects in 'raw mode'
         */
        [ActiveEvent(Name = "Magix.MetaType.EditMetaObjects_UnFiltered")]
        protected void Magix_MetaType_EditMetaObjects_UnFiltered(object sender, ActiveEventArgs e)
        {
            Node node = new Node();

            node["FullTypeName"].Value = typeof(MetaObject).FullName; // NO '-MEAT' here ... !!!
            node["Container"].Value = "content3";
            node["Width"].Value = 18;
            node["Last"].Value = true;
            node["CssClass"].Value = "edit-objects";
            node["WhiteListColumns"]["TypeName"].Value = true;
            node["WhiteListColumns"]["TypeName"]["ForcedWidth"].Value = 5;
            node["WhiteListColumns"]["Reference"].Value = true;
            node["WhiteListColumns"]["Reference"]["ForcedWidth"].Value = 6;
            node["WhiteListColumns"]["Copy"].Value = true;
            node["WhiteListColumns"]["Copy"]["ForcedWidth"].Value = 2;
            node["FilterOnId"].Value = true;
            node["IDColumnName"].Value = "Edit";
            node["IDColumnEvent"].Value = "Magix.MetaType.EditONEMetaObject_UnFiltered";
            node["DeleteColumnEvent"].Value = "Magix.MetaType.DeleteObjectRaw";

            node["ReuseNode"].Value = true;
            node["CreateEventName"].Value = "Magix.MetaType.CreateMetaObjectAndEdit";

            node["Type"]["Properties"]["TypeName"]["ReadOnly"].Value = false;
            node["Type"]["Properties"]["TypeName"]["Header"].Value = "Type";
            node["Type"]["Properties"]["Reference"]["ReadOnly"].Value = true;
            node["Type"]["Properties"]["Copy"]["NoFilter"].Value = true;
            node["Type"]["Properties"]["Copy"]["TemplateColumnEvent"].Value = "Magix.MetaType.GetCopyMetaObjectTemplateColumn";

            node["Criteria"]["C1"]["Name"].Value = "Sort";
            node["Criteria"]["C1"]["Value"].Value = "Created";
            node["Criteria"]["C1"]["Ascending"].Value = false;

            ActiveEvents.Instance.RaiseActiveEvent(
                this,
                "DBAdmin.Form.ViewClass",
                node);
        }

        /**
         * Will return a 'Copy Template LinkButton' back to caller
         */
        [ActiveEvent(Name = "Magix.MetaType.GetCopyMetaObjectTemplateColumn")]
        protected void Magix_MetaType_GetCopyMetaObjectTemplateColumn(object sender, ActiveEventArgs e)
        {
            // Extracting necessary variables ...
            string name = e.Params["Name"].Get<string>();
            string fullTypeName = e.Params["FullTypeName"].Get<string>();
            int id = e.Params["ID"].Get<int>();
            string value = e.Params["Value"].Get<string>();

            // Fetching specific user
            // Creating our SelectList
            LinkButton ls = new LinkButton();
            ls.Text = "Copy";
            ls.Click +=
                delegate
                {
                    Node tmp = new Node();
                    tmp["ID"].Value = id;
                    RaiseEvent(
                        "Magix.MetaType.CopyMetaObject",
                        tmp);

                    ActiveEvents.Instance.RaiseClearControls("content5");

                    Node node = new Node();
                    node["FullTypeName"].Value = typeof(MetaObject).FullName;

                    RaiseEvent(
                        "Magix.Core.UpdateGrids",
                        node);

                    node = new Node();
                    node["ID"].Value = id;

                    RaiseEvent(
                        "Magix.MetaType.EditONEMetaObject_UnFiltered",
                        node);

                    node = new Node();
                    node["ID"].Value = id;
                    node["FullTypeName"].Value = typeof(MetaObject).FullName;

                    RaiseEvent(
                        "DBAdmin.Grid.SetActiveRow",
                        node);

                };


            // Stuffing our newly created control into the return parameters, so
            // our Grid control can put it where it feels for it ... :)
            e.Params["Control"].Value = ls;
        }

        /**
         * Will copy the incoming MetaObject ['ID']
         */
        [ActiveEvent(Name = "Magix.MetaType.CopyMetaObject")]
        private void Magix_MetaType_CopyMetaObject(object sender, ActiveEventArgs e)
        {
            int id = e.Params["ID"].Get<int>();
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                MetaObject n = MetaObject.SelectByID(id).Clone();
                tr.Commit();
            }
        }

        /**
         * Creates a new MetaObject with some default values and returns the ID of the new MetaObject
         * as 'NewID'
         */
        [ActiveEvent(Name = "Magix.MetaType.CreateMetaObject")]
        protected void Magix_MetaType_CreateMetaObject(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                MetaObject m = new MetaObject();
                m.TypeName = "[Anonymous-Coward]";
                m.Reference = e.Name;

                MetaObject.Value val = new MetaObject.Value();
                val.Name = "Default Name";
                val.Val = "Default Value";
                m.Values.Add(val);

                m.Save();

                tr.Commit();
                e.Params["NewID"].Value = m.ID;
            }
        }

        /**
         * Creates a new MetaObject with some default values, and lets the end user edit it immediately
         */
        [ActiveEvent(Name = "Magix.MetaType.CreateMetaObjectAndEdit")]
        protected void Magix_MetaType_CreateMetaObjectAndEdit(object sender, ActiveEventArgs e)
        {
            Node x = new Node();
            RaiseEvent(
                "Magix.MetaType.CreateMetaObject",
                x);
 
            Node node = new Node();
            node["Start"].Value = 0;
            node["End"].Value = 10;
            node["FullTypeName"].Value = typeof(MetaObject).FullName;

            RaiseEvent(
                "Magix.Core.SetGridPageStart",
                node);

            node = new Node();
            node["ID"].Value = x["NewID"].Value;

            RaiseEvent(
                "Magix.MetaType.EditONEMetaObject_UnFiltered",
                node);
        }

        // TODO: Break further up. Too long ...
        /**
         * Allows for editing the MetaObject directly without any Views filtering out anything
         */
        [ActiveEvent(Name = "Magix.MetaType.EditONEMetaObject_UnFiltered")]
        protected void Magix_MetaType_EditONEMetaObject_UnFiltered(object sender, ActiveEventArgs e)
        {
            MetaObject m = MetaObject.SelectByID(e.Params["ID"].Get<int>());

            Node node = new Node();

            string cssClass = "";

            if (string.IsNullOrEmpty(m.TypeName))
            {
                cssClass = "no-typename";
            }
            else
            {
                int rnd = Math.Abs(m.TypeName.GetHashCode());
                switch (rnd % 7) // TODO: Improve statistical probability by 'smoothening' it ...
                {
                    case 0:
                        cssClass = "type-1";
                        break;
                    case 1:
                        cssClass = "type-2";
                        break;
                    case 2:
                        cssClass = "type-3";
                        break;
                    case 3:
                        cssClass = "type-4";
                        break;
                    case 4:
                        cssClass = "type-5";
                        break;
                    case 5:
                        cssClass = "type-6";
                        break;
                    case 6:
                        cssClass = "type-7";
                        break;
                }
            }

            node["CssClass"].Value = cssClass;

            node["FullTypeName"].Value = typeof(MetaObject).FullName;
            node["ID"].Value = m.ID;

            // First filtering OUT columns ...!
            node["WhiteListColumns"]["TypeName"].Value = true;
            node["WhiteListColumns"]["Reference"].Value = true;
            node["WhiteListColumns"]["Created"].Value = true;
            node["WhiteListColumns"]["Children"].Value = true;

            foreach (var idx in m.Values)
            {
                node["WhiteListColumns"][idx.Name].Value = true;
            }

            node["WhiteListProperties"]["Name"].Value = true;
            node["WhiteListProperties"]["Name"]["ForcedWidth"].Value = 3;
            node["WhiteListProperties"]["Value"].Value = true;
            node["WhiteListProperties"]["Value"]["ForcedWidth"].Value = 10;

            node["Type"]["Properties"]["TypeName"]["ReadOnly"].Value = false;
            node["Type"]["Properties"]["TypeName"]["Header"].Value = "Type";
            node["Type"]["Properties"]["Reference"]["ReadOnly"].Value = false;
            node["Type"]["Properties"]["Created"]["ReadOnly"].Value = true;
            node["Type"]["Properties"]["Created"]["Header"].Value = "When";
            node["Type"]["Properties"]["Children"]["Header"].Value = "Children";
            node["Type"]["Properties"]["Children"]["TemplateColumnEvent"].Value = "Magix.MetaType.GetChildrenTemplateColumn";

            foreach (var idx in m.Values)
            {
                node["Type"]["Properties"][idx.Name]["TemplateColumnEvent"].Value = "Magix.MetaType.GetValuesTemplateColumn";
                node["Type"]["Properties"][idx.Name]["TemplateColumnHeaderEvent"].Value = "Magix.MetaType.GetHeaderTemplateColumn";
                node["Type"]["Properties"][idx.Name]["ReadOnly"].Value = false;
                node["Object"]["Properties"][idx.Name].Value = idx.Val;
            }

            node["Width"].Value = 16;
            node["Last"].Value = true;
            node["Padding"].Value = 8;
            node["MarginBottom"].Value = 20;
            if (e.Params != null && 
                e.Params.Contains("Container") &&
                e.Params["Container"].Get<string>() != "content4")
            {
                node["Container"].Value = e.Params["Container"].Value;
                node["PullTop"].Value = 18;
            }
            else
            {
                node["Container"].Value = "content4";
                ActiveEvents.Instance.RaiseClearControls("content5");
            }

            RaiseEvent(
                "DBAdmin.Form.ViewComplexObject",
                node);

            string container = node["Container"].Get<string>();

            // Must append a 'New Property Button' after our object editing view
            node = new Node();

            node["Text"].Value = "+";
            node["ToolTip"].Value = "Click to create a New Property for your Object ...";
            node["ButtonCssClass"].Value = "span-2 clear-left";
            node["Append"].Value = true;
            node["Event"].Value = "Magix.MetaType.CreateNewProperty";
            node["Event"]["ObjectID"].Value = m.ID;
            node["Event"]["Container"].Value = container;

            LoadModule(
                "Magix.Brix.Components.ActiveModules.CommonModules.Clickable",
                container,
                node);
        }

        /**
         * Will Append an existing MetaObject [ID] to another existing MetaObject [ParentID] as a child
         */
        [ActiveEvent(Name = "Magix.MetaType.AppendChildMetaObjectToMetaObject")]
        protected void Magix_MetaType_AppendChildMetaObjectToMetaObject(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                MetaObject parent = MetaObject.SelectByID(e.Params["ParentID"].Get<int>());
                MetaObject child = MetaObject.SelectByID(e.Params["ID"].Get<int>());

                parent.Children.Add(child);

                parent.Save();

                tr.Commit();
            }
        }

        /**
         * Will Append an existing MetaObject [ID] to another existing MetaObject [ParentID] as a child
         * and immediately Edit the Parent MetaObject
         */
        [ActiveEvent(Name = "Magix.MetaType.AppendChildMetaObjectToMetaObjectAndEditParent")]
        protected void Magix_MetaType_AppendChildMetaObjectToMetaObjectAndEditParent(object sender, ActiveEventArgs e)
        {
            RaiseEvent(
                "Magix.MetaType.AppendChildMetaObjectToMetaObject",
                e.Params);

            ActiveEvents.Instance.RaiseClearControls("content6");

            Node node = new Node();

            node["ID"].Value = e.Params["ParentID"].Value;

            RaiseEvent(
                "Magix.MetaType.EditONEMetaObject_UnFiltered",
                node);
        }

        [ActiveEvent(Name = "Magix.MetaType.AppendMetaTypeValue")]
        protected void DBAdmin_Form_AppendObject(object sender, ActiveEventArgs e)
        {
            e.Params["Container"].Value = "content6";
            e.Params["ReUseNode"].Value = true;
            e.Params["Padding"].Value = 8;
            e.Params["Width"].Value = 16;
            e.Params["PullTop"].Value = 18;
            e.Params["Last"].Value = true;
            e.Params["SelectEvent"].Value = "Magix.MetaType.AppendChildMetaObjectToMetaObjectAndEditParent";

            e.Params["WhiteListColumns"]["TypeName"].Value = true;
            e.Params["WhiteListColumns"]["TypeName"]["ForcedWidth"].Value = 4;
            e.Params["WhiteListColumns"]["Reference"].Value = true;
            e.Params["WhiteListColumns"]["Reference"]["ForcedWidth"].Value = 6;

            RaiseEvent(
                "DBAdmin.Form.AppendObject",
                e.Params);
        }

        [ActiveEvent(Name = "Magix.MetaType.CreateNewProperty")]
        protected void Magix_MetaType_CreateNewProperty(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                MetaObject o = MetaObject.SelectByID(e.Params["ObjectID"].Get<int>());
                MetaObject.Value v = new MetaObject.Value();
                o.Values.Add(v);

                o.Save();

                tr.Commit();

                Node node = new Node();
                node["ID"].Value = o.ID;
                node["Container"].Value = e.Params["Container"].Value;

                // Easy out ...
                RaiseEvent(
                    "Magix.MetaType.EditONEMetaObject_UnFiltered",
                    node);
            }
        }

        [ActiveEvent(Name = "Magix.MetaType.EditObjectRaw-2")]
        protected void Magix_MetaType_EditObjectRaw_2(object sender, ActiveEventArgs e)
        {
            e.Params["Container"].Value = "content6";
            RaiseEvent(
                "Magix.MetaType.EditONEMetaObject_UnFiltered",
                e.Params);
        }

        [ActiveEvent(Name = "Magix.MetaType.GetChildrenTemplateColumn")]
        protected void Magix_Meta_GetChildrenTemplateColumn(object sender, ActiveEventArgs e)
        {
            // Extracting necessary variables ...
            string name = e.Params["Name"].Get<string>();
            string fullTypeName = e.Params["FullTypeName"].Get<string>();
            int id = e.Params["ID"].Get<int>();
            string value = e.Params["Value"].Get<string>();

            // Creating our SelectList
            LinkButton ls = new LinkButton();
            ls.Text = value;

            // Supplying our Event Handler for the Changed Event ...
            ls.Click +=
                delegate
                {
                    Node node = new Node();

                    node["Container"].Value = "content5";
                    node["Top"].Value = 1;
                    node["Width"].Value = 16;
                    node["Padding"].Value = 8;
                    node["Last"].Value = true;
                    node["PullTop"].Value = 18;
                    node["MarginBottom"].Value = 20;

                    node["ID"].Value = id;
                    node["PropertyName"].Value = "Children";
                    node["IsList"].Value = true;
                    node["FullTypeName"].Value = typeof(MetaObject).FullName;
                    node["ReUseNode"].Value = true;
                    node["IDColumnName"].Value = "Edit";
                    node["IDColumnEvent"].Value = "Magix.MetaType.EditObjectRaw-2";
                    node["AppendEventName"].Value = "Magix.MetaType.AppendMetaTypeValue";
                    node["RemoveEvent"].Value = "Magix.Publishing.RemoveChildObject";

                    node["WhiteListColumns"]["TypeName"].Value = true;
                    node["WhiteListColumns"]["TypeName"]["ForcedWidth"].Value = 5;
                    node["WhiteListColumns"]["Reference"].Value = true;
                    node["WhiteListColumns"]["Reference"]["ForcedWidth"].Value = 6;

                    node["Type"]["Properties"]["TypeName"]["ReadOnly"].Value = false;
                    node["Type"]["Properties"]["TypeName"]["Header"].Value = "Type";
                    node["Type"]["Properties"]["Reference"]["ReadOnly"].Value = true;

                    RaiseEvent(
                        "DBAdmin.Form.ViewListOrComplexPropertyValue",
                        node);

                    ActiveEvents.Instance.RaiseClearControls("content6");
                };

            // Stuffing our newly created control into the return parameters, so
            // our Grid control can put it where it feels for it ... :)
            e.Params["Control"].Value = ls;
        }

        [ActiveEvent(Name = "Magix.Publishing.RemoveChildObject")]
        protected void Magix_Publishing_RemoveChildObject(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                MetaObject parent = MetaObject.SelectByID(e.Params["ParentID"].Get<int>());
                MetaObject child = MetaObject.SelectByID(e.Params["ID"].Get<int>());

                parent.Children.Remove(child);

                parent.Save();

                tr.Commit();

                Node node = new Node();
                node["ID"].Value = parent.ID;

                // Easy out ...
                RaiseEvent(
                    "Magix.MetaType.EditONEMetaObject_UnFiltered",
                    node);
            }
        }

        [ActiveEvent(Name = "Magix.MetaType.GetValuesTemplateColumn")]
        protected void Magix_Meta_GetValuesTemplateColumn(object sender, ActiveEventArgs e)
        {
            // Extracting necessary variables ...
            string name = e.Params["Name"].Get<string>();
            string fullTypeName = e.Params["FullTypeName"].Get<string>();
            int id = e.Params["ID"].Get<int>();
            string value = e.Params["Value"].Get<string>();

            // Creating our Edit Value
            TextAreaEdit inplace = new TextAreaEdit();
            inplace.Text = value;
            inplace.CssClass = "mux-in-place-edit larger left-float";

            // Supplying our Event Handler for the Changed Event ...
            inplace.TextChanged +=
                delegate
                {
                    using (Transaction tr = Adapter.Instance.BeginTransaction())
                    {
                        MetaObject o = MetaObject.SelectByID(id);
                        MetaObject.Value va = o.Values.Find(
                            delegate(MetaObject.Value idxS)
                            {
                                return idxS.Name == name;
                            });
                        va.Val = inplace.Text;

                        va.Save();

                        tr.Commit();
                    }
                };

            System.Web.UI.WebControls.PlaceHolder p = new System.Web.UI.WebControls.PlaceHolder();
            p.Controls.Add(inplace);

            LinkButton lb = new LinkButton();
            lb.Text = "&nbsp;";
            lb.CssClass = "remove-property";
            lb.ToolTip = "Click to Permanently Remove Property ...";
            lb.Click +=
                delegate
                {
                    using (Transaction tr = Adapter.Instance.BeginTransaction())
                    {
                        MetaObject o = MetaObject.SelectByID(id);
                        MetaObject.Value va = o.Values.Find(
                            delegate(MetaObject.Value idxS)
                            {
                                return idxS.Name == name;
                            });
                        va.Delete();

                        o.Values.Remove(va);

                        tr.Commit();

                        // Since the property was deleted, we will
                        // re-databind the whole grid for simplicity ...

                        Node node = new Node();
                        node["ID"].Value = id;

                        RaiseEvent(
                            "Magix.MetaType.EditONEMetaObject_UnFiltered",
                            node);
                    }
                };
            p.Controls.Add(lb);

            // Stuffing our newly created control into the return parameters, so
            // our Grid control can put it where it feels for it ... :)
            e.Params["Control"].Value = p;
        }

        [ActiveEvent(Name = "Magix.MetaType.GetHeaderTemplateColumn")]
        protected void Magix_Meta_GetHeaderTemplateColumn(object sender, ActiveEventArgs e)
        {
            // Extracting necessary variables ...
            string name = e.Params["Name"].Get<string>();
            string fullTypeName = e.Params["FullTypeName"].Get<string>();
            int id = e.Params["ID"].Get<int>();
            string value = e.Params["Value"].Get<string>();
            string container = e.Params["Container"].Get<string>();

            // Creating our Edit Name of property editer ...
            InPlaceEdit ls = new InPlaceEdit();
            ls.Text = name;
            ls.CssClass = "mux-in-place-edit larger";

            // Supplying our Event Handler for the Changed Event ...
            ls.TextChanged +=
                delegate
                {
                    using (Transaction tr = Adapter.Instance.BeginTransaction())
                    {
                        MetaObject o = MetaObject.SelectByID(id);
                        MetaObject.Value va = o.Values.Find(
                            delegate(MetaObject.Value idxS)
                            {
                                return idxS.Name == name;
                            });
                        if (ls.Text == va.Name)
                            return;
                        va.Name = ls.Text;

                        va.Save();

                        tr.Commit();

                        // Since the Name of the Property has changed, we
                        // re-databind the whole grid for simplicity ...

                        Node node = new Node();
                        node["ID"].Value = id;
                        node["Container"].Value = container;

                        RaiseEvent(
                            "Magix.MetaType.EditONEMetaObject_UnFiltered",
                            node);
                    }
                };

            // Stuffing our newly created control into the return parameters, so
            // our Grid control can put it where it feels for it ... :)
            e.Params["Control"].Value = ls;
        }

        [ActiveEvent(Name = "Magix.MetaType.DeleteObjectRaw")]
        protected void Magix_MetaType_DeleteObjectRaw(object sender, ActiveEventArgs e)
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
<p>Are you sure you wish to delete this object? 
Deletion is permanent, and cannot be undone! 
Deletion of this object <span style=""color:Red;font-weight:bold;"">will also trigger 
deletion of several other objects</span>, since it may 
have relationships towards other instances in your database.</p>";
            node["OK"]["ID"].Value = id;
            node["OK"]["FullTypeName"].Value = fullTypeName;
            node["OK"]["Event"].Value = "Magix.MetaType.DeleteObjectRaw-Confirmed";
            node["Cancel"]["Event"].Value = "DBAdmin.Common.ComplexInstanceDeletedNotConfirmed";
            node["Cancel"]["FullTypeName"].Value = fullTypeName;
            node["Width"].Value = 15;

            LoadModule(
                "Magix.Brix.Components.ActiveModules.CommonModules.MessageBox",
                "child",
                node);
        }

        [ActiveEvent(Name = "Magix.MetaType.DeleteObjectRaw-Confirmed")]
        protected void Magix_MetaType_DeleteObjectRaw_Confirmed(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                MetaObject m = MetaObject.SelectByID(e.Params["ID"].Get<int>());
                m.Delete();

                tr.Commit();

                ActiveEvents.Instance.RaiseClearControls("content4");
                ActiveEvents.Instance.RaiseClearControls("child");

                Node node = new Node();
                node["FullTypeName"].Value = typeof(MetaObject).FullName;

                RaiseEvent(
                    "Magix.Core.UpdateGrids",
                    node);
            }
        }

        [ActiveEvent(Name = "DBAdmin.Common.CreateObjectAsChild")]
        protected void DBAdmin_Common_CreateObjectAsChild(object sender, ActiveEventArgs e)
        {
            if (e.Params["FullTypeName"].Get<string>() == typeof(MetaObject.Value).FullName)
            {
                Node node = new Node();
                node["FullTypeName"].Value = typeof(MetaObject).FullName;

                RaiseEvent(
                    "Magix.Core.UpdateGrids",
                    node);
            }
        }

        [ActiveEvent(Name = "DBAdmin.Common.ComplexInstanceDeletedConfirmed")]
        protected void DBAdmin_Common_ComplexInstanceDeletedConfirmed(object sender, ActiveEventArgs e)
        {
            if (e.Params["FullTypeName"].Get<string>() == typeof(MetaObject).FullName)
            {
                ActiveEvents.Instance.RaiseClearControls("content4");
            }
        }

        [ActiveEvent(Name = "Magix.Publishing.GetDataForAdministratorDashboard")]
        protected void Magix_Publishing_GetDataForAdministratorDashboard(object sender, ActiveEventArgs e)
        {
            e.Params["WhiteListColumns"]["MetaTypesCount"].Value = true;
            e.Params["Type"]["Properties"]["MetaTypesCount"]["ReadOnly"].Value = true;
            e.Params["Type"]["Properties"]["MetaTypesCount"]["Header"].Value = "Objects";
            e.Params["Type"]["Properties"]["MetaTypesCount"]["ClickLabelEvent"].Value = "Magix.MetaType.EditMetaObjects_UnFiltered";
            e.Params["Object"]["Properties"]["MetaTypesCount"].Value = MetaObject.Count.ToString();
        }

        [ActiveEvent(Name = "Magix.MetaType.SetMetaObjectValue")]
        protected void Magix_MetaType_SetMetaObjectValue(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                MetaObject o = MetaObject.SelectByID(e.Params["MetaObjectID"].Get<int>());
                MetaObject.Value val = o.Values.Find(
                    delegate(MetaObject.Value idx)
                    {
                        return idx.Name == e.Params["Name"].Get<string>();
                    });
                if (val == null)
                {
                    val = new MetaObject.Value();
                    val.Name = e.Params["Name"].Get<string>();
                    o.Values.Add(val);

                    o.Save();
                }
                val.Val = e.Params["Value"].Get<string>();

                val.Save();

                tr.Commit();
            }
        }
    }
}




















