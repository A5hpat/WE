adnsf$.validator.setDefaults({
	submitHandler: function (form) {
		adnsf$(form).trigger('adnsfFormValidated');
		return true;
	}
});