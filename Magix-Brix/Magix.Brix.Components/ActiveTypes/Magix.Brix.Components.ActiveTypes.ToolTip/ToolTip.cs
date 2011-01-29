﻿/*
 * Magix - A Modular-based Framework for building Web Applications 
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * Magix is licensed as GPLv3.
 */

using System;
using System.Configuration;
using System.Globalization;
using System.Collections.Generic;
using Magix.Brix.Data;

namespace Magix.Brix.Components.ActiveTypes
{
    public sealed class ToolTip
    {
        [ActiveType]
        private class Tip : ActiveType<Tip>
        {
            [ActiveField]
            public string Value { get; set; }

            [ActiveField]
            public int No { get; set; }
        }

        [ActiveType]
        private class TipPosition : ActiveType<TipPosition>
        {
            [ActiveField]
            public int Position { get; set; }

            [ActiveField]
            public string Seed { get; set; }
        }

        private static ToolTip _instance;

        private ToolTip()
        { }

        public static ToolTip Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(ToolTip))
                    {
                        if (_instance == null)
                        {
                            _instance = new ToolTip();
                        }
                    }
                }
                return _instance;
            }
        }

        public void CreateTip(string tip)
        {
            Tip t = new Tip();
            t.Value = tip;
            t.No = Tip.Count;
            t.Save();
        }

        public int Count
        {
            get { return Tip.Count; }
        }

        private static string GetToolTip(string seed, int addition)
        {
            TipPosition pos = TipPosition.SelectFirst(
                Criteria.Eq("Seed", seed));
            if (pos == null)
            {
                pos = new TipPosition();
                pos.Position = 0 - addition;
                pos.Seed = seed;
            }
            pos.Position = pos.Position + addition;
            if (pos.Position >= Tip.Count - 1)
                pos.Position = 0;
            if (pos.Position < 0)
                pos.Position = Tip.Count - 1;
            Tip retVal = Tip.SelectFirst(Criteria.Eq("No", pos.Position));
            if (retVal == null)
            {
                // Defaulting to first ...
                pos.Position = 0;
                retVal = Tip.SelectFirst(Criteria.Eq("No", pos.Position));
                if (retVal == null)
                {
                    pos.Save();
                    return null; // No tips here .....
                }
            }
            pos.Save();
            return retVal.Value;
        }

        public string Previous(string seed)
        {
            return GetToolTip(seed, -1);
        }

        public string Next(string seed)
        {
            return GetToolTip(seed, 1);
        }
    }
}