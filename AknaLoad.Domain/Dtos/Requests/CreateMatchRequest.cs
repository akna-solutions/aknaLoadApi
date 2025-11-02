using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AknaLoad.Domain.Dtos.Requests
{
    public class CreateMatchRequest
    {
        public long LoadId { get; set; }
        public long DriverId { get; set; }
        public long VehicleId { get; set; }
    }
}
