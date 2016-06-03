-- ------------------------------------------------------------------------------------------
-- Copyright AspDotNetStorefront.com.  All Rights Reserved.
-- http://www.aspdotnetstorefront.com
-- For details on this license please visit our homepage at the URL above.
-- THE ABOVE NOTICE MUST REMAIN INTACT.
-- ------------------------------------------------------------------------------------------
-- Run this to overwrite / install existing skin topics that are incompatible with ADNSF 10.0.0.0

-- THIS WILL DELETE EXISTING TOPICS WITH THE SAME NAMES AS THE FOLLOWING --
DECLARE @StoreId int;
SET @StoreId = 1;

delete from [dbo].[Topic] where Name = 'HomeTopIntro' AND StoreID = @StoreId
	insert into [dbo].[Topic] ([TopicGUID], [Name], [Title], [HTMLOk], [Published], [IsFrequent], [StoreID], [Description]) 
	values (newid(), N'HomeTopIntro', N'Home Page Contents', 1, 1, 1, @StoreId, N'
		(!TOPIC Name="HomePage.HomeImage"!)
		<div class="row">
			<div class="col-md-4">
				<div class="home-image">
					<img src="(!SkinPath!)/images/home1.jpg" alt="Store Image One" class="img-responsive center-block" />
				</div>
			</div>
			<div class="col-md-4">
				<div class="home-image">
					<img src="(!SkinPath!)/images/home2.jpg" alt="Store Image Two" class="img-responsive center-block" />
				</div>
			</div>
			<div class="col-md-4">
				<div class="home-image">
					<img src="(!SkinPath!)/images/home3.jpg" alt="Store Image Three" class="img-responsive center-block" />
				</div>
			</div>
		</div>

		<div class="row">
			<div class="col-md-6">
				<p>Your AspDotNetStorefront store is a thing of beauty right out of the box, but it''s almost certain that you''re going to want to stamp your own brand identity onto the ''skin''.</p>
				<h3>Three ways to personalize your store design</h3>
				<div class="row">
					<div class="col-md-4">
						<a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=adminskinning" class="thumbnail" target="_blank">
							<img alt="Use the admin wizard" src="(!SkinPath!)/images/box1.jpg" />
						</a>
					</div>
					<div class="col-md-4">
						<a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=learntoskin" class="thumbnail" target="_blank">
							<img alt="Learn how to skin a store" src="(!SkinPath!)/images/box2.jpg" />
						</a>
					</div>
					<div class="col-md-4">
						<a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=expertskinning" class="thumbnail" target="_blank">
							<img alt="Hire the experts to help" src="(!SkinPath!)/images/box3.jpg" />
						</a>
					</div>
				</div>
				<div class="row">
					<div class="col-md-4 text-center">
						<h4>
							<a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=adminskinning" target="_blank">
								Use the provided admin wizard
							</a>
						</h4>
					</div>
					<div class="col-md-4 text-center">
						<h4>
							<a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=learntoskin" target="_blank">Learn how to ''skin'' a store</a>
						</h4>
					</div>
					<div class="col-md-4 text-center">
						<h4>
							<a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=expertskinning" target="_blank">Hire the experts to help</a>
						</h4>
					</div>
				</div>
			</div>
			<div class="col-md-6">
				<div class="home-image">
					<img src="(!SkinPath!)/images/home4.jpg" alt="Store Image Four" class="img-responsive center-block" />
				</div>
			</div>
		</div>
		')

delete from [dbo].[Topic] where Name = 'HomePage.HomeImage' AND StoreID = @StoreId
	insert into [dbo].[Topic] ([TopicGUID], [Name], [Title], [HTMLOk], [Published], [IsFrequent], [StoreID], [Description]) 
	values (newid(), N'HomePage.HomeImage', N'The main home page image area', 1, 1, 1, @StoreId, N'
	<div class="home-image home-main-image">
		<img src="(!SkinPath!)/images/home.jpg" alt="AspDotNetStorefront" class="img-responsive center-block" />
	</div>
	')

delete from [dbo].[Topic] where Name = 'Template.Logo' AND StoreID = @StoreId
	insert into [dbo].[Topic] ([TopicGUID], [Name], [Title], [HTMLOk], [Published], [IsFrequent], [StoreID], [Description]) 
	values (newid(), N'Template.Logo', N'Template.Logo', 1, 1, 1, @StoreId, N'
	<a id="logo" class="logo" href="(!Url ActionName=''Index'' ControllerName=''Home''!)" title="This image size is 250px x 87px">
		<img src="(!SKINPATH!)/images/logo.jpg" alt="YourCompany.com" />
	</a>
	')

delete from [dbo].[Topic] where Name = 'Template.Footer' AND StoreID = @StoreId
	insert into [dbo].[Topic] ([TopicGUID], [Name], [Title], [HTMLOk], [Published], [IsFrequent], [StoreID], [Description]) 
	values (newid(), N'Template.Footer', N'The footer section of the template', 1, 1, 1, @StoreId, N'
	<div class="row">
		<div class="col-sm-12 col-md-3">
			<ul class="footer-list">
				<li class="footer-heading">Location & Hours</li>
				<li>1234 Main St.</li>
				<li>Ashland, OR 97520</li>
				<li>Phone: 541-867-5309</li>
				<li>M-F 9am - 5pm</li>
				<li><a href="(!Url ActionName=''Index'' ControllerName=''ContactUs''!)">Contact Us</a></li>
			</ul>
		</div>
		<div class="col-sm-12 col-md-3">
			<ul class="footer-list">
				<li class="footer-heading">Store Policies</li>
				<li><a href="(!TopicLink name=''security''!)">Security</a></li>
				<li><a href="(!TopicLink name=''privacy''!)">Privacy Policy</a></li>
				<li><a href="(!TopicLink name=''returns''!)">Return Policy</a></li>
				<li><a href="(!TopicLink name=''service''!)">Customer Service</a></li>
			</ul>
		</div>
		<div class="col-sm-12 col-md-3">
			<ul class="footer-list">
				<li class="footer-heading">Store Information</li>
				<li><a href="(!Url ActionName=''Index'' ControllerName=''Account''!)">Account</a></li>
				<li><a href="(!Url ActionName=''Index'' ControllerName=''Account''!)#OrderHistory">Order Tracking</a></li>
				<li><a href="(!Url ActionName=''Index'' ControllerName=''SiteMap''!)">Site Map</a></li>
			</ul>
		</div>
		<div class="col-sm-12 col-md-3">
			<ul class="footer-list">
				<li>
					<div class="social-links">
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=facebook"><i class="icon fa fa-facebook"></i></a>		
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=instagram"><i class="icon fa fa-instagram"></i></a>
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=pinterest"><i class="icon fa fa-pinterest"></i></a>	
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=twitter"><i class="icon fa fa-twitter"></i></a>				
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=youtube"><i class="icon fa fa-youtube"></i></a>		
					</div>
				</li>
				<li>
					<div class="seal-marker">
						<img src="(!SkinPath!)/images/seal.png" />
					</div>
				</li>
			</ul>
		</div>
	</div>
	')
