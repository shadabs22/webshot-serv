$.canvasHeight = function(){
    var w = $.windowSize(), w1 = $('body').height();
    return (w1 > w.height) ? w1 : w.height;
};

$.windowSize = function(){
    var myWidth = 0, myHeight = 0;
    if( typeof( window.innerWidth ) == 'number' ) {
        //Non-IE
        myWidth = window.innerWidth;
        myHeight = window.innerHeight;
    } else if( document.documentElement && ( document.documentElement.clientWidth || document.documentElement.clientHeight ) ) {
        //IE 6+ in 'standards compliant mode'
        myWidth = document.documentElement.clientWidth;
        myHeight = document.documentElement.clientHeight;
    } else if( document.body && ( document.body.clientWidth || document.body.clientHeight ) ) {
        //IE 4 compatible
        myWidth = document.body.clientWidth;
        myHeight = document.body.clientHeight;
    }
    return {"height":myHeight, "width":myWidth};
};

$.bodySize = function(){
    var w, h;
    if (self.innerHeight) {
        w = self.innerWidth;
        h = self.innerHeight;
    } else if (document.documentElement && document.documentElement.clientHeight) {
        w = document.documentElement.clientWidth;
        h = document.documentElement.clientHeight;
    } else if (document.body) {
        w = document.body.clientWidth;
        h = document.body.clientHeight;
    };
    return (w || h) ? {width: w, height: h} : 0;
};

$.bodyWidth = function(){
    var w;
    if (self.innerHeight) {
        w = self.innerWidth;
    } else if (document.documentElement && document.documentElement.clientHeight) {
        w = document.documentElement.clientWidth;
    } else if (document.body) {
        w = document.body.clientWidth;
    };
    return w ? w : 0;
};

$.bodyHeight = function(){
    var h;
    if (self.innerHeight) {
        h = self.innerHeight;
    } else if (document.documentElement && document.documentElement.clientHeight) {
        h = document.documentElement.clientHeight;
    } else if (document.body) {
        h = document.body.clientHeight;
    };
    return h ? h : 0;
};

$.getScrolling = function(){
    var x, y;
    if (self.pageYOffset) {
    x = self.pageXOffset;
    y = self.pageYOffset;
    } else if (document.documentElement && document.documentElement.scrollTop) {
    x = document.documentElement.scrollLeft;
    y = document.documentElement.scrollTop;
    } else if (document.body) {
    x = document.body.scrollLeft;
    y = document.body.scrollTop;
    };
    return (x || y) ? {scrollX: x, scrollY: y} : 0;
};

$.fn.nearestWindow = function(){
    var o = this;
    do {o = o.parent();} while (!o.is('body') && !o.is('.win'));
    return o;
};


jQuery.fn.scrollLeft = function() {
	if ( this[0] == window || this[0] == document )
		return self.pageXOffset ||
			jQuery.boxModel && document.documentElement.scrollLeft ||
			document.body.scrollLeft;

	return this[0].scrollLeft;
};

jQuery.fn.scrollTop = function() {
	if ( this[0] == window || this[0] == document )
		return self.pageYOffset ||
			jQuery.boxModel && document.documentElement.scrollTop ||
			document.body.scrollTop;

	return this[0].scrollTop;
};

$.fn.pos = function(){
    if (this.length == 0) return false;
    var w = this.nearestWindow(),
        deltaX = w.is('body') ? 0 : w.left(),
        deltaY = w.is('body') ? 0 : w.top(),
        o = this.get(0),
        x = y = 0;
    while (o) { 
        x += o.offsetLeft;
        y += o.offsetTop;
        o = o.offsetParent;
    };
    return {left: x-deltaX, top: y-deltaY};
};


$.fn.left = function(){
    if (this.length == 0) return 0;
    var w = this.nearestWindow(),
        delta = w.is('body') ? 0 : w.left(),
        o = this.get(0),
        x = 0;
    while (o) { 
        x += o.offsetLeft;
        o = o.offsetParent;
    };
    return x-delta;
};

$.fn.top = function(){
    if (this.length == 0) return 0;
    var w = this.nearestWindow(),
        delta = w.is('body') ? 0 : w.top(),
        o = this.get(0),
        y = 0;
    while (o) { 
        y += o.offsetTop;
        o = o.offsetParent;
    };
    return y-delta;
};

$.fn.width = function(){
    if (this.get(0) == self) return self.innerWidth;
    if (this.length == 0) return 0;
    var o = this.get(0), w, cssWidth = o.style ? o.style.width : false;
    if (cssWidth && /^\d+/.test(cssWidth)) return parseInt(cssWidth);
    if (o.offsetWidth) {
        w = o.offsetWidth;
    } else if (o.clip && o.clip.width) {
        w = o.clip.width;
    } else if (o.style && o.style.pixelWidth) {
        w = o.style.pixelWidth;
    };
    return w ? parseInt(w) : 0;
};

$.fn.height = function(){
    if (this.get(0) == self) return self.innerHeight;
    if (this.length == 0) return 0;
    var o = this.get(0), h, cssHeight = o.style ? o.style.height : false;
    if (cssHeight && /^\d+/.test(cssHeight)) return parseInt(cssHeight);
    if (o.offsetWidth) {
        h = o.offsetHeight;
    } else if (o.clip && o.clip.width) {
        h = o.clip.height;
    } else if (o.style && o.style.pixelWidth) {
        h = o.style.pixelHeight;
    };
    return h ? parseInt(h) : 0;
};

$.fn.getSize = function(){
    if (this.length == 0) return false;
    var o = this.get(0), w, h, cssWidth = o.style ? o.style.width : false, cssHeight = o.style ? o.style.height : false;
    if (cssWidth && cssHeight && /^\d+/.test(cssWidth) && /^\d+/.test(cssHeight)) return {width: parseInt(cssWidth), height: parseInt(cssHeight)};
    if (o.offsetWidth) {
        w = o.offsetWidth;
        h = o.offsetHeight;
    } else if (o.clip && o.clip.width) {
        w = o.clip.width;
        h = o.clip.height;
    } else if (o.style && o.style.pixelWidth) {
        w = o.style.pixelWidth;
        h = o.style.pixelHeight;
    };
    return (w || h) ? {width: parseInt(w), height: parseInt(h)} : 0;
};

$.fn.putToCenter = function(){
    var bodySize = $.bodySize(),
        scrolling = $.getScrolling(),
        scrollX = (scrolling) ? scrolling.scrollX : 0,
        scrollY = (scrolling) ? scrolling.scrollY : 0,
        elemSize = this.getSize();
    return this.css({
        position: 'absolute',
        left: Math.round(bodySize.width/2 - elemSize.width/2 + scrollX) + "px",
        top: Math.round(bodySize.height/2 - elemSize.height/2 + scrollY) + "px"
    });
};

$.fn.offsetLeft = function(){
    var o = this.get(0), p = o.offsetParent;
    if (!$.browser.msie || p.tagName == 'HTML' || p.tagName == 'BODY' || p.style.position == 'absolute' ) return o.offsetLeft;
    var outerHTML = p.outerHTML.replace(/\r\n/g,'');
    var w = /^(<[^>]+>).+(<\/[^>]+>)$/.exec(outerHTML), w = w[1]+w[2];
    p.style.width = $(p).width() + 'px';
    var offset = o.offsetLeft, children = $('>*', p);
    $(p).after(w).next().append(children).end().remove();
    return offset;
};

$.fn.zIndex = function(z, downDeep){
    var o = this.get(0), p = o.offsetParent;
    if (!$.browser.msie || p.tagName == 'HTML') return this.css('zIndex', z);
    if (downDeep) $(downDeep, p).css('zIndex', 1);
    var o = this;
    while (!o.is('html')) {
        if (o.css('position') == 'relative') o.css('zIndex', z);
        o = o.parent();
    };
    return this;
};

jQuery.fn.offset = function(options, returnObject) {
	var x = 0, y = 0, elem = this[0], parent = this[0], op, sl = 0, st = 0, options = jQuery.extend({ margin: true, border: true, padding: false, scroll: true }, options || {});
	do {
		x += parent.offsetLeft || 0;
		y += parent.offsetTop  || 0;

		// Mozilla and IE do not add the border
		if (jQuery.browser.mozilla || jQuery.browser.msie) {
			// get borders
			var bt = parseInt(jQuery.css(parent, 'borderTopWidth')) || 0;
			var bl = parseInt(jQuery.css(parent, 'borderLeftWidth')) || 0;

			// add borders to offset
			x += bl;
			y += bt;

			// Mozilla removes the border if the parent has overflow property other than visible
			if (jQuery.browser.mozilla && parent != elem && jQuery.css(parent, 'overflow') != 'visible') {
				x += bl;
				y += bt;
			}
		}

		if (options.scroll) {
			// Need to get scroll offsets in-between offsetParents
			op = parent.offsetParent;
			do {
				sl += parent.scrollLeft || 0;
				st += parent.scrollTop  || 0;

				parent = parent.parentNode;

				// Mozilla removes the border if the parent has overflow property other than visible
				if (jQuery.browser.mozilla && parent != elem && parent != op && jQuery.css(parent, 'overflow') != 'visible') {
					y += parseInt(jQuery.css(parent, 'borderTopWidth')) || 0;
					x += parseInt(jQuery.css(parent, 'borderLeftWidth')) || 0;
				}
			} while (parent != op);
		} else
			parent = parent.offsetParent;

		if (parent && (parent.tagName.toLowerCase() == 'body' || parent.tagName.toLowerCase() == 'html')) {
			// Safari doesn't add the body margin for elments positioned with static or relative
			if ((jQuery.browser.safari || (jQuery.browser.msie && jQuery.boxModel)) && jQuery.css(parent, 'position') != 'absolute') {
				x += parseInt(jQuery.css(op, 'marginLeft')) || 0;
				y += parseInt(jQuery.css(op, 'marginTop'))  || 0;
			}
			break; // Exit the loop
		}
	} while (parent);

	if ( !options.margin) {
		x -= parseInt(jQuery.css(elem, 'marginLeft')) || 0;
		y -= parseInt(jQuery.css(elem, 'marginTop'))  || 0;
	}

	// Safari and Opera do not add the border for the element
	if ( options.border && (jQuery.browser.safari || jQuery.browser.opera) ) {
		x += parseInt(jQuery.css(elem, 'borderLeftWidth')) || 0;
		y += parseInt(jQuery.css(elem, 'borderTopWidth'))  || 0;
	} else if ( !options.border && !(jQuery.browser.safari || jQuery.browser.opera) ) {
		x -= parseInt(jQuery.css(elem, 'borderLeftWidth')) || 0;
		y -= parseInt(jQuery.css(elem, 'borderTopWidth'))  || 0;
	}

	if ( options.padding ) {
		x += parseInt(jQuery.css(elem, 'paddingLeft')) || 0;
		y += parseInt(jQuery.css(elem, 'paddingTop'))  || 0;
	}

	// Opera thinks offset is scroll offset for display: inline elements
	if (options.scroll && jQuery.browser.opera && jQuery.css(elem, 'display') == 'inline') {
		sl -= elem.scrollLeft || 0;
		st -= elem.scrollTop  || 0;
	}

	var returnValue = options.scroll ? { top: y - st, left: x - sl, scrollTop:  st, scrollLeft: sl }
	                                 : { top: y, left: x };

	if (returnObject) { jQuery.extend(returnObject, returnValue); return this; }
	else              { return returnValue; }
};

window.resizeToBody = function(body, targetSize){
    var size = body.getSize(),
        deltaX,
        deltaY, 
        absDeltaX,
        absDeltaY,
        dirX,
        dirY,
        previousX = false,
        previousY = false;
    while ( (previousX != size.width || previousY != size.height) && ( targetSize.width != size.width || targetSize.height != size.height ) ) {
        deltaX = targetSize.width - size.width;
        deltaY = targetSize.height - size.height; 
        absDeltaX = Math.abs(deltaX);
        absDeltaY = Math.abs(deltaY);
        dirX = deltaX > 0 ? 1 : -1;
        dirY = deltaY > 0 ? 1 : -1;
        window.resizeBy(Math.ceil(absDeltaX/2)*dirX, Math.ceil(absDeltaY/2)*dirY);
        previousX = size.width;
        previousY = size.height;
        size = body.getSize();
    };
    return size;
};

