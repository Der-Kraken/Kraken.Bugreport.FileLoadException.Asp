using System.Collections.Generic;

namespace Kraken.Bugreport.FileLoadException.Share
{
    public interface ICalculate
    {
        decimal Calculate(string formula, Dictionary<string, object> parameter);
    }
}
