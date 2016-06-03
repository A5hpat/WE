<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.MultiImageManager" Codebehind="multiimagemanager.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<script type="text/javascript" src="../scripts/formValidate.js"></script>
	<script src="Scripts/jquery.min.js" type="text/javascript"></script>
	<script src="Scripts/bootstrap.min.js" type="text/javascript"></script>
</head>
<body class="body-tag multi-image-body-adjustments">
	<div class="container-fluid main-content-wrapper multi-image-main-adjustments">
		<h1>
			<i class="fa fa-file-image-o"></i>
			<asp:Label ID="lblHeader" runat="server" Text="<%$Tokens:StringResource, admin.title.MultiImageManager %>" />
		</h1>
		<div class="form-group">
			<div class="row admin-row">
				<div class="col-sm-2">
					<asp:Label ID="lblProductPrompt" runat="server" Text="<%$Tokens:StringResource, admin.multiimage.ProductPrompt %>" />
				</div>
				<div class="col-sm-10">
					<a href="javascript:window.close();"><%=ProductName %> (ProductID=<%=ProductID.ToString() %>)</a>
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-2">
					<asp:Label ID="lblSizePrompt" runat="server" Text="<%$Tokens:StringResource, admin.multiimage.SizePrompt %>" />
				</div>
				<div class="col-sm-10">
					<%=TheSize.ToUpper() %>
				</div>
			</div>
			<div class="row admin-row">
				<div class="col-sm-12">
					<asp:Label ID="lblPageInfo" runat="server" Text="<%$Tokens:StringResource, admin.multiimage.PageInfo %>" />
				</div>
			</div>
		</div>
		<div>
			<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
		</div>
		<div align="left">
			<form enctype="multipart/form-data" action="multiimagemanager.aspx?size=<%=TheSize %>&productid=<%=ProductID.ToString() %>&VariantID=<%=VariantID.ToString() %>" method="post" id="MultiImageForm" name="MultiImageForm" onsubmit="return (validateForm(this) && MultiImageForm_Validator(this));" onreset="return confirm('<%=AspDotNetStorefrontCore.AppLogic.GetString("admin.multiimage.ResetConfim",ThisCustomer.LocaleSetting)%>');">
				<p align="left"><a href="javascript:window.close();" class="btn btn-default"><%=AspDotNetStorefrontCore.AppLogic.GetString("admin.common.close", ThisCustomer.LocaleSetting) %></a><input class="btn btn-default" type="reset" value="<%=AspDotNetStorefrontCore.AppLogic.GetString("admin.common.reset", ThisCustomer.LocaleSetting) %>" name="reset" /><input class="btn btn-primary" type="submit" value="<%=AspDotNetStorefrontCore.AppLogic.GetString("admin.common.save", ThisCustomer.LocaleSetting)%>" name="submit" /></p>
				<input type="hidden" name="IsSubmit" value="true" />
				<asp:Literal ID="ltContent" runat="server" />
				<p align="left"><a href="javascript:window.close();" class="btn btn-default"><%=AspDotNetStorefrontCore.AppLogic.GetString("admin.common.close", ThisCustomer.LocaleSetting) %></a><input class="btn btn-default" type="reset" value="<%=AspDotNetStorefrontCore.AppLogic.GetString("admin.common.reset", ThisCustomer.LocaleSetting) %>" name="reset" /><input class="btn btn-primary" type="submit" value="<%=AspDotNetStorefrontCore.AppLogic.GetString("admin.common.save", ThisCustomer.LocaleSetting)%>" name="submit" /></p>
			</form>
		</div>
	</div>
</body>
<script type="text/javascript">
	function MultiImageForm_Validator(theForm) {
		submitonce(theForm);
		return (true);
	}
</script>
</html>