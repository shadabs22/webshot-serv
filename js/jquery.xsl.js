var ie = window.ActiveXObject,
    transformerObject = function() {};
    
$.xmlFilesHash = [];

transformerObject.prototype = {
  xslLib: [],
  getIEObject: function(){
    var o = new ActiveXObject("Microsoft.XMLDOM");
    o.async = false;
    return o;
  },
  fromString: function(str) {
    if (!/^<\?xml/.test(str)) return false;
    if (ie) {
        var o = this.getIEObject();
        o.loadXML(str);
        return o;
    } else {
        var parser = new DOMParser(), o = null;
        return parser.parseFromString(str, 'text/xml');
    };
  },
  fromObject: function(el) {
    if (!el.documentElement) return false;
    return el;
  },
  fromFile: function(file) {
    if (!$.xmlFilesHash[file]) {
        var o = ie ? this.getIEObject() : document.implementation.createDocument("", "test", null);
        try {
          o.async = false;
          o.load(file);
        } catch(e) {
            var t = this;
            $.ajax({
                async: false,
                url: file,
                success: function(xml){o = xml;}
            });
        };
        $.xmlFilesHash[file] = o;
    };
    return $.xmlFilesHash[file];
  },
  load: function(source) {
    return this.fromString(source) || this.fromObject(source) || this.fromFile(source);
  },
  transformTo: function(xmlSrc, xslSrc, doc){
    if (!this.xslLib[xslSrc]) {
        this.xslLib[xslSrc] = this.load(xslSrc);
    };
    this.xml = this.load(xmlSrc);
    if (ie) {
      return this.xml.transformNode(this.xslLib[xslSrc]);
    } else {
      this.processor = new XSLTProcessor();
      this.processor.importStylesheet(this.xslLib[xslSrc]);
      return doc ? this.processor.transformToDocument(this.xml) : this.processor.transformToFragment(this.xml, document);
    };
  }
};
$.xslTrans = function(xml, xsl){
    return (new transformerObject()).transformTo(xml, xsl);
};
$.fn.xslTransTo = function(xml, xsl){
    var result = $.xslTrans(xml, xsl);
	return this.each( function(){
        $(this).empty().append(result);
	});
};
