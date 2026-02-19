namespace VirtualSpeed.Model
{
    public record RideSegment(
        double DurationSeconds,
        double SpeedKmh,
        double PowerWatts,
        RouteSegment RouteSegment
    );
}
