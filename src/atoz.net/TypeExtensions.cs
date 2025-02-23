using System.CodeDom;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CSharp;

namespace Atoz;

public static class TypeExtensions
{
    /// <summary>
    /// Gets the <see cref="Type"/>'s friendly name.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> object.</param>
    /// <returns>
    ///     A text representing the <see cref="Type"/>, using C# language type keywords for built-in types.
    /// </returns>
    [return: NotNull]
    public static string GetFriendlyTypeName(this Type type)
    {
        GuardedArgument.ThrowIfNull(type, nameof(type));

        using var compiler = new CSharpCodeProvider();
        var typeRef = new CodeTypeReference(type);
        return compiler.GetTypeOutput(typeRef);
    }

    /// <summary>
    /// Determines if a <see cref="Type"/> is a primitive type.
    /// </summary>
    /// <remarks>
    ///     These built-in types are primitive types:
    ///     <see href="https://learn.microsoft.com/en-us/dotnet/api/system.type.isprimitive?view=net-9.0#remarks"/>.
    ///     <para />
    ///     In addition, the following types are also considered to be primitive types:
    ///     <see langword="string"/>, <see langword="decimal"/>, <see cref="DateTime"/>, <see cref="DateTimeOffset"/>,
    ///     <see cref="TimeSpan"/>, and <see cref="Guid"/>.
    ///     <para />
    ///     If <paramref name="includeEnum"/> is <see langword="true"/>, <see langword="enum"/> types
    ///     are considered to be primitive.
    ///     <para />
    ///     If <paramref name="includeNullable"/> is <see langword="true"/>, Nullable types are considered
    ///     to be primitive. For example, <see langword="DateTime?"/> is considered primitive.
    /// </remarks>
    /// <param name="type">The <see cref="Type"/> object.</param>
    /// <param name="includeEnum">Whether to consider an enum type as a primitive type.</param>
    /// <param name="includeNullable">Whether to consider a Nullable type as a primitive type.</param>
    /// <returns><see langword="true"/> if the type is primitive; <see langword="false"/> otherwise.</returns>
    public static bool IsPrimitiveIncludingExtendedAndNullables(
        this Type type,
        bool includeEnum = false,
        bool includeNullable = true)
        => type.IsPrimitiveInternal(includeEnum) ||
           (
            includeNullable &&
            type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
            type.GenericTypeArguments[0].IsPrimitiveInternal(includeEnum)
           );

    private static bool IsPrimitiveInternal(this Type type, bool includeEnum = false)
        // https://learn.microsoft.com/en-us/dotnet/api/system.type.isprimitive?view=net-9.0#remarks
        => type.IsPrimitive ||
            (includeEnum && type.IsEnum) ||
            type switch
            {
                // also consider the following types as primitive types.
                { } t when t == typeof(string) ||
                           t == typeof(decimal) ||
                           t == typeof(DateTime) ||
                           t == typeof(DateTimeOffset) ||
                           t == typeof(TimeSpan) ||
                           t == typeof(Guid) ||
                           t == typeof(Uri) => true,
                _ => false
            };

    /// <summary>
    ///     Converts the input value to an object of type <typeparamref name="TOutput"/>
    ///     and whose value is equivalent to the input value.
    /// </summary>
    /// <remarks>
    ///     If the <typeparamref name="TOutput"/> is a collection type, the input value must be a
    ///     string representing a valid JSON array.
    /// </remarks>
    /// <typeparam name="TOutput">The type of object to return.</typeparam>
    /// <param name="value">The input value represented as a string.</param>
    /// <returns>
    ///     The object of <typeparamref name="TOutput"/> and whose value is quivalent to the input value.
    /// </returns>
    public static TOutput? ConvertTo<TOutput>(this string value)
    {
        GuardedArgument.ThrowIfNullOrWhiteSpace(value);

        return (TOutput?)value.ConvertTo(typeof(TOutput));
    }

    /// <summary>
    ///     Converts the input value to an object of the <paramref name="outputType"/>
    ///     and whose value is equivalent to the input value.
    /// </summary>
    /// <remarks>
    ///     If the <paramref name="outputType"/> is a collection type, the input value must be a
    ///     string representing a valid JSON array.
    /// </remarks>
    /// <param name="value">The input value represented as a string.</param>
    /// <param name="outputType">The type of object to return.</param>
    /// <returns>
    ///     The object of <paramref name="outputType"/> and whose value is quivalent to the input value.
    /// </returns>
    public static object? ConvertTo(this string value, Type outputType)
    {
        GuardedArgument.ThrowIfNullOrWhiteSpace(value);
        GuardedArgument.ThrowIfNull(outputType);

        // the input value must be a valid json array
        if (outputType.IsArray)
        {
            return (object?)ConvertToArray(value, outputType.GetElementType()!);
        }

        // we expect the outputType implements IList, or IList<> if it is a generic type.
        if (outputType.IsAssignableTo(typeof(ICollection)))
        {
            if (IsCompatibleListType(outputType, out Type? elementType))
            {
                return ConvertToList(value, elementType);
            }

            throw new InvalidCastException(
                $"Unsupported collection type '{outputType.Name}'. Use a type compatible with `IList` or `IList<>`");
        }

        return ConvertValue(value, outputType);
    }

    private static bool IsCompatibleListType(Type type, [NotNullWhen(true)] out Type? elementType)
    {
        bool result = false;
        elementType = null;

        if (type.IsAssignableTo(typeof(IList)))
        {
            result = true;
            elementType = typeof(object);
        }

        if (type.IsGenericType)
        {
            elementType = type.GenericTypeArguments[0];
            if (type.IsAssignableTo(typeof(IList<>).MakeGenericType(elementType)))
            {
                result = true;
            }
            else
            {
                result = false;
                elementType = null;
            }
        }

        return result;
    }

    private static Array ConvertToArray(string value, Type elementType)
    {
        string[] elements = ParseJsonArray(value);
        var array = Array.CreateInstance(elementType, elements.Length);

        for (int i = 0; i < elements.Length; i++)
        {
            array.SetValue(ConvertValue(elements[i], elementType), i);
        }

        return array;
    }

    private static object ConvertToList(string value, Type elementType)
    {
        string[] elements = ParseJsonArray(value);
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

        foreach (string element in elements)
        {
            list.Add(ConvertValue(element, elementType));
        }

        return list;
    }

    private static string[] ParseJsonArray(string value)
        => value.Trim('[', ']')
                .Split(',', StringSplitOptions.TrimEntries);

    private static object? ConvertValue(string value, Type targetType)
    {
        // if not one of the "primitive" types
        if (!targetType.IsPrimitiveIncludingExtendedAndNullables(includeEnum: true, includeNullable: true))
        {
            throw new InvalidCastException($"Conversion target type '{targetType.Name}' is not supported.");
        }

        // input value represents a null value
        if (value.Equals("null", StringComparison.Ordinal))
        {
            return !targetType.IsValueType || IsNullableType(targetType)
                ? null
                : throw new InvalidCastException(
                    $"Requested value '{value}' represents null but target type '{targetType.Name}' is non-nullable.");
        }

        // beyond this point, converted object cannot be null
        targetType = IsNullableType(targetType) ? targetType.GenericTypeArguments[0] : targetType;
        value = value.Trim('"', '\'');

        return targetType switch
        {
            Type t when t.IsEnum => ParseEnum(targetType, value),
            Type t when t == typeof(DateTime) => DateTime.Parse(value, CultureInfo.InvariantCulture),
            Type t when t == typeof(DateTimeOffset) => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture),
            Type t when t == typeof(TimeSpan) => TimeSpan.Parse(value, CultureInfo.InvariantCulture),
            Type t when t == typeof(Guid) => Guid.Parse(value, CultureInfo.InvariantCulture),
            Type t when t == typeof(Uri) => new Uri(value),
            _ => Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture),
        };
    }

    private static object ParseEnum(Type enumType, string value)
    {
        if (Enum.IsDefined(enumType, value))
        {
            return Enum.Parse(enumType, value);
        }

        // invalid enum name (throws), or numeric value
        Type underlyingType = Enum.GetUnderlyingType(enumType);
        object convertedValue = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);

        if (Enum.IsDefined(enumType, convertedValue))
        {
            return Enum.ToObject(enumType, convertedValue);
        }

        throw new InvalidCastException(
            $"Requested value '{value}' is not a valid '{enumType.Name}' enum value.");
    }

    private static bool IsNullableType(Type type)
    {
        return
            type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}
