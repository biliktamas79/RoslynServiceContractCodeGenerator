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
    using System.ComponentModel.DataAnnotations;

    public class EntityInterfaceGenerator
    {
        public TextWriter Generate(TextWriter output, EntityContractDeclarationModel entityContractDeclaration, string targetNamespace)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (entityContractDeclaration == null)
                throw new ArgumentNullException(nameof(entityContractDeclaration));
            if (!entityContractDeclaration.DeclaringInterfaceType.IsInterface)
                throw new ArgumentException("The provided entity contract declaration is not an interface!", nameof(entityContractDeclaration));

            output.Write(
$@"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        /// Gets or sets the '{prop.Name}' primary key property value.
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
        /// Gets or sets the foreign key of the '{prop.Name}' entity reference that is part of the primary key.
        /// </summary>{GetAttributeDeclarations(prop)}
        {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
            }

            foreach (var prop in entityContractDeclaration.NonPkProperties)
            {
                output.WriteLine(
$@"
        /// <summary>
        /// Gets or sets the '{prop.Name}' simple property value.
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
        /// Gets or sets the '{prop.Name}' navigation property value.
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
        /// Gets or sets the '{prop.Name}' navigation property value.
        /// </summary>{GetAttributeDeclarations(prop)}
        [EntityReference({nameof(EntityReferenceMultiplicityEnum)}.{prop.EntityRefAttribute.Multiplicity})]
        {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
            }

            return output;
        }

        private static string GetAttributeDeclarations(PropertyDeclarationModel propertyDeclaration)
        {
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
            return $" : I{entityContractDeclaration.FriendlyName}, IHasPk<{GetPrimaryKeyTypeName(entityContractDeclaration)}>" +
                (((interfaces == null) || (interfaces.Length == 0))
                ? null
                : ", " + string.Join(", ", interfaces.Select(i => i.Name)));
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
