using System;
using System.Collections.Generic;
using System.Text;

namespace RoslynServiceContractCodeGeneration.Attributes
{
    /// <summary>
    /// Attribute for marking properties as part of the primary key.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the order number of this property in the primary key.
        /// (Default = -1)
        /// </summary>
        /// <value>
        /// The order number of this property in the primary key.
        /// </value>
        public int Order { get; set; } = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryKeyAttribute"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        public PrimaryKeyAttribute(int order = -1)
        {
            this.Order = order;
        }
    }
}
