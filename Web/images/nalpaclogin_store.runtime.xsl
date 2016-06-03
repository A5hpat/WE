<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:aspdnsf="urn:aspdnsf" exclude-result-prefixes="aspdnsf">
  <xsl:output method="html" omit-xml-declaration="yes" />
  <xsl:template match="/">
    <xsl:if test="root/Runtime/CustomerIsRegistered = 'false'">
      <div>Customer is not registered</div>
    </xsl:if>
    <xsl:if test="root/Runtime/CustomerIsRegistered = 'true'">
      <div class="cnt-account">
        <ul class="user-links">
          <li class="hidden-xs hidden-sm">
            <a href="/nalpac4/account.aspx" class="user-link">
              <i class="fa fa-user"></i>
              <xsl:value-of select="/root/customers/custs/LastName"></xsl:value-of>
            </a>
          </li>
          <li>
            <a href="/nalpac4/signout.aspx" class="user-link log-out-link">
              <i class="fa fa-sign-out"></i>
                  Logout
                </a>
          </li>
        </ul>
      </div>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>