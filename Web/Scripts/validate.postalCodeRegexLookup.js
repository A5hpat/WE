(function($, configuration){
	// Load the postal code regex map via AJAX. Until it's loaded, all client-side validation will pass.
	var postalCodeRegexMap = {};
	$.getJSON(configuration.postalCodeRegexMapUrl)
		.done(function(map) {
			postalCodeRegexMap = map;
		});

	// Register the postal code regex lookup validator with JQuery Validator and Unobtrusive Validation.
	$.validator.unobtrusive.adapters.addSingleVal("postalcoderegexlookup", "lookupkeyname");
	$.validator.addMethod("postalcoderegexlookup", function (value, element, lookupKeyName) {
		// Extract the prefix from the name of field being validated
		var prefix = element.name.substr(0, element.name.lastIndexOf(".") + 1);

		// Apply the prefix to the name of the lookup key to get the lookup key element name
		var fullLookupKeyName = lookupKeyName;
		if (fullLookupKeyName.indexOf("*.") === 0) {
			fullLookupKeyName = fullLookupKeyName.replace("*.", prefix);
		}
			
		// Get the value from the lookup key element
		var escapedLookupKeyName = fullLookupKeyName.replace(/([!"#$%&'()*+,./:;<=>?@@\[\\\]^`{|}~])/g, "\\$1");
		var lookupKeyValue = $(element.form)
			.find(":input")
			.filter("[name='" + escapedLookupKeyName + "']")
			.first()
			.val();

		// If we couldn't find the lookup key or if the key doesn't exist in the map, treat as valid
		if(!lookupKeyValue || !postalCodeRegexMap[lookupKeyValue]) {
			return true;
		}

		// Test the value against the regex for the lookup key value
		return RegExp(postalCodeRegexMap[lookupKeyValue]).test(value);
	});
})(adnsf$, window.postalCodeRegexConfiguration)
