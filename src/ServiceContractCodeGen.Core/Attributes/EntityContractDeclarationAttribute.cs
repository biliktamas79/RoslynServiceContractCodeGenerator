using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceContractCodeGen.Attributes
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class EntityContractDeclarationAttribute : Attribute
    {
        public string Name { get; private set; }

        public string Namespace { get; private set; }

        public EntityContractDeclarationAttribute(string @namespace = null, string name = null)
        {
            this.Namespace = @namespace;
            this.Name = name;
        }
    }
}
