function mp3DateSelect(input)
{
    this.input = input;

    var d = this.input.value.substring(8,10);
    var m = this.input.value.substring(5,7);
    var y = this.input.value.substring(0,4);

    this.d = this.createSelect(1, 31, d);
    this.m = this.createSelect(1, 12, m);
    this.y = this.createSelect(new Date().getFullYear()-10, 1940, y);

    var self = this;
    $([this.d, this.m, this.y]).change(function () {self.onChangeHandler()});

    $(this.input).after($(this.d));
    $(this.d).after($(this.m));
    $(this.m).after($(this.y));
}

mp3DateSelect.prototype = {
    createSelect: function (from, to, value)
    {
        var select = document.createElement('select');
        select.options[0] = new Option('','');
        var f = function (i) {
            var val = (i < 10) ? '0'+i : i;
            var o = new Option(val, val);
            select.options[select.options.length++] = o;
            if (value == i) {
                o.selected = true;
            }
        }
        if (from <= to) {
            for (var i=from; i <= to ; i++) {
                f(i);
            }
        } else {
            for (var i=from; i >= to ; i--) {
                f(i);
            }
        }
        return select;
    },
    onChangeHandler: function ()
    {
        this.input.value = this.y.value + '-' + this.m.value + '-' + this.d.value;
    }
}

$(function () {
    var input = $('#registration-form-birth//input[@type=\'hidden\']');
    new mp3DateSelect(input.get(0));
});
