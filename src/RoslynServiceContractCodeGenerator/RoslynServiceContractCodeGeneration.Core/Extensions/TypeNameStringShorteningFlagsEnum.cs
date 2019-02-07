using System;

namespace RoslynServiceContractCodeGeneration.Extensions
{
    /// <summary>
    /// Enumeration of supported type name shortening flag values
    /// </summary>
    [Flags]
    public enum TypeNameStringShorteningFlags : int
    {
        /// <summary>
        /// None of the supported type name string shortenings will apply
        /// </summary>
        None = 0,
        /// <summary>
        /// 1 = namespaces will get excluded
        /// </summary>
        ExcludeNamespace = 1,
        /// <summary>
        /// 2 = type name aliases (like "int", "bool", "string", ...) will be used instead of full class names
        /// </summary>
        UseTypeNameAliases = 2,
        /// <summary>
        /// 4 = "?" will be used instead of <see cref="Nullable{T}"/>
        /// </summary>
        ShortNullableTypeNames = 4,
        /// <summary>
        /// 7 = All
        /// </summary>
        All = ExcludeNamespace | UseTypeNameAliases | ShortNullableTypeNames,
    }
}
