$.fn.pager = function(){
    var a = false;
    return this
    .find('div.pager').each(function(){
        var container = $(this).parent('.pagerContainer');
        if (container.length > 0) {
            $('a[@rel != "skip"]', this).each(function(){
                this.href = this.href + (this.href.substr(this.href.length-1) == '/' ? '' : '/') + container.attr('id') + '?request=' + $(this).attr('rel');
            })
            .click(function(){
                $.get(this.href, function(data){
                    container.html(data).standartActivate();
                });
                return false;
            });
        };
    }).end();
};
