using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RoslynServiceContractCodeGeneration
{
    using Attributes;
    using Enums;
    using Extensions;

    public class EntityContractGeneratorModel
    {
        public readonly Type DeclaringInterfaceType;
        public readonly PropertyDeclarationModel[] PkProperties;
        public readonly PropertyDeclarationModel[] NonPkProperties;
        public readonly PropertyDeclarationModel[] EntityReferences;
        public readonly PropertyDeclarationModel[] PkEntityReferences;
        public readonly CustomAttributeData[] Attributes;
        public readonly string Name;
        public readonly string FriendlyName;

        public EntityContractGeneratorModel(Type entityContractDeclarationInterface)
        {
            if (entityContractDeclarationInterface == null)
                throw new ArgumentNullException(nameof(entityContractDeclarationInterface));
            if (!entityContractDeclarationInterface.IsInterface)
                throw new ArgumentException("The provided type is not an interface!", nameof(entityContractDeclarationInterface));
            // if it does not have the EntityContractDeclarationAttribute
            if (entityContractDeclarationInterface.GetCustomAttribute<EntityContractDeclarationAttribute>() == null)
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

            this.EnsureCompositePkOrder();
        }

        public bool HasPk
        {
            get { return this.PkProperties.Length + this.PkEntityReferences.Length > 0; }
        }

        public bool HasCompositePk
        {
            get { return this.PkProperties.Length + this.PkEntityReferences.Length > 1; }
        }

        public int PkPropertyCount
        {
            get { return this.PkProperties.Length + this.PkEntityReferences.Length; }
        }

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
    }
}