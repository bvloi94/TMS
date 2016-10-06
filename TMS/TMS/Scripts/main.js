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

function readURL(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#avatar-img').attr('src', e.target.result);
        };

        reader.readAsDataURL(input.files[0]);
    }
}