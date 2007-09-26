//##############################
// jQuery FCKEditor Plugin
// By Diego A. - diego@fyneworks.com
// Jun-29
/*
 USAGE:
		$.fck.start({ path:'/path/to/fck/editor/' }); // initialize FCK editor
		$.fck.update(); // update value in textareas of each FCK editor instance
*/

/*# AVOID COLLISIONS #*/
if(jQuery) (function($){
/*# AVOID COLLISIONS #*/

$.extend($, {
	fck:{
		config: { Config: {} }, // default configuration
		path: '/fckeditor/', // default path to FCKEditor directory
  list: [], // holds a list of instances
  loaded: false, // flag indicating whether FCK script is loaded
		intercepted: null, // variable to store intercepted method(s)
		
		// utility method to read contents of FCK editor
		content: function(i){
			try{
				var x = FCKeditorAPI.GetInstance(i);
				return x.GetXHTML(true);
			}catch(e){ return ''; };
		}, // fck.content function
		
		// utility method to update textarea contents before ajax submission
		update: function(){
			// Update contents of all instances
			var e = $.fck.list;
			for(var i=0;i<e.length;i++){
				var ta = $('#'+e[i].InstanceName);
				var ht = $.fck.content(e[i].InstanceName);
				ta.val(ht).filter('textarea').html(ht);
			}
		}, // fck.update
		
		// utility method to create instances of FCK editor (if any)
		create: function(o/* options */){
			o = (o || $.fck.config);
			// Plugin options
			$.extend(o,{
				selector: (o.selector || 'textarea.fck'),
			 BasePath: (o.path || o.BasePath || $.fck.path)
			});
			// Find fck.editor-instance 'wannabes'
			var e = $(o.selector); if(!(e.size()>0)) return;
			// Load script and create instances
            
			if($.fck.loaded){
				$.fck.editor(e,o);
			}
			else{
				$.getScript(
					o.BasePath+'fckeditor.js',
					function(){
						$.fck.loaded = true;
						$.fck.editor(e,o);
					}
				);
			};
			// Return matched elements...
			return e;
		},
		
		// utility method to integrate this plugin with others...
		intercept: function(){
			// If we have FCK editor instances, make the all
			// important integration with the jQuery Form plugin
   if($.fn.ajaxSubmit){
				$.fck.intercepted = $.fn.ajaxSubmit;
				$.fn.ajaxSubmit = function() {
     $.fck.update(); // update html
					return $.fck.intercepted.apply( this, arguments );
				};
			};
		},
		
		// utility method to create an instance of FCK editor
		editor: function(e /* elements */, o /* options */){
			o = (o || $.fck.config);
			// Default configuration
			$.extend(o,{
                 Width: (o.width || o.Width || '100%'),
                 Height: (o.height || o.Height|| '500px'),
                 BasePath: (o.path || o.BasePath || $.fck.path),
                 ToolbarSet: (o.toolbar || o.ToolbarSet || 'Default'),
                 Config: (o.config || o.Config || {})
			});
			$.extend(o.Config,{CustomConfigurationsPath: o.configpath});
			// Make sure we have a jQuery object
			e = $(e);
			if(e.size()>0){
				// Local array to store instances
				var a = ($.fck.list || []);
				// Go through objects and initialize fck.editor
          e.each(
					function(i,t){
						var T = $(t);// t = element, T = jQuery
						t.id = (t.id || 'fck'+location.pathname.replace(/\//g, '')+i);

              //T.before('<div style="position: relative;" id="' + t.id + '_tabs"><ul><li><a href="#WYSIWYG_' + t.id + '"><span>wysiwyg</span></a></li><li><a href="#HTML_'+t.id+'"><span>html</span></a></li></ul><div class="empty" id="WYSIWYG_'+t.id+'"></div><div class="empty" id="HTML_'+t.id+'"></div>');
              //T.after('</div>');
              
              /*
              $('#'+t.id+'_tabs').tabs($.cookie('Editor_'+t.id) == 1 ? 2 : 1, {
                  onClick: function(tab) {
                    if (t.fck) {
                        var e = FCKeditorAPI.GetInstance(t.id);
                        e.SwitchEditMode();
                        $.cookie('Editor_'+t.id, e.EditMode);
                    } else {
                        $.fck.set(a, t, o);
                        $.cookie('Editor_'+t.id, '0');
                    };
                  }
              });
              */

              T.before('<input type="button" value="'+( $.cookie('Editor_'+t.id) == 1 ? rus.turnEditorOn : rus.turnEditorOff )+'" id="'+t.id+'_switch" /><br />');
              $('#'+t.id+'_switch').click(function(){
                  if (t.fck) {
                      var e = FCKeditorAPI.GetInstance(t.id);
                      
                      e.SwitchEditMode();

                      if (e.EditMode == FCK_EDITMODE_WYSIWYG)
                      {
                        e.ToolbarSet.Expand();
                      } else
                      {
                        e.ToolbarSet.Collapse();
                      }

                      $.cookie('Editor_'+t.id, e.EditMode);
                  } else {
                      $.fck.set(a, t, o);
                      $.cookie('Editor_'+t.id, '0');
                  };
                  this.value = $.cookie('Editor_'+t.id) == 1 ? rus.turnEditorOn : rus.turnEditorOff;
              });

              if(t.id/* has id */ && !t.fck/* not already installed */ && $.cookie('Editor_'+t.id) != 1) $.fck.set(a, t, o);
					}
				);
				// Store instances in global array
				$.fck.list = a;
			};
			// return jQuery array of elements
		 return e;
		}, // fck.editor function
		
        set: function(a, t, o){
            var n = a.length;
            a[n] = new FCKeditor(t.id);
            $.extend(a[n], o);
            // XCMS file upload param:
            a[n].Config["dir"] = $('#'+t.id+'__dir').val();
            a[n].ReplaceTextarea();
            t.fck = a[n];
        },
        
		// start-up method
		start: function(o/* options */){
			// Attach itself to known plugins...
			$.fck.intercept();
			// Create FCK editors
			return $.fck.create(o);
		} // fck.start
		
 } // fck object
	//##############################
	
});
// extend $
//##############################

/*# AVOID COLLISIONS #*/
})(jQuery);
/*# AVOID COLLISIONS #*/
