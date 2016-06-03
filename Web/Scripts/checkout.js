(function ($) {
	// settings
	var settings = {
		captchaToggleField: '.js-toggles-captcha',
		captchaToggleTarget: '.js-captcha-wrap'
	};

	// Password toggles captcha
	var toggleCaptcha = function () {
		if($(settings.captchaToggleField).val() != '') {
			$(settings.captchaToggleTarget).slideDown(250);
		}
		else {
			$(settings.captchaToggleTarget).slideUp(250);
		}
	};

	// Wire up the keyup event so typing causes the show/hide
	$(settings.captchaToggleField).keyup(toggleCaptcha);

	// Browser autofill fires a change event we need to listen for
	$(settings.captchaToggleField).change(toggleCaptcha);

	// Fire once on pageload.
	toggleCaptcha();

	// Copy the email address from the email form so that users don't have to first submit their email 
	// when trying to log in with a different email/password combo
	$('.js-checkout-login-form').submit(function () {
		$('.js-login-email').val($('.js-account-email').val());
	});

	// Copy the email address from the email form so that users don't have to first submit their email
	// when trying to create an account with a different email/password combo
	$('.js-create-account-form').submit(function () {
		$('.js-create-account-email').val($('.js-account-email').val());
	});

})(adnsf$);
