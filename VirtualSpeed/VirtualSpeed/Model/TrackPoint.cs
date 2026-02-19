using System;

namespace VirtualSpeed.Model
{
    public record TrackPoint(
        double Latitude,
        double Longitude,
        double ElevationMeters,
        DateTime? Timestamp
    );
}
