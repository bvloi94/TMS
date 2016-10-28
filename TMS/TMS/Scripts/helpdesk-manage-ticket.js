var ticketTable = null;
var cancelTicketId = null;
var closeTicketId = null;
var reassignTicketId = null;
var selectedTickets = [];

function initTicketTable() {
    ticketTable = $("#ticket-table").DataTable({
        serverSide: true,
        processing: true,
        sort: true,
        filter: false,
        lengthMenu: [7],
        order: [[6, 'des']],
        lengthChange: false,
        fnDrawCallback: function () {
            checkSelectedCheckbox();
        },
        ajax: {
            "url": "/HelpDesk/ManageTicket/LoadAllTickets",
            "type": "POST",
            "data": function (d) {
                d.status_filter = $("#status-dropdown").val();
                d.search_text = $("#search-txt").val();
            }
        },
        columnDefs: [
        {
            "targets": [0],
            "sortable": false,
            "render": function (data, type, row) {
                return '<input type="checkbox" data-role="cbo-ticket" data-id="' + row.Id + '" data-requester="' + row.Requester + '"/>';
            }
        },
             {
                 "targets": [1],
                 "render": function (data, type, row) {
                     //var url = '@Url.Action("Edit","ManageTicket")?id=' + row.Id;
                     return $("<a/>",
                     {
                         "href": "javascript:openTicketDetailModal(" + row.Id + ")",
                         "html": row.Subject
                     })[0].outerHTML;
                 }
             },
            {
                "targets": [2],
                "render": function (data, type, row) {
                    return row.Requester != "" ? row.Requester : "-";
                }
            },
            {
                "targets": [3],
                "sortable": false,
                "render": function (data, type, row) {
                    return row.Technician != "" ? row.Technician : "-";
                }

            },
            {
                "targets": [4],
                "sortable": false,
                "render": function (data, type, row) {
                    return row.SolvedDate != "" ? row.SolvedDate : "-";
                }
            }, {
                "targets": [5],
                "render": function (data, type, row) {
                    return getStatusLabel(row.Status);
                }
            },
            {
                "targets": [6],
                "render": function (data, type, row) {
                    return row.ModifiedTime != "" ? row.ModifiedTime : "-";
                }
            },
            {
                "targets": [7],
                "sortable": false,
                "render": function (data, type, row) {
                    //var url = '@Url.Action("Edit","ManageTicket")?id=' + row.Id;
                    var ediBtn;
                    switch (row.Status) {
                        case "Closed":
                        case "Canceled":
                        case "Solved":
                            ediBtn = $("<a/>",
                            {
                                "class": "btn btn-sm btn-default",
                                "data-role": "btn-edit-ticket",
                                "disabled": "disabled",
                                "html": $("<i/>",
                                {
                                    "class": "fa fa-pencil"
                                }),
                                "data-id": row.Id
                            });
                            break;
                        case "New":
                        case "Assigned":
                        case "Unapproved":
                            ediBtn = $("<a/>",
                            {
                                "class": "btn btn-sm btn-default",
                                "href": "/HelpDesk/ManageTicket/EditTicket?id=" + row.Id,
                                "data-role": "btn-edit-ticket",
                                "html": $("<i/>",
                                {
                                    "class": "fa fa-pencil"
                                }),
                                "data-id": row.Id
                            });
                            break;
                    }

                    var solveBtn;
                    switch (row.Status) {
                        case "Solved":
                        case "Closed":
                        case "Canceled":
                            solveBtn = $("<a/>",
                            {
                                "class": "btn btn-sm btn-default margin-left10",
                                //"data-role": "btn-show-solve-modal",
                                "disabled": "disabled",
                                "html": "Solve",
                                "data-id": row.Id
                            });
                            break;
                        case "New":
                        case "Assigned":
                        case "Unapproved":
                            solveBtn = $("<a/>",
                            {
                                "class": "btn btn-sm btn-default margin-left10",
                                "href": "/Ticket/Solve/" + row.Id,
                                "html": "Solve",
                                "data-id": row.Id
                            });
                            break;
                    }

                    var cancelBtn;
                    switch (row.Status) {
                        case "Solved":
                        case "Closed":
                        case "Canceled":
                        case "Unapproved":
                            cancelBtn = $("<a/>",
                            {
                                "class": "btn btn-sm btn-default margin-left10",
                                "data-role": "btn-show-cancel-modal",
                                "html": "Cancel",
                                "disabled": "disabled",
                                "data-ticket-id": row.Id
                            });
                            break;
                        case "New":
                        case "Assigned":
                            cancelBtn = $("<a/>",
                            {
                                "class": "btn btn-sm btn-default margin-left10",
                                "data-role": "btn-show-cancel-modal",
                                "html": "Cancel",
                                "data-ticket-id": row.Id
                            });
                            break;
                    }

                    return ediBtn[0].outerHTML + solveBtn[0].outerHTML + cancelBtn[0].outerHTML;
                }
            }
        ],
        "oLanguage": {
            "sInfo": "Found _TOTAL_ tickets",
            "sLast": "Last page",
            "sFirst": "First",
            "sSearch": "Search:",
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
    $.ajax({
        url: '/Ticket/GetTicketDetail',
        type: "GET",
        dataType: "json",
        data: {
            id: ticketId
        },
        success: function (data) {
            $('#ticket-subject').text(data.subject);
            $('#ticket-description').text(data.description);
            $('#ticket-department').text(data.department);
            $('#ticket-technician').text(data.technician);
            $('#ticket-created-by').text(data.creater);
            $('#ticket-assigned-by').text(data.assigner);
            $('#ticket-solved-by').text(data.solver);
            $('#ticket-type').text(data.type);
            $('#ticket-mode').text(data.mode);
            $('#ticket-urgency').text(data.urgency);
            $('#ticket-priority').text(data.priority);
            $('#ticket-category').text(data.category);
            $('#ticket-impact').text(data.impact);
            $('#ticket-impact-detail').text(data.impactDetail);
            $("#reopen-close-btn").attr("data-ticket-id", data.id);
            $("#reopen-resolve-btn").attr("href", "/Ticket/Solve/" + data.id);
            $("#reopen-reassign-btn").attr("data-ticket-id", data.id);
            $("#refer-older-ticket-btn").attr("data-id", data.id);

            if (!data.solution || data.solution == "-") {
                $('#ticket-solution').text("This ticket is not solved yet.");
            }
            else {
                $('#ticket-solution').text(data.solution);
            }

            if (!data.attachments || data.attachments == "") {
                $('#ticket-attachment').text("-");
            } else {
                $('#ticket-attachment').text(data.attachments);
            }


            $('[data-role="modal-btn-solve"]').attr("href", "/Ticket/Solve/" + data.id);

            if (data.status == 1) {
                $('#ticket-status').html(getStatusLabel('New'));
                $('[data-role="modal-btn-solve"]').removeClass("disabled");
                $("#reopen-div").hide();
            } else if (data.status == 2) {
                $('#ticket-status').html(getStatusLabel('Assigned'));
                $('[data-role="modal-btn-solve"]').removeClass("disabled");
                $("#reopen-div").hide();
            } else if (data.status == 3) {
                $('#ticket-status').html(getStatusLabel('Solved'));
                $('[data-role="modal-btn-solve"]').addClass("disabled");
                $("#reopen-div").hide();
            } else if (data.status == 4) {
                $('#ticket-status').html(getStatusLabel('Unapproved'));
                $('[data-role="modal-btn-solve"]').addClass("invisible");
                $("#reopen-div").show();
            } else if (data.status == 5) {
                $('#ticket-status').html(getStatusLabel('Cancelled'));
                $('[data-role="modal-btn-solve"]').addClass("disabled");
                $("#reopen-div").hide();
            } else if (data.status == 6) {
                $('#ticket-status').html(getStatusLabel('Closed'));
                $('[data-role="modal-btn-solve"]').addClass("disabled");
                $("#reopen-div").hide();
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
        },
        failure: function (data) {
            alert(data.d);
        }
    });
}

$(document)
        .ready(function () {

            setActiveTicketMenu();
            initTicketTable();

            $("#search-txt").keyup(function () {
                ticketTable.draw();
            });

            $("#status-dropdown").change(function () {
                ticketTable.draw();
            });

            $('#ticket-table tbody')
                .on('click',
                    'a[data-role="btn-show-cancel-modal"]:not([disabled])',
                    function () {
                        cancelTicketId = this.getAttribute("data-ticket-id");
                        $("#modal-cancel-ticket").modal("show");
                    });

            $("[data-role='btn-confirm-cancel']")
                .on('click',
                    function () {
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
                                    layout: "topCenter",
                                    timeout: 2000
                                });
                            }
                        });
                    });

            $("a[data-role='btn-merge-ticket'")
                .on("click",
                    function () {
                        if (selectedTickets.length < 2) {
                            noty({
                                text: "Less than 2 tickets, can not merge!",
                                type: "error",
                                layout: "topCenter",
                                timeout: 2000
                            });
                        } else {
                            $("#modal-merge-ticket").modal("show");
                        }

                    });

            $("[data-role='btn-confirm-merge']")
                .on('click',
                    function () {
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
                                        layout: "topCenter",
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
                                    layout: "topCenter",
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

            $('#detail-modal').on('click', 'a[data-role="btn-show-close-modal"]', function () {
                closeTicketId = this.getAttribute("data-ticket-id");
                $("#modal-close-ticket").css("z-index", "1100");
                $("#modal-close-ticket").modal("show");
            });

            $('#detail-modal').on('click', 'a[data-role="btn-show-reassign-modal"]', function () {
                reassignTicketId = $(this).attr("data-ticket-id");
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
                                $("#modal-reassign-ticket").modal("hide");
                                $("#detail-modal").modal("hide");
                                ticketTable.draw();
                            } else {
                                noty({
                                    text: data.message,
                                    layout: "topRight",
                                    type: "error",
                                    timeout: 2000
                                });
                                $("#modal-reassign-ticket").modal("hide");
                                $("#detail-modal").modal("hide");
                                ticketTable.draw();
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
                    return (row.Description == "") ? "-" : '<span class="description-text">' + row.Description + '</span>';
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
        url: "/HelpDesk/ManageTicket/LoadTicketById",
        type: "GET",
        dataType: "json",
        data: {
            id: ticketId
        },
        success: function (data) {
            if (data.success) {
                var ticket = data.data;
                $("[name='Subject']").val(ticket.Subject);
                $("[name='Description']").val(ticket.Description);
                if (ticket.Type != 0) {
                    $("[name='Type']").val(ticket.Type);
                }
                $("[name='ImpactDetail']").val(ticket.ImpactDetail);
                $("[name='ScheduleStartDate']").val(ticket.ScheduleStartDate);
                $("[name='ScheduleEndDate']").val(ticket.ScheduleEndDate);
                $("[name='ActualStartDate']").val(ticket.ActualStartDate);
                $("[name='ActualEndDate']").val(ticket.ActualEndDate);
                $("[name='Solution']").val(ticket.Solution);

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