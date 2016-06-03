// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Entry.Component;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.HtmlHelperExtensions
{
	public static class HtmlHelperEntryExtensionMethods
	{
		/// <summary>
		/// Creates a standard form entry field with validation.
		/// </summary>
		/// <typeparam name="TModel">The type of the model containing the field or property to create the entry for.</typeparam>
		/// <typeparam name="TValue">The type of the field or property to create the entry for.</typeparam>
		/// <param name="htmlHelper">An HtmlHelper that will be used to construct the enty components.</param>
		/// <param name="expression">An expression that selects the field or property on the model to create the entry for.</param>
		/// <param name="@class">One or more CSS class names to add to the form group wrapper.</param>
		/// <param name="labelClass">One or more CSS class names to add to the label.</param>
		/// <param name="whatsThisClass">One or more CSS class names to add to the what's this.</param>
		/// <param name="whatsThisOverride">String value that, if !null, will be used in place of the field what's this Data Attribute.</param>
		/// <param name="editorClass">One or more CSS class names to add to the editor.</param>
		/// <param name="validationMessageClass">One or more CSS class names to add to the validation message.</param>
		/// <param name="feedbackClass">One or more CSS class names to add to the validation feedback.</param>
		/// <param name="descriptionClass">One or more CSS class names to add to the field description.</param>
		/// <param name="descriptionOverride">String value that, if !null, will be used in place of the field description Data Attribute.</param>
		public static IHtmlString EditorEntryFor<TModel, TValue>(
			this HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			string @class = null,
			string labelClass = null,
			string whatsThisClass = null,
			string whatsThisOverride = null,
			string editorClass = null,
			string validationMessageClass = null,
			string feedbackClass = null,
			string descriptionClass = null,
			string descriptionOverride = null,
			EntrySize displayWidth = EntrySize.Full,
			IDictionary<string, object> editorHtmlAttributes = null)
		{
			var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

			if(typeof(bool).IsAssignableFrom(metaData.ModelType))
				return CheckboxEditor(
					htmlHelper,
					expression,
					@class,
					labelClass,
					whatsThisClass,
					whatsThisOverride,
					editorClass,
					validationMessageClass,
					feedbackClass,
					descriptionClass,
					descriptionOverride,
					editorHtmlAttributes);

			return StandardEditor(
				htmlHelper,
				expression,
				@class,
				labelClass,
				whatsThisClass,
				whatsThisOverride,
				editorClass,
				validationMessageClass,
				feedbackClass,
				descriptionClass,
				descriptionOverride,
				displayWidth,
				editorHtmlAttributes);
		}

		/// <summary>
		/// Creates drop-down list form entry field with validation.
		/// </summary>
		/// <typeparam name="TModel">The type of the model containing the field or property to create the entry for.</typeparam>
		/// <typeparam name="TValue">The type of the field or property to create the entry for.</typeparam>
		/// <param name="htmlHelper">An HtmlHelper that will be used to construct the enty components.</param>
		/// <param name="expression">An expression that selects the field or property on the model to create the entry for.</param>
		/// <param name="values">The options to populate the dropdown list.</param>
		/// <param name="@default">The default "no value" display for the top of the dropdown list.</param>
		/// <param name="@class">One or more CSS class names to add to the form group wrapper.</param>
		/// <param name="labelClass">One or more CSS class names to add to the label.</param>
		/// <param name="whatsThisClass">One or more CSS class names to add to the what's this.</param>
		/// <param name="whatsThisOverride">String value that, if !null, will be used in place of the field what's this Data Attribute.</param>
		/// <param name="editorClass">One or more CSS class names to add to the editor.</param>
		/// <param name="validationMessageClass">One or more CSS class names to add to the validation message.</param>
		/// <param name="feedbackClass">One or more CSS class names to add to the validation feedback.</param>
		/// <param name="descriptionClass">One or more CSS class names to add to the field description.</param>
		/// <param name="descriptionOverride">String value that, if !null, will be used in place of the field description Data Attribute.</param>
		public static IHtmlString DropDownListEntryFor<TModel, TValue>(
			this HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			IEnumerable<SelectListItem> values,
			string @default = null,
			string @class = null,
			string labelClass = null,
			string whatsThisClass = null,
			string whatsThisOverride = null,
			string editorClass = null,
			string validationMessageClass = null,
			string feedbackClass = null,
			string descriptionClass = null,
			string descriptionOverride = null,
			string id = null,
			EntrySize? displayWidth = null,
			IDictionary<string, object> editorHtmlAttributes = null)
		{
			return new FormGroupEntryComponent().Build(
				htmlHelper: htmlHelper,
				expression: expression,
				context: new FormGroupContext(
					additionalCssClasses: @class,
					displayWidth: displayWidth ?? EntrySize.Full,
					contents: CombineHtmlStrings(
						new LabelEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new LabelContext(
								additionalCssClasses: labelClass)),
						new WhatsThisEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new WhatsThisContext(
								whatsThisOverride: whatsThisOverride,
								additionalCssClasses: whatsThisClass)),
						new WrapperEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new WrapperContext(
								tag: "div",
								@class: "entry-feedback-wrapper",
								contents: new DropDownListEntryComponent().Build(
										htmlHelper: htmlHelper,
										expression: expression,
										context: new DropDownListContext(
											values: values,
											@default: @default,
											additionalCssClasses: editorClass,
											htmlAttributes: editorHtmlAttributes,
											id: id)))),

						// Dropdown lists don't get a feedback icon
						new ValidationMessageEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new ValidationMessageContext(
								additionalCssClasses: validationMessageClass)),
						new DescriptionEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new DescriptionContext(
								descriptionOverride: descriptionOverride,
								additionalCssClasses: descriptionClass)))));
		}

		public static IHtmlString LabelEntryFor<TModel, TValue>(
			this HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			string labelClass = null,
			string whatsThisClass = null,
			string whatsThisOverride = null,
			IDictionary<string, object> editorHtmlAttributes = null)
		{
			return CombineHtmlStrings(
				new LabelEntryComponent().Build(
					htmlHelper: htmlHelper,
					expression: expression,
					context: new LabelContext(
						additionalCssClasses: labelClass)),
				new WhatsThisEntryComponent().Build(
					htmlHelper: htmlHelper,
					expression: expression,
					context: new WhatsThisContext(
						whatsThisOverride: whatsThisOverride,
						additionalCssClasses: whatsThisClass)));
		}

		public static IHtmlString CaptchaEntryFor<TModel, TValue>(
			this HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			string scope,
			string @class = null,
			string labelClass = null,
			string whatsThisClass = null,
			string whatsThisOverride = null,
			string editorClass = null,
			string validationMessageClass = null,
			string feedbackClass = null,
			string descriptionClass = null,
			string descriptionOverride = null,
			IDictionary<string, object> editorHtmlAttributes = null)
		{
			var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);

			return new FormGroupEntryComponent().Build(
				htmlHelper: htmlHelper,
				expression: expression,
				context: new FormGroupContext(
					additionalCssClasses: string.Format("{0} form-group-captcha", @class),
					displayWidth: EntrySize.Full,
					contents: CombineHtmlStrings(
						new LabelEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new LabelContext(
								additionalCssClasses: labelClass)),
						new WhatsThisEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new WhatsThisContext(
								whatsThisOverride: whatsThisOverride,
								additionalCssClasses: whatsThisClass)),
						new WrapperEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new WrapperContext(
								tag: "div",
								@class: "input-group entry-feedback-wrapper",
								contents: CombineHtmlStrings(
									new EditorEntryComponent().Build(
										htmlHelper: htmlHelper,
										expression: expression,
										context: new EditorContext(
											additionalCssClasses: editorClass,
											htmlAttributes: editorHtmlAttributes)),
									new WrapperEntryComponent().Build(
										htmlHelper: htmlHelper,
										expression: expression,
										context: new WrapperContext(
											tag: "span",
											@class: "input-group-addon input-group-addon-captchaimage",
											contents: new CaptchaImageEntryComponent().Build(
												htmlHelper: htmlHelper,
												expression: expression,
												context: new CaptchaImageContext(
													imageUrl: urlHelper.Action(
														ActionNames.Index,
														ControllerNames.Captcha,
														new { scope = scope })))))))),
						new ValidationMessageEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new ValidationMessageContext(
								additionalCssClasses: validationMessageClass)),
						new DescriptionEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new DescriptionContext(
								descriptionOverride: descriptionOverride,
								additionalCssClasses: descriptionClass)))));
		}

		static IHtmlString CombineHtmlStrings(params IHtmlString[] htmlStrings)
		{
			return MvcHtmlString.Create(string.Join(" ", (object[])htmlStrings));
		}

		static IHtmlString CheckboxEditor<TModel, TValue>(
			this HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			string @class = null,
			string labelClass = null,
			string whatsThisClass = null,
			string whatsThisOverride = null,
			string editorClass = null,
			string validationMessageClass = null,
			string feedbackClass = null,
			string descriptionClass = null,
			string descriptionOverride = null,
			IDictionary<string, object> editorHtmlAttributes = null)
		{
			var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

			return new WrapperEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new WrapperContext(
								tag: "div",
								@class: string.Join(" ", "form-group", @class),
								contents: CombineHtmlStrings(
									new WrapperEntryComponent().Build(
										htmlHelper: htmlHelper,
										expression: expression,
										context: new WrapperContext(
											tag: "div",
											@class: null,
											contents: new WrapperEntryComponent().Build(
												htmlHelper: htmlHelper,
												expression: expression,
												context: new WrapperContext(
													tag: "label",
													@class: "checkbox-label",
													contents: CombineHtmlStrings(
														new CheckboxEntryComponent().Build(
																htmlHelper: htmlHelper,
																expression: expression,
																context: new CheckboxContext(
																	additionalCssClasses: editorClass)),
															MvcHtmlString.Create(
																metadata.DisplayName
																?? metadata.PropertyName),
															new WhatsThisEntryComponent().Build(
																htmlHelper: htmlHelper,
																expression: expression,
																context: new WhatsThisContext(
																	whatsThisOverride: whatsThisOverride,
																	additionalCssClasses: whatsThisClass))))))),
									new ValidationMessageEntryComponent().Build(
										htmlHelper: htmlHelper,
										expression: expression,
										context: new ValidationMessageContext(
											additionalCssClasses: validationMessageClass)),
									new DescriptionEntryComponent().Build(
										htmlHelper: htmlHelper,
										expression: expression,
										context: new DescriptionContext(
											descriptionOverride: descriptionOverride,
											additionalCssClasses: descriptionClass)))));
		}

		static IHtmlString StandardEditor<TModel, TValue>(
			this HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			string @class = null,
			string labelClass = null,
			string whatsThisClass = null,
			string whatsThisOverride = null,
			string editorClass = null,
			string validationMessageClass = null,
			string feedbackClass = null,
			string descriptionClass = null,
			string descriptionOverride = null,
			EntrySize displayWidth = EntrySize.Full,
			IDictionary<string, object> editorHtmlAttributes = null)
		{
			return new FormGroupEntryComponent().Build(
				htmlHelper: htmlHelper,
				expression: expression,
				context: new FormGroupContext(
					additionalCssClasses: @class,
					displayWidth: displayWidth,
					contents: CombineHtmlStrings(
						new LabelEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new LabelContext(
								additionalCssClasses: labelClass)),
						new WhatsThisEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new WhatsThisContext(
								whatsThisOverride: whatsThisOverride,
								additionalCssClasses: whatsThisClass)),
						new WrapperEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new WrapperContext(
								tag: "div",
								@class: "entry-feedback-wrapper",
								contents: CombineHtmlStrings(
									new EditorEntryComponent().Build(
										htmlHelper: htmlHelper,
										expression: expression,
										context: new EditorContext(
											additionalCssClasses: editorClass,
											htmlAttributes: editorHtmlAttributes)),
									new FeedbackEntryComponent().Build(
										htmlHelper: htmlHelper,
										expression: expression,
										context: new FeedbackContext(
											additionalCssClasses: feedbackClass))))),
						new ValidationMessageEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new ValidationMessageContext(
								additionalCssClasses: validationMessageClass)),
						new DescriptionEntryComponent().Build(
							htmlHelper: htmlHelper,
							expression: expression,
							context: new DescriptionContext(
								descriptionOverride: descriptionOverride,
								additionalCssClasses: descriptionClass)))));
		}
	}
}
