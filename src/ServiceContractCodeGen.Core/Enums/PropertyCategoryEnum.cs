using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceContractCodeGen.Enums
{
    [Flags]
    public enum PropertyCategoryEnum : int
    {
        PrimaryKey = 1,
        EntityRef = 2,
        EntityRefAsPrimaryKey = 3,
        NonPk = 4,
    }
}
