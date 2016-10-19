﻿var ticketTable = null;
var cancelTicketId = null;

function initTicketTable() {
    ticketTable = $("#ticket-table").DataTable({
        serverSide: true,
        processing: true,
        sort: true,
        filter: false,
        lengthMenu: [8],
        "order": [[1, 'asc']],
        lengthChange: false,
        ajax: {
            "url": "/HelpDesk/ManageTicket/LoadAllTickets",
            "type": "POST",
            "data": function (d) {
                d.status_filter = $('#status-dropdown').val();
                d.search_text = $("#search-txt").val();
            }
            //"data": {
            //    "status_filter": $("#status-dropdown").val(),
            //    "search_text": $("#search-txt").val()
            //}
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
                "sortable": false,
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
                "data": "SolvedDate"
            }, {
                "targets": [5],
                "render": function (data, type, row) {
                    return getStatusLabel(row.Status);
                }
            },
            {
                "targets": [6],
                "data": "ModifiedTime"
            },
            {
                "targets": [7],
                "sortable": false,
                "render": function (data, type, row) {
                    //var url = '@Url.Action("Edit","ManageTicket")?id=' + row.Id;
                    var ediBtn = $("<a/>",
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

                    var solveBtn;
                    switch (row.Status) {
                        case "Solved":
                        case "Closed":
                        case "Canceled":
                            solveBtn = $("<a/>",
                            {
                                "class": "btn btn-sm btn-default margin-left10",
                                //"data-role": "btn-show-solve-modal",
                                "html": "Solve",
                                "disabled": "disabled",
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
                            cancelBtn = $("<a/>",
                            {
                                "class": "btn btn-sm btn-default margin-left10",
                                "data-role": "btn-show-cancel-modal",
                                "html": "Cancel",
                                "disabled": "disabled",
                                "data-tickeId": row.Id
                            });
                            break;
                        case "New":
                        case "Assigned":
                        case "Unapproved":
                            cancelBtn = $("<a/>",
                            {
                                "class": "btn btn-sm btn-default margin-left10",
                                "data-role": "btn-show-cancel-modal",
                                "html": "Cancel",
                                "data-tickeId": row.Id
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
    var checked = false;
    $('input[data-role="cbo-ticket"]')
        .each(function (index) {
            if (this.checked) {
                checked = true;
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
            var solveUrl = '/Ticket/Solve/' + data.id;

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

            if (!data.solution || data.solution == "-") {
                $('#ticket-solution').text("This ticket is not solved yet.");
            }
            else {
                $('#ticket-solution').text(data.solution);
            }

            if (data.status == 1) {
                $('#ticket-status').html(getStatusLabel('New'));
            } else if (data.status == 2) {
                $('#ticket-status').html(getStatusLabel('Assigned'));
            } else if (data.status == 3) {
                $('#ticket-status').html(getStatusLabel('Solved'));
            } else if (data.status == 4) {
                $('#ticket-status').html(getStatusLabel('Unapproved'));
            } else if (data.status == 5) {
                $('#ticket-status').html(getStatusLabel('Cancelled'));
            } else if (data.status == 6) {
                $('#ticket-status').html(getStatusLabel('Closed'));
            }

            $('#ticket-created-date').text(data.createdDate);
            $('#ticket-modified-date').text(data.lastModified);
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

function getSelectedTickets() {
    var selected = [];
    $('input[data-role="cbo-ticket"]').each(function () {
        if ($(this).is(":checked")) {
            selected.push($(this).data("id"));
        }
    });
    return selected;
}

$(document)
        .ready(function () {

            setActiveTicketMenu();
            initTicketTable();
            $("a[data-role='btn-merge-ticket']").addClass("disabled");

            $("#search-txt").keyup(function () {
                ticketTable.draw();
            });

            $("#status-dropdown").change(function () {
                ticketTable.draw();
            });

            $('#ticket-table tbody')
                .on('click',
                    'a[data-role="btn-show-cancel-modal"]',
                    function () {
                        cancelTicketId = this.getAttribute("data-tickeId");
                        $("#modal-cancel-ticket").modal("show");
                    });

            $("[data-role='btn-confirm-cancel']")
                .on('click',
                    function () {
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
                                        text: "Ticket was canceled!",
                                        layout: "top",
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
                                }
                            }
                        });
                    });

            $("a[data-role='btn-merge-ticket'")
                .on("click",
                    function () {
                        var selectedTickets = getSelectedTickets();
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
                        $.ajax({
                            "url": "/HelpDesk/ManageTicket/MergeTicket",
                            "method": "POST",
                            "data": {
                                selectedTickets: getSelectedTickets()
                            },
                            "success": function (data) {
                                if (data.success) {
                                    noty({
                                        text: "Ticket was merged!",
                                        type: "success",
                                        layout: "topCenter",
                                        timeout: 2000
                                    });
                                    ticketTable.draw();
                                    $("#modal-merge-ticket").modal("hide");
                                } else {
                                    noty({
                                        text: data.msg,
                                        type: "error",
                                        layout: "topCenter",
                                        timeout: 2000
                                    });
                                }
                            }
                        });
                    });

            $("[data-role='btn-cancel-merge']")
                .on('click',
                    function () {
                        $('input[data-role="cbo-ticket"]').each(function () {
                            $(this).prop("checked", false);
                        });
                        $("#modal-merge-ticket").modal("hide");
                        $("a[data-role='btn-merge-ticket']").addClass("disabled");
                    });

            $('#ticket-table tbody').on('click', 'input[data-role="cbo-ticket"]', function (e) {
                var selectedTickets = getSelectedTickets();
                if (selectedTickets.length < 2) {
                    $("a[data-role='btn-merge-ticket']").addClass("disabled");
                } else {
                    $("a[data-role='btn-merge-ticket']").removeClass("disabled");
                }
            });
        });
