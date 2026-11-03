namespace Traveller_backend.Models
{
    // A lat/lng box, used to pre-filter rows in SQL before the exact distance check
    public record GeoBounds(decimal MinLatitude, decimal MaxLatitude, decimal MinLongitude, decimal MaxLongitude);

    // Distance helpers for "nearby" searches
    public static class GeoHelper
    {
        private const double EarthRadiusKm = 6371;

        // Straight-line distance between two points (Haversine formula)
        public static double DistanceKm(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            var dLat = ToRadians((double)(lat2 - lat1));
            var dLng = ToRadians((double)(lng2 - lng1));

            var a = Math.Pow(Math.Sin(dLat / 2), 2)
                  + Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) * Math.Pow(Math.Sin(dLng / 2), 2);

            return EarthRadiusKm * 2 * Math.Asin(Math.Sqrt(a));
        }

        // Box that contains every point within radiusKm of the centre
        public static GeoBounds GetBounds(decimal latitude, decimal longitude, double radiusKm)
        {
            var latDelta = radiusKm / 111.0;
            var lngDelta = radiusKm / (111.0 * Math.Cos(ToRadians((double)latitude)));

            return new GeoBounds(
                latitude - (decimal)latDelta,
                latitude + (decimal)latDelta,
                longitude - (decimal)lngDelta,
                longitude + (decimal)lngDelta);
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;
    }
}
