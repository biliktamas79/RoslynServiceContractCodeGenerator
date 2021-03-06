﻿using MyCompany;
using ServiceContractCodeGen.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynServiceContractCodeGenerator
{
    [EntityContractDeclaration("MyRoslynTest.v01")]
    public interface IGetListItemsRequestV01 : IPagedBySkipTake
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        string User { get; }
    }

    [EntityContractDeclaration("MyRoslynTest.v01")]
    public interface IGetListItemsResponseV01 : IPagedBySkipTake, IHasId
    {
        IReadOnlyList<Tuple<int, string>> Items { get; }

        int? TotalCount { get; }
    }
}
