using Kraken.Bugreport.FileLoadException.Share;
using System;
using System.Collections.Generic;

namespace Kraken.Bugreport.FileLoadException.Core
{
    public class BusinessLogicHandler : IBusinessLogicHandler
    {
        private readonly ICalculate _calculate;

        public BusinessLogicHandler(ICalculate calculate)
        {
            _calculate = calculate ?? throw new ArgumentNullException(nameof(calculate));
        }

        public decimal Handle(string formula, Dictionary<string, object> parameter)
        {
            var result = _calculate.Calculate(formula, parameter);
            return result;
        }
    }
}
