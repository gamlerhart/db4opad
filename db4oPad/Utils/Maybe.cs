using System;

namespace Gamlor.Db4oPad.Utils
{
    public delegate bool OutFunction<T>(out T outArgument);
    /// <summary>
    /// Helper routines for <see cref="Maybe{T}"/>
    /// </summary>
    public static class Maybe
    {

        /// <summary>
        /// Creates new <see cref="Maybe{T}"/> from the provided value
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns><see cref="Maybe{T}"/> that matches the provided value</returns>
        /// <exception cref="ArgumentNullException">if argument is a null reference</exception>
        public static Maybe<TSource> From<TSource>(TSource item)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (null == item) throw new ArgumentNullException("item");
            // ReSharper restore CompareNonConstrainedGenericWithNull

            return new Maybe<TSource>(item);
        }


        public static Maybe<TSource> OutToMaybe<TSource>(OutFunction<TSource> methodWithOurParameter)
        {
            TSource outParam;
            var success = methodWithOurParameter(out outParam);
            return success ? From(outParam) : Maybe<TSource>.Empty;
        }
    }

    /// <summary>
    /// Helper class that indicates nullable value in a good-citizenship code
    /// </summary>
    /// <typeparam name="T">underlying type</typeparam>
    [Serializable]
    public struct Maybe<T> : IEquatable<Maybe<T>>
    {
        readonly T _value;
        private readonly bool _hasValue;

        /// <summary>
        /// Default empty instance.
        /// </summary>
        public static readonly Maybe<T> Empty = new Maybe<T>(default(T), false);

        Maybe(T item, bool hasValue)
        {
            _value = item;
            _hasValue = hasValue;
        }

        internal Maybe(T value)
            : this(value, true)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (value == null) throw new ArgumentNullException("value");
            // ReSharper restore CompareNonConstrainedGenericWithNull
        }

        /// <summary>
        /// Gets the underlying value.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get
            {
                if (!HasValue)
                    throw new InvalidOperationException("Cannot access value if no value is set");

                return _value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has value.
        /// </summary>
        /// <value><c>true</c> if this instance has value; otherwise, <c>false</c>.</value>
        public bool HasValue
        {
            get { return _hasValue; }
        }

        /// <summary>
        /// Converts this instance to <see cref="Maybe{T}"/>, 
        /// while applying <paramref name="converter"/> if there is a value.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="converter">The converter.</param>
        /// <returns></returns>
        public Maybe<TTarget> Convert<TTarget>(Func<T, TTarget> converter)
        {
            return HasValue
                ? converter(_value)
                : Maybe<TTarget>.Empty;
        }

        /// <summary>
        /// Applies the specified action to the value, if it is present.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>same instance for inlining</returns>
        public Maybe<T> Apply(Action<T> action)
        {
            if (HasValue)
            {
                action(_value);
            }
            return this;
        }

        /// <summary>
        /// Executes the specified action, if the value is absent
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>same instance for inlining</returns>
        public Maybe<T> Handle(Action action)
        {
            if (!HasValue)
            {
                action();
            }
            return this;
        }

        /// <summary>
        /// Retrieves value from this instance, using a 
        /// <paramref name="defaultValue"/> if it is absent.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>value</returns>
        public T GetValue(Func<T> defaultValue)
        {
            return HasValue ? Value : defaultValue();
        }

        /// <summary>
        /// Retrieves value from this instance, using a 
        /// <paramref name="defaultValue"/> if it is absent.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>value</returns>
        public T GetValue(T defaultValue)
        {
            return HasValue ? Value : defaultValue;
        }

        /// <summary>
        /// Retrieves converted value, using a 
        /// <paramref name="defaultValue"/> if it is absent.
        /// </summary>
        /// <typeparam name="TTarget">type of the conversion target</typeparam>
        /// <param name="converter">The converter.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>value</returns>
        public TTarget Convert<TTarget>(Func<T, TTarget> converter, Func<TTarget> defaultValue)
        {
            return HasValue ? converter(Value) : defaultValue();
        }

        /// <summary>
        /// Retrieves converted value, using a 
        /// <paramref name="defaultValue"/> if it is absent.
        /// </summary>
        /// <typeparam name="TTarget">type of the conversion target</typeparam>
        /// <param name="converter">The converter.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>value</returns>
        public TTarget Convert<TTarget>(Func<T, TTarget> converter, TTarget defaultValue)
        {
            return HasValue ? converter(Value) : defaultValue;
        }

        /// <summary>
        /// Combines this optional with the pipeline function
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="combinator">The combinator (pipeline funcion).</param>
        /// <returns>optional result</returns>
        public Maybe<TTarget> Combine<TTarget>(Func<T, Maybe<TTarget>> combinator)
        {
            if (!HasValue) return Maybe<TTarget>.Empty;
            return combinator(Value);
        }


        /// <summary>
        /// Determines whether the specified <see cref="Maybe{T}"/> is equal to the current <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="maybe">The <see cref="Maybe"/> to compare with.</param>
        /// <returns>true if the objects are equal</returns>
        public bool Equals(Maybe<T> maybe)
        {
            if (HasValue != maybe.HasValue) return false;
            if (!HasValue) return true;
            return _value.Equals(maybe._value);
        }

        /// <summary>
        /// If this value is empty, it will get the value of the given closure. 
        /// </summary>
        /// <param name="alternative"></param>
        /// <returns></returns>
        public Maybe<T> Otherwise(Func<Maybe<T>> alternative)
        {
            if(HasValue)
            {
                return this;
            }
            return alternative();   
        } 

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;

            if (obj is Maybe<T>) return Equals((Maybe<T>)obj);
            if (obj is T) return EqualsWithOriginal((T)obj);
            return false;
        }

        private bool EqualsWithOriginal(T obj)
        {
            return _value.Equals(obj);
        }

        /// <summary>
        /// Serves as a hash function for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Maybe{T}"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable CompareNonConstrainedGenericWithNull
                return ((_value != null ? _value.GetHashCode() : 0) * 397) ^ HasValue.GetHashCode();
                // ReSharper restore CompareNonConstrainedGenericWithNull
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Maybe<T> left, Maybe<T> right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Maybe<T> left, Maybe<T> right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Performs an implicit conversion from <typeparamref name="T"/> to <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Maybe<T>(T item)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (item == null) throw new ArgumentNullException("item");
            // ReSharper restore CompareNonConstrainedGenericWithNull

            return new Maybe<T>(item);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Maybe{T}"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator T(Maybe<T> item)
        {
            if (!item.HasValue) throw new ArgumentException("May be must have value");

            return item.Value;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (HasValue)
            {
                return "<" + _value + ">";
            }

            return "<Empty>";
        }
    }
}