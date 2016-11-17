var $actionSelect = $("#action_brselect");
var $actionValueSelect = $("#action_value_brselect");
var $technicianSelect = $("[data-role=ddl-technician]");

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
    initActionDropdown({
        control: $actionSelect
    });
    initTechnicianDropdown({
        control: $technicianSelect,
        ignore: function () {
            return $technicianSelect.val();
        }
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

    closeAllSelect2();

    var evt = window.event || event;
    var clickedFieldId = evt.target.id;
    var $clickedField = $("#" + clickedFieldId);
    var selectedNodeId = '#' + tree.jstree('get_selected')[0];
    var anchorNode = $("<a/>").append(data.node.text)[0];
    var criteria;
    var defaultCriteriaText = "-- Select criteria --";
    var condition;
    var defaultConditionText = "-- Select condition --";
    var tempInput;

    if (clickedFieldId.indexOf("rule_logic") != -1) {
        var logic = "AND";
        if ($clickedField[0].innerHTML == "AND") {
            logic = "OR";
        }
        anchorNode.children[1].innerHTML = logic;
        tree.jstree().get_node(selectedNodeId).data.Logic = (logic == "OR") ? 2 : 1;
        tree.jstree().get_node(selectedNodeId).data.LogicText = (logic == "OR") ? "OR" : "AND";
        tree.jstree().set_text(data.node.id, anchorNode.innerHTML);
    }

    if (clickedFieldId.indexOf("rule_add") != -1) {
        tree.jstree().create_node(data.node.parent, { "data": {} }, "last");
    }

    if (clickedFieldId.indexOf("rule_remove") != -1) {
        tree.jstree().delete_node(data.node);
        var treeData = tree.jstree().get_json('#', { no_a_attr: true, no_li_attr: true, no_state: true, flat: true });
        if (treeData.length == 0) {
            tree.jstree().create_node(data.node.parent, { "data": {} }, "last");
        }
        resetLogicDisplay();
    }

    if (clickedFieldId.indexOf("rule_criteria") != -1) {
        //$clickedField.addClass('list-open');
        $('#rulecriteria_fs').append('<div id="tempSelectCriteria" class="tempDiv select-sublinks-single" ' +
                'style="top:' + $clickedField[0].offsetTop + 'px; left:' + $clickedField[0].offsetLeft + 'px; ">' +
                '<select id="' + clickedFieldId + '_brselect"></select></div>');
        tempInput = $("#" + clickedFieldId + "_brselect");
        initCriteriaDropdown({
            control: tempInput,
            ignore: function () {
                return [];
            }
        });
        $('b[role="presentation"]').hide();
        $('#tempSelectCriteria .select2-selection__rendered').hide();
        tempInput.select2("open");
        tempInput.on("select2:close", function () {
            $('.tempDiv').remove();
        });
        tempInput.on("change", function () {
            anchorNode.children[2].innerHTML = tempInput.text();
            anchorNode.children[3].innerHTML = defaultConditionText;
            anchorNode.children[4].innerHTML = "";
            tree.jstree().get_node(selectedNodeId).data.Criteria = tempInput.val();
            tree.jstree().get_node(selectedNodeId).data.CriteriaText = tempInput.text();
            tree.jstree().get_node(selectedNodeId).data.Condition = 0;
            tree.jstree().get_node(selectedNodeId).data.ConditionText = defaultConditionText;
            tree.jstree().get_node(selectedNodeId).data.Value = null;
            tree.jstree().get_node(selectedNodeId).data.ValueMask = "";
            tree.jstree().set_text(data.node.id, anchorNode.innerHTML);
            $('.tempDiv').remove();
        });
    }

    if (clickedFieldId.indexOf("rule_condition") != -1) {
        criteria = anchorNode.children[2].innerHTML;
        if (criteria == defaultCriteriaText) {
            alert("Please select the criteria first.");
        } else {
            //$clickedField.addClass('list-open');
            $('#rulecriteria_fs').append('<div id="tempSelectCondition" class="tempDiv select-sublinks-single" ' +
                    'style="top:' + $clickedField[0].offsetTop + 'px; left:' + $clickedField[0].offsetLeft + 'px; ">' +
                    '<select id="' + clickedFieldId + '_brselect"></select></div>');
            tempInput = $("#" + clickedFieldId + "_brselect");
            initConditionDropdown({
                control: tempInput,
                criteria: criteria,
                ignore: function () {
                    return [];
                }
            });
            $('b[role="presentation"]').hide();
            $('#tempSelectCondition .select2-selection__rendered').hide();
            tempInput.select2("open");
            tempInput.on("select2:close", function () {
                $('.tempDiv').remove();
            });
            tempInput.on("change", function () {
                anchorNode.children[3].innerHTML = tempInput.text();
                anchorNode.children[4].innerHTML = "";
                tree.jstree().get_node(selectedNodeId).data.Condition = tempInput.val();
                tree.jstree().get_node(selectedNodeId).data.ConditionText = tempInput.text();
                tree.jstree().get_node(selectedNodeId).data.Value = null;
                tree.jstree().get_node(selectedNodeId).data.ValueMask = "";
                tree.jstree().set_text(data.node.id, anchorNode.innerHTML);
                $('.tempDiv').remove();
            });
        }
    }

    if (clickedFieldId.indexOf("rule_value") != -1) {
        criteria = anchorNode.children[2].innerHTML;
        condition = anchorNode.children[3].innerHTML;
        if (criteria == defaultCriteriaText) {
            alert("Please select the criteria first.");
        } else if (condition == defaultConditionText) {
            alert("Please select the condition.");
        } else {
            if (criteria == "Subject" || criteria == "Description") {
                $('#rulecriteria_fs').append('<div id="tempSelectValue" class="tempDiv select-sublinks-single" ' +
                        'style="top:' + $clickedField[0].offsetTop + 'px; left:' + $clickedField[0].offsetLeft + 'px; ">' +
                        '<input style="width:200px; font-size: 14px;" id="' + clickedFieldId + '_inp"></input></div>');
                tempInput = $('#' + clickedFieldId + '_inp');
                if (tree.jstree().get_node(selectedNodeId).data.Value) {
                    tempInput.val(tree.jstree().get_node(selectedNodeId).data.Value);
                }
                tempInput.focus();
                tempInput.focusout(function () {
                    anchorNode.children[4].innerHTML = tempInput[0].value;
                    tree.jstree().get_node(selectedNodeId).data.Value = tempInput[0].value;
                    tree.jstree().set_text(data.node.id, anchorNode.innerHTML);
                    $('.tempDiv').remove();
                });

            } else {
                var top = $clickedField[0].offsetTop + 24;
                $('#rulecriteria_fs').append('<div id="tempSelectValue" class="tempDiv select-sublinks-single" ' +
                        'style="top:' + top + 'px; left:' + $clickedField[0].offsetLeft + 'px; ">' +
                        '<select data-role="rule_value_select" multiple="multiple" id="' + clickedFieldId + '_brselect"></select></div>');
                tempInput = $('#' + clickedFieldId + '_brselect');
                switch (criteria) {
                    case "Requester Name":
                        initRequesterDropdown({
                            control: tempInput,
                            ignore: function () {
                                return []; //tempInput.val();
                            }
                        });
                        break;
                    case "Department":
                    case "Priority":
                    case "Impact":
                    case "Urgency":
                    case "Mode":
                        initConditionValueDropdown({
                            control: tempInput,
                            criteria: criteria,
                            ignore: function () {
                                return [];// tempInput.val();
                            }
                        });
                        break;
                    case "Category":
                        initCategoryDropdown({
                            control: tempInput,
                            ignore: function () {
                                return [];//tempInput.val();
                            }
                        });
                        break;
                }
                if (anchorNode.children[4].innerHTML != "") {
                    var masks = anchorNode.children[4].innerHTML.split(", ");
                    var values = tree.jstree().get_node(selectedNodeId).data.Value.split(",");
                    for (var i = 0; i < masks.length; i++) {
                        loadInitDropdown("rule_value_select", masks[i], values[i]);
                        //initDropdown(tempInput, masks[i], values[i]);
                    }
                }
                tempInput.select2("open");
                tempInput.on("select2:close", function () {
                    tree.jstree().set_text(data.node.id, anchorNode.innerHTML);
                    $('.tempDiv').remove();
                });
                tempInput.change(function () {
                    var a = tempInput.select2('data');
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
                    $clickedField[0].innerHTML = text;
                    anchorNode.children[4].innerHTML = text;
                    tree.jstree().get_node(selectedNodeId).data.Value = val;
                    tree.jstree().get_node(selectedNodeId).data.ValueMask = text;
                    var $search = tempInput.data('select2').dropdown.$search || tempInput.data('select2').selection.$search;
                    $search.val("");
                    $search.trigger('keydown');
                    $search.focus();
                    //window.dispatchEvent(new Event('resize'));
                });
            }
        }
    }
});

function closeAllSelect2() {
    var $tempSelect = $(".tempDiv select");
    if (typeof ($tempSelect.data("select2")) != "undefined") {
        $tempSelect.select2("close");
    }
    if (typeof ($actionSelect.data("select2")) != "undefined") {
        $actionSelect.select2("close");
    }
    if (typeof ($actionValueSelect.data("select2")) != "undefined") {
        $actionValueSelect.select2("close");
    }
}

$(document).bind("dnd_start.vakata", function (e, data) {
    closeAllSelect2();
    $(".tempDiv").remove();
}).bind("dnd_move.vakata", function (e, data) {
    //
}).bind("dnd_stop.vakata", function (e, data) {
    //
});

function onSelectBRActionChange() {
    switch ($actionSelect.val()) {
        case "1":
            initTechnicianDropdown({
                control: $actionValueSelect,
                ignore: function () {
                    return [];
                }
            });
            break;
        case "2":
            initCategoryDropdownByLevel({
                control: $actionValueSelect,
                ignore: function () {
                    return [];
                },
                level: 1
            });
            break;
        case "3":
            initCategoryDropdownByLevel({
                control: $actionValueSelect,
                ignore: function () {
                    return [];
                },
                level: 2
            });
            break;
        case "4":
            initCategoryDropdownByLevel({
                control: $actionValueSelect,
                ignore: function () {
                    return [];
                },
                level: 3
            });
            break;
        case "5":
            initPriorityDropdown({
                control: $actionValueSelect,
                ignore: function () {
                    return [];
                }
            });
            break;
    }
}

$("#add-action").click(function () {
    if ($actionSelect.val() == null) alert("Please choose action.");
    else if ($actionValueSelect.val() == null) alert("Please set value for action.");
    else {
        var actionSet = $actionSelect.select2('data')[0].text + ' \"' +
            $actionValueSelect.select2('data')[0].text + '\"';
        var actionKey = $actionSelect.val();
        var actionValue = $actionValueSelect.val();
        $('#action-table tr:last').after('<tr class="actionSet" ' +
            'data-id=' + actionKey + ' data-value=' + actionValue + '>' +
            '<td><i class="fa fa-trash remove-action"></i></td><td>' + actionSet + '</td></tr>');
        //$actionSelect.val('').trigger("change");
        $actionValueSelect.val('').trigger("change");
        //$actionValueSelect.children().remove();
        //$actionValueSelect.select2('destroy');
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
        if ((i > 0) && (treeData[i].data.Condition == 0 || treeData[i].data.Criteria == 0 || !treeData[i].data.Value)) {
            noty({
                text: "The condition has not been finish yet! \n Please check again.",
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
