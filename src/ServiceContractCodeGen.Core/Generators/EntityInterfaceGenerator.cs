using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ServiceContractCodeGen.Generators
{
    using Enums;
    using Extensions;
    using MyCompany.Enums;
    using System.ComponentModel.DataAnnotations;

    public class EntityInterfaceGenerator
    {
        private readonly static HashSet<string> defaultNamespacesUsed = new HashSet<string>()
        {
            "System",
            "System.Collections.Generic",
            "System.ComponentModel.DataAnnotations",
            "MyCompany",
            "MyCompany.Attributes",
            "MyCompany.Enums"
        };

        public TextWriter Generate(TextWriter output, EntityContractDeclarationModel entityContractDeclaration, string targetNamespace)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (entityContractDeclaration == null)
                throw new ArgumentNullException(nameof(entityContractDeclaration));
            if (!entityContractDeclaration.DeclaringInterfaceType.IsInterface)
                throw new ArgumentException("The provided entity contract declaration is not an interface!", nameof(entityContractDeclaration));

            output.Write(
$@"{GetAdditionalNamespaceUsings(entityContractDeclaration, targetNamespace)}

namespace {targetNamespace}
{{
    public interface I{entityContractDeclaration.FriendlyName}{GetBaseClassAndImplementedInterfaceListString(entityContractDeclaration)}
    {{");

            WritePropertyDeclarations(output, entityContractDeclaration);

            output.Write(
$@"
    }}
}}");
            return output;
        }

        private TextWriter WritePropertyDeclarations(TextWriter output, EntityContractDeclarationModel entityContractDeclaration)
        {
            foreach (var prop in entityContractDeclaration.PkProperties)
            {
                output.WriteLine(
$@"
        /// <summary>
        /// {GetPropertyGetSetXmlCommentPrefix(prop)} the '{prop.Name}' primary key property value.
        /// </summary>
        [Key]{GetAttributeDeclarations(prop)}
        {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
            }

            foreach (var prop in entityContractDeclaration.PkEntityReferences
                .Where(pker => pker.EntityRefAttribute.Multiplicity != EntityReferenceMultiplicityEnum.Many))
            {
                //prop.EntityRefAttribute.Multiplicity // TODO handle multiplicity
                output.WriteLine(
$@"
        /// <summary>
        /// {GetPropertyGetSetXmlCommentPrefix(prop)} the foreign key of the '{prop.Name}' entity reference that is part of the primary key.
        /// </summary>{GetAttributeDeclarations(prop)}
        {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
            }

            foreach (var prop in entityContractDeclaration.NonPkProperties)
            {
                output.WriteLine(
$@"
        /// <summary>
        /// {GetPropertyGetSetXmlCommentPrefix(prop)} the '{prop.Name}' simple property value.
        /// </summary>{GetAttributeDeclarations(prop)}
        {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
            }

            // XtoOne or XToOneOrZero navigation properties
            foreach (var prop in entityContractDeclaration.EntityReferences
                .Where(pker => pker.EntityRefAttribute.Multiplicity != EntityReferenceMultiplicityEnum.Many))
            {
                //prop.EntityRefAttribute.Multiplicity // TODO handle multiplicity
                output.WriteLine(
$@"
        /// <summary>
        /// {GetPropertyGetSetXmlCommentPrefix(prop)} the '{prop.Name}' navigation property value.
        /// </summary>{GetAttributeDeclarations(prop)}
        [EntityReference({nameof(EntityReferenceMultiplicityEnum)}.{prop.EntityRefAttribute.Multiplicity})]
        {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
            }

            // XtoMany navigation properties
            foreach (var prop in entityContractDeclaration.EntityReferences
                .Where(pker => pker.EntityRefAttribute.Multiplicity == EntityReferenceMultiplicityEnum.Many))
            {
                output.WriteLine(
$@"
        /// <summary>
        /// {GetPropertyGetSetXmlCommentPrefix(prop)} the '{prop.Name}' navigation property value.
        /// </summary>{GetAttributeDeclarations(prop)}
        [EntityReference({nameof(EntityReferenceMultiplicityEnum)}.{prop.EntityRefAttribute.Multiplicity})]
        {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
            }

            return output;
        }

        private static string GetAttributeDeclarations(PropertyDeclarationModel propertyDeclaration)
        {
            if (!propertyDeclaration.Attributes.Any())
                return null;

            var sb = new StringBuilder();
            foreach (var attr in propertyDeclaration.Attributes)
            {
                string attributeName = attr.AttributeType.Name.EndsWith("Attribute")
                    ? attr.AttributeType.Name.Substring(0, attr.AttributeType.Name.Length - 9)
                    : attr.AttributeType.Name;

                sb.Append($@"
        [{attributeName}");

                // if the attribute has any constructor or named parameters
                if ((attr.ConstructorArguments?.Count ?? 0) + (attr.NamedArguments?.Count ?? 0) > 0)
                {
                    sb.Append("(");

                    bool appendSeparator = false;
                    if (attr.ConstructorArguments != null)
                    {
                        foreach (var constrParam in attr.ConstructorArguments)
                        {
                            if (appendSeparator)
                                sb.Append(", ");

                            AppendConstructorParam(sb, constrParam);

                            if (!appendSeparator)
                                appendSeparator = true;
                        }
                    }

                    if (attr.NamedArguments != null)
                    {
                        foreach (var namedParam in attr.NamedArguments)
                        {
                            if (appendSeparator)
                                sb.Append(", ");

                            AppendConstructorNamedParam(sb, namedParam);

                            if (!appendSeparator)
                                appendSeparator = true;
                        }
                    }

                    sb.Append(")");
                }
                sb.Append("]");
            }

            return sb.ToString();
        }

        private static StringBuilder AppendConstructorParam(StringBuilder sb, CustomAttributeTypedArgument constrParam)
        {
            if (constrParam.ArgumentType == typeof(string))
                sb.Append("\"");
            else if (constrParam.ArgumentType == typeof(char))
                sb.Append("'");
            else if (constrParam.ArgumentType == typeof(Type))
                sb.Append("typeof(");

            sb.Append(constrParam.Value);

            if (constrParam.ArgumentType == typeof(string))
                sb.Append("\"");
            else if (constrParam.ArgumentType == typeof(char))
                sb.Append("'");
            else if (constrParam.ArgumentType == typeof(Type))
                sb.Append(")");

            return sb;
        }

        private static StringBuilder AppendConstructorNamedParam(StringBuilder sb, CustomAttributeNamedArgument namedParam)
        {
            sb.Append(namedParam.MemberName).Append(" = ");

            if (namedParam.TypedValue.ArgumentType == typeof(string))
                sb.Append("\"");
            else if (namedParam.TypedValue.ArgumentType == typeof(char))
                sb.Append("'");
            else if (namedParam.TypedValue.ArgumentType == typeof(Type))
                sb.Append("typeof(");

            sb.Append(namedParam.TypedValue.Value);

            if (namedParam.TypedValue.ArgumentType == typeof(string))
                sb.Append("\"");
            else if (namedParam.TypedValue.ArgumentType == typeof(char))
                sb.Append("'");
            else if (namedParam.TypedValue.ArgumentType == typeof(Type))
                sb.Append(")");

            return sb;
        }

        private static string GetBaseClassAndImplementedInterfaceListString(EntityContractDeclarationModel entityContractDeclaration)
        {
            var interfaces = entityContractDeclaration.DeclaringInterfaceType.GetInterfaces();

            bool appendSeparator = false;
            StringBuilder sb = new StringBuilder();
            
            if (entityContractDeclaration.HasPk)
            {
                sb.Append(" : IHasPk<").Append(GetPrimaryKeyTypeName(entityContractDeclaration)).Append(">");
                appendSeparator = true;
            }

            if ((interfaces != null) && (interfaces.Length > 0))
            {
                if (!appendSeparator)
                {
                    sb.Append(" : ");
                }

                foreach (var i in interfaces)
                {
                    if (appendSeparator)
                        sb.Append(", ");
                    else
                        appendSeparator = true;

                    sb.AppendFriendlyTypeName(i);
                }
            }
            return sb.ToString();
        }

        private static string GetAdditionalNamespaceUsings(EntityContractDeclarationModel entityContractDeclaration, string targetNamespace)
        {
            var namespaceUsings = new HashSet<string>(defaultNamespacesUsed, StringComparer.InvariantCulture);

            foreach (var i in entityContractDeclaration.DeclaringInterfaceType.GetInterfaces())
            {
                if (!namespaceUsings.Contains(i.Namespace))
                    namespaceUsings.Add(i.Namespace);
            }

            foreach (var prop in entityContractDeclaration.GetProperties())
            {
                var entityNamespace = entityContractDeclaration.EntityContractDeclarationAttribute?.Namespace ?? prop.DeclaringProperty.PropertyType.Namespace;
                if ((entityNamespace != targetNamespace) && !namespaceUsings.Contains(entityNamespace))
                    namespaceUsings.Add(entityNamespace);
            }

            foreach (var a in entityContractDeclaration.GetCustomAttributes())
            {
                if (!namespaceUsings.Contains(a.AttributeType.Namespace))
                    namespaceUsings.Add(a.AttributeType.Namespace);

                if ((a.ConstructorArguments != null) && (a.ConstructorArguments.Count > 0))
                {
                    foreach (var ca in a.ConstructorArguments)
                    {
                        if (!namespaceUsings.Contains(ca.ArgumentType.Namespace))
                            namespaceUsings.Add(ca.ArgumentType.Namespace);
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (var ns in namespaceUsings.OrderBy(s => s, StringComparer.InvariantCulture))
            {
                sb.Append(@"
using ").Append(ns).Append(";");
            }

            return sb.ToString();
        }

        private static string GetPropertyGetSetXmlCommentPrefix(PropertyDeclarationModel propertyDeclaration)
        {
            if (propertyDeclaration.CanGet && propertyDeclaration.CanSet)
                return "Gets or sets";
            else if (propertyDeclaration.CanGet)
                return "Gets";
            else if (propertyDeclaration.CanSet)
                return "Sets";
            else
                throw new NotSupportedException("Properties without get or set are not supported!");
        }

        private static string GetPropertyGetSetDeclaration(PropertyDeclarationModel propertyDeclaration)
        {
            if (propertyDeclaration.CanGet && propertyDeclaration.CanSet)
                return "get; set;";
            else if (propertyDeclaration.CanGet)
                return "get;";
            else if (propertyDeclaration.CanSet)
                return "set;";
            else
                throw new NotSupportedException("Properties without get or set are not supported!");
        }

        private static string GetPrimaryKeyTypeName(EntityContractDeclarationModel entityContractDeclaration)
        {
            if (entityContractDeclaration == null)
                throw new ArgumentNullException(nameof(entityContractDeclaration));
            if (!entityContractDeclaration.HasPk)
                throw new ArgumentException("Entity does not have a primary key.", nameof(entityContractDeclaration));

            if (entityContractDeclaration.HasCompositePk)
                return entityContractDeclaration.FriendlyName + "Pk";
            else if (entityContractDeclaration.PkProperties.Length == 1)
                return entityContractDeclaration.PkProperties[0].TypeFriendlyName;
            else if (entityContractDeclaration.PkEntityReferences.Length == 1)
                throw new NotSupportedException("Primary key entity references are not supported!");
            else
                throw new NotSupportedException();
        }

        //private static string GetNavigationPropertyMultiplicityEnumValueString(PropertyDeclarationModel propertyDeclaration)
        //{
        //    if (propertyDeclaration.DeclaringProperty.PropertyType.IsAssignableFrom(typeof(System.Collections.Generic.ICollection<>))
        //        || propertyDeclaration.DeclaringProperty.PropertyType.IsAssignableFrom(typeof(System.Collections.ICollection))
        //        || propertyDeclaration.DeclaringProperty.PropertyType.IsAssignableFrom(typeof(System.Collections.Generic.IList<>))
        //        || propertyDeclaration.DeclaringProperty.PropertyType.IsAssignableFrom(typeof(System.Collections.IList))
        //        || propertyDeclaration.DeclaringProperty.PropertyType.IsAssignableFrom(typeof(System.Array)))
        //    {

        //    }
        //}
    }
}
