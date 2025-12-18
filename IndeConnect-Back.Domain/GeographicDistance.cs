namespace IndeConnect_Back.Domain;

/// <summary>
/// Service du domaine pour les calculs de distance géographique.
/// Utilise l'algorithme de Haversine pour calculer la distance entre deux points GPS.
/// </summary>
public static class GeographicDistance
{
    private const double EarthRadiusKm = 6371; // Rayon de la Terre en kilomètres

    /// <summary>
    /// Calcule la distance entre deux points géographiques en utilisant l'algorithme de Haversine.
    /// </summary>
    /// <param name="lat1">Latitude du premier point (en degrés)</param>
    /// <param name="lon1">Longitude du premier point (en degrés)</param>
    /// <param name="lat2">Latitude du second point (en degrés)</param>
    /// <param name="lon2">Longitude du second point (en degrés)</param>
    /// <returns>La distance en kilomètres</returns>
    public static double CalculateKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    /// <summary>
    /// Convertit des degrés en radians.
    /// </summary>
    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
