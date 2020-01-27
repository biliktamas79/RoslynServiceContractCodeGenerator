using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using MyCompany;
using ServiceContractCodeGen.Attributes;

namespace RoslynServiceContractCodeGenerator.ContractDeclarations.Data
{
    [EntityContractDeclaration("MyProduct.Data.Entities")]
    public interface IProduct : IAuditableEntity
    {
        [PrimaryKey]
        int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        string Name { get; set; }

        int? WeightGramm { get; set; }
    }
}
