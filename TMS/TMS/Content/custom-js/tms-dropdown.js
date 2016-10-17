
function initUrgencyDropdown(param) {
    param.control.select2({
        ajax: {
            url: "/dropdown/loadurgencydropdown",
            dataType: "json",
            data: function (params) {
                var ajaxData = {
                    ignore: param.ignore()
                };
                if (param.data != undefined) {
                    var dat = param.data();
                    for (var i in dat) {
                        ajaxData[i] = dat[i];
                    }
                }
                return ajaxData;
            },
            processResults: function (data) {
                var result = {
                    results: []
                };
                if (param.allowAll) {
                    result.results.push({
                        allowAll: true,
                        id: "",
                        text: "All"
                    });
                }
                for (var i = 0; i < data.length; i++) {
                    data[i].id = data[i].Id;
                    data[i].text = data[i].Name;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        minimumResultsForSearch: Infinity,
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initPriorityDropdown(param) {
    param.control.select2({
        ajax: {
            url: "/dropdown/loadprioritydropdown",
            dataType: "json",
            data: function (params) {
                var ajaxData = {
                    ignore: param.ignore()
                };
                if (param.data != undefined) {
                    var dat = param.data();
                    for (var i in dat) {
                        ajaxData[i] = dat[i];
                    }
                }
                return ajaxData;
            },
            processResults: function (data) {
                var result = {
                    results: []
                };
                if (param.allowAll) {
                    result.results.push({
                        allowAll: true,
                        id: "",
                        text: "All"
                    });
                }

                for (var i = 0; i < data.length; i++) {
                    data[i].id = data[i].Id;
                    data[i].text = data[i].Name;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        minimumResultsForSearch: Infinity,
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initImpactDropdown(param) {
    param.control.select2({
        ajax: {
            url: "/dropdown/loadimpactdropdown",
            dataType: "json",
            data: function (params) {
                var ajaxData = {
                    ignore: param.ignore()
                };
                if (param.data != undefined) {
                    var dat = param.data();
                    for (var i in dat) {
                        ajaxData[i] = dat[i];
                    }
                }
                return ajaxData;
            },
            processResults: function (data) {
                var result = {
                    results: []
                };
                if (param.allowAll) {
                    result.results.push({
                        allowAll: true,
                        id: "",
                        text: "All"
                    });
                }
                for (var i = 0; i < data.length; i++) {
                    data[i].id = data[i].Id;
                    data[i].text = data[i].Name;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        minimumResultsForSearch: Infinity,
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        templateSelection: function (data) {
            return data.text;
        }
    });
}
function initCategoryDropdown(param) {
    var formatt = function (repo) {
        var markup = "";
        if (repo.allowAll) {
            markup = "<div class='technician-dropdown'>" +
                "<label class='category-name'>All</label>" +
                "</div>";

        } else {
            markup = "<div class='category-dropdown'>" +
                "<label class='category-" + repo.CategoryLevel + "'>" +
                repo.Name +
                "</label>" +
                "</div>";
        }
        return markup;
    }
    param.control.select2({
        ajax: {
            url: "/dropdown/loadcategorydropdown",
            dataType: "json",
            data: function (params) {
                var ajaxData = {
                    ignore: param.ignore()
                };
                if (param.data != undefined) {
                    var dat = param.data();
                    for (var i in dat) {
                        ajaxData[i] = dat[i];
                    }
                }
                return ajaxData;
            },
            processResults: function (data) {
                var result = {
                    results: []
                };
                if (param.allowAll) {
                    result.results.push({
                        allowAll: true,
                        id: "",
                        text: "All"
                    });
                }
                for (var i = 0; i < data.length; i++) {
                    data[i].id = data[i].Id;
                    data[i].text = data[i].Name;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        minimumResultsForSearch: Infinity,
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        templateResult: formatt, // omitted for brevity, see the source of this page
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initDepartmentDropdown(param) {
    param.control.select2({
        ajax: {
            url: "/dropdown/loaddepartmentdropdown",
            dataType: "json",
            data: function (params) {
                var ajaxData = {
                    ignore: param.ignore()
                };
                if (param.data != undefined) {
                    var dat = param.data();
                    for (var i in dat) {
                        ajaxData[i] = dat[i];
                    }
                }
                return ajaxData;
            },
            processResults: function (data) {
                var result = {
                    results: []
                };
                if (param.allowAll) {
                    result.results.push({
                        allowAll: true,
                        id: "",
                        text: "All"
                    });
                }
                for (var i = 0; i < data.length; i++) {
                    data[i].id = data[i].Id;
                    data[i].text = data[i].Name;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        minimumResultsForSearch: Infinity,
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initTechnicianDropdown(param) {
    var formatt = function (repo) {
        var markup = "";
        if (repo.allowAll) {
            markup = "<div class='technician-dropdown'>" +
                "<label class='technician-name'>All</label>" +
                "</div>";

        } else {
            markup = "<div class='technician-dropdown'>" +
                "<label class='technician-name'>" + repo.Name + "</label>" +
                "<label class='technician-email'>( " + repo.Email + " )</label>" +
                "</div>";
        }
        return markup;
    }
    param.control.select2({
        language: {
            // You can find all of the options in the language files provided in the
            // build. They all must be functions that return the string that should be
            // displayed.
            inputTooShort: function () {
                return "You must enter more characters...";
            }
        },
        ajax: {
            url: "/dropdown/loadtechniciandropdown",
            dataType: "json",
            data: function (params) {
                var ajaxData = {
                    query: params.term,
                    departmentId: param.departmentId
                };
                return ajaxData;
            },
            processResults: function (data) {
                var result = {
                    results: []
                };
                if (param.allowAll) {
                    result.results.push({
                        allowAll: true,
                        id: "",
                        text: "All"
                    });
                }
                for (var i = 0; i < data.length; i++) {
                    data[i].id = data[i].Id;
                    data[i].text = data[i].Name;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        escapeMarkup: function (markup) {
            return markup;
        }, // let our custom formatter work
        minimumInputLength: 0,
        templateResult: formatt, // omitted for brevity, see the source of this page
        templateSelection: function (data) {
            return data.Name;
        } // omitted for brevity, see the source of this page
    });
}

function initStudentDropdown(param) {
    var formatClass = function (repo) {
        if (repo.loading) {
            return "Đang tìm kiếm...";
        }
        var markup = $("<div>", {
            "class": "student-container",
            html: [
                    $("<div>", {
                        "class": "avartar-container",
                        html: $("<img>", {
                            "src": repo.ImageUrl
                        })
                    }),
                    $("<label>", {
                        "class": "student-name",
                        "html": repo.text
                    }),
                    $("<label>", {
                        "class": "student-code",
                        "html": repo.StudentCode
                    })
            ]
        })[0].outerHTML;
        return markup;
    }
    param.control.select2({
        language: {
            // You can find all of the options in the language files provided in the
            // build. They all must be functions that return the string that should be
            // displayed.
            inputTooShort: function () {
                return "You must enter more characters...";
            }
        },
        ajax: {
            url: "/dropdown/loadstudentdropdown",
            method: "post",
            dataType: "json",
            delay: 250,
            data: function (params) {
                var ajaxData = {
                    query: params.term,
                    classId: param.classId()
                };
                if (param.data != undefined) {
                    var dat = param.data();
                    for (var i in dat) {
                        ajaxData[i] = dat[i];
                    }
                }
                return ajaxData;
            },
            processResults: function (data) {
                var result = {
                    results: []
                };
                for (var i = 0; i < data.length; i++) {
                    data[i].id = data[i].Id;
                    data[i].text = data[i].Fullname;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        escapeMarkup: function (markup) {
            return markup;
        }, // let our custom formatter work
        minimumInputLength: 0,
        templateResult: formatClass, // omitted for brevity, see the source of this page
        templateSelection: function (data) {
            if (param.selectedCallback) {
                param.selectedCallback(data);
            }
            return data.text;
        }
    });
}

function initSchoolYearDropdown(param) {
    var formatSchoolYear = function (repo) {
        if (repo.loading) {
            return "Đang tìm kiếm...";
        }
        var markup;
        if (repo.allowAll) {
            markup = "<div class='schoolyear-dropdown'>" +
                "<i class='schoolyear-mask'></i>" +
                "<label class='schoolyear-name'>Tất cả</label>" +
                "</div>";
        } else {
            markup = "<div class='schoolyear-dropdown'>" +
                "<i class='schoolyear-mask " + (repo.IsActive ? "active" : "") + "'></i>" +
                "<label class='schoolyear-name'>Năm học " + repo.Year + "-" + (repo.Year + 1) + "</label>" +
                "</div>";
        }
        return markup;
    }
    param.control.select2({
        ajax: {
            url: "/dropdown/loadschoolyeardropdown",
            dataType: "json",
            delay: 250,
            data: function (params) {
                var ajaxData = {
                    ignore: param.ignore()
                };
                if (param.data != undefined) {
                    var dat = param.data();
                    for (var i in dat) {
                        ajaxData[i] = dat[i];
                    }
                }
                return ajaxData;
            },
            processResults: function (data) {
                var result = {
                    results: []
                };
                if (param.allowAll) {
                    result.results.push({
                        allowAll: true,
                        id: " ",
                        text: "Tất cả"
                    });
                }
                for (var i = 0; i < data.length; i++) {
                    data[i].id = data[i].Id;
                    data[i].text = data[i].Year + "-" + (data[i].Year + 1);
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        minimumResultsForSearch: Infinity,
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        templateResult: formatSchoolYear,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initGradeDropdown(param) {
    var formatSchoolYear = function (repo) {
        if (repo.loading) {
            return "Đang tìm kiếm...";
        }
        return repo.text;
    }
    param.control.select2({
        ajax: {
            url: "/dropdown/loadgradedropdown",
            dataType: "json",
            delay: 250,
            data: function (params) {
                var ajaxData = {
                    ignore: param.ignore()
                };
                if (param.data != undefined) {
                    var dat = param.data();
                    for (var i in dat) {
                        ajaxData[i] = dat[i];
                    }
                }
                return ajaxData;
            },
            processResults: function (data) {
                var result = {
                    results: []
                };
                if (param.allowAll) {
                    result.results.push({
                        allowAll: true,
                        id: "",
                        text: "Tất cả"
                    });
                }
                for (var i = 0; i < data.length; i++) {
                    data[i].id = data[i].grade;
                    data[i].text = data[i].grade;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        minimumResultsForSearch: Infinity,
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        templateResult: formatSchoolYear,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initPolicyDropdown(param) {
    var format = function (repo) {
        if (repo.loading) {
            return "Đang tìm kiếm...";
        }
        return repo.text;
    }
    param.control.select2({
        ajax: {
            url: "/dropdown/loadpolicydropdown",
            dataType: "json",
            delay: 250,
            data: function (params) {
                var ajaxData = {
                    "categoryId": param.category()
                };
                if (param.data != undefined) {
                    var dat = param.data();
                    for (var i in dat) {
                        ajaxData[i] = dat[i];
                    }
                }
                return ajaxData;
            },
            processResults: function (data) {
                var result = {
                    results: []
                };
                if (param.allowAll) {
                    result.results.push({
                        allowAll: true,
                        id: "",
                        text: "Tất cả"
                    });
                }
                for (var i = 0; i < data.length; i++) {
                    data[i].id = data[i].Id;
                    data[i].text = data[i].Name;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        minimumResultsForSearch: Infinity,
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        templateResult: format,
        templateSelection: function (data) {
            if (param.selectedCallback) {
                param.selectedCallback(data);
            }
            return data.text;
        }
    });
}

function initPolicyCategoryDropdown(param) {
    var formatSchoolYear = function (repo) {
        if (repo.loading) {
            return "Đang tìm kiếm...";
        }
        return repo.text;
    }
    param.data.setChangedFunction(function (data) {
        var controlData = [];
        if (param.allowAll) {
            controlData.push({
                id: " ",
                text: "Tất cả",
                visual: true
            });
        }
        for (var i in data) {
            controlData.push(data[i]);
        }
        param.control.select2({
            placeholder: param.placeholder,
            data: controlData,
            minimumResultsForSearch: Infinity,
            escapeMarkup: function (markup) {
                return markup;
            },
            minimumInputLength: 0,
            templateResult: formatSchoolYear,
            templateSelection: function (data) {
                if (param.selectedCallback != undefined) {
                    param.selectedCallback(data);
                }
                return data.text;
            }
        });
    });
}

function PolicyCategoryData() {
    this.data = [];
    this.changedFunction = [];
}

PolicyCategoryData.prototype.setChangedFunction = function (func) {
    this.changedFunction.push(func);
}

PolicyCategoryData.prototype.changed = function () {
    var changedFunction = this.changedFunction;
    for (var i in changedFunction) {
        if (changedFunction.hasOwnProperty(i)) {
            changedFunction[i](this.data);
        }
    }
}

PolicyCategoryData.prototype.update = function () {
    var obj = this;
    $.ajax({
        url: "/dropdown/loadpolicycategorydropdown",
        method: "POST",
        dataType: "json",
        success: function (data) {
            for (var i = 0; i < data.length; i++) {
                data[i].id = data[i].Id;
                data[i].text = data[i].Name;
            }
            obj.data = data;
            obj.changed();
        }
    });
}

function initFormularDropDown(param) {
    var formatSchoolYear = function (repo) {
        if (repo.loading) {
            return "Đang tìm kiếm...";
        }
        return repo.text;
    }
    param.data.setChangedFunction(function (data) {
        var controlData = [];
        if (param.allowAll) {
            controlData.push({
                id: " ",
                text: "Tất cả",
                visual: true
            });
        }
        for (var i in data) {
            controlData.push(data[i]);
        }
        param.control.val(null);
        param.control.select2({
            placeholder: param.placeholder,
            data: controlData,
            minimumResultsForSearch: Infinity,
            escapeMarkup: function (markup) {
                return markup;
            },
            minimumInputLength: 0,
            templateResult: formatSchoolYear,
            templateSelection: function (data) {
                if (param.selectedCallback != undefined) {
                    param.selectedCallback(data);
                }
                return data.text;
            }
        });
    });
}

function FormularData(type) {
    this.data = [];
    this.type = type;
    this.changedFunction = [];
}

FormularData.prototype.setChangedFunction = function (func) {
    this.changedFunction.push(func);
}

FormularData.prototype.changed = function () {
    var changedFunction = this.changedFunction;
    for (var i in changedFunction) {
        if (changedFunction.hasOwnProperty(i)) {
            changedFunction[i](this.data);
        }
    }
}

FormularData.prototype.update = function () {
    var obj = this;
    $.ajax({
        url: "/dropdown/loadformulardropdown",
        method: "POST",
        data: {
            type: obj.type()
        },
        dataType: "json",
        success: function (data) {
            for (var i = 0; i < data.length; i++) {
                data[i].id = data[i].Id;
                data[i].text = data[i].Name;
            }
            obj.data = data;
            obj.changed();
        }
    });
}