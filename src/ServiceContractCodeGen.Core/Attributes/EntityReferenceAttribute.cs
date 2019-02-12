using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceContractCodeGen.Attributes
{
    using Enums;

    /// <summary>
    /// Attribute for marking properties as navigation property.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class EntityReferenceAttribute : Attribute
    {
        /// <summary>
        /// Gets the multiplicity.
        /// </summary>
        /// <value>
        /// The multiplicity.
        /// </value>
        public EntityReferenceMultiplicityEnum Multiplicity { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryKeyAttribute"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        public EntityReferenceAttribute(EntityReferenceMultiplicityEnum multiplicity)
        {
            this.Multiplicity = multiplicity;
        }
    }
}
