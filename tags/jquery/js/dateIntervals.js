var dater = function(){};

dater.prototype.dayByM = function(M){
    var d = new Date(M);
    return d.getDate();
};

dater.prototype.MByYearAndMonth = function(year, month){
    return Date.parse(month+'/28/'+year+' 00:01');
};

dater.prototype.daysInMonth = function(year, month){
    var M = this.MByYearAndMonth(year, month), delta = 1000*3600*24;
    while (this.dayByM(M) > 1) M += delta;
    M -= delta;
    return this.dayByM(M);
};

dater.prototype.currentMonth = function(){
    var d = new Date();
    return {
        start: sprintf("%04d-%02d-%02d", d.getFullYear(), d.getMonth()+1, 1),
        end: sprintf("%04d-%02d-%02d", d.getFullYear(), d.getMonth()+1, d.getDate())
    };
};

dater.prototype.previousMonth = function(M){
    var d = M ? new Date(M) : new Date(),
            M = Date.parse((d.getMonth()+1)+'/'+15+'/'+d.getFullYear()) - 1000*3600*24*30,
            d = new Date(M);
    return {
        start: sprintf("%04d-%02d-%02d", d.getFullYear(), d.getMonth()+1, 1),
        end: sprintf("%04d-%02d-%02d", d.getFullYear(), d.getMonth()+1, this.daysInMonth(d.getFullYear(), d.getMonth()+1))
    };
};


dater.prototype.allTime = function(){
    return {
        start: '0000-00-00',
        end: '9999-99-99'
    };
};


dater.prototype.lastMonth = function(){
    var d = new Date(),
            m = d.getTime(),
            ld = new Date(m-1000*3600*24*30);
    return {
        start: sprintf("%04d-%02d-%02d", ld.getFullYear(), ld.getMonth()+1, ld.getDate()),
        end: sprintf("%04d-%02d-%02d", d.getFullYear(), d.getMonth()+1, d.getDate())
    };
};

dater.prototype.currentQuarterNumber = function(m){
    var d = new Date(),
            m = m ? m : (d.getMonth()+1);
    return Math.ceil(m/3);
};

dater.prototype.currentQuarter = function(M){
    var d = M ? new Date(M) : new Date();
    return {
        start: sprintf("%04d-%02d-%02d", d.getFullYear(), this.currentQuarterNumber(d.getMonth()+1)*3-2, 1),
        end: sprintf("%04d-%02d-%02d", d.getFullYear(), d.getMonth()+1, d.getDate())
    };
};

dater.prototype.previousQuarter = function(){
    var d = new Date(),
        currentQuarterFirstMonth = this.currentQuarterNumber()*3-2,
        previousMonth = this.previousMonth(this.MByYearAndMonth(d.getFullYear(), currentQuarterFirstMonth)).start.split('-'),
        d = new Date(this.MByYearAndMonth(previousMonth[0], previousMonth[1]));
    return {
        start: sprintf("%04d-%02d-%02d", d.getFullYear(), this.currentQuarterNumber(d.getMonth()+1)*3-2, 1),
        end: sprintf("%04d-%02d-%02d", d.getFullYear(), previousMonth[1], this.daysInMonth(d.getFullYear(), previousMonth[1]))
    };
};
dater = new dater();