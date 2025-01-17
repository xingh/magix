﻿/*
 * Magix-BRIX - A Web Application Framework for ASP.NET
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix-BRIX is licensed as GPLv3.
 */

using System;
using System.Web.UI;
using System.Collections.Generic;
using ASP = System.Web.UI.WebControls;
using Magix.UX.Widgets;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using Magix.UX.Effects;
using Magix.UX;

namespace Magix.Brix.Components.ActiveModules.CommonModules
{
    /**
     * Level2: Shows a Tree module for the end user for him to navigate and select single nodes from.
     * Change its properties by passing in 'TreeCssClass' or 'NoClose'. 'Items' should 
     * contain the tree items in a hierarchical fashion with e.g. 'Item/i-1/' containing
     * 'Name' 'CssClass' and 'ToolTip'. 'Name' being minimum. Child items of 'Items/i-1'
     * should be stored in the 'Items/i-1/Items' node. Will raise 'GetItemsEvent'
     * upon needing to refresh for some reasons, and 
     * 'ItemSelectedEvent' with 'SelectedItemID' parameter as selected item ID upon
     * user selecting an item
     */
    [ActiveModule]
    public class Tree : ActiveModule
    {
        protected Label header;
        protected TreeView tree;

        public override void InitialLoading(Node node)
        {
            base.InitialLoading(node);
            Load += delegate
            {
                if (node.Contains("TreeCssClass"))
                    tree.CssClass += " " + node["TreeCssClass"].Get<string>();

                if (node.Contains("NoClose"))
                    tree.NoCollapseOfItems = node["NoClose"].Get<bool>();

                if (node.Contains("Header"))
                {
                    header.Visible = true;
                    header.Text = node["Header"].Get<string>();

                    if (node.Contains("HeaderCssClass"))
                        header.CssClass += " " + node["HeaderCssClass"].Get<string>();
                }
                else
                {
                    header.Visible = false;
                }
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DataSource != null)
                DataBindTree(null);
        }

        private void DataBindTree(string selId)
        {
            foreach (Node idx in DataSource["Items"])
            {
                AddNodeToControl(idx, tree, selId);
            }
        }

        private void AddNodeToControl(Node node, Control ctrl, string selId)
        {
            TreeItem i = new TreeItem();
            i.Text = node["Name"].Get<string>();
            i.ID = "i-" + node["ID"].Value;
            i.Info = node["ID"].Value.ToString();
            if (node.Contains("CssClass"))
                i.CssClass += " " + node["CssClass"].Get<string>();
            if (node.Contains("ToolTip"))
                i.ToolTip = node["ToolTip"].Get<string>();

            if (node.Contains("Items"))
            {
                foreach (Node idxI in node["Items"])
                {
                    AddNodeToControl(idxI, i, selId);
                }
            }

            if (ctrl is TreeView)
                (ctrl as TreeView).Controls.Add(i);
            else
                (ctrl as TreeItem).Content.Controls.Add(i);
            if (i.ID == selId)
            {
                tree.SelectedItem = i;
                i.CssClass += " mux-tree-selected";
            }
        }

        /**
         * Level2: Overrides to make sure we also update this bugger upon changing of these types
         * of records, but only if 'relevant'
         */
        [ActiveEvent(Name = "Magix.Core.UpdateGrids")]
        protected void Magix_Core_UpdateGrids(object sender, ActiveEventArgs e)
        {
            if (CheckForTypeHit(e.Params))
            {
                DataSource["Items"].UnTie();

                RaiseSafeEvent(
                    DataSource["GetItemsEvent"].Get<string>(),
                    DataSource);

                ReDataBind();

                new EffectHighlight(tree, 250)
                    .Render();
            }
        }

        /**
         * Level2: Sets the selected tree item to the 'ID' given
         */
        [ActiveEvent(Name = "Magix.Core.SetTreeSelectedID")]
        protected void Magix_Core_SetTreeSelectedID(object sender, ActiveEventArgs e)
        {
            tree.SelectedItem = 
                Selector.FindControl<TreeItem>(
                    tree, 
                    "i-" + e.Params["ID"].Value);

            tree.SelectedItem.Expanded = true;
            tree.SelectedItem.CssClass =
                tree.SelectedItem.CssClass.Replace(" mux-tree-collapsed", " mux-tree-expanded");
        }

        /**
         * Level2: Makes sure the 'currently selected tree item' becomes expanded, 
         * if any are selected
         */
        [ActiveEvent(Name = "Magix.Core.ExpandTreeSelectedID")]
        protected void Magix_Core_ExpandTreeSelectedID(object sender, ActiveEventArgs e)
        {
            if (tree.SelectedItem != null)
            {
                tree.SelectedItem.Expanded = true;
                tree.SelectedItem.CssClass = tree.SelectedItem.CssClass.Replace(" mux-tree-collapsed", " mux-tree-expanded");
            }
        }

        /**
         * Level3: Overridden to handle some internal events
         */
        [ActiveEvent(Name = "Magix.Core.UpdateTree")]
        [ActiveEvent(Name = "DBAdmin.Data.ChangeSimplePropertyValue")]
        protected void DBAdmin_Data_ChangeSimplePropertyValue(object sender, ActiveEventArgs e)
        {
            if (CheckForTypeHit(e.Params))
            {
                DataSource["Items"].UnTie();

                RaiseSafeEvent(
                    DataSource["GetItemsEvent"].Get<string>(),
                    DataSource);

                ReDataBind();

                new EffectHighlight(tree, 250)
                    .Render();
            }
        }

        /**
         * Level2: Returns the selected tree item as 'ID' if any is selected
         */
        [ActiveEvent(Name = "Magix.Core.GetSelectedTreeItem")]
        protected void Magix_Core_GetSelectedTreeItem(object sender, ActiveEventArgs e)
        {
            if (tree.SelectedItem != null)
                e.Params["ID"].Value = int.Parse(tree.SelectedItem.Info);
        }

        protected void tree_SelectedItemChanged(object sender, EventArgs e)
        {
            if (DataSource.Contains("ItemSelectedEvent"))
            {
                DataSource["SelectedItemID"].Value = tree.SelectedItem.Info;

                RaiseSafeEvent(
                    DataSource["ItemSelectedEvent"].Get<string>(),
                    DataSource);
            }
        }

        private bool CheckForTypeHit(Node node)
        {
            if (DataSource.Contains("FullTypeName") &&
                !string.IsNullOrEmpty(DataSource["FullTypeName"].Get<string>()))
            {
                if (node["FullTypeName"].Get<string>().Contains(DataSource["FullTypeName"].Get<string>()))
                    return true;
                if (DataSource.Contains("CoExistsWith") &&
                    DataSource["CoExistsWith"].Get<string>().Contains(node["FullTypeName"].Get<string>()))
                    return true;
            }
            return false;
        }

        private void ReDataBind()
        {
            List<string> expanded = new List<string>();
            string selId = tree.SelectedItem == null ? "" : tree.SelectedItem.ID;
            foreach (TreeItem idx in Selector.Select<TreeItem>(
                tree,
                delegate(Control idxC)
                {
                    TreeItem i = idxC as TreeItem;
                    if (i != null)
                        return i.Expanded;
                    return false;
                }))
            {
                expanded.Add(idx.ID);
            }
            tree.Controls.Clear();
            DataBindTree(selId);

            foreach (TreeItem idx in Selector.Select<TreeItem>(tree))
            {
                if (expanded.Contains(idx.ID))
                {
                    TreeItem tmp = idx;
                    while (tmp != null)
                    {
                        tmp.Expanded = true;
                        tmp.CssClass = tmp.CssClass.Replace(" mux-tree-collapsed", " mux-tree-expanded");
                        tmp = tmp.Parent.Parent as TreeItem;
                    }
                }
            }
            tree.ReRender();
        }
    }
}
