﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;

namespace Magix.Brix.Loader
{
    /**
     * Level3: Mark your controllers with this Attribute. Notice that an Active Controller must
     * have a default constructor taking zero parameters. This constructor should also
     * ideally execute FAST since all controllers in your Magix-Brix project will be 
     * instantiated once every request.
     */
    public class ActiveControllerAttribute : Attribute
    {
    }
}
