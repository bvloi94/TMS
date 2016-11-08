var generateKeywords = function () {
    $.ajax({
        url: "/KnowledgeBase/GetKeywords",
        type: "GET",
        dataType: "json",
        data: {
            subject: $("#subject-txt").val()
        },
        success: function (data) {
            for (i = 0; i < data.data.length; i++) {
                $('#keyword').tagit('createTag', data.data[i]);
            }
        }
    });
}

var getPath = function () {
    $("#path")
            .val($("#subject-txt")
                .val()
                .trim()
                .toLowerCase()
                .replace(/\s/g, '-')
                .replace(/\-{2,}/g, '-')
                .replace(/[^\w-]+/g, ''));
}