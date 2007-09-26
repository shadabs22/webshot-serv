function mp3_update_subscribe_select_all(cb_list)
{
    var all_checked = true;
    cb_list.each(function (idx, elm) {
        if (!elm.checked) {
            all_checked = false;
        }
    });
    if (all_checked) {
        $('#subscribe-select-all').attr('checked', 'checked');
    } else {
        $('#subscribe-select-all').removeAttr('checked');
    }
}
$(function () {
    var cb_list = $('#subscribe-select-all/../..//input[@id!="subscribe-select-all"]');
    cb_list.click(function () {
            mp3_update_subscribe_select_all(cb_list);
    });
    mp3_update_subscribe_select_all(cb_list);
    $('#subscribe-select-all').click(function () {
        if (this.checked) {
            cb_list.attr('checked', 'checked');
        } else {
            cb_list.removeAttr('checked');
        }
    });
});