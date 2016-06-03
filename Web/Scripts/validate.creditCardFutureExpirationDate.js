(function($) {
	// Register the validator with JQuery Validator and Unobtrusive Validation.
	$.validator.unobtrusive.adapters.addBool("creditcardfutureexpirationdate");
	$.validator.addMethod("creditcardfutureexpirationdate", function(value, element) {

		// Allow empty values
		if(value === null || value === '') {
			return true;
		}

		// If the value can't be parsed, pass. This validator is only for correct inputs.
		var parts = value.match(/^\s*(\d+)\s*(?:\/|-)\s*(\d+)\s*$/);
		if(parts.length < 3) {
			return true;
		}
		
		// Parse out the string into numeric month and year
		if(!$.isNumeric(parts[1]) || !$.isNumeric(parts[2])) {
			return true;
		}

		var enteredMonth = Number(parts[1]);

		var enteredYear = Number(parts[2]);
		if(enteredYear < 100)
			enteredYear += 2000;

		// Compare the parsed date to the current date
		var currentDate = new Date();

		if(enteredYear < currentDate.getFullYear()) {
			return false;
		}

		// Noe: .getMonth() returns a zero-based month (Jan = 0), but enteredMonth is one-based (Jan = 1).
		if(enteredYear === currentDate.getFullYear() && enteredMonth <= currentDate.getMonth()) {
			return false;
		}

		return true;
	});
})(adnsf$)