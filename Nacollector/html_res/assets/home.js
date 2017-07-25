/**
 * Created by Zneia on 2017/7/14.
 */

/**
 * 页面初始化
 */
$(document).ready(function () {
    // 状态栏初始化
    NavBar.btnsAdd({
        downloadManager: {
            icon: 'download',
            title: '下载内容'
        },
        setting: {
            icon: 'settings',
            title: '设置',
            onClick: function () {
                if (ContentBlockLayer.sidebar.get('setting') === null)
                    ContentBlockLayer.sidebar.register("setting").setTitle("设置");
                ContentBlockLayer.sidebar.get("setting").show();
            }
        }
    });

    // 操作初始化
    AppUi.actionInit();

    // 点击操作按钮列表第一个
    $(BTNS_CONT+' a:nth-child(1)').click();

    // 下载面板初始化
    downloads.init();
});

const BTNS_CONT = '.app-select .btn-list';
const FORM_CONT = '.app-form';

// 当前操作
var CurrentAction = {
    label: null,
    callClassName: null,
    formElements: {}
};

window.AppUi = {
    // 操作初始化
    actionInit: function () {
        // 操作按钮
        $.each(ActionList, function (key, val) {
            $('<a data-call-class-name="'+key+'">'+val['label']+'</a>').appendTo(BTNS_CONT).click(function () {
                // 点击操作按钮事件
                var callClassName = $(this).data('call-class-name');
                if (!ActionList[callClassName])
                    throw ("ActionList 中没有 " + callClassName + "，这是一个无效的按钮！");

                // 清除当前操作所有表单元素
                $(FORM_CONT).html('');
                // 清除当前操作数据
                CurrentAction.label = null;
                CurrentAction.callClassName = null;
                CurrentAction.formElements = {};

                // 装入新的操作
                CurrentAction.label = ActionList[callClassName]['label'];
                CurrentAction.callClassName = callClassName;
                ActionList[callClassName].loadForm(); // 执行表单元素创建

                $('<div class="form-btns">\n<button class="start-task-btn" type="submit">执行任务</button>\n</div>')
                    .appendTo(FORM_CONT)
                    .click(function () {
                        AppUi.taskNewCall();
                        return false;
                    }); // 执行任务按钮
                // 按钮选中
                $(BTNS_CONT+' a').removeClass('active');
                $(this).addClass('active');
            });
        });
    },
    // 新建任务 通知C#
    taskNewCall: function () {
        var isInputComplete = true;
        $.each($(FORM_CONT+' .form-control'), function (index, obj) {
            if ($.trim(($(obj).val())) === "") {
                $(obj).focus();
                isInputComplete = false;
                return false;
            }
            // 验证不过关
            var inputElem = CurrentAction.formElements[$(obj).attr('name')];
            if (inputElem['validator'] && !inputElem['validator']($.trim(($(obj).val()))))
            {
                $(obj).addClass('has-error')
                    .focus()
                    .bind('input propertychange', function() {
                        if (inputElem['validator']($.trim(($(this).val()))))
                            $(this).unbind('input propertychange').removeClass('has-error');
                    });
                isInputComplete = false;
                return false;
            }
        });
        // 若表单有误则不提交
        if (!isInputComplete) return false;
        // C# 回调
        MainFormCallBack.taskNew(CurrentAction.callClassName, CurrentAction.label, JSON.stringify($(FORM_CONT).serializeArray()));
        console.log($.sprintf('MainFormCallBack.taskNew("%s", "%s", "%s");', CurrentAction.callClassName, CurrentAction.label, JSON.stringify($(FORM_CONT).serializeArray())));
    },
    // 表单控件创建
    formControls: {
        // 文本框
        textInput: function (fieldId, label, defaultVal, validator) {
            defaultVal = defaultVal || '';
            var textInput = $($.sprintf('<div class="form-group">\n<label for="%s">%s</label>\n<input id="%s" name="%s" type="text" class="form-control" autocomplete="off" spellcheck="false" placeholder="输入文字" value="%s">\n</div>', fieldId, label, fieldId, fieldId, defaultVal))
                .appendTo(FORM_CONT);
            CurrentAction.formElements[fieldId] = {
                input: textInput.find('.form-control'),
                validator: (validator ? validator : null)
            };
        },
        // 数字框
        numberInput: function (fieldId, label, defaultVal, min, max) {
            defaultVal = defaultVal || '';
            min = min || '';
            max = max || '';
            var textInput = $($.sprintf('<div class="form-group">\n<label for="%s">%s</label>\n<input id="%s" name="%s" type="number" class="form-control" autocomplete="off" spellcheck="false" placeholder="输入数字" value="%s" min="%s" max="%s">\n</div>', fieldId, label, fieldId, fieldId, defaultVal, min, max))
                .appendTo(FORM_CONT);
            CurrentAction.formElements[fieldId] = {
                input: textInput.find('.form-control')
            };
        },
        // 多行文本框
        textareaInput: function (fieldId, label, defaultVal, height) {
            defaultVal = defaultVal || '';
            var textareaInput = $($.sprintf('<div class="form-group">\n<label for="%s">%s</label>\n<textarea id="%s" name="%s" class="form-control" spellcheck="false" placeholder="输入文字">%s</textarea>\n</div>', fieldId, label, fieldId, fieldId, defaultVal))
                .appendTo(FORM_CONT);

            CurrentAction.formElements[fieldId] = {
                input: textareaInput.find('.form-control')
            };

            // 设置高度
            if (height)
                CurrentAction.formElements[fieldId]['input'].css('height', height);
        },
        // 选择菜单
        selectInput: function (fieldId, label, values, selectValue) {
            var valuesStr = '';
            $.each(values, function (val, label) {
                val = (typeof(val)!=="number") ? val : label;
                valuesStr += $.sprintf('<option value="%s"%s>%s</option>', val, (val === selectValue ? 'selected' : ''), label);
            });
            var selectInput = $($.sprintf('<div class="form-group">\n<label for="%s">%s</label>\n<select id="%s" name="%s" class="form-control">\n%s\n</select>\n</div>', fieldId, label, fieldId, fieldId, valuesStr))
                .appendTo(FORM_CONT);

            CurrentAction.formElements[fieldId] = {
                input: selectInput.find('.form-control')
            };
        }
    }
};

window.downloads = {
    data: {
        list: {}
    },
    statusList: {
        downloading: 1,
        pause: 2,
        done: 3,
        cancelled: 4,
        fail: 5
    },
    actionList: {
        pause: 1,
        resume: 2,
        cancel: 3
    },
    sel: {
        downloadsList: null
    },
    panelKey: 'downloads',
    // 初始化
    init: function () {
        var panelObj = NavBar.panel.register(this.panelKey, 'downloadManager');
        panelObj.setTitle('<i class="zmdi zmdi-download"></i> 下载内容');
        panelObj.setInner('<div class="downloads-list"></div>');
        panelObj.setSize(400, 430);
        this.sel.downloadsList = panelObj.getSel() + ' .downloads-list';
    },
    // 新增任务
    addTask: function (json) {
        // console.log("ADD: " + JSON.stringify(json));
        this._addTask(json['key'], json['fullPath'], json['downloadUrl'], json['totalBytes']);
    },
    _addTask: function (key, fullPath, downloadUrl, totalBytes) {
        if (this.data.list[key])
            throw (key + ' 下载任务已存在，无需再新建');

        this.data.list[key] = {
            fullPath: fullPath,
            downloadUrl: downloadUrl,
            totalBytes: totalBytes,
            receivedBytes: 0,
            currentSpeed: 0,
            status: 0
        };
    },
    // 更新任务
    updateTask: function (json) {
        // console.log("UPD: " + JSON.stringify(json));
        this._updateTask(json['key'], json['receivedBytes'], json['currentSpeed'], json['status'], json['fullPath'], json['downloadUrl']);
    },
    _updateTask: function (key, receivedBytes, currentSpeed, status, fullPath, downloadUrl) {
        if (!this.data.list[key])
            throw (key + ' 下载任务不存在，或许已被删除');

        this.data.list[key].receivedBytes = receivedBytes;
        this.data.list[key].currentSpeed = currentSpeed;
        this.data.list[key].status = status;

        if (this.data.list[key].fullPath !== fullPath && fullPath !== "")
            this.data.list[key].fullPath = fullPath;

        if (this.data.list[key].downloadUrl !== downloadUrl)
            this.data.list[key].downloadUrl = downloadUrl;

        this.updateItemUi(key); // 刷新界面
    },
    // 列表项目获取 Selector
    getItemSelector: function (key) {
        return $.sprintf('%s [data-key="%s"]', this.sel.downloadsList, key); // $().find() 导致界面不停更新；当不断执行一个方法时，拒绝使用 find()
    },
    // 更新列表项目 UI
    updateItemUi: function (key) {
        if (!this.data.list[key])
            throw (key + ' 下载任务不存在，或许已被删除');

        var taskData = this.data.list[key];
        var selItem = this.getItemSelector(key); // $().find() 导致界面不停更新；当不断执行一个方法时，拒绝使用 find()

        if ($(selItem).length === 0) {
            // 新增一个 item 并返回 selector
            $('<div class="download-item" data-key="' + key + '">\n<div class="details">\n<div class="header">\n<a class="file-name" onclick="downloads.fileLaunch(\''+key+'\')"></a><a class="download-url" onclick="downloads.urlOpenInDefaultBrowser(\''+key+'\')">' + taskData.downloadUrl + '</a>\n</div>\n<div class="description"></div>\n<div class="progress"><div class="progress-bar"></div></div>\n<div class="action-bar"></div>\n</div>\n<div class="icon-wrapper">\n<button class="remove-btn" title="从列表中移除" onclick="downloads.taskRemove(\''+key+'\')">✕</button>\n</div>\n</div>').prependTo(this.sel.downloadsList);
        }

        var fileName = $(selItem+' .file-name');
        var progress = $(selItem+' .progress');
        var progressBar = $(selItem+' .progress .progress-bar');
        var description = $(selItem+' .description');
        var actionBar = $(selItem+' .action-bar');

        // 若状态改变 则更新 dl-status
        if ($(selItem).attr('dl-status') !== this.getStatusName(taskData.status))
            $(selItem).attr('dl-status', this.getStatusName(taskData.status));

        // 文件名 （路径 => 文件名）
        if (fileName.text() !== this.extractFilename(taskData.fullPath))
            fileName.text(this.extractFilename(taskData.fullPath));

        // 下载数据
        var speed = this.bytesToSize(taskData.currentSpeed)+'/s'; // 当前速度
        var received = this.bytesToSize(taskData.receivedBytes); // 已下载
        var total = this.bytesToSize(taskData.totalBytes); // 总共大小

        // description & actionBar
        var descriptionText = '';
        var actionBarHtml = '';

        switch (taskData.status) {
            // 下载中
            case this.statusList.downloading:
                if (taskData.totalBytes !== 0) {
                    // Progress Bar
                    if (progress.hasClass('indeterminate'))
                        progress.removeClass('indeterminate');

                    // 进度百分比，保留2位小数
                    var progressPercentage = ((taskData.receivedBytes / taskData.totalBytes) * 100).toFixed(2) + '%';

                    if (progressBar.css('width') !== progressPercentage)
                        progressBar.css('width', progressPercentage);

                    descriptionText = $.sprintf('%s，速度 %s，已下载 %s，共 %s', progressPercentage, speed, received, total);
                } else {
                    // Indeterminate Progress
                    if (!progress.hasClass('indeterminate'))
                        progress.addClass('indeterminate');

                    descriptionText = $.sprintf('速度 %s，已下载 %s', speed, received);
                }

                actionBarHtml = '<a onclick="downloads.taskAction(\''+key+'\', downloads.actionList.pause)">暂停</a>'
                    + '<a onclick="downloads.taskAction(\''+key+'\', downloads.actionList.cancel)">取消</a>';
                break;
            // 暂停
            case this.statusList.pause:
                if (taskData.totalBytes !== 0) {
                    // Progress Bar
                    descriptionText = $.sprintf('%s，已下载 %s，共 %s', progressPercentage, received, total);
                } else {
                    // Indeterminate Progress
                    descriptionText = $.sprintf('已下载 %s', received);
                }

                actionBarHtml = '<a onclick="downloads.taskAction(\''+key+'\', downloads.actionList.resume)">恢复</a>'
                    + '<a onclick="downloads.taskAction(\''+key+'\', downloads.actionList.cancel)">取消</a>';
                break;
            // 完毕
            case this.statusList.done:
                descriptionText = $.sprintf('总大小：%s', received);
                actionBarHtml = '<a onclick="downloads.fileShowInExplorer(\''+key+'\')">在文件夹中显示</a>';
                break;
            // 已取消
            case this.statusList.cancelled:
                actionBarHtml = '<a onclick="downloads.downloadAgain(\''+key+'\')">重试下载</a>';
                break;
            // 错误
            case this.statusList.fail:
                actionBarHtml = '<a onclick="downloads.downloadAgain(\''+key+'\')">重试下载</a>';
                break;
        }

        // description & actionBar
        if (description.text() !== descriptionText)
            description.text(descriptionText);

        if (actionBar.html() !== actionBarHtml)
            actionBar.html(actionBarHtml);
    },
    // 下达任务操作命令
    taskAction: function (key, action) {
        if (!this.data.list[key])
            throw ('任务操作失败，或许已被删除，未找到 ' + key);

        MainFormCallBack.downloadTaskAction(key, action);
    },
    // 任务从列表移除
    taskRemove: function (key) {
        if (!this.data.list[key])
            throw ('任务从列表移除失败，或许已被删除，未找到 ' + key);

        if (this.isTaskInProgress(key)) {
            MainFormCallBack.downloadTaskAction(key, this.actionList.cancel);
        }

        delete this.data.list[key];

        $(this.getItemSelector(key)).hide();
    },
    // 获取正在下载的任务数
    countDownloadingTask: function () {
        var num = 0;
        for (var key in this.data.list) {
            if (this.isTaskInProgress(key))
                num ++;
        }
        return num;
    },
    // 程序启动事件
    appLoadEvent: function (downloadsListJson) {
        if (typeof(downloadsListJson) !== "object" || Object.prototype.toString.call(downloadsListJson).toLowerCase() !== "[object object]" || !!downloadsListJson.length)
            throw ("[downloads.appLoadEvent] 参数非JSON对象");

        // console.log(JSON.stringify(downloadsListJson));
        for (var key in downloadsListJson) {
            if (!downloadsListJson.hasOwnProperty(key))
                continue;

            this.data.list[key] = downloadsListJson[key];
            if (this.isTaskInProgress(key))
                this.data.list[key].status = this.statusList.cancelled;

            this.updateItemUi(key);
        }
    },
    // 程序退出事件
    appExitEvent: function () {
        for (var key in this.data.list) {
            if (this.isTaskInProgress(key))
                this.taskAction(key, this.actionList.cancel);
        }

        return JSON.stringify(this.data.list);
    },
    // 启动文件
    fileLaunch: function (key) {
        if (!this.data.list[key] || this.data.list[key].status !== this.statusList.done)
            return;

        MainFormCallBack.fileLaunch(this.data.list[key].fullPath).then(function(isSuccess){
            if (!isSuccess) {
                downloads.data.list[key].status = downloads.statusList.cancelled;
                downloads.updateItemUi(key);
            }
        });
    },
    // URL 在系统默认浏览器中打开
    urlOpenInDefaultBrowser: function (key) {
        if (!this.data.list[key])
            return;

        MainFormCallBack.urlOpenInDefaultBrowser(this.data.list[key].downloadUrl);
    },
    // 文件在资源管理器中显示
    fileShowInExplorer: function (key) {
        if (!this.data.list[key])
            return;

        MainFormCallBack.fileShowInExplorer(this.data.list[key].fullPath).then(function(isSuccess){
            if (!isSuccess) {
                downloads.data.list[key].status = downloads.statusList.cancelled;
                downloads.updateItemUi(key);
            }
        });
    },
    // 任务是否正在进行中
    isTaskInProgress: function (key) {
        if (!this.data.list[key])
            return false;

        if ((this.data.list[key].status !== this.statusList.done) &&
            (this.data.list[key].status !== this.statusList.cancelled)) {
            return true;
        } else {
            return false;
        }
    },
    // 重新下载
    downloadAgain: function (key) {
        if (!this.data.list[key])
            return false;

        var $a = $("<a></a>")
            .attr("href", this.data.list[key].downloadUrl)
            .attr("download", "");
        $a[0].click();
    },
    // 获取任务数据
    getTask: function (key) {
        if (!this.data.list[key])
            return null;

        return this.data.list[key];
    },
    // 获取状态名
    getStatusName: function (status) {
        for (var key in this.statusList) {
            if (this.statusList.hasOwnProperty(key) && this.statusList[key] === status) {
                return key;
            }
        }
    },
    // 路径中提取文件名
    extractFilename: function(path) {
        var lastSlash = Math.max(path.lastIndexOf('\\'), path.lastIndexOf('/'));
        return path.substring(lastSlash + 1);
    },
    // bytes 格式化
    bytesToSize: function (bytes) {
        if (bytes === 0) return '0 B';
        var k = 1000, // or 1024
            sizes = ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'],
            i = Math.floor(Math.log(bytes) / Math.log(k));
        return (bytes / Math.pow(k, i)).toPrecision(3) + ' ' + sizes[i];
    }
};