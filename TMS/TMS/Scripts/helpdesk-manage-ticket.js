var ticketTable = null;
var cancelTicketId = null;
var closeTicketId = null;
var reassignTicketId = null;
var selectedTickets = [];

$(window).on('resize', function () {
    $('.ticket-subject').trunk8({
        lines: 2,
        tooltip: false
    });
});

function initTicketTable() {

    ticketTable = $("#ticket-table").DataTable({
        serverSide: true,
        processing: true,
        sort: true,
        filter: false,
        //searchable: true,
        lengthMenu: [10],
        order: [[1, 'des']],
        lengthChange: false,
        ajax: {
            "url": "/HelpDesk/ManageTicket/LoadAllTickets",
            "type": "POST",
            "data": function (d) {
                d.filter_created = $("[data-role='filter_created_select']").val();
                d.filter_time_period = $("[data-role='filter_time_period_input']").val();
                d.filter_dueby = JSON.stringify(getDueByFilter());
                d.filter_status = JSON.stringify($("[data-role='filter_status_select']").val());
                d.filter_mode = JSON.stringify($("[data-role='filter_mode_select']").val());
                d.filter_requester = JSON.stringify($("[data-role='filter_requester_select']").val());
            }
        },
        drawCallback: function () {
            checkSelectedCheckbox();
            $('.ticket-subject').trunk8({
                lines: 2,
                tooltip: false
            });

        },
        autoWidth: false,
        columnDefs: [
            {
                "targets": [0],
                "sortable": false,
                "render": function (data, type, row) {
                    return '<div class="text-center"><input type="checkbox" data-role="cbo-ticket" data-id="' + row.Id + '" data-requester="' + row.Requester + '"/></div>';
                }
            },
            {
                "targets": [1],
                "render": function (data, type, row) {
                    var ticketInfo = "";
                    var code = $("<span/>",
                    {
                        "class": "text-muted text-sm",
                        "text": "#" + row.Code
                    })[0].outerHTML;

                    var status = $("<div/>",
                   {
                       "class": "label-status",
                       "html": getStatusLabel(row.Status)
                   })[0].outerHTML;

                    var subject = $("<a/>",
                    {
                        "class": "ticket-subject",
                        "href": "javascript:openTicketDetailModal(" + row.Id + ")",
                        "html": row.Subject
                    })[0].outerHTML;

                    var creater = row.Requester != "" ? row.Requester : "-";
                    var createdBy = $("<p/>",
                    {
                        "class": "text-muted",
                        "html": 'Request by <span class="text-bold">' + creater + '</span>'
                    })[0].outerHTML;

                    var priority = row.Priority == "" ? " None" : '<div class="priority-color" style="background-color: ' + row.PriorityColor + '"></div>&nbsp;<span class="text-bold">' + row.Priority
                    var priorityRow = $("<span/>",
                    {
                        "html": 'Priority: ' + priority
                    })[0].outerHTML;

                    var modifiedTime = $("<span/>",
                    {
                        "class": "text-muted",
                        "html": 'Last modified <span class="text-bold">' + row.ModifiedTimeString + '. </span>'
                    })[0].outerHTML;

                    var overdueTime = "";
                    var overdueDiv = "";
                    if (row.IsOverdue) {
                        overdueTime = $("<span/>",
                        {
                            "class": "text-red text-bold",
                            "text": row.OverdueDateString
                        })[0].outerHTML;
                        overdueDiv = $("<div/>",
                        {
                            "class": "overdue",
                            "html": "Overdue"
                        })[0].outerHTML;
                    } else {
                        overdueTime = $("<span/>",
                        {
                            "class": "text-black text-bold",
                            "text": row.OverdueDateString
                        })[0].outerHTML;
                    }


                    ticketInfo = $("<div/>",
                    {
                        "class": "col-lg-10 col-sm-9",
                        "style": "line-height: 14px",
                        "html": '<p>' + status + '&nbsp;&nbsp;' + subject + '&nbsp;&nbsp;' + code + '</p>' + createdBy + modifiedTime + overdueTime
                    })[0].outerHTML;

                    ticketInfo += $("<div/>",
                    {
                        "class": "col-lg-2 col-sm-3 pull-right",
                        "html": priorityRow + overdueDiv
                    })[0].outerHTML;

                    var ticketInfoContainer = $("<div/>",
                    {
                        "class": "row ticket-info",
                        "html": ticketInfo
                    })[0].outerHTML;

                    return ticketInfoContainer;
                },
                "width": "70%"
            },
            {
                "targets": [2],
                "sortable": false,
                "render": function (data, type, row) {
                    var links = '';
                    var edit = '<li>'
                    + '<a href="/HelpDesk/ManageTicket/EditTicket/' + row.Id + '">'
                    + '<i class="fa fa-pencil" aria-hidden="true"></i> Edit Ticket'
                    + '</a>'
                    + '</li>';
                    var history = '<li>'
                    + '<a href="/Ticket/History/' + row.Id + '">'
                    + '<i class="fa fa-history" aria-hidden="true"></i> Ticket History'
                    + '</a>'
                    + '</li>';
                    var detail = '<li>'
                    + '<a href="/Ticket/TicketDetail/' + row.Id + '">'
                    + '<i class="fa fa-info-circle" aria-hidden="true"></i> Ticket Detail'
                    + '</a>'
                    + '</li>';
                    switch (row.Status) {
                        case "Open":
                            links += edit
                       + history
                       + detail
                       + '<li>'
                       + '<a href="/Ticket/Solve/' + row.Id + '">'
                       + '<i class="fa fa-commenting" aria-hidden="true"></i> Solve Ticket'
                       + '</a>'
                       + '</li>'
                       + '<li>'
                       + '<a href="javascript:void(0)" data-role="btn-show-cancel-modal" data-ticket-id="' + row.Id + '">'
                       + '<i class="fa fa-ban" aria-hidden="true"></i> Cancel Ticket'
                       + '</a>'
                       + '</li>';
                            break;
                        case "Assigned":
                            links += edit
                       + history
                       + detail
                       + '<li>'
                       + '<a href="javascript:void(0)" data-role="btn-show-cancel-modal" data-ticket-id="' + row.Id + '">'
                       + '<i class="fa fa-ban" aria-hidden="true"></i> Cancel Ticket'
                       + '</a>'
                       + '</li>';
                            break;
                        case "Solved":
                            links += edit + history + detail;
                            break;
                        case "Unapproved":
                            links += edit
                        + history
                        + detail
                        + '<li><a href="javascript:void(0)"><strong>Reopen Ticket</strong></a></li>'
                        + '<li>'
                        + '<a href="javascript:void(0)" onclick="showCloseModal(this)" data-ticket-id="' + row.Id + '">'
                        + '<i class="fa fa-times-circle" aria-hidden="true"></i> Close Ticket'
                        + '</a>'
                        + '</li>'
                        + '<li>'
                        + '<a href="javascript:void(0)" onclick="showReassignModal(this)" data-ticket-id="' + row.Id + '">'
                        + '<i class="fa fa-chevron-circle-right" aria-hidden="true"></i> Reassign'
                        + '</a>'
                        + '</li>'
                        + '<li>'
                        + '<a href="/Ticket/Solve/' + row.Id + '">'
                        + '<i class="fa fa-commenting" aria-hidden="true"></i> Re-solve Ticket'
                        + '</a>'
                        + '</li>';
                            break;
                        case "Closed":
                            links += edit + history + detail;
                            break;
                        case "Cancelled":
                            links += edit + history + detail;
                            break;
                    }
                    var action = '<div class="btn-group">'
                        + '<button type="button" class="btn bg-olive btn-flat dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">'
                        + 'Action <span class="caret"></span>'
                        + '</button>'
                        + '<ul class="dropdown-menu dropdown-menu-right">';
                    action += links;
                    action += '</ul>'
                        + '</div>';

                    return action;
                }
            }
        ],
        "oLanguage": {
            "sInfo": "Found _TOTAL_ tickets",
            "sLast": "Last page",
            "sFirst": "First",
            //"sSearch": "Search:",
            "sZeroRecords": "No records",
            "sEmptyTable": "No data",
            "sInfoFiltered": " - filter from _MAX_ rows",
            "sLengthMenu": "Show _MENU_ rows",
            "sProcessing": "Processing..."
        },
        "bAutoWidth": false
    });
}

function checkSelectedCheckbox() {
    $('input[data-role="cbo-ticket"]').each(function (index, element) {
        var id = $(element).attr("data-id");
        if (selectedTickets.indexOf(id) != -1) {
            $(element).prop("checked", true);
        }
    });
}

function initDropdownControl() {
    initUrgencyDropdown({
        control: $("[data-role=ddl-urgency]"),
        ignore: function () {
            return [];
        }
    });
    initPriorityDropdown({
        control: $("[data-role=ddl-priority]"),
        ignore: function () {
            return [];
        }
    });
    initImpactDropdown({
        control: $("[data-role=ddl-impact]"),
        ignore: function () {
            return [];
        }
    });
    initDepartmentDropdown({
        control: $("[data-role=ddl-department]"),
        ignore: function () {
            return [];
        }
    });
    initCategoryDropdown({
        control: $("[data-role=ddl-category]"),
        ignore: function () {
            return [];
        }
    });
    initTechnicianDropdown({
        control: $("[data-role=ddl-technician]"),
        ignore: function () {
            return [];
        }
    });
}

function openTicketDetailModal(ticketId) {
    $('#ticket-merged-div').hide();
    $.ajax({
        url: '/Ticket/GetTicketDetail',
        type: "GET",
        dataType: "json",
        data: {
            id: ticketId
        },
        success: function (data) {
            if (data.success) {
                $('#ticket-subject').text(data.subject);
                //$('#ticket-description').text(data.description);
                $('#ticket-department').text(data.department);
                $('#ticket-technician').text(data.technician);
                $('#ticket-created-by').text(data.creater);
                $('#ticket-assigned-by').text(data.assigner);
                $("#ticket-requester").text(data.requester);
                $("#ticket-solved-by").text(data.solver);
                $('#ticket-type').text(data.type);
                $('#ticket-mode').text(data.mode);
                $('#ticket-urgency').text(data.urgency);
                $('#ticket-priority').text(data.priority);
                $('#ticket-category').text(data.category);
                $('#ticket-impact').text(data.impact);
                $('#ticket-impact-detail').text(data.impactDetail);
                if (data.mergeTicket) {
                    $('#ticket-merged').html(data.mergeTicket);
                    $('#ticket-merged-div').show();
                }
                $("#ticket-tags").html(loadKeywordToTags(data.tags));
                $("#ticket-note").html(data.note);
                $("#reopen-close-btn").attr("data-ticket-id", data.id);
                $("#reopen-resolve-btn").attr("href", "/Ticket/Solve/" + data.id);
                $("#reopen-reassign-btn").attr("data-ticket-id", data.id);
                $("#refer-older-ticket-btn").attr("data-id", data.id);
                //action button group
                $("#action-cancel-btn").attr("data-ticket-id", data.id);
                $("#action-solve-btn").attr("href", "/Ticket/Solve/" + data.id);
                $("#action-edit-btn").attr("href", "/HelpDesk/ManageTicket/EditTicket/" + data.id);
                $("#action-history-btn").attr("href", "/Ticket/History/" + data.id);
                $("#action-detail-btn").attr("href", "/Ticket/TicketDetail/" + data.id);

                var solution = "";
                if (!data.solution || data.solution == "-") {
                    solution = "This ticket is not solved yet.";
                }
                else {
                    solution = data.solution;
                }

                $('#ticket-description-attachments').empty();
                if (data.descriptionAttachment && data.descriptionAttachment != "") {
                    $('#ticket-description-attachments').append(data.descriptionAttachment);
                }

                $('#ticket-solution-attachments').empty();
                if (data.solutionAttachment && data.solutionAttachment != "") {
                    $('#ticket-solution-attachments').append(data.solutionAttachment);
                }

                //solve: new
                //edit: all
                //cancel: new, assigned
                //reopen: unapproved
                if (data.status == 1) {
                    $('#ticket-status').html(getStatusLabel('Open'));
                    $('#action-solve-btn').show();
                    $("#action-cancel-btn").show();
                    $(".reopen-li").hide();
                } else if (data.status == 2) {
                    $('#ticket-status').html(getStatusLabel('Assigned'));
                    $('#action-solve-btn').hide();
                    $("#action-cancel-btn").show();
                    $(".reopen-li").hide();
                } else if (data.status == 3) {
                    $('#ticket-status').html(getStatusLabel('Solved'));
                    $('#action-solve-btn').hide();
                    $("#action-cancel-btn").hide();
                    $(".reopen-li").hide();
                } else if (data.status == 4) {
                    $('#ticket-status').html(getStatusLabel('Unapproved'));
                    $('#action-solve-btn').hide();
                    $("#action-cancel-btn").hide();
                    $(".reopen-li").show();
                } else if (data.status == 5) {
                    $('#ticket-status').html(getStatusLabel('Cancelled'));
                    $('#action-solve-btn').hide();
                    $("#action-cancel-btn").hide();
                    $(".reopen-li").hide();
                } else if (data.status == 6) {
                    $('#ticket-status').html(getStatusLabel('Closed'));
                    $('#action-solve-btn').hide();
                    $("#action-cancel-btn").hide();
                    $(".reopen-li").hide();
                }

                $('#ticket-created-date').text(data.createdDate);
                $('#ticket-modified-date').text(data.lastModified);
                $('#ticket-solved-date').text(data.solvedDate);
                $('#ticket-schedule-start').text(data.scheduleStart);
                $('#ticket-schedule-end').text(data.scheduleEnd);
                $('#ticket-actual-start').text(data.actualStart);
                $('#ticket-actual-end').text(data.actualEnd);

                $('#ticket-solveUser').text(data.solveUser);

                $('#detail-modal').modal("show");
                $('#ticket-description').trunk8({
                    tooltip: false,
                    lines: 6,
                    fill: '&hellip; <a id="see-more" href="/Ticket/TicketDetail/' + data.id + '">See More</a>'
                });
                $('#ticket-description').trunk8("update", data.description);
                $('#ticket-solution').trunk8({
                    tooltip: false,
                    lines: 6,
                    fill: '&hellip; <a id="see-more" href="/Ticket/TicketDetail/' + data.id + '">See More</a>'
                });
                $('#ticket-solution').trunk8("update", solution);
            } else {
                noty({
                    text: data.message,
                    type: "error",
                    layout: "topRight",
                    timeout: 2500
                });
            }
        },
        error: function () {
            noty({
                text: "Cannot connect to server",
                type: "error",
                layout: "topRight",
                timeout: 2500
            });
        }
    });
}

function getDueByFilter() {
    return $('.due_by_item:checked')
        .map(function () { return $(this).val(); })
        .get();
}

function initFilter() {
    var created_select = $('[data-role="filter_created_select"');
    var mode_select = $('[data-role="filter_mode_select"');
    var status_select = $('[data-role="filter_status_select"');
    var requester_select = $('[data-role="filter_requester_select"');
    var time_period = $('[data-role="filter_time_period"');
    var time_period_inp = $('[data-role="filter_time_period_input"');

    created_select.select2();
    time_period_inp.daterangepicker({
        locale: {
            format: 'DD/MM/YYYY'
        }
    });
    time_period_inp.val("");
    mode_select.select2();
    status_select.select2();
    status_select.select2('val', "1");
    initRequesterDropdown({
        control: requester_select,
        ignore: function () {
            return requester_select.val();
        }
    });
    created_select
        .on("change",
            function () {
                time_period_inp.val("");
                if (created_select.val() == "set_date") {
                    if (time_period.hasClass("hidden")) {
                        time_period.removeClass("hidden");
                    }
                } else {
                    if (!time_period.hasClass("hidden")) {
                        time_period.addClass("hidden");
                    }
                    ticketTable.draw();
                }
            });
    $('.due_by_item, [data-role="filter_mode_select"],[data-role="filter_status_select"],[data-role="filter_requester_select"]')
        .on("change",
            function () {
                ticketTable.draw();
            });

    time_period_inp.on("change",
        function() {
            ticketTable.draw();
        });
}

$(document).ready(function () {
    //setActiveTicketMenu();

    // Init filter 
    initFilter();

    // Init ticket table
    initTicketTable();

    //$("#filterBtn").on("click", function () {

    //    // if submenu is hidden, does not have active class
    //    if (!$('#filterBtn').hasClass("active")) {

    //        $('#filter-div').removeClass("hidden");
    //        $('#filterBtn').addClass("active");
    //        $('#filter-div').slideDown(400);

    //        //if submenu is visible
    //    } else if ($('#filterBtn').hasClass("active")) {

    //        $('#filter-div').slideToggle(400);
    //        $('#filter-div').addClass("hidden");
    //        $('#filterBtn').removeClass("active");
    //    }
    //});

    $("#search-txt").keyup(function () {
        ticketTable.draw();
    });

    $('#ticket-table tbody').on('click', 'a[data-role="btn-show-cancel-modal"]:not([disabled])', function () {
        cancelTicketId = this.getAttribute("data-ticket-id");
        $("#modal-cancel-ticket").modal("show");
    });

    $("[data-role='btn-confirm-cancel']").on('click', function () {
        $("[data-role='btn-confirm-cancel']").prop("disabled", true);
        $.ajax({
            "url": "/HelpDesk/ManageTicket/CancelTicket",
            "method": "POST",
            "data": {
                ticketId: cancelTicketId
            },
            "success": function (data) {
                if (data.success) {
                    $("#modal-cancel-ticket").modal("hide");
                    noty({
                        text: data.msg,
                        layout: "topCenter",
                        type: "success",
                        timeout: 2000
                    });
                    ticketTable.draw();
                } else {
                    $("#modal-cancel-ticket").modal("hide");
                    noty({
                        text: data.msg,
                        type: "error",
                        layout: "topRight",
                        timeout: 2000
                    });
                    ticketTable.draw();
                }
                $("[data-role='btn-confirm-cancel']").prop("disabled", false);
            },
            "error": function () {
                $("#modal-cancel-ticket").modal("hide");
                noty({
                    text: "Cannot connect to server!",
                    type: "error",
                    layout: "topRight",
                    timeout: 2000
                });
            }
        });
    });

    $("a[data-role='btn-merge-ticket'").on("click", function () {
        if (selectedTickets.length < 2) {
            noty({
                text: "Less than 2 tickets, can not merge!",
                type: "error",
                layout: "topRight",
                timeout: 2000
            });
        } else {
            $("#modal-merge-ticket").modal("show");
        }

    });

    $("[data-role='btn-confirm-merge']").on('click', function () {
        $("[data-role='btn-confirm-merge']").prop("disabled", true);
        $.ajax({
            "url": "/HelpDesk/ManageTicket/MergeTicket",
            "method": "POST",
            "data": {
                selectedTickets: selectedTickets
            },
            "success": function (data) {
                if (data.success) {
                    noty({
                        text: data.msg,
                        type: "success",
                        layout: "topCenter",
                        timeout: 2000
                    });
                    $("#modal-merge-ticket").modal("hide");
                    selectedTickets = [];
                    $("a[data-role='btn-merge-ticket']").addClass("disabled");
                    ticketTable.draw();
                } else {
                    noty({
                        text: data.msg,
                        type: "error",
                        layout: "topRight",
                        timeout: 2000
                    });
                    $("#modal-merge-ticket").modal("hide");
                    selectedTickets = [];
                    $("a[data-role='btn-merge-ticket']").addClass("disabled");
                    ticketTable.draw();
                }
                $("[data-role='btn-confirm-merge']").prop("disabled", false);
            },
            "error": function () {
                $("#modal-merge-ticket").modal("hide");
                noty({
                    text: "Cannot connect to server!",
                    type: "error",
                    layout: "topRight",
                    timeout: 2000
                });
            }
        });
    });

    $('#ticket-table tbody').on('click', 'input[data-role="cbo-ticket"]', function (e) {
        var id = $(this).attr("data-id");
        if (selectedTickets.indexOf(id) == -1) {
            selectedTickets.push(id);
        } else {
            selectedTickets.splice(selectedTickets.indexOf(id), 1);
        }
        if (selectedTickets.length < 2) {
            $("a[data-role='btn-merge-ticket']").addClass("disabled");
        } else {
            $("a[data-role='btn-merge-ticket']").removeClass("disabled");
        }
    });

    $("[data-role='ddl-department']").on("change", function () {
        $("[data-role='ddl-technician']").select2("val", "");
    });

    $("[data-role='btn-confirm-reassign']").click(function () {
        var technicianId = $("#technician-select").val();
        if (technicianId == null || technicianId.trim() == "") {
            $("#reassign-validation-message").html("Please select technician!");
            $("#reassign-validation-message").show();
        } else {
            $.ajax({
                url: "/HelpDesk/ManageTicket/Reassign",
                type: "POST",
                dataType: "json",
                data: {
                    technicianId: technicianId,
                    ticketId: reassignTicketId
                },
                success: function (data) {
                    if (data.success) {
                        noty({
                            text: data.message,
                            layout: "topCenter",
                            type: "success",
                            timeout: 2000
                        });
                    } else {
                        noty({
                            text: data.message,
                            layout: "topRight",
                            type: "error",
                            timeout: 2000
                        });
                    }
                    $(".modal").modal("hide");
                    ticketTable.draw();
                },
                error: function () {
                    noty({
                        text: "Cannot connect to server!",
                        layout: "topRight",
                        type: "error",
                        timeout: 2000
                    });
                }
            });
        }
    });

    $("[data-role='btn-confirm-close']").on('click', function () {
        $.ajax({
            url: "/HelpDesk/ManageTicket/CloseTicket",
            type: "POST",
            data: {
                ticketId: closeTicketId
            },
            success: function (data) {
                if (data.success) {
                    noty({
                        text: data.msg,
                        layout: "topCenter",
                        type: "success",
                        timeout: 2000
                    });
                    $("#modal-close-ticket").modal("hide");
                    $('#detail-modal').modal("hide");
                    ticketTable.draw();
                } else {
                    noty({
                        text: data.msg,
                        type: "error",
                        layout: "topRight",
                        timeout: 2000
                    });
                    $("#modal-close-ticket").modal("hide");
                    $('#detail-modal').modal("hide");
                    ticketTable.draw();
                }
            },
            error: function () {
                noty({
                    text: "Cannot connect to server!",
                    type: "error",
                    layout: "topCenter",
                    timeout: 2000
                });
                $("#modal-cancel-ticket").modal("hide");
                $('#detail-modal').modal("hide");
            }
        });
    });

    $("*").tooltip({
        disabled: true
    });
});

$("#refer-older-ticket-link").click(function () {
    var keywords = $("[name='Subject']").val();
    $("#refer-older-tickets-search").val(keywords);
    $("#older-tickets-table").dataTable().api().ajax.reload();
    $("#older-tickets-modal").modal("show");
});

$("#refer-older-tickets-search").on("keyup", function () {
    $("#older-tickets-table").dataTable().api().ajax.reload();
});

var initOlderTicketsTable = function () {
    $("#older-tickets-table").dataTable({
        processing: true,
        serverSide: true,
        paging: true,
        sort: true,
        filter: false,
        lengthMenu: [5],
        order: [[0, 'asc']],
        lengthChange: false,
        drawCallback: function () {
            var options = {
                accuracy: "complementary"
            };
            var keywords = $("#refer-older-tickets-search").val().replace(/[^a-z0-9\s]/gi, '');
            $(".subject-link").mark(keywords, options);
            $("#older-tickets-table .description-text").trunk8({
                lines: 3,
                tooltip: false
            });
        },
        ajax: {
            url: "/HelpDesk/ManageTicket/GetOlderTickets",
            data: function (d) {
                d.keywords = $("#refer-older-tickets-search").val()
            }
        },
        autoWidth: false,
        columnDefs: [
            {
                "targets": 0,
                "width": "15%",
                "render": function (data, type, row) {
                    return row.Code;
                }
            },
            {
                "targets": 1,
                "width": "30%",
                "render": function (data, type, row) {
                    return '<a href="javascript:void(0)" onclick="openTicketDetailModal(\'' + row.ID + '\')" class="subject-link">' + row.Subject + '</a>';
                }
            },
            {
                "targets": 2,
                "sortable": false,
                "width": "45%",
                "render": function (data, type, row) {
                    return '<span class="description-text">' + row.Description + '</span>';
                }
            },
            {
                "targets": 3,
                "width": "10%",
                "sortable": false,
                "render": function (data, type, row) {
                    return '<button type="button" class="btn btn-flat btn-primary" data-role="refer-older-ticket"'
                        + ' data-id="' + row.ID + '">Refer</button>';
                }
            }
        ]
    });
}

$(".modal").on("click", "[data-role='refer-older-ticket']", function () {
    var ticketId = $(this).attr("data-id");
    $("#refer-older-ticket-confirm-btn").attr("data-id", ticketId);
    $("#older-tickets-confirm-modal").modal("show");
});

$("#refer-older-ticket-confirm-btn").on("click", function () {
    var ticketId = $(this).attr("data-id");
    $.ajax({
        url: "/HelpDesk/ManageTicket/LoadTicketToRefer",
        type: "GET",
        dataType: "json",
        data: {
            id: ticketId
        },
        success: function (data) {
            if (data.success) {
                var ticket = data.data;
                $("[name='Subject']:not([readonly])").val(ticket.Subject);
                $("[name='Description']:not([readonly])").val(ticket.Description);
                if (ticket.Type != 0) {
                    $("[name='Type']").val(ticket.Type);
                }
                $("[name='ImpactDetail']").val(ticket.ImpactDetail);
                $("[name='ScheduleStartDate']").val(ticket.ScheduleStartDateString);
                $("[name='ScheduleEndDate']").val(ticket.ScheduleEndDateString);
                $("[name='ActualStartDate']").val(ticket.ActualStartDateString);
                $("[name='ActualEndDate']").val(ticket.ActualEndDateString);
                $("[name='Solution']").val(ticket.Solution);
                if (ticket.Tags) {
                    var tags = ticket.Tags.split(',');
                    for (i = 0; i < tags.length; i++) {
                        $('#tags').tagit('createTag', tags[i]);
                    }
                }
                $("[name='Note']").val(ticket.Solution);

                if (ticket.UrgencyId != 0) {
                    loadInitDropdown('ddl-urgency', ticket.Urgency, ticket.UrgencyId);
                }
                if (ticket.PriorityId != 0) {
                    loadInitDropdown('ddl-priority', ticket.Priority, ticket.PriorityId);
                }
                if (ticket.ImpactId != 0) {
                    loadInitDropdown('ddl-impact', ticket.Impact, ticket.ImpactId);
                }
                if (ticket.CategoryId != 0) {
                    loadInitDropdown('ddl-category', ticket.Category, ticket.CategoryId);
                }
                if (ticket.DepartmentId != 0) {
                    loadInitDropdown('ddl-department', ticket.Department, ticket.DepartmentId);
                }
                if (ticket.TechnicianId != null) {
                    loadInitDropdown('ddl-technician', ticket.Technician, ticket.TechnicianId);
                }

                $(".modal").modal("hide");
            } else {
                noty({
                    text: data.message,
                    type: "error",
                    layout: "topRight",
                    timeout: 2000
                });
            }
        },
        error: function () {
            noty({
                text: "Cannot connect to server!",
                type: "error",
                layout: "topRight",
                timeout: 2000
            });
        }
    });
});

var showCloseModal = function (obj) {
    closeTicketId = $(obj).attr("data-ticket-id");
    $("#modal-close-ticket").css("z-index", "1100");
    $("#modal-close-ticket").modal("show");
}

var showReassignModal = function (obj) {
    reassignTicketId = $(obj).attr("data-ticket-id");
    initDropdownControl();
    $.ajax({
        url: "/HelpDesk/ManageTicket/GetTicketDetailForReassign",
        type: "GET",
        dataType: "json",
        data: {
            ticketId: reassignTicketId
        },
        success: function (data) {
            if (data.success) {
                loadInitDropdown('ddl-department', data.department, data.departmentId);
                loadInitDropdown('ddl-technician', data.technician, data.technicianId);
            } else {
                noty({
                    text: data.message,
                    layout: "topRight",
                    type: "error",
                    timeout: 2000
                });
            }
        },
        error: function () {
            noty({
                text: "Cannot connect to server!",
                layout: "topRight",
                type: "error",
                timeout: 2000
            });
        }
    });
    $("#reassign-validation-message").hide();
    $("#modal-reassign-ticket").css("z-index", "1100");
    $("#modal-reassign-ticket").modal("show");
}

var generateTags = function () {
    $.ajax({
        url: "/HelpDesk/ManageTicket/GetTags",
        type: "GET",
        dataType: "json",
        data: {
            subject: $("[name='Subject']").val()
        },
        success: function (data) {
            for (i = 0; i < data.data.length; i++) {
                $('#tags').tagit('createTag', data.data[i]);
            }
        }
    });
}

function loadKeywordToTags(keyword) {
    var keywordTags = (keyword).split(',');
    var content = '<ul class="keywords">';
    for (j = 0; j < keywordTags.length; j++) {
        if (keywordTags[j]) {
            var key = keywordTags[j].replace(/"/g, '');
            content += '<li><div class="key-tags">' + key + '</div></li>';
        }
    }
    content += '</ul>';
    return content;
}