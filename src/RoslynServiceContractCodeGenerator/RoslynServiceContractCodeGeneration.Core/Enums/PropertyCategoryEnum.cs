using System;
using System.Collections.Generic;
using System.Text;

namespace RoslynServiceContractCodeGeneration.Enums
{
    public enum PropertyCategoryEnum : int
    {
        PrimaryKey = 1,
        EntityRefAsPrimaryKey = 2,
        NonPk = 4,
        EntityRef = 8, 
    }
}
