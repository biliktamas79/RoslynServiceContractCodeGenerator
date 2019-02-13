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

    public class EntityClassGenerator
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
using System.ComponentModel.DataAnnotations.Schema;
using ServiceContractCodeGen.Attributes;

namespace {targetNamespace}
{{
    public class {entityContractDeclaration.FriendlyName}{GetBaseClassAndImplementedInterfaceListString(entityContractDeclaration)}
    {{");
            WriteStaticRegion(output, entityContractDeclaration);

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
        public {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
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
        public virtual {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
            }

            foreach (var prop in entityContractDeclaration.NonPkProperties)
            {
                output.WriteLine(
$@"
        /// <summary>
        /// Gets or sets the '{prop.Name}' simple property value.
        /// </summary>{GetAttributeDeclarations(prop)}
        public {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
            }

            foreach (var prop in entityContractDeclaration.EntityReferences
                .Where(pker => pker.EntityRefAttribute.Multiplicity != EntityReferenceMultiplicityEnum.Many))
            {
                //prop.EntityRefAttribute.Multiplicity // TODO handle multiplicity
                output.WriteLine(
$@"
        /// <summary>
        /// Gets or sets the '{prop.Name}' navigation property value.
        /// </summary>{GetAttributeDeclarations(prop)}
        public virtual {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
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
            var sb = new StringBuilder();
            sb.Append(" : I").Append(entityContractDeclaration.FriendlyName);
            if (entityContractDeclaration.HasPk)
            {
                sb.Append(", IHasPk<").Append(GetPrimaryKeyTypeName(entityContractDeclaration)).Append(">");
            }

            var interfaces = entityContractDeclaration.DeclaringInterfaceType.GetInterfaces();
            if ((interfaces != null) && (interfaces.Length > 0))
            {
            //    sb.Append(@"
            //");

                foreach (var intrfce in entityContractDeclaration.DeclaringInterfaceType.GetInterfaces())
                {
                    sb.Append(", ").AppendFriendlyTypeName(intrfce);
                }
            }

            return sb.ToString();
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

        private TextWriter WriteStaticRegion(TextWriter output, EntityContractDeclarationModel entityContractDeclaration)
        {
            if (entityContractDeclaration.HasPk)
            {
                output.Write(
$@"
        #region STATIC");

                WritePublicStaticPkComparerFields(output, entityContractDeclaration);
                output.Write(
@"
        ");

                WritePublicStaticGetPkMethod(output, entityContractDeclaration);
                output.Write(
@"
        ");

                WritePublicStaticSetPkMethod(output, entityContractDeclaration);

                output.Write(
$@"
        #endregion STATIC
        ");
            }
            return output;
        }

        private TextWriter WritePublicStaticPkComparerFields(TextWriter output, EntityContractDeclarationModel entityContractDeclaration)
        {
            output.Write(
$@"
        /// <summary>
        /// Read-only field for the primary key equality comparer of the '{entityContractDeclaration.FriendlyName}' entity.
        /// </summary>
        public static readonly IEqualityComparer<{entityContractDeclaration.FriendlyName}> PkEqualityComparer = new EqualityComparer<{entityContractDeclaration.FriendlyName}>(GetPk, EqualityComparer<{GetPrimaryKeyTypeName(entityContractDeclaration)}>.Default);
        
        /// <summary>
        /// Read-only field for the primary key comparer of the '{entityContractDeclaration.FriendlyName}' entity.
        /// </summary>
        public static readonly IComparer<{entityContractDeclaration.FriendlyName}> PkComparer = new Comparer<{entityContractDeclaration.FriendlyName}>(GetPk, Comparer<{GetPrimaryKeyTypeName(entityContractDeclaration)}>.Default);");

            return output;
        }

        private TextWriter WritePublicStaticGetPkMethod(TextWriter output, EntityContractDeclarationModel entityContractDeclaration)
        {
            output.Write(
$@"
        /// <summary>
        /// Gets the primary key of the given '{entityContractDeclaration.FriendlyName}' instance.
        /// </summary>
        /// <param name=""entity"">The '{entityContractDeclaration.FriendlyName}' instance to get primary key of.</param>
        /// <returns>The primary key value of the given '{entityContractDeclaration.FriendlyName}' instance.</returns>
        /// <exception cref=""ArgumentNullException"">Thrown if <paramref name=""entity""/> is null.</exception>
        public static {GetPrimaryKeyTypeName(entityContractDeclaration)} GetPk<TEntity>({entityContractDeclaration.FriendlyName} entity)
            where TEntity : I{entityContractDeclaration.FriendlyName}
        {{
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return ");
            AppendGettingPrimaryKey(output, entityContractDeclaration, "entity");
            output.Write(
@";
        }");

            return output;
        }

        private TextWriter WritePublicStaticSetPkMethod(TextWriter output, EntityContractDeclarationModel entityContractDeclaration)
        {
            output.Write(
$@"
        /// <summary>
        /// Sets the primary key of the given '{entityContractDeclaration.FriendlyName}' instance to the given value.
        /// </summary>
        /// <param name=""entity"">The '{entityContractDeclaration.FriendlyName}' instance to set primary key of.</param>
        /// <param name=""pk"">The primary key to set.</param>
        /// <exception cref=""ArgumentNullException"">Thrown if <paramref name=""entity""/> is null.</exception>
        public static void SetPk<TEntity>(TEntity entity, {GetPrimaryKeyTypeName(entityContractDeclaration)} pk)
            where TEntity : I{entityContractDeclaration.FriendlyName}
        {{
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            ");
            AppendSettingPrimaryKey(output, entityContractDeclaration, "entity", "pk");
            output.Write(
@";
        }");

            return output;
        }

        private TextWriter AppendGettingPrimaryKey(TextWriter output, EntityContractDeclarationModel entityContractDeclaration, string paramName)
        {
            if (entityContractDeclaration.HasCompositePk)
                return AppendGettingCompositePk(output, entityContractDeclaration, paramName);
            else
                return AppendGettingSimplePk(output, entityContractDeclaration, paramName);
        }

        private TextWriter AppendGettingSimplePk(TextWriter output, EntityContractDeclarationModel entityContractDeclaration, string paramName)
        {
            output.Write($"{paramName}.{entityContractDeclaration.PkProperties[0].Name}");

            return output;
        }

        private TextWriter AppendGettingCompositePk(TextWriter output, EntityContractDeclarationModel entityContractDeclaration, string paramName)
        {
            bool appendSeparator = false;

            output.Write($"new {entityContractDeclaration.FriendlyName}Pk(");

            foreach (var pkProp in entityContractDeclaration.PkProperties)
            {
                if (appendSeparator)
                    output.Write($", ");

                output.Write($"{paramName}.{pkProp.Name}");
                appendSeparator = true;
            }

            foreach (var pkProp in entityContractDeclaration.PkEntityReferences)
            {
                if (appendSeparator)
                    output.Write($", ");

                output.Write($"{paramName}.{pkProp.Name}Id");
                appendSeparator = true;
            }

            output.Write($");");

            return output;
        }

        private TextWriter AppendSettingPrimaryKey(TextWriter output, EntityContractDeclarationModel entityContractDeclaration, string entityParamName, string pkParamName)
        {
            if (entityContractDeclaration.HasCompositePk)
                return AppendSettingCompositePk(output, entityContractDeclaration, entityParamName, pkParamName);
            else
                return AppendSettingSimplePk(output, entityContractDeclaration, entityParamName, pkParamName);
        }

        private TextWriter AppendSettingSimplePk(TextWriter output, EntityContractDeclarationModel entityContractDeclaration, string entityParamName, string pkParamName)
        {
            output.Write($"{entityParamName}.{entityContractDeclaration.PkProperties[0].Name} = {pkParamName}");

            return output;
        }

        private TextWriter AppendSettingCompositePk(TextWriter output, EntityContractDeclarationModel entityContractDeclaration, string entityParamName, string pkParamName)
        {
            bool appendSeparator = false;

            foreach (var pkProp in entityContractDeclaration.PkProperties)
            {
                if (appendSeparator)
                    output.Write($@"
                && ");

                output.Write($"{entityParamName}.{pkProp.Name} = {pkParamName}.{pkProp.Name}");
                appendSeparator = true;
            }

            foreach (var pkProp in entityContractDeclaration.PkEntityReferences)
            {
                if (appendSeparator)
                    output.Write($@"
                && ");

                output.Write($"{entityParamName}.{pkProp.Name}Id = {pkParamName}.{pkProp.Name}Id");
                appendSeparator = true;
            }

            return output;
        }
    }
}
