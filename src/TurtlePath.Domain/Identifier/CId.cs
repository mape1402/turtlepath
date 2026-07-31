namespace TurtlePath.Domain.Identifier
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// Represents a strongly-typed identifier with a value of a specific allowed type.
    /// </summary>
    [TypeConverter(typeof(CIdTypeConverter))]
    public readonly struct CId : IEquatable<CId>
    {
        private readonly object _value;

        /// <summary>
        /// Initializes a new <see cref="CId"/> with the specified value.
        /// </summary>
        /// <param name="value">The value of the identifier. Must be of the allowed type.</param>
        /// <exception cref="ArgumentException">Thrown if the value is not of the allowed type.</exception>
        private CId(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _value = value;
        }

        /// <summary>
        /// Gets the value of the identifier.
        /// </summary>
        public object Value => _value;

        /// <summary>
        /// Gets a value indicating whether the identifier is empty.
        /// </summary>
        public bool IsEmpty => _value == null;

        /// <summary>
        /// Gets an instance of <see cref="CId"/> that represents an empty identifier.
        /// </summary>
        /// <remarks>This property returns a new instance of <see cref="CId"/> with default values,  which
        /// can be used to represent a non-initialized or empty state.</remarks>
        public static CId Empty => default;

        /// <summary>
        /// Converts the current value to its byte array representation.
        /// </summary>
        /// <remarks>Ensure that the object is in a ready state before calling this method to avoid a null
        /// return value.</remarks>
        /// <returns>A byte array that represents the current value, or null if the object is not ready or the value is null.</returns>
        public byte[] ToByteArray()
        {
            if (IsEmpty || Value is null)
                return null;

            return Value switch
            {
                Guid value => value.ToByteArray(),
                int value => BitConverter.GetBytes(value),
                long value => BitConverter.GetBytes(value),
                string value => Encoding.UTF8.GetBytes(value),
                _ => Encoding.UTF8.GetBytes(Value.ToString())
            };
        }

        /// <summary>
        /// Parses the specified string to create a new <see cref="CId"/> instance.
        /// </summary>
        /// <param name="value">The string representation of the CId to parse. Cannot be null or empty.</param>
        /// <returns>A <see cref="CId"/> instance that corresponds to the specified string.</returns>
        public static CId Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Empty;

            if (Guid.TryParse(value, out var guid))
                return From(guid);

            if (int.TryParse(value, out var integer))
                return From(integer);

            if (long.TryParse(value, out var longValue))
                return From(longValue);

            return From(value);
        }

        /// <summary>
        /// Tries to convert the specified string representation of a CId to its equivalent CId object.
        /// </summary>
        /// <remarks>This method handles exceptions internally and sets the result to Empty if the
        /// conversion fails.</remarks>
        /// <param name="value">The string representation of the CId to convert.</param>
        /// <param name="result">When this method returns, contains the CId equivalent of the string representation, or Empty if the
        /// conversion failed.</param>
        /// <returns>true if the conversion succeeded; otherwise, false.</returns>
        public static bool TryParse(string value, out CId result)
        {
            try
            {
                result = Parse(value);
                return true;
            }
            catch
            {
                result = Empty;
                return false;
            }
        }

        /// <summary>
        /// Casts the value of the identifier to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <returns>The value cast to type <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidCastException">Thrown if the value cannot be cast to the specified type or the identifier is empty.</exception>
        public T Cast<T>()
        {
            if (IsEmpty || Value is not T castValue)
                throw new InvalidCastException($"Value cannot be converted to {typeof(T)}.");

            return castValue;
        }

        /// <summary>
        /// Returns the string representation of the identifier value.
        /// </summary>
        /// <returns>The string representation of the value.</returns>
        public override string ToString()
        {
            if (IsEmpty)
                return string.Empty;

            return Value?.ToString();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current identifier.
        /// </summary>
        /// <param name="obj">The object to compare with the current identifier.</param>
        /// <returns><c>true</c> if the specified object is equal to the current identifier; otherwise, <c>false</c>.</returns>
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is not CId other)
                return false;

            if (other.IsEmpty && IsEmpty)
                return true;

            return Equals(Value, other.Value);
        }

        /// <summary>
        /// Determines whether the specified <see cref="CId"/> is equal to the current identifier.
        /// </summary>
        /// <param name="other">The identifier to compare with the current identifier.</param>
        /// <returns><c>true</c> if the specified identifier is equal to the current identifier; otherwise, <c>false</c>.</returns>
        public bool Equals(CId other)
            => Equals((object)other);

        /// <summary>
        /// Returns a hash code for the identifier.
        /// </summary>
        /// <returns>A hash code for the identifier.</returns>
        public override int GetHashCode()
        {
            if (IsEmpty)
                return default;

            return Value.GetHashCode();
        }

        /// <summary>
        /// Creates an identifier from a single value.
        /// </summary>
        /// <param name="value">The underlying value.</param>
        /// <returns>The identifier.</returns>
        public static CId From(object value)
            => new(value);

        /// <summary>
        /// Determines whether two <see cref="CId"/> instances are equal.
        /// </summary>
        /// <param name="left">The first identifier to compare.</param>
        /// <param name="right">The second identifier to compare.</param>
        /// <returns><c>true</c> if the identifiers are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(CId left, CId right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="CId"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first identifier to compare.</param>
        /// <param name="right">The second identifier to compare.</param>
        /// <returns><c>true</c> if the identifiers are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(CId left, CId right)
            => !left.Equals(right);
    }
}

