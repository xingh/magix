/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;
using System.Web.UI;
using System.Collections.Generic;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Effects
{
    public class EffectRollDown : Effect
    {
        public EffectRollDown()
            : base(null, 0)
        { }

        public EffectRollDown(Control control, int milliseconds)
			: base(control, milliseconds)
        { }

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.RollDown"; }
        }

        protected override string GetOptions()
        {
            return "";
        }

        protected override string RenderImplementation(bool topLevel, List<Effect> chainedEffects)
        {
            BaseWebControl tmp = this.Control as BaseWebControl;
            if (tmp != null)
            {
                tmp.Style.SetStyleValueViewStateOnly("display", "block");
                tmp.Style.SetStyleValueViewStateOnly("height", "");
            }
            return base.RenderImplementation(topLevel, chainedEffects);
        }
    }
}
