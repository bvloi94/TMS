
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
                    data[i].id = data[i].ID;
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
        placeholder: "--Select Department--",
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
                    departmentId: $("[data-role='ddl-department']").val()
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
        placeholder: "--Select Technician--",
        escapeMarkup: function (markup) {
            return markup;
        }, // let our custom formatter work
        minimumInputLength: 0,
        templateResult: formatt, // omitted for brevity, see the source of this page
        templateSelection: function (data) {
            return data.text;
        } // omitted for brevity, see the source of this page
    });
}