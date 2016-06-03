(function($){
	// Register the required checked validator with JQuery Validator and Unobtrusive Validation.
	$.validator.unobtrusive.adapters.addBool('requirechecked', 'required');
})(adnsf$)