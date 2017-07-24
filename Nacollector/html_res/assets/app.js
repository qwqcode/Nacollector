/**
 * Created by Zneia on 2017/7/15.
 */

/**
 * 页面初始化
 */
$(document).ready(function () {
    // 初始化 NavBar
    NavBar.init();
    // 初始化 Tooltip
    $('[data-toggle="tooltip"]').tooltip();
    // 浏览器初始化时白色闪光 减少违和感
    setTimeout(function () {
        $('body,html').css("opacity","1");
    }, 10);
});

const WRAP_CONT = '.wrap';
const NAVBAR_CONT = '.top-nav-bar';

// 操作列表
// Key 为 C# 调用类名
var ActionList = {
    CollItemDescImg: {
        label: "商品详情页图片解析",
        loadForm: function () {
            AppUi.formControls.textInput('PageUrl', '详情页链接', '', inputValidators.isUrl);
            AppUi.formControls.selectInput('PageType', '链接类型', {
                "Tmall": "天猫",
                "Taobao": "淘宝",
                "Alibaba": "阿里巴巴",
                "Suning": "苏宁易购",
                "Gome": "国美在线"
            });
            AppUi.formControls.selectInput('ImgType', '图片类型', {
                "Thumb": "主图",
                "Category": "分类图",
                "Desc": "详情图"
            });
            AppUi.formControls.selectInput('CollType', '采集模式', {
                "collImgSrcUrl": "显示图片链接",
                "collDownloadImgSrc": "显示图片链接 并 下载打包保存",
            });
        }
    },
    TaobaoSellerColl: {
        label: "淘宝店铺搜索卖家ID名采集",
        loadForm: function () {
            AppUi.formControls.textInput('PageUrl', '店铺搜索页链接', '', inputValidators.isUrl);
            AppUi.formControls.numberInput('CollBeginPage', '采集开始页码', 1, 1);
            AppUi.formControls.numberInput('CollEndPage', '采集结束页码', undefined, 1);
        }
    },
    TmallGxptInvite: {
        label: "天猫供销平台分销商一键邀请",
        loadForm: function () {
            AppUi.formControls.textareaInput('PageUrl', '分销商ID名（一行一个）', undefined, 250);
        }
    },
    TmallGxptInviteDelete: {
        label: "天猫供销平台分销商一键撤回",
        loadForm: function () {
            AppUi.formControls.numberInput('DeleteBeginPage', '撤回开始页码', 1, 1);
            AppUi.formControls.numberInput('DeleteEndPage', '撤回结束页码', undefined, 1);
        }
    }
};

var NavBar = {
    $navTitle: NAVBAR_CONT+' .nav-title',
    $navBtns: NAVBAR_CONT+' .nav-btns',
    // 初始化 Navbar
    init: function () {
        $('<div class="left-items"><div class="nav-title"></div></div><div class="right-items"><div class="nav-btns"></div></div>').appendTo(NAVBAR_CONT);
    },
    // NavBar 标题设置
    titleSet: function (value, base64) {
        if (typeof base64 === "boolean" && base64 === true)
            value = Base64.decode(value);

        $(this.$navTitle).text(value);
    },
    // 添加按钮
    btnAdd: function (name, icon, title, onClickEvent) {
        var dom = $('<a data-nav-btn="'+name+'" data-placement="bottom" title="'+title+'"><i class="zmdi zmdi-'+icon+'"></i></a>');
            if (onClickEvent !== undefined)
                dom.click(onClickEvent);
            dom.appendTo(this.$navBtns).tooltip();
    },
    // 按钮批量添加
    btnsAdd: function (navBarBtnList) {
        $.each(navBarBtnList, function (name, value) {
            NavBar.btnAdd(name, value['icon'], value['title'], value['onClick']);
        });
    },
    // 获取按钮 Dom
    btnGet: function (name) {
        return $('[data-nav-btn="'+name+'"]');
    },
    // 获取按钮 Selector
    btnGetSelector: function (name) {
        return '[data-nav-btn="'+name+'"]';
    }
};

// 根据URL创建一个下载任务
window.downloadFile = function (srcUrl) {
    var $a = $("<a></a>").attr("href", srcUrl).attr("download", "");
    $a[0].click();
};

/**
 * jQuery 扩展函数
 */
$.extend({
    getPosition: function ($element) {
        var el = $element[0];
        var isBody = el.tagName === 'BODY';

        var elRect = el.getBoundingClientRect();
        if (elRect.width === null) {
            // width and height are missing in IE8, so compute them manually; see https://github.com/twbs/bootstrap/issues/14093
            elRect = $.extend({}, elRect, {width: elRect.right - elRect.left, height: elRect.bottom - elRect.top})
        }
        var isSvg = window.SVGElement && el instanceof window.SVGElement;
        // Avoid using $.offset() on SVGs since it gives incorrect results in jQuery 3.
        // See https://github.com/twbs/bootstrap/issues/20280
        var elOffset = isBody ? {top: 0, left: 0} : (isSvg ? null : $element.offset());
        var scroll = {scroll: isBody ? document.documentElement.scrollTop || document.body.scrollTop : $element.scrollTop()};
        var outerDims = isBody ? {width: $(window).width(), height: $(window).height()} : null;

        return $.extend({}, elRect, scroll, outerDims, elOffset);
    },
    sprintf: function (str) {
        var args = arguments,
            flag = true,
            i = 1;

        str = str.replace(/%s/g, function () {
            var arg = args[i++];

            if (typeof arg === 'undefined') {
                flag = false;
                return '';
            }
            return arg;
        });
        return flag ? str : '';
    }
});

/**
 * 表单验证器
 */
window.inputValidators = {
    // 匹配Email地址
    isEmail: function (str) {
        if (str === null || str === "") return false;
        var result = str.match(/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/);
        return !!result;
    },
    // 匹配qq
    isQq: function (str) {
        if (str === null || str === "") return false;
        var result = str.match(/^[1-9]\d{4,12}$/);
        return !!result;
    },
    // 匹配 english
    isEnglish: function (str) {
        if (str === null || str === "") return false;
        var result = str.match(/^[A-Za-z]+$/);
        return !!result;
    },
    // 匹配integer
    isInteger: function (str) {
        if (str === null || str === "") return false;
        var result = str.match(/^[-\+]?\d+$/);
        return !!result;
    },
    // 匹配double或float
    isDouble: function (str) {
        if (str === null || str === "") return false;
        var result = str.match(/^[-\+]?\d+(\.\d+)?$/);
        return !!result;
    },
    // 匹配URL
    isUrl: function (str) {
        if (str === null || str === "") return false;
        var result = str.match(/^(http|https):\/\/[A-Za-z0-9]+\.[A-Za-z0-9]+[\/=\?%\-&_~`@[\]\’:+!]*([^<>\"])*$/);
        return !!result;
    }
};

/*
 * $Id: base64.js,v 2.15 2014/04/05 12:58:57 dankogai Exp dankogai $
 *
 *  Licensed under the BSD 3-Clause License.
 *    http://opensource.org/licenses/BSD-3-Clause
 *
 *  References:
 *    http://en.wikipedia.org/wiki/Base64
 */
(function(r){var j=r.Base64;var e="2.1.9";var s;if(typeof module!=="undefined"&&module.exports){try{s=require("buffer").Buffer}catch(g){}}var p="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";var c=function(C){var B={};for(var A=0,z=C.length;A<z;A++){B[C.charAt(A)]=A}return B}(p);var v=String.fromCharCode;var x=function(A){if(A.length<2){var z=A.charCodeAt(0);return z<128?A:z<2048?(v(192|(z>>>6))+v(128|(z&63))):(v(224|((z>>>12)&15))+v(128|((z>>>6)&63))+v(128|(z&63)))}else{var z=65536+(A.charCodeAt(0)-55296)*1024+(A.charCodeAt(1)-56320);return(v(240|((z>>>18)&7))+v(128|((z>>>12)&63))+v(128|((z>>>6)&63))+v(128|(z&63)))}};var k=/[\uD800-\uDBFF][\uDC00-\uDFFFF]|[^\x00-\x7F]/g;var h=function(z){return z.replace(k,x)};var q=function(C){var B=[0,2,1][C.length%3],z=C.charCodeAt(0)<<16|((C.length>1?C.charCodeAt(1):0)<<8)|((C.length>2?C.charCodeAt(2):0)),A=[p.charAt(z>>>18),p.charAt((z>>>12)&63),B>=2?"=":p.charAt((z>>>6)&63),B>=1?"=":p.charAt(z&63)];return A.join("")};var l=r.btoa?function(z){return r.btoa(z)}:function(z){return z.replace(/[\s\S]{1,3}/g,q)};var o=s?function(z){return(z.constructor===s.constructor?z:new s(z)).toString("base64")}:function(z){return l(h(z))};var f=function(z,A){return !A?o(String(z)):o(String(z)).replace(/[+\/]/g,function(B){return B=="+"?"-":"_"}).replace(/=/g,"")};var u=function(z){return f(z,true)};var d=new RegExp(["[\xC0-\xDF][\x80-\xBF]","[\xE0-\xEF][\x80-\xBF]{2}","[\xF0-\xF7][\x80-\xBF]{3}"].join("|"),"g");var t=function(B){switch(B.length){case 4:var z=((7&B.charCodeAt(0))<<18)|((63&B.charCodeAt(1))<<12)|((63&B.charCodeAt(2))<<6)|(63&B.charCodeAt(3)),A=z-65536;return(v((A>>>10)+55296)+v((A&1023)+56320));case 3:return v(((15&B.charCodeAt(0))<<12)|((63&B.charCodeAt(1))<<6)|(63&B.charCodeAt(2)));default:return v(((31&B.charCodeAt(0))<<6)|(63&B.charCodeAt(1)))}};var b=function(z){return z.replace(d,t)};var a=function(D){var z=D.length,B=z%4,C=(z>0?c[D.charAt(0)]<<18:0)|(z>1?c[D.charAt(1)]<<12:0)|(z>2?c[D.charAt(2)]<<6:0)|(z>3?c[D.charAt(3)]:0),A=[v(C>>>16),v((C>>>8)&255),v(C&255)];A.length-=[0,0,2,1][B];return A.join("")};var i=r.atob?function(z){return r.atob(z)}:function(z){return z.replace(/[\s\S]{1,4}/g,a)};var w=s?function(z){return(z.constructor===s.constructor?z:new s(z,"base64")).toString()}:function(z){return b(i(z))};var m=function(z){return w(String(z).replace(/[-_]/g,function(A){return A=="-"?"+":"/"}).replace(/[^A-Za-z0-9\+\/]/g,""))};var y=function(){var z=r.Base64;r.Base64=j;return z};r.Base64={VERSION:e,atob:i,btoa:l,fromBase64:m,toBase64:f,utob:h,encode:f,encodeURI:u,btou:b,decode:m,noConflict:y};if(typeof Object.defineProperty==="function"){var n=function(z){return{value:z,enumerable:false,writable:true,configurable:true}};r.Base64.extendString=function(){Object.defineProperty(String.prototype,"fromBase64",n(function(){return m(this)}));Object.defineProperty(String.prototype,"toBase64",n(function(z){return f(this,z)}));Object.defineProperty(String.prototype,"toBase64URI",n(function(){return f(this,true)}))}}if(r["Meteor"]){Base64=r.Base64}if(typeof module!=="undefined"&&module.exports){module.exports.Base64=r.Base64}if(typeof define==="function"&&define.amd){define([],function(){return r.Base64})}})(typeof self!=="undefined"?self:typeof window!=="undefined"?window:typeof global!=="undefined"?global:this);

/**
 * outclick.min.js
 * Version: 0.1.0
 * Author: Joseph Thomas
 * https://github.com/joe-tom/outclick/blob/master/release/outclick.min.js
 */
(function(e){var g={},f=[{listener:null,exceptions:[]}],h=Node.prototype.addEventListener,k=Node.prototype.removeEventListener;Object.defineProperty(Node.prototype,"onoutclick",{set:function(c){f[0]={exceptions:[this],listener:c&&c.bind(this)};return c}});e.Node.prototype.addEventListener=function(c,a,d){if("outclick"==c){for(var b;g[b=(1E5*Math.random()).toString()];);g[b]=a;d=d||[];d.push(this);f.push({exceptions:d,listener:a&&a.bind(this),id:b});return b}h.apply(this,arguments)};e.document.addEventListener("click",
    function(c){for(var a=f.length;a--;){for(var d=f[a],b=!1,e=d.exceptions.length;e--;)if(d.exceptions[e].contains(c.target)){b=!0;break}b||d.listener&&d.listener(c)}});e.Node.prototype.removeEventListener=function(c,a){if("outclick"==c){var d=-1;if("function"==typeof a)for(b in g){if(a.toString()==g[b].toString()){d=b;break}}else d=a;for(var b=f.length;b--;)if(f[b].id==d){f.splice(b,1);break}}else k.apply(this,arguments)};e=document.querySelectorAll("[outclick]");[].forEach.call(e,function(c){var a=
    c.getAttribute("outclick"),a=Function(a);f.push({listener:a,exceptions:[c]})})})(window);

/* ========================================================================
 * Bootstrap: tooltip.js v3.3.7
 * http://getbootstrap.com/javascript/#tooltip
 * Inspired by the original jQuery.tipsy by Jason Frame
 * ========================================================================
 * Copyright 2011-2016 Twitter, Inc.
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 * ======================================================================== */
+function(d){var c=function(f,e){this.type=null;this.options=null;this.enabled=null;this.timeout=null;this.hoverState=null;this.$element=null;this.inState=null;this.init("tooltip",f,e)};c.VERSION="3.3.7";c.TRANSITION_DURATION=150;c.DEFAULTS={animation:true,placement:"top",selector:false,template:'<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',trigger:"hover focus",title:"",delay:0,html:false,container:false,viewport:{selector:"body",padding:0}};c.prototype.init=function(l,j,g){this.enabled=true;this.type=l;this.$element=d(j);this.options=this.getOptions(g);this.$viewport=this.options.viewport&&d(d.isFunction(this.options.viewport)?this.options.viewport.call(this,this.$element):(this.options.viewport.selector||this.options.viewport));this.inState={click:false,hover:false,focus:false};if(this.$element[0] instanceof document.constructor&&!this.options.selector){throw new Error("`selector` option must be specified when initializing "+this.type+" on the window.document object!")}var k=this.options.trigger.split(" ");for(var h=k.length;h--;){var f=k[h];if(f=="click"){this.$element.on("click."+this.type,this.options.selector,d.proxy(this.toggle,this))}else{if(f!="manual"){var m=f=="hover"?"mouseenter":"focusin";var e=f=="hover"?"mouseleave":"focusout";this.$element.on(m+"."+this.type,this.options.selector,d.proxy(this.enter,this));this.$element.on(e+"."+this.type,this.options.selector,d.proxy(this.leave,this))}}}this.options.selector?(this._options=d.extend({},this.options,{trigger:"manual",selector:""})):this.fixTitle()};c.prototype.getDefaults=function(){return c.DEFAULTS};c.prototype.getOptions=function(e){e=d.extend({},this.getDefaults(),this.$element.data(),e);if(e.delay&&typeof e.delay=="number"){e.delay={show:e.delay,hide:e.delay}}return e};c.prototype.getDelegateOptions=function(){var e={};var f=this.getDefaults();this._options&&d.each(this._options,function(g,h){if(f[g]!=h){e[g]=h}});return e};c.prototype.enter=function(f){var e=f instanceof this.constructor?f:d(f.currentTarget).data("bs."+this.type);if(!e){e=new this.constructor(f.currentTarget,this.getDelegateOptions());d(f.currentTarget).data("bs."+this.type,e)}if(f instanceof d.Event){e.inState[f.type=="focusin"?"focus":"hover"]=true}if(e.tip().hasClass("in")||e.hoverState=="in"){e.hoverState="in";return}clearTimeout(e.timeout);e.hoverState="in";if(!e.options.delay||!e.options.delay.show){return e.show()}e.timeout=setTimeout(function(){if(e.hoverState=="in"){e.show()}},e.options.delay.show)};c.prototype.isInStateTrue=function(){for(var e in this.inState){if(this.inState[e]){return true}}return false};c.prototype.leave=function(f){var e=f instanceof this.constructor?f:d(f.currentTarget).data("bs."+this.type);if(!e){e=new this.constructor(f.currentTarget,this.getDelegateOptions());d(f.currentTarget).data("bs."+this.type,e)}if(f instanceof d.Event){e.inState[f.type=="focusout"?"focus":"hover"]=false}if(e.isInStateTrue()){return}clearTimeout(e.timeout);e.hoverState="out";if(!e.options.delay||!e.options.delay.hide){return e.hide()}e.timeout=setTimeout(function(){if(e.hoverState=="out"){e.hide()}},e.options.delay.hide)};c.prototype.show=function(){var o=d.Event("show.bs."+this.type);if(this.hasContent()&&this.enabled){this.$element.trigger(o);var p=d.contains(this.$element[0].ownerDocument.documentElement,this.$element[0]);if(o.isDefaultPrevented()||!p){return}var n=this;var l=this.tip();var h=this.getUID(this.type);this.setContent();l.attr("id",h);this.$element.attr("aria-describedby",h);if(this.options.animation){l.addClass("fade")}var k=typeof this.options.placement=="function"?this.options.placement.call(this,l[0],this.$element[0]):this.options.placement;var s=/\s?auto?\s?/i;var t=s.test(k);if(t){k=k.replace(s,"")||"top"}l.detach().css({top:0,left:0,display:"block"}).addClass(k).data("bs."+this.type,this);this.options.container?l.appendTo(this.options.container):l.insertAfter(this.$element);this.$element.trigger("inserted.bs."+this.type);var q=this.getPosition();var f=l[0].offsetWidth;var m=l[0].offsetHeight;if(t){var j=k;var r=this.getPosition(this.$viewport);k=k=="bottom"&&q.bottom+m>r.bottom?"top":k=="top"&&q.top-m<r.top?"bottom":k=="right"&&q.right+f>r.width?"left":k=="left"&&q.left-f<r.left?"right":k;l.removeClass(j).addClass(k)}var i=this.getCalculatedOffset(k,q,f,m);this.applyPlacement(i,k);var g=function(){var e=n.hoverState;n.$element.trigger("shown.bs."+n.type);n.hoverState=null;if(e=="out"){n.leave(n)}};d.support.transition&&this.$tip.hasClass("fade")?l.one("bsTransitionEnd",g).emulateTransitionEnd(c.TRANSITION_DURATION):g()}};c.prototype.applyPlacement=function(j,k){var l=this.tip();var g=l[0].offsetWidth;var q=l[0].offsetHeight;var f=parseInt(l.css("margin-top"),10);var i=parseInt(l.css("margin-left"),10);if(isNaN(f)){f=0}if(isNaN(i)){i=0}j.top+=f;j.left+=i;d.offset.setOffset(l[0],d.extend({using:function(r){l.css({top:Math.round(r.top),left:Math.round(r.left)})}},j),0);l.addClass("in");
    var e=l[0].offsetWidth;var m=l[0].offsetHeight;if(k=="top"&&m!=q){j.top=j.top+q-m}var p=this.getViewportAdjustedDelta(k,j,e,m);if(p.left){j.left+=p.left}else{j.top+=p.top}var n=/top|bottom/.test(k);var h=n?p.left*2-g+e:p.top*2-q+m;var o=n?"offsetWidth":"offsetHeight";l.offset(j);this.replaceArrow(h,l[0][o],n)};c.prototype.replaceArrow=function(g,e,f){this.arrow().css(f?"left":"top",50*(1-g/e)+"%").css(f?"top":"left","")};c.prototype.setContent=function(){var f=this.tip();var e=this.getTitle();f.find(".tooltip-inner")[this.options.html?"html":"text"](e);f.removeClass("fade in top bottom left right")};c.prototype.hide=function(j){var g=this;var i=d(this.$tip);var h=d.Event("hide.bs."+this.type);function f(){if(g.hoverState!="in"){i.detach()}if(g.$element){g.$element.removeAttr("aria-describedby").trigger("hidden.bs."+g.type)}j&&j()}this.$element.trigger(h);if(h.isDefaultPrevented()){return}i.removeClass("in");d.support.transition&&i.hasClass("fade")?i.one("bsTransitionEnd",f).emulateTransitionEnd(c.TRANSITION_DURATION):f();this.hoverState=null;return this};c.prototype.fixTitle=function(){var e=this.$element;if(e.attr("title")||typeof e.attr("data-original-title")!="string"){e.attr("data-original-title",e.attr("title")||"").attr("title","")}};c.prototype.hasContent=function(){return this.getTitle()};c.prototype.getPosition=function(g){g=g||this.$element;var i=g[0];var f=i.tagName=="BODY";var h=i.getBoundingClientRect();if(h.width==null){h=d.extend({},h,{width:h.right-h.left,height:h.bottom-h.top})}var k=window.SVGElement&&i instanceof window.SVGElement;var l=f?{top:0,left:0}:(k?null:g.offset());var e={scroll:f?document.documentElement.scrollTop||document.body.scrollTop:g.scrollTop()};var j=f?{width:d(window).width(),height:d(window).height()}:null;return d.extend({},h,e,j,l)};c.prototype.getCalculatedOffset=function(e,h,f,g){return e=="bottom"?{top:h.top+h.height,left:h.left+h.width/2-f/2}:e=="top"?{top:h.top-g,left:h.left+h.width/2-f/2}:e=="left"?{top:h.top+h.height/2-g/2,left:h.left-f}:{top:h.top+h.height/2-g/2,left:h.left+h.width}};c.prototype.getViewportAdjustedDelta=function(h,k,e,j){var m={top:0,left:0};if(!this.$viewport){return m}var g=this.options.viewport&&this.options.viewport.padding||0;var l=this.getPosition(this.$viewport);if(/right|left/.test(h)){var n=k.top-g-l.scroll;var i=k.top+g-l.scroll+j;if(n<l.top){m.top=l.top-n}else{if(i>l.top+l.height){m.top=l.top+l.height-i}}}else{var o=k.left-g;var f=k.left+g+e;if(o<l.left){m.left=l.left-o}else{if(f>l.right){m.left=l.left+l.width-f}}}return m};c.prototype.getTitle=function(){var g;var e=this.$element;var f=this.options;g=e.attr("data-original-title")||(typeof f.title=="function"?f.title.call(e[0]):f.title);return g};c.prototype.getUID=function(e){do{e+=~~(Math.random()*1000000)}while(document.getElementById(e));return e};c.prototype.tip=function(){if(!this.$tip){this.$tip=d(this.options.template);if(this.$tip.length!=1){throw new Error(this.type+" `template` option must consist of exactly 1 top-level element!")}}return this.$tip};c.prototype.arrow=function(){return(this.$arrow=this.$arrow||this.tip().find(".tooltip-arrow"))};c.prototype.enable=function(){this.enabled=true};c.prototype.disable=function(){this.enabled=false};c.prototype.toggleEnabled=function(){this.enabled=!this.enabled};c.prototype.toggle=function(g){var f=this;if(g){f=d(g.currentTarget).data("bs."+this.type);if(!f){f=new this.constructor(g.currentTarget,this.getDelegateOptions());d(g.currentTarget).data("bs."+this.type,f)}}if(g){f.inState.click=!f.inState.click;if(f.isInStateTrue()){f.enter(f)}else{f.leave(f)}}else{f.tip().hasClass("in")?f.leave(f):f.enter(f)}};c.prototype.destroy=function(){var e=this;clearTimeout(this.timeout);this.hide(function(){e.$element.off("."+e.type).removeData("bs."+e.type);if(e.$tip){e.$tip.detach()}e.$tip=null;e.$arrow=null;e.$viewport=null;e.$element=null})};function b(e){return this.each(function(){var h=d(this);var g=h.data("bs.tooltip");var f=typeof e=="object"&&e;if(!g&&/destroy|hide/.test(e)){return}if(!g){h.data("bs.tooltip",(g=new c(this,f)))}if(typeof e=="string"){g[e]()}})}var a=d.fn.tooltip;d.fn.tooltip=b;d.fn.tooltip.Constructor=c;d.fn.tooltip.noConflict=function(){d.fn.tooltip=a;return this}}(jQuery);