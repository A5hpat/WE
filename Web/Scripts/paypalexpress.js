var PayPalExpress = PayPalExpress || (function ($) {
	return {
		init: function (username, environment, scriptTarget, urlBuilderEndpoint) {
			window.paypalCheckoutReady = function () {
				paypal.checkout.setup(username, {
					environment: environment,
					button: [scriptTarget],
					click: function () {
						paypal.checkout.initXO();
						$.get(urlBuilderEndpoint)
							.done(function (url) {
								paypal.checkout.startFlow(url);
							})
							.fail(function (reason) {
								paypal.checkout.closeFlow();
							});
					}
				});
			};
		}
	};
}(adnsf$));