﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using Magix.Brix.Loader;
using Magix.Brix.Types;
using Magix.Brix.Components.ActiveTypes.TalkBack;
using Magix.Brix.Components.ActiveTypes.Users;
using Magix.Brix.Data;
using System.Web;
using Magix.Brix.Components.ActiveTypes;

namespace Magix.Brix.Components.ActiveControllers.TalkBack
{
    /**
     * Level2: Contains helper logic for the TalkBack module [forums] in Magix
     */
    [ActiveController]
    public class TalkBack_Controller : ActiveController
    {
        /**
         * Level3: Will fetch the top 5 postings with the most recent activity within
         */
        [ActiveEvent(Name = "Magix.Talkback.GetPostings")]
        protected void Magix_Talkback_GetPostings(object sender, ActiveEventArgs e)
        {
            Node node = e.Params;
            if (node == null)
                node = new Node();

            // Currently open item ...
            if (e.Params.Contains("Active"))
            {
                node["Active"].Value = e.Params["Active"].Get<int>();
            }

            int idxNo = 0;
            foreach (Posting idxReal in Posting.Select(Criteria.Sort("When", false)))
            {
                Posting idx = idxReal;
                if (idx.Parent != null)
                {
                    idx = idx.Parent;
                    if (node["Posts"].Contains("P" + idx.ID))
                        continue; // This one has been handled before ...
                }

                idxNo += 1;

                string content = idx.Content;
                content = content.Replace("\r\n", "\n");
                content = content.Replace("\n\n", "</p><p>");
                content = "<p>" + content + "</p>";
                while (true)
                {
                    if (content.Contains("</p><p></p><p>"))
                    {
                        content = content.Replace("</p><p></p><p>", "</p><p>");
                    }
                    else 
                        break;
                }
                content = content.Replace("\n", "<br />");
                content = content.Replace("<p><br />", "<p>");
                node["Posts"]["P" + idx.ID]["Header"].Value =
                    string.IsNullOrEmpty(idx.Header) ? "[empty]" : idx.Header;
                node["Posts"]["P" + idx.ID]["Content"].Value = content;
                node["Posts"]["P" + idx.ID]["ID"].Value = idx.ID;
                node["Posts"]["P" + idx.ID]["User"].Value = idx.User == null ? "[null]" : idx.User.Username;
                node["Posts"]["P" + idx.ID]["Date"].Value = idx.When;
                foreach (Posting idxChild in idx.Children)
                {
                    content = idxChild.Content;
                    content = content.Replace("\r\n", "\n");
                    content = content.Replace("\n", "</p><p>");
                    content = "<p>" + content + "</p>";
                    node["Posts"]["P" + idx.ID]["Children"]["P" + idxChild.ID]["Header"].Value = idxChild.Header;
                    node["Posts"]["P" + idx.ID]["Children"]["P" + idxChild.ID]["Content"].Value = content;
                    node["Posts"]["P" + idx.ID]["Children"]["P" + idxChild.ID]["ID"].Value = idxChild.ID;
                    node["Posts"]["P" + idx.ID]["Children"]["P" + idxChild.ID]["User"].Value = idxChild.User == null ? "[null]" : idxChild.User.Username;
                    node["Posts"]["P" + idx.ID]["Children"]["P" + idxChild.ID]["Date"].Value = idxChild.When;
                }
                if (idxNo > 6)
                    break;
            }
        }

        /**
         * Level2: Will launch the Talkback Module and allow for posting questions and such
         */
        [ActiveEvent(Name = "Magix.TalkBack.LaunchForum")]
        protected void Magix_TalkBack_LaunchForum(object sender, ActiveEventArgs e)
        {
            Node header = new Node();
            header["Caption"].Value = "TalkBack ...";

            if (e.Params.Contains("Caption"))
                header["Caption"].Value = e.Params["Caption"].Get<string>();

            RaiseEvent(
                "Magix.Core.SetFormCaption",
                header);

            RaiseEvent(
                "Magix.Talkback.GetPostings",
                e.Params);

            LoadModule(
                "Magix.Brix.Components.ActiveModules.TalkBack.Forum",
                "content3",
                e.Params);
        }

        /**
         * Level3: Will create a new posting with the Header and Body combination linked to the User.Current.
         * If Parent, will put it into the Parent as a Reply
         */
        [ActiveEvent(Name = "Magix.Talkback.CreatePost")]
        protected void Magix_Talkback_CreatePost(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                string header = e.Params["Header"].Get<string>();
                header = header.Replace("<", "&lt;").Replace(">", "&gt;");

                string body = e.Params["Body"].Get<string>();
                body = body.Replace("<", "&lt;").Replace(">", "&gt;");

                Posting p = new Posting();
                p.Header = header;
                p.Content = body;
                if (e.Params.Contains("Parent"))
                {
                    Posting parent = Posting.SelectByID(e.Params["Parent"].Get<int>());
                    p.Parent = parent;
                    parent.Children.Add(p);
                    parent.Save();
                }
                else
                {
                    p.Save();
                }
                tr.Commit();

                string content = p.Content;
                content = content.Replace("\r\n", "\n");
                content = content.Replace("\n", "</p><p>");
                content = "<p>" + content + "</p>";
                while (true)
                {
                    if (content.Contains("</p><p></p><p>"))
                    {
                        content = content.Replace("</p><p></p><p>", "</p><p>");
                    }
                    else
                        break;
                }

                foreach (UserBase idx in UserBase.Select())
                {
                    if (!string.IsNullOrEmpty(idx.Email) && 
                        idx.GetSetting("GlobalForumEmailNotifications", false))
                    {
                        string urlOfForum = 
                            HttpContext.Current.Request.Url.ToString().ToLower()
                            .Replace("default.aspx", "");
                        if (p.Parent != null)
                        {
                            if (urlOfForum.Contains("?"))
                                urlOfForum += "&TalkBack=" + p.Parent.ID;
                            else
                                urlOfForum += "?TalkBack=" + p.Parent.ID;
                        }
                        else
                        {
                            if (urlOfForum.Contains("?"))
                                urlOfForum += "&TalkBack=" + p.ID;
                            else
                                urlOfForum += "?TalkBack=" + p.ID;
                        }
                        Node node = new Node();
                        node["Header"].Value = "New posting at TalkBack  ...";
                        node["Body"].Value = "<html><body>" + string.Format(@"
<p><em style=""color:#999;"">New posting at the TalkBack forums, <a href=""{0}"">click here</a> to go to the new posting...</em></p>
<h3>{1}</h3>
{2}
",
                            urlOfForum,
                            p.Header,
                            content) + "</body></html>";
                        node["AdminEmail"].Value = 
                            Settings.Instance.Get("TalkBackForumsAdminEmail", "rick.gibson@winergyinc.com");
                        node["AdminEmailFrom"].Value =
                            Settings.Instance.Get("TalkBackForumsAdminEmailFrom", "Rick Gibson");
                        node["EmailAddresses"]["Address1"].Value = idx.Email;
                        ActiveEvents.Instance.RaiseActiveEvent(
                            this,
                            "WineTasting.SendEmailLocally",
                            node);
                    }
                }
            }
        }
    }
}
