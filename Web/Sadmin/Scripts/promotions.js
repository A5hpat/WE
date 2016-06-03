$(document).ready(function () {

	// Setup events
	$('.expandAllExpandables').click(function () {
		ToggleAllExpandables(true);
	});

	$('.collapseAllExpandables').click(function () {
		ToggleAllExpandables(false);
	});

	$(".checkHeader").click(function () {
		// Show the corresponding section when a checkbox or radio button is clicked
		$(this).has(":checkbox:checked, :radio:checked").next().show("fast");

		// Hide unselected sections
		$(this).closest('.checkTarget').find(".checkHeader").has(".checkToggler :checkbox:not(:checked), .checkToggler :radio:not(:checked)").next().hide("fast");
	});

	SetupExpandableToggles();
	InitializeExpandableState();
});

function ToggleAllExpandables(visible) {
	if (visible)
	{
		$(".promotion-panels a[data-toggle='collapse']").removeClass("collapsed");
		$(".promotion-panels .panel-collapse").addClass("in");
		$(".promotion-panels .panel-collapse").removeAttr("style");
	}
	else
	{
		$(".promotion-panels a[data-toggle='collapse']").addClass("collapsed");
		$(".promotion-panels .panel-collapse").removeClass("in");
		$(".promotion-panels .panel-collapse").removeAttr("style");
	}
}

function SwitchExpandableIcon(expandableElement, expanded) {

	var collapableAnchor = $(expandableElement)

	if (expanded) {
		collapableAnchor.removeClass("collapsed");
	}
	else {
		collapableAnchor.addClass("collapsed");
	}
}

function SetupExpandableToggles() {
	
	$(".promotion-panels a[data-toggle='collapse']").click(function () {
		if ($(this).parent().parent().next().is(":visible")) {
			$(this).removeClass("collapsed");
			$(this).parent().parent().next().addClass("in");
			$(this).parent().parent().next().removeAttr("style");
		}
		else {
			$(this).parent().parent().next().show();
		}
	});
}

function InitializeExpandableState() {

	var expandPanel = false;
	var tag = null;
	SwitchExpandableIcon($(".checkHeader").parents("[class*='panel-collapse collapse']").prev().find("a"), false);

	$(".checkHeader").each(function () {
		var hasChecked = $(this).find(":checkbox, :radio").is(":checked");
		$(this).next().toggle(hasChecked);
		if (hasChecked) {
			$(this).parents("[class*='panel-collapse collapse']").addClass("in");
			$(this).parents("[class*='panel-collapse collapse']").removeAttr("style");
			SwitchExpandableIcon($(this).parents("[class*='panel-collapse collapse']").prev(), true);
			SwitchExpandableIcon($(this).parents("[class*='panel-collapse collapse']").prev().find("a:first"), true);
		}
	});
}