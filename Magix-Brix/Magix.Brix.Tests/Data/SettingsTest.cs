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
using Magix.Brix.Data.Internal;
using Magix.Brix.Data.Adapters;

namespace Magix.Brix.Tests.Data
{
    [TestFixture]
    public class SettingsTest : BaseTest
    {
        [ActiveType]
        internal class Settings : ActiveType<Settings>
        {
            [ActiveType]
            internal class Setting : ActiveType<Setting>
            {
                [ActiveField]
                public string Name { get; set; }

                [ActiveField]
                public string Value { get; set; }
            }

            private static Settings _instance;
            private List<Setting> _values;

            private Settings()
            {
                _values = new List<Setting>();
            }

            public static void SetUp()
            {
                _instance = null;
            }

            public static Settings Instance
            {
                get
                {
                    // Checking to see if settings haven't been accessed before...
                    if (_instance == null)
                    {
                        // Checking to see if settings have been accessed and saved previously
                        // and then process was shut down or something...
                        _instance = Settings.SelectFirst();
                        if (_instance == null)
                        {
                            // Creating our onadonly settings object...
                            _instance = new Settings();
                            _instance.Save();
                        }
                    }
                    return _instance;
                }
            }

            [ActiveField]
            private List<Setting> Values
            {
                get
                {
                    return _values;
                }
                set
                {
                    _values = value;
                }
            }

            public string this[string key]
            {
                get
                {
                    Setting retVal = Values.Find(
                        delegate(Setting idx)
                        {
                            return idx.Name == key;
                        });
                    if (retVal != null)
                        return retVal.Value;
                    return null;
                }
                set
                {
                    Setting retVal = Values.Find(
                        delegate(Setting idx)
                        {
                            return idx.Name == key;
                        });
                    if (retVal != null)
                    {
                        retVal.Value = value;
                    }
                    else
                    {
                        Setting n = new Setting();
                        n.Name = key;
                        n.Value = value;
                        Values.Add(n);
                    }
                    // Must do it like this to get references right...!
                    this.Save();
                }
            }
        }

        [Test]
        public void VerifySaveWorks()
        {
            SetUp();
            Settings.SetUp();
            Settings.Instance["username"] = "thomas";
            string getUsername = Settings.Instance["username"];

            Assert.AreEqual("thomas", getUsername);
        }

        [Test]
        public void VerifySaveTwiceDoesntCreateNew()
        {
            SetUp();
            Settings.SetUp();
            int beforeCount = Settings.Setting.Count;
            Settings.Instance["username"] = "thomas";
            string getUsername = Settings.Instance["username"];
            Assert.AreEqual("thomas", getUsername);

            Settings.Instance["username"] = "mumbo";
            getUsername = Settings.Instance["username"];
            Assert.AreEqual("mumbo", getUsername);
            Assert.AreEqual(beforeCount + 1, Settings.Setting.Count);
        }

        [Test]
        public void VerifySaveMultipleValues()
        {
            SetUp();
            Settings.SetUp();
            int beforeCount = Settings.Setting.Count;
            Settings.Instance["one"] = "1";
            Settings.Instance["two"] = "2";
            Settings.Instance["three"] = "3";
            Settings.Instance["four"] = "4";
            Settings.Instance["five"] = "5";
            Settings.Instance["six"] = "6";
            Settings.Instance["seven"] = "7";

            Assert.AreEqual(beforeCount + 7, Settings.Setting.Count);

            Assert.AreEqual("1", Settings.Instance["one"]);
            Assert.AreEqual("2", Settings.Instance["two"]);
            Assert.AreEqual("3", Settings.Instance["three"]);
            Assert.AreEqual("4", Settings.Instance["four"]);
            Assert.AreEqual("5", Settings.Instance["five"]);
            Assert.AreEqual("6", Settings.Instance["six"]);
            Assert.AreEqual("7", Settings.Instance["seven"]);
        }
    }
}
