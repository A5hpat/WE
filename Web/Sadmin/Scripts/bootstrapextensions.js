// Binding the tooltip is wrapped in Sys.Appication.add_load because the bootstrap tooltip styling is lost when the page contains an update panel.
// This will rebind the bootstrap tooltip on page load
function BindTooltipEvents() {
	$('[data-toggle="tooltip"]').tooltip({ placement: 'auto right', container: 'body', html: true });
}
if (typeof (Sys) === 'object' && Sys.Application && Sys.Application.add_load) {
	Sys.Application.add_load(BindTooltipEvents);
}
else {
	BindTooltipEvents();
}

//bootstrap tab setup
$(function() {
	$('.tabcontainer .nav-tabs a').click(function (e) {
		e.preventDefault()
		//set hiddenfield tabId
		$('input[id$=hdnTabId]').val($(this).attr('href'));
		$(this).tab('show')
	});
	//go to the latest tab, if it exists
	var lastTab = $('input[id$=hdnTabId]').val();
	if (lastTab !== 0) {
		$('a[href="' + lastTab + '"]').tab('show');
	}
	else {
		$('.nav-tabs a:first').tab('show');
	}
});

// Opt-in confirmation on any clickable.
// Set the data-confirmation-prompt attribute to control the prompt.
$(document).ready(function() {
	$('.js-confirm-prompt').click(function() {
		var prompt = $(this).data('confirmationPrompt') || 'Are you sure?';
		return confirm(prompt);
	})
});
