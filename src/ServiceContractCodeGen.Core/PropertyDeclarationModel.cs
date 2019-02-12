﻿using System;
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
            this.Attributes = pi.CustomAttributes.Where(attr =>
                attr.AttributeType != typeof(PrimaryKeyAttribute)
                && attr.AttributeType != typeof(EntityReferenceAttribute))
                .ToArray() ?? new CustomAttributeData[0];

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