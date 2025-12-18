namespace IndeConnect_Back.Domain.order;

/// <summary>
/// Service du domaine responsable de la génération des numéros de facture.
/// Format: INV-{yyyyMMdd}-{orderId}-{brandId}
/// </summary>
public static class InvoiceNumberGenerator
{
    /// <summary>
    /// Génère un numéro de facture unique basé sur la commande et la marque.
    /// </summary>
    /// <param name="orderId">L'identifiant de la commande</param>
    /// <param name="brandId">L'identifiant de la marque</param>
    /// <returns>Un numéro de facture au format INV-{yyyyMMdd}-{orderId}-{brandId}</returns>
    public static string Generate(long orderId, long brandId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        return $"INV-{timestamp}-{orderId}-{brandId}";
    }
}
