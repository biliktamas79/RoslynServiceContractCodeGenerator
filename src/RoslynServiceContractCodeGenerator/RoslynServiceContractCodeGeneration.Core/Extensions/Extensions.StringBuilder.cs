using System;
using System.Text;

namespace RoslynServiceContractCodeGeneration.Extensions
{
    public static partial class Extensions
    {
        public static StringBuilder AppendFriendlyTypeName(this StringBuilder sb, Type type, TypeNameStringShorteningFlags typeNameShorteningFlags = TypeNameStringShorteningFlags.All)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            string typeName = typeNameShorteningFlags.HasFlag(TypeNameStringShorteningFlags.ExcludeNamespace) ? type.Name : type.FullName;

            if (type.IsConstructedGenericType)
            {
                Type[] genArgTypes = type.GenericTypeArguments;
                if ((genArgTypes != null) && (genArgTypes.Length > 0))
                {
                    if (typeNameShorteningFlags.HasFlag(TypeNameStringShorteningFlags.ShortNullableTypeNames) && type.FullName.StartsWith("System.Nullable`1"))
                        //sb.Append(genArgTypes[0].Name).Append("?");
                        AppendFriendlyTypeName(sb, genArgTypes[0], typeNameShorteningFlags).Append("?");
                    else
                    {
                        bool selfContaining = false;
                        int firstIndex = typeName.IndexOf('`');

                        sb.Append((firstIndex >= 0) ? typeName.Substring(0, firstIndex) : typeName);
                        sb.Append("<");
                        bool appendSeparator = false;
                        foreach (Type genArgType in genArgTypes)
                        {
                            if (appendSeparator)
                                sb.Append(", ");
                            if (genArgType.Equals(type))
                            {
                                selfContaining = true;
                                sb.Append("{0}");
                            }
                            else
                            {
                                AppendFriendlyTypeName(sb, genArgType, typeNameShorteningFlags);
                            }
                            appendSeparator = true;
                        }
                        sb.Append(">");
                        if (selfContaining)
                            throw new NotSupportedException("Generic classes containing themself as a generic argument are not supported!");
                    }
                }
            }
            else
            {
                if (typeNameShorteningFlags.HasFlag(TypeNameStringShorteningFlags.UseTypeNameAliases))
                {
                    if (TryGetNameAlias(type, out var alias))
                        sb.Append(alias);
                    else
                        sb.Append(typeName);
                }
                else
                    sb.Append(typeName);
            }
            return sb;
        }
    }
}
