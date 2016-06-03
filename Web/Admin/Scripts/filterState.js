//test to see if the browser supports local storage
var hasStorage = (function () {
	try {
		var test = "test";
		sessionStorage.setItem(test, test);
		sessionStorage.removeItem(test);
		return true;
	} catch(exception) {
		return false;
	}
}());

$(document).on('hide.bs.collapse', '#filterCollapse', function () {
	//if the browser supports local storage, store the filter panel state
	if(hasStorage) {
		sessionStorage.setItem("FilterClosed", "true");
	}

	$('.filter-actions').hide();
});

$(document).on('show.bs.collapse', '#filterCollapse', function () {
	//if the browser supports local storage, store the filter panel state
	if(hasStorage) {
		sessionStorage.setItem("FilterClosed", "false");
	}

	$('.filter-actions').show();
});

//on page load set the menu state from local storage
$(document).ready(function () {
	//if the browser supports local storage retrieve the filter collapse state
	if(hasStorage && sessionStorage.getItem("FilterClosed") === "true") {
		$('#filterCollapse').collapse('hide');
		$('.menu-column').addClass('closed');
		$('.filter-actions').hide();
	}
	else {
		$('.menu-column').removeClass('closed');
	}
});