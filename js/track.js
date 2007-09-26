function TrackAjax(prefix)
{
    this.idRegEx = eval('/'+prefix+'_item_(\\d+)/');
    this.prefix = prefix;
}

TrackAjax.prototype = {
    init : function () {
        var self = this;
        $('SPAN.'+self.prefix+'_addremove').each(function (idx, elm) {
            if (!$('A.ready',elm).length) {
                $('SPAN.'+self.prefix+'_add A', elm).click(function () {
                    return self.add(this);
                });
                $('SPAN.'+self.prefix+'_remove A', elm).click(function () {
                    return self.remove(this);
                });
                $('A',elm).addClass('ready');
            }
        });
    },
    add : function (elm) {
        var self = this;
        this.request(elm.href, function (xml) {
            if ('ok' == $('status', xml).text()) {
                var id = self.getId(elm);
                $('SPAN.'+self.prefix+'_item_'+id+'/SPAN.'+self.prefix+'_add').hide();
                $('SPAN.'+self.prefix+'_item_'+id+'/SPAN.'+self.prefix+'_remove').show();
                self.onadd(elm, xml);
            } else {
                alert('Ошибка!');
            }
        });
        return false;
    },
    remove : function (elm) {
        var self = this;
        this.request(elm.href, function (xml) {
            if ('ok' == $('status', xml).text()) {
                var id = self.getId(elm);
                $('SPAN.'+self.prefix+'_item_'+id+'/SPAN.'+self.prefix+'_remove').hide();
                $('SPAN.'+self.prefix+'_item_'+id+'/SPAN.'+self.prefix+'_add').show();
                self.onremove(elm, xml);
            } else {
                alert('Ошибка!');
            }
        });
        return false;
    },
    request : function (href, callback) {
        $.ajax({
            type : "GET",
            url : href,
            data : {
                ajax : 1
            },
            dataType : 'xml',
            success: callback,
            error: function (xml, error, ex) {
                alert(xml);
                alert(ex);
                alert(error);
            }
        });
    },
    getId : function (elm) {
        return this.idRegEx.exec($(elm).parents('SPAN.'+this.prefix+'_addremove').attr('class'))[1];
    },
    onadd : function () {},
    onremove : function () {}
}

function findPos(obj) {
  var curleft = obj.offsetLeft || 0;
  var curtop = obj.offsetTop || 0;
  while (obj = obj.offsetParent) {
    curleft += obj.offsetLeft
    curtop += obj.offsetTop
  }
  return {x:curleft,y:curtop};
}

function TrackPlayer(placeholderId)
{
    this.id = placeholderId.replace(/-/g,'_') + '_player'; // с минусами глючит под IE в SWFObject
    this.placeholderId = placeholderId;
    this.placeholder = $('#' + placeholderId).get(0);
    this.player = false;
    this.state = 0;
}

TrackPlayer.prototype = {
    show : function (elm, href) {
        if (!this.player) {
            this._initPlayer();
        }
        this.state = 0;
        this._attach(elm);
        this._loadFile(href);
     },
    hide : function () {
        $(this.placeholder).css('left', -500); // через hide плеер глючит под IE :(
    },
    init : function () {
        var self = this;
        $('SPAN.track-preview').each(function (idx, elm) {
            $(elm).removeClass('track-preview');
            $(elm).addClass('track-preview-ready');
            $(elm).css('cursor', 'pointer');
            $(elm).click(function() {
                 var preview = $('SPAN.track-preview-file', elm).text();
                 if (!preview) {
                     $.ajax({
                        type : "GET",
                        url : $('SPAN.track-preview-encode', elm).text(),
                        data : {
                            ajax : 1
                        },
                        dataType : 'xml',
                        success: function (xml) {
                            if ('ok' == $('status', xml).text()) {
                                preview = $('file', xml).text();
                                $('SPAN.track-preview-file', elm).text(preview);
                                self.show(elm, preview);
                            } else {
                                alert('Не удалось создать preview.');
                            }
                        },
                        error: function (xml, error, ex) {
                            alert(xml);
                            alert(ex);
                            alert(error);
                        }
                    });
                } else {
                    self.show(elm, preview);
                }
            });
        });
    },
    _loadFile : function (href) {
        if ('function' == typeof(this.player.loadFile)) {

            //alert(href);
            //alert(encodeURIComponent(href));
            //alert(encodeURI(href));
            //alert(escape(href));
            //href = encodeURIComponent(href);
            //href = '/files/886/preview/%D0%A1%20%D0%B4%D0%BE%D0%B1%D1%80%D1%8B%D0%BC%20%D1%83%D1%82%D1%80%D0%BE%D0%BC,%20%D0%BB%D1%8E%D0%B1%D0%B8%D0%BC%D0%B0%D1%8F.mp3';
            //href = encodeURI(href);

            this.player.loadFile({file : href});
            this.player.sendEvent('playitem', 1);
        } else {
            var self = this;
            setTimeout(function () {
                self._loadFile(href);
    		}, 10);
        }
    },
    _initPlayer : function () {
        $(this.placeholder).css('position','absolute');
        var so = new SWFObject('/media/mp3player.swf',this.id,'100','20','8');
        so.addVariable("enablejs","true");
        so.addVariable("javascriptid",this.id);
        so.addVariable("backcolor","0x000000");
        so.addVariable("frontcolor","0xffffff");
        so.addVariable("lightcolor","0x0099CC");
        so.write(this.placeholderId);
        this.player = $('#'+this.id).get(0);
    },
    _attach : function (obj) {

        var pos = findPos($(obj).parents('tr,div').get(0));
        //alert(pos.x);
        //var pos = findPos($(obj).parents('tr').get(0));
        //alert(pos);
        //alert($(obj).parents('td').get(0).className);
    
        $(this.placeholder).css('top', pos.y -20);
        $(this.placeholder).css('left', pos.x + 0);
    }
}

var track_player;

$(function () {
    var basket = new TrackAjax('basket');
    basket.onadd = function () {
        var o = $('#tracks-in-basket-number');
        o.text('' + (parseInt(o.text()) + 1));
    };
    basket.onremove = function () {
        var o = $('#tracks-in-basket-number');
        o.text('' + (parseInt(o.text()) - 1));
    };
    basket.init();

    track_player = new TrackPlayer('track-preview-overlay');
    track_player.init();

    $().ajaxComplete(function () {
        basket.init();
        track_player.init();
    });
});

function getUpdate(typ,pr1,pr2,pid) {
	if (/*pid == track_player.id && */'state' == typ) {
	    if (0 == pr1 && 0 != track_player.state) {
	        track_player.hide();
	    }
	    track_player.state = pr1;
	}
};