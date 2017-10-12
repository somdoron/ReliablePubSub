using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ReliablePubSub.Common
{
    public interface IKeyExtractor
    {
        string Extract(object value);
    }

    public class DefaultKeyExtractor<TValue> : IKeyExtractor
    {
        private readonly Func<TValue, string> _keyExtractorFunc;

        public DefaultKeyExtractor(Expression<Func<TValue, string>> keyExtractorExpression)
        {
            _keyExtractorFunc = keyExtractorExpression.Compile();
        }

        public string Extract(TValue value)
        {
            return _keyExtractorFunc(value);
        }

        public string Extract(object value)
        {
            return Extract((TValue)value);
        }
    }
}
