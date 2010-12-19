﻿/*
 * Magix - A Modular-based Framework for building Web Applications 
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * Magix is licensed as GPLv3.
 */

using System;

namespace Magix.Brix.Loader
{
    /**
     * Mark your methods with this attribute to make then catch Magix.Brix Active Events. 
     * The Name property is the second argument to the RaiseEvent, or the "name" of the 
     * event being raised. You can mark your methods with multiple instances of this 
     * attribute to catch multiple events in the same event handler.
     */
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true, Inherited=true)]
    public class ActiveEventAttribute : Attribute
    {
        /**
         * Name of event
         */
        public string Name;

        /**
         * If true, method will be called asynchronous. Note that if you use Async event handlers 
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
         * return ASAP to client with response.
         */
        public bool Async;

        /**
         * Empty CTOR
         */
        public ActiveEventAttribute()
        { }

        /**
         * CTOR taking the name of the event you want your method to catch.
         */
        public ActiveEventAttribute(string name)
        {
            Name = name;
        }

        /**
         * CTOR taking the name of the event you want your method to catch, plus a boolean indicating
         * if the event handler should be called asynchronously.
         */
        public ActiveEventAttribute(string name, bool async)
        {
            Name = name;
            Async = async;
        }
    }
}