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

$(document).ready(function () {

    $(".datetime").datetimepicker({
        timepicker: false,
        format: 'd/m/Y'
    });
});
