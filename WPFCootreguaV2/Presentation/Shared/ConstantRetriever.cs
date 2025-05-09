using System;
using System.Reflection;

namespace ArisWPF.Presentation.Shared;

/// <summary>
/// Class to get a constant value from a static class by the name of the constant
/// </summary>
public class ConstantRetriever
{
    public static object? Get(Type staticClassType, string constantName)
    {
        // Get the field info for the constant with the specified name
        FieldInfo? fieldInfo = staticClassType.GetField(constantName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (fieldInfo == null || !fieldInfo.IsLiteral)
        {
            throw new ArgumentException($"Constant '{constantName}' not found in {staticClassType.FullName}");
        }

        // Retrieve the value of the constant
        return fieldInfo.GetValue(null);
    }
}
