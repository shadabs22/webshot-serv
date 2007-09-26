$(function(){ // code here will be executed on body load

   $('#link').click(function(){

        $.ajax({
            type : "GET",
            url : "http://stat-web01/services/syted_usr_res/",
            data : {
                user_id: "42463"
            },
            dataType : 'xml',
            success: function (xml) {
              
              alert(xml.xml);
              //$('#target').html = $(xml).text();

            },
            error: function (xml, error, ex) {
                alert(xml);
                alert(ex);
                alert(error);
            }
        });

        return false;
  });

});
