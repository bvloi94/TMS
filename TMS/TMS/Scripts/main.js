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
        case "New":
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
        case "Canceled":
            cssClass = "label-default";
            break;
        case "Closed":
            cssClass = "label-default";
            break;
    }
    return lbl = $("<small/>",
    {
        "class": "label " + cssClass,
        "html": status
    });
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
                    return (!row.DepartmentName || 0 === row.DepartmentName.length) ? "-" : row.DepartmentName;
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
