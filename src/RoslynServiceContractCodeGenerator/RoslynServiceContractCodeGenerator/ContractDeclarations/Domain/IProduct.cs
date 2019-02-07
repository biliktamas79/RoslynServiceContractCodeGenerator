using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using RoslynServiceContractCodeGeneration.Attributes;

namespace RoslynServiceContractCodeGenerator.ContractDeclarations.Domain
{
    using RoslynServiceContractCodeGeneration.Enums;

    [EntityContractDeclaration("MyProduct.Domain.Entities")]
    public interface IProduct : IAuditableEntity
    {
        [PrimaryKey]
        int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        string Name { get; set; }

        int? WeightGramm { get; set; }

        [EntityReference(EntityReferenceMultiplicityEnum.One)]
        ICompany Owner { get; set; }
    }
}
