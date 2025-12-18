namespace IndeConnect_Back.Domain.order;

using IndeConnect_Back.Domain.catalog.brand;
using IndeConnect_Back.Domain.user;

/// <summary>
/// Service du domaine pour calculer les délais de livraison estimés.
/// Calcule la date de livraison basée sur la distance entre le dépôt et l'adresse de livraison,
/// ainsi que les délais de la méthode de livraison choisie.
/// </summary>
public class DeliveryEstimator
{
    /// <summary>
    /// Calcule la date estimée de livraison basée sur :
    /// 1. La distance entre le dépôt de la marque et l'adresse de livraison
    /// 2. Les délais de la méthode de livraison choisie
    /// </summary>
    /// <param name="deposit">Le dépôt d'où part la commande</param>
    /// <param name="deliveryAddress">L'adresse de livraison</param>
    /// <param name="startDate">La date de début (généralement la date de commande)</param>
    /// <param name="shippingMethod">La méthode de livraison (optionnel)</param>
    /// <returns>La date estimée de livraison</returns>
    public static DateTimeOffset CalculateEstimatedDeliveryDate(
        Deposit deposit,
        ShippingAddress deliveryAddress,
        DateTimeOffset startDate,
        BrandShippingMethod? shippingMethod = null)
    {
        if (deposit == null)
            throw new ArgumentNullException(nameof(deposit));
        if (deliveryAddress == null)
            throw new ArgumentNullException(nameof(deliveryAddress));

        // Calculer le délai de base selon la distance (en heures)
        int baseHours = CalculateBaseDeliveryHours(deposit, deliveryAddress);

        // Ajouter le délai de la méthode de livraison si fournie
        int shippingMethodHours = 0;
        if (shippingMethod != null)
        {
            var shippingMethodAvgDays = (shippingMethod.EstimatedMinDays + shippingMethod.EstimatedMaxDays) / 2.0;
            shippingMethodHours = (int)(shippingMethodAvgDays * 24);
        }

        var totalHours = baseHours + shippingMethodHours;
        return startDate.AddHours(totalHours);
    }

    /// <summary>
    /// Calcule le délai de base en heures selon la distance géographique.
    /// - Même ville : 24h
    /// - Même pays : 48h
    /// - Pays différent : 72h
    /// </summary>
    private static int CalculateBaseDeliveryHours(Deposit deposit, ShippingAddress deliveryAddress)
    {
        // Même ville : 24h
        if (deposit.City?.Trim().Equals(deliveryAddress.City?.Trim(), StringComparison.OrdinalIgnoreCase) == true)
        {
            return 24;
        }

        // Même pays : 48h
        if (deposit.Country?.Trim().Equals(deliveryAddress.Country?.Trim(), StringComparison.OrdinalIgnoreCase) == true)
        {
            return 48;
        }

        // Pays différent : 72h
        return 72;
    }
}
