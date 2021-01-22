using System.Collections.Generic;

namespace Kraken.Bugreport.FileLoadException.Share
{
    public interface IBusinessLogicHandler
    {
        decimal Handle(string formula, Dictionary<string, object> parameter);
    }
}