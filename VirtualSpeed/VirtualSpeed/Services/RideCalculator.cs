using System.Collections.Generic;
using VirtualSpeed.Model;

namespace VirtualSpeed.Services
{
    public class RideCalculator
    {
        private readonly Parameters _parameters;

        public RideCalculator(Parameters parameters)
        {
            _parameters = parameters;
        }

        public IReadOnlyList<RideSegment> Calculate(IReadOnlyList<RouteSegment> routeSegments, double powerWatts)
        {
            var rideSegments = new List<RideSegment>();
            var calculator = new VirtualSpeedCalculator(_parameters);

            foreach (var routeSegment in routeSegments)
            {
                _parameters.ClimbGrade = routeSegment.AverageGradient * 100;

                double speedKmh = calculator.CalculateVelocity(powerWatts);
                double speedMs = calculator.ConvertKmhToMS(speedKmh);
                double durationSeconds = speedMs > 0 ? routeSegment.LengthMeters / speedMs : 0;

                rideSegments.Add(new RideSegment(durationSeconds, speedKmh, powerWatts, routeSegment));
            }

            return rideSegments;
        }
    }
}
