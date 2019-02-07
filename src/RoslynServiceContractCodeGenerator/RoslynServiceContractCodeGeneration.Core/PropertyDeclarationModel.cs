using System;
using System.Linq;
using System.Reflection;

namespace RoslynServiceContractCodeGeneration
{
    using Attributes;
    using Enums;
    using Extensions;

    public class PropertyDeclarationModel : IComparable<PropertyDeclarationModel>
    {
        public readonly PropertyInfo DeclaringProperty;
        public readonly PropertyCategoryEnum PropertyCategory;
        public readonly PrimaryKeyAttribute PkAttribute;
        public readonly EntityReferenceAttribute EntityRefAttribute;
        public readonly CustomAttributeData[] Attributes;
        public readonly string Name;
        public readonly string TypeFriendlyName;

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
        }

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

        public bool IsPk
        {
            get { return this.PkAttribute != null; }
        }
    }
}
