using MyCompany;
using ServiceContractCodeGen.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynServiceContractCodeGenerator
{
    [EntityContractDeclaration("MyRoslynTest.v02")]
    public interface IGetListItemsResponseV02 : IPagedBySkipTake, IHasId
    {
        IGetListItemsRequestV01 Request { get; }

        IReadOnlyList<Tuple<int, string>> Items { get; }

        int? TotalCount { get; }
    }
}
