<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="QuickStart.ascx.cs" Inherits="AspDotNetStorefrontControls.QuickStart" %>

<div class="white-ui-box">
	<div class="white-box-heading">
		<asp:LinkButton OnCommand="HideQuickStart" runat="server" ID="lnkHideQuickStart" CssClass="pull-right" Text="<%$Tokens:StringResource, admin.quickstart.disable %>" />
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.quickstart.header %>" />
	</div>
	<div class="quick-start-contents">
		<div class="row">
			<div class="col-md-3">
				<div class="card style-one">
					<em class="fa fa-globe card-icon"></em>
					<h3 class="card-header">
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000gettingstarted&amp;type=1" class="header-link">
							<span class="card-link-arrow">
								<em class="fa fa-chevron-right"></em>
							</span>
							Increase awareness
						</a>
					</h3>
					<div class="card-content">
						<p>Completely free-format URLs, Google microdata, and full SEO capabilities.</p>
					</div>
				</div>
			</div>
			<div class="col-md-3">
				<div class="card style-two">
					<em class="fa fa-mobile card-icon"></em>
					<h3 class="card-header">
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000gettingstarted&amp;type=2" class="header-link">
							<span class="card-link-arrow">
								<em class="fa fa-chevron-right"></em>
							</span>
							Mobility matters
						</a>
					</h3>
					<div class="card-content">
						<p>Natively 'responsive' theme (Bootstrap) that flexes to fit all screen sizes.</p>
					</div>
				</div>
			</div>
			<div class="col-md-3">
				<div class="card style-three">
					<em class="fa fa-shield card-icon"></em>
					<h3 class="card-header">
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000gettingstarted&amp;type=3" class="header-link">
							<span class="card-link-arrow">
								<em class="fa fa-chevron-right"></em>
							</span>
							Showcase security
						</a>
					</h3>
					<div class="card-content">
						<p>HTTPS everywhere (recommended by Google for security). Honors PCI<br /> regulations.</p>
					</div>
				</div>
			</div>
			<div class="col-md-3">
				<div class="card style-four">
					<em class="fa fa-shopping-cart card-icon"></em>
					<h3 class="card-header">
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000gettingstarted&amp;type=4" class="header-link">
							<span class="card-link-arrow">
								<em class="fa fa-chevron-right"></em>
							</span>
							All-in-one Checkout
						</a>
					</h3>
					<div class="card-content">
						<p>Gone is the shopping cart. No more registration page. New "checkout dashboard."</p>
					</div>
				</div>
			</div>
		</div>
		<div class="row">
			<div class="col-md-3">
				<div class="card style-three">
					<em class="fa fa-users card-icon"></em>
					<h3 class="card-header">
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000gettingstarted&amp;type=5" class="header-link">
							<span class="card-link-arrow">
								<em class="fa fa-chevron-right"></em>
							</span>
							Guest checkout
						</a>
					</h3>
					<div class="card-content">
						<p>Guest shopping - quick &amp; easy. Creating a login - ideal for returners. Offer both.</p>
					</div>
				</div>
			</div>
			<div class="col-md-3">
				<div class="card style-four">
					<em class="fa fa-credit-card card-icon"></em>
					<h3 class="card-header">
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000gettingstarted&amp;type=6" class="header-link">
							<span class="card-link-arrow">
								<em class="fa fa-chevron-right"></em>
							</span>
							Innovative payments
						</a>
					</h3>
					<div class="card-content">
						<p>Amazon Payments. PayPal. Braintree. Silent gateways. COD. Quotes. Credit options.</p>
					</div>
				</div>
			</div>
			<div class="col-md-3">
				<div class="card style-two">
					<em class="fa fa-lock card-icon"></em>
					<h3 class="card-header">
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000gettingstarted&amp;type=7" class="header-link">
							<span class="card-link-arrow">
								<em class="fa fa-chevron-right"></em>
							</span>
							Data protection
						</a>
					</h3>
					<div class="card-content">
						<p>Full PCI adherence. Passwords are hashed, card data is encrypted, admins are logged.</p>
					</div>
				</div>
			</div>
			<div class="col-md-3">
				<div class="card style-one">
					<em class="fa fa-rocket card-icon"></em>
					<h3 class="card-header">
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000gettingstarted&amp;type=8" class="header-link">
							<span class="card-link-arrow">
								<em class="fa fa-chevron-right"></em>
							</span>
							Easy admin
						</a>
					</h3>
					<div class="card-content">
						<p>Intuitive menus. Bulk operations. Find-and-filter options. Easy order management.</p>
					</div>
				</div>
			</div>
		</div>
	</div>
</div>
