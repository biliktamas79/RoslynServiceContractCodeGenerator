
using MyCompany;
using MyCompany.Attributes;
using MyCompany.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyProduct.Domain.Entities
{
    public interface IProduct : IHasPk<int>, IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the 'Id' primary key property value.
        /// </summary>
        [Key]
        int Id { get; set; }

        /// <summary>
        /// Gets or sets the 'Name' simple property value.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 1)]
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the 'WeightGramm' simple property value.
        /// </summary>
        int? WeightGramm { get; set; }

        /// <summary>
        /// Gets or sets the 'Owner' navigation property value.
        /// </summary>
        [Required]
        [EntityReference(EntityReferenceMultiplicityEnum.One)]
        ICompany Owner { get; set; }

    }
}