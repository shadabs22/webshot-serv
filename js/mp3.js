// this object applies interactivity to the Album page
$.mp3Album = function(){
	$('#trackTable//TR').each(function(){ // walking around the track table
		$(this)
			.css({cursor:'pointer'})
			// arranging selection colors
            .mouseover(function(){$(this).toggleClass('trackHover');})
			.mouseout(function(){$(this).toggleClass('trackHover');})
			.mousedown(function(){$(this).toggleClass('trackHover').addClass('trackSelected');})
			.mouseup(function(){$(this).removeClass('trackSelected').toggleClass('trackHover');})
			
            // showing track details
            .click(function(){
				$('#trackTable//TR').each(function(){
					$(this).removeClass('trackHover');
					$(this).removeClass('trackSelected');
				});
				$(this).addClass('trackSelected');
				$('#trackDetailsDiv//div').hide();
				$('#trackDetailsDiv #track' + /track_(\w+)/.exec(this.className)[1] + 'Details').show();
				$('#trackDetailsDiv').show().parent().show();
			});
	});
};


$(function(){
    if ($.browser.msie) $('#leftMenu').css('padding', '1px 0 1px 0');

    $.fn.media.defaults.flvPlayer = '/media/mediaplayer.swf';

    $('a.media').media({ 
      width: 240, 
      height: 180,
      bgColor: 'transparent',
      flashvars: { 
          backcolor: '0x000000', 
          frontcolor: '0xffffff', 
          lightcolor: '0x0099CC' 
      } 
   });

   $('a.sales_summary,a.sales_log').mouseover(function(){
      var start = $('input[@name="start"]').get(0).value;
      var end = $('input[@name="end"]').get(0).value;

      if (this.href.indexOf("?") == -1)
      {
        this.href = this.href + '?start='+start+'&end='+end;
      }
      else
      {
        this.href = this.href.substr(0, this.href.indexOf("?")) + '?start='+start+'&end='+end;
      }
   });

   $('form/a.checkit').click(function(){

        var li =  $(this).parents('li');
        $.ajax({
            type : "POST",
            url : this.href,
            data : {
                status : "checked"
            },
            dataType : 'xml',
            success: function (xml) {
              if ('ok' == $('status', xml).text()) {
                  $(li).hide();
              } else {
                  alert('Ошибка!');
              }
            },
            error: function (xml, error, ex) {
                alert(xml);
                alert(ex);
                alert(error);
            }
        });

        return false;


   });
   
   $.mp3Album();

    //~ // dateFilterForm stat form
    $('#dateFilterForm')
    .bind('submit', function(){
        $('input[@type="submit"]', this).get(0).disabled = true;
        var start = $.cookie('start') ? $.cookie('start') : this.elements['start'].value,
                end = $.cookie('end') ? $.cookie('end') : this.elements['end'].value;
        location.href = location.pathname + '?start='+start+'&end='+end;
        return false;
    });
    // Orders statistic
    $('#ordersStatTable')
    .find('div.nodeClosed')
    .toggle(
        function(){
            $(this).removeClass("nodeClosed").addClass("nodeOpen");
            $('#ordersStatTable')
            .find('span.showWhenTracksClosed').hide().end()
            .find('span.showWhenTracksOpen').show().end()
            .find('tr.'+/(owner\w+)/.exec(this.className)[1])
            .each(function(){
                this.style.display = $.browser.msie ? 'block' : 'table-row';
            });
        },
        function(){
            $(this).removeClass("nodeOpen").addClass("nodeClosed");
            if ($('#ordersStatTable div.nodeOpen').length == 0) {
                $('#ordersStatTable span.showWhenTracksClosed').show();
                $('#ordersStatTable span.showWhenTracksOpen').hide();
            };
            $('#ordersStatTable')
            .find('tr.'+/(owner\w+)/.exec(this.className)[1]).hide();
        }
    );
    
    $('#userSearch').bind('submit', function(){
        location.href = location.href.split('?')[0] + '?q=' + this.elements['q'].value;
        return false;
    });    
    
});
