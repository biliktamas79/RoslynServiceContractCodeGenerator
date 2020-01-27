
using MyCompany;
using MyCompany.Attributes;
using MyCompany.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyRoslynTest.v01
{
    public class GetListItemsRequestV01 : IGetListItemsRequestV01, IPagedBySkipTake
    {
        /// <summary>
        /// Gets the 'User' simple property value.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string User { get; }

        
        #region IPagedBySkipTake implementation
        /// <summary>
        /// Gets the 'Skip' property value.
        /// </summary>
        [Required]
        [Range(0, 2147483647)]
        public int Skip { get; }

        /// <summary>
        /// Gets the 'Take' property value.
        /// </summary>
        [Required]
        [Range(1, 2147483647)]
        public int Take { get; }

        #endregion IPagedBySkipTake implementation
    }
}