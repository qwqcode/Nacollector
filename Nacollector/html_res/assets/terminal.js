/**
 * Created by Zneia on 2017/7/15.
 */

const CONTENT_CONT = '.app-terminal .terminal-content';

var ScrollKeepBottom = true; // 是否保持终端滚动条在底部

/**
 * 页面初始化
 */
$(document).ready(function () {
    setTimeout(function () {
        $('body,html').css("background", "#2b2b2b");
    }, 10);

    // 当滚动终端时，若 没有在页底 则 ScrollKeepBottom = false
    $(WRAP_CONT).scroll(function () {
        ScrollKeepBottom = false;

        var documentheight = $(WRAP_CONT + ' > .container').innerHeight(),
            totalheight = $(WRAP_CONT).height() + $(WRAP_CONT).scrollTop();

        if (documentheight === totalheight) {
            ScrollKeepBottom = true;
        }
    });
});

window.Te = {
    // 日志类型
    lt: {
        normal: {tn: "normal"},
        success: {tn: "success", tg: "成功"},
        info: {tn: "info", tg: "消息"},
        warning: {tn: "warning", tg: "警告"},
        error: {tn: "error", tg: "错误"}
    },
    // 记一条日志
    log: function (textBase64, logType) {
        var line = $('<div class="line" style="display: none" />')
            .addClass(logType['tn']);
        var htmlCode = '';
        // 添加标签
        if (logType['tg']) htmlCode += $.sprintf('<span class="tag">[%s]</span>', logType['tg']);
        // 添加内容
        htmlCode += this.logContent(Base64.decode(textBase64));
        // 把代码放到终端内
        line.html(htmlCode).appendTo(CONTENT_CONT);
        // 渐进 · 显示
        line.fadeIn();
        // 终端滚动到底部
        if (ScrollKeepBottom)
            $(WRAP_CONT).scrollTop($(WRAP_CONT)[0].scrollHeight);
    },
    // 日志内容处理
    logContent: function (str) {
        // $('<div>').text(str).html() // Html encode
        return str.replace(/\n/g, "<br/>")
            .replace(/\s/g, '&nbsp;')
    },
    // 测试
    test: function () {
        Te.log(Base64.encode("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et\n" +
            "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip\n" +
            "ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu\n" +
            "fugiat nulla pariatur."), Te.lt.normal);
        Te.log(Base64.encode("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et\n" +
            "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip\n" +
            "ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu\n" +
            "fugiat nulla pariatur."), Te.lt.success);
        Te.log(Base64.encode("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et\n" +
            "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip\n" +
            "ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu\n" +
            "fugiat nulla pariatur."), Te.lt.info);
        Te.log(Base64.encode("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et\n" +
            "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip\n" +
            "ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu\n" +
            "fugiat nulla pariatur."), Te.lt.warning);
        Te.log(Base64.encode("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et\n" +
            "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip\n" +
            "ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu\n" +
            "fugiat nulla pariatur."), Te.lt.error);
    }
};