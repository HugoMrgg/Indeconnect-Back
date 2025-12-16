using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.user;
using Microsoft.Extensions.Configuration;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class OrderEmailTemplateService : IOrderEmailTemplateService
{
    private readonly string _frontendUrl;

    public OrderEmailTemplateService(IConfiguration configuration)
    {
        _frontendUrl = configuration["FRONTEND_URL"] ?? "https://indeconnect.com";
    }

    public string GenerateOrderConfirmationEmail(Order order, User user, ShippingAddress address)
    {
        var orderItemsHtml = string.Join("", order.Items.Select(item =>
            $@"<tr>
                <td style=""padding: 10px; border-bottom: 1px solid #ddd;"">{item.ProductName}</td>
                <td style=""padding: 10px; border-bottom: 1px solid #ddd; text-align: center;"">{item.Quantity}</td>
                <td style=""padding: 10px; border-bottom: 1px solid #ddd; text-align: right;"">{item.UnitPrice:F2} {order.Currency}</td>
            </tr>"));

        return BuildEmailTemplate(
            user.FirstName,
            "Confirmation de votre commande",
            $@"
            <p>Merci pour votre commande ! Nous avons bien reçu votre demande et elle est en attente de paiement.</p>

            <div style=""background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                <h3 style=""margin-top: 0; color: #000;"">Commande #{order.Id}</h3>
                <p style=""margin: 5px 0;""><strong>Date :</strong> {order.PlacedAt:dd/MM/yyyy à HH:mm}</p>
                <p style=""margin: 5px 0;""><strong>Statut :</strong> En attente de paiement</p>
            </div>

            <h3 style=""color: #000;"">Articles commandés</h3>
            <table style=""width: 100%; border-collapse: collapse; background-color: #fff;"">
                <thead>
                    <tr style=""background-color: #000; color: #fff;"">
                        <th style=""padding: 10px; text-align: left;"">Produit</th>
                        <th style=""padding: 10px; text-align: center;"">Quantité</th>
                        <th style=""padding: 10px; text-align: right;"">Prix</th>
                    </tr>
                </thead>
                <tbody>
                    {orderItemsHtml}
                </tbody>
                <tfoot>
                    <tr style=""font-weight: bold; background-color: #f0f0f0;"">
                        <td colspan=""2"" style=""padding: 10px; text-align: right;"">Total :</td>
                        <td style=""padding: 10px; text-align: right;"">{order.TotalAmount:F2} {order.Currency}</td>
                    </tr>
                </tfoot>
            </table>

            <h3 style=""color: #000; margin-top: 20px;"">Adresse de livraison</h3>
            <div style=""background-color: #fff; padding: 15px; border-radius: 5px;"">
                <p style=""margin: 5px 0;"">{address.Street}</p>
                <p style=""margin: 5px 0;"">{address.PostalCode} {address.City}</p>
                <p style=""margin: 5px 0;"">{address.Country}</p>
            </div>

            <p style=""margin-top: 20px;"">Veuillez procéder au paiement pour que nous puissions traiter votre commande.</p>

            <a href=""{_frontendUrl}/orders/{order.Id}"" class=""button"">Voir ma commande</a>",
            $"Commande #{order.Id}");
    }

    public string GeneratePaymentConfirmationEmail(Order order, User user)
    {
        return BuildEmailTemplate(
            user.FirstName,
            "Paiement confirmé",
            $@"
            <p>Bonne nouvelle ! Nous avons bien reçu votre paiement.</p>

            <div style=""background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                <h3 style=""margin-top: 0; color: #000;"">Commande #{order.Id}</h3>
                <p style=""margin: 5px 0;""><strong>Montant payé :</strong> {order.TotalAmount:F2} {order.Currency}</p>
                <p style=""margin: 5px 0;""><strong>Date de paiement :</strong> {DateTime.UtcNow:dd/MM/yyyy à HH:mm}</p>
                <p style=""margin: 5px 0;""><strong>Statut :</strong> Payée</p>
            </div>

            <p>Votre commande est maintenant confirmée et va être traitée par nos équipes.</p>
            <p>Vous recevrez un email de confirmation dès que votre colis sera expédié avec un numéro de suivi.</p>

            <a href=""{_frontendUrl}/orders/{order.Id}"" class=""button"">Suivre ma commande</a>",
            $"Commande #{order.Id}");
    }

    public string GenerateOrderProcessingEmail(Order order, User user)
    {
        return BuildEmailTemplate(
            user.FirstName,
            "Votre commande est en cours de traitement",
            $@"
            <p>Votre commande est maintenant en cours de traitement.</p>

            <div style=""background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                <h3 style=""margin-top: 0; color: #000;"">Commande #{order.Id}</h3>
                <p style=""margin: 5px 0;""><strong>Statut :</strong> En cours de traitement</p>
                <p style=""margin: 5px 0;""><strong>Montant :</strong> {order.TotalAmount:F2} {order.Currency}</p>
            </div>

            <p>Nos équipes préparent votre colis avec soin. Vous recevrez une notification dès qu'il sera expédié.</p>

            <a href=""{_frontendUrl}/orders/{order.Id}"" class=""button"">Suivre ma commande</a>",
            $"Commande #{order.Id}");
    }

    public string GenerateOrderShippedEmail(Order order, User user, Delivery delivery)
    {
        return BuildEmailTemplate(
            user.FirstName,
            "Votre colis a été expédié",
            $@"
            <p>Excellente nouvelle ! Votre colis a été expédié.</p>

            <div style=""background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                <h3 style=""margin-top: 0; color: #000;"">Commande #{order.Id}</h3>
                <p style=""margin: 5px 0;""><strong>Numéro de suivi :</strong> <span style=""font-family: monospace; background-color: #f0f0f0; padding: 2px 8px; border-radius: 3px;"">{delivery.TrackingNumber}</span></p>
                <p style=""margin: 5px 0;""><strong>Date d'expédition :</strong> {delivery.ShippedAt?.ToString("dd/MM/yyyy à HH:mm") ?? DateTime.UtcNow.ToString("dd/MM/yyyy à HH:mm")}</p>
                <p style=""margin: 5px 0;""><strong>Statut :</strong> Expédié</p>
            </div>

            <p>Votre colis est en route ! Vous pouvez suivre votre livraison avec le numéro de suivi ci-dessus.</p>
            <p>Vous recevrez des notifications régulières sur l'avancement de votre livraison.</p>

            <a href=""{_frontendUrl}/orders/{order.Id}"" class=""button"">Suivre mon colis</a>",
            $"Commande #{order.Id}");
    }

    public string GenerateOrderInTransitEmail(Order order, User user, Delivery delivery)
    {
        return BuildEmailTemplate(
            user.FirstName,
            "Votre colis est en transit",
            $@"
            <p>Votre colis est en route vers sa destination.</p>

            <div style=""background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                <h3 style=""margin-top: 0; color: #000;"">Commande #{order.Id}</h3>
                <p style=""margin: 5px 0;""><strong>Numéro de suivi :</strong> <span style=""font-family: monospace; background-color: #f0f0f0; padding: 2px 8px; border-radius: 3px;"">{delivery.TrackingNumber}</span></p>
                <p style=""margin: 5px 0;""><strong>Statut :</strong> En transit</p>
            </div>

            <p>Votre colis progresse bien et devrait arriver bientôt. Vous recevrez une notification dès qu'il sera en cours de livraison.</p>

            <a href=""{_frontendUrl}/orders/{order.Id}"" class=""button"">Suivre mon colis</a>",
            $"Commande #{order.Id}");
    }

    public string GenerateOrderOutForDeliveryEmail(Order order, User user, Delivery delivery)
    {
        return BuildEmailTemplate(
            user.FirstName,
            "Votre colis arrive aujourd'hui",
            $@"
            <p><strong>Votre colis sera livré aujourd'hui !</strong></p>

            <div style=""background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                <h3 style=""margin-top: 0; color: #000;"">Commande #{order.Id}</h3>
                <p style=""margin: 5px 0;""><strong>Numéro de suivi :</strong> <span style=""font-family: monospace; background-color: #f0f0f0; padding: 2px 8px; border-radius: 3px;"">{delivery.TrackingNumber}</span></p>
                <p style=""margin: 5px 0;""><strong>Statut :</strong> En cours de livraison</p>
            </div>

            <p>Le livreur est en route avec votre colis. Assurez-vous d'être disponible pour réceptionner votre commande.</p>
            <p>Si vous êtes absent, le livreur laissera un avis de passage.</p>

            <a href=""{_frontendUrl}/orders/{order.Id}"" class=""button"">Suivre mon colis</a>",
            $"Commande #{order.Id}");
    }

    public string GenerateOrderDeliveredEmail(Order order, User user, Delivery delivery)
    {
        return BuildEmailTemplate(
            user.FirstName,
            "Votre colis a été livré",
            $@"
            <p><strong>Votre commande a été livrée avec succès !</strong></p>

            <div style=""background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                <h3 style=""margin-top: 0; color: #000;"">Commande #{order.Id}</h3>
                <p style=""margin: 5px 0;""><strong>Numéro de suivi :</strong> <span style=""font-family: monospace; background-color: #f0f0f0; padding: 2px 8px; border-radius: 3px;"">{delivery.TrackingNumber}</span></p>
                <p style=""margin: 5px 0;""><strong>Date de livraison :</strong> {delivery.DeliveredAt?.ToString("dd/MM/yyyy à HH:mm") ?? DateTime.UtcNow.ToString("dd/MM/yyyy à HH:mm")}</p>
                <p style=""margin: 5px 0;""><strong>Statut :</strong> Livré</p>
            </div>

            <p>Nous espérons que vous êtes satisfait de votre commande !</p>
            <p>N'hésitez pas à nous laisser votre avis sur les produits que vous avez reçus. Cela aide notre communauté à faire les meilleurs choix.</p>

            <div style=""margin-top: 20px;"">
                <a href=""{_frontendUrl}/orders/{order.Id}"" class=""button"">Voir ma commande</a>
            </div>

            <p style=""margin-top: 20px; font-size: 12px; color: #666;"">
                Une question ? Un problème avec votre commande ? Contactez notre service client.
            </p>",
            $"Commande #{order.Id}");
    }

    // BrandDelivery overloads
    public string GenerateOrderShippedEmail(Order order, User user, BrandDelivery brandDelivery)
    {
        var brandName = brandDelivery.Brand?.Name ?? "la marque";
        return BuildEmailTemplate(
            user.FirstName,
            $"Colis {brandName} expédié",
            $@"
            <p>Excellente nouvelle ! Votre colis de <strong>{brandName}</strong> a été expédié.</p>

            <div style=""background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                <h3 style=""margin-top: 0; color: #000;"">Commande #{order.Id} - {brandName}</h3>
                <p style=""margin: 5px 0;""><strong>Numéro de suivi :</strong> <span style=""font-family: monospace; background-color: #f0f0f0; padding: 2px 8px; border-radius: 3px;"">{brandDelivery.TrackingNumber}</span></p>
                <p style=""margin: 5px 0;""><strong>Date d'expédition :</strong> {brandDelivery.ShippedAt?.ToString("dd/MM/yyyy à HH:mm") ?? DateTime.UtcNow.ToString("dd/MM/yyyy à HH:mm")}</p>
                <p style=""margin: 5px 0;""><strong>Statut :</strong> Expédié</p>
            </div>

            <p>Ce colis est en route ! Vous pouvez suivre votre livraison avec le numéro de suivi ci-dessus.</p>
            <p>Vous recevrez des notifications régulières sur l'avancement de cette livraison.</p>

            <a href=""{_frontendUrl}/orders/{order.Id}"" class=""button"">Suivre mon colis</a>",
            $"Commande #{order.Id}");
    }

    public string GenerateOrderInTransitEmail(Order order, User user, BrandDelivery brandDelivery)
    {
        var brandName = brandDelivery.Brand?.Name ?? "la marque";
        return BuildEmailTemplate(
            user.FirstName,
            $"Colis {brandName} en transit",
            $@"
            <p>Votre colis de <strong>{brandName}</strong> est en route vers sa destination.</p>

            <div style=""background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                <h3 style=""margin-top: 0; color: #000;"">Commande #{order.Id} - {brandName}</h3>
                <p style=""margin: 5px 0;""><strong>Numéro de suivi :</strong> <span style=""font-family: monospace; background-color: #f0f0f0; padding: 2px 8px; border-radius: 3px;"">{brandDelivery.TrackingNumber}</span></p>
                <p style=""margin: 5px 0;""><strong>Statut :</strong> En transit</p>
            </div>

            <p>Ce colis progresse bien et devrait arriver bientôt. Vous recevrez une notification dès qu'il sera en cours de livraison.</p>

            <a href=""{_frontendUrl}/orders/{order.Id}"" class=""button"">Suivre mon colis</a>",
            $"Commande #{order.Id}");
    }

    public string GenerateOrderOutForDeliveryEmail(Order order, User user, BrandDelivery brandDelivery)
    {
        var brandName = brandDelivery.Brand?.Name ?? "la marque";
        return BuildEmailTemplate(
            user.FirstName,
            $"Colis {brandName} arrive aujourd'hui",
            $@"
            <p><strong>Votre colis de {brandName} sera livré aujourd'hui !</strong></p>

            <div style=""background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                <h3 style=""margin-top: 0; color: #000;"">Commande #{order.Id} - {brandName}</h3>
                <p style=""margin: 5px 0;""><strong>Numéro de suivi :</strong> <span style=""font-family: monospace; background-color: #f0f0f0; padding: 2px 8px; border-radius: 3px;"">{brandDelivery.TrackingNumber}</span></p>
                <p style=""margin: 5px 0;""><strong>Statut :</strong> En cours de livraison</p>
            </div>

            <p>Le livreur est en route avec votre colis. Assurez-vous d'être disponible pour réceptionner votre commande.</p>
            <p>Si vous êtes absent, le livreur laissera un avis de passage.</p>

            <a href=""{_frontendUrl}/orders/{order.Id}"" class=""button"">Suivre mon colis</a>",
            $"Commande #{order.Id}");
    }

    public string GenerateOrderDeliveredEmail(Order order, User user, BrandDelivery brandDelivery)
    {
        var brandName = brandDelivery.Brand?.Name ?? "la marque";
        return BuildEmailTemplate(
            user.FirstName,
            $"Colis {brandName} livré",
            $@"
            <p><strong>Votre colis de {brandName} a été livré avec succès !</strong></p>

            <div style=""background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;"">
                <h3 style=""margin-top: 0; color: #000;"">Commande #{order.Id} - {brandName}</h3>
                <p style=""margin: 5px 0;""><strong>Numéro de suivi :</strong> <span style=""font-family: monospace; background-color: #f0f0f0; padding: 2px 8px; border-radius: 3px;"">{brandDelivery.TrackingNumber}</span></p>
                <p style=""margin: 5px 0;""><strong>Date de livraison :</strong> {brandDelivery.DeliveredAt?.ToString("dd/MM/yyyy à HH:mm") ?? DateTime.UtcNow.ToString("dd/MM/yyyy à HH:mm")}</p>
                <p style=""margin: 5px 0;""><strong>Statut :</strong> Livré</p>
            </div>

            <p>Nous espérons que vous êtes satisfait de ce colis !</p>
            <p>N'hésitez pas à nous laisser votre avis sur les produits que vous avez reçus. Cela aide notre communauté à faire les meilleurs choix.</p>

            <div style=""margin-top: 20px;"">
                <a href=""{_frontendUrl}/orders/{order.Id}"" class=""button"">Voir ma commande</a>
            </div>

            <p style=""margin-top: 20px; font-size: 12px; color: #666;"">
                Une question ? Un problème avec votre commande ? Contactez notre service client.
            </p>",
            $"Commande #{order.Id}");
    }

    private static string BuildEmailTemplate(string firstName, string title, string content, string subtitle)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #000; color: #fff; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{
            display: inline-block;
            background-color: #000;
            color: #fff;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 5px;
            margin-top: 20px;
        }}
        .footer {{ text-align: center; padding-top: 20px; font-size: 12px; color: #666; }}
        table {{ margin: 10px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>IndeConnect</h1>
            <p style=""margin: 5px 0; font-size: 14px;"">{subtitle}</p>
        </div>
        <div class=""content"">
            <h2 style=""color: #000; margin-top: 0;"">{title}</h2>
            <p>Bonjour {firstName},</p>
            {content}
        </div>
        <div class=""footer"">
            <p>&copy; 2025 IndeConnect. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";
    }
}
