<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:aspdnsf="urn:aspdnsf" exclude-result-prefixes="aspdnsf">
  <xsl:output method="html" omit-xml-declaration="yes" />
  <xsl:param name="cartItems">
    <xsl:choose>
      <xsl:when test="sum(/root/ShoppingCart/Item/Quantity) &gt; 0">
        <xsl:value-of select="count(/root/ShoppingCart/Item/Quantity)" />
      </xsl:when>
      <xsl:otherwise>0</xsl:otherwise>
    </xsl:choose>
  </xsl:param>
  <xsl:template match="/">
    <!--<xsl:if test="root/Runtime/CustomerIsRegistered = 'false'">

          <div class="minicart-wrap" data-role="content">
            <div id="minicart-modal" class="modal  minicart-modal" data-keyboard="true" tabindex="-1">
              <div class="modal-dialog">
                <div class="modal-content">
                  <div class="modal-header">
                    -->
    <!--<a href="#" class="switch-mini-link js-switch-to-miniwish off">
                      <i class="fa fa-angle-left"></i>
                      View wishlist
                    </a>-->
    <!--
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                      <i class="fa fa-times-circle-o"></i>
                    </button>
                    <h4 class="minicart-title modal-title">
                      Shopping Cart:
                      <span class="minicart-title-count js-cart-title-count">
                        0
                      </span>
                      <span class="minicart-items-label js-cart-count-label">
                        Items
                      </span>
                    </h4>
                  </div>
                  <div class="modal-body">
                    <div class="minicart-content-wrap">
                      <div class="minicart-message-area js-minicart-message-area">

                      </div>
                      <div class="minicart-contents js-minicart-contents">
                        <div class="empty-mini-cart-text">
                          Your shopping cart is currently empty.<br />
                        </div>

                      </div>
                    </div>
                  </div>
                  <div class="modal-footer minicart-footer">
                    <div class="row">
                      <div class="col-sm-6 text-left-sm">
                        <div class="minicart-discount off">
                          Discounts:
                          <span class="minicart-discount js-minicart-discount"></span>
                        </div>
                        <div class="minicart-total-area">
                          <div class="minicart-total-wrap">
                            Total:
                            <span class="minicart-total js-minicart-total">$0.00</span>
                          </div>
                        </div>
                      </div>
                      <div class="col-sm-6">
                        <button type="button" id="minicart-close-button" class="btn btn-default close-minicart-button">Close</button>
                        <button type="button" id="minicart-checkout-button" class="btn btn-primary minicart-checkout-button">Checkout</button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </xsl:if>
        <xsl:if test="root/Runtime/CustomerIsRegistered = 'true'">-->
    <!-- Minicart modal -->
    <div class="minicart-wrap" data-role="content">
      <div id="minicart-modal" class="modal  minicart-modal" data-keyboard="true" tabindex="-1">
        <div class="modal-dialog">
          <div class="modal-content">
            <div class="modal-header">
              <!--<a href="#" class="switch-mini-link js-switch-to-miniwish">
                      <i class="fa fa-angle-left"></i>
                      View wishlist
                    </a>-->
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <i class="fa fa-times-circle-o"></i>
              </button>
              <h4 class="minicart-title modal-title">
                      Shopping Cart: 
                      <span class="minicart-title-count js-cart-title-count"><xsl:value-of select="root/ShoppingCartItems/NumItem/NumberOfItems"></xsl:value-of></span><span class="minicart-items-label js-cart-count-label">
                        Items
                      </span></h4>
            </div>
            <div class="modal-body">
              <div class="minicart-content-wrap">
                <div class="minicart-message-area js-minicart-message-area"></div>
                <div class="minicart-contents js-minicart-contents">
                  <form action="#" class="minicart-form" id="minicart-form" method="post">
                    <div class="minicart-items">
                      <xsl:apply-templates select="/root/ShoppingCart/Item" />
                    </div>
                  </form>
                </div>
              </div>
            </div>
            <div class="modal-footer minicart-footer">
              <div class="row">
                <div class="col-sm-6 text-left-sm">
                  <div class="minicart-discount off">
                          Discounts:
                          <span class="minicart-discount js-minicart-discount"></span></div>
                  <div class="minicart-total-area">
                    <div class="minicart-total-wrap">
                            Total:
                            <span class="minicart-total js-minicart-total"><xsl:value-of select="format-number(root/ShoppingCart/Items/SubTotal, '#.00')"></xsl:value-of></span></div>
                  </div>
                </div>
                <div class="col-sm-6">
                  <button type="button" id="minicart-close-button" class="btn btn-default close-minicart-button">Close</button>
                  <button type="button" class="btn btn-primary" href="(!Url ActionName='Index' ControllerName='Checkout' !)">
                          Checkout
                        </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!--</xsl:if>-->
  </xsl:template>
  <xsl:template match="Item">
    <input data-val="true" data-val-number="The field Id must be a number." data-val-required="The Id field is required." type="hidden" value="427">
      <xsl:attribute name="id">
        <xsl:value-of select="concat('CartItems_', LineNumber, '__Id')"></xsl:value-of>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="concat('CartItems[', LineNumber, '].Id')"></xsl:value-of>
      </xsl:attribute>
      <xsl:attribute name="value">
        <xsl:value-of select="Quantity"></xsl:value-of>
      </xsl:attribute>
    </input>
    <div class="minicart-item media">
      <div class="minicart-item-image-wrap media-left">
        <img class="media-object minicart-item-image" src="/nalpac4/skins/default/images/nopictureicon.gif">
          <xsl:attribute name="src">
            <xsl:value-of select="microimage"></xsl:value-of>
          </xsl:attribute>
        </img>
      </div>
      <div class="media-body">
        <div class="minicart-details">
          <div class="minicart-item-actions">
            <a href="/nalpac4/minicart/deleteminicartitem/427?cartType=ShoppingCart" title="Delete" class="js-minicart-delete-link minicart-delete-link text-danger">
              <i class="fa fa-times-circle-o"></i>
            </a>
          </div>
          <h5 class="minicart-item-title media-heading">
            <a class="minicart-product-name" href="/nalpac4/p-21887-adam-eve-adams-colossal-flesh.aspx">
              <xsl:attribute name="href">
                <xsl:value-of select="pname"></xsl:value-of>
              </xsl:attribute>
              <xsl:value-of select="ProductName"></xsl:value-of>
            </a>
          </h5>
          <div class="row">
            <div class="col-sm-7">
              <span class="minicart-item-quantity">
                <span class="minicart-quantity-label">Qty:</span>
                <xsl:value-of select="Quantity"></xsl:value-of>
              </span>
            </div>
            <div class="col-sm-5 text-right-sm">
              <div class="minicart-subtotal">
                <span class="minicart-subtotal-label">Subtotal:</span>
                <xsl:value-of select="format-number(ItemLinePrice, '###,###.00')"></xsl:value-of>
              </div>
            </div>
          </div>
        </div>
        <input data-val="true" data-val-number="The field ProductId must be a number." data-val-required="The ProductId field is required." type="hidden">
          <xsl:attribute name="id">
            <xsl:value-of select="concat('CartItems_',LineNumber, '__ProductId')"></xsl:value-of>
          </xsl:attribute>
          <xsl:attribute name="name">
            <xsl:value-of select="concat('CartItems[',LineNumber, '].ProductId')"></xsl:value-of>
          </xsl:attribute>
          <xsl:attribute name="value">
            <xsl:value-of select="ProductID"></xsl:value-of>
          </xsl:attribute>
        </input>
        <input data-val="true" data-val-number="The field VariantId must be a number." data-val-required="The VariantId field is required." type="hidden">
          <xsl:attribute name="id">
            <xsl:value-of select="concat('CartItems_',LineNumber, '__VariantId')"></xsl:value-of>
          </xsl:attribute>
          <xsl:attribute name="name">
            <xsl:value-of select="concat('CartItems[',LineNumber, '].VariantId')"></xsl:value-of>
          </xsl:attribute>
          <xsl:attribute name="value">
            <xsl:value-of select="VariantId"></xsl:value-of>
          </xsl:attribute>
        </input>
        <input type="hidden">
          <xsl:attribute name="id">
            <xsl:value-of select="concat('CartItems_',LineNumber, '__ChosenColorSkuModifier')"></xsl:value-of>
          </xsl:attribute>
          <xsl:attribute name="name">
            <xsl:value-of select="concat('CartItems[',LineNumber, '].ChosenColorSkuModifier')"></xsl:value-of>
          </xsl:attribute>
          <xsl:attribute name="value">
            <xsl:value-of select="ChosenColorSkuModifier"></xsl:value-of>
          </xsl:attribute>
        </input>
        <input id="CartItems_0__ChosenSizeSkuModifier" name="CartItems[0].ChosenSizeSkuModifier" type="hidden" value="" />
        <input id="CartItems_0__TextOption" name="CartItems[0].TextOption" type="hidden" value="" />
        <input data-val="true" data-val-min="Invalid quantity" data-val-min-val="0" data-val-number="The field Quantity must be a number." data-val-required="Invalid quantity" id="CartItems_0__Quantity" name="CartItems[0].Quantity" type="hidden" value="10" />
      </div>
    </div>
  </xsl:template>
</xsl:stylesheet>