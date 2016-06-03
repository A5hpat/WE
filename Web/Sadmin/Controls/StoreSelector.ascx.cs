// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

public partial class StoreSelector : System.Web.UI.UserControl
{
	EventHandler _SelectedIndexChanged;

	public StoreSelector()
	{
		AutoPostBack = false;
	}

	public event EventHandler SelectedIndexChanged
	{
		add
		{
			_SelectedIndexChanged += value;
		}
		remove
		{
			_SelectedIndexChanged -= value;
		}
	}

	public bool ShowText
	{
		get
		{
			return lblText.Visible;
		}
		set
		{
			lblText.Visible = value;
		}
	}

	public string Text
	{
		get
		{
			return lblText.Text;
		}
		set
		{
			lblText.Text = value;
		}
	}

	public int StoreCount
	{
		get
		{
			return Store.StoreCount;
		}
	}

	public bool ShowDefaultForAllStores { get; set; }

	public bool AutoPostBack { get; set; }

	public RepeatDirection ListRepeatDirection { get; set; }

	public StoreSelectedMode SelectMode { get; set; }

	public int SelectedIndex
	{
		get
		{
			if(SelectMode == StoreSelectedMode.SingleDropDown)
				return cmbSingleList.SelectedIndex;
			if(SelectMode == StoreSelectedMode.SingleRadioList)
				return lstSingleSelect.SelectedIndex;
			return -1;
		}
		set
		{
			if(SelectMode == StoreSelectedMode.SingleDropDown)
				cmbSingleList.SelectedIndex = value;
			if(SelectMode == StoreSelectedMode.SingleRadioList)
				lstSingleSelect.SelectedIndex = value;

		}
	}

	public int SelectedStoreID
	{
		get
		{
			if(SelectMode == StoreSelectedMode.SingleDropDown)
			{
				return int.Parse(cmbSingleList.SelectedValue);
			}
			else if(SelectMode == StoreSelectedMode.SingleRadioList)
			{
				return int.Parse(lstSingleSelect.SelectedValue);
			}

			return -1;
		}
		set
		{
			if(SelectMode == StoreSelectedMode.SingleDropDown)
			{
				if (cmbSingleList.Items.FindByValue(value.ToString()) != null)
					cmbSingleList.SelectedValue = value.ToString();
			}
			else if(SelectMode == StoreSelectedMode.SingleRadioList)
			{
				if(lstSingleSelect.Items.FindByValue(value.ToString()) != null)
					lstSingleSelect.SelectedValue = value.ToString();
			}
		}
	}

	public int[] SelectedStoreIDs
	{
		get
		{
			List<int> selectedStores = new List<int>();
			foreach(ListItem xItm in lstMultiSelect.Items)
			{
				if(xItm.Selected)
				{
					selectedStores.Add(int.Parse(xItm.Value));
				}
			}
			return selectedStores.ToArray();
		}
		set
		{
			List<int> storeList = new List<int>(value);
			foreach(ListItem xItm in lstMultiSelect.Items)
			{
				xItm.Selected = false;
			}
			foreach(ListItem xItm in lstMultiSelect.Items)
			{
				foreach(int store in storeList)
				{
					if(int.Parse(xItm.Value) == store)
					{
						xItm.Selected = true;
						storeList.Remove(store);
						break;
					}
				}
			}
		}
	}

	public int[] UnSelectedStoreIDs
	{
		get
		{
			List<int> selectedStores = new List<int>();
			foreach(ListItem xItm in lstMultiSelect.Items)
			{
				if(!xItm.Selected)
				{
					selectedStores.Add(int.Parse(xItm.Value));
				}
			}
			return selectedStores.ToArray();
		}
	}

	protected override void OnInit(EventArgs e)
	{
		if(IsPostBack)
			return;

		GetData();

		base.OnLoad(e);
	}

	protected void lstSingleSelect_SelectedIndexChanged(object sender, EventArgs e)
	{
		if(_SelectedIndexChanged != null)
		{
			_SelectedIndexChanged(this, e);
		}
	}

	void GetData()
	{
		var stores = Store.GetStoreList();
		var storeListItems = stores
			.Select(s => new ListItem(s.Name, s.StoreID.ToString()))
			.ToArray();

		switch(SelectMode)
		{
			case StoreSelectedMode.MultiCheckList:
				lstMultiSelect.RepeatDirection = ListRepeatDirection;
				lstMultiSelect.AutoPostBack = this.AutoPostBack;
				lstMultiSelect.Items.AddRange(storeListItems);
				break;

			case StoreSelectedMode.SingleRadioList:
				lstSingleSelect.RepeatDirection = ListRepeatDirection;
				lstSingleSelect.AutoPostBack = this.AutoPostBack;
				lstSingleSelect.Items.AddRange(storeListItems);
				if(ShowDefaultForAllStores)
					cmbSingleList.Items.Insert(0, new ListItem(AppLogic.GetString("admin.storeselector.default", Context.GetCustomer().LocaleSetting), "0"));
				break;

			case StoreSelectedMode.SingleDropDown:
				cmbSingleList.AutoPostBack = this.AutoPostBack;
				cmbSingleList.Items.AddRange(storeListItems);
				if(ShowDefaultForAllStores)
					cmbSingleList.Items.Insert(0, new ListItem(AppLogic.GetString("admin.storeselector.default", Context.GetCustomer().LocaleSetting), "0"));

				break;
		}

		if(stores.Count > 1)
		{
			lstMultiSelect.Visible = SelectMode == StoreSelectedMode.MultiCheckList;
			lstSingleSelect.Visible = SelectMode == StoreSelectedMode.SingleRadioList;
			cmbSingleList.Visible = SelectMode == StoreSelectedMode.SingleDropDown;
		}
		else
		{
			lblText.Visible = false;
			lstMultiSelect.Visible = false;
			lstSingleSelect.Visible = false;
			cmbSingleList.Visible = false;
		}
	}

	public enum StoreSelectedMode
	{
		SingleRadioList,
		SingleDropDown,
		MultiCheckList
	}
}
