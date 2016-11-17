function removeActiveMenu() {
    $("#menu-ticket").removeClass("active");
    $("#menu-knowledgebase").removeClass("active");
    $("#menu-requester").removeClass("active");
    $("#menu-report").removeClass("active");
}

function setActiveTicketMenu() {
    removeActiveMenu();
    $("#menu-ticket").addClass("active");
}

function setActiveKnowledgeBaseMenu() {
    removeActiveMenu();
    $("#menu-knowledgebase").addClass("active");
}

function setActiveRequesterMenu() {
    removeActiveMenu();
    $("#menu-requester").addClass("active");
}

function setActiveReportMenu() {
    removeActiveMenu();
    $("#menu-report").addClass("active");
}

function getStatusLabel(status) {
    var cssClass = "label-default";
    switch (status) {
        case "Open":
            cssClass = "label-info";
            break;
        case "Assigned":
            cssClass = "label-warning";
            break;
        case "Solved":
            cssClass = "label-success";
            break;
        case "Unapproved":
            cssClass = "label-danger";
            break;
        case "Cancelled":
            cssClass = "label-violet";
            break;
        case "Closed":
            cssClass = "label-default";
            break;
    }
    return $("<small/>",
    {
        "class": "label " + cssClass,
        "html": status
    })[0].outerHTML;
}

function readURL(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#avatar-img').attr('src', e.target.result);
        };

        reader.readAsDataURL(input.files[0]);
    }
}

function loadInitDropdown(selector, text, value) {
    var opt = new Option(text, value, true, true);
    $("[data-role='" + selector + "']:not([disabled])").append(opt);
    $("[data-role='" + selector + "']:not([disabled])").trigger("change");
}

function initDropdown(selector, text, value) {
    var opt = new Option(text, value, true, true);
    selector.append(opt);
    selector.trigger("change");
}

function initRequesterTable(selector) {
    return selector.dataTable({
        processing: true,
        serverSide: true,
        paging: true,
        sort: true,
        filter: false,
        searching: true,
        lengthMenu: [8],
        order: [[0, 'asc']],
        lengthChange: false,
        ajax: {
            url: "/HelpDesk/ManageTicket/GetRequesterList"
        },
        "columnDefs": [
            {
                "targets": 0,
                "render": function (data, type, row) {
                    return '<a href="javascript:addSelectedUserToForm(\'' +
                        row.Id + "\',\'" +
                        row.Fullname +
                        '\')">' +
                        row.Fullname +
                        '</a>';
                }
            },
            {
                "targets": 1,
                "render": function (data, type, row) {
                    return (!row.Email || 0 === row.Email.length) ? "-" : row.Email;
                }
            },
            {
                "targets": 2,
                "sortable": false,
                "render": function (data, type, row) {
                    return (!row.GroupName || 0 === row.GroupName.length) ? "-" : row.GroupName;
                }
            },
            {
                "targets": 3,
                "sortable": false,
                "render": function (data, type, row) {
                    return (!row.PhoneNumber || 0 === row.PhoneNumber.length) ? "-" : row.PhoneNumber;
                }
            },
            {
                "targets": 4,
                "sortable": false,
                "render": function (data, type, row) {
                    return (!row.JobTitle || 0 === row.JobTitle.length) ? "-" : row.JobTitle;
                }
            }
        ]
    });
}

function handleFileSelect(evt) {
    var files = evt.target.files;
    var f = files[0];
    var reader = new FileReader();

    reader.onload = (function (theFile) {
        return function (e) {
            document.getElementById('list').innerHTML = ['<img src="', e.target.result, '" title="', theFile.name, '" width="50" />'].join('');
        };
    })(f);

    reader.readAsDataURL(f);
}

var notifyFlashMessage = function (options) {
    options = $.extend({}, options, { timeout: 3000 });
    if (!options.message) {
        options.message = getFlashMessageFromCookie();
        options.status = getFlashMessageStatusFromCookie();
        deleteFlashMessageCookie();
    }
    if (options.message) {
        if (options.status) {
            if (options.status == "success") {
                noty({
                    text: options.message,
                    layout: "topCenter",
                    type: "success",
                    timeout: 2000
                });
            } else {
                noty({
                    text: options.message,
                    type: "error",
                    layout: "topRight",
                    timeout: 2000
                });
            }
        }
    }
};

function getFlashMessageFromCookie() {
    return $.cookie("FlashMessage");
}

function getFlashMessageStatusFromCookie() {
    return $.cookie("FlashMessageStatus");
}

function deleteFlashMessageCookie() {
    $.removeCookie('FlashMessage', { path: '/' });
    $.removeCookie('FlashMessageStatus', { path: '/' });
}

function formatDateTime(date) {
    var hours = date.getHours();
    var minutes = date.getMinutes();
    var seconds = date.getSeconds();
    //var ampm = hours >= 12 ? 'pm' : 'am';
    //hours = hours % 12;
    //hours = hours ? hours : 12; // the hour '0' should be '12'
    hours = hours < 10 ? '0' + hours : hours;
    minutes = minutes < 10 ? '0' + minutes : minutes;
    seconds = seconds < 10 ? '0' + seconds : seconds;
    //var strTime = hours + ':' + minutes + ' ';// + ampm;
    //return date.getMonth() + 1 + "/" + date.getDate() + "/" + date.getFullYear() + "  " + strTime;
    return date.getMonth() + 1 + "/" + date.getDate() + "/" + date.getFullYear() + "  " + hours + ":" + minutes + ":" + seconds;
}

var checkUploadFile = function () {
    var maxUploadSize = 4;
    var size = 0;
    $("input[type='file']").each(function (i, e) {
        for (i = 0; i < $(e).get(0).files.length; i++) {
            size += parseFloat(($(e).get(0).files[i].size / 1024 / 1024).toFixed(2));
        }
    });
    if (size < maxUploadSize) {
        return true;
    }
    noty({
        text: "Upload files must less than 4MB!",
        type: "error",
        layout: "topRight",
        timeout: 2000
    });
    return false;
}

$(document).ready(function () {

    $(".datetime").datetimepicker({
        timepicker: false,
        format: 'd/m/Y'
    });

    $(".datetimep").datetimepicker({
        timepicker: true,
        format: "d/m/Y H:i",
        pickMinute: true
    });
});
