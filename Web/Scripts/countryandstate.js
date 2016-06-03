var SelectDataBinder = (function ($) {
	return {
		BindCountryAndStates: function (countrySelector, stateSelector, queryStatesRoute, defaultOptionText) {
			$(countrySelector).change(function () {
				$.ajax({
					cache: false,
					url: queryStatesRoute + "?countryName=" + $(this).val()
				})
				.done(function (states) {
					var $stateSelector = $(stateSelector);

					$stateSelector
						.children()
						.remove();
					if(defaultOptionText && states.length > 1)
					{
						$stateSelector
							.append("<option value=''>" + defaultOptionText + "</option>");
					}
					$stateSelector
						.append($.map(states, function (item) {
							return $("<option value='" + item.abbreviation + "'>" + item.name + "</option>");
						})
					);
				})
				.fail(function () {
					alert('An error occurred updating states for the selected country.');
				});
			});
		}
	};
})(adnsf$);