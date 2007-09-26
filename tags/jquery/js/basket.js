function TrackAjax(prefix)
{
    this.idRegEx = /basket_item_(\d+)/;
    this.prefix = prefix;
}

TrackAjax.prototype = {
    init : function () {
        var self = this;
        $('SPAN.'+self.prefix+'_addremove SPAN.'+self.prefix+'_add A').click(function () {
            return self.add(this);
        });
        $('SPAN.'+self.prefix+'_addremove SPAN.'+self.prefix+'_remove A').click(function () {
            return self.remove(this);
        });
    },
    add : function (elm) {
        var self = this;
        this.request(elm.href, function (xml) {
            if ('ok' == $('status', xml).text()) {
                var id = self.idRegEx.exec($(elm).parents('SPAN.'+self.prefix+'_addremove').attr('class'))[1];
                $('SPAN.'+self.prefix+'_item_'+id+'/SPAN.'+self.prefix+'_add').hide();
                $('SPAN.'+self.prefix+'_item_'+id+'/SPAN.'+self.prefix+'_remove').show();
                self.onadd();
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
                var id = self.idRegEx.exec($(elm).parents('SPAN.'+self.prefix+'_addremove').attr('class'))[1];
                $('SPAN.'+self.prefix+'_item_'+id+'/SPAN.'+self.prefix+'_remove').hide();
                $('SPAN.'+self.prefix+'_item_'+id+'/SPAN.'+self.prefix+'_add').show();
                self.onremove();
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
    onadd : function () {},
    onremove : function () {}
}

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

    var encoding = new TrackAjax('encoding');
    encoding.init();
});