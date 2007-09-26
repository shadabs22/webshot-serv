$.fn.classifier = function(cfg){

    if (!cfg) cfg = {};
    if (typeof(cfg) == 'string' ) cfg = {id: cfg};

    cfg.title = cfg.title || 'Object title';
    cfg.updateUrl = cfg.updateUrl || '/root/' + cfg.id + '/admin_tag/';
    cfg.rubricTreeXSL = cfg.rubricTreeXSL || '/std/xslt/tree.xsl';
    cfg.objectTreeXSL = cfg.objectTreeXSL || '/std/xslt/tree2.xsl';
    cfg.rubricTreeXML = cfg.rubricTreeXML || '/root/root/admin_tag_tree';
    cfg.bodyXSL = cfg.bodyXSL || '/std/xslt/classifierBody.xsl';
    
    
    var ids = [], z = $.topWin() ? $.topWin().css('zIndex') : 1, _this = this;

    function reBuild(){
        $('.monitor li.node span', _this).each(function(){
            ids.push(this.id.substr(1));
        });
        
        var a = [];
        $(ids).each(function(){
            a.push("@id='"+this+"'");
        });
        var xslPart = a.length > 0 ? '<xsl:apply-templates select="@*|node[descendant-or-self::node['+a.join(' or ')+']]" />' : '',
                xsl = '<?xml version="1.0" encoding="UTF-8"?><xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"><xsl:output encoding="UTF-8" indent="yes" method="xml" /><xsl:template match="@*|node"><xsl:copy>'+xslPart+'</xsl:copy></xsl:template></xsl:stylesheet>',
                d = new transformerObject(), xml = d.transformTo(cfg.rubricTreeXML, xsl, true);
        $('.monitor', _this)
            .xslTransTo(xml, cfg.objectTreeXSL)
            .find('>ul:first').Treeview()
            .find('img.del').click(function(){
                $(this).parent().empty();
                ids = [];
                reBuild();
            });
        $('input.show', _this).each(function(){
            this.value=rus.save;
            this.disabled = false;
        });
    };

    cfg.title = cfg.title.replace(/"/g, '&quot;');

    return this
    .xslTransTo('<?xml version="1.0" encoding="UTF-8"?><root id="'+cfg.id+'" title="'+cfg.title+'" />', cfg.bodyXSL)
    .find('div.tree').createTree({
            xml: cfg.rubricTreeXML,
            onReady: function(tree){
                $(tree).find('li span.title')
                    .dblclick(function(){
                        ids = [this.id];
                        reBuild();
                    })
                    .Draggable({
                            revert		: true,
                            autoSize		: true,
                            ghosting		: true,
                            zIndex       : z
                        }
                    );
                return $(tree);
            }
        })
    .nearestWindow()
    .find('div.classifierObject', _this)
        .Droppable( {
            accept  : 'title',
            ondrop  : function(dropped){
                ids = [dropped.id];
                reBuild();
            }
        }
    ).end()
    .find('input.show', _this).click(function(){
        var s = [], but = this;
        but.disabled = true;
        $('.monitor li:not([ul])', _this).each(function(){
            s.push($('span', this).get(0).id.substr(1));
        });
        $.post(cfg.updateUrl, 'tag_ids='+s.join(','), function(data){
            but.value=rus.saved;
        });
    }).end()
    .find('.monitor').each(function(){
        $.get(cfg.updateUrl + '?xml', function(xml){
            var tags = xml.documentElement.getElementsByTagName('object_tags')[0].getElementsByTagName('object');
            $(tags).each(function(){
                ids.push($(this).attr('obj_id'));
            });
            reBuild();
        });
    });
};
