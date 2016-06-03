//header help search
$('.js-help-button').click(function (event) {
	var searchText = $('.js-help-box').val();
	var helpUrl = (searchText.length > 0 )
		? 'http://help.aspdotnetstorefront.com/manual/1000/default.aspx?pageid=_search_&searchtext=' + searchText
		: 'http://help.aspdotnetstorefront.com/manual/1000/default.aspx?pageid=welcome';

	window.open(helpUrl);
	event.preventDefault();
});

$('.js-help-box').keyup(function (event) {
	if (event.keyCode == 13) {
		$('.js-help-button').click();
	}
});

function showAdminLoadingOverlay() {
	$('.js-admin-loading-overlay').show();
};

function hideAdminLoadingOverlay() {
	$('.js-admin-loading-overlay').hide();
};
