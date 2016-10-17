var ticketTable = null;
var cancelTicketId = null;

function initTicketTable() {
    ticketTable = $("#ticket-table").DataTable({
        serverSide: true,
        processing: true,
        sort: true,
        filter: false,
        lengthMenu: [8],
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
                "sortable": false,
                "data": "No"
            },
             {
                 "targets": [2],
                 "data": "Subject"
             },
            {
                "targets": [3],
                "sortable": false,
                "data": "Requester"
            },
            {
                "targets": [4],
                "sortable": false,
                "data": "Technician"
            },
            {
                "targets": [5],
                "sortable": false,
                "data": "SolvedDate"
            }, {
                "targets": [6],
                "render": function (data, type, row) {
                    var lbl = getStatusLabel(row.Status);
                    return lbl[0].outerHTML;
                }
            },
            {
                "targets": [7],
                "data": "CreatedTime"
            },
            {
                "targets": [8],
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
                                "data-role": "btn-show-solve-modal",
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
                                "data-role": "btn-show-solve-modal",
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
            "sZeroRecords": "No compatible date",
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
            $('#ticket-table tbody').on('click', 'input[data-role="cbo-ticket"]', function (e) {
                var selectedTickets = getSelectedTickets();
                if (selectedTickets.length < 2) {
                    $("a[data-role='btn-merge-ticket']").addClass("disabled");
                } else {
                    $("a[data-role='btn-merge-ticket']").removeClass("disabled");
                }
            });
        });
