/*
 * MagicUX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - Ra-Software, Inc. - info@rasoftwarefactory.com
 * MagicUX is licensed as GPLv3.
 */

using System;
using System.Web.UI;

namespace Magic.UX.Effects
{
    public class EffectHighlight : Effect
    {
        public EffectHighlight()
            : base(null, 0)
        { }

        public EffectHighlight(Control control, int milliseconds)
			: base(control, milliseconds)
		{ }

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.Highlight"; }
        }

        protected override string GetOptions()
        {
            return "";
        }
    }
}
