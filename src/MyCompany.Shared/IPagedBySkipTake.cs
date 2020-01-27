using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyCompany
{
    public interface IPagedBySkipTake
    {
        [Required]
        [Range(0, int.MaxValue)]
        int Skip { get; }

        [Required]
        [Range(1, int.MaxValue)]
        int Take { get; }
    }
}
