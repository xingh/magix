﻿/*
 * Magix - A Modular-based Framework for building Web Applications 
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * Magix is licensed as GPLv3.
 */

using Magix.Brix.Types;

namespace Magix.Brix.Loader
{
    /**
     * Optional interface you can mark your Modules with. If you do your Modules will be called the
     * first time they load through the InitialLoading with whatever object you choose to RaiseYour 
     * events with.
     */
    public interface IModule
    {
        /**
         * Will be called when the Module is initially loaded with the initializationObject
         * parameter you pass into your LoadModule - if any.
         */
        void InitialLoading(Node node);
    }
}