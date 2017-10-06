using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ReliablePubSub.Common
{
    public interface IKeyExtractor<in TValue, out TKey>
    {
        TKey Extract(TValue value);
    }

    public class DefaultKeyExtractor<TValue, TKey> : IKeyExtractor<TValue, TKey>
    {
        private readonly Func<TValue, TKey> _keyExtractorFunc;

        public DefaultKeyExtractor(Expression<Func<TValue, TKey>> keyExtractorExpression)
        {
            _keyExtractorFunc = keyExtractorExpression.Compile();
        }

        public TKey Extract(TValue value)
        {
            return _keyExtractorFunc(value);
        }
    }
}
