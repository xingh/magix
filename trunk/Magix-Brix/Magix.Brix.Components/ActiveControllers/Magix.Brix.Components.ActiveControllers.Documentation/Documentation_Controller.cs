﻿/*
 * Magix - A Modular-based Framework for building Web Applications 
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using Doxygen.NET;
using System.Reflection;
using System.Collections.Generic;

namespace Magix.Brix.Components.ActiveControllers.Documentation
{
    /**
     * Contains the logic for our 'Class Browser' which can browse all the 
     * classes in the system and make some changes to them by enabling them and
     * disabling them by either overriding specific events or by disabling entire
     * controllers or modules all together
     */
    [ActiveController]
    public class Documentation_Controller : ActiveController
    {
        /**
         * Will return the menu items needed to start the class browser
         */
        [ActiveEvent(Name = "Magix.Publishing.GetPluginMenuItems")]
        protected void Magix_Publishing_GetPluginMenuItems(object sender, ActiveEventArgs e)
        {
            e.Params["Items"]["Admin"]["Caption"].Value = "Admin";

            Node tmp = e.Params["Items"]["Admin"]["Items"]["DoxDotNet"];
            tmp["Caption"].Value = "Class Browser ...";
            tmp["Event"]["Name"].Value = "Magix.MetaType.ViewDoxygenFiles";
        }

        /**
         * Gets every namespace and class into a tree hierarchy and loads up the Tree module
         * to display it
         */
        [ActiveEvent(Name = "Magix.MetaType.ViewDoxygenFiles")]
        protected void Magix_MetaType_ViewDoxygenFiles(object sender, ActiveEventArgs e)
        {
            Node node = new Node();
            foreach (Namespace idx in Docs.Namespaces)
            {
                Node outerMostNamespace = AddNamespaceToNode(idx, node);
                foreach (Class idxC in idx.Classes)
                {
                    AddClassToNode(idxC, outerMostNamespace);
                }
            }

            node["Width"].Value = 9;
            node["MarginBottom"].Value = 10;
            node["Top"].Value = 1;
            node["Last"].Value = false;
            node["FullTypeName"].Value = typeof(Namespace).FullName;
            node["ItemSelectedEvent"].Value = "Magix.Doxygen.ViewNamespaceOrClass";

            LoadModule(
                "Magix.Brix.Components.ActiveModules.CommonModules.Tree",
                "content3",
                node);
        }

        #region [ -- Helper methods for viewing doxygen files -- ]

        /*
         * Helper for above
         */
        private Node AddNamespaceToNode(Namespace idx, Node node)
        {
            string fullName = idx.FullName;
            string[] splits = fullName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            Node tmp = node;
            string tmpFullNs = "";
            while (splits.Length > 0)
            {
                string currentNamespace = splits[0];
                if (!string.IsNullOrEmpty(tmpFullNs))
                    tmpFullNs += ".";
                tmpFullNs += currentNamespace;

                tmp = tmp["Items"][currentNamespace];

                AddMicroNamespaceToNode(
                    idx,
                    currentNamespace,
                    tmp,
                    tmpFullNs,
                    fullName.Substring(currentNamespace.Length).Length == 0);

                fullName = fullName.Substring(currentNamespace.Length);

                if (fullName.Length == 0)
                    break;
                else
                    fullName = fullName.Substring(1);

                splits = fullName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return tmp;
        }

        /*
         * Helper for above
         */
        private void AddMicroNamespaceToNode(
            Namespace nameSp,
            string currentNamespace,
            Node node,
            string fullName,
            bool last)
        {
            node["ID"].Value = fullName.Replace(".", "_XX_").Replace("+", "_XQ_");
            node["Name"].Value = currentNamespace;
            if (last)
            {
                if (!string.IsNullOrEmpty(nameSp.Description))
                    node["ToolTip"].Value = nameSp.Description;
                else
                    node["ToolTip"].Value = fullName;
            }
            else
                node["ToolTip"].Value = fullName;
            node["CssClass"].Value = "tree-item-namespace";
        }

        /*
         * Helper for above
         */
        private void AddClassToNode(Class cls, Node outerMostNamespace)
        {
            outerMostNamespace["Items"][cls.FullName]["ID"].Value = cls.FullName.Replace(".", "_XX_").Replace("+", "_XQ_");
            outerMostNamespace["Items"][cls.FullName]["Name"].Value = cls.Name;
            if (!string.IsNullOrEmpty(cls.Description))
                outerMostNamespace["Items"][cls.FullName]["ToolTip"].Value = cls.Description.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "'");
            else
                outerMostNamespace["Items"][cls.FullName]["ToolTip"].Value = cls.FullName;
            outerMostNamespace["Items"][cls.FullName]["CssClass"].Value = "tree-item-class";
        }

        #endregion

        /**
         * Expects a SelectedItemID which should point to either a Namespace or
         * a Class, and will show this namespace/class features. Such as which
         * dll(s) implements tha namespace/class, if a class, which methods it has etc
         */
        [ActiveEvent(Name = "Magix.Doxygen.ViewNamespaceOrClass")]
        protected void Magix_Doxygen_ViewNamespaceOrClass(object sender, ActiveEventArgs e)
        {
            string fullName = e.Params["SelectedItemID"].Get<string>().Replace("_XX_", ".").Replace("_XQ_", "+");


            Namespace nSpc = Docs.Namespaces.Find(
                delegate(Namespace idx)
                {
                    return idx.FullName == fullName;
                });
            if(nSpc != null)
            {
                // Namespace
                ViewNamespaceDlls(nSpc);
            }
            else
            {
                // Class 
                Class cls = Docs.GetAllClasses().Find(
                    delegate(Class idx)
                    {
                        return idx.FullName == fullName;
                    });
                if (cls != null)
                    ShowClassDllsAndMethods(cls);
            }
        }

        #region [ -- Helper methods for Viewing specific class or Namespace -- ]

        /*
         * Helper for above
         */
        private void ViewNamespaceDlls(Namespace nSpc)
        {
            Node node = new Node();

            node["IsDelete"].Value = false;
            node["NoIdColumn"].Value = true;
            node["Container"].Value = "content4";
            node["Width"].Value = 9;
            node["MarginBottom"].Value = 10;
            node["Top"].Value = 1;
            node["Last"].Value = true;
            node["IsCreate"].Value = false;
            node["ReuseNode"].Value = true;

            node["FullTypeName"].Value = typeof(Documentation_Controller).FullName + "-META";

            node["Namespace"].Value = nSpc.FullName;

            ActiveEvents.Instance.RaiseActiveEvent(
                this,
                "DBAdmin.Form.ViewClass",
                node);

            ActiveEvents.Instance.RaiseClearControls("content5");
        }

        /*
         * Helper for above
         */
        private void ShowClassDllsAndMethods(Class cls)
        {
            ViewClassDlls(cls);
            ViewClassMembers(cls);
        }

        /*
         * Shows all DLLs that implements class
         */
        private void ViewClassDlls(Class cls)
        {
            // Showing file(s) that contain the class
            Node node = new Node();

            node["IsDelete"].Value = false;
            node["NoIdColumn"].Value = true;
            node["Container"].Value = "content4";
            node["Width"].Value = 9;
            node["MarginBottom"].Value = 10;
            node["Top"].Value = 1;
            node["Last"].Value = true;
            node["IsCreate"].Value = false;
            node["ReuseNode"].Value = true;

            node["FullTypeName"].Value = typeof(Documentation_Controller).FullName + "-META";

            node["Class"].Value = cls.FullName;

            ActiveEvents.Instance.RaiseActiveEvent(
                this,
                "DBAdmin.Form.ViewClass",
                node);
        }

        /*
         * Shows the members of the Class in a Grid plus all documentation in regards to the class
         */
        private void ViewClassMembers(Class cls)
        {
            // Loading 'View Class Details' Module ...
            Node node = new Node();

            node["Width"].Value = 18;
            node["Padding"].Value = 6;
            node["MarginBottom"].Value = 10;
            node["PullTop"].Value = 8;
            node["Last"].Value = true;
            node["Description"].Value = cls.Description;
            if (cls.FullName.IndexOf('.') > 0)
                node["Name"].Value = cls.FullName.Substring(cls.FullName.LastIndexOf('.') + 1);
            else
                node["Name"].Value = cls.FullName;
            node["FullName"].Value = cls.FullName;

            // Showing all Methods of class
            foreach (Method idx in cls.Methods)
            {
                if (string.IsNullOrEmpty(idx.Description))
                    continue; // Don't do methods not described ...

                node["Methods"][idx.ID]["Name"].Value = idx.Name;
                node["Methods"][idx.ID]["Access"].Value = idx.AccessModifier;
                node["Methods"][idx.ID]["Returns"].Value = idx.ReturnType;
                node["Methods"][idx.ID]["Description"].Value =
                    idx.Description
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");
                node["Methods"][idx.ID]["DescriptionHTML"].Value =
                    idx.Description
                    .Replace("<para", "<p")
                    .Replace("</para>", "</p>");

                foreach (Parameter idxP in idx.Parameters)
                {
                    node["Methods"][idx.ID]["Pars"][idxP.Name]["Name"].Value = idxP.Name;
                    node["Methods"][idx.ID]["Pars"][idxP.Name]["Type"].Value = idxP.Type;
                }
            }

            LoadModule(
                "Magix.Brix.Components.ActiveModules.Documentation.ShowClassDetails",
                "content5",
                node);
        }

        #endregion

        /**
         * Handled to make sure we handle the "Documentation_Controller-META" type for the Grid system
         * so that we can see our DLLs
         */
        [ActiveEvent(Name = "DBAdmin.DynamicType.GetObjectTypeNode")]
        protected void DBAdmin_DynamicType_GetObjectTypeNode(object sender, ActiveEventArgs e)
        {
            if (e.Params["FullTypeName"].Get<string>() == 
                typeof(Documentation_Controller).FullName + "-META")
            {
                e.Params["WhiteListColumns"]["Name"].Value = true;
                e.Params["WhiteListColumns"]["ForcedWidth"].Value = 9;
                e.Params["Type"]["Properties"]["Name"]["ReadOnly"].Value = true;
                e.Params["Type"]["Properties"]["Name"]["Header"].Value = "File";
                e.Params["Type"]["Properties"]["Name"]["NoFilter"].Value = true;
            }
        }

        /**
         * Handled to make sure we handle the "Documentation_Controller-META" type for the Grid system
         * so that we can see our DLLs
         */
        [ActiveEvent(Name = "DBAdmin.DynamicType.GetObjectsNode")]
        protected void DBAdmin_DynamicType_GetObjectsNode(object sender, ActiveEventArgs e)
        {
            if (e.Params["FullTypeName"].Get<string>() == 
                typeof(Documentation_Controller).FullName + "-META")
            {
                List<Assembly> asm = new List<Assembly>();
                foreach (Assembly idx in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if (idx.FullName.StartsWith("System") ||
                            idx.FullName.Contains("mscorlib") ||
                            idx.FullName.StartsWith("Microsoft") ||
                            idx.FullName.StartsWith("DotNetOpenAuth"))
                            continue;
                        foreach (System.Type idxT in idx.GetTypes())
                        {
                            if (idxT.FullName.StartsWith("Microsoft"))
                                continue;
                            if (idxT.Namespace == null)
                                continue;
                            if (e.Params.Contains("Namespace"))
                            {
                                if (idxT.Namespace.Contains(e.Params["Namespace"].Get<string>()))
                                {
                                    if (!asm.Exists(
                                        delegate(Assembly idxA)
                                        {
                                            return idxA.FullName == idxT.Namespace;
                                        }))
                                        asm.Add(idx);
                                }
                            }
                            else if (e.Params.Contains("Class"))
                            {
                                if (idxT.FullName == e.Params["Class"].Get<string>())
                                {
                                    if (!asm.Exists(
                                        delegate(Assembly idxA)
                                        {
                                            return idxA.FullName == idxT.Namespace;
                                        }))
                                        asm.Add(idx);
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        Node xx = new Node();
                        xx["Message"].Value = err.Message +
                            "File; " +
                            idx.FullName;
                        xx["IsError"].Value = true;

                        RaiseEvent(
                            "Magix.Core.ShowMessage",
                            xx);
                    }
                }
                int idxNo = 1;
                foreach (Assembly idx in asm)
                {
                    e.Params["Objects"]["o-" + idx.FullName]["ID"].Value = idxNo;
                    e.Params["Objects"]["o-" + idx.FullName]["Properties"]["Name"].Value = idx.FullName.Split(',')[0] + ".dll";
                    e.Params["Objects"]["o-" + idx.FullName]["Properties"]["Name"]["ToolTip"].Value = idx.FullName;
                    idxNo += 1;
                }
            }
        }

        /**
         * Will cache our Doxygen.NET objects, such that they're faster available
         */
        public Docs Docs
        {
            get
            {
                // Loading it ONCE and caching it for ALL users. VERY expensive operation ...!
                if (Page.Application["Magix.Brix.Components.ActiveControllers.Documentation.Doc"] == null)
                {
                    lock (typeof(Documentation_Controller))
                    {
                        if (Page.Application["Magix.Brix.Components.ActiveControllers.Documentation.Doc"] == null)
                        {
                            Page.Application["Magix.Brix.Components.ActiveControllers.Documentation.Doc"] =
                                new Docs(Page.MapPath("~/DoxFiles"));
                        }
                    }
                }
                return (Docs)Page.Application["Magix.Brix.Components.ActiveControllers.Documentation.Doc"];
            }
        }
    }
}