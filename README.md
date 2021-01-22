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

## StackTrace


```
System.IO.FileLoadException: 
File name: 'Flee.NetStandard20, Version=1.0.0.0, Culture=neutral, PublicKeyToken=85238e57c1c34b81'
   at Kraken.Bugreport.FileLoadException.Asp.FleeAdapter.Calculate(String formula, Dictionary`2 parameter)
   at Kraken.Bugreport.FileLoadException.Core.BusinessLogicHandler.Handle(String formula, Dictionary`2 parameter) in D:\_src\temp\Kraken.Bugreport.FileLoadException.Asp\Kraken.Bugreport.FileLoadException.Core\BusinessLogicHandler.cs:line 18
   at Kraken.Bugreport.FileLoadException.Asp.Controllers.ImpersonateDemoController.<>c__DisplayClass4_0.<UseImpersonation>b__0() in D:\_src\temp\Kraken.Bugreport.FileLoadException.Asp\Kraken.Bugreport.FileLoadException.Asp\Controllers\ImpersonateDemoController.cs:line 46
   at System.Security.Principal.WindowsIdentity.<>c__DisplayClass67_0.<RunImpersonatedInternal>b__0(Object <p0>)
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location where exception was thrown ---
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Security.Principal.WindowsIdentity.RunImpersonatedInternal(SafeAccessTokenHandle token, Action action)
   at System.Security.Principal.WindowsIdentity.RunImpersonated(SafeAccessTokenHandle safeAccessTokenHandle, Action action)
   at Kraken.Bugreport.FileLoadException.Asp.Controllers.ImpersonateDemoController.RunImpersonatedIfRequired(Action action) in D:\_src\temp\Kraken.Bugreport.FileLoadException.Asp\Kraken.Bugreport.FileLoadException.Asp\Controllers\ImpersonateDemoController.cs:line 66
   at Kraken.Bugreport.FileLoadException.Asp.Controllers.ImpersonateDemoController.UseImpersonation() in D:\_src\temp\Kraken.Bugreport.FileLoadException.Asp\Kraken.Bugreport.FileLoadException.Asp\Controllers\ImpersonateDemoController.cs:line 43
   at lambda_method(Closure , Object , Object[] )
   at Microsoft.Extensions.Internal.ObjectMethodExecutor.Execute(Object target, Object[] parameters)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.SyncObjectResultExecutor.Execute(IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeActionMethodAsync()
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeNextActionFilterAsync()
--- End of stack trace from previous location where exception was thrown ---
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
--- End of stack trace from previous location where exception was thrown ---
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|19_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|6_0(Endpoint endpoint, Task requestTask, ILogger logger)
   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware.Invoke(HttpContext context)
```

## Configuration on IIS

![iisconfig](https://github.com/Der-Kraken/Kraken.Bugreport.FileLoadException.Asp/blob/master/blob/apppoolconfig.png?raw=true)

