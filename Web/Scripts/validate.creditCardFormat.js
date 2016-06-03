(function($){
	// Register the credit card number validator with JQuery Validator and Unobtrusive Validation.
	$.validator.unobtrusive.adapters.addBool("creditcard");
	$.validator.addMethod("creditcard", function (value, element) {
		if (value.startsWith('•')) {
			//The card number is obfuscated, so they've already been through validation and aren't changing that field
			return true;
		}

		//Otherwise, use the validator in card.js
		return Payment.fns.validateCardNumber(value);
	});
})(adnsf$)