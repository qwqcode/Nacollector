/**
 * Created by Zneiat on 2017/7/15.
 * https://github.com/Zneiat/Nacollector
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
    // 任务生成器初始化
    TaskGen.init();
    // 任务管理器层初始化
    Task.taskManagerLayer.init();
    // 点击操作按钮列表第一个
    $(TaskGen.sel.formToggleBtns+' a:nth-child(1)').click();
    // 下载面板初始化
    downloads.init();
    // 设置初始化
    setting.init();
    // 设置程序当前版本号
    AppAction.getVersion().then(function (version) {
        AppAction.version = version;
        // 检测更新
        AppUpdate.check(true);
    });
    // 开发者工具显示方式
    $(document).keydown(function (e) {
        if (e.altKey && event.keyCode === 123) {
            AppAction.showDevTools();
        }
    });
});

/**
 * functions in .cs
 * @type {{getVersion, _utilsReqIeProxy, logFileClear, appUpdateAction}}
 */
window.AppAction = AppAction || {};
window.AppAction.utilsReqIeProxy = function (isEnable) {
    if (isEnable === undefined)
        isEnable = !!setting.get('UtilsReqIeProxy');

    setting.set('UtilsReqIeProxy', isEnable);
    AppAction._utilsReqIeProxy(isEnable);
};

/**
 * Selectors
 */
const WRAP_SEL = '.wrap';

/**
 * 导航栏
 */
window.AppNavbar = {
    sel: {
        nav: '.top-nav-bar',
        navTitle: '.top-nav-bar .nav-title'
    },
    // 初始化 Navbar
    init: function () {
        $('<div class="left-items"><div class="nav-title"></div></div><div class="right-items"><div class="nav-btns"></div></div>').appendTo(this.sel.nav);
        // 导航栏操作按钮
        AppNavbar.btn.groupAdd('main-btns', {
            taskManager: {
                icon: 'assignment',
                title: '任务列表',
                onClick: function () {
                    Task.taskManagerLayer.toggleLayer();
                }
            },
            downloadManager: {
                icon: 'download',
                title: '下载列表'
            },
            setting: {
                icon: 'settings',
                title: '设置',
                onClick: function () {
                    setting.getSidebar().toggle();
                }
            }
        });

        AppNavbar.btn.groupAdd('task-runtime', {
            backToTaskGen: {
                icon: 'chevron-left',
                title: '返回任务生成器',
                onClick: function () {
                    Task.hide();
                }
            },
            removeTask: {
                icon: 'close',
                title: '删除任务',
                onClick: function () {
                    Task.getCurrent().remove();
                }
            },
            showTaskInfo: {
                icon: 'info',
                title: '任务详情',
                onClick: function () {
                    if (!Task.getCurrent())
                        return;

                    Task.getCurrent().showInfo();
                }
            }
        }).setMostLeft().hide();
    },
    // 标题设置
    setTitle: function (value, base64) {
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
    getTitle: function () {
        return $(this.sel.navTitle).text();
    }
};


/**
 * 导航栏按钮
 */
window.AppNavbar.btn = {
    sel: {
        navBtns: '.top-nav-bar .nav-btns'
    },
    groupList: {},
    // 按钮批量添加
    groupAdd: function (groupName, btnList) {
        var btnGroup, btnGroupDom;
        if (!this.getGroup(groupName)) {
            // 创建新的按钮组对象
            btnGroup = {};
            btnGroupDom = $('<div class="btn-group" data-nav-btn-group="' + groupName + '"></div>').appendTo(this.sel.navBtns);
            // 设置按钮组显示在最左
            btnGroup.setMostLeft = function () {
                btnGroupDom.insertBefore($(AppNavbar.btn.sel.navBtns+' .btn-group:first-child'));
                return btnGroup;
            };
            // 设置按钮组显示在最右
            btnGroup.setMostRight = function () {
                btnGroupDom.insertAfter($(AppNavbar.btn.sel.navBtns+' .btn-group:last-child'));
                return btnGroup;
            };
            // 隐藏按钮组
            btnGroup.hide = function () {
                btnGroupDom.hide();
                return btnGroup;
            };
            // 获取 Dom
            btnGroup.getDom = function () {
                return btnGroupDom;
            };
            // 显示
            btnGroup.show = function () {
                btnGroup.getDom().show();
                return btnGroup;
            };
            // 隐藏
            btnGroup.hide = function () {
                btnGroup.getDom().hide();
                return btnGroup;
            };
            // 添加图标
            btnGroup.btnList = {};
            btnGroup.addBtn = function (btnName, btnObj) {
                btnGroup.btnList[btnName] = btnObj;
                return btnObj;
            };
            btnGroup.getBtn = function (btnName) {
                if (!btnGroup.btnList[btnName]) return null;
                return btnGroup.btnList[btnName];
            };
            this.groupList[groupName] = btnGroup;
        } else {
            btnGroup = this.getGroup(groupName);
            btnGroupDom = btnGroup.getDom();
        }

        // 遍历在按钮组中添加每一个按钮
        $.each(btnList, function (btnName, value) {
            var dom = $('<a data-nav-btn="'+groupName+'.'+btnName+'" data-placement="bottom" title="'+value['title']+'"><i class="zmdi zmdi-'+value['icon']+'"></i></a>');
            if (!!value['onClick']) dom.click(value['onClick']);
            dom.appendTo(btnGroupDom);
            dom.tooltip();
            var btnObj = {};
            btnObj.showBadge = function () {
                dom.addClass('show-top-badge');
                return btnObj;
            };
            btnObj.hideBadge = function () {
                dom.removeClass('show-top-badge');
                return btnObj;
            };
            btnObj.getDom = function () {
                return dom;
            };
            btnGroup.addBtn(btnName, btnObj);
        });

        return btnGroup;
    },
    // 获取按钮组
    getGroup: function (groupName) {
        if (!this.groupList.hasOwnProperty(groupName)) return null;
        return this.groupList[groupName];
    },
    // 获取 按钮组 / 按钮 对象
    get: function (name) {
        name = name.split('.');
        if (!!name[0] && !!name[1])
            return this.getGroup(name[0]).getBtn(name[1]);
        if (!!name[0] && !name[1])
            return this.getGroup(name[0]);
        return null;
    }
};

/**
 * 任务选择列表
 */
(function () {
    // 表单控件
    var Form = {
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
    };
    // 操作列表
    window.SpiderList = {};
    window.SpiderList.Business = { _NamespaceLabel: "电商" };
    window.SpiderList.Business.CollItemDescImg = {
        label: "商品详情页图片解析",
        genForm: function () {
            Form.textInput('PageUrl', '详情页链接', '', inputValidators.isUrl);
            Form.selectInput('PageType', '链接类型', {
                "Tmall": "天猫",
                "Taobao": "淘宝",
                "Alibaba": "阿里巴巴",
                "Suning": "苏宁易购",
                "Gome": "国美在线"
            });
            Form.selectInput('ImgType', '图片类型', {
                "Thumb": "主图",
                "Category": "分类图",
                "Desc": "详情图"
            });
            Form.selectInput('CollType', '采集模式', {
                "collImgSrcUrl": "显示图片链接",
                "collDownloadImgSrc": "显示图片链接 并 下载打包保存"
            });
        }
    };
    window.SpiderList.Business.TaobaoSellerColl = {
        label: "淘宝店铺搜索卖家ID名采集",
        genForm: function () {
            Form.textInput('PageUrl', '店铺搜索页链接', '', inputValidators.isUrl);
            Form.numberInput('CollBeginPage', '采集开始页码', 1, 1);
            Form.numberInput('CollEndPage', '采集结束页码', undefined, 1);
            Form.selectInput('IgnoreTmall', '忽略天猫卖家', {
                "on": "开启",
                "off": "关闭"
            });
        }
    };
    window.SpiderList.Business.TmallGxptInvite = {
        label: "天猫供销平台分销商一键邀请",
        genForm: function () {
            Form.textareaInput('SellerId', '分销商ID名（一行一个）', undefined, 250);
        }
    };
    window.SpiderList.Business.TmallGxptInviteDelete = {
        label: "天猫供销平台分销商一键撤回",
        genForm: function () {
            Form.numberInput('DeleteBeginPage', '撤回开始页码', 1, 1);
            Form.numberInput('DeleteEndPage', '撤回结束页码', undefined, 1);
        }
    };
    window.SpiderList.Picture = { _NamespaceLabel: "图片" };
    window.SpiderList.Picture.Test = {
        label: "开发测试 DEBUG...",
        genForm: function () {}
    };
    window.SpiderList.Debug = { _NamespaceLabel: "调试" };
    window.SpiderList.Debug.Default = {
        label: "Default",
        genForm: function () {}
    };
})();

/**
 * Task Generator (任务生成器)
 */
window.TaskGen = {
    sel: {
        form: '.taskgen-form',
        formToggle: '.taskgen-form-toggle',
        formToggleDropdown: '.taskgen-form-toggle .namespace-dropdown',
        formToggleBtns: '.taskgen-form-toggle .classname-btns'
    },
    // 当前
    current: {
        typeName: null,
        inputs: {}
    },
    // 初始化
    init: function () {
        // 遍历列表 生成按钮
        var dropdownDom = $('<div class="namespace-dropdown"><div class="dropdown-selected"></div><ul class="dropdown-option anim-fade-in"></ul></div>'),
            dropdownSelectedDom = dropdownDom.find('.dropdown-selected'),
            dropdownOptionDom = dropdownDom.find('.dropdown-option');
        var btnsDom = $('<div class="classname-btns"></div>');

        dropdownDom.appendTo(this.sel.formToggle);
        btnsDom.appendTo(this.sel.formToggle);

        var dropdownOptionShow = function () {
            dropdownOptionDom.addClass('show');
            // 若点击其他地方
            setTimeout(function () {
                $(document).bind('click.dropdown-option', function (e) {
                    if(!$(e.target).is('.dropdown-option') && !$(e.target).closest('.dropdown-option').length) {
                        dropdownOptionHide();
                    }
                });
            }, 20);
        };
        var dropdownOptionHide = function () {
            $(document).unbind('click.dropdown-option');
            dropdownOptionDom.removeClass('show');
        };
        dropdownSelectedDom.click(function () {
            dropdownOptionShow();
        });

        $.each(SpiderList, function (namespace, eachClass) {
            var li = $('<li data-namespace="'+namespace+'">'+eachClass._NamespaceLabel+'</li>');
            // 点击 li
            li.click(function () {
                // 按钮显示
                btnsDom.html(''); // 删除原有的所有按钮
                $.each(eachClass, function (classname, classInfo) {
                    if (classname.substr(0, 1) === '_') return;
                    var typeName = namespace + '.' + classname;
                    var btn = $('<a>' + classInfo['label'] + '</a>').appendTo(btnsDom);
                    // 选中之前点击过的按钮
                    if (!!TaskGen.current.typeName && TaskGen.current.typeName === typeName) {
                        btnsDom.find('a').removeClass('active');
                        $(btn).addClass('active');
                    }
                    btn.click(function () {
                        // 表单生成
                        TaskGen.formLoad(typeName);
                        // 按钮选中
                        btnsDom.find('a').removeClass('active');
                        $(this).addClass('active');
                    });
                });
                dropdownSelectedDom.text($(this).text());
                dropdownSelectedDom.attr('data-namespace', namespace);
                // 选中当前 li
                dropdownOptionDom.find('li').removeClass('selected');
                $(this).addClass('selected');
                // 取消显示 dropdown-option
                dropdownOptionHide();
                // 当前 li 置顶
                $(this).insertBefore(dropdownOptionDom.find('li:first-child'));
            });
            li.appendTo(dropdownOptionDom);
        });

        // 打开第一个任务生成器
        dropdownOptionDom.find('li:first-child').click();
        btnsDom.find('a:first-child').click();
    },
    // 分析 TypeName
    spiderListGet: function (typeNameStr) {
        var typeName = typeNameStr.split('.') || null;
        if (!typeName || !typeName[0] || !typeName[1]) return null;
        var namespace = typeName[0],
            classname = typeName[1];
        if (!SpiderList.hasOwnProperty(namespace) || !SpiderList[namespace].hasOwnProperty(classname)) return null;
        return SpiderList[namespace][classname];
    },
    // 表单装载
    formLoad: function (typeName) {
        // 点击操作按钮事件
        if (!this.spiderListGet(typeName))
            throw ('SpiderList 中没有 ' + typeName + '，无法创建表单！');

        var spider = this.spiderListGet(typeName);
        var formDom = $(this.sel.form);

        // 清除当前表单
        formDom.html('');
        // 清除当前数据
        this.current.typeName = null;
        this.current.inputs = {};

        // 装入新数据
        this.current.typeName = typeName;
        // 执行表单创建
        spider.genForm();
        // 提交按钮
        var submitBtn = $('<div class="form-btns">\n<button class="submit-btn" type="submit">执行任务</button>\n</div>')
            .appendTo(formDom);

        submitBtn.click(function () {
            if (!TaskGen.formCheck())
                return false;

            Task.createTask(typeName, spider.label, formDom.serializeArray());

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
    }
};

window.Task = {
    sel: {
        runtime: '.task-runtime'
    },
    // 任务列表
    list: {},
    // 当前显示的任务ID
    currentDisplayedId: null,
    // 创建任务
    createTask: function (typeName, classLabel, parmsObj) {
        var taskId = new Date().getTime().toString();
        var runtimeSel = this.sel.runtime;
        // 创建元素
        $('<div class="task-item" data-task-id="'+taskId+'" style="display: none">\n<div class="container" style="width: 95%;">\n<div class="task-log-table"></div>\n</div>').appendTo(runtimeSel);
        var taskItemSel = '[data-task-id="'+taskId+'"]';
        var taskLogTableSel = taskItemSel + ' .task-log-table';
        // 工厂模式
        var taskObj = {};
        // 获取任务ID
        taskObj.getId = function () {
            return taskId;
        };
        // 获取任务调用类名
        taskObj.getTypeName = function () {
            return typeName;
        };
        // 获取任务调用类标签
        taskObj.getClassLabel = function () {
            return classLabel;
        };
        // 获取任务参数对象
        taskObj.getParmsObj = function () {
            return parmsObj;
        };
        // 设置标题
        taskObj.setTitle = function () {
            AppNavbar.setTitle(taskObj.getTitle());
        };
        // 获取标题
        taskObj.getTitle = function () {
            return classLabel + ' 任务ID：' + taskId;
        };
        // 恢复成原来的标题
        taskObj.setOriginalTitle = function () {
            AppNavbar.setTitle('');
        };
        // 显示
        taskObj.show = function () {
            Task.show(taskId);
        };
        // 隐藏
        taskObj.hide = function () {
            Task.hide();
        };
        // 显示任务信息
        taskObj.showInfo = function () {
            AppLayer.dialog.open('任务信息', 'ID：'+taskId+'<br>标题：'+taskObj.getTitle()+'<br><br>调用类标签：'+taskObj.getClassLabel()+'<br>调用类名：'+taskObj.getTypeName()+'<br><br>执行开始时间：'+new Date(parseInt(taskId))+'<br><br>参数：'+JSON.stringify(parmsObj)+'');
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
                return str.replace(/\n/g, '<br/>');
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
            // 功能有待优化，当终端快速显示日志时有问题
            /*if (!taskObj.allowAutoScrollToBottom)
                return; // throw ('不允许自动滚动到底部啦');*/

            $(runtimeSel).scrollTop($(runtimeSel)[0].scrollHeight);
        };
        // 删除
        taskObj.remove = function () {
            if (taskObj.getIsInProgress()) {
                AppLayer.dialog.open('删除任务', '任务 “'+taskObj.getTitle()+'” 正在执行中...',
                    ['中止并删除任务', function () {
                        TaskController.abortTask(taskId).then(function (isSuccess) {
                            if (isSuccess) {
                                taskObj._remove();
                            } else {
                                AppLayer.notify.error('任务中止失败');
                            }
                        });
                    }],
                    ['取消', function () {}]);
            } else {
                taskObj._remove();
            }
        };
        taskObj._remove = function () {
            if (!!Task.getCurrent() && Task.getCurrent().getId() === taskId) {
                taskObj.hide();
            }
            // 对象删掉！
            delete Task.list[taskId];
            // 任务管理器删除项目
            Task.taskManagerLayer.removeItem(taskId);
            // 提示
            AppLayer.notify.success('任务删除成功');
        };
        // 任务是否正在进行中
        taskObj.isInProgress = true;
        // 设置任务已结束状态
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

        // 让任务控制器 开始一个执行新任务
        TaskController.createTask(taskId, typeName, classLabel, JSON.stringify(parmsObj)).then(function(callback){
            taskObj.show();
            Task.taskManagerLayer.addItem(taskId);
        });

        return taskObj;
    },
    // 获取任务
    get: function (taskId) {
        if (!this.list.hasOwnProperty(taskId))
            return null;

        return this.list[taskId];
    },
    // 获取当前显示任务
    getCurrent: function () {
        if (!this.get(this.currentDisplayedId))
            return null;

        return this.get(this.currentDisplayedId);
    },
    // 显示指定任务
    show: function (taskId) {
        if (!this.get(taskId))
            throw ('未找到任务 '+taskId);

        if (this.getCurrent() !== null)
            this.hide();

        var taskObj = this.get(taskId);
        var runtimeSel = this.sel.runtime;
        var taskItemSel = taskObj.getSel();

        $(taskItemSel).show();
        $(runtimeSel).show();

        $(runtimeSel).on('scroll', function () {
            taskObj.allowAutoScrollToBottom = false;

            var documentheight = $(taskObj.getSel() + ' > .container').innerHeight(),
                totalheight = $(runtimeSel).height() + $(runtimeSel).scrollTop();

            if (documentheight === totalheight) {
                taskObj.allowAutoScrollToBottom = true;
            }
        });

        taskObj.setTitle();

        // 显示导航栏控制按钮组
        AppNavbar.btn.get('task-runtime').show();

        this.currentDisplayedId = taskId;
    },
    // 隐藏
    hide: function () {
        if (this.currentDisplayedId === null)
            throw ('未显示任何任务');

        var runtimeSel = this.sel.runtime;
        var taskItemSel = Task.getCurrent().getSel();

        $(runtimeSel).hide();
        $(runtimeSel).off('scroll');
        $(taskItemSel).hide();

        this.getCurrent().setOriginalTitle();

        // 隐藏导航栏控制按钮组
        AppNavbar.btn.get('task-runtime').hide();

        this.currentDisplayedId = null;
    },
    // 日志
    log: function (taskId, text, level, timeStamp, textIsBase64) {
        if (!this.get(taskId))
            throw ('未找到任务 '+taskId);

        if (typeof textIsBase64 === "boolean" && textIsBase64 === true)
            text = Base64.decode(text);

        this.get(taskId).log(text, level);
    },
    // 任务管理器层
    taskManagerLayer: {
        init: function () {
            var taskManager = AppLayer.sidebar.register('taskManager');
            taskManager.setTitle('任务列表', '#4265c7');
            taskManager.setWidth(450);
            taskManager.setInner('<div class="task-manager"></div>');
        },
        getItemSel: function (taskId) {
            return '[data-taskmanager-taskid="'+taskId+'"]';
        },
        addItem: function (taskId) {
            if (!Task.get(taskId))
                throw ('未找到此任务 '+taskId);

            var task = Task.get(taskId);
            var taskItem = $('<div class="task-item" data-taskmanager-taskid="'+taskId+'">\n<div class="left">\n<i class="zmdi zmdi-view-carousel" data-toggle="task-show"></i>\n</div>\n<div class="right">\n<h2 class="task-title" data-toggle="task-show">'+task.getClassLabel()+'</h2>\n<p class="task-desc"><span class="task-id">任务ID：'+taskId+'</span></p>\n<div class="action-bar">\n<a class="action-btn" data-toggle="task-show"><i class="zmdi zmdi-layers"></i> 显示</a>\n<a class="action-btn" data-toggle="task-remove"><i class="zmdi zmdi-close"></i> 删除</a>\n</div>\n</div>\n</div>');
            taskItem.find('[data-toggle="task-show"]').click(function () {
                Task.show(taskId);
            });
            taskItem.find('[data-toggle="task-remove"]').click(function () {
                Task.get(taskId).remove();
            });
            taskItem.prependTo(this.getLayer().getSel() + ' .task-manager');
        },
        removeItem: function (taskId) {
            if ($(this.getItemSel(taskId)).length === 0)
                throw ('未找到此任务 '+taskId);

            setTimeout(function () {
                $(Task.taskManagerLayer.getItemSel(taskId)).remove();
            }, 20);
        },
        toggleLayer: function () {
            this.getLayer().toggle();
        },
        getLayer: function () {
            return AppLayer.sidebar.get('taskManager');
        }
    }
};

/**
 * 小部件
 */
window.AppWidget =  {
    loadingIndicator: function (putInto) {
        $('<div class="loading-indicator" style="opacity: .9;"><div class="inner"><svg viewBox="25 25 50 50"><circle cx="50" cy="50" r="20" fill="none" stroke-width="2" stroke-miterlimit="10"></circle></svg></div></div>').prependTo(putInto);

        var indicatorObj = {};
        indicatorObj.remove = function () {
            $(putInto).find('.loading-indicator').remove();
        };

        return indicatorObj;
    },
    floatImg: function (parent, imgSrc) {
        if ($('body .widget-float-img').length !== 0)
            return;

        var parentDom = $(parent);
        var parentPos = $.getPosition($(parent));

        setTimeout(function () {
            if ($(":hover").filter(parentDom).length === 0)
                return;

            var left = parentPos['left'];
            var top = parentPos['top'];
            if (parentPos['top'] >= ($(WRAP_SEL).height() - parentPos['bottom'])) {
                // Floater 显示在父元素之上
                top = top-250 -10;
            } else {
                // Floater 显示在父元素之下
                top = top+parentDom.height() +10;
            }

            var floaterDom = $('<div class="widget-float-img anim-fade-in" style="left: '+left+'px; top: '+top+'px;"></div>').appendTo('body');

            var loadingIndicator = AppWidget.loadingIndicator(floaterDom);

            var imgDom = $('<img src="'+imgSrc+'" class="anim-fade-in" style="display: none;">').appendTo(floaterDom);
            imgDom.load(function () {
                loadingIndicator.remove();
                imgDom.show();
            });

            parentDom.on('mouseout', function(e){
                floaterDom.remove();
            });
        }, 200);
    }
};

/**
 * 内容层
 */
window.AppLayer = {

};

/**
 * 内容层 侧边栏
 */
window.AppLayer.sidebar = {
    list: {},
    // 当前显示
    currentDisplayedKey: null,
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
        sidebarObj.setTitle = function (val, titleBg) {
            var header = $('<div class="sidebar-header"><div class="header-left">'+val+'</div><div class="header-right"><button type="button" data-toggle="sidebar-layer-hide"><i class="zmdi zmdi-close"></i></button></div></div>');
            if (!!titleBg) header.css('background', titleBg);
            header.prependTo(sidebarSel);
            $(sidebarSel + ' [data-toggle="sidebar-layer-hide"]').click(function () {
                sidebarObj.hide();
            });
        };
        // 设置内容
        sidebarObj.setInner = function (val) {
            $('<div class="sidebar-inner">'+val+'</div>').appendTo(sidebarSel);
        };
        sidebarObj.width = 360;
        // 设置宽度
        sidebarObj.setWidth = function (width) {
            if (!!width && !isNaN(parseInt(width)))
                sidebarObj.width = parseInt(width);

            $(sidebarSel).css('width', sidebarObj.width + 'px').css('transform', 'translate('+sidebarObj.width+'px, 0px)');
        };
        // 显示
        sidebarObj.show = function () {
            if (AppLayer.sidebar.currentDisplayedKey !== null && AppLayer.sidebar.currentDisplayedKey !== key) {
                AppLayer.sidebar.get(AppLayer.sidebar.currentDisplayedKey).hide();
            }

            if (AppLayer.sidebar.currentDisplayedKey === key)
                throw ('侧边栏层：' + key + ' 已显示');

            if (!$(layerSel).hasClass('show'))
                $(layerSel).addClass('show');

            // 设置宽度
            sidebarObj.setWidth();

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

            AppLayer.sidebar.currentDisplayedKey = key;
        };
        // 隐藏
        sidebarObj.hide = function () {
            if (AppLayer.sidebar.currentDisplayedKey === null || AppLayer.sidebar.currentDisplayedKey !== key)
                throw ('侧边栏层：' + key + ' 未显示');

            $(sidebarSel).removeClass('show');

            if ($('.sidebar-layer > .sidebar-block.show').length === 0) {
                // 若已经没有显示层
                $(layerSel).removeClass('show');
                $('body').css('overflow', '');
            }

            $(document).unbind('click.sidebar-layer-' + key); // 解绑事件

            AppLayer.sidebar.currentDisplayedKey = null;
        };
        // 显隐切换
        sidebarObj.toggle = function () {
            if (!sidebarObj.isShow()) {
                sidebarObj.show();
            } else {
                sidebarObj.hide();
            }
        };
        // 是否显示
        sidebarObj.isShow = function () {
            return $(layerSel).hasClass('show') && $(sidebarSel).hasClass('show');
        };
        // 获取 Selector
        sidebarObj.getSel = function () {
            return sidebarSel;
        };
        // 获取 Inner Selector
        sidebarObj.getInnerSel = function () {
            if ($(sidebarObj.getSel()+' .sidebar-inner').length === 0)
                sidebarObj.setInner('');

            return sidebarObj.getSel()+' .sidebar-inner';
        };
        // 获取 InnerDom
        sidebarObj.getInnerDom = function () {
            if ($(sidebarObj.getSel()+' .sidebar-inner').length === 0)
                sidebarObj.setInner('');

            return $(sidebarObj.getSel()+' .sidebar-inner');
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
};

/**
 * 内容层 对话框
 */
window.AppLayer.dialog = {
    sel: {
        dialogLayer: '.dialog-layer'
    },
    open: function (title, content, yesBtn, cancelBtn) {
        var layerSel = this.sel.dialogLayer;

        if ($(layerSel).length !== 0)
            $(layerSel).remove();

        var dialogLayerDom = $('<div class="dialog-layer anim-fade-in" />').appendTo('body');
        var dialogLayerHide = function () {
            dialogLayerDom.addClass('anim-fade-out');
            setTimeout(function () {
                dialogLayerDom.hide();
            }, 200);
        };

        var dialogDom = $('<div class="dialog-inner"><div class="dialog-title">'+title+'</div>\n<div class="dialog-content">'+content+'</div></div>').appendTo(dialogLayerDom);

        // 底部按钮
        if (!!yesBtn || !!cancelBtn) {
            var dialogBottomDom = $('<div class="dialog-bottom"></div>')
                .appendTo(dialogDom);

            // 确定按钮
            if (!!yesBtn) {
                var yesOnClick = yesBtn[1] || function () {};
                var yesBtnText = yesBtn[0] || '确定';

                $('<a class="dialog-btn yes-btn">' + yesBtnText + '</a>').click(function () {
                    dialogLayerHide();
                    yesOnClick();
                }).appendTo(dialogBottomDom);
            }

            // 取消按钮
            if (!!cancelBtn) {
                var cancelBtnText = cancelBtn[0] || '取消';
                var cancelOnClick = cancelBtn[1] || function () {};

                $('<a class="dialog-btn cancel-btn">' + cancelBtnText + '</a>').click(function () {
                    dialogLayerHide();
                    cancelOnClick();
                }).appendTo(dialogBottomDom);
            }
        } else {
            $('<a class="right-btn"><i class="zmdi zmdi-close"></i></a>').appendTo($(dialogDom).find('.dialog-title')).click(function () {
                dialogLayerHide();
            });
        }
    }
};

/**
 * 内容层 通知
 */
window.AppLayer.notify = {
    sel: {
        notifyLayer: '.notify-layer'
    },
    success: function (message) {
        this.show(message, 's');
    },
    error: function (message) {
        this.show(message, 'e');
    },
    // level: s, e
    show: function (message, level, timeout) {
        timeout = (timeout !== undefined && typeof timeout === 'number') ? timeout : 2000;

        var layerDom = $(this.sel.notifyLayer);
        if (layerDom.length === 0)
            layerDom = $('<div class="notify-layer" />').appendTo('body');

        var notifyDom = $('<div class="notify-item anim-fade-in '+(!!level ? 'type-'+level : '')+'"><p class="notify-content">'+message+'</p></div>').prependTo(layerDom);

        var notifyRemove = function () {
            notifyDom.addClass('anim-fade-out');
            setTimeout(function () {
                notifyDom.remove();
            }, 200);
        };

        var autoOut = true;
        notifyDom.click(function () {
            notifyRemove();
            autoOut = false;
        });

        if (timeout > 0) {
            setTimeout(function () {
                if (!autoOut) return;
                notifyRemove();
            }, timeout);
        }
    }
};

/**
 * 导航栏 面板
 */
AppNavbar.panel = {
    list: {},
    // 注册新面板
    register: function (key, btnName) {
        if (this.list.hasOwnProperty(key))
            return '导航栏面板： ' + key + ' 已存在于list中';

        var btnDom = AppNavbar.btn.get(btnName).getDom();
        btnDom.after('<div class="navbar-panel anim-fade-in" data-navbar-panel="'+key+'" />');

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
            var position = $.getPosition(btnDom);
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
            // 导航栏按钮隐藏通知小红点
            AppNavbar.btn.get(btnName).hideBadge();
        };
        // 隐藏
        panelObj.hide = function () {
            if (!panelObj.isShow())
                throw ('导航栏面板：' + key + ' 未显示');

            $(window).unbind('resize.nav-panel-' + key);
            $(document).unbind('click.nav-panel-' + key); // 解绑事件

            $(panelSel).removeClass('show');
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
        btnDom.bind('click', function () {
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
};

/**
 * 浏览器下载管理器
 */
window.downloads = {
    navbarBtnName: 'main-btns.downloadManager',
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
    localStorageConf: {
        key: 'downloads'
    },
    // 初始化
    init: function () {
        var panelObj = AppNavbar.panel.register(this.panelKey, this.navbarBtnName);
        panelObj.setTitle('<i class="zmdi zmdi-download"></i> 下载列表');
        panelObj.setInner('<div class="downloads-list"></div>');
        panelObj.setSize(400, 430);
        this.sel.downloadsList = panelObj.getSel() + ' .downloads-list';
        // 读取 localStorage 恢复下载列表
        this.restoreDataList();
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

        // 导航栏按钮显示通知小红点
        AppNavbar.btn.get(this.navbarBtnName).showBadge();
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
        this.storeDataList(); // 存储下载列表
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

        this.storeDataList(); // 存储下载列表
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
    // localStorage 恢复下载列表
    restoreDataList: function () {
        var data = localStorage.getItem(this.localStorageConf.key);
        if (data === null) return;

        var downloadsListObj = JSON.parse(data);
        this.data.list = {};

        // console.log(JSON.stringify(downloadsListJson));
        for (var key in downloadsListObj) {
            if (!downloadsListObj.hasOwnProperty(key))
                continue;

            this.data.list[key] = downloadsListObj[key];
            if (this.isTaskInProgress(key))
                this.data.list[key].status = this.statusList.cancelled;

            this.updateItemUi(key);
        }
    },
    // localStorage 储存下载列表
    storeDataList: function () {
        localStorage.setItem(this.localStorageConf.key, JSON.stringify(this.data.list));
    },
    // 清空下载列表
    removeDataList: function () {
        // 将正在执行的下载任务取消
        for (var key in this.data.list) {
            if (this.isTaskInProgress(key))
                this.taskAction(key, this.actionList.cancel);
        }

        this.data.list = {};
        $(this.sel.downloadsList).find('.download-item').remove();
        localStorage.setItem(this.localStorageConf.key, null);

        // 导航栏按钮隐藏通知小红点
        AppNavbar.btn.get(this.navbarBtnName).hideBadge();
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

// 设置
window.setting = {
    init: function () {
        AppAction.utilsReqIeProxy(); // 无参数代表同步
        var settingSidebar = AppLayer.sidebar.register(this.sidebarKey);
        settingSidebar.setTitle('设置', '#0089ff');
        settingSidebar.setWidth(360);
        this.setSidebarInner(settingSidebar.getInnerDom());
    },
    get: function (key) {
        var settingValue = JSON.parse(localStorage.getItem('setting')) || {};
        return settingValue.hasOwnProperty(key) ? settingValue[key] : null;
    },
    set: function (key, val) {
        var settingValue = JSON.parse(localStorage.getItem('setting')) || {};
        settingValue[key] = val;
        localStorage.setItem('setting', JSON.stringify(settingValue));
    },
    sidebarKey: 'setting',
    getSidebar: function () {
        return AppLayer.sidebar.get(this.sidebarKey);
    },
    // 设置侧边栏内容
    setSidebarInner: function (innerDom) {
        var settingDom = $('<div class="setting"></div>').appendTo(innerDom);

        var group = function (name, title) {
            return $('<div class="setting-group" data-setting-sidebar-group="'+name+'"><h2 class="setting-group-title">'+title+'</h2></div>').appendTo(settingDom);
        };
        var itemAt = function (groupDom) {
            var boxDom = $('<div class="setting-item"></div>').appendTo(groupDom);

            var innerElement = {};
            // 按钮
            innerElement.btnBlock = function (text, onClick) {
                return $('<button type="button" class="setting-btn-block">'+text+'</button>').click(onClick).appendTo(boxDom);
            };
            // 切换按钮
            innerElement.btnToggle = function (text, turnOnEvent, turnOffEvent) {
                var btnDom = $('<button type="button" class="setting-btn-block setting-btn-toggle"><div class="left-text">'+text+'</div><div class="toggle"><div class="toggle-bar"></div><div class="toggle-button"></div></div></button>');
                var toggleDom = btnDom.find('.toggle');
                var btnObj = {};
                btnObj.setVal = function (bool) {
                    if (typeof bool !== 'boolean') return;
                    if (bool) toggleDom.addClass('turn-on'); else toggleDom.removeClass('turn-on');
                };
                btnObj.setOn = function () {
                    btnObj.setVal(true);
                };
                btnObj.setOff = function () {
                    btnObj.setVal(false);
                };
                btnObj.toggle = function () {
                    if (!toggleDom.hasClass('turn-on')) { // On
                        toggleDom.addClass('turn-on');
                        turnOnEvent();
                    } else { // Off
                        toggleDom.removeClass('turn-on');
                        turnOffEvent();
                    }
                };
                btnObj.getDom = function () {
                    return btnDom;
                };
                btnDom.click(function () {
                    btnObj.toggle();
                }).appendTo(boxDom);
                return btnObj;
            };
            // 信息展示
            innerElement.infoShow = function (label, value) {
                return $('<div class="two-line"><span class="label">'+label+'</span><span class="value">'+value+'</span></div>').appendTo(boxDom);
            };

            return innerElement;
        };

        var groupDownloads = group('downloads', '下载内容');
        itemAt(groupDownloads).btnBlock('下载列表清空', function () {
            downloads.removeDataList();
            AppLayer.notify.success('下载列表已清空');
        });

        var groupNetwork = group('downloads', '网络配置');
        itemAt(groupNetwork).btnToggle('采集时使用IE代理进行请求', function () {
            AppAction.utilsReqIeProxy(true);
        }, function () {
            AppAction.utilsReqIeProxy(false);
        }).setVal(!!this.get('UtilsReqIeProxy'));

        var groupMaintenance = group('maintenance', '维护');
        itemAt(groupMaintenance).btnBlock('日志文件清理', function () {
            AppAction.logFileClear().then(function () {
                AppLayer.notify.success('日志文件已清理');
            });
        });
        var updateBtn = itemAt(groupMaintenance).btnBlock('检查更新', function () {
            updateBtn.text("正在检查更新...");
            AppUpdate.check(false, function () {
                updateBtn.text("检查更新");
            });
        });
        var groupAbout = group('about', '关于');
        var infoAppVersion = itemAt(groupAbout).infoShow('版本号', '').find('.value');
        AppAction.getVersion().then(function (version) {
            infoAppVersion.text(version);
        });
        itemAt(groupAbout).infoShow('作者', '<a href="https://github.com/Zneiat" target="_blank">ZNEIAT</a>');
        itemAt(groupAbout).infoShow('联系', '1149527164@qq.com');
        itemAt(groupAbout).infoShow('博客', '<a href="http://www.qwqaq.com" target="_blank">http://www.qwqaq.com</a>');
        itemAt(groupAbout).infoShow('GitHub', '<a href="https://github.com/Zneiat/Nacollector" target="_blank">Zneiat/Nacollector</a>');
        itemAt(groupAbout).infoShow('', '<a href="https://raw.githubusercontent.com/Zneiat/Nacollector/master/LICENSE" target="_blank">您使用 Nacollector 即视为您已阅读并同意本《Nacollector 用户使用许可协议》的约束</a>');
        itemAt(groupAbout).infoShow('', '<a href="https://github.com/Zneiat/Nacollector" target="_blank">Nacollector</a> Copyright (C) 2018 <a href="https://github.com/Zneiat" target="_blank">Zneiat</a>');
    }
};

// 升级检测
window.AppUpdate = {
    check: function (atDocumentReady, onFinish) {
        atDocumentReady = atDocumentReady || false;
        var ajaxOpt = {
            type: 'GET',
            url: AppConfig.updateCheckUrl,
            dataType: 'json',
            data: {'token': AppConfig.updateCheckToken},
            beforeSend: function() {}
        };
        ajaxOpt.success = function (json) {
            !!onFinish ? onFinish(json) : null;
            var UpdateVersion = json['latest'] || null;
            if (!!UpdateVersion && UpdateVersion !== AppAction.version) {
                // 有更新
                var UpdateLog = (!!json['updateLog'] && json['updateLog'].hasOwnProperty(UpdateVersion)) ? json['updateLog'][UpdateVersion] : '无说明';

                AppLayer.dialog.open('Nacollector 可更新至 ' + json['latest'] + ' 版本', UpdateLog,
                    ['现在更新', function () {
                        if (!json['updateRes'] || !json['updateRes'].hasOwnProperty(UpdateVersion)) {
                            AppLayer.notify.error('更新地址获取失败');
                            return;
                        }
                        var updateType = 'a';
                        if (!!json['updateType'] && json['updateType'].hasOwnProperty(UpdateVersion)) {
                            updateType = json['updateType'][UpdateVersion];
                        }
                        AppAction.appUpdateAction(json['updateRes'][UpdateVersion], updateType);
                    }],
                    ['以后再说', function () {}]);
            } else {
                if (!atDocumentReady) AppLayer.notify.success('暂无更新');
            }
        };
        ajaxOpt.error = function () {
            if (!atDocumentReady) AppLayer.notify.success('更新信息获取失败');
        };
        $.ajax(ajaxOpt);
    }
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