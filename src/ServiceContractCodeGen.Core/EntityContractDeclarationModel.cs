﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ServiceContractCodeGen
{
    using System.Collections;
    using Attributes;
    using Enums;
    using Extensions;

    /// <summary>
    /// Model class representing an entity declaration.
    /// </summary>
    public class EntityContractDeclarationModel : IEnumerable<PropertyDeclarationModel>
    {
        [NonSerialized]
        public readonly Type DeclaringInterfaceType;
        [NonSerialized]
        public readonly EntityContractDeclarationAttribute EntityContractDeclarationAttribute;
        public readonly PropertyDeclarationModel[] PkProperties;
        public readonly PropertyDeclarationModel[] NonPkProperties;
        public readonly PropertyDeclarationModel[] EntityReferences;
        public readonly PropertyDeclarationModel[] PkEntityReferences;
        [NonSerialized]
        public readonly CustomAttributeData[] Attributes;
        public readonly string Name;
        public readonly string FriendlyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityContractDeclarationModel"/> class with the given entity contract declaration interface.
        /// </summary>
        /// <param name="entityContractDeclarationInterface">The entity contract declaration interface.</param>
        /// <exception cref="System.ArgumentNullException">entityContractDeclarationInterface</exception>
        /// <exception cref="System.ArgumentException">
        /// The provided type is not an interface! - entityContractDeclarationInterface
        /// or
        /// The provided entity contract declaration interface does not have the EntityContractDeclarationAttribute applied
        /// </exception>
        /// <exception cref="System.NotSupportedException"></exception>
        public EntityContractDeclarationModel(Type entityContractDeclarationInterface)
        {
            if (entityContractDeclarationInterface == null)
                throw new ArgumentNullException(nameof(entityContractDeclarationInterface));
            if (!entityContractDeclarationInterface.IsInterface)
                throw new ArgumentException("The provided type is not an interface!", nameof(entityContractDeclarationInterface));

            this.EntityContractDeclarationAttribute = entityContractDeclarationInterface.GetCustomAttribute<EntityContractDeclarationAttribute>();
            // if it does not have the EntityContractDeclarationAttribute
            if (this.EntityContractDeclarationAttribute == null)
                throw new ArgumentException($"The provided '{entityContractDeclarationInterface.Name}' type does not have the {nameof(EntityContractDeclarationAttribute)}!", nameof(entityContractDeclarationInterface));

            this.DeclaringInterfaceType = entityContractDeclarationInterface;

            List<PropertyDeclarationModel> pkPropList = new List<PropertyDeclarationModel>();
            List<PropertyDeclarationModel> nonPkPropList = new List<PropertyDeclarationModel>();
            List<PropertyDeclarationModel> entRefPropList = new List<PropertyDeclarationModel>();
            List<PropertyDeclarationModel> pkEntRefPropList = new List<PropertyDeclarationModel>();

            foreach (var prop in entityContractDeclarationInterface.GetProperties())
            {
                var model = new PropertyDeclarationModel(prop);

                switch (model.PropertyCategory)
                {
                    case PropertyCategoryEnum.PrimaryKey:
                        pkPropList.Add(model);
                        break;

                    case PropertyCategoryEnum.NonPk:
                        nonPkPropList.Add(model);
                        break;

                    case PropertyCategoryEnum.EntityRef:
                        entRefPropList.Add(model);
                        break;

                    case PropertyCategoryEnum.EntityRefAsPrimaryKey:
                        pkEntRefPropList.Add(model);
                        break;

                    default:
                        throw new NotSupportedException($"{nameof(PropertyCategoryEnum)}.{model.PropertyCategory} is not supported!");
                }
            }

            this.PkProperties = pkPropList.ToArray();
            this.NonPkProperties = nonPkPropList.ToArray();
            this.EntityReferences = entRefPropList.ToArray();
            this.PkEntityReferences = pkEntRefPropList.ToArray();

            this.Attributes = entityContractDeclarationInterface.CustomAttributes.Where(attr =>
                attr.AttributeType != typeof(EntityContractDeclarationAttribute)
                && attr.AttributeType != typeof(ServiceContractDeclarationAttribute))
                .ToArray() ?? new CustomAttributeData[0];

            this.Name = entityContractDeclarationInterface.Name;
            this.FriendlyName = entityContractDeclarationInterface.GetFriendlyTypeName();
            if ((this.Name[0] == 'I') && (this.Name.Length > 1) && char.IsUpper(this.Name[1]))
            {
                this.Name = this.Name.Substring(1);
                this.FriendlyName = this.FriendlyName.Substring(1);
            }

            this.EnsureCompositePkOrder();
        }

        /// <summary>
        /// Gets a value indicating whether this instance has pk.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has pk; otherwise, <c>false</c>.
        /// </value>
        public bool HasPk
        {
            get { return this.PkProperties.Length + this.PkEntityReferences.Length > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has composite pk.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has composite pk; otherwise, <c>false</c>.
        /// </value>
        public bool HasCompositePk
        {
            get { return this.PkProperties.Length + this.PkEntityReferences.Length > 1; }
        }

        /// <summary>
        /// Gets the pk property count.
        /// </summary>
        /// <value>
        /// The pk property count.
        /// </value>
        public int PkPropertyCount
        {
            get { return this.PkProperties.Length + this.PkEntityReferences.Length; }
        }

        /// <summary>
        /// Ensures the composite pk order.
        /// </summary>
        /// <exception cref="System.ArgumentException">Order of properties marked as private key is not continuous.</exception>
        private void EnsureCompositePkOrder()
        {
            if (this.HasCompositePk)
            {
                var list = new List<PropertyDeclarationModel>(this.PkPropertyCount);
                list.AddRange(this.PkProperties);
                list.AddRange(this.PkEntityReferences);
                list.Sort();

                int pkOrder = -1;
                foreach (var pk in list)
                {
                    if (pk.PkAttribute.Order != pkOrder + 1)
                        throw new ArgumentException($"Order of properties marked as private key is not continuous.");
                }
            }
        }

        public IEnumerator<PropertyDeclarationModel> GetEnumerator()
        {
            foreach (var p in this.PkProperties)
                yield return p;

            foreach (var p in this.PkEntityReferences)
                yield return p;

            foreach (var p in this.EntityReferences)
                yield return p;

            foreach (var p in this.NonPkProperties)
                yield return p;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerable<PropertyDeclarationModel> GetProperties(PropertyCategoryEnum propertyCategoryFlagsToInclude = PropertyCategoryEnum.PrimaryKey | PropertyCategoryEnum.EntityRef | PropertyCategoryEnum.EntityRefAsPrimaryKey | PropertyCategoryEnum.NonPk)
        {
            if (propertyCategoryFlagsToInclude.HasFlag(PropertyCategoryEnum.PrimaryKey))
            {
                foreach (var p in this.PkProperties)
                    yield return p;
            }

            if (propertyCategoryFlagsToInclude.HasFlag(PropertyCategoryEnum.EntityRefAsPrimaryKey))
            {
                foreach (var p in this.PkEntityReferences)
                    yield return p;
            }

            if (propertyCategoryFlagsToInclude.HasFlag(PropertyCategoryEnum.EntityRef))
            {
                foreach (var p in this.EntityReferences)
                    yield return p;
            }

            if (propertyCategoryFlagsToInclude.HasFlag(PropertyCategoryEnum.NonPk))
            {
                foreach (var p in this.NonPkProperties)
                    yield return p;
            }
        }

        public IEnumerable<CustomAttributeData> GetCustomAttributes()
        {
            foreach (var a in this.Attributes)
                yield return a;

            foreach (var prop in this.GetProperties(PropertyCategoryEnum.PrimaryKey | PropertyCategoryEnum.EntityRef | PropertyCategoryEnum.EntityRefAsPrimaryKey | PropertyCategoryEnum.NonPk))
            {
                foreach (var a in prop.Attributes)
                {
                    yield return a;
                }
            }
        }
    }
}