namespace IndeConnect_Back.Domain.catalog.brand;

public enum BrandStatus
{
    Draft = 0,      // créée par modo, en édition
    Submitted = 1,  // questionnaire soumis, en revue
    Approved = 2,   // publiée
    Rejected = 3,   // renvoyée avec motif
    Disabled = 4    // désactivée
}
