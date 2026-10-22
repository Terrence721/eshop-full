namespace eShop.Ordering.API.Extensions
{
    /// <summary>
    /// LINQ helpers that apply a selector over an enumerable and capture any exception
    /// per element instead of aborting the whole sequence.
    /// </summary>
    public static class LinqSelectExtensions
    {
        public static IEnumerable<SelectTryResult<TSource, TResult>> SelectTry<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TResult> selector)
        {
            foreach (TSource element in enumerable)
            {
                SelectTryResult<TSource, TResult> returnedValue;
                try
                {
                    returnedValue = new SelectTryResult<TSource, TResult>(element, selector(element), null);
                }
                catch (Exception ex)
                {
                    // No meaningful TResult exists on the exception path -- same "no real
                    // value" situation as the default! pattern elsewhere in this migration.
                    returnedValue = new SelectTryResult<TSource, TResult>(element, default!, ex);
                }
                yield return returnedValue;
            }
        }

        public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(this IEnumerable<SelectTryResult<TSource, TResult>> enumerable, Func<Exception, TResult> exceptionHandler)
        {
            return enumerable.Select(x => x.CaughtException == null ? x.Result : exceptionHandler(x.CaughtException));
        }

        public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(this IEnumerable<SelectTryResult<TSource, TResult>> enumerable, Func<TSource, Exception, TResult> exceptionHandler)
        {
            return enumerable.Select(x => x.CaughtException == null ? x.Result : exceptionHandler(x.Source, x.CaughtException));
        }

        public class SelectTryResult<TSource, TResult>
        {
            // CaughtException is genuinely null on the success path (SelectTry passes null
            // explicitly above when the selector doesn't throw) -- real optionality, not a
            // gap in construction.
            internal SelectTryResult(TSource source, TResult result, Exception? exception)
            {
                Source = source;
                Result = result;
                CaughtException = exception;
            }

            public TSource Source { get; private set; }
            public TResult Result { get; private set; }
            public Exception? CaughtException { get; private set; }
        }
    }
}
