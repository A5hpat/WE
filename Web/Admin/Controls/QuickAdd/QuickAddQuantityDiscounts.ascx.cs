// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;

using AspDotNetStorefrontCore;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls
{
	public partial class QuickAddQuantityDiscounts : BaseControl
	{
		#region Properties
		public int QuantityDiscountId { get; private set; }
		public string QuantityDiscountName { get; private set; }
		#endregion

		#region Event Handlers

		protected void Page_Load(Object sender, EventArgs e)
		{
			InitializeClientSideHelpers("QuantityDiscounts");
		}

		protected void btnSubmit_Click(Object sender, EventArgs e)
		{
			//only validates this button's validationgroup
			if(txtName.Text.Length > 0 && Page.IsValid)
				CreateQuantityDiscountTable();
		}

		#endregion

		#region Protected Methods

		private void InitializeClientSideHelpers(string suffix)
		{
            cmpTxtHighQuantity.ValidationGroup = string.Format("QuickAdd{0}", suffix);
			rfvName.ValidationGroup = string.Format("QuickAdd{0}", suffix);
            revTxtLowQuantity.ValidationGroup = string.Format("QuickAdd{0}", suffix);
			rfvHighQuantity.ValidationGroup = string.Format("QuickAdd{0}", suffix);
            revTxtDiscount.ValidationGroup = string.Format("QuickAdd{0}", suffix);          
			btnSubmit.ValidationGroup = string.Format("QuickAdd{0}", suffix);            
			pnlQuickAddFields.Attributes.Add("class", string.Format("QuickAdd{0} panel panel-default quick-add-field-panel", suffix));
			linkQuickAdd.Attributes.Add("class", string.Format("LinkAdd{0}", suffix));
			linkQuickClose.Attributes.Add("class", string.Format("LinkClose{0} btn btn-sm btn-default", suffix));
			ltScript.Text = string.Format(@"<script type='text/javascript'>
                                                    $('.LinkAdd{0}').click(function(event) {{
                                                        $('.QuickAdd{0}').show();
                                                    }});
                                                   $('.LinkClose{0}').click(function(event) {{
                                                        $('.QuickAdd{0}').hide();
                                                    }});
                                            </script>", suffix);
		}

		private void CreateQuantityDiscountTable()
		{
			try
			{
				var newGuid = Guid.NewGuid();
				var discountType = 0;
				int.TryParse(ddlDiscountType.SelectedValue, out discountType);

				using(var cn = new SqlConnection(DB.GetDBConn()))
				{
					cn.Open();
					using(var cmd = new SqlCommand(@"insert into quantitydiscount(QuantityDiscountGUID,Name,DiscountType) 
                                                                values(@newGuid, @name, @discountType)", cn))
					{
						cmd.Parameters.Add(new SqlParameter("@newGuid", SqlDbType.UniqueIdentifier));
						cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar));
						cmd.Parameters.Add(new SqlParameter("@discountType", SqlDbType.TinyInt));

						cmd.Parameters["@newGuid"].Value = newGuid;
						cmd.Parameters["@name"].Value = txtName.Text;
						cmd.Parameters["@discountType"].Value = discountType;
						cmd.ExecuteNonQuery();
					}
				}

				using(var conn = DB.dbConn())
				{
					conn.Open();
					using(var rs = DB.GetRS("select QuantityDiscountID, Name from QuantityDiscount with (NOLOCK) where QuantityDiscountGUID=" + DB.SQuote(newGuid.ToString()), conn))
					{
						rs.Read();
						QuantityDiscountId = DB.RSFieldInt(rs, "QuantityDiscountID");
						QuantityDiscountName = DB.RSField(rs, "Name");
					}
				}

				int low = 0;
				int high = 999999;
				decimal discount = 0m;
				int.TryParse(txtLowQuantity.Text, out low);
				int.TryParse(txtHighQuantity.Text, out high);
				decimal.TryParse(txtDiscount.Text, out discount);

				using(var cn = new SqlConnection(DB.GetDBConn()))
				{
					cn.Open();
					using(var cmd = new SqlCommand(@"insert into QuantityDiscountTable(QuantityDiscountTableGUID,QuantityDiscountID,LowQuantity,HighQuantity,DiscountPercent,CreatedOn) 
                                values(@newGuid,@quantityDiscountId,@lowQuantity,@highQuantity,@discountAmount,@createdOn)", cn))
					{

						cmd.Parameters.Add(new SqlParameter("@newGuid", SqlDbType.UniqueIdentifier));
						cmd.Parameters.Add(new SqlParameter("@quantityDiscountId", SqlDbType.Int));
						cmd.Parameters.Add(new SqlParameter("@lowQuantity", SqlDbType.Int));
						cmd.Parameters.Add(new SqlParameter("@highQuantity", SqlDbType.Int));
						cmd.Parameters.Add(new SqlParameter("@discountAmount", SqlDbType.Decimal));
						cmd.Parameters.Add(new SqlParameter("@createdOn", SqlDbType.DateTime));

						cmd.Parameters["@newGuid"].Value = Guid.NewGuid();
						cmd.Parameters["@quantityDiscountId"].Value = QuantityDiscountId;
						cmd.Parameters["@lowQuantity"].Value = low;
						cmd.Parameters["@highQuantity"].Value = high;
						cmd.Parameters["@discountAmount"].Value = discount;
						cmd.Parameters["@createdOn"].Value = DateTime.Now;
						cmd.ExecuteNonQuery();
					}
				}

				txtName.Text = txtDiscount.Text = txtHighQuantity.Text = txtLowQuantity.Text = string.Empty;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
			}
		}

		#endregion
       
	}    
}
