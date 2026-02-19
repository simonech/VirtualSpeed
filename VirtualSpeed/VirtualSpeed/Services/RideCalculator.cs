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

        public static double GetGradientPacingMultiplier(double gradient)
        {
            double gradientPct = gradient * 100.0;

            if (gradientPct < -5.0)
                return 0.85;
            if (gradientPct < -1.0)
                return 0.90;
            if (gradientPct < 1.0)
                return 1.00;
            if (gradientPct < 3.0)
                return 1.02;
            if (gradientPct < 6.0)
                return 1.07;
            return 1.10;
        }

        public IReadOnlyList<RideSegment> Calculate(IReadOnlyList<RouteSegment> routeSegments, double powerWatts)
        {
            var rideSegments = new List<RideSegment>();
            var calculator = new VirtualSpeedCalculator(_parameters);

            foreach (var routeSegment in routeSegments)
            {
                double adjustedPower = powerWatts * GetGradientPacingMultiplier(routeSegment.AverageGradient);
                double speedKmh = calculator.CalculateVelocity(adjustedPower, routeSegment.AverageGradient);
                double speedMs = calculator.ConvertKmhToMS(speedKmh);
                double durationSeconds = speedMs > 0 ? routeSegment.LengthMeters / speedMs : 0;

                rideSegments.Add(new RideSegment(durationSeconds, speedKmh, adjustedPower, routeSegment));
            }

            return rideSegments;
        }
    }
}
