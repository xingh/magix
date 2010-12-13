﻿/*
 * MagicBRIX - A Web Application Framework for ASP.NET
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * MagicBRIX is licensed as GPLv3.
 */

using System;
using System.Web;
using System.Web.UI;
using Magic.Brix.Loader;
using Magic.Brix.Types;
using HelloWorldTypes;

namespace HelloWorldController
{
    [ActiveController]
    public class HelloWorldController
    {
        [ActiveEvent(Name = "Page_Init_InitialLoading")]
        private void Page_Init_InitialLoading(object sender, ActiveEventArgs e)
        {
            (HttpContext.Current.Handler as Page).Title = "Magix-Brix Hello World";
            Node n = new Node();
            n["Message"].Value = "I am Marvin, I am your guide through the Universe...";
            ActiveEvents.Instance.RaiseLoadControl(
                "HelloWorldModules.Hello",
                "dyn",
                n);
        }

        [ActiveEvent(Name = "Hitchhike")]
        private void GetNextCounter(object sender, ActiveEventArgs e)
        {
            Counter c = Counter.SelectFirst();
            if (c == null)
                c = new Counter();
            c.Value += 1;
            c.Save();
            e.Params["Value"].Value = c.Value;
        }
    }
}






















