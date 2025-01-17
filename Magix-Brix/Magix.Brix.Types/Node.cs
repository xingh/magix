﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;

namespace Magix.Brix.Types
{
    /**
     * Level3: Helper class to pass around data in a "JSON kind of way" without having
     * to convert to JSON strings. Create a new instance, and just start appending
     * items to it like this;
     * <pre>
     *    Node n = new Node();
     *    n["Customer"]["Name"] = "John Doe";
     *    n["Customer"]["Adr"] = "NY";
     * </pre>
     * This is at the core of Magix, being the 'protocol' we're using to pass
     * data around within the system. If you don't understand this class, you're
     * in trouble! Make sure you understand, at least roughly, what this class does
     * if you'd like to code C# for Magix
     */
    [Serializable]
    public class Node : IList<Node>
    {
        // Implementation of list
        private readonly List<Node> _children = new List<Node>();

        // Parent node
        private Node _parent;

        private string _name;
        private object _value;

        /**
         * Default CTOR, creates a new node with no name and no value and no children
         */
        [DebuggerStepThrough]
        public Node()
            : this(null)
        { }

        /**
         * Creates a new node with the given name
         */
        [DebuggerStepThrough]
        public Node(string name)
            : this(name, null)
        { }

        /**
         * Creates a new node with the given name and the given value
         */
        [DebuggerStepThrough]
        public Node(string name, object value)
            : this(name, value, null)
        { }

        [DebuggerStepThrough]
        private Node(string name, object value, Node parent)
        {
            _name = name;
            _value = value;
            _parent = parent;
        }

        // TODO: Refactor to allow for types to be serialized
        /**
         * Level3: Will de-serialize the given JSON string into a Node structure. PS!
         * Even though Nodes can be serialized to JSON, the type information is lost,
         * meaning you can not go back again 100% correctly, since you're 'loosing
         * your types' when converting from Node to JSON. This is on our road map
         * for fixing, but currently not finished
         */
        [DebuggerStepThrough]
        public static Node FromJSONString(string json)
        {
            List<string> tokens = ExtractTokens(json);

            if (tokens.Count == 0)
                throw new ArgumentException("JSON string was empty");

            if (tokens[0] != "{")
                throw new ArgumentException("JSON string wasn't an object, missing opening token at character 0");

            int idxToken = 1;
            Node retVal = new Node();

            while (tokens[idxToken] != "}")
            {
                ParseToken(tokens, ref idxToken, retVal);
            }

            return retVal;
        }

        private static void ParseToken(IList<string> tokens, ref int idxToken, Node node)
        {
            if (tokens[idxToken] == "{")
            {
                // Opening new object...
                Node next = new Node();
                node._children.Add(next);
                next._parent = node;
                idxToken += 1;
                while (tokens[idxToken] != "}")
                {
                    ParseToken(tokens, ref idxToken, next);
                }
            }
            else if (tokens[idxToken] == ":")
            {
                switch(tokens[idxToken - 1])
                {
                    case "\"Name\"":
                        node.Name = tokens[idxToken + 1].Replace("\\\"", "\"").Trim('"');
                        break;
                    case "\"Value\"":
                        node.Value = tokens[idxToken + 1].Replace("\\\"", "\"").Trim('"');
                        break;
                    case "\"TypeName\"":
                        node.Value = Convert(node.Get<string>(), tokens[idxToken + 1]);
                        break;
                    case "\"Children\"":
                        idxToken += 1;
                        while (tokens[idxToken] != "]")
                        {
                            ParseToken(tokens, ref idxToken, node);
                        }
                        break;
                }
            }
            idxToken += 1;
            return;
        }

        private static object Convert(string value, string type)
        {
            object retVal = value;
            switch (type.Trim('\\').Trim('"'))
            {
                case "System.DateTime":
                    retVal = DateTime.ParseExact(value, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                    break;
                case "System.Boolean":
                    retVal = bool.Parse(value);
                    break;
                case "System.Int32":
                    retVal = int.Parse(value);
                    break;
                case "System.Decimal":
                    retVal = decimal.Parse(value, CultureInfo.InvariantCulture);
                    break;
            }
            return retVal;
        }

        [DebuggerStepThrough]
        private static List<string> ExtractTokens(string json)
        {
            List<string> tokens = new List<string>();
            for (int idx = 0; idx < json.Length; idx++)
            {
                switch (json[idx])
                {
                    case '{':
                    case '}':
                    case ':':
                    case ',':
                    case '[':
                    case ']':
                        tokens.Add(new string(json[idx], 1));
                        break;
                    case '"':
                        {
                            string str = "\"";
                            idx += 1;
                            while (true)
                            {
                                if (json[idx] == '"' && str.Substring(str.Length - 1, 1) != "\\")
                                {
                                    break;
                                }
                                str += json[idx];
                                idx += 1;
                            }
                            str += "\"";
                            tokens.Add(str);
                        } break;
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        break;
                    default:
                        throw new ArgumentException(
                            string.Format(
                                "Illegal token found in JSON string at character {0}, token was {1}, string was; \"{2}\"",
                                idx,
                                json[idx],
                                json.Substring(Math.Max(0, idx - 5))));
                }
            }
            return tokens;
        }

        /**
         * Level3: Returns the Parent node of the current node in the hierarchy. Normally
         * you'd never need to know the 'Parent' of a node, due to the intrinsic
         * logic of Magix, so be careful. If you're using this method, you're probably
         * doing something wrong on an architectural level. Be warned ...
         */
        public Node Parent
        {
            [DebuggerStepThrough]
            get { return _parent; }
        }

        /**
         * Level3: Assigns a Parent to the current node. Useful for 
         * moving nodes around
         */
        public void SetParent(Node n)
        {
            _parent = n;
        }

        /**
         * Level3: Returns the name of the node
         */
        public string Name
        {
            [DebuggerStepThrough]
            get { return _name; }

            [DebuggerStepThrough]
            set { _name = value; }
        }

        /**
         * Level3: Returns the value of the object
         */
        public object Value
        {
            [DebuggerStepThrough]
            get { return _value; }

            [DebuggerStepThrough]
            set { _value = value; }
        }

        /**
         * Level3: Returns the value of the object to type of T. Will try to cast and
         * throw exception if unsuccessful
         */
        [DebuggerStepThrough]
        public T Get<T>()
        {
            if (_value == null)
                return default(T);

            if (typeof(T) != _value.GetType())
            {
                switch (typeof(T).FullName)
                {
                    case "System.Int32":
                        return (T)(object)int.Parse(_value.ToString(), CultureInfo.InvariantCulture);
                    case "System.Boolean":
                        return (T)(object)bool.Parse(_value.ToString());
                    case "System.Decimal":
                        return (T)(object)decimal.Parse(_value.ToString(), CultureInfo.InvariantCulture);
                    case "System.DateTime":
                        return (T)(object)DateTime.ParseExact(_value.ToString(), "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                    case "System.String":
                        return (T)GetString(_value);
                }
            }

            return (T)_value;
        }

        private object GetString(object _value)
        {
            switch (_value.GetType().FullName)
            {
                case "System.Int32":
                    return _value.ToString();
                case "System.Boolean":
                    return _value.ToString();
                case "System.Decimal":
                    return ((decimal)_value).ToString(CultureInfo.InvariantCulture);
                case "System.DateTime":
                    return ((DateTime)_value).ToString("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                default:
                    return _value.ToString();
            }
        }

        /**
         * Level3: Returns the value of the object to type of T, and if
         * object is null it will return 
         * the "default" value. Be careful, might throw if type is wrong
         */
        [DebuggerStepThrough]
        public T Get<T>(T defaultValue)
        {
            if (_value == null)
                return defaultValue;

            return (T)_value;
        }

        /**
         * Level3: Returns the first node that matches the given Predicate. Will
         * search recursively. Be creful here, if you're dereferncing nodes that
         * don't exist inside your function, you might very well never return
         * from this method ... ;)
         */
        [DebuggerStepThrough]
        public Node Find(Predicate<Node> functor)
        {
            if (functor(this))
                return this;

            foreach (Node idx in _children)
            {
                Node tmp = idx.Find(functor);
                if (tmp != null)
                    return tmp;
            }
            return null;
        }

        /**
         * Level3: Returns true if node exists as a direct child only, and not search
         * recursive
         */
        [DebuggerStepThrough]
        public bool Exists(Predicate<Node> functor)
        {
            return Exists(functor, false);
        }

        /**
         * Level3: Returns true if node exists anywhere as a child or child of child
         */
        [DebuggerStepThrough]
        public bool ExistsDeep(Predicate<Node> functor)
        {
            return Exists(functor, true);
        }

        [DebuggerStepThrough]
        private bool Exists(Predicate<Node> functor, bool flat)
        {
            foreach (Node idx in this._children)
            {
                if (functor(idx))
                    return true;

                if (flat)
                    continue; // Don't traverse children if not supposed to

                bool tmp = idx.Exists(functor, flat);

                if (tmp)
                    return true;
            }
            return false;
        }

        /**
         * Level3: Will "disconnect" the node from its parent node. Useful
         * for removing nodes and trees out of Node structures
         */
        [DebuggerStepThrough]
        public Node UnTie()
        {
            _parent.Remove(this);
            _parent = null;
            return this;
        }

        /**
         * Level3: Returns the node with the given Name. If that node doesn't exist
         * a new node will be created with the given name and appended into the
         * Children collection and then be returned. Please notice that this
         * method CREATES NODES during DE-REFERENCING. And it is its INTENT TOO! ;)
         */
        public Node this[string name]
        {
            [DebuggerStepThrough]
            get
            {
                Node retVal = _children.Find(
                    delegate(Node idx)
                    {
                        return idx.Name == name;
                    });
                if (retVal == null)
                {
                    retVal = new Node(name, null, this);
                    _children.Add(retVal);
                }
                return retVal;
            }
            [DebuggerStepThrough]
            set
            {
                int idxNo = -1;
                if (_children.Exists(
                    delegate(Node idx)
                    {
                        idxNo += 1;
                        return idx.Name == name;
                    }))
                {
                    _children[idxNo] = value;
                }
                else
                    _children.Add(value);
            }
        }

        /**
         * Level3: Returns the index of the given item, if it exists within the children
         * collection. Otherwise it returns -1
         */
        [DebuggerStepThrough]
        public int IndexOf(Node item)
        {
            return _children.IndexOf(item);
        }

        /**
         * Level3: Inserts a new item into the children collection
         */
        [DebuggerStepThrough]
        public void Insert(int index, Node item)
        {
            _children.Insert(index, item);
            item._parent = this;
        }

        /**
         * Level3: Removes node at given index
         */
        [DebuggerStepThrough]
        public void RemoveAt(int index)
        {
            _children[index]._parent = null;
            _children.RemoveAt(index);
        }

        /**
         * Level3: Returns the n'th node
         */
        public Node this[int index]
        {
            [DebuggerStepThrough]
            get
            {
                return _children[index];
            }
            [DebuggerStepThrough]
            set
            {
                _children[index] = value;
                value._parent = this;
            }
        }

        /**
         * Level3: Adds a new node to the collection
         */
        [DebuggerStepThrough]
        public void Add(Node item)
        {
            string itemName = item.Name;
            if (!this.Exists(
                delegate(Node idx2)
                {
                    return itemName == idx2.Name;
                },
                true))
                _children.Add(item);
            else
                this[item.Name] = item;
            item._parent = this;
        }

        /**
         * Level3: Adds a new node to the collection
         */
        [DebuggerStepThrough]
        public void AddForceNew(Node item)
        {
            string itemName = item.Name;
            _children.Add(item);
            item._parent = this;
        }

        /**
         * Level3: Adds a range of nodes to collection
         */
        [DebuggerStepThrough]
        public void AddRange(IEnumerable<Node> items)
        {
            foreach (Node idx in items)
            {
                Add(idx);
            }
        }

        /**
         * Level3: Entirely empties the collection
         */
        [DebuggerStepThrough]
        public void Clear()
        {
            foreach (Node idx in _children)
            {
                idx._parent = null;
            }
            _children.Clear();
        }

        /**
         * Level3: Returns true if node exists within child collection [flat]
         */
        [DebuggerStepThrough]
        public bool Contains(Node item)
        {
            return _children.Contains(item);
        }

        /**
         * Level3: Returns true if node exists within child collection [flat]
         */
        [DebuggerStepThrough]
        public bool Contains(string itemName)
        {
            return _children.Exists(
                delegate(Node idx)
                {
                    return idx.Name == itemName;
                });
        }

        [DebuggerStepThrough]
        public void CopyTo(Node[] array, int arrayIndex)
        {
            foreach (Node idx in _children)
            {
                idx._parent = null;
            }
            _children.CopyTo(array, arrayIndex);
        }

        /**
         * Level3: Returns the number of items in the children collection
         */
        public int Count
        {
            [DebuggerStepThrough]
            get { return _children.Count; }
        }

        public bool IsReadOnly
        {
            [DebuggerStepThrough]
            get { return false; }
        }

        /**
         * Level3: Removes the given node from the child collection
         */
        [DebuggerStepThrough]
        public bool Remove(Node item)
        {
            bool retVal = _children.Remove(item);

            if (retVal)
                item._parent = null;

            return retVal;
        }

        /**
         * Level3: Supports enumerating items
         */
        [DebuggerStepThrough]
        public IEnumerator<Node> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        [DebuggerStepThrough]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        /**
         * Level4: Will return name/value and number of children as a string
         */
        [DebuggerStepThrough]
        public override string ToString()
        {
            string retVal = "";
            retVal += _name;

            if (_value != null)
                retVal += ":" + _value;

            if (_children != null)
                retVal += ":" + _children.Count;

            return retVal;
        }

        /**
         * Level3: Will translate the Node structure to a JSON string. Useful
         * for passing stuff around to other systems, and integrating with client-side
         * etc. Be warned! No TYPE information is being passed, so you cannot build
         * the same Node structure by reversing the method and call FromJSON after
         * creating JSON out of your node
         */
        [DebuggerStepThrough]
        public string ToJSONString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            bool hasChild = false;

            if(!string.IsNullOrEmpty(Name))
            {
                builder.AppendFormat(@"""Name"":""{0}""", Name);
                hasChild = true;
            }
            if(Value != null)
            {
                if (hasChild)
                    builder.Append(",");
                string value = "";

                // ORDER COUNTS !!
                switch(Value.GetType().FullName)
                {
                    case "System.String":
                        value = Value.ToString().Replace("\"", "\\\"");
                        break;

                    case "System.DateTime":
                        value = ((DateTime)Value).ToString("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                        break;

                    case "System.Decimal":
                        value = ((Decimal)Value).ToString(CultureInfo.InvariantCulture);
                        break;

                    default:
                        value = Value.ToString();
                        break;
                }
                builder.AppendFormat(@"""Value"":""{0}""", value);
                hasChild = true;

                string typeName = Value.GetType().FullName;
                if (typeName != typeof(string).FullName)
                {
                    builder.Append(",");
                    builder.AppendFormat(@"""TypeName"":""{0}""", typeName);
                }
            }
            if(_children.Count > 0)
            {
                if (hasChild)
                    builder.Append(",");
                builder.Append(@"""Children"":[");
                bool first = true;

                foreach(Node idx in _children)
                {
                    if (!first)
                        builder.Append(",");
                    first = false;
                    builder.Append(idx.ToJSONString());
                }
                builder.Append("]");
            }
            builder.Append("}");
            return builder.ToString();
        }

        /**
         * Level3: Will sort the nodes according to your given comparison delegate
         */
        [DebuggerStepThrough]
        public void Sort(Comparison<Node> del)
        {
            _children.Sort(del);
        }

        /**
         * Level3 Returns the outer most parent node, the top node of the hierarchy
         */
        [DebuggerStepThrough]
        public Node RootNode()
        {
            Node tmp = this;
            while (tmp.Parent != null)
                tmp = tmp.Parent;
            return tmp;
        }

        /**
         * Level3: Clones the given node. Deep copy of all nodes
         */
        [DebuggerStepThrough]
        public Node Clone()
        {
            return Clone(this);
        }

        [DebuggerStepThrough]
        private Node Clone(Node node)
        {
            Node r = new Node();
            r.Name = node.Name;
            r.Value = node.Value;
            foreach (Node idx in node)
            {
                r.Add(Clone(idx));
            }
            return r;
        }
    }
}
