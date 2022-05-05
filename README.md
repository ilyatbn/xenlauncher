# XenLauncher - A Citrix web service for launching published applications
This web app is an alternative to Citrix Storefront (Web Interface) that will generate ICA files using a single GET request. It can easily be embeded in any website or application, added to favorites or set as desktop icons.  
It can also be used as a server-side tool for ICA file generation of different users (Like secure browsing technologies). This app does not integrate with, or require the use of Storefront/Web Interface or it's APIs.


# Features:
- Request any publised app directly by the user.
- Request published apps on behalf or users (Impersonation).


# Configuration:
web.config - modify appSettings and add your Citrix farms. 7.x is compatible with CVAD. 
ImpUser is the only user that can impersonate launch requests for other users.  
Example:  
```
<add key="Farm:MyFarmName" value="6.x,https://MyDataCollector.MyCompany.io"/>
<add key="Farm:Xs-Farm" value="7.x,http://MyDeliveryController.MyCompany.io"/>
<add key="ImpUser" value="MyDomain\MyUsername"/>  
```  

# Installation:
1. Copy all files to a folder on your server.
2. Install IIS with .Net Framework 4.5.1 or higher.
3. Add your farms in web.config
3. Create an application within IIS and point it to the folder.
4. Enable Windows Authentication.


# Known Issues
- Session sharing will not work between apps launched by XenLauncher and Storefront/Workspace App.


# Compatibility
- Xenapp 6.5
- Xenapp and Xendesktop 7.x
- Virtual Apps And Desktops 1912 LTSR
- Virtual Apps and Desktops 2xxx CR (Early versions worked. Not sure about anything after late 2020 releases)

***This code is not maintained anymore.***