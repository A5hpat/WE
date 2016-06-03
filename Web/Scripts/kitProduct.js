(function ($) {
	var settings = {
		dropdownSelector: '.js-kit-select',
		radioSelector: '.js-kit-radio',
		checkboxSelector: '.js-kit-checkbox',
		textOptionSelector: '.js-kit-text-option',
		textAreaSelector: '.js-kit-text-area',
		hiddenFileUploadSelector: '.js-kit-hidden-text',
		fileUploadImageSelector: '.js-file-upload-image',
		kitControlWrapSelector: '.js-kit-control',
		kitItemIdentierSelector: '.js-kit-identifier',
		kitSummarySelector: '.js-kit-summary',
		kitQuantitySelector: '.js-kit-quantity',
		kitItemUploadLinkSelector: '.js-kit-item-upload-link',
		kitItemUploadHiddenTextOptionSelector: '.js-kit-hidden-text',
		temporaryFileStubSelector: '.js-temporary-file-stub',
		groupSelector: '.js-kit-group',
		kitItemMediaSelector: '.js-kit-item-info',
		kitItemNameSelector: '.js-kit-name-display',
		url: "/shoppingcart/ajaxgetkitdata", // url gets passed in from the _KitAddToCartForm.cshtml file
		kitUploadUrl: "/kitupload/detail" // url gets passed in from the _KitAddToCartForm.cshtml file
	};

	var updatePageDispaly = function (form, summaryContents, itemDisplayNames) {
		if(summaryContents && summaryContents.length > 0) {
			$(form).find(settings.kitSummarySelector).html(summaryContents);
		}
		if(itemDisplayNames) {
			for(var key in itemDisplayNames) {
				if(itemDisplayNames.hasOwnProperty(key)) {
					// Update the name
					var selector = settings.kitItemNameSelector
						+ '[data-kit-item-id="' + key + '"]';

					$(selector).html(itemDisplayNames[key]);

					// Update the dropdowns
					$(settings.dropdownSelector)
						.find('option[value="' + key + '"]')
						.html(itemDisplayNames[key]);
					
				}
			}
		}
	};

	var getKitSummary = function (form) {
		$.ajax({
			type: "POST",
			url: settings.url,
			data: $(form).serialize(),
			success: function (data) {
				if(data) {
					updatePageDispaly(form, data.SummaryHtml, data.ItemDisplayNames);
				}
			},
		});
	};

	var initialize = function (options) {
		settings = $.extend(settings, options);

		// Controls to update on change
		var onChangeSelector = [
			settings.dropdownSelector,
			settings.radioSelector,
			settings.checkboxSelector,
			settings.hiddenFileUploadSelector]
			.join();

		$(onChangeSelector).change(function () {
			getKitSummary($(this).closest('form'));
		});

		// Controls to update on blur
		var onBlurSelector = [
			settings.textOptionSelector,
			settings.textAreaSelector]
			.join();

		$(onBlurSelector).blur(function () {
			getKitSummary($(this).closest('form'));
		});

		// Update when we change the quantity
		$(settings.kitQuantitySelector).change(function () {
			var quantity = Number($(this).val());
			if(quantity && quantity > 0) {
				getKitSummary($(this).closest('form'));
			}
		});

		// Setup the file upload link click events
		$(settings.kitItemUploadLinkSelector).click(function (event) {
			var temporaryFileStub = $(settings.temporaryFileStubSelector).val();
			var url = $(this).attr('href') + '?stub=' + temporaryFileStub;
			var uploadWindow = window.open(
				url,
				'kitupload',
				'width=320,height=480');
			uploadWindow.focus();
			event.preventDefault();
		});

		//Setup an onchange event for the dropdowns to toggle the display image and description
		$(settings.dropdownSelector).change(function () {
			var selectedItemId = $(this).val();
			// Find the wrapping div as a reference point. From there find the display images and descriptions and hide or show them appropriately
			$(this).closest(settings.groupSelector)
				.find(settings.kitItemMediaSelector)
				.each(function () {
					if($(this).attr('data-kit-item-id') === selectedItemId){
						$(this).removeClass('off');
					}
					else{
						$(this).addClass('off');
					}
				});
		});
	}

	var updateFileUpload = function (kitItemId, imageUrl) {
		var kitItemElement = $(settings.kitControlWrapSelector + '[data-kit-item-id="' + kitItemId + '"]');
		kitItemElement.find(settings.hiddenFileUploadSelector).val(imageUrl);
		kitItemElement.find(settings.fileUploadImageSelector).attr('src', imageUrl)
			.removeClass('off');
	};

	$.kitForm = {
		initialize: initialize,
		updateFileUpload: updateFileUpload
	};

})(adnsf$);