(function($) {

	// classes used by the plugin
	// need to be styled via external stylesheet, see first example
	var CLASSES = {
		open: "open",
		closed: "closed",
		expandable: "expandable",
		collapsable: "collapsable",
		lastCollapsable: "lastCollapsable",
		lastExpandable: "lastExpandable",
		last: "last",
		hitarea: "hitarea"
	};
	
	// styles for hitareas
	var hitareaCSS = {
		height: 15,
		width: 15,
		marginLeft: "-15px",
		"float": "left",
		cursor: "pointer"
	};
	
	// ie specific styles for hitareas
	if( $.browser.msie ) {
		$.extend( hitareaCSS, {
			background: "#fff",
			filter: "alpha(opacity=0)",
			display: "inline"
		});
	}

	$.extend($.fn, {
		swapClass: function(c1, c2) {
			return this.each(function() {
				var $this = $(this);
				if ( $.className.has(this, c1) )
					$this.removeClass(c1).addClass(c2);
				else if ( $.className.has(this, c2) )
					$this.removeClass(c2).addClass(c1);
			});
		},
		replaceclass: function(c1, c2) {
			return this.each(function() {
				var $this = $(this);
				if ( $.className.has(this, c1) )
					$this.removeClass(c1).addClass(c2);
			});
		},
		Treeview: function(settings) {
			// currently no defaults necessary, all implicit
			settings = $.extend({}, settings);
			// handle node-toggle event
			function toggler() {
				$(this).parent()
					.swapClass( CLASSES.collapsable, CLASSES.expandable )
					.swapClass( CLASSES.lastCollapsable, CLASSES.lastExpandable )
					.find( ">ul" )
					.toggle();
			}
            
            
			this.addClass("treeview");
            
            // preventing select
            //~ $('li', this)
                //~ .bind('selectstart', function(){return false}) // in IE
                //~ .mousedown(function(e){
                    //~ e.preventDefault(); // in non-IE
                //~ });
                
            $('li[>ul:not([li])]', this).removeClass([CLASSES.expandable, CLASSES.lastExpandable, CLASSES.collapsable, CLASSES.lastCollapsable].join(' '));
            $('li>ul:not([li])', this).remove();
            $('li:not([ul]):not(:last-child)', this).removeClass(CLASSES.last);
            $("li:last-child", this).addClass(CLASSES.last);
			$( (settings.collapsed ? "li" : "li." + CLASSES.closed) + ":not(." + CLASSES.open + ") > ul", this).hide();
            
			$('li[>ul]', this)
				.filter("[>ul:hidden]")
                    .filter(':last-child')
                        .swapClass(CLASSES.last, CLASSES.lastExpandable)
                    .end()
                    .not(':last-child')
                        .removeClass(CLASSES.lastExpandable).addClass(CLASSES.expandable)
                    .end()
                .end()
				.not("[>ul:hidden]")
                    .filter(':last-child')
                        .swapClass(CLASSES.last, CLASSES.lastCollapsable)
                    .end()
                    .not(':last-child')
                        .removeClass(CLASSES.lastCollapsable).addClass(CLASSES.collapsable)
                    .end()
                .end()
				.not('[>div.'+CLASSES.hitarea+']')
                .prepend("<div class=\"" + CLASSES.hitarea + "\">")
				.find(">div." + CLASSES.hitarea)
				.css(hitareaCSS)
				.toggle( toggler, toggler );
                
                
            //~ $('li span.edit', this)
            //~ .add('li span.add', this)
            
            $('li span.edit', this)
            .click(function(e){
                var span = this, titleSpan = $('span.title:first', this.parentNode), w = $(this).nearestWindow(), id = titleSpan.attr('id');
                w.append('<input type="text" class="inplaceEdit"/>').find('input:last')
                    .css({
                        position: 'absolute',
                        width: titleSpan.width() + 10 + 'px',
                        height: titleSpan.height() + 'px',
                        top: $(this).top() + 'px',
                        left: $(this).left() + 'px',
                        zIndex: w.css('zIndex') + 1
                    })
                    .bind('keydown', function(e){
                        if (e.keyCode == 27) $(this).remove(); // escape
                        if (e.keyCode == 13) {
                            var i = this;
                            $.post("/root/" + id + "/admin_req/?action=updateobj", "title="+i.value, function(){
                                titleSpan.html(i.value);
                                $(i).remove();
                            });
                        };
                    })
                    .each(function(){
                        $.xmlFilesHash = [];
                        this.value = titleSpan.html();
                        this.focus();
                    });
            });
            
 
            $('li span.add', this)
            .click(function(e){
                var span = this, titleSpan = $('span.title:first', this.parentNode), w = $(this).nearestWindow(), id = titleSpan.attr('id');
                w.append('<input type="text" class="inplaceEdit"/>').find('input:last')
                    .css({
                        position: 'absolute',
                        width: '100px',
                        height: '20px',
                        top: $(this).top() + 'px',
                        left: $(this).left() + 'px',
                        zIndex: w.css('zIndex') + 1
                    })
                    .bind('keydown', function(e){
                        if (e.keyCode == 27) $(this).remove(); // escape
                        if (e.keyCode == 13) {
                            var i = this;
                            $.post("/root/" + (id?id:'root') + "/admin_req/?action=createobj&type_id=tag", (id?"links[tree]="+id+"&":"")+"title="+i.value, function(){
                                $.xmlFilesHash = [];
                                var t = titleSpan.parents('.tree').get(0);
                                $(i).remove();
                                $(t).empty().createTree(t.cfg);
                            });
                        };
                    })
                    .get(0).focus();
            });


            $('li span.delete', this).click(function(e){
                var titleSpan = $('span.title:first', this.parentNode), id = $(titleSpan).attr('id'), title = titleSpan.html();
                if (window.confirm(rus.reallyDelete + title+' (ID: '+id+')?'))
                $.get('/root/' + id + '/admin_req/?action=deleteobj', function(){
                    titleSpan.parent().remove();
                })
            });

               
            if (settings.editable) {
                $('li', this)
                    .Draggable(
                        {
                            revert		: true,
                            autoSize		: true,
                            ghosting		: true
                        }
                    );
                $('li span', this).Droppable(
                    {
                        accept  : 'node',
                        onhover : function(dragged) {
                            $(this).toggleClass('hover');
                        },
                        onout   : function() {
                            $(this).toggleClass('hover');
                        },
                        ondrop  : function(dropped){
                            if ($('>ul', this.parentNode).length == 0) $(this.parentNode).append('<ul></ul>');
                            var u = dropped.parentNode;
                            $('>ul:first', this.parentNode).append(dropped);
                            if ($('>li', u.parentNode).length == 0) $(u.parentNode).find('>div').remove().find('>ul').remove();
                            $(this).toggleClass('hover');
                            $('.tree > ul:first').Treeview();
                        }
                    }
                );
            };
               
			return this;
		}
	});
})($);


$.fn.createTree = function(cfg){
    cfg = cfg || {};
    cfg.xsl = cfg.xsl || '/std/xslt/tree.xsl';
    var tree = this.get(0);
    tree.cfg = cfg;
    //~ tree.renew = function(){
        //~ $(tree).createTree(tree.cfg);
    //~ };
    return this.xslTransTo(cfg.xml, cfg.xsl)
    .find('>ul:first').Treeview().end().get(0).cfg.onReady(tree);
    
    //~ each(function(){
        //~ //alert(this.cfg+'__');
        //~ this.cfg.onReady();
    //~ });
};