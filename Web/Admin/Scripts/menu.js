$('.main-menu .toggle').click(function (event) {
	//find this menu
	var thisMenu = $(this).closest('.flyout');
	//decide what to do
	if (!thisMenu.hasClass('active')) {
		//hide all menus
		$('.flyout').removeClass('active');
		//show this one
		thisMenu.addClass('active');
	}
	else {
		//hide this one
		thisMenu.removeClass('active');
	}
	event.preventDefault();
});

$('html').click(function () {
	//hide all menus
	$('.flyout').removeClass('active');
	//hide configuration menu
	$('#configuration-menu').removeClass('active');
	//hide manual search
	$('#help-menu').removeClass('active');
});

$('.main-menu, #configuration-menu, #configuration-menu-toggle, #help-menu-toggle, #help-menu').click(function (event) {
	event.stopPropagation();
});

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

$('#menu-toggle').click(function (event) {
	//if the browser supports local storage store the menu toggle state
	if(hasStorage)
	{
		if($('.menu-column').hasClass('closed')){
			sessionStorage.setItem("MenuClosed", "false");
		}
		else {
			sessionStorage.setItem("MenuClosed", "true");
		}
	}
	//hide or show the menu.
	$('.menu-column').toggleClass('closed');
	event.preventDefault();
});

//on page load set the menu state from local storage
$(document).ready(function () {
	//if the browser supports local storage retrieve the menu toggle state
	if(hasStorage && sessionStorage.getItem("MenuClosed") === "true") {
		$('.menu-column').addClass('closed');
	}
	else {
		$('.menu-column').removeClass('closed');
	}

	$('#configuration-menu-toggle').click(function (event) {
		$('#configuration-menu').toggleClass('active');
		event.preventDefault();
	});
		
	$('#help-menu-toggle').click(function (event) {
		$('#help-menu').toggleClass('active');
		$('#txtManualSearch').focus();
		event.preventDefault();
	});
});

