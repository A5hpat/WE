$(document).ready(function() {
	
	/* select colors ======================================= */
	$('.product-colors > li > a').click(function() {
		$('.product-colors > li').removeClass('selected');
		$(this).parent().addClass('selected');
		return false;
	});
	$('.product-colors a').tooltip();


	/* off canvas menu ======================================= */
	$('.menu-link, .close-menu').on('click', function(){
		$('#wrap').toggleClass('menu-open');
		// $('.menu-wrapper').toggleClass('menu-show');
		return false;
	});	
	$(window).bind("resize",function(){
		// console.log($(this).width())
		if($(this).width() >768){
			$('div').removeClass('menu-open');
		}
	});

	$('#showmore').on('click', function(){
		$('#more-items').show();
		$(this).hide();
		return false
	});
});