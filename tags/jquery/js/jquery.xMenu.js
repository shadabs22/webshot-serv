jQuery.fn.isMenuVert = function() {
    return !this.is('.xMenu') || this.is('.vertMenu');
};

jQuery.fn.xMenu = function(firstMouseOver) {
    return this.xMenuFirstStep().xMenuSecondStep();
};

jQuery.fn.xMenuFirstStep = function(firstMouseOver) {
    this.each(function(){
        $('a', this)
        .css('padding', ($.browser.msie && $.browser.msieVersion < 7 )  ? '3px' : '3px 0px 3px 0px')
        .each(function(){
            if ($('>span', this).length == 0) $(this).html('<span>' + $(this).html() + '</span>');
        })
        .find('>span').each(function(){
            if (!$.browser.msie || $.browser.msieVersion > 6) $(this).css('marginLeft', '3px');
        });
        if (firstMouseOver) $(this).mouseover(firstMouseOver);
    });
    return this;
};

jQuery.fn.xMenuSecondStep = function(cfg) {
    $.xMenuHideTimeOut = false;
    cfg = cfg || {};
    //cfg.padding = cfg.padding || 4;
    var showSubs = function(div){
        if (mouseIsDown) return true;
        var div = div ? ( $(div).is('a') ? $(div).parent() : $(div) ) : $(this),
            a = div.find('>a'),
            vert = div.is('.xMenu') ? false : div.parent().isMenuVert(),
            subDiv = div.find('>div:first'),
            mainDiv = div.is('.xMenu') ? div : div.parents('div.xMenu');
        if (mainDiv.get(0).xMenuHideTimeOut) clearTimeout(mainDiv.get(0).xMenuHideTimeOut);
        div.parent().find('div.xMenuSub').hide();
        var left = ( $.windowSize().width >= (div.left() + div.width() + (vert?subDiv.width():(subDiv.width()-div.width())))) ? ( vert ? div.width() : 0 ) : ( (vert ? 0 : 1 )*(div.width()) - subDiv.width() ),
            top = ( $.canvasHeight() >= (a.height() + a.top() + subDiv.height()) ? (( vert ? 0 : a.height() ) + a.get(0).offsetTop ) : -subDiv.height() + (vert ? subDiv.height() : 0 ) );
        if (div.parent().css('position') == 'static') left = left + div.left();
        subDiv
        .css({
            'display': 'block',
            'position': 'absolute'
        })
        .css('width', subDiv.is('.widthSet') ? subDiv.css('width') : ((subDiv.width() + 10) + 'px'))
        .addClass('widthSet')
        .find('>div>a').css('width', '100%').end()
        .css({
            left: left + 'px',
            top: top + 'px'
        });
    },
    hideSubs = function(div, quick){
        var div = div ? ( $(div).is('a') ? $(div).parent() : $(div) ) : $(this),
            a = div.find('>a'),
            subDiv = div.find('>div:first'),
            mainDiv = div.is('.xMenu') ? div : div.parents('div.xMenu');
        if (quick) subDiv.hide();
        mainDiv.get(0).xMenuHideTimeOut = setTimeout(function(){subDiv.hide();}, 250);
    },
    _this = this.get(0);
    
    _this.first = function(){
        if (cfg.onShow) cfg.onShow(_this);
        showSubs(_this);
    };
    this.each(function(){
        $(this)
        .hover(
            function(){
                if (cfg.onShow) cfg.onShow(this);
                return true;
            },
            function(){
                if (cfg.onHide) cfg.onHide(this);
                return true;
            }
        )
        .find('div.xMenuItem[>div]').add(this).each(function(){
            if ($(this).parent().isMenuVert() && !$(this).is('.xMenu')) {
                var span = $('>a>span', this).get(0);
                if (span) span.innerHTML = span.innerHTML + '<span>&#160;&#187;</span>';
            };
            $(this).hover(
                function(){
                    showSubs(this);
                },
                function(){
                    if (!mouseIsDown) hideSubs(this);
                }
            )
            .find('>a')
            .mousedown(function(){
                mouseIsDown = true;
                hideSubs(this, true);
            })
            .mouseup(function(){
                mouseIsDown = false;
                showSubs(this);
            });
        })
    });
    return this;
};
