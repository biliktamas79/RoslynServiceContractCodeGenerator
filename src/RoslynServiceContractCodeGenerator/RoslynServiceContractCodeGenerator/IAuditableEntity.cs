using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RoslynServiceContractCodeGenerator
{
    public interface IAuditableEntity
    {
        [Required]
        DateTime CreatedAtTimeUtc { get; set; }
        string CreatedBy { get; set; }

        [Required]
        DateTime LastModifiedAtTimeUtc { get; set; }
        string LastModifiedBy { get; set; }

        DateTime? DeletedAtTimeUtc { get; set; }
        string DeletedBy { get; set; }
    }
}
