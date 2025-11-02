using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AknaLoad.Domain.Dtos.Requests
{
    public class RejectMatchRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
