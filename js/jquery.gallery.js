$.fn.gallery = function(){
    this
    .find('div.gallery').each(function(){
        var pics = this.innerHTML;
        $(this).empty()
        .css({
            width: $(this).parent().width()+'px',
            overflow: 'auto'
        })
        .html('<table cellpadding="10" cellspacing="0"><tbody><tr></tr></tbody></table>')
        .find('tr').append(pics).find('img').wrap('<td></td>').css('float', 'none').end().end()
        .find('td').hover(
            function(){$(this).css('background', 'yellow');},
            function(){$(this).css('background', 'none');}
        ).end()
        .css('height', ($(this).height()+20)+'px');
    });
};