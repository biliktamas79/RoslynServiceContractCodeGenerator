using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ServiceContractCodeGen.Attributes;

namespace RoslynServiceContractCodeGenerator.ContractDeclarations.Data
{
    [EntityContractDeclaration("MyProduct.Data.Entities")]
    public interface ICompany : IAuditableEntity
    {
        [PrimaryKey]
        int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        string Name { get; set; }

        [StringLength(40, MinimumLength = 0)]
        string ExternalId { get; set; }
    }
}
