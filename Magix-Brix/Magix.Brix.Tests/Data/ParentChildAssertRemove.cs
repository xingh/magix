﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using System.Reflection;
using System.Collections.Generic;
using NUnit.Core;
using NUnit.Framework;
using Magix.Brix.Data;
using Magix.Brix.Types;
using Magix.Brix.Data.Internal;
using Magix.Brix.Data.Adapters;

namespace Magix.Brix.Tests.Data
{
    [TestFixture]
    public class ParentChildAssertRemove : BaseTest
    {
        [ActiveType]
        internal class Role : ActiveType<Role>
        {
            public Role()
            {
                Roles = new LazyList<Role>();
            }

            [ActiveField]
            public string Name { get; set; }

            [ActiveField]
            public LazyList<Role> Roles { get; set; }
        }

        /* The whole point of this test is to verify that if we remove a child document
         * from the collection of a parent node, and the "IsOwner" is true, then that
         * document should be removed from the collection of the child, and also in fact
         * removed entirely from the data-storage...!
         */
        [Test]
        public void CreateParentChildRemoveChildSaveParent()
        {
            SetUp();

            Role r1 = new Role();
            r1.Name = "parent";

            Role r2 = new Role();
            r2.Name = "child";

            r1.Roles.Add(r2);
            r1.Save();

            Role rafter1 = Role.SelectByID(r1.ID);
            Assert.AreEqual(1, rafter1.Roles.Count);
            Assert.AreEqual("parent", rafter1.Name);
            Assert.AreEqual("child", rafter1.Roles[0].Name);

            rafter1.Roles.Clear();
            rafter1.Save();

            // Now the "parent" role should have NO children...!
            // And the "child" should NOT exist in database...!
            Role rafter2 = Role.SelectByID(rafter1.ID);
            Assert.AreEqual(0, rafter2.Roles.Count);
            Assert.AreEqual("parent", rafter2.Name);

            // Checking to see if the role also is physically REMOVED from hte database
            // and not only removed from the parent...!
            Assert.AreEqual(1, Role.Count);
        }

        /* The whole point of this test is to verify that if we remove a child document
         * from the collection of a parent node, and the "IsOwner" is true, then that
         * document should be removed from the collection of the child, and also in fact
         * removed entirely from the data-storage...!
         */
        [Test]
        public void CreateParentAndTWOChildrenRemoveONEChildSaveParent()
        {
            SetUp();

            Role r1 = new Role();
            r1.Name = "parent";

            Role r2 = new Role();
            r2.Name = "child1";

            Role r3 = new Role();
            r3.Name = "child2";

            r1.Roles.Add(r2);
            r1.Roles.Add(r3);
            r1.Save();

            Assert.AreEqual(3, Role.Count);

            Role rafter1 = Role.SelectByID(r1.ID);
            Assert.AreEqual(2, rafter1.Roles.Count);
            Assert.AreEqual("parent", rafter1.Name);
            Assert.AreEqual("child1", rafter1.Roles[0].Name);
            Assert.AreEqual("child2", rafter1.Roles[1].Name);

            rafter1.Roles.RemoveAt(0);
            rafter1.Save();

            // Now the "parent" role should have NO children...!
            // And the "child" should NOT exist in database...!
            Role rafter2 = Role.SelectByID(rafter1.ID);
            Assert.AreEqual(1, rafter2.Roles.Count);
            Assert.AreEqual("parent", rafter2.Name);
            Assert.AreEqual("child2", rafter2.Roles[0].Name);

            // Checking to see if the role also is physically REMOVED from hte database
            // and not only removed from the parent...!
            Assert.AreEqual(2, Role.Count);
        }
    }
}
