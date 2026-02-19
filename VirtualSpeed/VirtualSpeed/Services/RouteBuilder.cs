using System;
using System.Collections.Generic;
using System.Linq;
using VirtualSpeed.Model;

namespace VirtualSpeed.Services
{
    public class RouteBuilder
    {
        private readonly double _segmentLengthMeters;

        public RouteBuilder(double segmentLengthMeters = 100.0)
        {
            _segmentLengthMeters = segmentLengthMeters;
        }

        public IReadOnlyList<RouteSegment> Build(IEnumerable<TrackPoint> points)
        {
            var pointList = points.ToList();

            // Sort by timestamp if all points have one
            if (pointList.All(p => p.Timestamp.HasValue))
                pointList = pointList.OrderBy(p => p.Timestamp!.Value).ToList();

            var segments = new List<RouteSegment>();

            if (pointList.Count < 2)
                return segments;

            double segmentStart = 0.0;
            double segmentStartElevation = pointList[0].ElevationMeters;
            double accumulatedDistance = 0.0;

            for (int i = 1; i < pointList.Count; i++)
            {
                var prev = pointList[i - 1];
                var curr = pointList[i];

                double stepDistance = HaversineDistance(prev, curr);
                if (stepDistance <= 0)
                    continue;
                accumulatedDistance += stepDistance;

                while (accumulatedDistance >= _segmentLengthMeters)
                {
                    // How far into this step does the segment boundary fall?
                    double overshoot = accumulatedDistance - _segmentLengthMeters;
                    double fraction = (stepDistance - overshoot) / stepDistance;

                    double boundaryElevation = prev.ElevationMeters + fraction * (curr.ElevationMeters - prev.ElevationMeters);

                    double gradient = _segmentLengthMeters > 0
                        ? (boundaryElevation - segmentStartElevation) / _segmentLengthMeters
                        : 0.0;

                    segments.Add(new RouteSegment(
                        segmentStart,
                        _segmentLengthMeters,
                        segmentStartElevation,
                        boundaryElevation,
                        gradient
                    ));

                    segmentStart += _segmentLengthMeters;
                    segmentStartElevation = boundaryElevation;
                    accumulatedDistance -= _segmentLengthMeters;
                }
            }

            // Final (possibly shorter) segment
            double lastElevation = pointList[pointList.Count - 1].ElevationMeters;
            double remaining = accumulatedDistance;
            if (remaining > 0)
            {
                double gradient = (lastElevation - segmentStartElevation) / remaining;

                segments.Add(new RouteSegment(
                    segmentStart,
                    remaining,
                    segmentStartElevation,
                    lastElevation,
                    gradient
                ));
            }

            return segments;
        }

        private static double HaversineDistance(TrackPoint a, TrackPoint b)
        {
            const double R = 6371000.0; // Earth radius in meters

            double lat1 = ToRadians(a.Latitude);
            double lat2 = ToRadians(b.Latitude);
            double dLat = ToRadians(b.Latitude - a.Latitude);
            double dLon = ToRadians(b.Longitude - a.Longitude);

            double h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(lat1) * Math.Cos(lat2)
                     * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return R * 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
