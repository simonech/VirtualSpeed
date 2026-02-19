using System;
using System.Linq;
using VirtualSpeed.Services;

namespace VirtualSpeed
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = null;
            double power = 0;
            bool hasPower = false;

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--gpx")
                    filePath = args[i + 1];
                else if (args[i] == "--power")
                {
                    if (!double.TryParse(args[i + 1], System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out power))
                    {
                        Console.WriteLine("Error: --power must be a valid number.");
                        return;
                    }
                    hasPower = true;
                }
            }

            if (filePath == null || !hasPower)
            {
                Console.WriteLine("Usage: app.exe --gpx <route.gpx> --power <watts>");
                return;
            }

            var parser = new GpxParser();
            var builder = new RouteBuilder();

            System.Collections.Generic.IEnumerable<Model.TrackPoint> points;
            try
            {
                points = parser.Parse(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while loading GPX file: " + e.Message);
                return;
            }

            var segments = builder.Build(points);

            if (segments.Count == 0)
            {
                Console.WriteLine("No segments found – check that the GPX file contains track points.");
                return;
            }

            var rideCalculator = new RideCalculator(new Parameters());
            var rideSegments = rideCalculator.Calculate(segments, power);

            double totalDistance = segments.Sum(s => s.LengthMeters);
            double totalElevationGain = segments
                .Where(s => s.EndElevationMeters > s.StartElevationMeters)
                .Sum(s => s.EndElevationMeters - s.StartElevationMeters);
            double totalTimeSeconds = rideSegments.Sum(r => r.DurationSeconds);
            double avgSpeedKmh = totalTimeSeconds > 0
                ? (totalDistance / totalTimeSeconds) * 3.6
                : 0;

            Console.WriteLine($"Total Distance: {totalDistance:F0} m");
            Console.WriteLine($"Total Elevation Gain: {totalElevationGain:F0} m");
            Console.WriteLine($"Total Time: {TimeSpan.FromSeconds(totalTimeSeconds):hh\\:mm\\:ss}");
            Console.WriteLine($"Avg Speed: {avgSpeedKmh:F1} km/h");
            Console.WriteLine();
            Console.WriteLine("Segments:");

            foreach (var ride in rideSegments)
            {
                var seg = ride.RouteSegment;
                double gradientPct = seg.AverageGradient * 100;
                Console.WriteLine($"{seg.StartDistanceMeters:F0}m - {seg.StartDistanceMeters + seg.LengthMeters:F0}m | Gradient: {gradientPct:F1}% | Speed: {ride.SpeedKmh:F1} km/h | Time: {ride.DurationSeconds:F0} s");
            }
        }
    }
}

