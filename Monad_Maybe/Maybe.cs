using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    /// <summary>
    /// Монада для возврата значения или ошибки:
    /// <code>
    /// var r = getUserInput();
    /// switch (r)
    /// {
    ///     case Success&lt;int&gt; i:
    ///         Console.WriteLine("User input: {0}", i.Value);
    ///         break;
    ///     case Failed&lt;int&gt; f:
    ///         Console.WriteLine("Error: {0}", f.Message);
    ///         break;           
    /// }
    ///
    /// или:
    /// 
    /// getUserInput()
    ///     .Case(succ => Console.WriteLine("User input: {0}", succ),
    ///           fail => Console.WriteLine("Error: {0}", fail.Message));
    /// ...
    /// static Maybe&lt;int&gt; getUserInput()
    /// {
    ///     var s1 = Console.ReadLine();
    ///     return int.TryParse(s1, out var value)
    ///         ? Maybe&lt;int&gt;.Success(value)
    ///         : Maybe&lt;int&gt;.Failed("Can't parse");
    /// }
    /// </code> 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Maybe<T>
    {
        public static Success<T> Success(T value) => new(value);

        #region Failed<T>

        public static Failed<T> Failed(string message, Exception? exception = null) => new(message, exception);

        public static Failed<T> Failed(Exception exception) => new(exception);

        public static Failed<T> Failed<FROM>(Maybe<FROM> from) =>
            from is not Failed<FROM> f
                ? throw new ArgumentException($"Only Failed<T> expected ({from.GetType()})")
                : new Failed<T>(f.Message, f.Exception);

        #endregion

        public static Maybe<T> Try(Func<T> factory)
        {
            try
            {
                return new Success<T>(factory());
            }
            catch (Exception e)
            {
                return new Failed<T>(e.InnerException ?? e);
            }
        }

        #region Case<RESULT>

        public void Case(Action<T> successAction, Action<Failed<T>>? failedAction = null)
        {
            switch (this)
            {
                case Success<T> success:
                    successAction(success.Value);
                    break;
                case Failed<T> failed:
                    failedAction?.Invoke(failed);
                    break;
                default: throw new ArgumentException(GetType().Name);
            }
        }

        public RESULT Case<RESULT>(Func<T, RESULT> successFunc, Func<Failed<T>, RESULT> failedFunc) =>
            this switch
            {
                Success<T> success => successFunc(success.Value),
                Failed<T> failed => failedFunc(failed),
                _ => throw new ArgumentException(GetType().Name)
            };

        /// <summary> асинхронная версия Case&lt;RESULT&gt; </summary>
        public Task<RESULT> Case<RESULT>(Func<T, Task<RESULT>> successFunc, Func<Failed<T>, Task<RESULT>> failedFunc) =>
            this switch
            {
                Success<T> success => successFunc(success.Value),
                Failed<T> failed => failedFunc(failed),
                _ => throw new ArgumentException(GetType().Name)
            };

        #endregion

        #region GetValueOrDefault

        public T GetValueOrDefault(T defaultValue) =>
            this switch
            {
                Success<T> success => success.Value,
                Failed<T> => defaultValue,
                _ => throw new ArgumentException(GetType().Name)
            };

        public T GetValueOrDefault(Func<T> defaultValueFactory) =>
            this switch
            {
                Success<T> success => success.Value,
                Failed<T> => defaultValueFactory(),
                _ => throw new ArgumentException(GetType().Name)
            };

        #endregion

        public static implicit operator Maybe<T>(T value) => new Success<T>(value);
    }

    #region Success<T> / Failed<T>

    [DebuggerDisplay("Success: {Value}")]
    public class Success<T> : Maybe<T>
    {
        internal Success(T value) => Value = value;

        public T Value { get; }
    }

    [DebuggerDisplay("Failed: {Message}")]
    public class Failed<T> : Maybe<T>
    {
        public string     Message   { get; }
        public Exception? Exception { get; }

        internal Failed(string message, Exception? exception = null)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException(nameof(message));

            Message   = message;
            Exception = exception;
        }

        internal Failed(Exception exception)
        {
            Message   = exception.Message;
            Exception = exception;
        }
    }

    #endregion
}