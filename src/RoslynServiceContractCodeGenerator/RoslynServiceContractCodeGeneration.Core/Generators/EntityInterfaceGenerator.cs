using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RoslynServiceContractCodeGeneration.Generators
{
    using Enums;
    using Extensions;

    public class EntityInterfaceGenerator
    {
        public TextWriter Generate(TextWriter output, EntityContractGeneratorModel entityContractDeclaration, string targetNamespace)
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
    public interface {entityContractDeclaration.FriendlyName}{GetBaseClassAndImplementedInterfaceListString(entityContractDeclaration)}
    {{");

            WritePropertyDeclarations(output, entityContractDeclaration);

            output.Write(
$@"
    }}
}}");
        }

        private TextWriter WritePropertyDeclarations(TextWriter output, EntityContractGeneratorModel entityContractDeclaration)
        {
            foreach (var prop in entityContractDeclaration.PkProperties)
            {
                output.Write(
$@"
        /// <summary>
        /// Gets or sets the '{prop.Name}' primary key property value.
        /// </summary>
        [Key]{GetAttributeDeclarations(prop)}
        {prop.TypeFriendlyName} {prop.Name} {{ get; set; }}");
            }

            foreach (var prop in entityContractDeclaration.PkEntityReferences
                .Where(pker => pker.EntityRefAttribute.Multiplicity != EntityReferenceMultiplicityEnum.Many))
            {
                prop.EntityRefAttribute.Multiplicity
                output.Write( itt folytatni
$@"
        /// <summary>
        /// Gets or sets the primary key of '{prop.Name}' entity reference as part of the primary key of this instance.
        /// </summary>
        [Key]{GetAttributeDeclarations(prop)}
        {prop.TypeFriendlyName} {prop.Name} {{ get; set; }}");
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

        private static string GetBaseClassAndImplementedInterfaceListString(EntityContractGeneratorModel entityContractDeclaration)
        {
            var interfaces = entityContractDeclaration.DeclaringInterfaceType.GetInterfaces();
            return ((interfaces == null) || (interfaces.Length == 0))
                ? null
                : " : " + string.Join(", ", interfaces.Select(i => i.Name));
        }
    }
}
