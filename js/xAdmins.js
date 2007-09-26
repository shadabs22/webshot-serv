$.fn.addEditor = function(win) {
    FCKeditor_OnComplete = function(editorInstance) {
        oEditor = FCKeditorAPI.GetInstance(editorInstance.Name); //oEditor.Focus();
        win.adjustWinToContent();
    };
    $.fck.start({
        path: '/lib/shared/fckeditor/', 
        configpath: '/std/js/fckconfig.js', 
        selector: 'textarea.x_richtextcomponent',
        toolbar: 'xcms',
        width: '610px',
        height: '300px'
    });
    return this;
};

$(document).keydown(function(e) { // the 'magic' Scroll Lock button
    if (e.keyCode == 145 && $('div.atX').length > 0){
        $('div.atX').toggle();
        $.cookie('@X_INPLACE_VISIBLE', $('div.atX').get(0).style.display == 'none' ? 0 : 1);
    }
});


$(function(){
    // normally, the menus are hidden.
    // we need to show them on page reload in case they're set to be shown
    $('body').append('<div id="showEditBlock"></div>');
});

$.fn.activateXAdmins = function(){
    if (this.length == 0) return this;
    $('div.atX').each(function(){
        if (this.prev && this.prev.parents('body').length==0) $(this).remove();
    });
	if ($.cookie('@X_INPLACE_VISIBLE') == 1) $('div.atX').show();
    return this
    .css('opacity', 0.7)
    .each(function(){ // putting each if the admin control near the object is belongs to
        var e = $(this).parent().css('position', 'relative'); // the element containing the object
        //if ($.browser.msie) e.css('height', '1%'); // the element containing the object
        $(this).css({top: '0px', left: '0px'});
    })
    .xMenuFirstStep(function(){
        if (!$(this).is('.adminContainer')) {
            $(this).jqDrag($('>a', this)).activateXAdmins2();
            if (!$.browser.msie) $(this).get(0).first();
        };
    });
};

$.fn.activateXAdmins2 = function(){
    return this
    .addClass('adminContainer')
    .xMenuSecondStep({
        onShow: function(m){
            var cont = $(m).parent();
            if ($('div.redBorder', cont).length == 0) {
                if ($.browser.msie) {
                    var h = cont.css('height');
                    cont.css('height', 'auto');
                };
                cont.append('<div class="redBorder"></div>').find('div:last').css({
                    position: 'absolute',
                    width: cont.width()+'px',
                    height: cont.height()+'px',
                    top: '0px',
                    left: $.browser.msie && m.offsetLeft > 0 ? -m.offsetLeft+'px' : '0px',
                    border: '2px dashed red'
                });
                if ($.browser.msie) cont.css('height', h);
                $(m).zIndex(999, 'div.adminContainer').css('opacity', 1);
            };
        },
        onHide: function(m){
            $(m).zIndex(1).css('opacity', 0.5).parent().find('div.redBorder').remove();
        }
    }) // this makes the admin control to be a drop-down menu
    .find('a')
    .click(function(){return false;})
    .filter('.x_delete').mousedown(function(){ // Adding behaviour to "Delete" links
      var o = $(this).attr('rel').split('_:_');
      if (confirm(rus.reallyDelete + '"'+o[0]+'" (ID '+o[2]+', '+rus.type+' '+o[1]+')?')) {
          $.post(o[3]+'admin_edit/?update=update', 'delete=delete', function(data){location.reload();});
      };
      return false;
    })
   	.addClass('adminAdded').end().filter('.x_tag').mousedown(function(){ // Enabling The Tagger
      //alert($.xslTrans('<?xml version="1.0" encoding="UTF-8"?><root/>', '/lib/classifier/body.xsl'));
      var rel = $(this).attr('rel').split(':_:'), id = rel[1], title=rel[0];
      $('body').win({
          modal: true,
          title: rus.classifier,
          height: 400,
          onReady: function(win){
              //alert($('.winSubContent', win).length);
              $('.winSubContent', win).classifier({
                      title: title,
                      id: id,
                      rubricTreeXSL: '/std/xslt/tree.xsl',
                      objectTreeXSL: '/std/xslt/tree2.xsl',
                      rubricTreeXML: '/root/root/admin_tag_tree',
                      bodyXSL: '/std/xslt/classifierBody.xsl'
                  });
            win.finishWindow();
              //$('div.tree', win).xslTransTo('/std/rubricTree.xml', '/std/xslt/tree.xsl').find('>ul:first').Treeview();
          }
      });
      return false;
     })
     .addClass('adminAdded').end().filter(':not(.adminAdded)').mousedown(function(e) { // All the rest links (not treated before) should be opened in windows
                var a = this, url = this.href;
                if (url) {
                    $('div.atX').hide();
                    var lib = {
                        saveEnabled: false,
                        onSaved: false,
                        move: function(inp){
                            $('input.ordlist', this).remove();
                            $('select option', this).each(function(){
                                if (this.selected) {
                                    if (inp.name == 'Up' && $(this).prev().get(0)) this.parentNode.insertBefore(this, $(this).prev().get(0));
                                    if (inp.name == 'Down' && $(this).next().get(0)) {
                                        if ($(this).next().next().get(0)) this.parentNode.insertBefore(this, $(this).next().next().get(0));
                                        else this.parentNode.appendChild(this);
                                    };
                                };
                            });
                            $('select option', this).each(function(){
                                $(this).parents('form').append('<input type="hidden" class="ordlist" name="ordlist[]" value="'+this.value+'">');
                            });
                        },
                        exe: function(win){
                            $(win).keydown(function(e) { 
                                if (win.get(0) && e.keyCode == 27) {win.get(0).close();}
                            });
                            $('form', win).bind('submit', function(){
                                showWait();
                                window.iframeLoaded = function(i){
                                    var response = $(i).frameContent();
                                    hideWait();
                                    if (/\[\{_ERROR/.test(response)) {
                                        win.get(0).style.visibility = 'visible';
                                        alert(rus.errorsOccured + ':\r\n'+/\[\{_ERROR(.+)_\}\]/.exec(response.replace(/[\r\n]/g, ''))[1].replace(/<[^>]+>/gi,''));
                                    } else {
                                        if (win.cfg.lib.onSaved) win.cfg.lib.onSaved();
                                        win.addEditor(win);
                                        $('input[@name="save"]', win).get(0).value = rus.saved;
                                    };
                                };
                            });
                            
                            var cont = $('.winSubContent', win);
                            
                            // user Add
                            cont.find('input[@name="addUser"]').click(function(){
                                $('body').win({
                                    title: this.value,
                                    modal: true,
                                    lib: lib,
                                    content: {url: '/root/root/admin_edit/?type_id=user&create=create'}
                                });
                            });
                            
                            // group Add
                            cont.find('input[@name="addGroup"]').click(function(){
                                $('body').win({
                                    title: this.value,
                                    modal: true,
                                    parent: win,
                                    lib: lib,
                                    content: {url: '/root/root/admin_edit/?type_id=user_group&create=create'}
                                });
                            });
                            
                            // treating pop selects
                            cont.find('input.popSelect')
                                .click(function(){
                                    var clicked = this;
                                    $('body').win({
                                        title: this.value,
                                        modal: true,
                                        parent: win,
                                        status: '<a target="_blank" href="'+$(clicked).prev().attr('rel')+'">'+rus.inNewWindow+'</a>',
                                        content: {
                                            url: $(clicked).prev().attr('rel'),
                                            onReady: function(win){
                                                var checkedFields = $(clicked).prev().prev().get(0).value.split(',');
                                                $(checkedFields).each(function(){
                                                    if ($('form input[@name="'+this+'"]', win).length > 0)
                                                    $('form input[@name="'+this+'"]', win).get(0).checked = true;
                                                });
                                                $('form[@name="titleFilter"]', win).bind('submit', function(){
                                                    this.elements['submit'].disabled = true;
                                                    win.get(0).loadWin(win.get(0).url, {query: this.elements['query'].value});
                                                    return false;
                                                });
                                                $('input[@name="Go"]', win).click(function(){
                                                    var a = [];
                                                    $('input[@type="checkbox"]', win).each(function(){
                                                        if (this.checked) a.push(this.value);
                                                    });
                                                    $(clicked).prev().prev().get(0).value = a.join(',');
                                                    //win.cfg.parent.cfg.lib.enableSave();
                                                    win.get(0).close();
                                                });
                                            }
                                        }
                                    });
                                })
                            .end()
                            .find('form').attr('action', win.cfg.content.url ? win.cfg.content.url : url).addEditor(win)
                                // sort
                                .find('input[@name="Up"]').click(function(){win.cfg.lib.move(this);}).end()
                                .find('input[@name="Down"]').click(function(){win.cfg.lib.move(this);}).end()
                            .end()
                            .find('div.atX').activateXAdmins();
                            
                            
                            // activating the buttons
                            $('form tr:last input', win).each(function(){
                                switch ($(this).attr('name')) {
                                
                                    case 'delete':
                                        $(this).click(function(){
                                            $(a)
                                            .parents('div.atX')
                                            .find('a')
                                            .filter('.x_delete')
                                            .trigger('mousedown');
                                            return false;
                                        });
                                    break;

                                    case 'cancel':
                                        $(this).click(win.get(0).close);
                                    break;
                                    
                                    case 'save':
                                        $(this).click(function(){
                                            win.cfg.lib.onSaved = false;
                                            return true;
                                        });
                                    break;

                                    case 'saveAndClose':
                                         $(this).click(function() {
                                            win.get(0).style.visibility = 'hidden';
                                            win.cfg.lib.onSaved = function(){
                                                win.get(0).close;
                                                location.reload();
                                            };
                                            return true;
                                        });
                                    break;
                                    
                                }; // switch
                            }); // buttons
                            
                        }
                    };
                    
                    $('body').win({
                        title: this.innerHTML,
                        modal: true,
                        lib: lib,
                        status: '<a target="_blank" href="'+url+'">'+rus.inNewWindow+'</a>',
                        content: {url: url},
                        onClose: function(win){
                            //if (win.removeEditor) win.removeEditor();
                            if ($.cookie('@X_INPLACE_VISIBLE') == 1) $('div.atX').show();
                        }
                    });
                };
                return false;
            })
        .end().end();
};
