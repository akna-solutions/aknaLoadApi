using AknaLoad.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AknaLoad.Domain.Dtos.Requests
{
    public class UpdateStatusRequest
    {
        public TrackingStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
