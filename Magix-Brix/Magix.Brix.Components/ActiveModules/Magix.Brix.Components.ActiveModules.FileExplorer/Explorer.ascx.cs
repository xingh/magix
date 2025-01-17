﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using System.IO;
using System.Web.UI;
using Magix.UX.Widgets;
using Magix.Brix.Types;
using Magix.Brix.Loader;
using Magix.UX.Effects;
using System.Globalization;
using Magix.UX;
using Magix.UX.Widgets.Core;
using Magix.UX.Core;

namespace Magix.Brix.Components.ActiveModules.FileExplorer
{
    // TODO: Refactor saving logic. Call controller ...!
    /**
     * Level2: Containe the UI for the Explorer component, which allows you to browse your 
     * File System on your server, through your browser. Basically a File System Explorer
     * kind of control, which allows for renaming, deleting, and editing [to some extent]
     * the files in your installation. Can be instantiated in Select mode by setting its
     * 'IsSelect' input parameter. If 'CanCreateNewCssFile' is true, the end user is allowed
     * to create a new default CSS file which he can later edit. 'RootAccessFolder' is the root
     * of the system from where the current user is allowed to browse, while 'Folder' is his
     * current folder. The control does some basic paging and such, and has support for 
     * will raise 'Magix.FileExplorer.GetFilesFromFolder' to get its items. The module will
     * raise the value of the 'SelectEvent' paeameter when an item has been selected.
     * The module supports browsing hierarchical folder structures
     */
    [ActiveModule]
    public class Explorer : ActiveModule
    {
        protected Panel pnl;
        protected Panel prop;
        protected Label header;
        protected Label extension;
        protected Label size;
        protected Label imageWarning;
        protected HyperLink fullUrl;
        protected Button previous;
        protected Button next;
        protected InPlaceEdit name;
        protected Image preview;
        protected Button delete;
        protected Button select;
        protected Label imageSize;
        protected Button newCss;
        protected Uploader uploader;

        public override void InitialLoading(Node node)
        {
            base.InitialLoading(node);

            Load +=
                delegate
                {
                    // The below lines of code must be there to allow the browser to cache the images
                    // to avoid "flickering" during load ... :(
                    DataSource = node;
                    prop.Visible = false;
                    delete.Enabled = false;
                    select.Enabled = false;
                    if (node.Contains("IsSelect") &&
                        node["IsSelect"].Get<bool>())
                    {
                        select.Visible = true;
                    }
                    else
                    {
                        select.Visible = false;
                    }
                    Start = 0;
                    End = 18;
                    newCss.Visible = node.Contains("CanCreateNewCssFile") && node["CanCreateNewCssFile"].Get<bool>();
                };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DataBindFiles();
        }

        private void DataBindFiles()
        {
            DataBindExplorer();
        }

        private int Start
        {
            get { return (int)ViewState["Start"]; }
            set { ViewState["Start"] = value; }
        }

        private int End
        {
            get { return (int)ViewState["End"]; }
            set { ViewState["End"] = value; }
        }

        private void DataBindExplorer()
        {
            int start = Start;
            int end = End;
            previous.Visible = DataSource["Directories"].Count + DataSource["Files"].Count > 17;
            next.Visible = DataSource["Directories"].Count + DataSource["Files"].Count > 17;
            previous.Enabled = Start > 0;
            next.Enabled = End < (DataSource["Directories"].Count + DataSource["Files"].Count + (Start == 0 ? 1 : 0));
            int idxNo = 0;
            if (start == 0)
            {
                if (DataSource["Folder"].Get<string>().Length
                    > DataSource["RootAccessFolder"].Get<string>().Length)
                {
                    Label btn = new Label();
                    btn.ToolTip = "Go up one level ...";
                    Panel p = new Panel();
                    p.Info = "../";
                    p.Click +=
                        delegate(object sender, EventArgs e)
                        {
                            Panel pp = sender as Panel;
                            string folderName = pp.Info;
                            DataSource["FolderToOpen"].Value = folderName;
                            DataSource["Directories"].UnTie();
                            DataSource["Files"].UnTie();
                            RaiseSafeEvent(
                                "Magix.FileExplorer.GetFilesFromFolder",
                                DataSource);
                            DataSource["FolderToOpen"].UnTie();
                            Start = 0;
                            End = 18;
                            ReDataBind();
                        };
                    p.CssClass = "folderUpIcon";
                    p.Controls.Add(btn);
                    pnl.Controls.Add(p);
                    idxNo += 1;
                }
            }
            if (DataSource.Contains("Directories"))
            {
                foreach (Node idx in DataSource["Directories"])
                {
                    if (idxNo >= end)
                        break;
                    if (idxNo >= start)
                    {
                        string name = idx["Name"].Get<string>();
                        Label btn = new Label();
                        btn.Text = name;
                        btn.CssClass = "text";
                        Label icon = new Label();
                        icon.CssClass = "icon";
                        Panel p = new Panel();
                        p.Info = name;
                        p.Click +=
                            delegate(object sender, EventArgs e)
                            {
                                Panel pp = sender as Panel;
                                string folderName = pp.Info;
                                DataSource["FolderToOpen"].Value = folderName;
                                DataSource["Directories"].UnTie();
                                DataSource["Files"].UnTie();

                                RaiseSafeEvent(
                                    "Magix.FileExplorer.GetFilesFromFolder",
                                    DataSource);

                                ReDataBind();
                            };
                        p.CssClass = "folderIcon";
                        if (((idxNo + 1) - start) % 6 == 0)
                            p.CssClass += " lastImage";
                        if (DataSource.Contains("SelectedFile") &&
                            DataSource["SelectedFile"].Get<string>().ToLower() == name.ToLower())
                            p.CssClass += " selected";
                        p.ToolTip = name.Substring(name.LastIndexOf("\\") + 1) + " - Click to Open Folder ...";
                        p.Controls.Add(btn);
                        p.Controls.Add(icon);
                        pnl.Controls.Add(p);
                    }
                    idxNo += 1;
                }
            }
            if (DataSource.Contains("Files"))
            {
                foreach (Node idx in DataSource["Files"])
                {
                    if (idxNo >= end)
                        break;
                    if (idxNo >= start)
                    {
                        if (!(idx.Contains("IsImage") && idx["IsImage"].Get<bool>()))
                        {
                            // Allowing ASCII editing of non-image files ...
                            string name = idx["Name"].Get<string>();
                            Image btn = new Image();
                            btn.AlternateText = name;
                            btn.ImageUrl =
                                "media/images/" +
                                name.Substring(name.LastIndexOf('.') + 1) + ".png";

                            Panel p = new Panel();
                            p.Style[Styles.position] = "relative";
                            Label l = new Label();
                            l.CssClass = "small-image-label";
                            l.Text = name;
                            p.Controls.Add(l);
                            p.CssClass = "imageIcon";
                            if (((idxNo + 1) - start) % 6 == 0)
                                p.CssClass += " lastImage";
                            if (DataSource.Contains("SelectedFile") &&
                                DataSource["SelectedFile"].Get<string>().ToLower() == name.ToLower())
                                p.CssClass += " selected";
                            p.Click +=
                                delegate(object sender, EventArgs e)
                                {
                                    Panel pp = sender as Panel;
                                    pp.CssClass += " viewing";
                                    Panel old = Selector.SelectFirst<Panel>(pp.Parent,
                                        delegate(System.Web.UI.Control idx3)
                                        {
                                            return (idx3 is BaseWebControl) &&
                                                (idx3 as BaseWebControl).Info == SelectedPanelID;
                                        });
                                    if (old != null)
                                        old.CssClass = old.CssClass.Replace(" viewing", "");
                                    SelectedPanelID = pp.Info;
                                    string folderName = pp.Info;
                                    DataSource["File"].Value = folderName;
                                    RaiseSafeEvent(
                                        "Magix.FileExplorer.FileSelected",
                                        DataSource);
                                    UpdateSelectedFile();
                                    new EffectFadeIn(prop, 500)
                                        .Render();
                                    prop.Visible = true;
                                    prop.Style[Styles.display] = "none";
                                };
                            p.Info = name;
                            p.ToolTip = name + " - Click for more options/info ...";
                            p.Controls.Add(btn);
                            pnl.Controls.Add(p);
                        }
                        else
                        {
                            string name = idx["Name"].Get<string>();
                            Image btn = new Image();
                            btn.AlternateText = name;
                            btn.ImageUrl =
                                DataSource["Folder"].Get<string>() +
                                name;

                            Panel p = new Panel();
                            p.Style[Styles.position] = "relative";
                            Label l = new Label();
                            l.CssClass = "small-image-label";
                            l.Text = name;
                            p.Controls.Add(l);
                            if (idx.Contains("Wide") && idx["Wide"].Get<bool>())
                            {
                                p.CssClass = "imageIcon wide";
                            }
                            else
                                p.CssClass = "imageIcon";
                            if (((idxNo + 1) - start) % 6 == 0)
                                p.CssClass += " lastImage";
                            if (DataSource.Contains("SelectedFile") &&
                                DataSource["SelectedFile"].Get<string>().ToLower() == name.ToLower())
                                p.CssClass += " selected";
                            p.Click +=
                                delegate(object sender, EventArgs e)
                                {
                                    Panel pp = sender as Panel;
                                    pp.CssClass += " viewing";
                                    Panel old = Selector.SelectFirst<Panel>(pp.Parent,
                                        delegate(System.Web.UI.Control idx3)
                                        {
                                            return (idx3 is BaseWebControl) &&
                                                (idx3 as BaseWebControl).Info == SelectedPanelID;
                                        });
                                    if (old != null)
                                        old.CssClass = old.CssClass.Replace(" viewing", "");
                                    SelectedPanelID = pp.Info;
                                    string folderName = pp.Info;
                                    DataSource["File"].Value = folderName;
                                    RaiseSafeEvent(
                                        "Magix.FileExplorer.FileSelected",
                                        DataSource);
                                    UpdateSelectedFile();
                                    new EffectFadeIn(prop, 500)
                                        .Render();
                                    prop.Visible = true;
                                    prop.Style[Styles.display] = "none";
                                };
                            p.Info = name;
                            p.ToolTip = name + " - Click for more options/info ...";
                            p.Controls.Add(btn);
                            pnl.Controls.Add(p);
                        }
                    }
                    idxNo += 1;
                }
            }
        }

        protected void preview_Click(object sender, EventArgs e)
        {
            if (DataSource["File"].Contains("IsImage") && DataSource["File"]["IsImage"].Get<bool>())
            {
                OpenFullPreviewOfImage(
                    DataSource["Folder"].Get<string>() +
                    DataSource["File"]["FullName"].Get<string>());
            }
            else
            {
                // Raising the "EditFile" event ...
                Node node = new Node();

                node["File"].Value =
                    DataSource["Folder"].Get<string>() + DataSource["File"]["FullName"].Get<string>();

                RaiseSafeEvent(
                    "Magix.FileExplorer.EditAsciiFile",
                    node);
            }
        }

        protected void delete_Click(object sender, EventArgs e)
        {
            Node node = new Node();
            node["Folder"].Value = DataSource["Folder"].Value;
            node["File"].Value = DataSource["File"]["FullName"].Value;

            DataSource["Directories"].UnTie();
            DataSource["Files"].UnTie();

            RaiseSafeEvent(
                "Magix.FileExplorer.DeleteFile",
                node);

            DataSource["FolderToOpen"].Value = "";

            RaiseSafeEvent(
                "Magix.FileExplorer.GetFilesFromFolder",
                DataSource);

            ReDataBind();
        }

        protected void newCss_Click(object sender, EventArgs e)
        {
            string folder = DataSource["Folder"].Get<string>();
            string path = Page.Server.MapPath("~/" + folder + "empty.css");
            if (File.Exists(path))
            {
                Node n = new Node();
                n["Message"].Value = "Couldn't create file! You must rename the existing 'empty.css' file before you can create new CSS files.";

                RaiseSafeEvent(
                    "Magix.Core.ShowMessage",
                    n);
            }
            else
            {
                using (TextWriter writer = File.CreateText(path))
                {
                    writer.Write(@"
/*
 * Magix Generated CSS file,
 * please modify to fit your needs ...
 */
");
                }
                DataSource["FolderToOpen"].Value = "";
                DataSource["File"].UnTie();
                DataSource["Files"].UnTie();

                RaiseSafeEvent(
                    "Magix.FileExplorer.GetFilesFromFolder",
                    DataSource);
                ReDataBind();

                SelectedPanelID = "empty.css";
                DataSource["File"].Value = "empty.css";

                RaiseSafeEvent(
                    "Magix.FileExplorer.FileSelected",
                    DataSource);

                UpdateSelectedFile();

                new EffectFadeIn(prop, 500)
                    .Render();

                prop.Visible = true;
                prop.Style[Styles.display] = "none";

                Panel pl = Selector.SelectFirst<Panel>(this,
                    delegate(Control idx)
                    {
                        return (idx is BaseWebControl) &&
                            (idx as BaseWebControl).Info == SelectedPanelID;
                    });
                if (pl != null)
                    pl.CssClass += " viewing";
            }
        }

        protected void select_Click(object sender, EventArgs e)
        {
            Node node = new Node();
            node["FileName"].Value = DataSource["File"]["FullName"].Value;
            if (DataSource.Contains("Seed"))
                node["Seed"].Value = DataSource["Seed"].Value;
            node["Folder"].Value = DataSource["Folder"].Value;
            node["Params"].AddRange(DataSource["SelectEvent"]["Params"]);

            RaiseSafeEvent(
                DataSource["SelectEvent"].Get<string>(),
                node);
        }

        protected void uploader_Uploaded(object sender, EventArgs e)
        {
            string filBase64 = uploader.GetFileRawBASE64();
            string fileName = uploader.GetFileName();

            using (FileStream writer = 
                File.Create(
                    MapPath(
                        "~/" + 
                        DataSource["Folder"].Get<string>() + 
                        fileName)))
            {
                byte[] bytes = Convert.FromBase64String(filBase64);
                writer.Write(bytes, 0, bytes.Length);
            }

            DataSource["FolderToOpen"].Value = "";
            DataSource["File"].UnTie();
            DataSource["Files"].UnTie();

            RaiseSafeEvent(
                "Magix.FileExplorer.GetFilesFromFolder",
                DataSource);

            ReDataBind();

            SelectedPanelID = fileName;
            DataSource["File"].Value = fileName;

            RaiseSafeEvent(
                "Magix.FileExplorer.FileSelected",
                DataSource);

            UpdateSelectedFile();

            new EffectFadeIn(prop, 500)
                .Render();

            prop.Visible = true;
            prop.Style[Styles.display] = "none";

            Panel pl = Selector.SelectFirst<Panel>(this,
                delegate(Control idx)
                {
                    return (idx is BaseWebControl) &&
                        (idx as BaseWebControl).Info == SelectedPanelID;
                });

            if (pl != null)
                pl.CssClass += " viewing";
        }

        [WebMethod]
        protected void SubmitFile(string fileName, string fileBase64Content)
        {
            string webServerApp = Server.MapPath("~/");
            if (DataSource.Contains("Filter") && 
                DataSource["Filter"].Get<string>().Trim().Length > 0)
            {
                bool message = true;
                foreach (string idx in 
                    DataSource["Filter"].Get<string>().Trim().Split(';'))
                {
                    if (idx == null)
                        continue;
                    if (idx == "*.*")
                    {
                        message = false;
                        break;
                    }
                    if (fileName.Substring(
                        fileName.LastIndexOf(".") + 1).
                        Contains(idx.Replace("*.", "")))
                    {
                        message = false;
                        break;
                    }
                }
                if (message)
                {
                    Node node = new Node();
                    node["Message"].Value = "You cannot save files of that type...";

                    RaiseSafeEvent(
                        "Magix.Core.ShowMessage",
                        node);
                    return;
                }
            }

            using (Stream writer = File.Create(
                webServerApp + DataSource["Folder"].Get<string>().Replace("/", "\\") +
                fileName))
            {
                byte[] bytes = Convert.FromBase64String(fileBase64Content.Substring(fileBase64Content.IndexOf(",") + 1));
                writer.Write(bytes, 0, bytes.Length);
            }

            DataSource["FolderToOpen"].Value = "";
            DataSource["File"].UnTie();
            DataSource["Files"].UnTie();

            RaiseSafeEvent(
                "Magix.FileExplorer.GetFilesFromFolder",
                DataSource);

            ReDataBind();

            SelectedPanelID = fileName;
            DataSource["File"].Value = fileName;

            RaiseSafeEvent(
                "Magix.FileExplorer.FileSelected",
                DataSource);

            UpdateSelectedFile();

            new EffectFadeIn(prop, 500)
                .Render();

            prop.Visible = true;
            prop.Style[Styles.display] = "none";

            Panel pl = Selector.SelectFirst<Panel>(this,
                delegate(Control idx)
                {
                    return (idx is BaseWebControl) &&
                        (idx as BaseWebControl).Info == SelectedPanelID;
                });

            if (pl != null)
                pl.CssClass += " viewing";
        }

        [ActiveEvent(Name = "Magix.FileExplorer.FileSelected")]
        protected void Magix_FileExplorer_FileSelected(object sender, ActiveEventArgs e)
        {
            new EffectTimeout(100)
                .ChainThese(
                    new EffectFocusAndSelect(select))
                .Render();
        }

        private void UpdateSelectedFile()
        {
            if (!DataSource.Contains("IsDelete") ||
                DataSource["IsDelete"].Get<bool>())
                delete.Enabled = true;
            select.Enabled = true;
            header.Text = "Name: " + DataSource["File"]["Name"].Get<string>();
            extension.Text = "Extension: " + DataSource["File"]["Extension"].Get<string>();
            name.Text = DataSource["File"]["Name"].Get<string>() + 
                    DataSource["File"]["Extension"].Get<string>();
            name.Info = DataSource["File"]["FullName"].Get<string>();
            preview.AlternateText = DataSource["File"]["Name"].Get<string>();

            string imageUrl = "";

            if (DataSource["File"].Contains("IsImage") && DataSource["File"]["IsImage"].Get<bool>())
            {
                imageUrl = DataSource["Folder"].Get<string>() +
                    DataSource["File"]["FullName"].Get<string>();
                preview.ToolTip = "Click Image to view full-size ...";
                preview.Visible = true;
            }
            else
            {
                switch(DataSource["File"]["FullName"].Get<string>().Substring(DataSource["File"]["FullName"].Get<string>().LastIndexOf('.') + 1))
                {
                    case "css":
                    case "txt":
                    case "config":
                    case "xml":
                    case "json":
                    case "html":
                    case "cs":
                        imageUrl = "media/images/" +
                            DataSource["File"]["FullName"].Get<string>().Substring(DataSource["File"]["FullName"].Get<string>().LastIndexOf('.') + 1) + ".png";
                        preview.ToolTip = "Click file to edit in ASCII editor ...";
                        preview.Visible = true;
                        break;
                    default:
                        preview.Visible = false;
                        break;
                }
            }

            preview.ImageUrl = imageUrl;
            preview.CssClass =
                DataSource["Files"][DataSource["File"]["FullName"].Get<string>()]
                .Contains("Wide") &&
                DataSource["Files"][DataSource["File"]["FullName"].Get<string>()]["Wide"]
                .Get<bool>() ? 
                    "span-4 preview wide" : 
                    "span-4 preview";
            size.Text = "Size: " +
                (((double)DataSource["File"]["Size"].Get<long>()) / 1024D)
                .ToString("###,###,###,##0.0", CultureInfo.InvariantCulture) +
                "KB";
            fullUrl.Text = " Full URL: " +
                DataSource["Folder"].Get<string>() +
                DataSource["File"]["FullName"].Get<string>();
            fullUrl.URL = DataSource["Folder"].Get<string>() +
                DataSource["File"]["FullName"].Get<string>();
            if (DataSource["File"].Contains("IsImage") && DataSource["File"]["IsImage"].Get<bool>())
            {
                int width = DataSource["File"]["ImageWidth"].Get<int>();
                int height = DataSource["File"]["ImageHeight"].Get<int>();
                int optimalWidth = width - (width < 30 ? -(30 - width) : ((width + 10) % 40));
                int optimalHeight = height - (height < 18 ? -(18 - height) : height % 18);
                if ((width + 10) % 40 != 0 || height % 18 != 0 || width > 950)
                {
                    string imageSizeText =
                        width +
                        "px - " +
                        height +
                        "px - optimal " +
                        optimalWidth +
                        "x" +
                        optimalHeight;
                    imageSize.Text = imageSizeText;
                }
                else
                {
                    string imageSizeText =
                        width +
                        "px - " +
                        height +
                        "px - grids " +
                        (width + 10) / 40 +
                        "x" +
                        height / 18;
                    imageSize.Text = imageSizeText;
                }
            }
        }

        protected void name_TextChanged(object sender, EventArgs e)
        {
            string newName = name.Text.Replace(DataSource["File"]["Extension"].Get<string>(), "");
            string oldName = (sender as InPlaceEdit).Info;
            Node node = DataSource;
            node["Directories"].UnTie();
            node["Files"].UnTie();
            node["NewName"].Value = newName;
            node["OldName"].Value = oldName;

            RaiseSafeEvent(
                "Magix.FileExplorer.ChangeFileName",
                node);

            node["NewName"].UnTie();
            node["OldName"].UnTie();
            SelectedPanelID = newName + oldName.Substring(oldName.LastIndexOf("."));
            ReDataBind();
            Panel pl = Selector.SelectFirst<Panel>(this,
                delegate(Control idx)
                {
                    return (idx is BaseWebControl) &&
                        (idx as BaseWebControl).Info == SelectedPanelID;
                });
            if (pl != null)
                pl.CssClass += " viewing";
            UpdateSelectedFile();
            prop.Visible = true;
        }

        private void OpenFullPreviewOfImage(string file)
        {
            Node node = new Node();

            int width = DataSource["File"]["ImageWidth"].Get<int>() + 80;
            width += 40 - ((width + 10) % 40);
            node["ForcedSize"]["width"].Value = width;
            int height = DataSource["File"]["ImageHeight"].Get<int>() + 90;
            height += 18 - (height % 18);
            node["ForcedSize"]["height"].Value = height + 2;
            node["ImageURL"].Value = file;
            node["Top"].Value = 10;
            node["Push"].Value = 3;
            node["SetFocus"].Value = true;
            node["ToolTip"].Value = 
                @"Grid is optimal size(s) of image to work perfectly with the Typography 
Layout System in our WinePad product ...";
            node["AlternateText"].Value = "Preview of image in full size...";
            node["DynCssClass"].Value = "showgrid";
            node["Caption"].Value =
                "Preview";

            ActiveEvents.Instance.RaiseLoadControl(
                "Magix.Brix.Components.ActiveModules.CommonModules.ImageModule",
                "child",
                node);
        }

        protected void previous_Click(object sender, EventArgs e)
        {
            int delta = End - Start;
            if (Start == 17)
                delta -= 1;
            Start -= delta;
            End -= delta;
            pnl.Controls.Clear();
            DataBindExplorer();
            pnl.ReRender();
            prop.Visible = false;
            delete.Enabled = false;
            select.Enabled = false;
        }

        protected void next_Click(object sender, EventArgs e)
        {
            int delta = End - Start;
            if (Start == 0)
                delta -= 1;
            Start += delta;
            End += delta;
            pnl.Controls.Clear();
            DataBindExplorer();
            pnl.ReRender();
            prop.Visible = false;
            delete.Enabled = false;
            select.Enabled = false;
        }

        private void ReDataBind()
        {
            // The below lines of code must be there to allow the browser to cache the images
            // to avoid "flickering" during load ... :(
            pnl.Style[Styles.display] = "block";
            prop.Visible = false;
            delete.Enabled = false;
            select.Enabled = false;
            pnl.Controls.Clear();
            DataBindExplorer();
            pnl.ReRender();
            if (Parent.Parent.Parent is Window)
            {
                (Parent.Parent.Parent as Window).Caption = 
                    DataSource["Caption"].Get<string>();
            }
            else
            {
                //Node node = new Node();
                //node["Caption"].Value = DataSource["Caption"].Get<string>();

                //RaiseSafeEvent(
                //    "Magix.Core.SetFormCaption",
                //    node);
            }
        }

        private string SelectedPanelID
        {
            get { return ViewState["SelectedPanelID"] as string; }
            set { ViewState["SelectedPanelID"] = value; }
        }
    }
}
