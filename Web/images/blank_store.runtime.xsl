<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:aspdnsf="urn:aspdnsf" exclude-result-prefixes="aspdnsf">
  <xsl:output method="html" omit-xml-declaration="yes" />
  <xsl:template match="/">
    <div>
      <xsl:value-of select="/root/customers/custs/LastName"></xsl:value-of>
      <xsl:value-of select="/root/customers/custs/customerid"></xsl:value-of>
    </div>
    <div>
      <xsl:value-of select="root/Runtime/blu"></xsl:value-of>
    </div>
    <xsl:if test="root/Runtime/CustomerIsRegistered = 'false'">
      <div>Customer is not registered</div>
    </xsl:if>
    <xsl:if test="root/Runtime/CustomerIsRegistered = 'true'">
      <div>Customer IS registered</div>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>