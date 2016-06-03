
// hook 'show' event for ALL bootstrap modals so we can ensure no 2 modals are open simultaneously

$('.modal').on('show.bs.modal', function () {
	$('.modal').not($(this)).each(function () {
		$(this).modal('hide');
	});
});