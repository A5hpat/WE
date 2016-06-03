(function ($) {

	// These match the ShoppingCart.cs CartTypeEnum values (also match equal values in minicart.js)
	var cartType_Shopping = 0;
	var cartType_Wish = 1;

	$.initializeAddToCartForms = function (options) {

		var minicartEnabled = false;
		if ($.minicart && $.minicart.cartEnabled) 
			minicartEnabled = $.minicart.cartEnabled;
		
		var miniwishEnabled = false;
		if ($.minicart && $.minicart.wishEnabled) 
			miniwishEnabled = $.minicart.wishEnabled;

		var settings = $.extend({
			quantitySelector: '.js-validate-quantity',
			errorSelector: '.js-add-to-cart-errors',
			cartFormSelector: '.js-add-to-cart-form',
			kitFormSelector: '.kit-add-to-cart-form',
			addToCartButtonSelector: '.js-add-to-cart-button',
			addToWishButtonSelector: '.js-wishlist-button',
			isWishListSelector: '.js-is-wishlist',
			ajaxAddToCartUrl: 'minicart/ajaxaddtocart',
			ajaxAddKitToCartUrl: 'minicart/ajaxaddkittocart',
			addToCartMessage: 'Adding to cart',
			addToWishMessage: 'Adding to wishlist',
			invalidQuantityMessage: 'Please enter a valid quantity', 
			addToCartFailureMessage: 'Your item was not added to the cart because of an error', 
			addToWishFailureMessage: 'Your item was not added to the wishlist because of an error',
			// These Urls should always be sent in as params on jscript contructor !!
			checkoutUrl: 'checkout',
			loginUrl: 'signin'
		}, options);

		function clearMessages(form) {
			$(form)
				.find(settings.errorSelector)
				.html('');
		};

		function addMessage(message, form) {
			$(form)
				.find(settings.errorSelector)
				.append('<div class="alert alert-danger">' + message + '</div>');
		}

		function validateAddToCart(form) {
			clearMessages(form);
			var quantity = $(form).find(settings.quantitySelector);
			if(quantity.length && !(quantity.val() > 0)) {
				addMessage(settings.invalidQuantityMessage, form);
				return false;
			}
			return true;
		}

		function ajaxAddToCart(form, isWish, isKit) {
			var cartType = isWish == 'true'
				? cartType_Wish
				: cartType_Shopping;
			var loadingMessage = isWish == 'true'
				? settings.addToWishMessage
				: settings.addToCartMessage;
			var errorMsg = isWish == 'true'
				? settings.addToWishFailureMessage
				: settings.addToCartFailureMessage;
			var ajaxUrl = isKit == 'true'
				? settings.ajaxAddKitToCartUrl
				: settings.ajaxAddToCartUrl;

			clearMessages(form);
			$.minicart.showLoading(loadingMessage);
			$.ajax({
				type: 'POST',
				url: ajaxUrl,
				data: $(form).serialize(),
				success: function (data, status) {
					$.minicart.hideLoading();
					if (data && data.Status) {
						// If the add was successful
						if (data.Status === 'Success') {
							if (((cartType == cartType_Shopping) && ($.minicart.cartEnabled) && (!$.minicart.onCheckoutPage))
							|| (cartType == cartType_Wish) && ($.minicart.wishEnabled)){
								$.minicart.showMiniCart(data.MinicartData, cartType);
							}
							else {
								window.location.href = settings.checkoutUrl;
							}
						}
						// If the add was not successful
						else {
							// Response requires a redirect
							if (data.Status === 'RequiresLogin' || data.Status === 'SessionTimeout' || data.Status === 'Forbidden')
								window.location.href = settings.loginUrl;

							// Stay here, display msgs
							if(data.Messages) {
								for(var i = 0; i < data.Messages.length; i++) {
									addMessage(data.Messages[i].Message, form);
								}
							}
						}
					}
					else {
						addMessage(errorMsg, form);
					}
				},
				error: function () {
					$.minicart.hideLoading();
					addMessage(errorMsg, form);
				}
			});
		}

		$(settings.cartFormSelector).on("submit", function (event) {
			var isWish = $(this).find(settings.isWishListSelector).val();

			if (!validateAddToCart(this)) {
				event.preventDefault();
			}
			else if (((isWish.toLowerCase() === 'false') && (minicartEnabled)) || ((isWish.toLowerCase() === 'true') && (miniwishEnabled))) {
				ajaxAddToCart(this, isWish, 'false');
				event.preventDefault();
			}
			// Otherwise post normally
		});

		$(settings.kitFormSelector).on("submit", function (event) {
			var isWish = $(this).find(settings.isWishListSelector).val();

			if(((isWish.toLowerCase() === 'false') && (minicartEnabled)) || ((isWish.toLowerCase() === 'true') && (miniwishEnabled))) {
				ajaxAddToCart(this, isWish, 'true');
				event.preventDefault();
			}
			// Otherwise post normally
		});

		$(settings.addToCartButtonSelector).on('click', function (event) {
			var form = $(this).closest('form');
			form.find(settings.isWishListSelector).val('false');
		});

		$(settings.addToWishButtonSelector).on('click', function (event) {
			var form = $(this).closest('form');
			form.find(settings.isWishListSelector).val('true');
		});
	}

}(adnsf$));
