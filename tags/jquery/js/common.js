if ($.browser.msie) $.browser.msieVersion = parseInt(/MSIE (\d)/.exec(navigator.userAgent)[1], 10);

$.fn.standartActivate = function(){
    this.pager();
    this.find('div.xMenu').activateXAdmins();
    $('dl.accordion', this).Accordion({
        headerSelector: 'dt',
        panelSelector: 'dd',
        panelHeight: 200,
        speed: 300
    });
    this.find('input.dateInput').cal();
    $('img[@height > 50]', this).each(function(){
        if ($(this).attr('height') > 50 && !$(this).parent().is('a')) {
            $(this)
            .css({cursor: 'pointer'})
            .click(function(){
                var img_url;
                if(this.src.indexOf(';') == -1) img_url = this.src;
                else {
                    var arr = this.src.split(';');
                    img_url = arr[1];
                };
                img_url = '/std/pic.php?'+img_url + '&maxX=' + Math.round($.bodyWidth()*0.8) + '&maxY=' + Math.round($.bodyHeight()*0.8);
                if ($(this).next().is('form')) {
                    var winName = 'pic' + String(Math.random()).substr(2), form = $(this).next().get(0);
                    window.open('', winName ,'left='+Math.round((window.screen.width - 300)/2)+',top='+Math.round((window.screen.height - 300)/2)+',width=300,height=300,resizable=1,status=0,titlebar=0,scrollbars=0');
                    form.action = img_url;
                    form.target = winName;
                    form.submit();
                } else {
                    var w = window.open(img_url, String(Math.random()).substr(2) ,'left='+Math.round((window.screen.width - 300)/2)+',top='+Math.round((window.screen.height - 300)/2)+',width=300,height=300,resizable=1,status=0,titlebar=0,scrollbars=0');
                };
            });
        };
    });
    
    return this;
};

$(function(){ // code here will be executed on body load

    mouseIsDown = false;
    $('body')
    .mousedown(function(){mouseIsDown = true;})
    .mouseup(function(){mouseIsDown = false;});

    $('body').standartActivate();

    /* Login error */
    if ($('#loginError').length > 0) alert(rus.wrongLogin);

    /* Login window behaviour */
    $('#loginLink').click(function() { // when we click on "Login"..
        $(this).hide(); // hiding ligin link
         
        // Hiding banner. We have to do this coz we can't get it down the login window when it's flash ()
        // (In order to get it down we need to alter its code
        //  but is it out of our control )
        // $('#banner *').hide('normal');
        
        $('body').win({
            title: rus.authForm,
            top: (!$.getScrolling().scrollY) ? 7 : $.getScrolling().scrollY + 7, 
            left: $.bodyWidth() -  ($.browser.mozilla ? 315 : 295), 
            modal: true,
            width: 289, 
            height: 225,
            resizeable: 'no',
            content: $.xslTrans('<?xml version="1.0" encoding="UTF-8"?><root/>', '/std/xslt/loginForm.xsl'),
            onReady: function(win){
                
                win.adjustWin();
                // Making Submit button enabled when Login and Password fields aren't empty only
                //~ function manageButtonsAbility() {
                    //~ var empty = [];
                    //~ $('input.text', win).each(function(){
                        //~ empty.push(this.value == '');
                    //~ });
                    //~ $('#loginButtons input', win).get(0).disabled = empty[0] || empty[1];
                //~ };
                //~ manageButtonsAbility();
                //~ $('input.text', win).bind('keyup', manageButtonsAbility);
                
                // Disabling Submit button on submit
                $('#loginForm', win).bind('submit', function(){
                    $.cookie('@X_INPLACE_VISIBLE', 1);
                    $('input.text', win).unbind('keyup');
                    $('#loginButtons input', win).each(function(){
                        this.disabled = true;
                    });
                    //alert('dddddddddd');
                });
                
                // placing focus into Login field
                $('#loginForm input.text', win).get(0).focus();
                
                // Adding to the Close button the same behaviour as one in the top right corner
                $('#loginForm input.loginClose', win).click(function(){
                    $('input.winClose', win).trigger('click');
                });
                
            },
            onClose: function(){
                //$('#banner *').show('normal');
                $('#loginLink').show();
            }
        });
       
    });
    
    // when we do an AJAX request we need a "waiting" window to be shown
	$(document).ajaxStart(function(){showWait();}).ajaxStop(function(){hideWait();});
    
    // activating calendar presets
    $("select[@name='datePresets']").bind('change', function(){
        eval('var preset = dater.'+this.value+'();');
        $(this).parents('table').find('input.dateInput').each(function(){
            eval('this.value = preset.'+this.name+';');
            this.changed();
        });
    
    });
    
});

function showWait() {
    $('body').win({
        top: (!$.getScrolling().scrollY) ? 7 : $.getScrolling().scrollY + 7, 
        left: $.bodyWidth() - ($.browser.mozilla ? 176 : 157), 
        width: 150, 
        height: 40,
        id: 'wait',
        resizeable: 'no',
        content: '<div style="width: 120px;font: 10pt Tahoma;padding-top: 5px;"><img src="/std/css/public/icon_animated_busy.gif" align="left" alt="" width="16" height="16"><b style="padding-left: 10px;">@X</b> - '+rus.loading+'.</div>'
    });
};

function hideWait() {
    $("#wait").remove();
};

var getInputValue = function(inp){ // gathers string value from any form field
  if(typeof(inp.nodeName)=="undefined"){
    for(var i=0; i<inp.length; i++)
      if(inp[i].checked) return (inp[i].name? encodeURIComponent(inp[i]).name+"=":"") + encodeURIComponent(inp[i].value);
    return "";
  };
  if(inp.type=="select-multiple"){
    var ret=[];
    for(var i=0; i<inp.options.length; i++)
      if(inp.options[i].selected) ret[ret.length]=(inp.options[i].name? encodeURIComponent(inp.options[i].name)+"[]=":"")+encodeURIComponent(inp.options[i].value);
    return ret.join("&");
  } else if (inp.type=="select-one")
     return (inp.selectedIndex>=0)?
       ((inp.options[inp.selectedIndex].name? encodeURIComponent(inp.options[inp.selectedIndex].name)+"=":inp.name+"=")+encodeURIComponent(inp.options[inp.selectedIndex].value)) : "";
  if(inp.type=="image") return (inp.name? encodeURIComponent(inp.name)+"=":"")+encodeURIComponent(inp.src);
  else return (inp.name? encodeURIComponent(inp.name)+"=":"")+encodeURIComponent(inp.value);
};

var encodeForm = function(form) { // gathers all form's data as a string
  if (!form.tagName) return form;
  var ret=[], el;
  for(var i=0; i<form.elements.length; i++){
    el=form.elements[i];
    if("checkboxradio".indexOf(el.type)>=0) {if(el.checked) ret[ret.length]=getInputValue(el);}
    else if(el.type!="button"&&el.type!="submit") ret[ret.length]=getInputValue(el);
  };
  return ret.join("&");
};


function addEvent(el, evName, func) {
  if (el.addEventListener) el.addEventListener(evName, func, false); else if (el.attachEvent) el.attachEvent('on' + evName, func);
};

function rmEvent(el, evName, func) {
  if (el.removeEventListener) el.removeEventListener(evName, func, false); else if (el.detachEvent) el.detachEvent('on' + evName, func);
};
