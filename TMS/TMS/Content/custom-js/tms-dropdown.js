
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
            markup = "<div class='category-dropdown'>" +
                "<label class='category-name'>All</label>" +
                "</div>";

        } else {
            markup = "<div class='category-dropdown'>" +
                "<label class='category-" + repo.Level + "'>" +
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
                    ignore: param.ignore(),
                    query: params.term
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
                    data[i].id = data[i].ID+"";
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
        templateResult: formatt,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initGroupDropdown(param) {
    param.control.select2({
        ajax: {
            url: "/dropdown/loadgroupdropdown",
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
        placeholder: "-- Select Group --",
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
        ajax: {
            url: "/dropdown/loadtechniciandropdown",
            dataType: "json",
            data: function (params) {
                var ajaxData = {
                    ignore: JSON.stringify(param.ignore()),
                    query: params.term,
                    groupId: $("[data-role='ddl-group']").val()
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
        placeholder: "-- Select Technician --",
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        templateResult: formatt,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initCriteriaDropdown(param) {
    var formatt = function (repo) {
        var markup = "";
        if (repo.allowAll) {
            markup = "<div class='criteria-dropdown'>" +
                "<label class='criteria-name'>All</label>" +
                "</div>";

        } else {
            markup = "<div class='criteria-dropdown'>" +
                "<label class='criteria-name'>" + repo.Name + "</label>" +
                "</div>";
        }
        return markup;
    }
    param.control.select2({
        ajax: {
            url: "/dropdown/LoadCriteriaDropdown",
            dataType: "json",
            data: function (params) {
                var ajaxData = {
                    query: params.term
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
        },
        minimumInputLength: 0,
        templateResult: formatt,
        templateSelection: function (data) {
            return data.text;
        }
    });


}

function initConditionDropdown(param) {
    var formatt = function (repo) {
        var markup = "";
        if (repo.allowAll) {
            markup = "<div class='condition-dropdown'>" +
                "<label class='condition-name'>All</label>" +
                "</div>";

        } else {
            markup = "<div class='condition-dropdown'>" +
                "<label class='condition-name'>" + repo.Name + "</label>" +
                "</div>";
        }
        return markup;
    }
    param.control.select2({
        ajax: {
            url: "/dropdown/LoadConditionDropdown",
            dataType: "json",
            data: function (params) {
                var ajaxData = {
                    query: params.term,
                    criteria: param.criteria
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
        },
        minimumInputLength: 0,
        templateResult: formatt,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initRequesterDropdown(param) {
    var formatt = function (repo) {
        var markup = "";
        if (repo.allowAll) {
            markup = "<div class='requester-dropdown'>" +
                "<label class='requester-name'>All</label>" +
                "</div>";

        } else {
            markup = "<div class='requester-dropdown'>" +
                "<label class='requester-name'>" + repo.Name + "</label>" +
                "<label class='requester-email'>( " + repo.Email + " )</label>" +
                "</div>";
        }
        return markup;
    }
    param.control.select2({
        ajax: {
            url: "/dropdown/LoadRequesterDropdown",
            dataType: "json",
            type: "POST",
            data: function (params) {
                var ajaxData = {
                    ignore: JSON.stringify(param.ignore()),
                    query: params.term
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
        multiple: true,
        //placeholder: "-- Select Requester --",
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        closeOnSelect: false,
        templateResult: formatt,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initConditionValueDropdown(param) {
    var formatt = function (repo) {
        var markup = "";
        if (repo.allowAll) {
            markup = "<div><label>All</label></div>";
        } else {
            markup = "<div><label>" + repo.Name + "</label></div>";
        }
        return markup;
    }
    param.control.select2({
        ajax: {
            url: "/dropdown/LoadConditionValueDropdown",
            dataType: "json",
            type: "POST",
            data: function (params) {
                var ajaxData = {
                    ignore: JSON.stringify(param.ignore()),
                    query: params.term,
                    criteria: param.criteria
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
                    data[i].id = data[i].Id + "";
                    data[i].text = data[i].Name;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        multiple: true,
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        closeOnSelect: false,
        templateResult: formatt,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initActionDropdown(param) {
    var formatt = function (repo) {
        var markup = "";
        if (repo.allowAll) {
            markup = "<div><label>All</label></div>";
        }
        else { markup = "<div><label >" + repo.Name + "</label></div>"; }
        return markup;
    }
    param.control.select2({
        ajax: {
            url: "/dropdown/LoadActionDropdown",
            dataType: "json",
            type: "POST",
            data: function (params) {
                var ajaxData = {
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
        placeholder: "-- Select action --",
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        templateResult: formatt,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initStatusDropdown(param) {
    var formatt = function (repo) {
        var markup = "";
        if (repo.allowAll) {
            markup = "<div><label>All</label></div>";
        }
        else { markup = "<div><label >" + repo.Name + "</label></div>"; }
        return markup;
    }
    param.control.select2({
        ajax: {
            url: "/dropdown/LoadStatusDropdown",
            dataType: "json",
            type: "POST",
            data: function (params) {
                var ajaxData = {
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
                    data[i].id = data[i].Id+"";
                    data[i].text = data[i].Name;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        placeholder: "-- Select status --",
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        templateResult: formatt,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initCategoryConditionDropdown(param) {
    var formatt = function (repo) {
        var markup = "";
        if (repo.allowAll) {
            markup = "<div class='category-dropdown'>" +
                "<label class='category-name'>All</label>" +
                "</div>";

        } else {
            markup = "<div class='category-dropdown'>" +
                "<label class='category-" + repo.Level + "'>" +
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
            data: function(params) {
                var ajaxData = {
                    ignore: param.ignore(),
                    query: params.term
                };
                if (param.data != undefined) {
                    var dat = param.data();
                    for (var i in dat) {
                        ajaxData[i] = dat[i];
                    }
                }
                return ajaxData;
            },
            processResults: function(data) {
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
                    data[i].id = data[i].ID + "";
                    data[i].text = data[i].Name;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        multiple: true,
        minimumResultsForSearch: Infinity,
        escapeMarkup: function(markup) {
            return markup;
        },
        closeOnSelect: false,
        templateResult: formatt,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

function initCategoryDropdownByLevel(param) {
    var formatt = function (repo) {
        var markup = "";
        if (repo.allowAll) {
            markup = "<div><label>All</label></div>";
        }
        else { markup = "<div><label >" + repo.Name + "</label></div>"; }
        return markup;
    }
    param.control.select2({
        ajax: {
            url: "/dropdown/LoadCategoryDropdownByLevel",
            dataType: "json",
            type: "POST",
            data: function (params) {
                var ajaxData = {
                    level: param.level
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
                    data[i].id = data[i].Id+"";
                    data[i].text = data[i].Name;
                    result.results.push(data[i]);
                }
                return result;
            },
            cache: true
        },
        placeholder: "-- Select category --",
        escapeMarkup: function (markup) {
            return markup;
        },
        minimumInputLength: 0,
        templateResult: formatt,
        templateSelection: function (data) {
            return data.text;
        }
    });
}

