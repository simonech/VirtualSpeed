namespace VirtualSpeed.Model
{
    public record RouteSegment(
        double StartDistanceMeters,
        double LengthMeters,
        double StartElevationMeters,
        double EndElevationMeters,
        double AverageGradient
    );
}
