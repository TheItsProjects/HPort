using System.Reflection;

namespace ItsNameless.HPort.Bases;

/// <summary>
/// Base class providing methods for complex enums.
/// </summary>
public abstract class ComplexEnum
{
    /// <summary>
    /// Retrieves an array of the values of the constants in a specified complex enumeration type.
    /// </summary>
    /// <typeparam name="TComplexEnum">The type of the enumeration.</typeparam>
    /// <exception cref="MissingMemberException">Thrown when the accessed class is not a complex enum type.</exception>
    /// <returns>An array that contains the values of the constants in <typeparamref name="TComplexEnum" />.</returns>
    public static TComplexEnum[] GetValues<TComplexEnum>()
    {
        var values =
            typeof(TComplexEnum)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(
                    field =>
                        field.IsInitOnly &&
                        field.FieldType == typeof(TComplexEnum)
                )
                .Select(field => (TComplexEnum)field.GetValue(null)!)
                .ToArray();

        if (values.Length == 0)
        {
            throw new MissingMemberException(
                $"The class {typeof(TComplexEnum)} has no public static members that could be interpreted as complex enum values."
            );
        }

        return values;
    }
}
