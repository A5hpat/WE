(function ($) {

	// These match the ShoppingCart.cs CartTypeEnum values (also match equal values in addtocart.js)
	var cartType_Shopping = 0;
	var cartType_Wish = 1;

	// settings
	var settings = {
		loadingOverlaySelector: '.js-minicart-loading-overlay',
		loadingMessageSelector: '.js-minicart-loading-message',

		minicart_DeleteMessage: 'Deleting...',
		miniwish_MoveSingleMessage: 'Moving item to cart.',
		miniwish_MoveAllMessage: 'Moving all items to cart.',
		minicart_UpdateFailureMessage: 'Sorry, there was an error updating your cart.',
		miniwish_UpdateFailureMessage: 'Sorry, there was an error updating your wishlist.',
		minicart_CountLabel: 'Items',
		minicart_CountLabelSingular: 'Item',

		minicart_TopLinkSelector: '#js-show-cart',
		miniwish_TopLinkSelector: '#js-show-wish',
		checkout_TopLinkSelector: '#js-header-checkout-link',

		minicart_ModalSelector: '#minicart-modal',
		miniwish_ModalSelector: '#miniwish-modal',

		minicart_FormSelector: '#minicart-form',
		miniwish_FormSelector: '#miniwish-form',

		minicart_MessageAreaSelector: '.js-minicart-message-area',
		miniwish_MessageAreaSelector: '.js-miniwish-message-area',

		minicart_CountSelector: '.js-minicart-count',
		miniwish_CountSelector: '.js-miniwish-count',
		minicart_TitleCountSelector: '.js-cart-title-count',
		miniwish_TitleCountSelector: '.js-wish-title-count',
		minicart_CountLabelSelector: '.js-cart-count-label',
		miniwish_CountLabelSelector: '.js-wish-count-label',
		minicart_ContentsSelector: '.js-minicart-contents',
		miniwish_ContentsSelector: '.js-miniwish-contents',
		minicart_SubTotalSelector: '.js-minicart-sub-total',
		minicart_DiscountSelector: '.js-minicart-discount',
		minicart_TotalSelector: '.js-minicart-total',
		miniwish_SubTotalSelector: '.js-miniwish-sub-total',
		miniwish_DiscountSelector: '.js-miniwish-discount',
		miniwish_TotalSelector: '.js-miniwish-total',

		minicart_SaveButtonSelector: '#minicart-save-button',
		miniwish_SaveButtonSelector: '#miniwish-save-button',

		minicart_CloseButtonSelector: '#minicart-close-button',
		miniwish_CloseButtonSelector: '#miniwish-close-button',

		minicart_ItemDeleteLinkSelector: '.js-minicart-delete-link',
		miniwish_ItemDeleteLinkSelector: '.js-miniwish-delete-link',

		minicart_CheckoutButtonSelector: '#minicart-checkout-button',

		miniwish_MoveSingleToCartSelector: '.js-move-single-to-cart',
		miniwish_MoveAllToCartSelector: '#miniwish-move-all-to-cart',

		minicart_SwitchToMiniwishSelector: '.js-switch-to-miniwish',
		miniwish_SwitchToMinicartSelector: '.js-switch-to-minicart',

		// These Urls should always be sent in as params on jscript contructor !!
		checkoutUrl: 'checkout',
		loginUrl: 'signin'
	};

	function clearMessages(cartType) {
		if (!cartTypeIsValid(cartType))
			return;

		if (cartType === cartType_Shopping)
			$(settings.minicart_MessageAreaSelector).html('');
		if (cartType === cartType_Wish)
			$(settings.miniwish_MessageAreaSelector).html('');
	};

	function addMessage(alert, cartType) {
		if (!cartTypeIsValid(cartType))
			return;

		var messageAreaSelector = cartType === cartType_Shopping
			? settings.minicart_MessageAreaSelector
			: settings.miniwish_MessageAreaSelector;

		if (alert.Type === 'Success') {
			$(messageAreaSelector).append('<div class="alert alert-success">' + alert.Message + '</div>');
		}
		if (alert.Type === 'Info') {
			$(messageAreaSelector).append('<div class="alert alert-info">' + alert.Message + '</div>');
		}
		else {
			$(messageAreaSelector).append('<div class="alert alert-danger">' + alert.Message + '</div>');
		}
	};

	var initialize = function (options) {

		settings = $.extend(settings, options);

		// EVENT REGISTRATION

		// Minicart - OpenLink: on click
		$(settings.minicart_TopLinkSelector).on('click', function (event) {
			if ($(settings.minicart_TopLinkSelector).attr('disabled'))
				$(settings.minicart_TopLinkSelector).attr('data-target', '');
			else
				$(settings.minicart_TopLinkSelector).attr('data-target', settings.minicart_ModalSelector);
		});
		// Miniwish - OpenLink: on click
		$(settings.miniwish_TopLinkSelector).on('click', function (event) {
			if ($(settings.miniwish_TopLinkSelector).attr('disabled'))
				$(settings.miniwish_TopLinkSelector).attr('data-target', '');
			else
				$(settings.miniwish_TopLinkSelector).attr('data-target', settings.miniwish_ModalSelector);
		});

		// Minicart - Switch to Wish: on click
		$(settings.minicart_SwitchToMiniwishSelector).on('click', function (event) {
			$(settings.miniwish_ModalSelector).modal('show');
			event.preventDefault();
		});
		// Miniwish - Switch to Cart: on click
		$(settings.miniwish_SwitchToMinicartSelector).on('click', function (event) {
			$(settings.minicart_ModalSelector).modal('show');
			event.preventDefault();
		});

		// Minicart - Bind the "update quantities" link to submit the form via ajax
		$(settings.minicart_SaveButtonSelector).on('click', function (event) {
			if($(settings.minicart_FormSelector).valid()) {
				saveCart($(settings.minicart_FormSelector), cartType_Shopping);
			}

			return false;
		});

		// Miniwish - Bind the "update quantities" link to submit the form via ajax
		$(settings.miniwish_SaveButtonSelector).on('click', function(event) {
			if($(settings.miniwish_FormSelector).valid()) {
				saveCart($(settings.miniwish_FormSelector), cartType_Wish);
			}

			return false;
		});

		// Minicart - Close: on click
		$(settings.minicart_CloseButtonSelector).on('click', function (event) {
			saveCart(settings.minicart_FormSelector, cartType_Shopping);
			$(settings.minicart_ModalSelector).modal('hide');
		});
		// Miniwish - Close on click
		$(settings.miniwish_CloseButtonSelector).on('click', function (event) {
			saveCart(settings.miniwish_FormSelector, cartType_Wish);
			$(settings.miniwish_ModalSelector).modal('hide');
		});

		function saveCart(formSelector, cartType) {
			var requestData = $(formSelector).serialize();
			if (requestData != '') {
				$.ajax({
					type: 'POST',
					url: $(formSelector).attr('action'),
					data: $(formSelector).serialize(),
					success: function (data, status) {
						if(updatePageDisplay(data, cartType) == true)
							hideLoading();
						else
							window.location.href = settings.loginUrl;
					},
					error: function (data) {
						hideLoading();
					}
				});
			}
		};

		// Minicart - Delete: on click (Bind to document b/c delete links are in returned ajax html, need to guarantee that click event fires after DOM manipulation)
		$(document).on('click', settings.minicart_ItemDeleteLinkSelector, function (event) {
			deleteItem(this, cartType_Shopping);
			event.preventDefault();
		});
		// Miniwish - Delete: on click (Bind to document, same as above)
		$(document).on('click', settings.miniwish_ItemDeleteLinkSelector, function (event) {
			deleteItem(this, cartType_Wish);
			event.preventDefault();
		});
		function deleteItem(caller, cartType) {
			showLoading(settings.minicart_DeleteMessage);
			var deleteUrl = $(caller).attr('href');
			if (deleteUrl.length > 0) {
				$.ajax({
					cache: false,
					type: 'GET',
					url: deleteUrl,
					success: function (data) {
						if (updatePageDisplay(data, cartType) == true)
							hideLoading();
						else
							window.location.href = settings.loginUrl;
					},
					error: function (data) {
						hideLoading();
					}
				});
			}
		};

		// Minicart - Checkout: on click
		$(settings.minicart_CheckoutButtonSelector).on('click', function () {
			$(settings.minicart_FormSelector).submit();
		});

		// Miniwish - MoveSingleToCart: on click (Bind to document, same as above)
		$(document).on('click', settings.miniwish_MoveSingleToCartSelector, function (event) {
			moveSingleToCart(this);
			event.preventDefault();
		});
		function moveSingleToCart(caller) {
			$(settings.miniwish_ModalSelector).modal('hide');
			showLoading(settings.miniwish_MoveSingleMessage);
			var moveToCartUrl = $(caller).attr('href');
			if (moveToCartUrl.length > 0) {
				$.ajax({
					cache: false,
					type: 'GET',
					url: moveToCartUrl,
					success: function (data) {
						hideLoading();
						updatePageDisplay(data.Wish, cartType_Wish);
						updatePageDisplay(data.Cart, cartType_Shopping);
						if (($.minicart.cartEnabled) && (!$.minicart.onCheckoutPage)) {
							$(settings.minicart_ModalSelector).modal('show');
						}
						else {
							window.location.href = settings.checkoutUrl;
						}
					},
					error: function (data) {
						hideLoading();
					}
				});
			}
		};
		// Miniwish - MoveAllToCart: on click
		$(settings.miniwish_MoveAllToCartSelector).on('click', function (event) {
			moveAllToCart();
			event.preventDefault();
		});
		function moveAllToCart() {
			$(settings.miniwish_ModalSelector).modal('hide');
			showLoading(settings.miniwish_MoveAllMessage);
			var moveToCartUrl = $(settings.miniwish_MoveAllToCartSelector).attr('href');

			if (moveToCartUrl.length > 0) {
				$.ajax({
					type: 'POST',
					url: moveToCartUrl,
					data: $(settings.miniwish_FormSelector).serialize(),
					success: function (data) {
						hideLoading();
						updatePageDisplay(data.Wish, cartType_Wish);
						updatePageDisplay(data.Cart, cartType_Shopping);
						if (($.minicart.cartEnabled) && (!$.minicart.onCheckoutPage)) {
							$(settings.minicart_ModalSelector).modal('show');
						}
						else {
							window.location.href = settings.checkoutUrl;
						}
					},
					error: function (data) {
						hideLoading();
					}
				});
			}
		};

	};

	var updatePageDisplay = function (minicartData, cartType) {
		// Response requires a redirect
		if (minicartData.Status === 'RequiresLogin' || minicartData.Status === 'SessionTimeout' || minicartData.Status === 'Forbidden')
			return false;

		// Response indicates Success or Validation Errors, update display
		var contentsSelector = cartType === cartType_Shopping
			? settings.minicart_ContentsSelector
			: settings.miniwish_ContentsSelector;
		var countSelector = cartType === cartType_Shopping
			? settings.minicart_CountSelector
			: settings.miniwish_CountSelector;
		var titleCountSelector = cartType === cartType_Shopping
			? settings.minicart_TitleCountSelector
			: settings.miniwish_TitleCountSelector;
		var titleCountLabelSelector = cartType === cartType_Shopping
			? settings.minicart_CountLabelSelector
			: settings.miniwish_CountLabelSelector;
		var subTotalSelector = cartType === cartType_Shopping
			? settings.minicart_SubTotalSelector
			: settings.miniwish_SubTotalSelector;
		var discountSelector = cartType === cartType_Shopping
			? settings.minicart_DiscountSelector
			: settings.miniwish_DiscountSelector;
		var totalSelector = cartType === cartType_Shopping
			? settings.minicart_TotalSelector
			: settings.miniwish_TotalSelector;
		var saveButtonSelector = cartType === cartType_Shopping
			? settings.minicart_SaveButtonSelector
			: settings.miniwish_SaveButtonSelector;
		var primaryActionSelector = cartType === cartType_Shopping
			? settings.minicart_CheckoutButtonSelector
			: settings.miniwish_MoveAllToCartSelector;
		var updateFailureMessage = cartType === cartType_Shopping
			? settings.minicart_UpdateFailureMessage
			: settings.miniwish_UpdateFailureMessage;
		var switchMiniSelector = cartType === cartType_Shopping
			? settings.miniwish_SwitchToMinicartSelector
			: settings.minicart_SwitchToMiniwishSelector;
		var topLinkSelector = cartType === cartType_Shopping
			? settings.minicart_TopLinkSelector
			: settings.miniwish_TopLinkSelector;

		clearMessages(cartType);

		if (minicartData) {
			if (minicartData.MinicartContentsHtml)
				$(contentsSelector).html(minicartData.MinicartContentsHtml);

			if (minicartData.CartCount != 'undefined') {
				$(countSelector).text(minicartData.CartCount);
				$(titleCountSelector).text(minicartData.CartCount);

				if (minicartData.CartCount === 1) {
					$(titleCountLabelSelector).text(settings.minicart_CountLabelSingular);
				}
				else {
					$(titleCountLabelSelector).text(settings.minicart_CountLabel);
				}
			}

			if (minicartData.SubTotal) {
				$(subTotalSelector).text(minicartData.SubTotal);
			}

			if (minicartData.Discount) {
				$(discountSelector).text(minicartData.Discount);
				$(discountSelector).parent().removeClass('off');
			}
			else {
				$(discountSelector).text("");
				$(discountSelector).parent().addClass('off');
			}

			if (minicartData.Total) {
				$(totalSelector).text(minicartData.Total);
			}

			if (minicartData.Messages) {
				for (var i = 0; i < minicartData.Messages.length; i++)
					addMessage(minicartData.Messages[i], cartType);
			}

			if (minicartData.CartCount === 0) {
				$(primaryActionSelector).addClass('off');
				$(saveButtonSelector).addClass('off');
				$(switchMiniSelector).addClass('off');
				$(topLinkSelector).attr('disabled', true);
				if (cartType === cartType_Shopping) {
					$(settings.checkout_TopLinkSelector).attr('disabled', true);
				}

			}
			else {
				$(primaryActionSelector).removeClass('off');
				$(saveButtonSelector).removeClass('off');
				$(switchMiniSelector).removeClass('off');
				$(topLinkSelector).attr('disabled', false);
				if (cartType === cartType_Shopping) {
					$(settings.checkout_TopLinkSelector).attr('disabled', false);
				}
			}
		}
		else
			addMessage({
				Type: "failure",
				Message: updateFailureMessage
			},
			cartType);

		// Reapply unobtrusive validation and event handlers on the response
		$.validator.unobtrusive.parse(settings.minicart_FormSelector);
		$.validator.unobtrusive.parse(settings.miniwish_FormSelector);

		return true;
	};

	var showLoading = function (message) {
		$(settings.loadingMessageSelector).html(message);
		$(settings.loadingOverlaySelector).removeClass('off');
	};

	var hideLoading = function () {
		$(settings.loadingMessageSelector).html('Loading...');
		$(settings.loadingOverlaySelector).addClass('off');
	};

	var showMinicart = function (data, cartType) {
		updatePageDisplay(data, cartType);

		if (cartType === cartType_Shopping)
			$(settings.minicart_ModalSelector).modal('show');
		else if (cartType === cartType_Wish)
			$(settings.miniwish_ModalSelector).modal('show');
	};

	var cartTypeIsValid = function (cartType) {
		if ((cartType === cartType_Shopping) || (cartType === cartType_Wish))
			return true;

		return false;
	};

	$.minicart = {
		cartEnabled: true,
		wishEnabled: true,
		onCheckoutPage: false,
		initialize: initialize,
		updatePageDisplay: updatePageDisplay,
		showMiniCart: showMinicart,
		showLoading: showLoading,
		hideLoading: hideLoading
	};

})(adnsf$);
