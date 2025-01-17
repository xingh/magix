﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using NUnit.Framework;
using Magix.Brix.Types;

namespace Magix.Brix.Tests.Types
{
    [TestFixture]
    public class NodeSerializationTest
    {
        [Test]
        public void SimpleSerializationTest()
        {
            Node node = new Node("SomeNode");
            node["First"].Value = "1";
            node["Second"].Value = "2";
            string str = node.ToJSONString();
            Assert.AreEqual(
@"{""Name"":""SomeNode"",""Children"":[{""Name"":""First"",""Value"":""1""},{""Name"":""Second"",""Value"":""2""}]}", str);
        }

        [Test]
        public void SerializationDeserializationVerifyParentTest()
        {
            Node node = new Node("SomeNode");
            node["First"].Value = "1";
            node["Second"].Value = "2";
            string str = node.ToJSONString();
            Assert.AreEqual(
@"{""Name"":""SomeNode"",""Children"":[{""Name"":""First"",""Value"":""1""},{""Name"":""Second"",""Value"":""2""}]}", str);
            Node tmp = Node.FromJSONString(str);
            Assert.IsNotNull(tmp["First"].Parent);
        }

        [Test]
        public void SimpleSerializationTestNoRootName()
        {
            Node node = new Node();
            node["First"].Value = "1";
            node["Second"].Value = "2";
            string str = node.ToJSONString();
            Assert.AreEqual(
@"{""Children"":[{""Name"":""First"",""Value"":""1""},{""Name"":""Second"",""Value"":""2""}]}", str);
        }

        [Test]
        public void SimpleSerializationTestNoRootChildren()
        {
            Node node = new Node("SomeNode");
            node.Value = "thomas";
            string str = node.ToJSONString();
            Assert.AreEqual(
@"{""Name"":""SomeNode"",""Value"":""thomas""}", str);
        }

        [Test]
        public void SimpleSerializationTestNoRootChildrenNoValue()
        {
            Node node = new Node("SomeNode");
            string str = node.ToJSONString();
            Assert.AreEqual(
@"{""Name"":""SomeNode""}", str);
        }

        [Test]
        public void SimpleSerializationTestNoRootChildrenNoName()
        {
            Node node = new Node();
            node.Value = "thomas";
            string str = node.ToJSONString();
            Assert.AreEqual(
@"{""Value"":""thomas""}", str);
        }

        [Test]
        public void SimpleSerializationTestCompletelyEmpty()
        {
            Node node = new Node();
            string str = node.ToJSONString();
            Assert.AreEqual(
@"{}", str);
        }

        [Test]
        public void SimpleSerializationTestOnlyChildren()
        {
            Node node = new Node();
            node["One"].Value = "1";
            node["Two"].Value = "2";
            node["Three"].Value = "3";
            string str = node.ToJSONString();
            Assert.AreEqual(
@"{""Children"":[{""Name"":""One"",""Value"":""1""},{""Name"":""Two"",""Value"":""2""},{""Name"":""Three"",""Value"":""3""}]}", str);
        }

        [Test]
        public void SimpleSerializationTestChildrenWithChildren()
        {
            Node node = new Node();
            node["One"]["Two"].Value = "2";
            string str = node.ToJSONString();
            Assert.AreEqual(
@"{""Children"":[{""Name"":""One"",""Children"":[{""Name"":""Two"",""Value"":""2""}]}]}", str);
        }

        [Test]
        public void SerializeDeserializeThenUntie()
        {
            Node node = new Node("SomeNode");
            node["First"].Value = "1";
            node["Second"].Value = "2";
            node["Params"]["Par1"].Value = "one";
            node["Params"]["Par2"].Value = 2;
            string str = node.ToJSONString();
            Assert.AreEqual(
@"{""Name"":""SomeNode"",""Children"":[{""Name"":""First"",""Value"":""1""},{""Name"":""Second"",""Value"":""2""},{""Name"":""Params"",""Children"":[{""Name"":""Par1"",""Value"":""one""},{""Name"":""Par2"",""Value"":""2""}]}]}", str);
            Node result = Node.FromJSONString(str);
            Node tmp = result["Params"].UnTie();
            Assert.AreEqual(tmp["Par1"].Value, "one");
            Assert.AreEqual(tmp["Par2"].Value, "2");
        }
    }
}



















