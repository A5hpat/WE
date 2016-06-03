-- ------------------------------------------------------------------------------------------
-- Copyright AspDotNetStorefront.com.  All Rights Reserved.
-- http://www.aspdotnetstorefront.com
-- For details on this license please visit our homepage at the URL above.
-- THE ABOVE NOTICE MUST REMAIN INTACT.
-- ------------------------------------------------------------------------------------------

-- ------------------------------------------------------------------------------------------
-- Make sure to set your old skin id properly
-- You may need to run this for each skin you have
-- ------------------------------------------------------------------------------------------

SET NOEXEC OFF
GO

declare @OldSkinId varchar(20)
set @OldSkinId = ''

IF @OldSkinId = ''
BEGIN
	print 'THE SCRIPT DID NOT REALLY RUN.'
	print 'MAKE SURE TO SET YOUR @OldSkinId variable.'
	SET NOEXEC ON
END

update Topic set description = replace(description, '/App_Templates/Skin_' + @OldSkinId, '(!SkinPath!)')
update Topic set description = replace(description, '/App_Themes/Skin_' + @OldSkinId, '(!SkinPath!)')
update Topic set description = replace(description, '/App_Templates/Skin_(!SKINID!)', '(!SkinPath!)')
update Topic set description = replace(description, '/App_Themes/Skin_(!SKINID!)', '(!SkinPath!)')
update Topic set description = replace(description, 'App_Templates/Skin_' + @OldSkinId, '(!SkinPath!)')
update Topic set description = replace(description, 'App_Themes/Skin_' + @OldSkinId, '(!SkinPath!)')
update Topic set description = replace(description, 'App_Templates/Skin_(!SKINID!)', '(!SkinPath!)')
update Topic set description = replace(description, 'App_Themes/Skin_(!SKINID!)', '(!SkinPath!)')


update Product set description = replace(description, '/App_Templates/Skin_' + @OldSkinId, '(!SkinPath!)')
update Product set description = replace(description, '/App_Themes/Skin_' + @OldSkinId, '(!SkinPath!)')
update Product set description = replace(description, '/App_Templates/Skin_(!SKINID!)', '(!SkinPath!)')
update Product set description = replace(description, '/App_Themes/Skin_(!SKINID!)', '(!SkinPath!)')
update Product set description = replace(description, 'App_Templates/Skin_' + @OldSkinId, '(!SkinPath!)')
update Product set description = replace(description, 'App_Themes/Skin_' + @OldSkinId, '(!SkinPath!)')
update Product set description = replace(description, 'App_Templates/Skin_(!SKINID!)', '(!SkinPath!)')
update Product set description = replace(description, 'App_Themes/Skin_(!SKINID!)', '(!SkinPath!)')

update Category set description = replace(description, '/App_Templates/Skin_' + @OldSkinId, '(!SkinPath!)')
update Category set description = replace(description, '/App_Themes/Skin_' + @OldSkinId, '(!SkinPath!)')
update Category set description = replace(description, '/App_Templates/Skin_(!SKINID!)', '(!SkinPath!)')
update Category set description = replace(description, '/App_Themes/Skin_(!SKINID!)', '(!SkinPath!)')
update Category set description = replace(description, 'App_Templates/Skin_' + @OldSkinId, '(!SkinPath!)')
update Category set description = replace(description, 'App_Themes/Skin_' + @OldSkinId, '(!SkinPath!)')
update Category set description = replace(description, 'App_Templates/Skin_(!SKINID!)', '(!SkinPath!)')
update Category set description = replace(description, 'App_Themes/Skin_(!SKINID!)', '(!SkinPath!)')

update Manufacturer set description = replace(description, '/App_Templates/Skin_' + @OldSkinId, '(!SkinPath!)')
update Manufacturer set description = replace(description, '/App_Themes/Skin_' + @OldSkinId, '(!SkinPath!)')
update Manufacturer set description = replace(description, '/App_Templates/Skin_(!SKINID!)', '(!SkinPath!)')
update Manufacturer set description = replace(description, '/App_Themes/Skin_(!SKINID!)', '(!SkinPath!)')
update Manufacturer set description = replace(description, 'App_Templates/Skin_' + @OldSkinId, '(!SkinPath!)')
update Manufacturer set description = replace(description, 'App_Themes/Skin_' + @OldSkinId, '(!SkinPath!)')
update Manufacturer set description = replace(description, 'App_Templates/Skin_(!SKINID!)', '(!SkinPath!)')
update Manufacturer set description = replace(description, 'App_Themes/Skin_(!SKINID!)', '(!SkinPath!)')

update Section set description = replace(description, '/App_Templates/Skin_' + @OldSkinId, '(!SkinPath!)')
update Section set description = replace(description, '/App_Themes/Skin_' + @OldSkinId, '(!SkinPath!)')
update Section set description = replace(description, '/App_Templates/Skin_(!SKINID!)', '(!SkinPath!)')
update Section set description = replace(description, '/App_Themes/Skin_(!SKINID!)', '(!SkinPath!)')
update Section set description = replace(description, 'App_Templates/Skin_' + @OldSkinId, '(!SkinPath!)')
update Section set description = replace(description, 'App_Themes/Skin_' + @OldSkinId, '(!SkinPath!)')
update Section set description = replace(description, 'App_Templates/Skin_(!SKINID!)', '(!SkinPath!)')
update Section set description = replace(description, 'App_Themes/Skin_(!SKINID!)', '(!SkinPath!)')

SET NOEXEC OFF
GO
