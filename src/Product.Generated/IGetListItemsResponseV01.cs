
using MyCompany;
using MyCompany.Attributes;
using MyCompany.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyRoslynTest.v01
{
    public interface IGetListItemsResponseV01 : IPagedBySkipTake, IHasId
    {
        /// <summary>
        /// Gets the 'Items' simple property value.
        /// </summary>
        IReadOnlyList<Tuple<int, string>> Items { get; }

        /// <summary>
        /// Gets the 'TotalCount' simple property value.
        /// </summary>
        int? TotalCount { get; }

    }
}