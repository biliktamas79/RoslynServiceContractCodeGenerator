using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace ServiceContractCodeGen
{
    using Attributes;
    using Enums;
    using Extensions;

    /// <summary>
    /// Model class representing a property declaration.
    /// </summary>
    /// <seealso cref="System.IComparable{ServiceContractCodeGen.PropertyDeclarationModel}" />
    public class PropertyDeclarationModel : IComparable<PropertyDeclarationModel>
    {
        public readonly PropertyInfo DeclaringProperty;
        public readonly PropertyCategoryEnum PropertyCategory;
        public readonly PrimaryKeyAttribute PkAttribute;
        public readonly EntityReferenceAttribute EntityRefAttribute;
        public readonly CustomAttributeData[] Attributes;
        public readonly string Name;
        public readonly string TypeFriendlyName;
        public readonly bool IsStatic;
        public readonly MethodInfo GetMethod;
        public readonly MethodInfo SetMethod;

        private static class Helper
        {
            [Required(AllowEmptyStrings = true)]
            private static string RequiredAllowEmptyStringsProperty { get; }

            [Required]
            private static string RequiredProperty { get; }

            internal static CustomAttributeData RequiredAllowEmptyStringsAttributeData;
            internal static CustomAttributeData RequiredAttributeData;

            static Helper()
            {
                RequiredAllowEmptyStringsAttributeData = typeof(Helper).GetProperty(nameof(RequiredAllowEmptyStringsProperty), BindingFlags.Static | BindingFlags.NonPublic)
                    .CustomAttributes.Where(attr => attr.AttributeType == typeof(RequiredAttribute))
                    .SingleOrDefault();
                RequiredAttributeData = typeof(Helper).GetProperty(nameof(RequiredProperty), BindingFlags.Static | BindingFlags.NonPublic)
                    .CustomAttributes.Where(attr => attr.AttributeType == typeof(RequiredAttribute))
                    .SingleOrDefault();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDeclarationModel"/> class for the given <see cref="PropertyInfo"/> instance.
        /// </summary>
        /// <param name="pi">The property info.</param>
        /// <exception cref="System.ArgumentNullException">pi</exception>
        public PropertyDeclarationModel(PropertyInfo pi)
        {
            this.DeclaringProperty = pi ?? throw new ArgumentNullException(nameof(pi));
            this.PkAttribute = pi.GetCustomAttribute<PrimaryKeyAttribute>(true);
            this.EntityRefAttribute = pi.GetCustomAttribute<EntityReferenceAttribute>(true);
            var attributeList = pi.CustomAttributes.Where(attr =>
                attr.AttributeType != typeof(PrimaryKeyAttribute)
                && attr.AttributeType != typeof(EntityReferenceAttribute))
                .ToList() ?? new List<CustomAttributeData>();
            
            // if this is an entity reference
            if ((this.EntityRefAttribute != null)
                // if it cannot be null
                && this.EntityRefAttribute.Multiplicity == EntityReferenceMultiplicityEnum.One
                // and the Required attribute is not applied yet
                && !pi.CustomAttributes.Any(customAttribute => customAttribute.AttributeType == typeof(RequiredAttribute)))
            {
                // we add it to the property declaration model attributes
                attributeList.Add(Helper.RequiredAttributeData);
            }

            this.Attributes = attributeList.ToArray();

            if ((this.PkAttribute != null) && (this.EntityRefAttribute != null))
                this.PropertyCategory = PropertyCategoryEnum.EntityRefAsPrimaryKey;
            else if (this.PkAttribute != null)
                this.PropertyCategory = PropertyCategoryEnum.PrimaryKey;
            else if (this.EntityRefAttribute != null)
                this.PropertyCategory = PropertyCategoryEnum.EntityRef;
            else
                this.PropertyCategory = PropertyCategoryEnum.NonPk;

            this.Name = pi.Name;
            this.TypeFriendlyName = pi.PropertyType.GetFriendlyTypeName();

            if (pi.CanRead)
            {
                this.GetMethod = pi.GetGetMethod();
                if (this.GetMethod.IsStatic)
                    this.IsStatic = true;
            }

            if (pi.CanWrite)
            {
                this.SetMethod = pi.GetSetMethod();
                if (this.SetMethod.IsStatic)
                    this.IsStatic = true;
            }
        }

        /// <summary>
        /// Compares this to the other instance.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns></returns>
        public int CompareTo(PropertyDeclarationModel other)
        {
            if (other == null)
                return 1;

            var result = Math.Sign(this.PropertyCategory - other.PropertyCategory);
            if (result != 0)
                return result;

            if ((this.PkAttribute != null) && (other.PkAttribute != null))
            {
                result = Math.Sign(this.PkAttribute.Order - other.PkAttribute.Order);
                if (result != 0)
                    return result;
            }

            return string.Compare(this.DeclaringProperty.Name, other.DeclaringProperty.Name);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is pk.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is pk; otherwise, <c>false</c>.
        /// </value>
        public bool IsPk
        {
            get { return this.PkAttribute != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is entity reference.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is entity reference; otherwise, <c>false</c>.
        /// </value>
        public bool IsEntityReference
        {
            get { return this.EntityRefAttribute != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this property can get.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can get; otherwise, <c>false</c>.
        /// </value>
        public bool CanGet
        {
            get { return this.GetMethod != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this property can set.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can set; otherwise, <c>false</c>.
        /// </value>
        public bool CanSet
        {
            get { return this.SetMethod != null; }
        }
    }
}
