var ticketTable = null;

function addFilterParams(aoData) {
    //aoData.push({
    //    "name": "schoolYearId",
    //    "value": $("[data-role=txt-school-year-filter]").val()
    //});

}

function initTicketTable() {
    //ticketTable = $("#ticket-table")
    //    .DataTable({
    //        processing: true,
    //        serverSide: true,
    //        paging: true,
    //        sort: true,
    //        ajax: {
    //            url: "/HelpDesk/ManageTicket/GetTickets"
    //        },
    //        "columnDefs": [
    //            {
    //                "render": function (data, type, row) {
    //                    return row[0];
    //                },
    //                "targets": 0
    //            },
    //            {
    //                "render": function (data, type, row) {
    //                    return row[1];
    //                },
    //                "targets": 1
    //            },
    //            {
    //                "render": function (data, type, row) {
    //                    return row[2];
    //                },
    //                "targets": 2
    //            },
    //            {
    //                "render": function (data, type, row) {
    //                    return row[3];
    //                },
    //                "targets": 3
    //            },
    //            {
    //                "render": function (data, type, row) {
    //                    return row[4];
    //                },
    //                "targets": 4
    //            },
    //            {
    //                "render": function (data, type, row) {
    //                    return row[5];
    //                },
    //                "targets": 5
    //            },
    //            {
    //                "render": function (data, type, row) {
    //                    return row[6];
    //                },
    //                "targets": 6,
    //                "type": "date"
    //            },
    //            {
    //                "render": function (data, type, row) {
    //                    var url = '@Url.Action("Edit", "ManageTicket")/' + row[7];
    //                    return "<a href='" +
    //                        url +
    //                        "'  class='btn btn-xs btn-primary btn-edit-ticket' ><i class='fa fa-pencil'></i></a>";
    //                },
    //                "targets": 7
    //            }
    //        ]
    //    });

    table = $("#ticket-table").dataTable({
        "bServerSide": true,
        "sAjaxSource": "/HelpDesk/ManageTicket/LoadAllTickets",
        "sAjaxDataProp": "aaData",
        "fnServerParams": addFilterParams,
        "aoColumnDefs": [
        {
            "aTargets": [0],
            "mRender": function (data, type, row) {
                return '<input type="checkbox" data-role="cbo-ticket" data-id="' + row.Id + '" data-requester="' + row.Requester + '"/>';
            }
        },
            {
                "aTargets": [1],
                "mData": "No"
            },
             {
                 "aTargets": [2],
                 "mData": "Subject"
             },
            {
                "aTargets": [3],
                "mData": "Requester"
            },
            {
                "aTargets": [4],
                "mData": "AssignedTo"
            },
            {
                "aTargets": [5],
                "mData": "Department"
            },
            {
                "aTargets": [6],
                "mData": "SolvedDate"
            }, {
                "aTargets": [7],
                "mRender": function (data, type, row) {
                    var cssClass = "";
                    switch (row.Status) {
                        case "Open":
                            cssClass = "label-info";
                            break;
                        case "Assigned":
                            cssClass = "label-danger";
                            break;
                        case "Solved":
                            cssClass = "label-success";
                            break;
                        case "Close":
                            cssClass = "label-warning";
                            break;
                    }

                    var lbl = $("<small/>",
                    {
                        "class": "label " + cssClass,
                        "html": row.Status
                    });

                    return lbl[0].outerHTML;
                }
            },
            {
                "aTargets": [8],
                "mData": "CreatedTime"
            },
            {
                "aTargets": [9],
                "mRender": function (data, type, row) {
                    //var url = '@Url.Action("Edit","ManageTicket")?id=' + row.Id;
                    var ediBtn = $("<a/>",
                    {
                        "class": "btn btn-sm btn-primary",
                        "href": "/HelpDesk/ManageTicket/Edit?id=" + row.Id,
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
                                "class": "btn btn-sm btn-danger margin-left10",
                                "data-role": "btn-show-solve-modal",
                                "html": "Solve",
                                "disabled": "disabled",
                                "data-id": row.Id
                            });
                            break;
                        case "Open":
                        case "Assigned":
                            solveBtn = $("<a/>",
                            {
                                "class": "btn btn-sm btn-danger margin-left10",
                                "data-role": "btn-show-solve-modal",
                                "html": "Solve",
                                "data-id": row.Id
                            });
                            break;
                    }

                    return ediBtn[0].outerHTML + solveBtn[0].outerHTML;
                },
                "mData": "Id"
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

$(document)
        .ready(function () {
            setActiveTicketMenu();
            initTicketTable();

            $("a[data-role='btn-merge-ticket'")
                .on("click",
                    function () {

                    });
            var i = 1;
            $('#ticket-table tbody').on('click', 'input[data-role="cbo-ticket"]', function (e) {
                alert($(this).data("requester"));
            });
        });
