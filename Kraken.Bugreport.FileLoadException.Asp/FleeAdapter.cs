using Flee.PublicTypes;
using Kraken.Bugreport.FileLoadException.Share;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Kraken.Bugreport.FileLoadException.Asp
{
    public class FormelRechnerException : Exception
    {
        public FormelRechnerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class FleeAdapter : ICalculate
    {
        public bool ConvertParameterToDecimal { get; set; } = true;

        public decimal Calculate(string formula, Dictionary<string, object> parameter)
        {
            if (formula is null) throw new ArgumentNullException(nameof(formula));

            var fleeContext = CreateFleeContext();

            if (parameter?.Any() == true)
            {
                foreach (var param in parameter)
                {
                    var value = param.Value;

                    if (ConvertParameterToDecimal && param.Value != null)
                    {
                        if (param.Value is double
                            || param.Value is float)
                        {
                            value = Convert.ToDecimal(param.Value, fleeContext.Options.ParseCulture ?? CultureInfo.CurrentCulture);
                        }
                    }

                    fleeContext.Variables.Add(param.Key, value);
                }
            }

            IGenericExpression<object> expression;
            try
            {
                expression = fleeContext.CompileGeneric<object>(formula);
            }
            catch (Exception ex)
            {
                throw new FormelRechnerException("Die Formel bzw. deren Syntax ist invalide. ", ex);
            }

            try
            {
                var result = expression.Evaluate();
                return Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                throw new FormelRechnerException("Rechnen nicht möglich obwohl Formelsyntax valide ist. ", ex);
            }
        }

        private ExpressionContext CreateFleeContext()
        {
            var context = new ExpressionContext();

            // Configure Options
            {
                context.Options.ParseCulture = new CultureInfo("de-DE");
                context.Options.RealLiteralDataType = RealLiteralDataType.Decimal;
                context.Options.Checked = true;
            }

            // Configure ParserOptions
            {
                context.ParserOptions.DecimalSeparator = ',';
                context.ParserOptions.RequireDigitsBeforeDecimalPoint = true;
            }

            return context;
        }
    }
}
