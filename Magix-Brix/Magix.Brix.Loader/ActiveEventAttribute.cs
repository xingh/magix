﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;

namespace Magix.Brix.Loader
{
    /**
     * Level3: Mark your methods with this attribute to make then handle Magix.Brix Active Events. 
     * The Name property is the second argument to the RaiseEvent, or the "name" of the 
     * event being raised. You can mark your methods with multiple instances of this 
     * attribute to catch multiple events in the same event handler. However, as a general
     * rule of thumb it's often better to have one method handling one event
     */
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true, Inherited=true)]
    public class ActiveEventAttribute : Attribute
    {
        /**
         * Level3: Name of event
         */
        public string Name;

        /**
         * Level3: If true, method will be called asynchronous. Note that if you use Async event handlers 
         * then you **CANNOT** in ANY ways access any parts of the Page, Response, Request, HttpContext
         * etc since these might very well be discarded and returned back to the client when you try
         * to access it. You can also obviously NOT write to the
         * response at all using Async event handlers. Do not LoadControls or anything else like
         * that. Async event handlers are PURELY meant for "fire and forget" event handlers, and
         * in general terms you should in fact try to avoid them all together if you can, due to
         * these problems.
         * And in fact, due to the current implementation of the Active Type design pattern in
         * Magix-Brix, you cannot even access the database. However, for "fire and forget" event handlers
         * that for instance is supposed to just ping some webservice or something similar, they
         * can be quite useful since you then can asynchronously make those calls, but still
         * return ASAP to the client with response.
         */
        public bool Async;

        public ActiveEventAttribute()
        { }
    }
}
