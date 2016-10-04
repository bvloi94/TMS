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