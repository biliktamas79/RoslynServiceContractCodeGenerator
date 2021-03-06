﻿using System;
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

    public class EntityClassGenerator
    {
        private readonly static HashSet<string> defaultNamespacesUsed = new HashSet<string>()
        {
            "System",
            "System.Collections.Generic",
            "System.ComponentModel.DataAnnotations",
            "System.ComponentModel.DataAnnotations.Schema",
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
    public class {entityContractDeclaration.FriendlyName}{GetBaseClassAndImplementedInterfaceListString(entityContractDeclaration)}
    {{");
            WriteStaticRegion(output, entityContractDeclaration);

            WritePropertyDeclarations(output, entityContractDeclaration);

            if (entityContractDeclaration.HasPk)
                WriteIHasPkInterfaceImplementation(output, entityContractDeclaration);

            foreach (var intrfce in entityContractDeclaration.DeclaringInterfaceType.GetInterfaces())
            {
                output.Write(
@"
        ");
                WriteInterfaceImplementation(output, entityContractDeclaration, intrfce);
            }

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
        public {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
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
        public virtual {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
            }

            foreach (var prop in entityContractDeclaration.NonPkProperties)
            {
                output.WriteLine(
$@"
        /// <summary>
        /// {GetPropertyGetSetXmlCommentPrefix(prop)} the '{prop.Name}' simple property value.
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
        /// {GetPropertyGetSetXmlCommentPrefix(prop)} the '{prop.Name}' navigation property value.
        /// </summary>{GetAttributeDeclarations(prop)}
        public virtual {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
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

            foreach (var pkEntityRefProp in entityContractDeclaration.PkEntityReferences)
            {
                if (appendSeparator)
                    output.Write($", ");

                output.Write($"{paramName}.{pkEntityRefProp.Name}Id");
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

        private TextWriter WriteIHasPkInterfaceImplementation(TextWriter output, EntityContractDeclarationModel entityContractDeclaration)
        {
            output.Write(
$@"
        #region IHasPk<{GetPrimaryKeyTypeName(entityContractDeclaration)}> implementation");

            WritePublicInstanceGetPkMethod(output, entityContractDeclaration);

            output.Write(
@"
        ");
            WritePublicInstanceSetPkMethod(output, entityContractDeclaration);

            output.Write(
$@"
        #endregion IHasPk<{GetPrimaryKeyTypeName(entityContractDeclaration)}> implementation");

            return output;
        }

        private TextWriter WritePublicInstanceGetPkMethod(TextWriter output, EntityContractDeclarationModel entityContractDeclaration)
        {
            output.Write(
$@"
        /// <summary>
        /// Gets the primary key of this '{entityContractDeclaration.FriendlyName}' instance.
        /// </summary>
        /// <returns>The primary key value of this '{entityContractDeclaration.FriendlyName}' instance.</returns>
        public {GetPrimaryKeyTypeName(entityContractDeclaration)} GetPk()
        {{
            return ");
            AppendGettingPrimaryKey(output, entityContractDeclaration, "this");
            output.Write(
@";
        }");

            return output;
        }

        private TextWriter WritePublicInstanceSetPkMethod(TextWriter output, EntityContractDeclarationModel entityContractDeclaration)
        {
            output.Write(
$@"
        /// <summary>
        /// Sets the primary key of this '{entityContractDeclaration.FriendlyName}' instance to the given value.
        /// </summary>
        /// <param name=""pk"">The primary key to set.</param>
        public void SetPk({GetPrimaryKeyTypeName(entityContractDeclaration)} pk)
        {{
            ");
            AppendSettingPrimaryKey(output, entityContractDeclaration, "this", "pk");
            output.Write(
@";
        }");

            return output;
        }

        private TextWriter WriteInterfaceImplementation(TextWriter output, EntityContractDeclarationModel entityContractDeclaration, Type intrfceType)
        {
            var intrfaceFriendlyTypeName = intrfceType.GetFriendlyTypeName();

            output.Write(
$@"
        #region {intrfaceFriendlyTypeName} implementation");

            output = WriteImplicitImplementationForInterfaceProperties(output, entityContractDeclaration, intrfceType);

//            output.Write(
//@"
//        ");
            output = WriteImplicitImplementationForInterfaceMethods(output, entityContractDeclaration, intrfceType);

            output.Write(
$@"
        #endregion {intrfaceFriendlyTypeName} implementation");

            return output;
        }

        private TextWriter WriteImplicitImplementationForInterfaceProperties(TextWriter output, EntityContractDeclarationModel entityContractDeclaration, Type intrfceType)
        {
            foreach (var pi in intrfceType.GetProperties())
            {
                var prop = new PropertyDeclarationModel(pi);

                output.WriteLine(
$@"
        /// <summary>
        /// {GetPropertyGetSetXmlCommentPrefix(prop)} the '{prop.Name}' property value.
        /// </summary>{GetAttributeDeclarations(prop)}
        public {prop.TypeFriendlyName} {prop.Name} {{ {GetPropertyGetSetDeclaration(prop)} }}");
            }

            return output;
        }

        private TextWriter WriteImplicitImplementationForInterfaceMethods(TextWriter output, EntityContractDeclarationModel entityContractDeclaration, Type intrfceType)
        {
            foreach (var mi in intrfceType.GetMethods())
            {
                if (mi.IsStatic || !mi.IsPublic || mi.IsSpecialName)
                    continue;

                var methodParams = mi.GetParameters() ?? Array.Empty<ParameterInfo>();

                output.WriteLine(
$@"
        /// <summary>
        /// 
        /// </summary>
        public {mi.ReturnType.GetFriendlyTypeName()} {mi.Name}({GetMethodParameterDeclaration(mi)})
        {{
            // TODO Implement method
        }}");

            }

            return output;
        }

        private string GetMethodParameterDeclaration(MethodInfo mi)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var methodParam in mi.GetParameters())
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(methodParam.ParameterType.GetFriendlyTypeName()).Append(" ").Append(methodParam.Name);
            }

            return sb.ToString();
        }
    }
}
