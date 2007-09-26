(function($){
$.fn.jqDrag=function(r){$.jqDnR.init(this,r,'d'); return this;};
$.fn.jqResize=function(r){$.jqDnR.init(this,r,'r'); return this;};
$.jqDnR={
init:function(w,r,t){
  r=(r)?$(r,w):w;
  if (r.is('a')) r.click(function(){return false;});
	r
  .bind('selectstart', function(){return false;})
  .bind('mousedown',{w:w,t:t},function(e){
  e.preventDefault();
  var h=e.data; var w=h.w;
	//hash=$.extend({oX:w.left(),oY:w.top(),oW:(w.width()+'px'),oH:(w.height()+'px'),pX:e.pageX,pY:e.pageY,o:w.css('opacity')},h);
	hash=$.extend({oX:w.offsetLeft(),oY:w.get(0).offsetTop,oW:(w.width()+'px'),oH:(w.height()+'px'),pX:e.pageX,pY:e.pageY,o:w.css('opacity')},h);
	//h.w.css('opacity',0.8);
	$().mousemove($.jqDnR.drag).mouseup($.jqDnR.stop);
	return false;});
},
drag:function(e) {var h=hash; var w=h.w[0];
	if(h.t == 'd') h.w.css({left:h.oX + e.pageX - h.pX,top:h.oY + e.pageY - h.pY});
	else h.w.css({width:Math.max(e.pageX - h.pX + h.oW,0),height:Math.max(e.pageY - h.pY + h.oH,0)});
	return false;},
stop:function(){var j=$.jqDnR;
//hash.w.css('opacity',hash.o);
$().unbind('mousemove',j.drag).unbind('mouseup',j.stop);},
h:false};
var hash=$.jqDnR.h;
})(jQuery);
