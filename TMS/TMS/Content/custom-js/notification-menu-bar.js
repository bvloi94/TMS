$(document).ready(function () {
    loadNotification();
    setInterval(loadNotification, 500);
});

function loadNotification() {
    $.ajax({
        url: "/Notification/GetNotifications",
        method: "GET",
        success: function (data) {
            var dataItems = data.data;
            if (dataItems.length > 0) {
                $("#notification-count").text(dataItems.length);
                $("#notification-list").empty();
                var url = "";
                if (data.userRole == "Requester") {
                    url = "/Ticket/Detail/";
                } else {
                    url = "/Ticket/TicketDetail/";
                }
                for (i = 0; i < dataItems.length; i++) {
                    $("#notification-list").append('<li><a class="notification-item text-blue" id="' + dataItems[i].Id + '" href="' + url + dataItems[i].TicketId + '">'
                        + '<div class="col-sm-12" style="word-wrap: break-word;"><i class="fa fa-tag"></i>&nbsp;&nbsp;' + dataItems[i].NotificationContent + '</div>'
                        + '<div class="col-sm-12 text-muted text-sm">' + dataItems[i].NotifiedTime + '</div></a></li>');
                }
            }
            else {
                $("#notification-count").text('');
            }
        }
    });
}

function setNotificationToRead(id) {
    $.ajax({
        url: "/Notification/SetNotificationToRead",
        method: "POST",
        data: {
            id: id
        },
        success: function (data) {
            loadNotification();
        }
    });
}

$("#notification-list").on("mouseup", ".notification-item", function () {
    var notiID = $(this).attr("id");
    setNotificationToRead(notiID);
});

$('.dropdown-toggle').on("click", function () {
    loadNotification();
});