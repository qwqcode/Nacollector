/**
 * Created by Zneia on 2017/7/15.
 */

/**
 * 页面初始化
 */
$(document).ready(function () {
    // 初始化 NavBar
    AppNavbar.init();
    // 初始化 Tooltip
    $('[data-toggle="tooltip"]').tooltip();
    // 浏览器初始化时白色闪光 减少违和感
    setTimeout(function () {
        $(WRAP_SEL).css("opacity","1");
    }, 10);
    // 状态栏初始化
    AppNavbar.btnsAdd({
        downloadManager: {
            icon: 'download',
            title: '下载内容'
        },
        setting: {
            icon: 'settings',
            title: '设置',
            onClick: function () {
                if (AppLayer.sidebar.get('setting') === null)
                    AppLayer.sidebar.register("setting").setTitle("设置");
                AppLayer.sidebar.get("setting").toggle();
            }
        }
    });
    // 任务生成器初始化
    TaskGen.init();
    // 点击操作按钮列表第一个
    $(TaskGen.sel.formToggleBtns+' a:nth-child(1)').click();
    // 下载面板初始化
    downloads.init();
});

/**
 * Selectors
 */
const WRAP_SEL = '.wrap';

/**
 * 导航栏
 */
var AppNavbar = {
    sel: {
        nav: '.top-nav-bar',
        navTitle: '.top-nav-bar .nav-title',
        navBtns: '.top-nav-bar .nav-btns'
    },
    // 初始化 Navbar
    init: function () {
        $('<div class="left-items"><div class="nav-title"></div></div><div class="right-items"><div class="nav-btns"></div></div>').appendTo(this.sel.nav);
    },
    // 标题设置
    titleSet: function (value, base64) {
        if (typeof base64 === "boolean" && base64 === true)
            value = Base64.decode(value);

        var navTitleSel = this.sel.navTitle;
        $(navTitleSel).addClass('changing');
        setTimeout(function() {
            $(navTitleSel)
                .text(value)
                .removeClass('changing');
        }, 100);
    },
    // 标题获取
    titleGet: function () {
        return $(this.sel.navTitle).text();
    },
    // 添加按钮
    btnAdd: function (name, icon, title, onClickEvent) {
        var dom = $('<a data-nav-btn="'+name+'" data-placement="bottom" title="'+title+'"><i class="zmdi zmdi-'+icon+'"></i></a>');
            if (onClickEvent !== undefined)
                dom.click(onClickEvent);
            dom.appendTo(this.sel.navBtns).tooltip();
    },
    // 按钮批量添加
    btnsAdd: function (navbarBtnList) {
        $.each(navbarBtnList, function (name, value) {
            AppNavbar.btnAdd(name, value['icon'], value['title'], value['onClick']);
        });
    },
    // 获取按钮 Selector
    getBtnSel: function (name) {
        return '[data-nav-btn="'+name+'"]';
    },
    // 面板
    panel: {
        list: {},
        // 注册新面板
        register: function (key, btnName) {
            if (this.list.hasOwnProperty(key))
                return '导航栏面板： ' + key + ' 已存在于list中';

            var btnSel = AppNavbar.getBtnSel(btnName);
            $(btnSel).after('<div class="navbar-panel" data-navbar-panel="'+key+'" />');

            var panelSel = '[data-navbar-panel="'+key+'"]';
            // 工厂模式
            var panelObj = {};
            // 设置标题
            panelObj.setTitle = function (val) {
                $('<div class="panel-header"><div class="panel-title">'+val+'</div></div>').prependTo(panelSel);
            };
            // 设置内容
            panelObj.setInner = function (val) {
                $('<div class="panel-inner">'+val+'</div>').appendTo(panelSel);
            };
            // 设置尺寸
            panelObj.setSize = function (width, height) {
                $(panelSel).css('width', width + 'px');
                $(panelSel).css('height', height + 'px');
            };
            // 自动调整位置
            panelObj.setPosition = function () {
                var position = $.getPosition($(btnSel));
                var panelWidth = $(panelSel).outerWidth();
                $(panelSel)
                    .css('top', position['top'] + 'px')
                    .css('left', position['right'] - panelWidth + 'px');
            };
            // 显示
            panelObj.show = function () {
                if (panelObj.isShow())
                    throw ('导航栏面板：' + key + ' 已显示');

                panelObj.setPosition();
                $(panelSel).addClass('show');
                // 若点按的元素非面板内元素
                setTimeout(function () {
                    $(document).bind('click.nav-panel-' + key, function (e) {
                        if(!$(e.target).is(panelSel) && !$(e.target).closest(panelSel).length) {
                            panelObj.hide();
                        }
                    });
                }, 20);
                // 自动调整面板位置
                $(window).bind('resize.nav-panel-' + key, function () {
                    panelObj.setPosition();
                });
            };
            // 隐藏
            panelObj.hide = function () {
                if (!panelObj.isShow())
                    throw ('导航栏面板：' + key + ' 未显示');

                $(panelSel).removeClass('show');
                $(window).unbind('resize.nav-panel-' + key);
                $(document).unbind('click.nav-panel-' + key); // 解绑事件
            };
            // 切换
            panelObj.toggle = function () {
                if (!panelObj.isShow(key)) {
                    panelObj.show(key);
                } else {
                    panelObj.hide(key);
                }
            };
            // 是否显示
            panelObj.isShow = function () {
                return !!($(panelSel).hasClass('show'));
            };
            // 获取 Selector
            panelObj.getSel = function () {
                return panelSel;
            };

            // 导航栏按钮点击绑定
            $(btnSel).bind('click', function () {
                panelObj.toggle();
            });

            // 加入 List
            this.list[key] = panelObj;

            return panelObj;
        },
        // 获取面板
        get: function (key) {
            if (!this.list.hasOwnProperty(key))
                return null;

            return this.list[key];
        }
    }
};

/**
 * 操作列表 (Key 为 className (C#调用类名) )
 */
var SpiderList = {
    CollItemDescImg: {
        label: "商品详情页图片解析",
        genForm: function () {
            TaskGen.formHelper.textInput('PageUrl', '详情页链接', '', inputValidators.isUrl);
            TaskGen.formHelper.selectInput('PageType', '链接类型', {
                "Tmall": "天猫",
                "Taobao": "淘宝",
                "Alibaba": "阿里巴巴",
                "Suning": "苏宁易购",
                "Gome": "国美在线"
            });
            TaskGen.formHelper.selectInput('ImgType', '图片类型', {
                "Thumb": "主图",
                "Category": "分类图",
                "Desc": "详情图"
            });
            TaskGen.formHelper.selectInput('CollType', '采集模式', {
                "collImgSrcUrl": "显示图片链接",
                "collDownloadImgSrc": "显示图片链接 并 下载打包保存",
            });
        }
    },
    TaobaoSellerColl: {
        label: "淘宝店铺搜索卖家ID名采集",
        genForm: function () {
            TaskGen.formHelper.textInput('PageUrl', '店铺搜索页链接', '', inputValidators.isUrl);
            TaskGen.formHelper.numberInput('CollBeginPage', '采集开始页码', 1, 1);
            TaskGen.formHelper.numberInput('CollEndPage', '采集结束页码', undefined, 1);
        }
    },
    TmallGxptInvite: {
        label: "天猫供销平台分销商一键邀请",
        genForm: function () {
            TaskGen.formHelper.textareaInput('PageUrl', '分销商ID名（一行一个）', undefined, 250);
        }
    },
    TmallGxptInviteDelete: {
        label: "天猫供销平台分销商一键撤回",
        genForm: function () {
            TaskGen.formHelper.numberInput('DeleteBeginPage', '撤回开始页码', 1, 1);
            TaskGen.formHelper.numberInput('DeleteEndPage', '撤回结束页码', undefined, 1);
        }
    }
};

/**
 * Task Generator (任务生成器)
 */
window.TaskGen = {
    sel: {
        form: '.taskgen-form',
        formToggleBtns: '.taskgen-form-toggle .btn-list'
    },
    // 当前
    current: {
        className: null,
        inputs: {}
    },
    // 初始化
    init: function () {
        // 遍历列表 生成按钮
        $.each(SpiderList, function (key, val) {
            var btn = $('<a data-action-class-name="' + key + '">' + val['label'] + '</a>');
            // 按钮点击事件
            btn.click(function () {
                // 表单生成
                TaskGen.formLoad($(this).attr('data-action-class-name'));
                // 按钮选中
                $(TaskGen.sel.formToggleBtns + ' a').removeClass('active');
                $(this).addClass('active');
            });
            btn.appendTo(TaskGen.sel.formToggleBtns);
        });
    },
    // 表单装载
    formLoad: function (className) {
        // 点击操作按钮事件
        if (!SpiderList[className])
            throw ('SpiderList 中没有 ' + className + '，无法创建表单！');

        // 清除当前表单
        $(this.sel.form).html('');
        // 清除当前数据
        this.current.className = null;
        this.current.inputs = {};

        // 装入新数据
        this.current.className = className;
        // 执行表单创建
        SpiderList[className].genForm();

        // 提交按钮
        var submitBtn = $('<div class="form-btns">\n<button class="submit-btn" type="submit">执行任务</button>\n</div>').appendTo(this.sel.form);
        submitBtn.click(function () {
            TaskGen.taskCreate();
            return false;
        });
    },
    // 表单提交检验
    formCheck: function () {
        var isInputAllRight = true;
        $.each(TaskGen.current.inputs, function (i, obj) {
            if (!obj.inputSel || $(obj.inputSel).length === 0)
                throw ('表单输入元素 '+i+' 的 Selector 无效');

            var inputSel = obj.inputSel,
                inputDom = $(inputSel),
                inputVal = inputDom.val().trim();

            if (inputVal === '') {
                inputDom.focus();
                isInputAllRight = false;
                return false;
            }

            // 验证器
            if (!!obj.validator && !obj.validator(inputVal)) {
                inputDom.addClass('has-error').focus();
                inputDom.bind('input propertychange', function() {
                    if (obj.validator($(this).val().trim())) $(this).unbind('input propertychange').removeClass('has-error');
                });
                isInputAllRight = false;
                return false;
            }
        });

        return isInputAllRight;
    },
    // 新建任务
    taskCreate: function () {
        if (!this.formCheck())
            return false;

        Task.createTask(this.current.className, SpiderList[this.current.className].label, $(this.sel.form).serializeArray());
    },
    // 表单控件
    formHelper: {
        // 当前表单数据添加
        _currentInfoAdd: function (fieldName, label, inputTagId, validator) {
            TaskGen.current.inputs[fieldName] = {
                label: label,
                inputSel: '#'+inputTagId
            };
            // 验证器
            if (!!validator)
                TaskGen.current.inputs[fieldName].validator = validator;
        },
        // 文本框
        textInput: function (fieldName, label, defaultVal, validator) {
            defaultVal = defaultVal || '';
            var tagId = 'TaskGen_'+fieldName;
            var formGroup = $('<div class="form-group">\n<label for="'+tagId+'">' + label + '</label>\n</div>').appendTo(TaskGen.sel.form);
            var formInput = $('<input id="'+tagId+'" name="'+fieldName+'" type="text" class="form-control" autocomplete="off" spellcheck="false" placeholder="输入文字" value="'+defaultVal+'">').appendTo(formGroup);
            this._currentInfoAdd(fieldName, label, tagId, validator);
        },
        // 数字框
        numberInput: function (fieldName, label, defaultVal, min, max) {
            defaultVal = defaultVal || '';
            min = min || '';
            max = max || '';
            var tagId = 'TaskGen_'+fieldName;
            var formGroup = $('<div class="form-group">\n<label for="'+tagId+'">' + label + '</label>\n</div>').appendTo(TaskGen.sel.form);
            var formInput = $('<input id="'+tagId+'" name="'+fieldName+'" type="number" class="form-control" autocomplete="off" spellcheck="false" placeholder="输入数字" value="'+defaultVal+'" min="'+min+'" max="'+max+'">').appendTo(formGroup);
            this._currentInfoAdd(fieldName, label, tagId);
        },
        // 多行文本框
        textareaInput: function (fieldName, label, defaultVal, height) {
            defaultVal = defaultVal || '';
            var tagId = 'TaskGen_'+fieldName;
            var formGroup = $('<div class="form-group">\n<label for="'+tagId+'">'+label+'</label>\n</div>').appendTo(TaskGen.sel.form);
            var formInput = $('<textarea id="'+tagId+'" name="'+fieldName+'" class="form-control" spellcheck="false" placeholder="输入文字">'+defaultVal+'</textarea>').appendTo(formGroup);
            if (!!height) formInput.css('height', height); // 设置高度
            this._currentInfoAdd(fieldName, label, tagId);
        },
        // 选择菜单
        selectInput: function (fieldName, label, values, selectValue) {
            var tagId = 'TaskGen_'+fieldName;
            var formGroup = $('<div class="form-group">\n<label for="'+tagId+'">'+label+'</label>\n</div>').appendTo(TaskGen.sel.form);
            var inputHtml = '<select id="'+tagId+'" name="'+fieldName+'" class="form-control">\n';
            $.each(values, function (val, label) {
                val = (typeof(val)!=="number") ? val : label;
                var afterOpt = (val === selectValue ? 'selected' : '');
                inputHtml += '<option value="'+val+'" '+afterOpt+'>'+label+'</option>';
            });
            inputHtml += '\n</select>';
            var formInput = $(inputHtml).appendTo(formGroup);
            this._currentInfoAdd(fieldName, label, tagId);
        }
    }
};

window.Task = {
    sel: {
        runtime: '.task-runtime'
    },
    // 任务列表
    list: {},
    // 当前显示的任务ID
    currentDisplayedTaskId: null,
    // 创建任务
    createTask: function (className, classLabel, parmsObj) {
        var taskId = new Date().getTime().toString();
        var runtimeSel = this.sel.runtime;
        // 创建元素
        $('<div class="task-item" data-task-id="'+taskId+'" style="display: none">\n<div class="container" style="width: 95%;">\n<div class="task-log-table"></div>\n</div>').appendTo(runtimeSel);
        var taskItemSel = '[data-task-id="'+taskId+'"]';
        var taskLogTableSel = taskItemSel + ' .task-log-table';
        // 工厂模式
        var taskObj = {};
        taskObj.originalTitle = null;
        // 设置标题
        taskObj.setTitle = function () {
            taskObj.originalTitle = AppNavbar.titleGet();
            AppNavbar.titleSet(classLabel + ' 任务ID：' + taskId);
        };
        // 恢复成原来的标题
        taskObj.setOriginalTitle = function () {
            AppNavbar.titleSet(taskObj.originalTitle);
        };
        // 显示
        taskObj.show = function () {
            Task.show(taskId);
        };
        // 隐藏
        taskObj.hide = function () {
            Task.hide();
        };
        // 日志
        taskObj.log = function (text, level) {
            var line = $('<div class="line" style="display: none" />');
            var levelsList = {I: '消息', S: '成功', W: '警告', E: '错误'};
            var innerText = '';
            if (!!levelsList[level]) {
                line.attr('data-level', level);
                innerText += '<span class="tag">['+levelsList[level]+']</span> ';
            }
            var textHandle = function (str) {
                str.replace(/\n/g, "<br/>")
                    .replace(/\s/g, '&nbsp;');

                return str;
            };
            innerText += textHandle(text);
            line.html(innerText);
            line.appendTo(taskLogTableSel);
            line.css('display', '');
            taskObj.scrollToBottom();
        };
        // 自动滚动到底部
        taskObj.allowAutoScrollToBottom = true;
        taskObj.scrollToBottom = function () {
            if (!taskObj.allowAutoScrollToBottom)
                throw ('不允许自动滚动到底部啦');

            $(runtimeSel).scrollTop($(runtimeSel)[0].scrollHeight);
        };
        // 中止
        taskObj.abort = function () {
            // C# 调用中止


            taskObj.taskIsEnd();
        };
        // 删除
        taskObj.remove = function () {
            if (taskObj.getIsInProgress()) {
                alert('任务执行中，无法删除 但你可以中止任务');
            }

            // 对象删掉！
            delete Task.list[taskId];
        };
        // 任务是否正在进行中
        taskObj.isInProgress = true;
        // 设置任务已结束
        taskObj.taskIsEnd = function () {
            taskObj.isInProgress = false;
        };
        // 获取任务是否在进行中
        taskObj.getIsInProgress = function () {
            return taskObj.isInProgress;
        };
        // 获取 Selector
        taskObj.getSel = function () {
            return taskItemSel;
        };
        // 获取 Log Table Selector
        taskObj.getLogTableSel = function () {
            return taskLogTableSel;
        };

        this.list[taskId] = taskObj;

        TaskController.createTask(taskId, className, classLabel, JSON.stringify(parmsObj)).then(function(callback){
            taskObj.show();
        });

        return taskObj;
    },
    // 获取任务
    get: function (taskId) {
        if (!this.list.hasOwnProperty(taskId))
            return null;

        return this.list[taskId];
    },
    show: function (taskId) {
        if (!this.get(taskId))
            throw ('未找到任务 '+taskId);

        if (this.currentDisplayedTaskId !== null)
            this.hide();

        var taskObj = this.get(taskId);
        var runtimeSel = this.sel.runtime;
        var taskItemSel = taskObj.getSel();

        $(runtimeSel + ' > *').hide();
        $(taskItemSel).show();
        $(runtimeSel).fadeIn(300);

        $(runtimeSel).on('scroll', function () {
            taskObj.allowAutoScrollToBottom = false;

            var documentheight = $(taskObj.getSel() + ' > .container').innerHeight(),
                totalheight = $(runtimeSel).height() + $(runtimeSel).scrollTop();

            if (documentheight === totalheight) {
                taskObj.allowAutoScrollToBottom = true;
            }
        });

        taskObj.setTitle();

        this.currentDisplayedTaskId = taskId;
    },
    // 隐藏
    hide: function () {
        if (this.currentDisplayedTaskId === null)
            throw ('未显示任何任务');

        var runtimeSel = this.sel.runtime;

        $(runtimeSel).fadeOut(300);
        $(runtimeSel).off('scroll');

        this.get(this.currentDisplayedTaskId).setOriginalTitle();

        this.currentDisplayedTaskId = null;
    }
};

/**
 * 内容层
 */
var AppLayer = {
    // 侧边栏
    sidebar: {
        list: {},
        // 注册新的 Sidebar
        register: function (key) {
            if (this.list.hasOwnProperty(key))
                return '侧边栏层： ' + key + ' 已存在于list中';

            var layerSel = this.getLayerSel();
            $('<div class="sidebar-block" data-sidebar-layer-key="'+key+'" />')
                .appendTo($(layerSel));
            var sidebarSel = '[data-sidebar-layer-key="' + key + '"]';
            var sidebarObj = {};
            // 设置标题
            sidebarObj.setTitle = function (val) {
                $('<div class="sidebar-header"><div class="header-left">'+val+'</div><div class="header-right"><button type="button" data-toggle="sidebar-layer-hide"><i class="zmdi zmdi-close"></i></button></div></div>').prependTo(sidebarSel);
                $(sidebarSel + ' [data-toggle="sidebar-layer-hide"]').click(function () {
                    sidebarObj.hide();
                });
            };
            // 设置内容
            sidebarObj.setInner = function (val) {
                $('<div class="sidebar-inner">'+val+'</div>').appendTo(sidebarSel);
            };
            // 设置宽度
            sidebarObj.setWidth = function (width) {
                $(sidebarSel).css('width', width + 'px');
            };
            // 显示
            sidebarObj.show = function () {
                if (sidebarObj.isShow())
                    throw ('侧边栏层：' + key + ' 已显示');

                if (!$(layerSel).hasClass('show'))
                    $(layerSel).addClass('show');

                $(sidebarSel)
                    .css('transform', 'translate(' + ($(sidebarSel).width() + 10) + 'px, 0px)')
                    .addClass('show');

                $('body').css('overflow', 'hidden');

                if ($('.sidebar-layer > .sidebar-block.show').length !== 0) {
                    // 变为标签内最后一个元素，显示置顶
                    $(sidebarSel).insertAfter($('.sidebar-layer > .sidebar-block.show:last-child'));
                }

                // 若点按的元素非 block 内元素
                setTimeout(function () {
                    $(document).bind('click.sidebar-layer-' + key, function (e) {
                        if(!$(e.target).is(sidebarSel) && !$(e.target).closest(sidebarSel).length) {
                            sidebarObj.hide();
                        }
                    });
                }, 20);
            };
            // 隐藏
            sidebarObj.hide = function () {
                if (!sidebarObj.isShow())
                    throw ('侧边栏层：' + key + ' 未显示');

                $(sidebarSel).removeClass('show');

                if ($('.sidebar-layer > .sidebar-block.show').length === 0) {
                    // 若已经没有显示层
                    $(layerSel).removeClass('show');
                    $('body').css('overflow', '');
                }

                $(document).unbind('click.sidebar-layer-' + key); // 解绑事件
            };
            sidebarObj.toggle = function () {
                if (!sidebarObj.isShow())
                    sidebarObj.show();
                else
                    sidebarObj.hide();
            };
            // 是否显示
            sidebarObj.isShow = function () {
                return $(layerSel).hasClass('show') && $(sidebarSel).hasClass('show');
            };
            // 获取 Selector
            sidebarObj.getSel = function () {
                return sidebarSel;
            };

            // 加入 List
            this.list[key] = sidebarObj;

            return sidebarObj;
        },
        // 获取 sidebarObj
        get: function (key) {
            if (!this.list.hasOwnProperty(key))
                return null;

            return this.list[key];
        },
        // 获取层 Selector
        getLayerSel: function () {
            var layerSel = '.sidebar-layer';

            if ($(layerSel).length === 0)
                $('<div class="sidebar-layer" />').appendTo('body');

            return layerSel;
        }
    },
    card: {
        layerList: {

        },
        newLayer: function (key) {

        },
        removeLayer: function (key) {

        }
    }
};

/**
 * 浏览器下载管理器
 */
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
        var panelObj = AppNavbar.panel.register(this.panelKey, 'downloadManager');
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

        CrDownloadsCallBack.downloadingTaskAction(key, action);
    },
    // 任务从列表移除
    taskRemove: function (key) {
        if (!this.data.list[key])
            throw ('任务从列表移除失败，或许已被删除，未找到 ' + key);

        if (this.isTaskInProgress(key)) {
            CrDownloadsCallBack.downloadTaskAction(key, this.actionList.cancel);
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

        CrDownloadsCallBack.fileLaunch(this.data.list[key].fullPath).then(function(isSuccess){
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

        CrDownloadsCallBack.urlOpenInDefaultBrowser(this.data.list[key].downloadUrl);
    },
    // 文件在资源管理器中显示
    fileShowInExplorer: function (key) {
        if (!this.data.list[key])
            return;

        CrDownloadsCallBack.fileShowInExplorer(this.data.list[key].fullPath).then(function(isSuccess){
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