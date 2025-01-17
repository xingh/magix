/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

(function() {

  MUX.Effect = MUX.klass();
  MUX.Effect.prototype = {
    init: function(el, opt) {
      this.initEffect(el, opt);
    },

    initEffect: function(el, opt) {
      this.options = MUX.extend({
        duration: 1000,
        onStart: function() { },
        onFinished: function() { },
        condition: function() { return true; },
        onRender: null,
        transition: 'Linear',
        autoStart: true
      }, opt || {});
      if (el) {
        this.element = MUX.$(el);
      }
      if (this.options.autoStart) {
        this.execute();
      }
    },

    execute: function() {
      if (!this.options.condition.call(this)) {
        return;
      }
      this.options.onStart.call(this);
      this.startTime = new Date().getTime();
      this.finishOn = this.startTime + (this.options.duration);
      this.loop();
    },

    loop: function() {
      if (this.stopped) {
        return;
      }
      var T = this;

      setTimeout(function() {
        var cur = new Date().getTime();
        var dlt = (cur - T.startTime) / (T.options.duration);
        if (cur >= T.finishOn) {
          T.options.onFinished.call(T);
        } else {
          T.render(dlt);
          T.loop();
        }
      }, 10);
    },

    render: function(pos) {
      switch (this.options.transition) {
        case 'Linear':
          break;
        case 'Accelerating':
          pos = Math.cos((pos * (Math.PI / 2)) + Math.PI) + 1;
          break;
        case 'Explosive':
          pos = Math.sin(pos * (Math.PI / 2));
          break;
      }
      this.options.onRender.call(this, pos);
    }
  };


  // Common for all Client-Side effects...
  MUX.Effect.ClientSide = MUX.klass();
  MUX.extend(MUX.Effect.ClientSide.prototype, MUX.Effect.prototype);
  MUX.extend(MUX.Effect.ClientSide.prototype, {

    init: function(el, opt) {
      this.initC(el, opt);
    },

    initC: function(el, opt) {
      this.options = MUX.extend({
        onStart: this.onStart,
        onFinished: this.onFinished,
        onRender: this.onRender,
        joined: []
      }, opt || {});
      this.initEffect(el, this.options);
    },

    onStart: function() {
      this.doStart();
      var e = this.options.joined;
      var idx = e.length;
      while (idx--) {
        e[idx].doStart();
      }
    },

    onRender: function(pos) {
      this.doRender(pos);
      var e = this.options.joined;
      var idx = e.length;
      while (idx--) {
        e[idx].doRender(pos);
      }
    },

    onFinished: function() {
      this.doFinish();
      var e = this.options.joined;
      var idx = e.length;
      while (idx--) {
        e[idx].doFinish();
      }
      if (this.options.chained) {
        this.options.chained.execute(this);
      }
    }
  });

  // Effect Border...
  MUX.Effect.Border = MUX.klass();
  MUX.extend(MUX.Effect.Border.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Border.prototype, {

    init: function(el, opt) {
      this.initBorder(el, opt);
    },

    initBorder: function(el, opt) {
      this.options = MUX.extend({
        border: 0
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {
      this.element.setStyle('borderStyle', 'dashed');
      this.element.setStyle('borderWidth', '1px');
    },

    doRender: function(pos) {
      var x = parseInt(pos * this.options.border, 10);
      this.element.setStyle('borderWidth', x + 'px');
    },

    doFinish: function() {
      this.element.setStyle('borderWidth', this.options.border + 'px');
    }
  });

  // Effect CssClass...
  MUX.Effect.CssClass = MUX.klass();
  MUX.extend(MUX.Effect.CssClass.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.CssClass.prototype, {

    init: function(el, opt) {
      this.initBorder(el, opt);
    },

    initBorder: function(el, opt) {
      this.options = MUX.extend({
        cssClass: null
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {
      this.element.toggleClassName(this.options.cssClass);
    },

    doRender: function() { },

    doFinish: function() {
    }
  });

  // Effect Opacity...
  MUX.Effect.Opacity = MUX.klass();
  MUX.extend(MUX.Effect.Opacity.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Opacity.prototype, {

    init: function(el, opt) {
      this.initOpacity(el, opt);
    },

    initOpacity: function(el, opt) {
      this.options = MUX.extend({
        from: 0,
        to: 1.0
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {
      this.element.setOpacity(this.options.from);
      this.element.setStyle('display', '');
    },

    doRender: function(pos) {
      this.element.setOpacity(this.options.from + ((this.options.to - this.options.from) * pos));
    },

    doFinish: function() {
      this.element.setOpacity(this.options.to);
      if (this.options.to == 0) {
        this.element.setStyle('display', 'none');
      }
    }
  });

  // Effect CssClass...
  MUX.Effect.FocusSelect = MUX.klass();
  MUX.extend(MUX.Effect.FocusSelect.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.FocusSelect.prototype, {

    init: function(el, opt) {
      this.options = MUX.extend({
        isFocus: false
      }, opt || {});
      this.initC(el, opt);
    },

    doStart: function() { },

    doRender: function() { },

    doFinish: function() {
      this.element.focus();
      if (this.element.select && !this.options.isFocus) {
        this.element.select();
      }
    }
  });

  // Effect Border...
  MUX.Effect.Highlight = MUX.klass();
  MUX.extend(MUX.Effect.Highlight.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Highlight.prototype, {

    init: function(el, opt) {
      this.initC(el, opt);
    },

    doStart: function() {
      this._startColor = this.element.getStyle('backgroundColor');
    },

    doRender: function(pos) {
      var a = 1.0 - pos;
      this.element.setStyle('backgroundColor', 'rgba(255, 255, 0,' + a);
    },

    doFinish: function() {
      this.element.setStyle('backgroundColor', this._startColor);
    }
  });

  // Effect Move...
  MUX.Effect.Move = MUX.klass();
  MUX.extend(MUX.Effect.Move.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Move.prototype, {

    init: function(el, opt) {
      this.initMove(el, opt);
    },

    initMove: function(el, opt) {
      this.options = MUX.extend({
        x: -1,
        y: -1
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {
      this.startL = parseInt(this.element.getStyle('left'), 10) || 0;
      this.startT = parseInt(this.element.getStyle('top'), 10) || 0;
    },

    doRender: function(pos) {
      if (this.options.x != -1) {
        var deltaL = ((this.options.x) - this.startL) * pos;
        var newL = parseInt((deltaL) + this.startL, 10);
        this.element.setStyle('left', (newL) + 'px');
      }
      if (this.options.y != -1) {
        var deltaT = ((this.options.y) - this.startT) * pos;
        var newT = parseInt((deltaT) + this.startT, 10);
        this.element.setStyle('top', (newT) + 'px');
      }
    },

    doFinish: function() {
      if (this.options.x != -1) {
        this.element.setStyle('left', this.options.x + 'px');
      }
      if (this.options.y != -1) {
        this.element.setStyle('top', this.options.y + 'px');
      }
    }
  });

  // Effect RollUp...
  MUX.Effect.RollUp = MUX.klass();
  MUX.extend(MUX.Effect.RollUp.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.RollUp.prototype, {

    init: function(el, opt) {
      this.initC(el, opt);
    },

    doStart: function() {
      this._fromHeight = this.element.dimensions().y;
      this._overflow = this.element.getStyle('overflow');
      this.element.setStyle('overflow', 'hidden');
      this.element.setStyle('display', 'block');
    },

    doRender: function(pos) {
      this.element.setStyle('height', ((1.0 - pos) * this._fromHeight) + 'px'); ;
    },

    doFinish: function() {
      this.element.setStyle('display', 'none');
      this.element.setStyle('height', '');
      if (this.options.overflow) {
        this.element.setStyle('overflow', this.options.overflow);
      } else {
        this.element.setStyle('overflow', this._overflow);
      }
    }
  });

  // Effect RollDown...
  MUX.Effect.RollDown = MUX.klass();
  MUX.extend(MUX.Effect.RollDown.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.RollDown.prototype, {

    init: function(el, opt) {
      this.initC(el, opt);
    },

    doStart: function() {
      this._toHeight = this.element.dimensions().y;
      this.element.setStyle('height', '0px');
      this.element.setStyle('display', 'block');
      this._overflow_x = this.element.getStyle('overflowX');
      this._overflow_y = this.element.getStyle('overflowY');
      this.element.setStyle('overflow', 'hidden');
    },

    doRender: function(pos) {
      this.element.setStyle('height', parseInt(this._toHeight * pos, 10) + 'px');
    },

    doFinish: function() {
      this.element.setStyle('height', '');
      this.element.setStyle('overflowX', this._overflow_x);
      this.element.setStyle('overflowY', this._overflow_y);
    }
  });

  // Effect Size...
  MUX.Effect.Size = MUX.klass();
  MUX.extend(MUX.Effect.Size.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Size.prototype, {

    init: function(el, opt) {
      this.initSize(el, opt);
    },

    initSize: function(el, opt) {
      this.options = MUX.extend({
        x: -1,
        y: -1
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {
      this.startSize = this.element.dimensions();
    },

    doRender: function(pos) {
      if (this.options.y != -1) {
        var deltaH = (this.options.y - this.startSize.y) * pos;
        var newH = parseInt(deltaH + this.startSize.y, 10);
        this.element.setStyle('height', newH + 'px');
      }
      if (this.options.x != -1) {
        var deltaW = (this.options.x - this.startSize.x) * pos;
        var newW = parseInt(deltaW + this.startSize.x, 10);
        this.element.setStyle('width', newW + 'px');
      }
    },

    doFinish: function() {
      if (this.options.y != -1) {
        this.element.setStyle('height', this.options.y + 'px');
      }
      if (this.options.x != -1) {
        this.element.setStyle('width', this.options.x + 'px');
      }
    }
  });

  // Effect Scroll Browser...
  MUX.Effect.ScrollBrowser = MUX.klass();
  MUX.extend(MUX.Effect.ScrollBrowser.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.ScrollBrowser.prototype, {

    init: function(el, opt) {
      this.initScroll(el, opt);
    },

    initScroll: function(el, opt) {
      this.options = opt;
      this.initC(el, this.options);
    },

    getScrollPos: function() {
      var scrollTop = document.body.scrollTop;
      if (scrollTop == 0) {
        if (window.pageYOffset)
          scrollTop = window.pageYOffset;
        else
          scrollTop = (document.body.parentElement) ? document.body.parentElement.scrollTop : 0;
      }
      return scrollTop;
    },

    setScrollPos: function(val) {
      window.scrollTo(0, val);
    },

    doStart: function() {
      this.startPos = this.getScrollPos();
    },

    doRender: function(pos) {
      var nPos = (1.0 - pos) * this.startPos;
      this.setScrollPos(nPos);
    },

    doFinish: function() {
      this.setScrollPos(0);
    }
  });

  // Effect Timeout...
  MUX.Effect.Timeout = MUX.klass();
  MUX.extend(MUX.Effect.Timeout.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Timeout.prototype, {

    init: function(el, opt) {
      this.initC(el, opt);
    },

    doStart: function() { },

    doRender: function() { },

    doFinish: function() { }
  });

  // Effect Toggle...
  MUX.Effect.Toggle = MUX.klass();
  MUX.extend(MUX.Effect.Toggle.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Toggle.prototype, {

    init: function(el, opt) {
      this.initToggle(el, opt);
    },

    initToggle: function(el, opt) {
      this.options = MUX.extend({
        isFade: false
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {

      var options = {};
      MUX.extend(options, this.options);
      options.autoStart = true;
      options.chained = null;
      options.joined = [];

      if (this.options.isFade) {
        if (this.element.getStyle('display') == 'none') {
          options.from = 0;
          options.to = 1;
        } else {
          options.from = 1;
          options.to = 0;
        }
        new MUX.Effect.Opacity(this.element.id, options);
      } else {
        if (this.element.getStyle('display') == 'none') {
          new MUX.Effect.RollDown(this.element.id, options);
        } else {
          new MUX.Effect.RollUp(this.element.id, options);
        }
      }
    },

    doRender: function(pos) { },
    doFinish: function() { }
  });

  // Effect Toggle Elements...
  MUX.Effect.ToggleElements = MUX.klass();
  MUX.extend(MUX.Effect.ToggleElements.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.ToggleElements.prototype, {

    init: function(el, opt) {
      this.initToggleElements(el, opt);
    },

    initToggleElements: function(el, opt) {
      this.options = MUX.extend({
        elements: []
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {
      if (this.options.animationMode != 'MultipleOpen' &&
      this.element.getStyle('display') != 'none') {
        return;
      }
      var hiding;
      var idx = this.options.elements.length;
      while (idx--) {
        var el = MUX.$(this.options.elements[idx]);
        if (el.getStyle('display') != 'none') {
          hiding = el;
          break;
        }
      }
      var animMode = this.options.animationMode;

      if (animMode == 'NoAnimations') {
        // No animations here...
        hiding.setStyle('display', 'none');
        this.element.setStyle('display', '');
      } else {
        // Copying options from this effect...
        var options = {};
        MUX.extend(options, this.options);
        options.autoStart = false;

        // Creating actual effect
        var effect = new MUX.Effect.RollUp(el.id, options);

        if (animMode == 'Chained') {
          effect.options.chained = new MUX.Effect.RollDown(this.element.id, options);
        } else if (animMode == 'Joined') {
          effect.options.joined = [new MUX.Effect.RollDown(this.element.id, options)];
        }

        // Executing actual effect
        effect.execute();
      }
    },

    doRender: function(pos) {
    },

    doFinish: function() {
    }
  });

  // Effect Slide...
  MUX.Effect.Slide = MUX.klass();
  MUX.extend(MUX.Effect.Slide.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Slide.prototype, {

    init: function(el, opt) {
      this.initSlide(el, opt);
    },

    initSlide: function(el, opt) {
      this.options = MUX.extend({
        offset: 0
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {
      MUX.extend(this.element.parentNode, MUX.Element.prototype);
      this._width = this.element.parentNode.dimensions().x;
      this._startMargin = parseInt(this.element.getStyle('marginLeft'), 10);
    },

    doRender: function(pos) {
      var fullDelta = (this.options.offset * this._width) - this._startMargin;
      var n = parseInt(pos * fullDelta, 10);
      this.element.setStyle('marginLeft', (n + this._startMargin) + 'px');
    },

    doFinish: function() {
      this.element.setStyle('marginLeft', (this.options.offset * this._width) + 'px');
    }
  });

  // Effect Generic...
  MUX.Effect.Generic = MUX.klass();
  MUX.extend(MUX.Effect.Generic.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Generic.prototype, {

    init: function(el, opt) {
      this.initGeneric(el, opt);
    },

    initGeneric: function(el, opt) {
      this.options = MUX.extend({
        offset: 0,
        start: function() { },
        loop: function() { },
        end: function() { }
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {
      this.options.start();
    },

    doRender: function(pos) {
      this.options.loop(pos);
    },

    doFinish: function() {
      this.options.end();
    }
  });
})();
