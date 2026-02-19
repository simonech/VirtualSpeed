using System;
using System.Linq;
using VirtualSpeed.Services;

namespace VirtualSpeed
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args[0] != "--gpx")
            {
                Console.WriteLine("Usage: app.exe --gpx <route.gpx>");
                return;
            }

            string filePath = args[1];

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

            double totalDistance = segments.Sum(s => s.LengthMeters);
            double totalElevationGain = segments
                .Where(s => s.EndElevationMeters > s.StartElevationMeters)
                .Sum(s => s.EndElevationMeters - s.StartElevationMeters);

            Console.WriteLine($"Total Distance: {totalDistance:F0} m");
            Console.WriteLine($"Total Elevation Gain: {totalElevationGain:F0} m");
            Console.WriteLine();
            Console.WriteLine("Segments:");

            foreach (var seg in segments)
            {
                double gradientPct = seg.AverageGradient * 100;
                Console.WriteLine($"{seg.StartDistanceMeters:F0}m - {seg.StartDistanceMeters + seg.LengthMeters:F0}m | Gradient: {gradientPct:F1}%");
            }
        }
    }
}

