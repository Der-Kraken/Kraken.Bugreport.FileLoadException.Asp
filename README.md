# Kraken.Bugreport.FileLoadException.Asp
App to reproduce a bug if running code impersonated on IIS.

_The project based on Asp.Net-Core 3.1 (but the error occurrs in dot.Net 5 too)._

# Details
This a a small project to reproduce an error which occurrs in our production one. 

## Explanation of Production Project

In our real project there is a database which requires access with an impersonated end-user (ActiveDirectory).
So before accessing the database (with EntityFrameworkCore) the context is impersonated with 
```System.Security.Principal.WindowsIdentity.RunImpersonated(winIdent.AccessToken, delegateAccessingDatabase)```.

Sometimes the webserver crashes with an FileLoadException 'AccessDenied' for the assembly System.Reflection.Extension.

## Explanation of Demo Project

In this demo project there is no database access in order to reduce complexitiy. 
Instead there is used a nuget called Flee (we using that nuget in the real project too and there is happening the same error but with another assembly).

## Explanation of the Problem

The error reason ist because the webfolder on wwwroot is readable only for the apppool user. When IIS starts the app then dot.Net loads some binaries but not all (just in time). 
If the code which runs impersonated try to load an binary (because it is the first who access is) then dot.Net will throw an FileLoadException because reading the binaries is not allowed for common end users.

Our workaround is to allow end users acessing wwwroot. But that is a security problem.

# How to reproduce

The error happens only in production after the project has been published to IIS. In order you can impersonate you will need AD-Users (I think).


1. Setup
   - Setup an IIS-WebSite with its own AppPool (see see screens below for configuration)
   - Run publish on VisualStudio with the profile **FolderProfile.pubxml**
   - Copy the compiled 'publish'-binaries to the websites folder in wwwroot
   - Allow the IISuser permission on the binaries
   - Allow one ActiveDirectory user (we call it **UserA**) reading+executing the binaries
   - Disalow another ActiveDirectory user (we call it **UserB**) permissions for the binaries
2. App test without problems
   - Browse the website with **UserA**
   - Click on the first link (you will see the number 6)
   - Browse back and click the second link
   - Check if first line prints ```The code has been run: I have Impersonated``` if not then impersonation has not worked and AD or something was configured wrong
   - Change user and browse the website again with **UserB**
   - Do the same things as before. All should work the same (because the DLL's are already loaded)
3. App test with problems
   - Restart the AppPool or wait until the website goes to sleep
   - Browse the webiste with **UserB**
   - **Important** click on the second link first (with impersonation)
   - You will see the 'FileLoadException' because **UserB** has no native permission to read binaries
   - Note that if you had clicked the first link before the second one then has no problem because the DLL has already been loaded

# Attachements

## Configuration on IIS



