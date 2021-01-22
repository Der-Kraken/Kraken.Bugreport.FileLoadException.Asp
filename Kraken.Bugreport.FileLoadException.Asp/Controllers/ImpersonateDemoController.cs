using Kraken.Bugreport.FileLoadException.Share;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Kraken.Bugreport.FileLoadException.Asp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImpersonateDemoController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public ImpersonateDemoController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            var linkA = "<a href=\"ImpersonateDemo/withoutimpersonation\">- Without Impersonation</a>";
            var linkB = "<a href=\"ImpersonateDemo/useimpersonation\">- Use Impersonation</a>";

            return Content($"<html><body>{linkA}</br></br>{linkB}</body></html>", "text/html");
        }

        [HttpGet("withoutimpersonation")]
        public ActionResult<string> WithoutImpersonation()
        {
            var handler = (IBusinessLogicHandler)_serviceProvider.GetService(typeof(IBusinessLogicHandler));
            var result = handler.Handle("1+length+3", new Dictionary<string, object>() { { "length", 2 }, });

            return result.ToString();
        }

        [HttpGet("useimpersonation")]
        public ActionResult<string> UseImpersonation()
        {
            decimal? result = null;

            var mode = RunImpersonatedIfRequired(() =>
            {
                var handler = (IBusinessLogicHandler)_serviceProvider.GetService(typeof(IBusinessLogicHandler));
                result = handler.Handle("1+length+3", new Dictionary<string, object>() { { "length", 2 }, });
            });

            return $"The code has been run: {mode}" + Environment.NewLine
                + $"Result: {result?.ToString() ?? "[NULL]"}" + Environment.NewLine
                + $"Time: {DateTime.Now.ToLongTimeString()}"
                ;
        }

        public string RunImpersonatedIfRequired(Action action)
        {
            var winIdent = HttpContext?.User?.Identity as WindowsIdentity;
            if (winIdent == null)
            {
                action.Invoke();
                return "Not Impersonated";
            }
            else
            {
                WindowsIdentity.RunImpersonated(winIdent.AccessToken, action);
                return "I have Impersonated";
            }
        }
    }
}
