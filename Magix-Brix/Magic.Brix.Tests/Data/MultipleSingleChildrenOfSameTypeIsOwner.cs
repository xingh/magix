﻿/*
 * MagicBrix - A Modular-based Framework for building Web Applications 
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * MagicBrix is licensed as GPLv3.
 */

using System;
using System.Reflection;
using System.Collections.Generic;
using NUnit.Core;
using NUnit.Framework;
using Magic.Brix.Data;
using Magic.Brix.Data.Internal;
using Magic.Brix.Data.Adapters;

namespace Magic.Brix.Tests.Data
{
    [TestFixture]
    public class MultipleSingleChildrenOfSameTypeIsOwner : BaseTest
    {
        [ActiveType]
        public class Role : ActiveType<Role>
        {
            [ActiveField]
            public int Value { get; set; }
            
            [ActiveField]
            public Role ChildRole { get; set; }
        }

        [ActiveType]
        public class User : ActiveType<User>
        {
            [ActiveField]
            public string Name{ get; set; }

            [ActiveField]
            public Role Role1 { get; set; }

            [ActiveField]
            public Role Role2 { get; set; }
        }

        [Test]
        public void SaveWithTwoDifferentValue()
        {
            User u1 = new User();
            u1.Name = "Thomas";
            u1.Role1 = new Role();
            u1.Role1.Value = 1;
            u1.Role2 = new Role();
            u1.Role2.Value = 2;
            u1.Save();

            User u2 = new User();
            u2.Name = "Kariem";
            u2.Role1 = new Role();
            u2.Role1.Value = 2;
            u2.Role2 = new Role();
            u2.Role2.Value = 1;
            u2.Save();

            User u1_after = User.SelectByID(u1.ID);
            Assert.AreEqual(1, u1_after.Role1.Value);
            Assert.AreEqual(2, u1_after.Role2.Value);

            User u2_after = User.SelectByID(u2.ID);
            Assert.AreEqual(2, u2_after.Role1.Value);
            Assert.AreEqual(1, u2_after.Role2.Value);
        }

        [Test]
        public void ShouldSaveObjectActiveField()
        {
            Role childRole = new Role();
            childRole.Value = 4;
            childRole.Save();

            Role role = new Role();
            role.Value = 5;
            role.ChildRole = childRole;
            role.Save();

            Role roleAfterSave = Role.SelectByID(role.ID);
            Assert.AreEqual(5, roleAfterSave.Value);
            Assert.AreEqual(4, roleAfterSave.ChildRole.Value);
        }
    }
}
