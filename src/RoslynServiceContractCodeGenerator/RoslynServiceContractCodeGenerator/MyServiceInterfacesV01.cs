using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRoslynTest01
{
    interface IPagedBySkipTake
    {
        [Required]
        [Range(0, int.MaxValue)]
        int Skip { get; }

        [Required]
        [Range(1, int.MaxValue)]
        int Take { get; }
    }

    interface IHasId
    {
        [Key]
        string Id { get; }
    }

    interface IGetListItemsRequestV01 : IPagedBySkipTake
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        string User { get; }
    }

    interface IGetListItemsResponseV01 : IPagedBySkipTake, IHasId
    {
        IReadOnlyList<Tuple<int, string>> Items { get; }

        int? TotalCount { get; }
    }
}
