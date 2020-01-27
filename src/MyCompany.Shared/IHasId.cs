using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyCompany
{
    public interface IHasId
    {
        [Key]
        string Id { get; }
    }
}
