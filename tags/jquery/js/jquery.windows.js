$.wins = [];

window.iframeLoaded = function(i){};

$.fn.adjustWin = function(startHeight) {
    $('div.winContent', this).css('height', (startHeight ? startHeight : this.height() - $('div.winTitle', this).height() - 15)+'px');
};

$.fn.adjustWinToContent = function() {
    return this.adjustWinHeightToContent().adjustWinWidthToContent();
};

$.fn.finishWindow = function() {
    return this.adjustWinToContent().css('visibility', 'visible').placeIframeBelowWin();
};

$.fn.placeIframeBelowWin = function() {
    if ($.browser.msie) {
        var win = this;
        $('body')
        .append('<iframe frameborder="0" marginwidth="0" marginheight="0"></iframe>')
        .find('iframe:last')
        .css({
            position: 'absolute',
            zIndex: $(win).css('zIndex')-1,
            left: $(win).left()+'px',
            top: $(win).top()+'px',
            width: $(win).width()+'px',
            height: $(win).height()+'px',
            opacity: 0
        });
    };
    return this;
};

$.fn.removeIframeBelowWin = function() {
    if ($.browser.msie) {
        var z = $(this).css('zIndex')-1;
        $('iframe[@frameborder="0"]').each(function(){
            if ($(this).css('zIndex') == z) $(this).remove();
        });
    };
    return this;
};

$.fn.adjustWinWidthToContent = function() {
    if (!this.cfg.width) {
        var cont = $('.winSubContent', this),
            contentWidth = cont.width(),
            maxWidth = parseInt(this.cfg.maxWidth*$.bodyWidth()),
            width = this.width();
        while (width < maxWidth && contentWidth*1.2 > width) {
            width = width+10;
            this.css('width', width+'px');
        };
        this.putToCenter().adjustWin();
    };
    return this;
};

$.fn.adjustWinHeightToContent = function() {
    if (!this.cfg.height) {
        var cont = $('.winSubContent', this),
                contentHeight = cont.height(),
                maxHeight = parseInt(this.cfg.maxHeight*$.bodyHeight()),
                height = this.height();
        while (height < maxHeight && contentHeight*1.2 > height) {
            height = height+10;
            this.css('height', height+'px');
        };
        this.putToCenter().adjustWin();
    };
    return this;
};



$.fn.win = function(cfg) {
    cfg = cfg || {};
    cfg.modal = cfg.modal ? cfg.modal : false;
    cfg.center = cfg.center || false;
    cfg.title = cfg.title || false;
    cfg.id = cfg.id || false;
    cfg.width = cfg.width ? cfg.width : false;
    cfg.height = cfg.height ? cfg.height : false;
    cfg.maxWidth = cfg.maxWidth || 0.8;
    cfg.maxHeight = cfg.maxHeight || 0.8;
    cfg.time = cfg.time || 0; // 0 - show until close
    cfg.resizeable = cfg.resizeable || "yes";
    cfg.top = cfg.top || 100;
    cfg.left = cfg.left || 100;
    cfg.content = cfg.content || '';
    cfg.onReady = cfg.onReady || false;
    cfg.onClose = cfg.onClose || false;
    cfg.status = cfg.status ? cfg.status : false;
    cfg.startHeight = cfg.height ? cfg.height : 100;
    cfg.startWidth = cfg.width ? cfg.width : 100;
    
	if (cfg.modal) {
        var w = $.windowSize();
        $('body').append('<div id="xWinOverlay"></div>')
        .find('#xWinOverlay')
        .css({
            width: (20 + w.width) + 'px',
            height: $.canvasHeight() + 'px'
        });

        document.body.style.overflow = 'hidden';
    };
    this.append('<div class="win"></div>');
    var win = this.find('div:last'),
    
    winDiv = win.get(0);
    win.cfg = cfg;
    
    winDiv.close = function(){
        if (cfg.onClose) cfg.onClose(win);
        win.removeIframeBelowWin().remove();
        if (cfg.modal) $('#xWinOverlay').remove();
        document.body.style.overflow = '';
        win = null;
    };
    
    winDiv.loadWin = function(url, params){
        params = params || {};
        winDiv.url = url;
        $.get(url, params, function(data){
            $('.winSubContent', win).html(data).standartActivate();
            if (win.cfg.content.onReady) win.cfg.content.onReady(win);
            if (win.cfg.lib && win.cfg.lib.exe) win.cfg.lib.exe(win);
            win.finishWindow();
        });
    };
    
    win.html((cfg.title?'<div class="winTitle">'+cfg.title+'</div>':'')+'<div class="winContent"><table cellpadding="0" cellspacing="0"><tbody><tr><td class="winSubContent"></td></tr></tbody></table></div><div class="winStatus">'+(cfg.status?cfg.status:'')+'</div>'+(cfg.title?'<input class="winClose" value="&#215;" type="button">':'')+(cfg.resizeable=='yes'?'<div class="winResize"></div>':''))
    .css({
        width: cfg.startWidth + 'px',
        height: cfg.startHeight + 'px',
        top: cfg.top + 'px',
        left: cfg.left + 'px',
        visibility: 'hidden',
        zIndex: $.topZ() + 10
    })
    //.bind('selectstart', function(){return false}) // preventing select in IE
    .find('div.winTitle')
        .css({height: $('input.winClose', this).height()+'px'})
        .bind('selectstart', function(){return false}) // preventing select in IE
        .mousedown(function(e){
            e.preventDefault();
            var topWin = $.topWin(), topZ = topWin.css('zIndex');
            topWin.css({zIndex: win.css('zIndex')});
            win.css({zIndex: topZ});
        })
        .end()
    .find('input.winClose').click(winDiv.close).end()
    .find('div.winStatus')
        .css({height: '14px'})
        .end()
    .jqDrag($('div.winTitle', win))
    .jqResize($('div.winResize', win), function(){win.adjustWin();});
    
    var cont = $('.winSubContent', win);
    if (win.cfg.content.url) winDiv.loadWin(win.cfg.content.url);
    else if (cfg.content.tagName) cont.get(0).appendChild(cfg.content);
    else cont.append(cfg.content);
    
    //~ win.adjustWin(cfg.startHeight);
    //~ if (cfg.center) win.putToCenter();
    
    
    $.wins.push(win);
    if (cfg.id) win.attr('id', cfg.id);
    
    // adjusting size
    if (!win.cfg.content.url) win.finishWindow();
    
    if (win.cfg.onReady) win.cfg.onReady(win);
     
    if (win.cfg.time !=0 ) setTimeout(function(){
        $('input.winClose', win).trigger('click');
    }, win.cfg.time);
};
$.topWin = function() {
    var win = false, z = 0;
    if ($.wins.length > 0) $($.wins).each(function(){
        if ($(this).css('zIndex') && $(this).css('zIndex') > z) {
            win = this;
            z = $(this).css('zIndex');
        };
    });
    return win;
};
$.topZ = function() {
    var topWin = $.topWin();
    return parseInt(topWin ? topWin.css('zIndex') : 101);
};

$.fn.nearestWindow = function(){
    var o = this;
    while (!o.is('body') && !o.is('.win')) o = o.parent();
    return o;
};

$.fn.frameContent = function(){
    var f = this.get(0);
    try {
        var c = (f.contentWindow) ? f.contentWindow : f.contentDocument;
        if (!c || !c.document) return false;
        return c.document.documentElement.innerHTML;
    } catch (e) {
        return false;
    };
};
