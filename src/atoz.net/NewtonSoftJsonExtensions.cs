using Newtonsoft.Json.Linq;

namespace Atoz;
public static class NewtonSoftJsonExtensions
{
    /// <summary>
    /// Infers the <see cref="Type"/> which the <see cref="JToken"/>'s value is compatible with or convertible to.
    /// </summary>
    /// <param name="jtoken">The <see cref="JToken"/> object.</param>
    /// <returns>The <see cref="Type"/> inferred.</returns>
    /// <exception cref="ArgumentException"></exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0072:Add missing cases", Justification = "Default to throw for unrecognized types")]
    public static Type InferJTokenValueType(this JToken jtoken)
    {
        GuardedArgument.ThrowIfNull(jtoken);

        return jtoken.Type switch
        {
            JTokenType.Object => typeof(JObject),
            JTokenType.Array => typeof(JArray),
            JTokenType.Property => ((JProperty)jtoken).Value.InferJTokenValueType(),
            JTokenType.Boolean => typeof(bool),
            JTokenType.Integer => typeof(long),
            JTokenType.Float => typeof(double),
            JTokenType.String => typeof(string),
            JTokenType.Date => typeof(DateTime),
            JTokenType.TimeSpan => typeof(TimeSpan),
            JTokenType.Guid => typeof(Guid),
            JTokenType.Uri => typeof(Uri),
            _ => throw new ArgumentException($"Unsupported JTokenType: {jtoken.Type}", nameof(jtoken))
        };
    }

    /// <summary>
    /// Infers the <see cref="Type"/> of the elements in a <see cref="JArray"/>.
    /// </summary>
    /// <remarks>
    ///     This extension method assumes that all the elements in the <see cref="JArray"/>
    ///     are of the same JSON data type, and are not <c>null</c>.
    ///     <para />
    ///     It infers the <see cref="Type"/> of the elements from the <see cref="JArray"/>'s first element.
    /// </remarks>
    /// <param name="jarray">The <see cref="JArray"/> object.</param>
    /// <returns>The <see cref="Type"/> inferred.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Type InferJArrayElementType(this JArray jarray)
    {
        GuardedArgument.ThrowIfNull(jarray);
        GuardedArgument.RequireOrThrow<JArray, ArgumentException>(
            arr => arr.Count > 0 && arr[0].Type != JTokenType.Null,
            jarray,
            message: "The JSON array must contain at least one non-null element."
        );

        return jarray[0].InferJTokenValueType();
    }
}
