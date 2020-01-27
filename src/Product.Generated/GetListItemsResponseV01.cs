
using MyCompany;
using MyCompany.Attributes;
using MyCompany.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyRoslynTest.v01
{
    public class GetListItemsResponseV01 : IGetListItemsResponseV01, IPagedBySkipTake, IHasId
    {
        /// <summary>
        /// Gets the 'Items' simple property value.
        /// </summary>
        public IReadOnlyList<Tuple<int, string>> Items { get; }

        /// <summary>
        /// Gets the 'TotalCount' simple property value.
        /// </summary>
        public int? TotalCount { get; }

        
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
        
        #region IHasId implementation
        /// <summary>
        /// Gets the 'Id' property value.
        /// </summary>
        [Key]
        public string Id { get; }

        #endregion IHasId implementation
    }
}