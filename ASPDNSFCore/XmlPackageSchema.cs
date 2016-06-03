// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.XmlPackageSchema
{
	// XmlPackage XSD schema

	using System;
	using System.Xml.Serialization;

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class sql
	{

		private string nameField;

		private string valueField;

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		[System.Xml.Serialization.XmlTextAttribute()]
		public string Value
		{
			get
			{
				return this.valueField;
			}
			set
			{
				this.valueField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class HTTPHeader
	{

		private string headernameField;

		private string headervalueField;

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string headername
		{
			get
			{
				return this.headernameField;
			}
			set
			{
				this.headernameField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string headervalue
		{
			get
			{
				return this.headervalueField;
			}
			set
			{
				this.headervalueField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class HTTPHeaders
	{

		private HTTPHeader[] hTTPHeaderField;

		[System.Xml.Serialization.XmlElementAttribute("HTTPHeader")]
		public HTTPHeader[] HTTPHeader
		{
			get
			{
				return this.hTTPHeaderField;
			}
			set
			{
				this.hTTPHeaderField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class queryparam
	{

		private string paramnameField;

		private QueryParamType paramtypeField;

		private string requestparamnameField;

		private SqlDataType sqlDataTypeField;

		private string defvalueField;

		private string validationpatternField;

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string paramname
		{
			get
			{
				return this.paramnameField;
			}
			set
			{
				this.paramnameField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public QueryParamType paramtype
		{
			get
			{
				return this.paramtypeField;
			}
			set
			{
				this.paramtypeField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string requestparamname
		{
			get
			{
				return this.requestparamnameField;
			}
			set
			{
				this.requestparamnameField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public SqlDataType sqlDataType
		{
			get
			{
				return this.sqlDataTypeField;
			}
			set
			{
				this.sqlDataTypeField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string defvalue
		{
			get
			{
				return this.defvalueField;
			}
			set
			{
				this.defvalueField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string validationpattern
		{
			get
			{
				return this.validationpatternField;
			}
			set
			{
				this.validationpatternField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	public enum QueryParamType
	{

		request,

		appconfig,

		runtime,

		webconfig,

		xpath,

		system,

		form,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	public enum SqlDataType
	{

		bigint,

		bit,

		@char,

		datetime,

		@decimal,

		@float,

		@int,

		money,

		nchar,

		ntext,

		nvarchar,

		real,

		smalldatetime,

		smallint,

		smallmoney,

		text,

		tinyint,

		uniqueidentifier,

		varchar,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class querystringreplace
	{

		private string replaceTagField;

		private QueryParamType replacetypeField;

		private string replaceparamnameField;

		private string defvalueField;

		private string validationpatternField;

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string replaceTag
		{
			get
			{
				return this.replaceTagField;
			}
			set
			{
				this.replaceTagField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public QueryParamType replacetype
		{
			get
			{
				return this.replacetypeField;
			}
			set
			{
				this.replacetypeField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string replaceparamname
		{
			get
			{
				return this.replaceparamnameField;
			}
			set
			{
				this.replaceparamnameField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string defvalue
		{
			get
			{
				return this.defvalueField;
			}
			set
			{
				this.defvalueField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string validationpattern
		{
			get
			{
				return this.validationpatternField;
			}
			set
			{
				this.validationpatternField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class postdata
	{

		private QueryParamType paramtypeField;

		private string paramnameField;

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public QueryParamType paramtype
		{
			get
			{
				return this.paramtypeField;
			}
			set
			{
				this.paramtypeField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string paramname
		{
			get
			{
				return this.paramnameField;
			}
			set
			{
				this.paramnameField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class querytransform
	{

		private System.Xml.XmlElement[] anyField;

		[System.Xml.Serialization.XmlAnyElementAttribute()]
		public System.Xml.XmlElement[] Any
		{
			get
			{
				return this.anyField;
			}
			set
			{
				this.anyField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class query
	{

		private sql sqlField;

		private querystringreplace[] querystringreplaceField;

		private queryparam[] queryparamField;

		private querytransform querytransformField;

		private string nameField;

		private string rowElementNameField;

		private string runifField;

		private string retTypeField;

		private string connectionStringNameField;

		private int commandTimeoutField;

		private bool commandTimeoutFieldSpecified;

		public sql sql
		{
			get
			{
				return this.sqlField;
			}
			set
			{
				this.sqlField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("querystringreplace")]
		public querystringreplace[] querystringreplace
		{
			get
			{
				return this.querystringreplaceField;
			}
			set
			{
				this.querystringreplaceField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("queryparam")]
		public queryparam[] queryparam
		{
			get
			{
				return this.queryparamField;
			}
			set
			{
				this.queryparamField = value;
			}
		}

		public querytransform querytransform
		{
			get
			{
				return this.querytransformField;
			}
			set
			{
				this.querytransformField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string rowElementName
		{
			get
			{
				return this.rowElementNameField;
			}
			set
			{
				this.rowElementNameField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string runif
		{
			get
			{
				return this.runifField;
			}
			set
			{
				this.runifField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string retType
		{
			get
			{
				return this.retTypeField;
			}
			set
			{
				this.retTypeField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string connectionStringName
		{
			get
			{
				return this.connectionStringNameField;
			}
			set
			{
				this.connectionStringNameField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public int commandTimeout
		{
			get
			{
				return this.commandTimeoutField;
			}
			set
			{
				this.commandTimeoutField = value;
			}
		}

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool commandTimeoutSpecified
		{
			get
			{
				return this.commandTimeoutFieldSpecified;
			}
			set
			{
				this.commandTimeoutFieldSpecified = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class webquery
	{

		private string urlField;

		private postdata postdataField;

		private querystringreplace[] querystringreplaceField;

		private querytransform querytransformField;

		private string nameField;

		private string retTypeField;

		private string runifField;

		private int timeoutField;

		private bool timeoutFieldSpecified;

		private string methodField;

		[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public string url
		{
			get
			{
				return this.urlField;
			}
			set
			{
				this.urlField = value;
			}
		}

		public postdata postdata
		{
			get
			{
				return this.postdataField;
			}
			set
			{
				this.postdataField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("querystringreplace")]
		public querystringreplace[] querystringreplace
		{
			get
			{
				return this.querystringreplaceField;
			}
			set
			{
				this.querystringreplaceField = value;
			}
		}

		public querytransform querytransform
		{
			get
			{
				return this.querytransformField;
			}
			set
			{
				this.querytransformField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string RetType
		{
			get
			{
				return this.retTypeField;
			}
			set
			{
				this.retTypeField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string runif
		{
			get
			{
				return this.runifField;
			}
			set
			{
				this.runifField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public int timeout
		{
			get
			{
				return this.timeoutField;
			}
			set
			{
				this.timeoutField = value;
			}
		}

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool timeoutSpecified
		{
			get
			{
				return this.timeoutFieldSpecified;
			}
			set
			{
				this.timeoutFieldSpecified = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string method
		{
			get
			{
				return this.methodField;
			}
			set
			{
				this.methodField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class PackageTransform
	{

		private System.Xml.XmlElement[] anyField;

		[System.Xml.Serialization.XmlAnyElementAttribute()]
		public System.Xml.XmlElement[] Any
		{
			get
			{
				return this.anyField;
			}
			set
			{
				this.anyField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class SearchEngineSettings
	{

		private SearchEngineSettingsSectionTitle sectionTitleField;

		private SearchEngineSettingsSETitle sETitleField;

		private SearchEngineSettingsSEKeywords sEKeywordsField;

		private SearchEngineSettingsSEDescription sEDescriptionField;

		private SearchEngineSettingsSENoScript sENoScriptField;

		[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public SearchEngineSettingsSectionTitle SectionTitle
		{
			get
			{
				return this.sectionTitleField;
			}
			set
			{
				this.sectionTitleField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public SearchEngineSettingsSETitle SETitle
		{
			get
			{
				return this.sETitleField;
			}
			set
			{
				this.sETitleField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public SearchEngineSettingsSEKeywords SEKeywords
		{
			get
			{
				return this.sEKeywordsField;
			}
			set
			{
				this.sEKeywordsField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public SearchEngineSettingsSEDescription SEDescription
		{
			get
			{
				return this.sEDescriptionField;
			}
			set
			{
				this.sEDescriptionField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public SearchEngineSettingsSENoScript SENoScript
		{
			get
			{
				return this.sENoScriptField;
			}
			set
			{
				this.sENoScriptField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class SearchEngineSettingsSectionTitle
	{

		private System.Xml.XmlNode[] anyField;

		private string actionTypeField;

		[System.Xml.Serialization.XmlTextAttribute()]
		[System.Xml.Serialization.XmlAnyElementAttribute()]
		public System.Xml.XmlNode[] Any
		{
			get
			{
				return this.anyField;
			}
			set
			{
				this.anyField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string actionType
		{
			get
			{
				return this.actionTypeField;
			}
			set
			{
				this.actionTypeField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class SearchEngineSettingsSETitle
	{

		private System.Xml.XmlNode[] anyField;

		private string actionTypeField;

		[System.Xml.Serialization.XmlTextAttribute()]
		[System.Xml.Serialization.XmlAnyElementAttribute()]
		public System.Xml.XmlNode[] Any
		{
			get
			{
				return this.anyField;
			}
			set
			{
				this.anyField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string actionType
		{
			get
			{
				return this.actionTypeField;
			}
			set
			{
				this.actionTypeField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class SearchEngineSettingsSEKeywords
	{

		private System.Xml.XmlNode[] anyField;

		private string actionTypeField;

		[System.Xml.Serialization.XmlTextAttribute()]
		[System.Xml.Serialization.XmlAnyElementAttribute()]
		public System.Xml.XmlNode[] Any
		{
			get
			{
				return this.anyField;
			}
			set
			{
				this.anyField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string actionType
		{
			get
			{
				return this.actionTypeField;
			}
			set
			{
				this.actionTypeField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class SearchEngineSettingsSEDescription
	{

		private System.Xml.XmlNode[] anyField;

		private string actionTypeField;

		[System.Xml.Serialization.XmlTextAttribute()]
		[System.Xml.Serialization.XmlAnyElementAttribute()]
		public System.Xml.XmlNode[] Any
		{
			get
			{
				return this.anyField;
			}
			set
			{
				this.anyField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string actionType
		{
			get
			{
				return this.actionTypeField;
			}
			set
			{
				this.actionTypeField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class SearchEngineSettingsSENoScript
	{

		private System.Xml.XmlNode[] anyField;

		private string actionTypeField;

		[System.Xml.Serialization.XmlTextAttribute()]
		[System.Xml.Serialization.XmlAnyElementAttribute()]
		public System.Xml.XmlNode[] Any
		{
			get
			{
				return this.anyField;
			}
			set
			{
				this.anyField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string actionType
		{
			get
			{
				return this.actionTypeField;
			}
			set
			{
				this.actionTypeField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class queryafter
	{

		private object sqlField;

		private querystringreplace[] querystringreplaceField;

		private queryparam[] queryparamField;

		private queryafterRunif[] runifField;

		[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public object sql
		{
			get
			{
				return this.sqlField;
			}
			set
			{
				this.sqlField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("querystringreplace")]
		public querystringreplace[] querystringreplace
		{
			get
			{
				return this.querystringreplaceField;
			}
			set
			{
				this.querystringreplaceField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("queryparam")]
		public queryparam[] queryparam
		{
			get
			{
				return this.queryparamField;
			}
			set
			{
				this.queryparamField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("runif", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public queryafterRunif[] runif
		{
			get
			{
				return this.runifField;
			}
			set
			{
				this.runifField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class queryafterRunif
	{

		private queryafterRunifParamtype paramtypeField;

		private string paramsourceField;

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public queryafterRunifParamtype paramtype
		{
			get
			{
				return this.paramtypeField;
			}
			set
			{
				this.paramtypeField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string paramsource
		{
			get
			{
				return this.paramsourceField;
			}
			set
			{
				this.paramsourceField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public enum queryafterRunifParamtype
	{

		request,

		appconfig,

		xpath,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class webqueryafter
	{

		private object urlField;

		private querystringreplace[] querystringreplaceField;

		private webqueryafterRunif[] runifField;

		[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public object url
		{
			get
			{
				return this.urlField;
			}
			set
			{
				this.urlField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("querystringreplace")]
		public querystringreplace[] querystringreplace
		{
			get
			{
				return this.querystringreplaceField;
			}
			set
			{
				this.querystringreplaceField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("runif", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public webqueryafterRunif[] runif
		{
			get
			{
				return this.runifField;
			}
			set
			{
				this.runifField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class webqueryafterRunif
	{

		private webqueryafterRunifParamtype paramtypeField;

		private string paramsourceField;

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public webqueryafterRunifParamtype paramtype
		{
			get
			{
				return this.paramtypeField;
			}
			set
			{
				this.paramtypeField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string paramsource
		{
			get
			{
				return this.paramsourceField;
			}
			set
			{
				this.paramsourceField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public enum webqueryafterRunifParamtype
	{

		request,

		appconfig,

		xpath,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class setcookie
	{

		private string cookienameField;

		private setcookieValuetype valuetypeField;

		private string cookiesourceField;

		private int expiresField;

		private bool expiresFieldSpecified;

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string cookiename
		{
			get
			{
				return this.cookienameField;
			}
			set
			{
				this.cookienameField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public setcookieValuetype valuetype
		{
			get
			{
				return this.valuetypeField;
			}
			set
			{
				this.valuetypeField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string cookiesource
		{
			get
			{
				return this.cookiesourceField;
			}
			set
			{
				this.cookiesourceField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public int expires
		{
			get
			{
				return this.expiresField;
			}
			set
			{
				this.expiresField = value;
			}
		}

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool expiresSpecified
		{
			get
			{
				return this.expiresFieldSpecified;
			}
			set
			{
				this.expiresFieldSpecified = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public enum setcookieValuetype
	{

		request,

		appconfig,

		webconfig,

		xpath,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class PostProcessing
	{

		private queryafter[] queryafterField;

		private webqueryafter[] webqueryafterField;

		private setcookie[] setcookieField;

		[System.Xml.Serialization.XmlElementAttribute("queryafter")]
		public queryafter[] queryafter
		{
			get
			{
				return this.queryafterField;
			}
			set
			{
				this.queryafterField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("webqueryafter")]
		public webqueryafter[] webqueryafter
		{
			get
			{
				return this.webqueryafterField;
			}
			set
			{
				this.webqueryafterField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("setcookie")]
		public setcookie[] setcookie
		{
			get
			{
				return this.setcookieField;
			}
			set
			{
				this.setcookieField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class runtime
	{

		private string paramnameField;

		private QueryParamType paramtypeField;

		private string requestparamnameField;

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string paramname
		{
			get
			{
				return this.paramnameField;
			}
			set
			{
				this.paramnameField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public QueryParamType paramtype
		{
			get
			{
				return this.paramtypeField;
			}
			set
			{
				this.paramtypeField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string requestparamname
		{
			get
			{
				return this.requestparamnameField;
			}
			set
			{
				this.requestparamnameField = value;
			}
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class package
	{

		private HTTPHeader[] hTTPHeadersField;

		private runtime[] runtimeField;

		private query[] queryField;

		private webquery[] webqueryField;

		private PackageTransform packageTransformField;

		private SearchEngineSettings searchEngineSettingsField;

		private PostProcessing postProcessingField;

		private bool debugField;

		private bool debugFieldSpecified;

		private bool requiresParserField;

		private bool requiresParserFieldSpecified;

		private bool includeentityhelperField;

		private bool includeentityhelperFieldSpecified;

		private decimal versionField;

		private string displaynameField;

		private string contenttypeField;

		private bool allowengineField;

		private bool allowengineFieldSpecified;

		[System.Xml.Serialization.XmlArrayItemAttribute("HTTPHeader", IsNullable = false)]
		public HTTPHeader[] HTTPHeaders
		{
			get
			{
				return this.hTTPHeadersField;
			}
			set
			{
				this.hTTPHeadersField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("runtime")]
		public runtime[] runtime
		{
			get
			{
				return this.runtimeField;
			}
			set
			{
				this.runtimeField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("query")]
		public query[] query
		{
			get
			{
				return this.queryField;
			}
			set
			{
				this.queryField = value;
			}
		}

		[System.Xml.Serialization.XmlElementAttribute("webquery")]
		public webquery[] webquery
		{
			get
			{
				return this.webqueryField;
			}
			set
			{
				this.webqueryField = value;
			}
		}

		public PackageTransform PackageTransform
		{
			get
			{
				return this.packageTransformField;
			}
			set
			{
				this.packageTransformField = value;
			}
		}

		public SearchEngineSettings SearchEngineSettings
		{
			get
			{
				return this.searchEngineSettingsField;
			}
			set
			{
				this.searchEngineSettingsField = value;
			}
		}

		public PostProcessing PostProcessing
		{
			get
			{
				return this.postProcessingField;
			}
			set
			{
				this.postProcessingField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public bool debug
		{
			get
			{
				return this.debugField;
			}
			set
			{
				this.debugField = value;
			}
		}

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool debugSpecified
		{
			get
			{
				return this.debugFieldSpecified;
			}
			set
			{
				this.debugFieldSpecified = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public bool RequiresParser
		{
			get
			{
				return this.requiresParserField;
			}
			set
			{
				this.requiresParserField = value;
			}
		}

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool RequiresParserSpecified
		{
			get
			{
				return this.requiresParserFieldSpecified;
			}
			set
			{
				this.requiresParserFieldSpecified = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public bool includeentityhelper
		{
			get
			{
				return this.includeentityhelperField;
			}
			set
			{
				this.includeentityhelperField = value;
			}
		}

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool includeentityhelperSpecified
		{
			get
			{
				return this.includeentityhelperFieldSpecified;
			}
			set
			{
				this.includeentityhelperFieldSpecified = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal version
		{
			get
			{
				return this.versionField;
			}
			set
			{
				this.versionField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string displayname
		{
			get
			{
				return this.displaynameField;
			}
			set
			{
				this.displaynameField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string contenttype
		{
			get
			{
				return this.contenttypeField;
			}
			set
			{
				this.contenttypeField = value;
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public bool allowengine
		{
			get
			{
				return this.allowengineField;
			}
			set
			{
				this.allowengineField = value;
			}
		}

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool allowengineSpecified
		{
			get
			{
				return this.allowengineFieldSpecified;
			}
			set
			{
				this.allowengineFieldSpecified = value;
			}
		}
	}

	[XmlRoot(ElementName = "System")]
	public class SystemData
	{
		public Boolean IsAdminSite
		{ get; set; }
		public Int32 IsAdminSiteInt
		{ get; set; }
		public Int32 CustomerID
		{ get; set; }

		public Int32 DefaultVATSetting
		{ get; set; }
		public Int32 CustomerVATSetting
		{ get; set; }
		public Int32 UseVATSetting
		{ get; set; }
		public Int32 CustomerLevelID
		{ get; set; }
		public String CustomerLevelName
		{ get; set; }
		public String CustomerFirstName
		{ get; set; }
		public String CustomerLastName
		{ get; set; }
		public String CustomerFullName
		{ get; set; }
		public String CustomerRoles
		{ get; set; }
		public Boolean IsAdminUser
		{ get; set; }
		public Boolean IsSuperUser
		{ get; set; }

		[XmlElement(ElementName = "VAT.Enabled")]
		public Boolean VAT_Enabled
		{ get; set; }

		[XmlElement(ElementName = "VAT.AllowCustomerToChooseSetting")]
		public Boolean VAT_AllowCustomerToChooseSetting
		{ get; set; }

		public String LocaleSetting
		{ get; set; }
		public String CurrencySetting
		{ get; set; }
		public String CurrencyDisplayLocaleFormat
		{ get; set; }
		public String WebConfigLocaleSetting
		{ get; set; }
		public String SqlServerLocaleSetting
		{ get; set; }
		public String PrimaryCurrency
		{ get; set; }
		public String PrimaryCurrencyDisplayLocaleFormat
		{ get; set; }
		public DateTime Date
		{ get; set; }
		public DateTime Time
		{ get; set; }
		public Int32 SkinID
		{ get; set; }
		public Int32 AffiliateID
		{ get; set; }

		public String IPAddress
		{ get; set; }
		public String QueryStringRAW
		{ get; set; }
		public String PageName
		{ get; set; }
		public String FullPageName
		{ get; set; }
		public String XmlPackageName
		{ get; set; }
		public String StoreUrl
		{ get; set; }
		public DateTime CurrentDateTime
		{ get; set; }
		public Boolean CustomerIsRegistered
		{ get; set; }
		public Int32 StoreID
		{ get; set; }
		public Boolean FilterProduct
		{ get; set; }
		public Boolean FilterEntity
		{ get; set; }
		public Boolean FilterTopic
		{ get; set; }
		public Boolean FilterNews
		{ get; set; }
		public String RequestedPage
		{ get; set; }
		public String RequestedQuerystring
		{ get; set; }

		public String AdnsfVersion
		{ get; set; }

		public Int32 AdnsfVersionMajor
		{ get; set; }

		public Int32 AdnsfVersionMinor
		{ get; set; }

		public Int32 AdnsfVersionRevision
		{ get; set; }

		public Int32 AdnsfVersionBuild
		{ get; set; }
	}
}
