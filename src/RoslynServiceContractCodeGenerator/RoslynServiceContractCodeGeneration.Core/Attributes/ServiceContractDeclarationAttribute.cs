using System;
using System.Collections.Generic;
using System.Text;

namespace RoslynServiceContractCodeGeneration.Attributes
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ServiceContractDeclarationAttribute : Attribute
    {
        public string Name { get; private set; }

        public string Namespace { get; private set; }

        public ServiceContractDeclarationAttribute(string ns = null, string name = null)
        {
            this.Namespace = ns;
            this.Name = name;
        }
    }
}
