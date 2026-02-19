using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using VirtualSpeed.Model;

namespace VirtualSpeed.Services
{
    public class GpxParser
    {
        public IEnumerable<TrackPoint> Parse(string filePath)
        {
            var doc = new XmlDocument();
            doc.Load(filePath);

            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("gpx", "http://www.topografix.com/GPX/1/1");

            // Try namespaced query first, fall back to no-namespace
            var trkptNodes = doc.SelectNodes("//gpx:trk[1]/gpx:trkseg[1]/gpx:trkpt", ns);
            if (trkptNodes == null || trkptNodes.Count == 0)
                trkptNodes = doc.SelectNodes("//trk[1]/trkseg[1]/trkpt");

            if (trkptNodes == null)
                yield break;

            foreach (XmlNode node in trkptNodes)
            {
                var latAttr = node.Attributes?["lat"]?.Value;
                var lonAttr = node.Attributes?["lon"]?.Value;

                if (latAttr == null || lonAttr == null)
                    continue;

                double lat = double.Parse(latAttr, CultureInfo.InvariantCulture);
                double lon = double.Parse(lonAttr, CultureInfo.InvariantCulture);

                double elevation = 0;
                var eleNode = node.SelectSingleNode("gpx:ele", ns) ?? node.SelectSingleNode("ele");
                if (eleNode != null)
                    elevation = double.Parse(eleNode.InnerText, CultureInfo.InvariantCulture);

                DateTimeOffset? timestamp = null;
                var timeNode = node.SelectSingleNode("gpx:time", ns) ?? node.SelectSingleNode("time");
                if (timeNode != null && DateTimeOffset.TryParse(timeNode.InnerText, null, DateTimeStyles.RoundtripKind, out var parsedTime))
                    timestamp = parsedTime;

                yield return new TrackPoint(lat, lon, elevation, timestamp);
            }
        }
    }
}
