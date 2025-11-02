using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AknaLoad.Domain.Dtos
{
    public class RouteConstraint
    {
        public int StopOrder { get; set; }
        public string ConstraintType { get; set; } = string.Empty; // MustBeBefore, MustBeAfter, TimeWindow, etc.
        public object? Value { get; set; }
    }
}
