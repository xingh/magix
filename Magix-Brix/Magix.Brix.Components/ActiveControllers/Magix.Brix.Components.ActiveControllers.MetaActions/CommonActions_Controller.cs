﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using Magix.Brix.Loader;
using Magix.Brix.Types;
using Magix.Brix.Components.ActiveTypes.MetaTypes;
using Magix.Brix.Data;
using Magix.UX.Widgets;
using Magix.Brix.Components.ActiveTypes.Publishing;
using System.Collections.Generic;
using System.Globalization;
using Magix.Brix.Components.ActiveTypes.MetaViews;
using System.IO;
using Magix.UX;
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;
using System.Collections;

namespace Magix.Brix.Components.ActiveControllers.MetaTypes
{
    /**
     * Level2: Contains common end user useful actions which doesn't really belong 
     * any particular place, but which can still be immensily useful for
     * scripting purposes. Perceive these as pugins, or extra 
     * candy if you wish. Often they're simplifications of other more
     * hard core Active Events. This is one of the most interesting classes for you 
     * to look at in order to figure out what events to start using
     */
    [ActiveController]
    public class CommonActions_Controller : ActiveController
    {
        /**
         * Level2: Returns the 'Name' setting for the current logged in User. Sets the
         * setting to 'Default' if no existing value is found. Both are mandatory params.
         * Returns new value as 'Value'
         */
        [ActiveEvent(Name = "Magix.Common.GetUserSetting")]
        protected void Magix_Common_GetUserSetting(object sender, ActiveEventArgs e)
        {
            string val = User.Current.GetSetting<string>(
                e.Params["Name"].Get<string>(),
                e.Params["Default"].Value.ToString());

            e.Params["Value"].Value = val;
        }

        /**
         * Level2: Sets the given setting 'Name' to the given value 'Value' for the currently
         * logged in User
         */
        [ActiveEvent(Name = "Magix.Common.SetUserSetting")]
        protected void Magix_Common_SetUserSetting(object sender, ActiveEventArgs e)
        {
            User.Current.SetSetting<string>(
                e.Params["Name"].Get<string>(),
                e.Params["Value"].Get<string>());
        }

        /**
         * Level2: Will add the given 'Value' with the given 'Key' to the Caching.Cache Page object. If
         * the Value of 'Value' contains anything but null or '' it will be serialized as JSON, otherwise
         * the Value object of the given node will be serialized
         */
        [ActiveEvent(Name = "Magix.Common.AddToCache")]
        protected void Magix_Common_AddToCache(object sender, ActiveEventArgs e)
        {
            if (!e.Params.Contains("Key") && !e.Params.Contains("Value"))
                throw new ArgumentException("You need to specify what 'Key' and 'Value' you wish to add ...");

            if (!string.IsNullOrEmpty(e.Params["Value"].Get<string>()))
            {
                // Some funky object attempted being added
                Page.Cache[e.Params["Key"].Get<string>()] = e.Params["Value"].Value;
            }
            else
            {
                // Assuming serialization of entire node structure ...
                Page.Cache[e.Params["Key"].Get<string>()] = Node.FromJSONString(e.Params["Value"].ToJSONString());
            }
        }

        /**
         * Level2: Will return either the 'Key' which must be exact match, or the first item of 'Contains'
         * that contains the given string as its key
         */
        [ActiveEvent(Name = "Magix.Common.GetFromCache")]
        protected void Magix_Common_GetFromCache(object sender, ActiveEventArgs e)
        {
            if (!e.Params.Contains("Key") && !e.Params.Contains("Contains"))
                throw new ArgumentException("You need to specify what 'Key' you wish to remove ...");

            e.Params["Value"].UnTie();

            if (e.Params.Contains("Key"))
            {
                object tmp = Page.Cache.Get(e.Params["Key"].Get<string>());
                if (tmp != null)
                {
                    if (tmp is string)
                    {
                        e.Params["Value"].Value = Node.FromJSONString(tmp as string);
                    }
                    else
                        e.Params["Value"].Value = tmp;
                }
            }
            else
            {
                IDictionaryEnumerator enume = Page.Cache.GetEnumerator();
                while (enume.MoveNext())
                {
                    if (enume.Key.ToString().Contains(e.Params["Contains"].Get<string>()))
                    {
                        e.Params["Key"].Value = enume.Key;
                        if (enume.Value is string)
                        {
                            e.Params["Value"].Value = Node.FromJSONString(enume.Value as string);
                        }
                        else
                            e.Params["Value"].Value = enume.Value;
                        return;
                    }
                }
            }
        }

        /**
         * Level2: Will remove the given 'Key' item from your Caching. Cache object 
         * associated with the Page object. If 'Key' is a direct hit, it will only remove 
         * one entry, if not, it will remove everything containing 'Key' from its cache
         */
        [ActiveEvent(Name = "Magix.Common.RemoveFromCache")]
        protected void Magix_Common_RemoveFromCache(object sender, ActiveEventArgs e)
        {
            if (!e.Params.Contains("Key"))
                throw new ArgumentException("You need to specify what 'Key' you wish to remove ...");

            if (Page.Cache.Get(e.Params["Key"].Get<string>()) != null)
            {
                Page.Cache.Remove(e.Params["Key"].Get<string>());
            }
            else
            {
                string key = e.Params["Key"].Get<string>();
                List<string> keys = new List<string>();
                IDictionaryEnumerator enume = Page.Cache.GetEnumerator();
                while (enume.MoveNext())
                {
                    if (enume.Key.ToString().Contains(key))
                        keys.Add(enume.Key.ToString());
                }
                for (int i = 0; i < keys.Count; i++)
                {
                    Page.Cache.Remove(keys[i]);
                }
            }
        }

        /**
         * Level2: Clears the Caching.Cache associated with the Page object
         */
        [ActiveEvent(Name = "Magix.Common.ClearCache")]
        protected void Magix_Common_ClearCache(object sender, ActiveEventArgs e)
        {
            List<string> keys = new List<string>();
            IDictionaryEnumerator enume = Page.Cache.GetEnumerator();
            while (enume.MoveNext())
            {
                keys.Add(enume.Key.ToString());
            }
            for (int i = 0; i < keys.Count; i++)
            {
                Page.Cache.Remove(keys[i]);
            }
        }

        /**
         * Level2: Executes the given node recursively. This is the Magix Turing Executor which 
         * allows for logic to be implemented without having to code. The Magix Turing Executor 
         * can modify code at its instruction pointer if you wish. You can create selfmodifiable 
         * code which changes the code as it is being executed almost. Make sure you understand
         * Expressions to get the most out of the Magix Turing Executor. @executor
         */
        [ActiveEvent(Name = "Magix.System.execute")]
        protected void Magix_System_execute(object sender, ActiveEventArgs e)
        {
            List<string> keyWords = new List<string>();
            List<string> buffer = new List<string>();
            bool hasFound = false;
            int length = e.Params.Count;
            for (int idxNo = 0; idxNo < length; idxNo++)
            {
                Node idx = e.Params[idxNo];
                keyWords.Add(idx.Name);
                switch (idx.Name)
                {
                    case "execute":
                        RaiseEvent(
                            "Magix.System.execute",
                            idx);
                        idx.UnTie();
                        break;
                    case "clean":
                        RaiseEvent(
                            "Magix.System.untie",
                            idx);
                        break;
                    case "set":
                        RaiseEvent(
                            "Magix.System.set",
                            idx);
                        break;
                    case "untie":
                        RaiseEvent(
                            "Magix.System.untie",
                            idx);
                        break;
                    case "if":
                        if (CheckStatement(idx.Get<string>(), idx))
                        {
                            hasFound = true;
                            RaiseEvent(
                                "Magix.System.if",
                                idx);
                        }
                        else
                            hasFound = false;
                        break;
                    case "else if":
                        if (!hasFound)
                        {
                            if (keyWords[keyWords.Count - 2] == "if" ||
                                keyWords[keyWords.Count - 2] == "else if")
                            {
                                if (CheckStatement(idx.Get<string>(), idx))
                                {
                                    hasFound = true;
                                    RaiseEvent(
                                        "Magix.System.else if",
                                        idx);
                                }
                            }
                            else
                            {
                                throw new ArgumentException("You can't have an 'else if' without an 'if' or an 'else if' in front of it");
                            }
                        }
                        break;
                    case "else":
                        if (!hasFound)
                        {
                            if (keyWords[keyWords.Count - 2] == "if" ||
                                keyWords[keyWords.Count - 2] == "else if")
                            {
                                RaiseEvent(
                                    "Magix.System.else",
                                    idx);
                            }
                            else
                            {
                                throw new ArgumentException("You can't have an 'else' without an 'if' or an 'else if' in front of it");
                            }
                        }
                        break;
                    case "raise":
                        RaiseEvent(
                            "Magix.System.raise",
                            idx);
                        break;
                    case "throw":
                        RaiseEvent(
                            "Magix.System.throw",
                            idx);
                        break;
                    case "foreach":
                        RaiseEvent(
                            "Magix.System.foreach",
                            idx);
                        break;
                    case "when":
                        RaiseEvent(
                            "Magix.System.when",
                            idx);
                        break;
                    default:
                        if (idx.Name.StartsWith("@"))
                        {
                            RaiseEvent(
                                "Magix.Dynamic.Evaluate." + idx.Name,
                                idx);
                        }
                        break;
                }
                length = e.Params.Count;
            }
        }

        /**
         * Level2: Expects two values, the first parameter must must be an expression, the 
         * second parameter might either be an expression or a fixed static value. The two 
         * parameters are separated by '=' and expected to be found in the Value of the current
         * 'set' node. Warning, this will create the left hand parameter node path if not 
         * existing, and it will also set the value of that path to null of no right hand side
         * value is found. The left hand side of your component expression can be either 
         * '.Value', '.Name' or a resulting Node list. If the left hand side is a Node list
         * Expression, then all previous nodes in the Expression result, if any, will 
         * be removed, and all nodes from the right hand side result will be 
         * cloned [deep copy] and put into the left hand side's Node list
         */
        [ActiveEvent(Name = "Magix.System.set")]
        protected void Magix_System_set(object sender, ActiveEventArgs e)
        {
            CheckStatement(e.Params.Get<string>(), e.Params);
        }

        /**
         * Level2: Branching according to statement contained within 'if' Value node.
         * 'if' can take either one or three components. If one components is given, the if will
         * evaluate to true if the components contains anything but "" and null. If 3 components are
         * given, it will evaluate to true depending upon the comparison operator 
         * that must be the second components. Legal operators are '&gt;', '&lt;', 
         * '==', '!=', '&gt;=' and '&lt;='. The quotes are not a parts of the components, 
         * and only here for illustrative purposes. All comparisons are done as if both sides were
         * strings, meaning 4 will be evaluated more than 33. So numbers cannot be correctly 
         * compared without additional logic on your behalf for instance. 'if' will if it 
         * evaluates to true work like an 'execute' scope, except it will not untie itself in
         * any ways after execution
         */
        [ActiveEvent(Name = "Magix.System.if")]
        protected void Magix_System_if(object sender, ActiveEventArgs e)
        {
            ExecuteNode(e.Params);
        }

        /**
         * Level2: Branching according to statement contained within 'else if' Value node.
         * 'else if' can take either one or three components. If one components is given, the else if will
         * evaluate to true if the components contains anything but "" and null. If 3 components are
         * given, it will evaluate to true depending upon the comparison operator 
         * that must be the second components. Legal operators are '&gt;', '&lt;', 
         * '==', '!=', '&gt;=' and '&lt;='. The quotes are not a parts of the components, 
         * and only here for illustrative purposes. All comparisons are done as if both sides were
         * strings, meaning 4 will be evaluated more than 33. So numbers cannot be correctly 
         * compared without additional logic on your behalf for instance. 'else if' must directly 
         * follow either another 'else if' or an 'if'. the 'else if' will only be tried if 
         * none of the previous 'if' and 'else if' did not evaluate to true. 'else if' will if it 
         * evaluates to true work like an 'execute' scope, except it will not untie itself in
         * any ways after execution
         */
        [ActiveEvent(Name = "Magix.System.else if")]
        protected void Magix_System_else_if(object sender, ActiveEventArgs e)
        {
            ExecuteNode(e.Params);
        }

        /**
         * Level2: Branching according to statement contained within previous 'if' or 'else if'.
         * The 'else' will only be evaluated if none of the previous 'else if' or 'if' evaluated 
         * to true. 'else' will if it 
         * is being executed work like an 'execute' scope, except it will not untie itself in
         * any ways after execution
         */
        [ActiveEvent(Name = "Magix.System.else")]
        protected void Magix_System_else(object sender, ActiveEventArgs e)
        {
            ExecuteNode(e.Params);
        }

        /**
         * Level2: Raises the Active Event defined in the Value of the node. If you 
         * concatenate an Expression inside of normal parantheses, e.g.; 
         * 'Magix.Core.ShowMessage({root[MessageNode]})', then the given 
         * value Node list of your Expression, inside of your parantheses, will be
         * passed in instead of the default which is the 'raise' node itself
         */
        [ActiveEvent(Name = "Magix.System.raise")]
        protected void Magix_System_raise(object sender, ActiveEventArgs e)
        {
            string evtName = e.Params.Get<string>();
            Node node = e.Params;
            if (evtName.Contains("("))
            {
                string exp = evtName.Split('(')[1].Split(')')[0];
                evtName = evtName.Substring(0, evtName.IndexOf('(')).Trim();
                Node pre = node;
                node = CommonActions_Controller.GetExpressionValue(exp, node) as Node;
                if (node == null)
                    throw new ArgumentException("Sorry, but your Node expression didn't evaluate to a valid Node: '" + exp + "', current node: " + pre.Name);
            }
            RaiseEvent(
                evtName,
                node);
        }

        /**
         * Level2: Raises an ApplicationException with the error message of the exception being 
         * the Value of the node. Will instantly stop execution of everything, unwind the stack, 
         * probably log some error message, and return control back to the caller
         */
        [ActiveEvent(Name = "Magix.System.throw")]
        protected void Magix_System_throw(object sender, ActiveEventArgs e)
        {
            throw new ApplicationException(e.Params.Get<string>());
        }

        /**
         * Level2: Loops through all nodes in Expression and raises 'execute' with every one as 
         * a 'this' instance. 'foreach' needs an Expression, which must return a Node list,
         * in its Value part. This node list will be iterated upon, and the content of the 'foreach'
         * will function as an execute block, though without any untie, once for every single node in
         * your result set of the Expression within the 'foreach' Value. The current iterator object 
         * of your foreach iteration can be de-referenced using 'idx', e.g. '{idx[Name].Value}'
         */
        [ActiveEvent(Name = "Magix.System.foreach")]
        protected void Magix_System_foreach(object sender, ActiveEventArgs e)
        {
            Node node = CommonActions_Controller.GetExpressionValue(e.Params.Get<string>(), e.Params) as Node;

            foreach (Node idx in node)
            {
                object tmp = null;
                if (e.Params.Contains("idx"))
                    tmp = e.Params["idx"].Value;
                e.Params["idx"].Value = idx;
                try
                {
                    RaiseEvent(
                        "Magix.System.execute",
                        e.Params);
                }
                finally
                {
                    if (tmp != null)
                        e.Params["idx"].Value = tmp;
                    else
                        e.Params["idx"].UnTie();
                }
            }
        }

        /**
         * Level2: Branching according to statement contained within 'while' Value node.
         * 'while' can take either one or three components. If one components is given, the while will
         * evaluate to true if the components contains anything but "" and null. If 3 components are
         * given, it will evaluate to true depending upon the comparison operator 
         * that must be the second components. Legal operators are '&gt;', '&lt;', 
         * '==', '!=', '&gt;=' and '&lt;='. The quotes are not a parts of the components, 
         * and only here for illustrative purposes. All comparisons are done as if both sides were
         * strings, meaning 4 will be evaluated more than 33. So numbers cannot be correctly 
         * compared without additional logic on your behalf for instance. 'while' will if it 
         * evaluates to true work like an 'execute' scope, except it will not untie itself in
         * any ways after execution. When it is done executing, it will re-evaluate its Value 
         * Expression and start all over again if the Expression still evaluates to true
         */
        [ActiveEvent(Name = "Magix.System.while")]
        protected void Magix_System_while(object sender, ActiveEventArgs e)
        {
            bool res = ObjectWhenExpressionIsTrue(e.Params.Get<string>(), e.Params);

            while (res)
            {
                RaiseEvent(
                    "Magix.System.execute",
                    e.Params);
                res = ObjectWhenExpressionIsTrue(e.Params.Get<string>(), e.Params);
            }
        }

        private bool ObjectWhenExpressionIsTrue(string exp, Node node)
        {
            return new Expression(exp, node).Compute();
        }

        private void ExecuteNode(Node node)
        {
            RaiseEvent(
                "Magix.System.execute",
                node);
        }

        private class Expression
        {
            public Expression(string expr, Node node)
            {
                RawExpression = expr;
                Node = node;
            }

            public readonly string RawExpression;

            public readonly Node Node;

            public bool Compute()
            {
                List<string> components = new List<string>(GetComponents());
                return Compute(components);
            }

            private IEnumerable<string> GetComponents()
            {
                List<string> r = new List<string>();
                string tmp = RawExpression;
                tmp.Trim();

                string expr1 = null;
                string expr2 = null;
                string expr3 = null;
                string buffer = "";

                foreach (char idx in tmp)
                {
                    buffer += idx;
                    if (idx == '}')
                    {
                        if (expr1 == null)
                        {
                            expr1 = buffer;
                            buffer = "";
                        }
                        else if (expr2 == null)
                        {
                            expr2 = buffer;
                            buffer = "";
                        }
                    }
                    else if (idx == ' ' && buffer.Trim().Length > 0)
                    {
                        if (expr1 != null && expr2 == null)
                        {
                            expr2 = buffer.Trim();
                            buffer = "";
                            expr3 = "";
                        }
                    }
                }

                if (buffer != null)
                    expr3 = buffer;

                if (expr1 != null)
                {
                    if (expr2 != null)
                    {
                        if (expr2 == "=") // Defer evaluation to Compute ...
                            r.Add(expr1);
                        else
                            r.Add(CommonActions_Controller.GetExpressionValue(expr1, Node) as string);
                    }
                }
                if (expr2 != null)
                    r.Add(CommonActions_Controller.GetExpressionValue(expr2, Node) as string);
                if (expr3 != null)
                    r.Add(CommonActions_Controller.GetExpressionValue(expr3, Node) as string);

                return r;
            }

            private bool Compute(List<string> components)
            {
                if (components.Count == 0)
                    return false;
                if (components.Count == 1)
                    return components != null;
                if (components.Count == 3)
                {
                    switch (components[1])
                    {
                        case "==":
                            return components[0] == components[2];
                        case "!=":
                            return components[0] != components[2];
                        case ">":
                            return (components[0] ?? "").CompareTo(components[2]) == -1;
                        case "<":
                            return (components[0] ?? "").CompareTo(components[2]) == 1;
                        case ">=":
                            return (components[0] ?? "").CompareTo(components[2]) != -1;
                        case "<=":
                            return (components[0] ?? "").CompareTo(components[2]) != 1;
                        case "=":
                            return SetExpressionValue(components[0], components[2]);
                        default:
                            throw new ArgumentException("Sorry, unknown operator; '" + components[1] + "' ...");
                    }
                }
                throw new ArgumentException("Sorry, but your Expression; '" + RawExpression + "' just doesn't compute. Too many/few components in it ...");
            }

            private bool SetExpressionValue(string expr, string value)
            {
                CommonActions_Controller.SetExpressionValue(expr, value, Node);
                return true;
            }
        }

        private bool CheckStatement(string expr, Node node)
        {
            return new Expression(expr, node).Compute();
        }

        /**
         * Level2: Will untie the current Node. Can also be given 
         * an Epression that returns a Node list, at which it will 
         * untie that node. Afterwards, all child nodes will become 
         * executed, but only if no Expression is given. Which creates 
         * a useful way to start exeution paths, on their own temporary data, 
         * which does not mess up the DataSource node structure of your Module, etc
         */
        [ActiveEvent(Name = "Magix.System.untie")]
        protected void Magix_System_untie(object sender, ActiveEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Params.Get<string>()))
            {
                Node n = CommonActions_Controller.GetExpressionValue(
                    e.Params.Get<string>(), 
                    e.Params) 
                    as Node;
                if (n == null)
                    throw new ArgumentException(
                        "untie given a null expression; '" + 
                        e.Params.Get<string>() + 
                        "', with node; " + 
                        e.Params.Name);
                if(n.Parent == null)
                    throw new ArgumentException(
                        "untie given an expression without parent; '" +
                        e.Params.Get<string>() +
                        "', with node; " +
                        e.Params.Name);
                n.UnTie();
            }
            else
            {
                e.Params.UnTie();
                RaiseEvent(
                    "Magix.System.execute",
                    e.Params);
            }
        }

        /**
         * Level2: Changes the CSS class of the Widget or Server Side Control with the givn 'ID'.
         * The Control must be of type BaseWebControl. The CSS class will be
         * rolled according to all values in 'Values' parameter
         */
        [ActiveEvent(Name = "Magix.Common.ToggleCSSClassOfWidgetOrControl")]
        protected void Magix_Common_ToggleCSSClassOfWidgetOrControl(object sender, ActiveEventArgs e)
        {
            BaseWebControl c = Selector.FindControl<BaseWebControl>(Page, e.Params["ID"].Get<string>());

            if (c == null)
                throw new ArgumentException("Control or Widget doesn't exist, make sure you give your Widgets a unique ID. And that you give a correct 'ID' parameter");

            if (!e.Params.Contains("Values"))
                throw new ArgumentException("No Values parameter to roll by ...?");

            int idxNo = 0;

            foreach (Node idx in e.Params["Values"])
            {
                string cn = idx.Get<string>().Trim();
                if (c.CssClass.Contains(cn))
                {
                    break;
                }
                idxNo += 1;
            }

            if (idxNo >= e.Params["Values"].Count)
            {
                c.CssClass += " " + e.Params["Values"][0].Get<string>();
            }
            else
            {
                string old = " " + e.Params["Values"][idxNo].Get<string>();
                string ne = " " + (e.Params["Values"].Count > idxNo + 1 ?
                    e.Params["Values"][idxNo + 1].Get<string>() :
                    e.Params["Values"][0].Get<string>());
                c.CssClass = c.CssClass.Replace(old, ne);
            }
        }

        /**
         * Level2: Will roll a property value through the different values within 'Enums' node
         */
        [ActiveEvent(Name = "Magix.MetaTypes.RollThroughEnumValuesForProperty")]
        protected void Magix_MetaTypes_RollThroughEnumValuesForProperty(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                MetaObject t = MetaObject.SelectByID(e.Params["ID"].Get<int>());

                MetaObject.Property v = t.Values.Find(
                    delegate(MetaObject.Property idx)
                    {
                        return idx.Name == e.Params["PropertyName"].Get<string>();
                    });
                if (v == null)
                {
                    v = new MetaObject.Property();
                    v.Name = e.Params["PropertyName"].Get<string>();
                    v.Value = e.Params["Enums"][0].Get<string>();
                    t.Values.Add(v);
                }
                else
                {
                    string nValue = "";

                    int idxNo = 0;
                    foreach (Node idx in e.Params["Enums"])
                    {
                        idxNo += 1;
                        if (v.Value == idx.Get<string>())
                            break;
                    }
                    if (idxNo < e.Params["Enums"].Count)
                    {
                        nValue = e.Params["Enums"][idxNo].Get<string>();
                    }
                    else
                    {
                        nValue = e.Params["Enums"][0].Get<string>();
                    }
                    v.Value = nValue;

                }

                t.Save();

                tr.Commit();
            }
        }

        /**
         * Level2: Will return objects of given 'TypeName' back to caller as 'Objects' with
         * every instance within 'Objects' will contain one 'ID' node, and another node
         * called 'Properties' which will contain values for every Property within that
         * object within a flat hierarchy
         */
        [ActiveEvent(Name = "Magix.Common.GetActiveTypeObjects")]
        protected void Magix_Common_GetActiveTypeObjects(object sender, ActiveEventArgs e)
        {
            RaiseEvent(
                "DBAdmin.Data.GetContentsOfClass",
                e.Params);
        }

        /**
         * Level2: Will scroll the Client browser all the way back to the top over a period
         * of 500 Milliseconds [.5 seconds]
         */
        [ActiveEvent(Name = "Magix.MetaView.ScrollClientToTop")]
        protected void Magix_MetaView_ScrollClientToTop(object sender, ActiveEventArgs e)
        {
            new EffectScrollBrowser(500)
                .Render();
        }

        /**
         * Level2: Will put the given 'ParamName' GET parameter into the given node under
         * the name of the Parameter
         */
        [ActiveEvent(Name = "Magix.Common.PutGETParameterIntoDataSource")]
        protected void Magix_Common_PutGETParameterIntoDataSource(object sender, ActiveEventArgs e)
        {
            string nameOfParam = e.Params["ParamName"].Get<string>();
            if (string.IsNullOrEmpty(nameOfParam))
                throw new ArgumentException("Need a name of a specific GET parameter ...");

            object tmp = Page.Request.Params[nameOfParam];

            if (e.Params.Contains("ConvertToType"))
            {
                switch (e.Params["ConvertToType"].Get<string>())
                {
                    case "System.Int32":
                        tmp = int.Parse(tmp.ToString());
                        break;
                    case "System.Boolean":
                        tmp = bool.Parse(tmp.ToString());
                        break;
                    case "System.Decimal":
                        tmp = decimal.Parse(tmp.ToString());
                        break;
                    case "System.DateTime":
                        tmp = DateTime.ParseExact(tmp.ToString(), "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new ArgumentException("Dont't know how to convert to that type ...");
                }
            }

            if (tmp != null)
                e.Params[nameOfParam].Value = tmp;
        }

        /**
         * Level2: Will return an entire Graph of an object as 'Object', meaning all its child objects too.
         * Expects to find the ID of the MetaObject to fetch in the 'ID' parameter
         */
        [ActiveEvent(Name = "Magix.Common.GetMetaObjectGraph")]
        protected void Magix_Common_GetMetaObjectGraph(object sender, ActiveEventArgs e)
        {
            MetaObject o = MetaObject.SelectByID(e.Params["ID"].Get<int>());

            e.Params["Object"].UnTie();

            SerializeMetaObject(o, e.Params["Object"]);
        }

        /**
         * Level2: Will return from 'Start' to 'End', sorted according to newest first 
         * Meta Objects of type 'MetaTypeName' in the 'Objects' parameter. If you set 
         * 'Ascending' to true, it will sort according to oldest first. If you give
         * no 'End' parameter, then it will fetch the 10 items starting from start,
         * if you give no 'Start' parameter, it'll fetch from the beginning
         */
        [ActiveEvent(Name = "Magix.MetaObjects.GetMetaObjects")]
        protected void Magix_MetaObjects_GetMetaObjects(object sender, ActiveEventArgs e)
        {
            string typeName = e.Params["MetaTypeName"].Get<string>();
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentException("You need to specify a 'MetaTypeName' parameter");

            int start = e.Params["Start"].Get<int>(0);
            int end = e.Params["End"].Get<int>(-1);

            if (end == -1)
                end = start + 10;

            bool ascending = false;

            if (e.Params.Contains("Ascending"))
                ascending = e.Params["Ascending"].Get<bool>();

            e.Params["Objects"].UnTie();

            foreach (MetaObject idx in
                MetaObject.Select(
                    Criteria.Eq("TypeName", typeName),
                    Criteria.Range(start, end, "Created", ascending)))
            {
                GetOneObject(e.Params["Objects"], idx);
            }
        }

        /**
         * Level2: Requires 'Start', 'End' and 'MetaTypeName', just like its GetMetaObjects event counterpart.
         * Will first UnTie the 'Objects' node, then it will increase 'Start' and 'End' by 'End' - 'Start', 
         * and the same increase for 'End', and then return these objects to the caller in 'Objects'. You can 
         * also add 'Ascending' as a boolean to signigy sorting order. It will also return its new 'Start' and 
         * 'End' value according to which objects where actually fetched. Will display a message to user if there 
         * are no more objects to be fetched
         */
        [ActiveEvent(Name = "Magix.MetaObjects.GetNextMetaObjects")]
        protected void Magix_MetaObjects_GetNextMetaObjects(object sender, ActiveEventArgs e)
        {
            string typeName = e.Params["MetaTypeName"].Get<string>();
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentException("You need to specify a 'MetaTypeName' parameter");

            int start = e.Params["Start"].Get<int>(-1);
            int end = e.Params["End"].Get<int>(-1);

            if (end == -1 || 
                start == -1)
                throw new ArgumentException("You need to specify a 'Start' and 'End' parameter");

            e.Params["Objects"].UnTie();

            int delta = end - start;

            start += delta;
            end += delta;

            int objCount = MetaObject.CountWhere(Criteria.Eq("TypeName", typeName));

            if (end > objCount)
            {
                ShowMessage("No more objects here ...");
                start = objCount - delta;
                end = objCount;
            }

            bool ascending = false;

            if (e.Params.Contains("Ascending"))
                ascending = e.Params["Ascending"].Get<bool>();

            foreach (MetaObject idx in
                MetaObject.Select(
                    Criteria.Eq("TypeName", typeName),
                    Criteria.Range(start, end, "Created", ascending)))
            {
                GetOneObject(e.Params["Objects"], idx);
            }

            e.Params["Start"].Value = start;
            e.Params["End"].Value = end;
        }

        /**
         * Level2: Requires 'Start', 'End' and 'MetaTypeName', just like its GetMetaObjects event counterpart.
         * Will first UnTie the 'Objects' node, then it will decrease 'Start' and 'End' by 'End' - 'Start', 
         * and the same decrease for 'End', and then return these objects to the caller in 'Objects'. You can 
         * also add 'Ascending' as a boolean to signigy sorting order. It will also return its new 'Start' and 
         * 'End' value according to which objects where actually fetched. Will display a message to user if there 
         * are no more objects to be fetched
         */
        [ActiveEvent(Name = "Magix.MetaObjects.GetPreviousMetaObjects")]
        protected void Magix_MetaObjects_GetPreviousMetaObjects(object sender, ActiveEventArgs e)
        {
            Node old = e.Params["Objects"].UnTie();

            string typeName = e.Params["MetaTypeName"].Get<string>();
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentException("You need to specify a 'MetaTypeName' parameter");

            int start = e.Params["Start"].Get<int>(-1);
            int end = e.Params["End"].Get<int>(-1);

            if (end == -1)
                throw new ArgumentException("You need to specify an 'End' parameter");

            int delta = end - start;

            start -= delta;
            end -= delta;

            if (start < 0)
            {
                ShowMessage("These are the first objects of that type at the moment ...");
                start = 0;
                end = delta;
            }

            bool ascending = false;

            if (e.Params.Contains("Ascending"))
                ascending = e.Params["Ascending"].Get<bool>();

            e.Params["Objects"].UnTie();

            foreach (MetaObject idx in
                MetaObject.Select(
                    Criteria.Eq("TypeName", typeName),
                    Criteria.Range(start, end, "Created", ascending)))
            {
                GetOneObject(e.Params["Objects"], idx);
            }

            if (!e.Params.Contains("Objects") ||
                e.Params["Objects"].Count == 0)
            {
                // No more objects, showing user ...
                // And retying old object back in loop ...
                e.Params["Object"] = old;

                ShowMessage("Sorry, there are no more objects to be fetched of that type ... ");
            }
            else
            {
                e.Params["Start"].Value = start;
                e.Params["End"].Value = end;
            }
        }

        /*
         * Helper for above ...
         */
        private static void GetOneObject(Node node, MetaObject o)
        {
            node["o-" + o.ID]["ID"].Value = o.ID;
            node["o-" + o.ID]["TypeName"].Value = o.TypeName;
            node["o-" + o.ID]["Created"].Value = o.Created.ToString("yyyy.MM.dd HH:mm:ss");

            foreach (MetaObject.Property idx in o.Values)
            {
                node["o-" + o.ID]["Properties"][idx.Name]["ID"].Value = idx.ID;
                node["o-" + o.ID]["Properties"][idx.Name]["Name"].Value = idx.Name;
                node["o-" + o.ID]["Properties"][idx.Name]["Value"].Value = idx.Value;
            }

            foreach (MetaObject idx in o.Children)
            {
                GetOneObject(node["o-" + o.ID]["Objects"], idx);
            }
        }

        /*
         * Helper for above ...
         */
        private void SerializeMetaObject(MetaObject o, Node node)
        {
            node["TypeName"].Value = o.TypeName;
            node["Reference"].Value = o.Reference;
            foreach (MetaObject.Property idx in o.Values)
            {
                node[idx.Name].Value = idx.Value;
            }
            foreach (MetaObject idx in o.Children)
            {
                SerializeMetaObject(idx, node[idx.TypeName]["o-" + idx.ID]);
            }
        }

        /**
         * Level2: Will validate the given object, sent from a MetaView SingleView, such that it conforms 
         * in its 'PropertyName' value towards the 'Type' parameter, which can either be 'email' or
         * 'number'. Meaning if the MetaView has a Property called 'Email', then if the property 
         * was written such; 'email:Email' - Then if the user writes anything but a legal Email 
         * address, according to RFC 5321, minus length restrictions, in the 'Email' field, 
         * and exception will be thrown, and the current execution chain of Actions will be 
         * stopped and no more Actions will be executed. The Message of the Exception will be, at least
         * to some extent, friendly. For implementers of plugins, handle this Active Event, and 
         * check towards the 'Type' parameter which then will contain your chosen prefix. Prefixes
         * for this method are 'email', 'number', 'mandatory', 'url' and 'full-name'.
         * PS! Some things might get changed 
         * during the course of this event, for instance if you have a 'url' field, without http:// 
         * or https:// in the beginning, those characters will be appended. 'full-name' will normalize the 
         * given value such that it says 'Hansen, Thomas', it will also Auto Capitalize. If name is given like 
         * thomas hansen it will assume first name is first. If there's a comma, it will assert that the last 
         * name(s) are behind the comma, and the first name(s) before it. It will still produce the result 
         * 'Hansen, Thomas Polterguy Hoeoeg' from e.g. 'thomas poLTerguy Hoeoeg hanseN', or from 
         * 'hansen, thomas POLTERGUY hoeoeg'. While 'Hoeoeg Hansen, Thomas Polterguy' will emerge
         * from e.g. 'hoeoeg hansen, thomas polterGUY'. Unless the 'AcceptNull' is given, most controls 
         * will not accept an empty value. While for 'full-name', unless 'AcceptHalfName' is given, or 
         * 'AcceptNull' is given, it will either accept only a full name, meaning at least two names 
         * or only one name if 'AcceptHalfName' is true. While unless 'AcceptNull' is null, it will still 
         * need at least one name, with more than 2 letters within. It will only accept one comma. If 
         * the Parameter 'FirstFirst' equals to true, it'll put the First Name BEFORE the Last Name, and 
         * use no commas
         */
        [ActiveEvent(Name = "Magix.Common.ValidateObjectProperty")]
        protected void Magix_Common_ValidateObjectProperty(object sender, ActiveEventArgs e)
        {
            if (!e.Params.Contains("Type"))
                throw new ArgumentException("Missing 'Type' parameter for 'Magix.Common.ValidateObjectProperty' event");

            switch (e.Params["Type"].Get<string>())
            {
                case "email":
                    {
                        ValidateEmail(e.Params);
                    } break;
                case "number":
                    {
                        ValidateNumber(e.Params);
                    } break;
                case "mandatory":
                    {
                        ValidateMandatory(e.Params);
                    } break;
                case "url":
                    {
                        ValidateURL(e.Params);
                    } break;
                case "full-name":
                    {
                        ValidateFullName(e.Params);
                    } break;
            }
            e.Params["Type"].UnTie();
            e.Params["PropertyName"].UnTie();
        }

        private void ValidateFullName(Node node)
        {
            string valueToValidate = node["PropertyValues"][node["PropertyName"].Get<string>()]["Value"].Get<string>().Trim();

            if (string.IsNullOrEmpty(valueToValidate))
            {
                if (!node.Contains("AcceptNull") ||
                    !node["AcceptNull"].Get<bool>())
                {
                    throw new ArgumentException(
                        string.Format("Sorry, but the '{0}' field is mandatory ... ",
                            node["PropertyValues"][node["PropertyName"].Get<string>()]["Name"].Get<string>()));
                }
            }

            string firstNames = "";
            string lastNames = "";

            if (valueToValidate.Contains(","))
            {
                if (valueToValidate.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length != 2)
                {
                    throw new ArgumentException(@"Sorry, but this control only accepts [0, 1} comma, 
either as a list of separated names, seaparated by spaces, 
or with one comma signigying the end of the last name(s) and the beginning 
of the first name(s)...");
                }

                // Assuming last name is first ...
                lastNames = valueToValidate.Split(',')[0];
                firstNames = valueToValidate.Split(',')[1];
            }
            else
            {
                if (valueToValidate.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length < 2)
                {
                    // Only one name ...
                    // Need to verify that it's legal ...
                    if (!node.Contains("AcceptHalfName") ||
                        !node["AcceptHalfName"].Get<bool>())
                    {
                        throw new ArgumentException("Sorry, but this field requires a FULL name ...");
                    }
                    firstNames = valueToValidate;
                }
                else
                {
                    // Assuming there's only one last name ...
                    firstNames = valueToValidate.Substring(0, valueToValidate.LastIndexOf(' '));
                    string[] names = valueToValidate.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    lastNames = names[names.Length - 1];
                }
            }

            // Removing unnecessary white-spaces at the endings ...
            firstNames = firstNames.Trim();
            lastNames = lastNames.Trim();

            string[] allFirstNames = firstNames.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            firstNames = "";
            foreach (string idx in allFirstNames)
            {
                string tmpName = char.ToUpper(idx[0]) + idx.Substring(1).Trim();

                firstNames += " " + tmpName;
            }

            string[] allLastNames = lastNames.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            lastNames = "";
            foreach (string idx in allLastNames)
            {
                lastNames += " " + char.ToUpper(idx[0]) + idx.Substring(1).Trim();
            }

            if (node.Contains("FirstFirst") &&
                node["FirstFirst"].Get<bool>())
                node["PropertyValues"][node["PropertyName"].Get<string>()]["Value"].Value =
                    ((firstNames.Length > 0 ? (firstNames + " ") : "") + lastNames).Replace("  ", " ").Trim();
            else
                node["PropertyValues"][node["PropertyName"].Get<string>()]["Value"].Value =
                    ((lastNames.Length > 0 ? (lastNames + ", ") : "") + firstNames).Replace("  ", " ").Trim();
        }

        private void ValidateURL(Node node)
        {
            string valueToValidate = node["PropertyValues"][node["PropertyName"].Get<string>()]["Value"].Get<string>().Trim();

            if (string.IsNullOrEmpty(valueToValidate))
            {
                if (!node.Contains("AcceptNull") ||
                    !node["AcceptNull"].Get<bool>())
                {
                    throw new ArgumentException(
                        string.Format("Sorry, but the '{0}' field is mandatory ... ",
                            node["PropertyName"].Get<string>()));
                }
            }
            if (valueToValidate.IndexOf("http://") != 0 &&
                valueToValidate.IndexOf("https://") != 0)
            {
                valueToValidate = "http://" + valueToValidate;

                // Doing a little bit more than just pure validation here .... ;)
                node["PropertyValues"][node["PropertyName"].Get<string>()]["Value"].Value = valueToValidate;
            }
            if (valueToValidate.IndexOf(".") == -1)
            {
                throw new ArgumentException(
                    string.Format("A legal URL needs to contain at least one '.'"));
            }
            if (valueToValidate.Substring(valueToValidate.LastIndexOf('.') + 1).Length < 1)
            {
                throw new ArgumentException(
                    string.Format("The shortest top domain I'm aware of is still one character large, add at least one character to the end of this URL ..."));
            }
            foreach (char idx in valueToValidate)
            {
                if (("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$-_.+!*'(),/:").IndexOf(idx) == -1)
                {
                    throw new ArgumentException(
                        string.Format("Found an illegal character in your URL '{0}'",
                            idx));
                }
            }
        }

        private void ValidateMandatory(Node node)
        {
            string valueToValidate = node["PropertyValues"][node["PropertyName"].Get<string>()]["Value"].Get<string>().Trim();

            if (string.IsNullOrEmpty(valueToValidate))
            {
                throw new ArgumentException(
                    string.Format(@"Oops, that field ['{0}'] unfortunately is Mandatory, 
and you didn't type anything in ...",
                        node["PropertyName"].Get<string>()));
            }
        }

        private void ValidateNumber(Node node)
        {
            string valueToValidate = node["PropertyValues"][node["PropertyName"].Get<string>()]["Value"].Get<string>().Trim();

            if (string.IsNullOrEmpty(valueToValidate))
            {
                if (!node.Contains("AcceptNull") ||
                    !node["AcceptNull"].Get<bool>())
                {
                    throw new ArgumentException(
                        string.Format("Sorry, but the '{0}' field is mandatory ... ",
                            node["PropertyName"].Get<string>()));
                }
            }

            foreach (char idx in valueToValidate)
            {
                if (("0123456789., ").IndexOf(idx) == -1)
                {
                    throw new ArgumentException(
                        string.Format(@"According to most people's definition, 
your content in '{0}' is either not a number, or a highly irregular one if so. Content is '{1}', but 
can only contain numerical characters to be legal",
                        node["PropertyName"].Get<string>(),
                        valueToValidate));
                }
            }
        }

        private static void ValidateEmail(Node node)
        {
            bool hasFront = false;
            bool hasBack = false;
            bool pastAt = false;
            bool hasValidUpperDomain = false;
            bool hasSeenDomainDot = false;
            string valueToValidate = node["PropertyValues"][node["PropertyName"].Get<string>()]["Value"].Get<string>().Trim();

            if (string.IsNullOrEmpty(valueToValidate))
            {
                if (!node.Contains("AcceptNull") ||
                    !node["AcceptNull"].Get<bool>())
                {
                    throw new ArgumentException(
                        string.Format("Sorry, but the '{0}' field is mandatory ... ",
                            node["PropertyName"].Get<string>()));
                }
            }

            foreach (char idx in valueToValidate)
            {
                if (!pastAt)
                {
                    if (("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!#$%&'*+-/=?^_`{|}~").IndexOf(idx) > 0)
                    {
                        hasFront = true;
                    }
                    else if (idx == '@')
                    {
                        pastAt = true;
                        continue;
                    }
                }
                else
                {
                    if (idx == '.')
                        hasSeenDomainDot = true;
                    else if (hasSeenDomainDot)
                    {
                        hasValidUpperDomain = true;
                    }
                    else
                        hasBack = true;
                }
            }
            if (!hasBack || !hasFront || !hasValidUpperDomain)
            {
                throw new ArgumentException("Opps, that doesn't entirely look like a valid Email, does it ...?");
            }
        }

        /**
         * Level2: Simplification of 'Magix.Core.SendEmail', will among
         * other things using the User.Current as sender unless explicitly
         * overridden. Will also unless to email address is given, send email to
         * yourself. Takes these parameters 'Header', 'Body', 'Email' [from email], 'From' [name]
         * 'To' [which can be a list of emails or one email]
         */
        [ActiveEvent(Name = "Magix.Common.SendEmail")]
        protected void Magix_Common_SendEmail(object sender, ActiveEventArgs e)
        {
            Node node = new Node();

            node["Header"].Value =
                e.Params.Contains("Header") && !string.IsNullOrEmpty(e.Params["Header"].Get<string>()) ?
                    e.Params["Header"].Get<string>() :
                    "Message from Marvin ...";

            node["Body"].Value =
                e.Params.Contains("Body") && !string.IsNullOrEmpty(e.Params["Body"].Get<string>()) ?
                    e.Params["Body"].Get<string>() :
                    "Opps, someone forgot to attach the message ...";

            node["AdminEmail"].Value =
                e.Params.Contains("Email") && !string.IsNullOrEmpty(e.Params["Email"].Get<string>()) ?
                    e.Params["Email"].Get<string>() :
                    User.Current.Email;

            node["AdminEmailFrom"].Value =
                e.Params.Contains("From") && !string.IsNullOrEmpty(e.Params["From"].Get<string>()) ?
                    e.Params["From"].Get<string>() :
                    User.Current.FullName;

            if (e.Params.Contains("To"))
            {
                if (!string.IsNullOrEmpty(e.Params["To"].Get<string>()))
                {
                    node["EmailAddresses"]["only"].Value = e.Params["To"].Get<string>();
                }
                else
                {
                    node["EmailAddresses"].AddRange(e.Params["To"].UnTie());
                    if (node["EmailAddresses"][0].Value == null)
                    {
                        // Just in case this is a 'template action' with empty placeholders for end-user
                        // to fill in ...
                        node["EmailAddresses"][0].Value = User.Current.Email;
                    }
                }
            }
            else
            {
                // Sending yourself an email ...
                node["EmailAddresses"]["only"].Value = User.Current.Email;
            }

            RaiseEvent(
                "Magix.Core.SendEmail",
                node);
        }

        /**
         * Level2: Will transform the input node according to the 
         * expressions within its 'Expression' parameter node and its children. Will return the 
         * transformed Node as its 'Expression' return value, which will become a completely 
         * new node, depending upon the expressios transforming it. Every node containing
         * an expression as either its Name or Value, will become transformed into the result of its 
         * expression. All other nodes will be considered static values and left untouched.
         * Only nodes within the 'Expression' parameter will be transformed. If there are no 'Expression'
         * node, then the entire hierarchy will be treated as an Expression. Regardless, the result
         * will be returned in the 'Expression' node, which will become the result. @expression
         */
        [ActiveEvent(Name = "Magix.Common.Transform")]
        protected void Magix_Common_TransformNode(object sender, ActiveEventArgs e)
        {
            Node retVal = new Node();

            Node expr = e.Params;

            if (e.Params.Contains("Expression"))
                expr = e.Params["Expression"];

            Transform(expr, e.Params, retVal);

            retVal.SetParent(e.Params);
            e.Params["Expression"] = retVal;
        }

        private void Transform(Node expr, Node source, Node destination)
        {
            destination.Name = GetExpressionValue(expr.Name, source) as string;
            destination.Value = GetExpressionValue(expr.Value as string, source);

            if (expr.Value != null &&
                expr.Value.ToString().Contains(":{") &&
                expr.Value.ToString().Contains("}"))
            {
                // We have children declaration ...
                destination.AddRange(
                    GetExpressionValue(expr.Value.ToString().Split(':')[1], source) 
                        as IEnumerable<Node>);
            }

            foreach (Node idx in expr)
            {
                Node x = new Node();
                destination.Add(x);
                Transform(idx, source, x);
            }
        }

        /**
         * Level2: Documented, although not directly accessible for anyone outside of code, though used
         * heavily in virtually every single module and component which uses Expressions, which are
         * statements of text starting with '{', without the quotes. This method will return one out of 
         * four different possible values, a null value, meaning 'empty value', as in for instance; 
         * 'not found' or 'didn't have a value'. It can also return itself, if you try to check a
         * text literal for its expression value, and it contains no expression, it will return 
         * 'itself' such as if for instance your expression parameter was for instance; 'Hello World...',
         * it would return; 'Hello World...'. If your expression starts with a '{', it will be treated 
         * as a 'lookup' into your given source Node structure, for instance the expression;
         * {[Objects][0][Email].Value} will return whatever happens to be at the Value property of 
         * the 'Email' node, which is in the 'first child' of the 'Objects' node within your DataSource, if 
         * you're in a Form of some sort for instance. While; {[0].Name} will return the name of the 
         * first node, and {[Objects]} will return all nodes in the 'Objects' node. If the expression starts 
         * with '{DataSource[', it will traverse and starts its traversion process at the outer most parent Node,
         * the first Node within its Hierarchy
         */
        public static object GetExpressionValue(string expression, Node source)
        {
            if (expression == null)
                return null;

            if (!expression.StartsWith("{"))
            {
                if (!expression.Contains("{"))
                    return expression;
                return expression;
            }

            string expr = expression.Split('{')[1].Split('}')[0].Trim();

            Node x = source;

            if (expr.StartsWith("DataSource[") || expr.StartsWith("root["))
                x = source.RootNode();
            else if (!expr.StartsWith("["))
            {
                x = source[expr.Split('.')[0]].Value as Node;
                expr = expr.Substring(expr.Split('.')[0].Length);
            }

            bool isInside = false;
            string bufferNodeName = null;
            string lastEntity = null;

            for (int idx = 0; idx < expr.Length; idx++)
            {
                char tmp = expr[idx];
                if (isInside)
                {
                    if (tmp == ']')
                    {
                        if (string.IsNullOrEmpty(bufferNodeName))
                            throw new ArgumentException("Opps, empty node name/index ...");

                        lastEntity = "";

                        bool allNumber = true;
                        if (bufferNodeName == "../")
                        {
                            if (x.Parent == null)
                                throw new NullReferenceException("Attempted at trying to traverse up to a level which doesn't exist. Parent Root Node reached, and found '../' still ...");
                            x = x.Parent;
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
                        else
                        {
                            foreach (char idxC in bufferNodeName)
                            {
                                if (("0123456789").IndexOf(idxC) == -1)
                                {
                                    allNumber = false;
                                    break;
                                }
                            }
                            if (allNumber)
                            {
                                int intIdx = int.Parse(bufferNodeName);
                                if (x.Count >= intIdx)
                                    x = x[intIdx];
                                return null;
                            }
                            else
                            {
                                if (!x.Contains(bufferNodeName))
                                    return null;
                                x = x[bufferNodeName];
                            }
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
                    }
                    bufferNodeName += tmp;
                }
                else
                {
                    if (tmp == '[')
                    {
                        bufferNodeName = "";
                        isInside = true;
                        continue;
                    }
                    lastEntity += tmp;
                }
            }
            if (lastEntity == ".Value")
                return x.Value;
            else if (lastEntity == ".Name")
                return x.Name;
            else if (lastEntity == "")
                return x;

            return null;
        }

        private static void SetExpressionValue(string expression, string value, Node source)
        {
            if (string.IsNullOrEmpty(expression) || !expression.StartsWith("{"))
            {
                throw new ArgumentException("Assignments must have Expressions as left hand sides ... ");
            }

            string expr = expression.Split('{')[1].Split('}')[0].Trim();

            Node x = source;

            if (expr.StartsWith("DataSource[") || expr.StartsWith("root["))
                x = source.RootNode();
            else if (!expr.StartsWith("["))
            {
                x = source[expr.Split('.')[0]].Value as Node;
                expr = expr.Substring(expr.Split('.')[0].Length);
            }

            bool isInside = false;
            string bufferNodeName = null;
            string lastEntity = null;

            for (int idx = 0; idx < expr.Length; idx++)
            {
                char tmp = expr[idx];
                if (isInside)
                {
                    if (tmp == ']')
                    {
                        if (string.IsNullOrEmpty(bufferNodeName))
                            throw new ArgumentException("Opps, empty node name/index ...");

                        lastEntity = "";
                        isInside = false;

                        bool allNumber = true;
                        if (bufferNodeName == "../")
                        {
                            if (x.Parent == null)
                                throw new NullReferenceException("Attempted at trying to traverse up to a level which doesn't exist. Parent Root Node reached, and found '../' still ...");
                            x = x.Parent;
                            continue;
                        }
                        else
                        {
                            foreach (char idxC in bufferNodeName)
                            {
                                if (("0123456789").IndexOf(idxC) == -1)
                                {
                                    allNumber = false;
                                    break;
                                }
                            }
                            if (allNumber)
                            {
                                int intIdx = int.Parse(bufferNodeName);
                                if (x.Count >= intIdx)
                                    x = x[intIdx];
                                else
                                    throw new ArgumentException("Oops, tried to de-reference non existing nodes: " + intIdx + ", in: " + expression);
                            }
                            else
                            {
                                x = x[bufferNodeName];
                            }
                            bufferNodeName = "";
                            continue;
                        }
                    }
                    bufferNodeName += tmp;
                }
                else
                {
                    if (tmp == '[')
                    {
                        bufferNodeName = "";
                        isInside = true;
                        continue;
                    }
                    lastEntity += tmp;
                }
            }
            if (lastEntity == ".Value")
            {
                x.Value = GetExpressionValue(value, source) as string;
            }
            else if (lastEntity == ".Name")
            {
                x.Name = GetExpressionValue(value, source) as string;
            }
            else if (lastEntity == "")
            {
                x.Clear();
                Node tmp = GetExpressionValue(value, source) as Node;
                if (tmp != null)
                {
                    foreach (Node idx2 in tmp)
                    {
                        x.Add(idx2.Clone());
                    }
                }
            }
        }

        /**
         * Level2: Will replace all occurencies of 'Replace' Value, which must be an Expression with 
         * the expression in 'Replacement' Value node, within the 'Source' expression. For instance, 
         * if you have a node in your DataSource 
         * at DataSource[Object][Properties][Email].Value containing;
         * "sdfouh sdfouh sdfu sdfigqweeqw !!Name!! mumbo-jumbo" and you want to replace all occurencies 
         * of the string literal '!!Name!!' with whatever happens to be in your current 
         * DataSource[Name][0].Value, then the Node structure you'd have to create to pass into 
         * this Event is;
         * [Replace].Value equals '!!Name!!'
         * [Replacement].Value equals '{[Name][0].Value}'
         * [Source].Value = '[Object][Properties][Email].Value'
         * Both 'Replace' and 'Replacement' might either be static text or Expressions. 
         * All three parameters must somehow point to single instances, and not Node lists
         * or anything, but they can all end with either 'Value' or 'Name'
         */
        [ActiveEvent(Name = "Magix.Common.ReplaceStringLiteral")]
        protected void Magix_Common_ReplaceStringLiteral(object sender, ActiveEventArgs e)
        {
            string replace = e.Params["Replace"].Get<string>();
            if (string.IsNullOrEmpty(replace))
                throw new ArgumentException("You must supply a 'Replace' parameter to the ReplaceStringLiteral Event");

            string replacement = e.Params["Replacement"].Get<string>();
            if (string.IsNullOrEmpty(replacement))
                throw new ArgumentException("You must supply a 'Replacement' parameter to the ReplaceStringLiteral Event");

            string source = e.Params["Source"].Get<string>();
            if (string.IsNullOrEmpty(source))
                throw new ArgumentException("You must supply a 'Source' parameter to the ReplaceStringLiteral Event");

            ReplaceStringLiteral(replace, replacement, source, e.Params);
        }

        /*
         * Helper for above ...
         */
        private void ReplaceStringLiteral(string replace, string replacement, string source, Node node)
        {
            object replaceObj = GetExpressionValue(replace, node) as string;
            if (replaceObj == null)
                throw new ArgumentException("Invalid 'Replace' parameter; " + replace);

            object replacementObj = GetExpressionValue(replacement, node);
            if (replacementObj == null)
                throw new ArgumentException("Invalid 'Replacement' parameter; " + replacement);

            string tmp = GetExpressionValue(source, node) as string;
            if (tmp == null) // must be string ...!!
                throw new ArgumentException("Invalid 'Source' parameter; " + replacement);

            string nodeStr = source.Trim().Trim('{').Trim('}').Trim();
            bool isValue = nodeStr.EndsWith(".Value");
            nodeStr = nodeStr.Substring(0, nodeStr.LastIndexOf('.'));
            nodeStr = "{" + nodeStr + "}";

            Node tmp2 = GetExpressionValue(nodeStr, node) as Node;

            if (isValue)
                tmp2.Value = tmp2.Value.ToString().Replace(replaceObj.ToString(), replacementObj.ToString());
            else
                tmp2.Name = tmp2.Value.ToString().Replace(replaceObj.ToString(), replacementObj.ToString());
        }

        /**
         * Level1: Will serialize the Active SingleView Form and send an email from 'Email' and 'From' 
         * to the 'Email' field on the given form. Before the email is sendt both the 'Header' and
         * the 'Body' will substitute every single occurency of [x] with the MetaView's property 
         * with the same name [x]
         */
        [ActiveEvent(Name = "Magix.Common.SendEmailFromForm")]
        protected void Magix_Common_SendEmailFromForm(object sender, ActiveEventArgs e)
        {
            if (!e.Params.Contains("OriginalWebPartID"))
                throw new ArgumentException("This Action can only be raised from within a SingleView form");

            if (!e.Params.Contains("Email"))
                throw new ArgumentException("You do need at the very least a 'From Address' for the SendEmailFromForm Event to work");

            // Getting current MetaView content...
            Node node = new Node();
            node["OriginalWebPartID"].Value = e.Params["OriginalWebPartID"].Value;

            RaiseEvent(
                "Magix.MetaView.SerializeSingleViewForm",
                node);

            if (node.Count == 0)
            {
                throw new ArgumentException("There are no forms on the screen now that can be used for raising this action. This action can only be raised from within a Single View MetaView ...");
            }

            if (!node.Contains("Email"))
            {
                throw new ArgumentException("The SingleView MetaView must at the minimum contain at least one field, and this field must be called 'Email'. It will serve as the To Email Address ...");
            }

            node["OriginalWebPartID"].UnTie();

            string header = e.Params["Header"].Get<string>();
            string body = e.Params["Body"].Get<string>();

            // Substituting ...
            foreach (Node idx in node)
            {
                header = header.Replace(string.Format("[{0}]", idx.Name), idx.Value.ToString());
                body = body.Replace(string.Format("[{0}]", idx.Name), idx.Value.ToString());
            }

            e.Params["Header"].Value = header;
            e.Params["Body"].Value = body;

            // Have to rename from 'Email' [View Property Name] to 'To' which our Email logic understands ...
            e.Params["To"].Value = node["Email"].Value;

            RaiseEvent(
                "Magix.Common.SendEmail",
                e.Params);
        }

        /**
         * Level2: Will to a String.Replace on the given 'Source' or 'SourceNode'. Will replace 'OldString' or 'OldStringNode'
         * with 'NewString' or 'NewStringNode' and return the value either in 'Result' or 'ResultNode', direct
         * value [no 'Node' part] always have preference
         */
        [ActiveEvent(Name = "Magix.Common.ReplaceStringValue")]
        protected void Magix_Common_ReplaceStringValue(object sender, ActiveEventArgs e)
        {
            string source = 
                e.Params.Contains("Source") ? 
                    e.Params["Source"].Get<string>() : 
                    e.Params[e.Params["SourceNode"].Get<string>()].Get<string>();
            string oldString =
                e.Params.Contains("OldString") ?
                    e.Params["OldString"].Get<string>() :
                    e.Params[e.Params["OldStringNode"].Get<string>()].Get<string>();
            string newString =
                e.Params.Contains("NewString") ?
                    e.Params["NewString"].Get<string>() :
                    e.Params[e.Params["NewStringNode"].Get<string>()].Get<string>();

            string transformed = source.Replace(oldString, newString);

            if (e.Params.Contains("ResultNode"))
                e.Params[e.Params["ResultNode"].Get<string>()].Value = transformed;
            else
                e.Params["Result"].Value = transformed;
        }

        /**
         * Level2: Will call 'Magix.MetaAction.RaiseAction' for every single 'ActionName' in the Actions [list]
         * Parameter. Useful for creating complex abstractions, doing multiple tasks at once or 'encapsulating'
         * your entire logic inside one Action
         */
        [ActiveEvent(Name = "Magix.Common.MultiAction")]
        protected void Magix_Common_MultiAction(object sender, ActiveEventArgs e)
        {
            foreach (Node idx in e.Params["Actions"])
            {
                Node eventNodes = idx;

                RaiseEvent(
                    "Magix.MetaAction.RaiseAction",
                    eventNodes);
            }
        }

        /**
         * Level2: Will rename the given 'FromName' to 'ToName'. Will throw exception if no 'FromName' exists,
         * or parameters are missing
         */
        [ActiveEvent(Name = "Magix.Common.RenameNode")]
        protected void Magix_Common_RenameNode(object sender, ActiveEventArgs e)
        {
            if (!e.Params.Contains("FromName") || !e.Params.Contains("ToName"))
                throw new ArgumentException("You need to specify both FromName and ToName to RenameNode ...");
            string fromName = e.Params["FromName"].Get<string>();

            if (e.Params.Contains(fromName))
                throw new ArgumentException("No such FromName in RenameNode ...");

            string toName = e.Params["ToName"].Get<string>();
            e.Params[fromName].Name = toName;
        }

        /**
         * Level2: Will strip every single Parameter OUT of the Node structure except the given 'But'. But can be
         * either one single name of an object or a list of nodes containing several names. Useful for
         * shrinking nodes as the grow due to being passed around or being parts of MultiActions or something
         * similar
         */
        [ActiveEvent(Name = "Magix.Common.StripAllParametersExcept")]
        protected void Magix_Common_StripAllParametersExcept(object sender, ActiveEventArgs e)
        {
            string but = e.Params["But"].Get<string>();
            if (!string.IsNullOrEmpty(but))
            {
                // Keeping only ONE node ...
                List<Node> nodes = new List<Node>();
                foreach (Node ix in e.Params)
                {
                    if (ix.Name != but)
                        nodes.Add(ix);
                }
                foreach (Node idx in nodes)
                {
                    idx.UnTie();
                }
            }
            else
            {
                // Array of stuff to keep ...
                List<Node> nodes = new List<Node>();
                foreach (Node ix in e.Params["But"])
                {
                    if (ix.Name != but)
                        nodes.Add(ix);
                }
                foreach (Node idx in nodes)
                {
                    idx.UnTie();
                }
            }
        }

        /**
         * Level2: Will return the given MetaObject [ID] as a Key/Value pair. Will not traverse
         * Child Objects though. Useful for fetching objects for any one reasons you might have,
         * as long as you know their ID
         */
        [ActiveEvent(Name = "Magix.Common.GetSingleMetaObject")]
        protected void Magix_Common_GetSingleMetaObject(object sender, ActiveEventArgs e)
        {
            MetaObject o = MetaObject.SelectByID(e.Params["ID"].Get<int>());

            if (o == null)
                throw new ArgumentException(
                    @"Some wize-guy have deleted your object dude. 
Update the ID property of your Action to another Meta Object ...");

            e.Params["Properties"].UnTie();

            foreach (MetaObject.Property idx in o.Values)
            {
                e.Params["Properties"][idx.GetName()].Value = idx.Value;
            }
        }

        /**
         * Level2: Will reload the Original WebPart, intended to be, within the 'current WebPart container'
         * on the page. Meaning, if you've allowed the user to 'fuzz around all over the place' till he
         * no longer can remember what originally was within a specific WebPart Container, he can 
         * raise this event [somehow], which will 'reload the original content' into the 'current
         * container' [container raising the event]
         */
        [ActiveEvent(Name = "Magix.Common.ReloadOriginalWebPart")]
        protected void Magix_Common_ReloadOriginalWebPart(object sender, ActiveEventArgs e)
        {
            int po = e.Params["Parameters"]["OriginalWebPartID"].Get<int>();

            Node node = new Node();

            node["OriginalWebPartID"].Value = po;

            RaiseEvent(
                "Magix.Publishing.ReloadWebPart",
                node);
        }

        /**
         * Level2: If raised from within a MetaView on a specific MetaObject ['ID'], 
         * somehow, will show the Signature Module for that particular MetaObject for
         * its 'ActionSenderName' property. When Signature is done [signing complete]
         * the original content of the Container will be reloaded
         */
        [ActiveEvent(Name = "Magix.Common.LoadSignatureForCurrentMetaObject")]
        protected void Magix_Common_LoadSignatureForCurrentMetaObject(object sender, ActiveEventArgs e)
        {
            if (!e.Params.Contains("OriginalWebPartID") ||
                !e.Params.Contains("ID"))
            {
                throw new ArgumentException("Sorry buddy, but this Action only works from a MetaView WebPart ...");
            }
            else
            {
                e.Params["OKEvent"].Value = "Magix.MetaView.UnLoadSignature";
                e.Params["OKEvent"]["Params"]["ID"].Value = e.Params["ID"].Value;
                e.Params["OKEvent"]["Params"]["OriginalWebPartID"].Value = e.Params["OriginalWebPartID"].Value;
                e.Params["OKEvent"]["Params"]["Name"].Value = e.Params["ActionSenderName"].Value;

                e.Params["CancelEvent"].Value = "Magix.Publishing.ReloadWebPart";
                e.Params["CancelEvent"]["Params"]["OriginalWebPartID"].Value = e.Params["OriginalWebPartID"].Value;

                if (e.Params.Contains("Value") &&
                    !string.IsNullOrEmpty(e.Params["Value"].Get<string>()))
                    e.Params["Coords"].Value = e.Params["Value"].Value;

                RaiseEvent(
                    "Magix.Signature.LoadSignature",
                    e.Params);
            }
        }

        /**
         * Level2: Helper for Signature Module, to store it correctly upon finishing and saving a new Signature.
         * Will extract the 'Signature' content and store into the 'Name' property of the given
         * 'ID' MetaObject and save the MetaObject
         */
        [ActiveEvent(Name = "Magix.MetaView.UnLoadSignature")]
        protected void Magix_Signature_UnLoadSignature(object sender, ActiveEventArgs e)
        {
            using (Transaction tr = Adapter.Instance.BeginTransaction())
            {
                MetaObject o = MetaObject.SelectByID(e.Params["ID"].Get<int>());

                MetaObject.Property val = o.Values.Find(
                    delegate(MetaObject.Property idx)
                    {
                        return idx.Name == e.Params["Name"].Get<string>();
                    });
                if (val == null)
                {
                    val = new MetaObject.Property();
                    val.Name = e.Params["Name"].Get<string>();
                    o.Values.Add(val);

                    o.Save();
                }

                val.Value = e.Params["Signature"].Get<string>();

                val.Save();

                tr.Commit();
            }

            RaiseEvent(
                "Magix.Publishing.ReloadWebPart",
                e.Params);
        }

        /**
         * Level2: Will set the given Session Variable ['Name'] to the 'Value'. Useful for creating caches
         * of huge things, you need to occur really fast [or something]. Session Variables like
         * these can later be retrieved by its sibling method 'Magix.Common.GetSessionVariable'.
         * Things stored into the Session will be on a per user level [meaning, it'll take a LOT of 
         * memory on your server], but it will be very fast to retrieve later. Be Cautious here!
         */
        [ActiveEvent(Name = "Magix.Common.SetSessionVariable")]
        protected void Magix_Common_SetSessionVariable(object sender, ActiveEventArgs e)
        {
            Page.Session[e.Params["Name"].Get<string>()] = e.Params["Value"].Value;
        }

        /**
         * Level2: Will return the given Session Variable ['Name'] to the 'Value' output node. Useful for retrieving caches
         * of huge things, you need to occur really fast [or something]. Session Variables like
         * these can be set by its sibling method 'Magix.Common.SetSessionVariable'.
         * Things stored into the Session will be on a per user level [meaning, it'll take a LOT of 
         * memory on your server], but it will be very fast to retrieve later. Be Cautious here!
         */
        [ActiveEvent(Name = "Magix.Common.GetSessionVariable")]
        protected void Magix_Common_GetSessionVariable(object sender, ActiveEventArgs e)
        {
            e.Params["Value"].Value = Page.Session[e.Params["Name"].Get<string>()];
        }

        /**
         * Level2: Expects to be given a 'MetaViewName' which it will turn into a MetaView
         * object, which it will use as its foundation for exporting all MetaObjects of the 
         * MetaView's TypeName into a CSV file, which it will redirect the client's web 
         * browser towards. You can also override how the type is being rendered by 
         * adding up 'WhiteListColumns' and 'Type' parameters, which will override 
         * the default behavior for the MetaView. Set 'Redirect' to false if you wish
         * to stop redirecting to the newly created file to occur. Regardless the relative 
         * path to the file created will be returned as 'FileName' and the number of 
         * records as 'NoRecords'
         */
        [ActiveEvent(Name = "Magix.Common.ExportMetaView2CSV")]
        protected void Magix_Common_ExportMetaView2CSV(object sender, ActiveEventArgs e)
        {
            DateTime begin = DateTime.Now;

            Node n = new Node();

            n["FullTypeName"].Value = typeof(MetaObject).FullName + "-META";
            n["MetaViewName"].Value = e.Params["MetaViewName"].Value;

            // Signalizing 'everything' to the GetContentsOfClass event handler ...
            n["Start"].Value = 0;
            n["End"].Value = -1;

            // Making sure everything is sorted according to Newest FIRST ...!
            // TODO: There are tons of OTHER Grids in this system, e.g. SearchActions grid, 
            // which are NOT sorted correctly. Fix this at some point ... !
            n["Criteria"]["C1"]["Name"].Value = "Sort";
            n["Criteria"]["C1"]["Value"].Value = "Created";
            n["Criteria"]["C1"]["Ascending"].Value = false;

            // TODO: Support 'Massively Large DataSets' by allowing for 'CallBackEvent' to be supplied
            // ... which then will call CallBackEventHandler every n'th iteration or something ...
            RaiseEvent(
                "DBAdmin.Data.GetContentsOfClass",
                n);

            n["FileName"].Value = 
                "Tmp/" + 
                e.Params["MetaViewName"].Get<string>() + 
                "-" + 
                DateTime.Now.ToString("yyyy-MM-mm-HH-mm-ss", CultureInfo.InvariantCulture) + ".csv";

            if (e.Params.Contains("Redirect"))
                n["Redirect"].Value = e.Params["Redirect"].Value;

            RaiseEvent(
                "Magix.Common.ExportMetaViewObjectList2CSV",
                n);

            e.Params["FileName"].Value = n["FileName"].Value;
            e.Params["NoRecords"].Value = n["Objects"].Count;

            TimeSpan timer = DateTime.Now - begin;

            Node log = new Node();

            log["LogItemType"].Value = "Magix.Common.ExportMetaView2CSV";
            log["Header"].Value = "CSV File was Created";

            log["Message"].Value = string.Format(@"
CSV File '{0}' was created at {1} from MetaView '{5}' of MetaType '{2}'. 
The file has {3} records in it. Time to create file was {4} milliseconds",
                e.Params["FileName"].Value,
                DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"),
                e.Params["MetaViewTypeName"].Value,
                n["Objects"].Count,
                timer.TotalMilliseconds,
                e.Params["MetaViewName"].Get<string>());

            if (User.Current != null)
                log["ObjectID"].Value = User.Current.ID;

            RaiseEvent(
                "Magix.Core.Log",
                log);
        }

        /**
         * Level2: Will export a node list in 'Objects' List form to a CSV file, 
         * and redirect the client to that newly created CSV file. Set 'Redirect' to 
         * false to stop redirecting to the newly created CSV file to occur
         */
        [ActiveEvent(Name = "Magix.Common.ExportMetaViewObjectList2CSV")]
        protected void Magix_Common_ExportMetaViewObjectList2CSV(object sender, ActiveEventArgs e)
        {
            MetaView v = MetaView.SelectFirst(
                Criteria.Eq(
                    "Name", 
                    e.Params["MetaViewName"].Get<string>()));

            List<string> cols = new List<string>();
            foreach (MetaView.MetaViewProperty idx in v.Properties)
            {
                if (idx.Name.StartsWith(":"))
                    continue; // e.g. ":Save" or ":Delete" - System columns. Do NOT render to csv ...
                if (idx.Name.Contains(":"))
                {
                    // 'Complex prperty', e.g. "select:Gender.Sex:Sex".
                    // ALWAYS renders the 'Column Name' at the END ...!
                    cols.Add(idx.Name.Substring(idx.Name.LastIndexOf(":") + 1));
                }
                else
                {
                    cols.Add(idx.Name);
                }
            }

            using (TextWriter text = File.CreateText(Page.MapPath("~/" + e.Params["FileName"].Get<string>())))
            {
                // Rendering headers ...
                text.Write("ID, Created");
                foreach (string idx in cols)
                {
                    text.Write(",");
                    text.Write(idx);
                }
                text.WriteLine();

                // Rendering objects ...
                foreach (Node idx in e.Params["Objects"])
                {
                    text.Write(idx["ID"].Value.ToString() + ",");
                    text.Write(idx["Created"].Value.ToString());
                    foreach (string idxCol in cols)
                    {
                        text.Write(",");
                        if (idx["Properties"].Contains(idxCol))
                        {
                            string content = idx["Properties"][idxCol].Get<string>() ?? "";
                            content = content
                                .Replace("\\", "\\\\")
                                .Replace("\"", "'");
                            text.Write("\"" + content + "\"");
                        }
                    }
                    text.WriteLine();
                }
            }

            // Checking to see if we're supposed to redirect client browser
            if (!e.Params.Contains("Redirect") ||
                e.Params["Redirect"].Get<bool>())
            {
                Node xx = new Node();
                xx["URL"].Value = e.Params["FileName"].Get<string>();

                RaiseEvent(
                    "Magix.Common.RedirectClient",
                    xx);
            }
        }

        /**
         * Level2: Redirect clients to the given 'URL' parameter
         */
        [ActiveEvent(Name = "Magix.Common.RedirectClient")]
        protected void Magix_Common_RedirectClient(object sender, ActiveEventArgs e)
        {
            string path = e.Params["URL"].Get<string>();

            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Tried to redirect to a 'null' URL ...??");

            if (!path.StartsWith("http"))
                path = "~/" + path;

            AjaxManager.Instance.Redirect(path);
        }

        /**
         * Level2: Given a 'FileName' a 'Folder' and a 'MetaViewName' this method will 
         * try to import the given CSV file according to the given MetaView ['MetaViewName']
         * into your MetaObject data storage
         */
        [ActiveEvent(Name = "Magix.Common.ImportCSVFile")]
        protected void Magix_Common_ImportCSVFile(object sender, ActiveEventArgs e)
        {
            MetaView m = 
                MetaView.SelectFirst(
                    Criteria.Eq(
                        "Name", 
                        e.Params["MetaViewName"].Get<string>()));

            if (m == null)
                throw new ArgumentException(
                    @"Sorry, but you need to submit a 'MetaViewName' to an 
existing MetaView to import CSV files");

            if (string.IsNullOrEmpty(m.TypeName))
                throw new ArgumentException("Sorry, but your MetaView doesn't have a TypeName, which means we don't know which types to create from your CSV file");

            if (m.Properties.Count == 0)
                throw new ArgumentException(
                    @"Sorry, but your MetaView doesn't contain any properties, 
hence nothing will become imported, and this function call is useless. 
Add up properties that corresponds to the columns in your CSV file if you wish to import it");

            string folder = (e.Params["Folder"].Get<string>() ?? "").Trim().Trim('/');
            if (!string.IsNullOrEmpty(folder))
                folder += "/";

            string fileName = Page.MapPath(
                "~/" +
                folder +
                e.Params["FileName"].Get<string>());

            int count = ImportCSVFileFromMetaView(fileName, m);

            if (!e.Params.Contains("NoMessage") || 
                !e.Params["NoMessage"].Get<bool>())
            {
                Node node = new Node();

                node["Message"].Value = string.Format(@"
You've successfully imported {0} items of type '{1}' from the file '{2}' using the MetaView '{3}'",
                    count,
                    m.TypeName,
                    fileName,
                    m.Name);

                RaiseEvent(
                    "Magix.Core.ShowMessage",
                    node);
            }

            Node l = new Node();
            l["LogItemType"].Value = "Magix.Common.ImportCSVFile";
            l["Header"].Value = "File '" + fileName + "' was imported";
            l["Message"].Value = string.Format(@"
File '{0}' was imported, creating {1} items of type '{2}' from MetaView '{3}' on page '{4}'",
                fileName,
                count,
                m.TypeName,
                m.Name,
                Page.Request.Url.ToString());

            RaiseEvent(
                "Magix.Core.Log",
                l);
        }

        private int ImportCSVFileFromMetaView(string fileName, MetaView m)
        {
            int retVal = 0;
            List<string> viewCols = GetViewCols(m);
            using (TextReader reader = File.OpenText(fileName))
            {
                List<string> fileCols = GetFileCols(reader);

                using (Transaction tr = Adapter.Instance.BeginTransaction())
                {
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                            break;

                        List<string> values = GetFileValues(line);

                        CreateMetaObject(viewCols, fileCols, values, m.TypeName, fileName);
                        retVal += 1;
                    }
                    tr.Commit();
                }
            }
            return retVal;
        }

        /*
         * Creates, and saves, one meta object mapping the values into the viewCols from
         * indexing the values through the fileCols
         */
        private void CreateMetaObject(
            List<string> viewCols, 
            List<string> fileCols, 
            List<string> values, 
            string typeName,
            string fileName)
        {
            MetaObject o = new MetaObject();
            o.TypeName = typeName;
            string fileName2 = fileName;
            if (fileName2.IndexOf('\\') != -1)
                fileName2 = fileName2.Substring(fileName2.LastIndexOf('\\') + 1);
            o.Reference = "Import: " + fileName2;

            int idxNo = 0;
            foreach (string idxViewColumnName in viewCols)
            {
                MetaObject.Property p = new MetaObject.Property();
                p.Name = idxViewColumnName;

                int indexOfViewInFileCols = fileCols.IndexOf(idxViewColumnName);

                if (indexOfViewInFileCols > -1 && 
                    indexOfViewInFileCols < values.Count) // In case line in file is 'chopped' ...
                {
                    p.Value = values[indexOfViewInFileCols];
                }
                o.Values.Add(p);
                idxNo += 1;
            }

            o.Save();
        }

        /*
         * Given a string, which is one line from a CSV file, breaks it down into a
         * list of values it returns back to caller ...
         */
        private List<string> GetFileValues(string line)
        {
            line = line.Trim();
            List<string> values = new List<string>();
            bool isInside = true; // We start OUT by being inside ...
            bool hasFnutt = false;
            string buffer = "";
            int startNo = 0;
            if (line.Length > 0 && line[0] == '"')
            {
                startNo += 1;
                hasFnutt = true;
            }
            for (int idxNo = startNo; idxNo < line.Length; idxNo++)
            {
                if (isInside)
                {
                    // Inside of a "
                    char idxC = line[idxNo];
                    if (idxC == '\\')
                    {
                        idxNo += 1; // skipping this one, reading next

                        if (idxNo >= line.Length)
                            break; // Ops. At end ...! Buffer being added further down ...

                        idxC = line[idxNo];
                        buffer += idxC;
                    }
                    else if (hasFnutt && idxC == '"')
                    {
                        // Ending entity ...
                        hasFnutt = false;
                        values.Add(buffer);
                        buffer = "";
                        isInside = false;
                    }
                    else if (!hasFnutt && idxC == ',')
                    {
                        // Ending entity ...
                        values.Add(buffer);
                        buffer = "";
                        isInside = false;
                        idxNo -= 1; // Need this ...!
                    }
                    else
                    {
                        buffer += idxC;
                    }
                }
                else
                {
                    // OUTSIDE of the " [or ,]
                    char idxC = line[idxNo];
                    if (idxC == ',')
                    {
                        // Going inside again ...
                        // Need to discard the first '"' if existing ...
                        if (line.Length > idxNo + 1)
                        {
                            if (line[idxNo + 1] == '"')
                            {
                                hasFnutt = true;
                                idxNo += 1; // Skipping the " parts ...
                            }
                        }
                        isInside = true;
                    }
                }
            }
            values.Add(buffer); // LAST value ...
            return values;
        }

        /*
         * Helper for above, returns a List containing all 'columns' in the file. Expects
         * to be positioned at the beginning of the CSV file before being called ...
         */
        private List<string> GetFileCols(TextReader reader)
        {
            List<string> cols = new List<string>();

            string firstLine = reader.ReadLine();

            foreach (string idx in firstLine.Split(','))
            {
                string col = idx.Trim().Trim('"');
                cols.Add(col);
            }

            return cols;
        }

        /*
         * Helper for above. Returns a List of names from properties from the MetaView
         */
        private List<string> GetViewCols(MetaView m)
        {
            List<string> viewCols = new List<string>();
            foreach (MetaView.MetaViewProperty idx in m.Properties)
            {
                string name = idx.Name;
                if (name.IndexOf(":") == 0)
                    continue;

                if (name.Contains(":"))
                {
                    name = name.Substring(name.LastIndexOf(":"));
                }
                viewCols.Add(name);
            }
            return viewCols;
        }
    }
}
