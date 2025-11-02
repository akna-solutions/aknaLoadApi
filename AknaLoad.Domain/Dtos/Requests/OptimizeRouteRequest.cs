
namespace AknaLoad.Domain.Dtos.Requests
{
    /// <summary>
    /// Request for route optimization
    /// </summary>
    public class OptimizeRouteRequest
    {
        public long LoadId { get; set; }
        public string OptimizationCriteria { get; set; } = "Distance"; // Distance, Time, Cost
        public List<RouteConstraint>? Constraints { get; set; }
    }
}
