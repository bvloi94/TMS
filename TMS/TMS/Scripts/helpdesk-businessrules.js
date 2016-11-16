var tree = $('#criteriaeditordiv').jstree({
    "core": {
        "animation": 0,
        "check_callback": true,
        "themes": {
            "dots": false,
            "icons": false
        }
    },
    "plugins": [
        "dnd", "crrm"
    ]
});

function initDropdownControl() {
    initTechnicianDropdown({
        control: $("[data-role=ddl-technician]"),
        ignore: function () {
            return $("[data-role=ddl-technician]").val();
        }
    });

    initActionDropdown({
        control: $("#rule-action")
    });

}

function resetLogicDisplay() {
    var nodeIds = tree.jstree().get_node("#").children_d;
    var anchorTag;
    var nodeId;
    var node;
    for (var i = 0; i < nodeIds.length; i++) {
        nodeId = nodeIds[i];
        node = tree.jstree().get_node(nodeId);
        anchorTag = $("<a/>").append(node.text)[0];
        if (anchorTag.children[1].className.indexOf("invisible") != -1) {
            anchorTag.children[1].className = anchorTag.children[1].className.replace(" invisible", "");
        }
        tree.jstree().set_text(nodeId, anchorTag.innerHTML);
    }
    nodeId = tree.jstree().get_node("#").children[0];
    node = tree.jstree().get_node(nodeId);
    anchorTag = $("<a/>").append(node.text)[0];
    if (anchorTag.children[1].className.indexOf("invisible") == -1) {
        anchorTag.children[1].className += " invisible";
    }
    tree.jstree().set_text(nodeId, anchorTag.innerHTML);
}

tree.on('move_node.jstree', function (e, data) {
    resetLogicDisplay();
    tree.jstree().deselect_all(true);
    tree.jstree().select_node("#" + data.parent);
    tree.jstree().open_node("#" + data.parent);
});

tree.on('create_node.jstree', function (e, data) {
    var no = data.node.id.split('_').pop();
    var nodeData = data.node.data;
    if (typeof (nodeData.LogicText) == "undefined") {
        nodeData.Logic = 1;
        nodeData.LogicText = 'AND';
    }
    if (typeof (nodeData.CriteriaText) == "undefined") {
        nodeData.Criteria = 0;
        nodeData.CriteriaText = '-- Select criteria --';
    }
    if (typeof (nodeData.ConditionText) == "undefined") {
        nodeData.Condition = 0;
        nodeData.ConditionText = '-- Select condition --';
    }
    if (typeof (nodeData.ValueMask) == "undefined") {
        nodeData.Value = null;
        nodeData.ValueMask = '';
    }
    var nodeText = '<span><i class=\"fa fa-arrows draggable\" style=\"color: #aaa;\"></i></span>' +
        '<span id=\"rule_logic_' + no + '\" class=\"logicalcss\">' + nodeData.LogicText + '</span>' +
        '<span id=\"rule_criteria_' + no + '\" class=\"fieldcss\">' + nodeData.CriteriaText + '</span>' +
        '<span id=\"rule_condition_' + no + '\" class=\"operatorcss\" >' + nodeData.ConditionText + '</span>' +
        '<span id=\"rule_value_' + no + '\"  class="valuecss">' + nodeData.ValueMask + '</span>' +
        '<span class=\"pull-right\"><i id=\"rule_remove_' + no + '\" class=\"fa fa-remove addcss\" style="color: #aaa;\"></i></span>' +
        '<span class=\"pull-right\"><i id=\"rule_add_' + no + '\" class=\"fa fa-plus removecss\" style="color: #aaa;\"></i></span>';
    tree.jstree().get_node(data.node.id).data = nodeData;
    tree.jstree().set_text(data.node.id, nodeText);
});

tree.on("select_node.jstree", function (event, data) {

    var evt = window.event || event;
    var clickedId = evt.target.id;
    var selectedId = '#' + tree.jstree('get_selected')[0];

    var anchorTag = $("<a/>").append(data.node.text)[0];

    if (clickedId.indexOf("rule_logic") != -1) {
        var logic = "AND";
        if ($('#' + clickedId)[0].innerHTML == "AND") {
            logic = "OR";
        }
        anchorTag.children[1].innerHTML = logic;
        tree.jstree().get_node(selectedId).data.Logic = (logic == "OR") ? 2 : 1;
        tree.jstree().get_node(selectedId).data.LogicText = (logic == "OR") ? "OR" : "AND";
        tree.jstree().set_text(data.node.id, anchorTag.innerHTML);
    }

    if (clickedId.indexOf("rule_add") != -1) {
        var id = tree.jstree().create_node(data.node.parent, { "data": {} }, "last");
    }

    if (clickedId.indexOf("rule_remove") != -1) {
        var treeData = tree.jstree()
            .get_json('#', { no_a_attr: true, no_li_attr: true, no_state: true, flat: true });
        if (treeData.length > 1) {
            var deleted = tree.jstree().delete_node(data.node);
            resetLogicDisplay();
        }
    }

    if (clickedId.indexOf("rule_criteria") != -1) {
        var inputId = clickedId + '_inp';
        var p = $("#" + clickedId);
        p.addClass('list-open');
        $('#rulecriteria_fs').append('<div id="tempSelectCriteria" class="tempDiv select-sublinks-single" ' +
                'style="top:' + p[0].offsetTop + 'px; left:' + p[0].offsetLeft + 'px; ">' +
                '<select id="' + inputId + '"></select></div>');
        initCriteriaDropdown({
            control: $("#" + inputId),
            ignore: function () {
                return [];
            }
        });
        $('b[role="presentation"]').hide();
        $('#tempSelectCriteria .select2-selection__rendered').hide();
        $("#" + inputId).select2("open");
        $("#" + inputId).on("change", function () {
            anchorTag.children[2].innerHTML = $("#" + inputId).text();
            tree.jstree().get_node(selectedId).data.Criteria = $("#" + inputId).val();
            tree.jstree().get_node(selectedId).data.CriteriaText = $("#" + inputId).text();
            anchorTag.children[3].innerHTML = "-- Select condition --";
            anchorTag.children[4].innerHTML = "";
            tree.jstree().set_text(data.node.id, anchorTag.innerHTML);
            $('.tempDiv').remove();
        });
    }

    if (clickedId.indexOf("rule_condition") != -1) {
        var criteria = anchorTag.children[2].innerHTML;
        if (criteria == "-- Select criteria --") {
            alert("Please select the criteria first.");
        } else {
            var inputId = clickedId + '_inp';
            var p = $("#" + clickedId);
            p.addClass('list-open');
            $('#rulecriteria_fs').append('<div id="tempSelectCondition" class="tempDiv select-sublinks-single" ' +
                    'style="top:' + p[0].offsetTop + 'px; left:' + p[0].offsetLeft + 'px; ">' +
                    '<select id="' + inputId + '"></select></div>');
            initConditionDropdown({
                control: $("#" + inputId),
                criteria: criteria,
                ignore: function () {
                    return [];
                }
            });
            $('b[role="presentation"]').hide();
            $('#tempSelectCondition .select2-selection__rendered').hide();
            $("#" + inputId).select2("open");
            $("#" + inputId).on("change", function () {
                anchorTag.children[3].innerHTML = $("#" + inputId).text();
                tree.jstree().get_node(selectedId).data.Condition = $("#" + inputId).val();
                tree.jstree().get_node(selectedId).data.ConditionText = $("#" + inputId).text();
                anchorTag.children[4].innerHTML = "";
                tree.jstree().set_text(data.node.id, anchorTag.innerHTML);
                $('.tempDiv').remove();
            });
        }
    }

    if (clickedId.indexOf("rule_value") != -1) {
        $('.tempDiv').remove();
        var criteria = anchorTag.children[2].innerHTML;
        var condition = anchorTag.children[3].innerHTML;
        if (criteria == "-- Select criteria --") {
            alert("Please select the criteria first.");
        } else if (condition == "-- Select condition --") {
            alert("Please select the condition.");
        } else {
            var inputId = clickedId + '_inp';
            var p = $("#" + clickedId);
            if (criteria == "Subject" || criteria == "Description") {
                $('#rulecriteria_fs').append('<div id="tempSelectValue" class="tempDiv select-sublinks-single" ' +
                        'style="top:' + p[0].offsetTop + 'px; left:' + p[0].offsetLeft + 'px; ">' +
                        '<input style="width:200px; font-size: 14px;" id="' + inputId + '"></input></div>');
                if (tree.jstree().get_node(selectedId).data.Value) {
                    $("#" + inputId).val(tree.jstree().get_node(selectedId).data.Value);
                }
                $("#" + inputId).focus();
                $("#" + inputId)
                    .focusout(function () {
                        anchorTag.children[4].innerHTML = $("#" + inputId)[0].value;
                        tree.jstree().get_node(selectedId).data.Value = $("#" + inputId)[0].value;
                        tree.jstree().set_text(data.node.id, anchorTag.innerHTML);
                        $('#tempSelectValue').remove();
                    });
            } else {
                var top = p[0].offsetTop + 24;
                $('#rulecriteria_fs').append('<div id="tempSelectValue" class="tempDiv select-sublinks-single" ' +
                        'style="top:' + top + 'px; left:' + p[0].offsetLeft + 'px; ">' +
                        '<select data-role="rule_value_select" multiple="multiple" id="' + inputId + '"></select></div>');
                switch (criteria) {
                    case "Requester Name":
                        initRequesterDropdown({
                            control: $("#" + inputId),
                            ignore: function () {
                                return []; //$("#" + inputId).val();
                            }
                        });
                        break;
                    case "Department":
                    case "Priority":
                    case "Impact":
                    case "Urgency":
                    case "Mode":
                        initConditionValueDropdown({
                            control: $("#" + inputId),
                            criteria: criteria,
                            ignore: function () {
                                return [];// $("#" + inputId).val();
                            }
                        });
                        break;
                    case "Category":
                        initCategoryDropdown({
                            control: $("#" + inputId),
                            ignore: function () {
                                return [];//$("#" + inputId).val();
                            }
                        });
                        break;

                }

                if (anchorTag.children[4].innerHTML != "") {
                    var masks = anchorTag.children[4].innerHTML.split(", ");
                    var values = tree.jstree().get_node(selectedId).data.Value.split(",");
                    for (var i = 0; i < masks.length; i++) {
                        loadInitDropdown("rule_value_select", masks[i], values[i]);
                        //initDropdown($("#" + inputId), masks[i], values[i]);
                    }
                }
                $("#" + inputId).select2("open");
                $("#" + inputId).on("select2:close", function () {
                    tree.jstree().set_text(data.node.id, anchorTag.innerHTML);
                    $('#tempSelectValue').remove();
                });
                $("#" + inputId).change(function () {
                    var a = $("#" + inputId).select2('data');
                    var text = "";
                    var val = "";
                    if (a != null && a.length > 0) {
                        text = a[0].text;
                        val = a[0].id;
                        if (a.length > 1)
                            for (var i = 1; i < a.length; i++) {
                                text += ", " + a[i].text;
                                val += "," + a[i].id;
                            }
                    }
                    p[0].innerHTML = text;
                    anchorTag.children[4].innerHTML = text;
                    tree.jstree().get_node(selectedId).data.Value = val;
                    tree.jstree().get_node(selectedId).data.ValueMask = text;
                    window.dispatchEvent(new Event('resize'));
                });
            }
        }
    }
});

$(document).bind("dnd_start.vakata", function (e, data) {
    //$('#tempSelectValue').select2('destroy');
    //$('#tempSelectValue').remove();
    //$('.select2-container').remove();
}).bind("dnd_move.vakata", function (e, data) {
    //
}).bind("dnd_stop.vakata", function (e, data) {
    //
});

function onSelectBRActionChange() {
    switch ($("#rule-action").val()) {
        case "1":
            initTechnicianDropdown({
                control: $("#rule-action-select"),
                ignore: function () {
                    return [];
                }
            });
            break;
        case "2":
            initCategoryDropdownByLevel({
                control: $("#rule-action-select"),
                ignore: function () {
                    return [];
                },
                level: 1
            });
            break;
        case "3":
            initCategoryDropdownByLevel({
                control: $("#rule-action-select"),
                ignore: function () {
                    return [];
                },
                level: 2
            });
            break;
        case "4":
            initCategoryDropdownByLevel({
                control: $("#rule-action-select"),
                ignore: function () {
                    return [];
                },
                level: 3
            });
            break;
        case "5":
            initPriorityDropdown({
                control: $("#rule-action-select"),
                ignore: function () {
                    return [];
                }
            });
            break;
    }
}

$("#add-action").click(function () {
    if ($("#rule-action").val() == null) alert("Please choose action.");
    else if ($("#rule-action-select").val() == null) alert("Please set value for action.");
    else {
        var actionSet = $("#rule-action").select2('data')[0].text + ' \"' +
            $("#rule-action-select").select2('data')[0].text + '\"';
        var actionKey = $("#rule-action").val();
        var actionValue = $("#rule-action-select").val();
        $('#action-table tr:last').after('<tr class="actionSet" ' +
            'data-id=' + actionKey + ' data-value=' + actionValue + '>' +
            '<td><i class="fa fa-trash remove-action"></i></td><td>' + actionSet + '</td></tr>');
        //$("#rule-action").val('').trigger("change");
        $("#rule-action-select").val('').trigger("change");
        //$("#rule-action-select").children().remove();
        //$("#rule-action-select").select2('destroy');
    }
});

$('#action-table').on('click', 'tr i.remove-action', function (e) {
    $(this).closest('tr').remove();
});

$("#submitBtn").click(function () {
    $("#submitBtn").prop("disabled", true);
    $("#cancelBtn").prop("disabled", true);
    var ruleName = $("#rule-name").val().trim();
    if (!ruleName) {
        noty({
            text: "Please enter rule name!",
            type: "error",
            layout: "topRight",
            timeout: 2000
        });
        return;
    } else {
        if (ruleName.length > 100) {
            noty({
                text: "Rule name must be under 100 characters!",
                type: "error",
                layout: "topRight",
                timeout: 2000
            });
            return;
        }
    }
    var ruleDescription = $("#rule-description").val();
    var ruleTechnicians = $("#ddl-technician").val();

    // rule
    var treeData = tree.jstree().get_json('#', { no_a_attr: true, no_li_attr: true, no_state: true, flat: true });
    for (var i = 0; i < treeData.length; i++) {
        delete treeData[i].text;
        delete treeData[i].icon;
        if (treeData[i].Condition == 0 || treeData[i].Criteria == 0 || treeData[i].Value == null) {
            noty({
                text: "The rule has not been finish yet! \n Please check again.",
                type: "error",
                layout: "topRight",
                timeout: 2000
            });
            return;
        }
    };

    //action
    var actionSet = [];
    $(".actionSet")
        .each(function (index, element) {
            var actionId = element.getAttribute("data-id");
            var actionValue = element.getAttribute("data-value");
            actionSet.push({ id: actionId, value: actionValue });
        });
    var url = $(this).attr("data-url");
    $.ajax({
        "url": "/BusinessRule/" + url,
        "method": "POST",
        "data": {
            Id: $("#busnessruleId").val(),
            Name: ruleName,
            Description: ruleDescription,
            Enable: $("#enableRule").prop('checked'),
            Conditions: JSON.stringify(treeData),
            Actions: JSON.stringify(actionSet),
            Technicians: JSON.stringify(ruleTechnicians)
        },
        "success": function (data) {
            if (data.success) {
                noty({
                    text: "Business rule was " + (url == "Update" ? "updated" : "created") + " successfully!",
                    layout: "topCenter",
                    type: "success",
                    timeout: 300,
                    callback: {
                        onClose: function () {
                            window.location.href = "/HelpDesk/BusinessRule/";
                        }
                    }
                });
            } else {
                $("#submitBtn").prop("disabled", false);
                $("#cancelBtn").prop("disabled", false);
                noty({
                    text: "Some error occur! Please try again later!",
                    type: "error",
                    layout: "topRight",
                    timeout: 2000
                });
            }
        },
        "error": function () {
            $("#submitBtn").prop("disabled", false);
            $("#cancelBtn").prop("disabled", false);
            noty({
                text: "Cannot connect to server!",
                type: "error",
                layout: "topRight",
                timeout: 2000
            });
        }
    });
});
