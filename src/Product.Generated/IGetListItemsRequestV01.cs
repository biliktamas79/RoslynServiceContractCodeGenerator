
using MyCompany;
using MyCompany.Attributes;
using MyCompany.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyRoslynTest.v01
{
    public interface IGetListItemsRequestV01 : IPagedBySkipTake
    {
        /// <summary>
        /// Gets the 'User' simple property value.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 1)]
        string User { get; }

    }
}