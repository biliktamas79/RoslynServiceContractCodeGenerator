using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using MyCompany;
using MyCompany.Attributes;
using MyCompany.Enums;
using ServiceContractCodeGen.Attributes;
using ServiceContractCodeGen.Enums;

namespace RoslynServiceContractCodeGenerator.ContractDeclarations.Domain
{
    [EntityContractDeclaration("MyProduct.Domain.Entities")]
    public interface ICompany : IAuditableEntity
    {
        [PrimaryKey]
        int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the external identifier.
        /// </summary>
        [StringLength(40, MinimumLength = 0)]
        string ExternalId { get; set; }

        [EntityReference(EntityReferenceMultiplicityEnum.Many)]
        IProduct Products { get; set; }
    }
}
