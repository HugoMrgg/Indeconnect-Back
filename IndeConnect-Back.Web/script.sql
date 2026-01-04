-- ============================================
-- SCRIPT MASSIF COMPLET - INDECONNECT V2
-- AVEC ETHICS V2 (ACTIVE + DRAFT) + SIZES PAR CATEGORY
-- ============================================

-- 1. NETTOYER TOUT
DELETE FROM "BrandQuestionResponseOptions";
DELETE FROM "BrandQuestionResponses";
DELETE FROM "BrandQuestionnaires";
DELETE FROM "UserReviews";
DELETE FROM "BrandEthicScores";
DELETE FROM "BrandEthicTags";
DELETE FROM "BrandShippingMethods";
DELETE FROM "Deposits";
DELETE FROM "ProductReviews";
DELETE FROM "ProductMedia";
DELETE FROM "OrderItems";
DELETE FROM "Orders";
DELETE FROM "CartItems";
DELETE FROM "ShippingAddresses";
DELETE FROM "ProductVariants";
DELETE FROM "Products";
DELETE FROM "ProductGroups";
DELETE FROM "Brands";
DELETE FROM "EthicsOptions";
DELETE FROM "EthicsQuestions";
DELETE FROM "CatalogVersions";
DELETE FROM "Keywords";
DELETE FROM "Sizes";
DELETE FROM "Categories";
DELETE FROM "Colors";
DELETE FROM "Users";

-- ============================================
-- 2. UTILISATEURS
-- ============================================

INSERT INTO "Users" ("Id", "Email", "FirstName", "LastName", "CreatedAt", "IsEnabled", "Role", "PasswordHash")
VALUES
    (97, 'lesmurgias@gmail.com', 'Hugo', 'Murgia', NOW(), true, 'Client', '$2a$12$2V8bLTSUcmoq1F8xgtZYJeSz2PO6jFHYVsFKyWUFm4yk.dsun6VtC'),
    (98, 'hugo03.murgia@gmail.com', 'Hugo', 'Murgia', NOW(), true, 'Moderator', '$2a$12$2V8bLTSUcmoq1F8xgtZYJeSz2PO6jFHYVsFKyWUFm4yk.dsun6VtC'),
    (99, 'hg.murgia@gmail.com', 'Hugo', 'Murgia', NOW(), true, 'Administrator', '$2a$12$2V8bLTSUcmoq1F8xgtZYJeSz2PO6jFHYVsFKyWUFm4yk.dsun6VtC');

INSERT INTO "Users" ("Id", "Email", "FirstName", "LastName", "CreatedAt", "IsEnabled", "Role")
VALUES
    (100, 'alice@test.com', 'Alice', 'Martin', NOW(), true, 'Vendor'),
    (101, 'bob@test.com', 'Bob', 'Dupont', NOW(), true, 'Vendor'),
    (102, 'charlie@test.com', 'Charlie', 'Bernard', NOW(), true, 'Vendor'),
    (103, 'diana@test.com', 'Diana', 'Petit', NOW(), true, 'Client'),
    (104, 'emma@test.com', 'Emma', 'Durand', NOW(), true, 'Client'),
    (105, 'frank@test.com', 'Frank', 'Leroy', NOW(), true, 'Client'),
    (106, 'grace@test.com', 'Grace', 'Moreau', NOW(), true, 'SuperVendor'),
    (107, 'henry@test.com', 'Henry', 'Simon', NOW(), true, 'SuperVendor'),
    (108, 'iris@test.com', 'Iris', 'Laurent', NOW(), true, 'SuperVendor'),
    (109, 'jack@test.com', 'Jack', 'Lefebvre', NOW(), true, 'Moderator');

-- ============================================
-- 3. DONNÉES DE RÉFÉRENCE
-- ============================================

-- 3.1 Colors
INSERT INTO "Colors" ("Id", "Name", "Hexa")
VALUES
    (100, 'Red', '#FF0000'), (101, 'Blue', '#0000FF'), (102, 'Black', '#000000'),
    (103, 'White', '#FFFFFF'), (104, 'Green', '#00AA00'), (105, 'Yellow', '#FFFF00'),
    (106, 'Navy', '#000080'), (107, 'Gray', '#808080'), (108, 'Purple', '#800080'),
    (109, 'Orange', '#FFA500');

-- 3.2 Categories
INSERT INTO "Categories" ("Id", "Name")
VALUES
    (100, 'T-Shirts'), (101, 'Jeans'), (102, 'Shoes'), (103, 'Accessories'),
    (104, 'Dresses'), (105, 'Jackets'), (106, 'Hoodies'), (107, 'Pants'),
    (108, 'Skirts'), (109, 'Swimwear');

-- 3.3 Sizes (AVEC CategoryId et SortOrder)
INSERT INTO "Sizes" ("Id", "Name", "CategoryId", "SortOrder")
VALUES
    -- T-Shirts (100)
    (1, 'XS', 100, 1), (2, 'S', 100, 2), (3, 'M', 100, 3),
    (4, 'L', 100, 4), (5, 'XL', 100, 5), (6, 'XXL', 100, 6), (7, 'XXXL', 100, 7),

    -- Hoodies (106)
    (50, 'XS', 106, 1), (51, 'S', 106, 2), (52, 'M', 106, 3),
    (53, 'L', 106, 4), (54, 'XL', 106, 5), (55, 'XXL', 106, 6),

    -- Jackets (105)
    (60, 'XS', 105, 1), (61, 'S', 105, 2), (62, 'M', 105, 3),
    (63, 'L', 105, 4), (64, 'XL', 105, 5), (65, 'XXL', 105, 6),

    -- Dresses (104)
    (70, 'XS', 104, 1), (71, 'S', 104, 2), (72, 'M', 104, 3),
    (73, 'L', 104, 4), (74, 'XL', 104, 5), (75, 'XXL', 104, 6),

    -- Jeans (101)
    (20, '28', 101, 1), (21, '30', 101, 2), (22, '32', 101, 3),
    (23, '34', 101, 4), (24, '36', 101, 5), (25, '38', 101, 6),

    -- Pants (107)
    (80, '28', 107, 1), (81, '30', 107, 2), (82, '32', 107, 3),
    (83, '34', 107, 4), (84, '36', 107, 5), (85, '38', 107, 6),

    -- Shoes (102)
    (10, '36', 102, 1), (11, '37', 102, 2), (12, '38', 102, 3),
    (13, '39', 102, 4), (14, '40', 102, 5), (15, '41', 102, 6),
    (16, '42', 102, 7), (17, '43', 102, 8), (18, '44', 102, 9), (19, '45', 102, 10),

    -- Accessories (103), Skirts (108), Swimwear (109)
    (99, 'Unique', 103, 1), (100, 'Unique', 108, 1), (101, 'Unique', 109, 1);

-- 3.4 Keywords
INSERT INTO "Keywords" ("Id", "Name")
VALUES
    (100, 'eco-friendly'), (101, 'organic'), (102, 'sustainable'), (103, 'ethical'),
    (104, 'fair-trade'), (105, 'vegan'), (106, 'premium'), (107, 'casual'),
    (108, 'elegant'), (109, 'sporty');

-- ============================================
-- 4. ETHICS V2 - CATALOG VERSIONS
-- ============================================

INSERT INTO "CatalogVersions" ("Id", "VersionNumber", "CreatedAt", "PublishedAt", "IsActive", "IsDraft")
VALUES
    (1, 'v1.0', NOW() - INTERVAL '60 days', NOW() - INTERVAL '60 days', true, false),   -- ACTIVE
    (2, 'v2.0-draft', NOW(), NULL, false, true);                                         -- DRAFT

-- ============================================
-- VERSION 1 (ACTIVE) - Questions 100-105
-- ============================================

INSERT INTO "EthicsQuestions" ("Id", "CatalogVersionId", "Category", "Key", "Label", "Order", "AnswerType", "IsActive")
VALUES
    -- MaterialsManufacturing (Category=0)
    (100, 1, 0, 'material_origin', 'Où proviennent vos matériaux ?', 10, 0, true),
    (101, 1, 0, 'manufacturing_conditions', 'Conditions de travail ?', 20, 0, true),
    (102, 1, 0, 'organic_certified', 'Certifiés bio ?', 30, 0, true),

    -- Transport (Category=1)
    (103, 1, 1, 'transport_method', 'Mode de transport ?', 10, 0, true),
    (104, 1, 1, 'carbon_offset', 'Compensation carbone ?', 20, 0, true),
    (105, 1, 1, 'local_production', 'Production locale ?', 30, 0, true);

INSERT INTO "EthicsOptions" ("Id", "QuestionId", "Key", "Label", "Score", "Order", "IsActive")
VALUES
    -- material_origin (QuestionId=100)
    (1000, 100, 'local', 'Local', 100.0, 10, true),
    (1001, 100, 'regional', 'Régional', 75.0, 20, true),
    (1002, 100, 'imported', 'Importé', 40.0, 30, true),
    (1003, 100, 'unknown', 'Inconnu', 0.0, 40, true),

    -- manufacturing_conditions (QuestionId=101)
    (1004, 101, 'excellent', 'Excellent', 100.0, 10, true),
    (1005, 101, 'good', 'Bon', 80.0, 20, true),
    (1006, 101, 'fair', 'Correct', 50.0, 30, true),
    (1007, 101, 'poor', 'Mauvais', 10.0, 40, true),

    -- organic_certified (QuestionId=102)
    (1008, 102, 'fully', 'Entièrement certifié', 100.0, 10, true),
    (1009, 102, 'mostly', 'Majoritaire (80%+)', 70.0, 20, true),
    (1010, 102, 'partial', 'Partiel', 40.0, 30, true),
    (1011, 102, 'none', 'Aucun', 0.0, 40, true),

    -- transport_method (QuestionId=103)
    (1012, 103, 'sea', 'Maritime', 90.0, 10, true),
    (1013, 103, 'train', 'Train', 85.0, 20, true),
    (1014, 103, 'truck', 'Camion', 60.0, 30, true),
    (1015, 103, 'air', 'Avion', 10.0, 40, true),

    -- carbon_offset (QuestionId=104)
    (1016, 104, 'yes_cert', 'Oui certifié', 100.0, 10, true),
    (1017, 104, 'partial_off', 'Partiellement', 60.0, 20, true),
    (1018, 104, 'planned', 'En projet', 20.0, 30, true),
    (1019, 104, 'no', 'Non', 0.0, 40, true),

    -- local_production (QuestionId=105)
    (1020, 105, 'local_100', '100% local', 100.0, 10, true),
    (1021, 105, 'local_70', '70%+ local', 75.0, 20, true),
    (1022, 105, 'partial_local', 'Partiel', 40.0, 30, true),
    (1023, 105, 'no_local', 'Non local', 0.0, 40, true);

-- ============================================
-- VERSION 2 (DRAFT) - Questions 200-205
-- ============================================

INSERT INTO "EthicsQuestions" ("Id", "CatalogVersionId", "Category", "Key", "Label", "Order", "AnswerType", "IsActive")
VALUES
    -- MaterialsManufacturing (Category=0)
    (200, 2, 0, 'material_origin', 'Où proviennent vos matériaux ?', 10, 0, true),
    (201, 2, 0, 'manufacturing_conditions', 'Conditions de travail ?', 20, 0, true),
    (202, 2, 0, 'organic_certified', 'Certifiés bio ?', 30, 0, true),

    -- Transport (Category=1)
    (203, 2, 1, 'transport_method', 'Mode de transport ?', 10, 0, true),
    (204, 2, 1, 'carbon_offset', 'Compensation carbone ?', 20, 0, true),
    (205, 2, 1, 'local_production', 'Production locale ?', 30, 0, true);

INSERT INTO "EthicsOptions" ("Id", "QuestionId", "Key", "Label", "Score", "Order", "IsActive")
VALUES
    -- material_origin (QuestionId=200)
    (2000, 200, 'local', 'Local', 100.0, 10, true),
    (2001, 200, 'regional', 'Régional', 75.0, 20, true),
    (2002, 200, 'imported', 'Importé', 40.0, 30, true),
    (2003, 200, 'unknown', 'Inconnu', 0.0, 40, true),

    -- manufacturing_conditions (QuestionId=201)
    (2004, 201, 'excellent', 'Excellent', 100.0, 10, true),
    (2005, 201, 'good', 'Bon', 80.0, 20, true),
    (2006, 201, 'fair', 'Correct', 50.0, 30, true),
    (2007, 201, 'poor', 'Mauvais', 10.0, 40, true),

    -- organic_certified (QuestionId=202)
    (2008, 202, 'fully', 'Entièrement certifié', 100.0, 10, true),
    (2009, 202, 'mostly', 'Majoritaire (80%+)', 70.0, 20, true),
    (2010, 202, 'partial', 'Partiel', 40.0, 30, true),
    (2011, 202, 'none', 'Aucun', 0.0, 40, true),

    -- transport_method (QuestionId=203)
    (2012, 203, 'sea', 'Maritime', 90.0, 10, true),
    (2013, 203, 'train', 'Train', 85.0, 20, true),
    (2014, 203, 'truck', 'Camion', 60.0, 30, true),
    (2015, 203, 'air', 'Avion', 10.0, 40, true),

    -- carbon_offset (QuestionId=204)
    (2016, 204, 'yes_cert', 'Oui certifié', 100.0, 10, true),
    (2017, 204, 'partial_off', 'Partiellement', 60.0, 20, true),
    (2018, 204, 'planned', 'En projet', 20.0, 30, true),
    (2019, 204, 'no', 'Non', 0.0, 40, true),

    -- local_production (QuestionId=205)
    (2020, 205, 'local_100', '100% local', 100.0, 10, true),
    (2021, 205, 'local_70', '70%+ local', 75.0, 20, true),
    (2022, 205, 'partial_local', 'Partiel', 40.0, 30, true),
    (2023, 205, 'no_local', 'Non local', 0.0, 40, true);

-- ============================================
-- 5. MARQUES
-- ============================================

INSERT INTO "Brands" ("Id", "Name", "LogoUrl", "BannerUrl", "Description", "AboutUs", "WhereAreWe", "Contact", "PriceRange", "Status")
VALUES
    (100, 'EcoWear', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219572/logo1_insma8.jpg', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219594/banner1_r6zngx.png', 'Vêtements éco-responsables', 'Depuis 2015', 'Paris', 'contact@ecowear.fr', '€€', 'Approved'),
    (101, 'NaturalStyle', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219573/logo2_rrsvlr.jpg', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219595/banner2_nz3xqv.png', 'Bio 100%', 'Certifiés', 'Lyon', 'info@natural.fr', '€€€', 'Approved'),
    (102, 'UrbanFit', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219574/logo3_gqaxct.jpg', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219596/banner3_oskzkn.png', 'Streetwear', 'Urbain', 'Marseille', 'hello@urban.fr', '€€', 'Approved'),
    (103, 'LuxeBrand', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219576/logo4_kzpwmj.jpg', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219597/banner4_zsolth.png', 'Luxe durable', 'Premium', 'Milan', 'luxury@luxe.com', '€€€', 'Approved'),
    (104, 'SportWear Pro', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219577/logo5_uatpoc.jpg', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219599/banner5_y3xcpv.png', 'Sport', 'Perf', 'Amsterdam', 'sport@pro.nl', '€€', 'Approved'),
    (105, 'VintageTales', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219578/logo6_nn5h5q.png', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219600/banner6_lzzfoe.png', 'Vintage', 'Rétro', 'Berlin', 'vintage@tales.de', '€', 'Approved'),
    (106, 'EthicalDenim', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219579/logo7_tmcupb.jpg', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219601/banner7_g133qm.png', 'Denim éthique', 'Jean responsable', 'Barcelona', 'denim@ethical.es', '€€', 'Approved'),
    (107, 'MinimalStyle', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219581/logo8_tvb97m.jpg', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219601/banner7_g133qm.png', 'Minimalisme', 'Intemporel', 'Stockholm', 'minimal@style.se', '€€', 'Approved'),
    (108, 'ArtisanCraft', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219581/logo9_fbhq1z.jpg', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219601/banner7_g133qm.png', 'Artisanal', 'Handmade', 'Athens', 'craft@artisan.gr', '€€€', 'Approved'),
    (109, 'EcoKids', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219583/logo10_bmv4db.jpg', 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219601/banner7_g133qm.png', 'Enfants', 'Safe', 'Copenhagen', 'kids@eco.dk', '€€', 'Approved');

-- ============================================
-- 6. SHIPPING ADDRESSES
-- ============================================

INSERT INTO "ShippingAddresses" ("Id", "UserId", "Street", "Number", "PostalCode", "City", "Country", "IsDefault")
VALUES
    (2000, 97, 'Avenue Louise', '100', '1050', 'Bruxelles', 'BE', true),
    (2001, 97, 'Rue Léopold', '25', '4000', 'Liège', 'BE', false),
    (2002, 97, 'Rue de Rivoli', '50', '75001', 'Paris', 'FR', false);

-- ============================================
-- 7. DEPOSITS (BELGIQUE)
-- ============================================

INSERT INTO "Deposits" ("Id", "Number", "Street", "City", "PostalCode", "Country", "Latitude", "Longitude", "BrandId")
VALUES
    ('deposit-eco-1', 42, 'Rue de la Loi', 'Bruxelles', '1000', 'Belgium', 50.8503, 4.3517, 100),
    ('deposit-eco-2', 15, 'Avenue Louise', 'Bruxelles', '1050', 'Belgium', 50.8263, 4.3602, 100),
    ('deposit-nat-1', 88, 'Meir', 'Anvers', '2000', 'Belgium', 51.2194, 4.4025, 101),
    ('deposit-urb-1', 12, 'Veldstraat', 'Gand', '9000', 'Belgium', 51.0543, 3.7174, 102),
    ('deposit-urb-2', 7, 'Korenmarkt', 'Gand', '9000', 'Belgium', 51.0548, 3.7198, 102),
    ('deposit-lux-1', 25, 'Markt', 'Bruges', '8000', 'Belgium', 51.2093, 3.2247, 103),
    ('deposit-spo-1', 33, 'Sauvenière', 'Liège', '4000', 'Belgium', 50.6326, 5.5797, 104),
    ('deposit-spo-2', 18, 'Léopold', 'Liège', '4000', 'Belgium', 50.6413, 5.5734, 104),
    ('deposit-vin-1', 9, 'Montagne', 'Charleroi', '6000', 'Belgium', 50.4108, 4.4446, 105),
    ('deposit-den-1', 56, 'Fer', 'Namur', '5000', 'Belgium', 50.4674, 4.8720, 106),
    ('deposit-min-1', 22, 'Grand Place', 'Mons', '7000', 'Belgium', 50.4542, 3.9522, 107),
    ('deposit-art-1', 14, 'Oude Markt', 'Louvain', '3000', 'Belgium', 50.8798, 4.7005, 108),
    ('deposit-art-2', 3, 'Naamsestraat', 'Louvain', '3000', 'Belgium', 50.8771, 4.7005, 108),
    ('deposit-kid-1', 8, 'Hoogstraat', 'Hasselt', '3500', 'Belgium', 50.9307, 5.3378, 109);

-- ============================================
-- 8. PRODUCT GROUPS
-- ============================================

INSERT INTO "ProductGroups" ("Id", "Name", "BaseDescription", "BrandId", "CategoryId")
VALUES
    (100, 'Organic Cotton Tee', 'T-shirt en coton bio ultra-doux', 100, 100),
    (101, 'Eco Hoodie', 'Hoodie confortable et éco-responsable', 100, 106),
    (102, 'Sustainable Jeans', 'Jean durable et éthique', 101, 101),
    (103, 'Premium Jacket', 'Veste premium haut de gamme', 103, 105),
    (104, 'Sport Leggings', 'Leggings haute performance', 104, 107);

-- ============================================
-- 9. PRODUCTS
-- ============================================

INSERT INTO "Products" ("Id", "Name", "Description", "Price", "IsEnabled", "CreatedAt", "Status", "BrandId", "CategoryId", "ProductGroupId", "PrimaryColorId")
VALUES
    (100, 'Organic Cotton Tee - Red', 'T-shirt coton bio rouge vif', 39.99, true, NOW(), 'Online', 100, 100, 100, 100),
    (101, 'Organic Cotton Tee - Blue', 'T-shirt coton bio bleu océan', 39.99, true, NOW(), 'Online', 100, 100, 100, 101),
    (102, 'Organic Cotton Tee - White', 'T-shirt coton bio blanc pur', 39.99, true, NOW(), 'Online', 100, 100, 100, 103),
    (103, 'Eco Hoodie - Black', 'Hoodie éco noir classique', 59.99, true, NOW(), 'Online', 100, 106, 101, 102),
    (104, 'Eco Hoodie - Gray', 'Hoodie éco gris chiné', 59.99, true, NOW(), 'Online', 100, 106, 101, 107),
    (105, 'Sustainable Jeans - Blue', 'Jean durable bleu denim', 89.99, true, NOW(), 'Online', 101, 101, 102, 101),
    (106, 'Sustainable Jeans - Black', 'Jean durable noir intense', 89.99, true, NOW(), 'Online', 101, 101, 102, 102),
    (107, 'Premium Jacket - Black', 'Veste premium noire élégante', 149.99, true, NOW(), 'Online', 103, 105, 103, 102),
    (108, 'Premium Jacket - Navy', 'Veste premium bleu marine', 149.99, true, NOW(), 'Online', 103, 105, 103, 106),
    (109, 'Premium Jacket - Gray', 'Veste premium grise sophistiquée', 149.99, true, NOW(), 'Online', 103, 105, 103, 107),
    (110, 'Sport Leggings - Black', 'Leggings sport noir technique', 44.99, true, NOW(), 'Online', 104, 107, 104, 102),
    (111, 'Sport Leggings - Purple', 'Leggings sport violet dynamique', 44.99, true, NOW(), 'Online', 104, 107, 104, 108),
    (112, 'Vintage Dress', 'Robe vintage unique', 54.99, true, NOW(), 'Online', 105, 104, NULL, NULL),
    (113, 'Ethical Denim Blue', 'Jean éthique bleu', 94.99, true, NOW(), 'Online', 106, 101, NULL, 101),
    (114, 'Minimal Tee White', 'T-shirt minimal blanc', 34.99, true, NOW(), 'Online', 107, 100, NULL, 103),
    (115, 'Artisan Bag Brown', 'Sac artisanal marron', 79.99, true, NOW(), 'Online', 108, 103, NULL, NULL),
    (116, 'Kids Organic Tee', 'T-shirt enfant bio', 24.99, true, NOW(), 'Online', 109, 100, NULL, NULL);

-- ============================================
-- 10. PRODUCT MEDIA
-- ============================================

INSERT INTO "ProductMedia" ("Id", "ProductId", "Url", "Type", "DisplayOrder", "IsPrimary")
VALUES
    (100, 100, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219587/tee-red-1_ty3s4c.jpg', 'Image', 1, true),
    (101, 100, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219589/tee-red-2_ddqvj6.jpg', 'Image', 2, false),
    (102, 101, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219585/tee-blue-1_wp9kx0.jpg', 'Image', 1, true),
    (103, 101, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219587/tee-blue-2_mdhtta.jpg', 'Image', 2, false),
    (104, 102, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219590/tee-white-1_vaowyq.jpg', 'Image', 1, true),
    (105, 103, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219559/hoodie-black-1_wahcdy.jpg', 'Image', 1, true),
    (106, 103, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219561/hoodie-black-2_lcuov4.jpg', 'Image', 2, false),
    (107, 104, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219562/hoodie-grey-1_jhovpf.jpg', 'Image', 1, true),
    (108, 105, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219558/denim-blue-1_dcodgv.jpg', 'Image', 1, true),
    (109, 106, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219566/jeans-black-1_m3hnra.jpg', 'Image', 1, true),
    (110, 107, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219563/jacket-black-1_e2oetx.jpg', 'Image', 1, true),
    (111, 108, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219565/jacket-navy-1_fcm5yv.jpg', 'Image', 1, true),
    (112, 109, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219564/jacket-grey-1_skcskk.jpg', 'Image', 1, true),
    (113, 110, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219570/leggings-black-1_oqdhm2.jpg', 'Image', 1, true),
    (114, 111, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219571/leggings-purple-1_dpg97y.jpg', 'Image', 1, true),
    (115, 112, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219558/dress-vintage-1_lae3x2.jpg', 'Image', 1, true),
    (116, 113, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219569/denim-blue-1_dcodgv.jpg', 'Image', 1, true),
    (117, 114, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219591/tee-white-2_emlexh.jpg', 'Image', 1, true),
    (118, 115, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219592/bag-brown-1_yawuge.jpg', 'Image', 1, true),
    (119, 116, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219569/kids-tee-1_rmwbfh.jpg', 'Image', 1, true);

-- ============================================
-- 11. PRODUCT VARIANTS
-- ============================================

INSERT INTO "ProductVariants" ("Id", "ProductId", "SizeId", "SKU", "StockCount")
VALUES
    -- T-Shirts (catégorie 100, tailles Id 1-7)
    (100, 100, 2, 'ECO-TEE-RED-S', 20), (101, 100, 3, 'ECO-TEE-RED-M', 50), (102, 100, 4, 'ECO-TEE-RED-L', 30),
    (103, 101, 2, 'ECO-TEE-BLUE-S', 25), (104, 101, 3, 'ECO-TEE-BLUE-M', 60), (105, 101, 4, 'ECO-TEE-BLUE-L', 35),
    (106, 102, 2, 'ECO-TEE-WHITE-S', 15), (107, 102, 3, 'ECO-TEE-WHITE-M', 40), (108, 102, 4, 'ECO-TEE-WHITE-L', 25),
    (142, 114, 3, 'MIN-TEE-M', 30), (144, 116, 2, 'KIDS-TEE-S', 40),

    -- Hoodies (catégorie 106, tailles Id 50-55)
    (109, 103, 51, 'ECO-HOOD-BLACK-S', 10), (110, 103, 52, 'ECO-HOOD-BLACK-M', 30), (111, 103, 53, 'ECO-HOOD-BLACK-L', 25), (112, 103, 54, 'ECO-HOOD-BLACK-XL', 15),
    (113, 104, 51, 'ECO-HOOD-GRAY-S', 12), (114, 104, 52, 'ECO-HOOD-GRAY-M', 28), (115, 104, 53, 'ECO-HOOD-GRAY-L', 20), (116, 104, 54, 'ECO-HOOD-GRAY-XL', 10),

    -- Jeans (catégorie 101, tailles Id 20-25)
    (117, 105, 20, 'SUST-JEAN-BLUE-28', 15), (118, 105, 21, 'SUST-JEAN-BLUE-30', 40), (119, 105, 22, 'SUST-JEAN-BLUE-32', 35), (120, 105, 23, 'SUST-JEAN-BLUE-34', 25),
    (121, 106, 20, 'SUST-JEAN-BLACK-28', 10), (122, 106, 21, 'SUST-JEAN-BLACK-30', 30), (123, 106, 22, 'SUST-JEAN-BLACK-32', 28), (124, 106, 23, 'SUST-JEAN-BLACK-34', 20),
    (141, 113, 21, 'ETH-DENIM-30', 20),

    -- Jackets (catégorie 105, tailles Id 60-65)
    (125, 107, 62, 'PREM-JACK-BLACK-M', 15), (126, 107, 63, 'PREM-JACK-BLACK-L', 20), (127, 107, 64, 'PREM-JACK-BLACK-XL', 10),
    (128, 108, 62, 'PREM-JACK-NAVY-M', 12), (129, 108, 63, 'PREM-JACK-NAVY-L', 18), (130, 108, 64, 'PREM-JACK-NAVY-XL', 8),
    (131, 109, 62, 'PREM-JACK-GRAY-M', 10), (132, 109, 63, 'PREM-JACK-GRAY-L', 15), (133, 109, 64, 'PREM-JACK-GRAY-XL', 5),

    -- Pants/Leggings (catégorie 107, tailles Id 80-85)
    (134, 110, 81, 'SPORT-LEG-BLACK-S', 30), (135, 110, 82, 'SPORT-LEG-BLACK-M', 45), (136, 110, 83, 'SPORT-LEG-BLACK-L', 25),
    (137, 111, 81, 'SPORT-LEG-PURPLE-S', 25), (138, 111, 82, 'SPORT-LEG-PURPLE-M', 40), (139, 111, 83, 'SPORT-LEG-PURPLE-L', 20),

    -- Dresses (catégorie 104, tailles Id 70-75)
    (140, 112, 72, 'VINT-DRESS-M', 10),

    -- Accessories (catégorie 103, taille Id 99)
    (143, 115, 99, 'ART-BAG-ONESIZE', 15);

-- ============================================
-- 12. BRAND QUESTIONNAIRES (V1 ACTIVE)
-- ============================================

INSERT INTO "BrandQuestionnaires"
("Id", "BrandId", "CatalogVersionId", "Status", "NeedsUpdate",
 "CreatedAt", "SubmittedAt", "ReviewedAt")
VALUES
    (100, 100, 1, 3, false, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (101, 101, 1, 3, false, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (102, 102, 1, 3, false, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (103, 103, 1, 3, false, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (104, 104, 1, 3, false, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (105, 105, 1, 3, false, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (106, 106, 1, 3, false, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (107, 107, 1, 3, false, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (108, 108, 1, 3, false, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (109, 109, 1, 3, false, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days');

INSERT INTO "BrandQuestionResponses" ("Id", "QuestionnaireId", "QuestionId", "QuestionKey", "CalculatedScore")
VALUES
    -- Questionnaire 100 (EcoWear) - Questions 100-105
    (1000, 100, 100, 'material_origin', 100.0), (1001, 100, 101, 'manufacturing_conditions', 100.0),
    (1002, 100, 102, 'organic_certified', 100.0), (1003, 100, 103, 'transport_method', 90.0),
    (1004, 100, 104, 'carbon_offset', 100.0), (1005, 100, 105, 'local_production', 100.0),

    -- Questionnaire 101 (NaturalStyle)
    (1006, 101, 100, 'material_origin', 75.0), (1007, 101, 101, 'manufacturing_conditions', 80.0),
    (1008, 101, 102, 'organic_certified', 70.0), (1009, 101, 103, 'transport_method', 85.0),
    (1010, 101, 104, 'carbon_offset', 60.0), (1011, 101, 105, 'local_production', 75.0),

    -- Questionnaire 102 (UrbanFit)
    (1012, 102, 100, 'material_origin', 40.0), (1013, 102, 101, 'manufacturing_conditions', 50.0),
    (1014, 102, 102, 'organic_certified', 40.0), (1015, 102, 103, 'transport_method', 60.0),
    (1016, 102, 104, 'carbon_offset', 20.0), (1017, 102, 105, 'local_production', 40.0),

    -- Questionnaire 103 (LuxeBrand)
    (1018, 103, 100, 'material_origin', 100.0), (1019, 103, 101, 'manufacturing_conditions', 100.0),
    (1020, 103, 102, 'organic_certified', 70.0), (1021, 103, 103, 'transport_method', 90.0),
    (1022, 103, 104, 'carbon_offset', 100.0), (1023, 103, 105, 'local_production', 75.0),

    -- Questionnaire 104 (SportWear Pro)
    (1024, 104, 100, 'material_origin', 75.0), (1025, 104, 101, 'manufacturing_conditions', 80.0),
    (1026, 104, 102, 'organic_certified', 40.0), (1027, 104, 103, 'transport_method', 60.0),
    (1028, 104, 104, 'carbon_offset', 60.0), (1029, 104, 105, 'local_production', 40.0),

    -- Questionnaire 105 (VintageTales)
    (1030, 105, 100, 'material_origin', 40.0), (1031, 105, 101, 'manufacturing_conditions', 50.0),
    (1032, 105, 102, 'organic_certified', 0.0), (1033, 105, 103, 'transport_method', 10.0),
    (1034, 105, 104, 'carbon_offset', 0.0), (1035, 105, 105, 'local_production', 0.0),

    -- Questionnaire 106 (EthicalDenim)
    (1036, 106, 100, 'material_origin', 100.0), (1037, 106, 101, 'manufacturing_conditions', 100.0),
    (1038, 106, 102, 'organic_certified', 100.0), (1039, 106, 103, 'transport_method', 85.0),
    (1040, 106, 104, 'carbon_offset', 100.0), (1041, 106, 105, 'local_production', 100.0),

    -- Questionnaire 107 (MinimalStyle)
    (1042, 107, 100, 'material_origin', 75.0), (1043, 107, 101, 'manufacturing_conditions', 50.0),
    (1044, 107, 102, 'organic_certified', 40.0), (1045, 107, 103, 'transport_method', 10.0),
    (1046, 107, 104, 'carbon_offset', 20.0), (1047, 107, 105, 'local_production', 40.0),

    -- Questionnaire 108 (ArtisanCraft)
    (1048, 108, 100, 'material_origin', 100.0), (1049, 108, 101, 'manufacturing_conditions', 100.0),
    (1050, 108, 102, 'organic_certified', 100.0), (1051, 108, 103, 'transport_method', 90.0),
    (1052, 108, 104, 'carbon_offset', 100.0), (1053, 108, 105, 'local_production', 100.0),

    -- Questionnaire 109 (EcoKids)
    (1054, 109, 100, 'material_origin', 75.0), (1055, 109, 101, 'manufacturing_conditions', 50.0),
    (1056, 109, 102, 'organic_certified', 70.0), (1057, 109, 103, 'transport_method', 85.0),
    (1058, 109, 104, 'carbon_offset', 60.0), (1059, 109, 105, 'local_production', 75.0);

INSERT INTO "BrandQuestionResponseOptions" ("ResponseId", "OptionId")
VALUES
    (1000, 1000), (1001, 1004), (1002, 1008), (1003, 1012), (1004, 1016), (1005, 1020),
    (1006, 1001), (1007, 1005), (1008, 1009), (1009, 1013), (1010, 1017), (1011, 1021),
    (1012, 1002), (1013, 1006), (1014, 1010), (1015, 1014), (1016, 1018), (1017, 1022),
    (1018, 1000), (1019, 1004), (1020, 1009), (1021, 1012), (1022, 1016), (1023, 1021),
    (1024, 1001), (1025, 1005), (1026, 1010), (1027, 1014), (1028, 1017), (1029, 1022),
    (1030, 1002), (1031, 1007), (1032, 1011), (1033, 1015), (1034, 1019), (1035, 1023),
    (1036, 1000), (1037, 1004), (1038, 1008), (1039, 1013), (1040, 1016), (1041, 1020),
    (1042, 1001), (1043, 1006), (1044, 1010), (1045, 1015), (1046, 1018), (1047, 1022),
    (1048, 1000), (1049, 1004), (1050, 1008), (1051, 1012), (1052, 1016), (1053, 1020),
    (1054, 1001), (1055, 1006), (1056, 1009), (1057, 1013), (1058, 1017), (1059, 1021);

INSERT INTO "BrandEthicScores" ("BrandId", "Category", "QuestionnaireId", "RawScore", "FinalScore", "IsOfficial", "CreatedAt")
VALUES
    (100, 0, 100, 100.0, 100.0, true, NOW() - INTERVAL '20 days'),
    (100, 1, 100, 96.67, 96.67, true, NOW() - INTERVAL '20 days'),
    (101, 0, 101, 75.0, 75.0, true, NOW() - INTERVAL '20 days'),
    (101, 1, 101, 73.33, 73.33, true, NOW() - INTERVAL '20 days'),
    (102, 0, 102, 43.33, 43.33, true, NOW() - INTERVAL '20 days'),
    (102, 1, 102, 40.0, 40.0, true, NOW() - INTERVAL '20 days'),
    (103, 0, 103, 90.0, 90.0, true, NOW() - INTERVAL '20 days'),
    (103, 1, 103, 88.33, 88.33, true, NOW() - INTERVAL '20 days'),
    (104, 0, 104, 65.0, 65.0, true, NOW() - INTERVAL '20 days'),
    (104, 1, 104, 53.33, 53.33, true, NOW() - INTERVAL '20 days'),
    (105, 0, 105, 30.0, 30.0, true, NOW() - INTERVAL '20 days'),
    (105, 1, 105, 3.33, 3.33, true, NOW() - INTERVAL '20 days'),
    (106, 0, 106, 100.0, 100.0, true, NOW() - INTERVAL '20 days'),
    (106, 1, 106, 95.0, 95.0, true, NOW() - INTERVAL '20 days'),
    (107, 0, 107, 55.0, 55.0, true, NOW() - INTERVAL '20 days'),
    (107, 1, 107, 23.33, 23.33, true, NOW() - INTERVAL '20 days'),
    (108, 0, 108, 100.0, 100.0, true, NOW() - INTERVAL '20 days'),
    (108, 1, 108, 96.67, 96.67, true, NOW() - INTERVAL '20 days'),
    (109, 0, 109, 65.0, 65.0, true, NOW() - INTERVAL '20 days'),
    (109, 1, 109, 73.33, 73.33, true, NOW() - INTERVAL '20 days');

-- ============================================
-- 13. USER REVIEWS
-- ============================================

INSERT INTO "UserReviews" ("UserId", "BrandId", "Rating", "Comment", "CreatedAt")
VALUES
    (100, 100, 5, 'Excellent quality and truly eco-friendly!', NOW() - INTERVAL '10 days'),
    (101, 100, 5, 'Love their sustainable approach', NOW() - INTERVAL '8 days'),
    (102, 100, 4, 'Great products, a bit pricey', NOW() - INTERVAL '5 days'),
    (103, 100, 5, 'Best eco brand I have found', NOW() - INTERVAL '3 days'),
    (104, 100, 4, 'Good quality, fast shipping', NOW() - INTERVAL '1 day'),
    (100, 101, 5, 'Amazing organic materials', NOW() - INTERVAL '12 days'),
    (105, 101, 4, 'Very comfortable clothing', NOW() - INTERVAL '9 days'),
    (106, 101, 4, 'Good but expensive', NOW() - INTERVAL '6 days'),
    (107, 101, 4, 'Highly recommend', NOW() - INTERVAL '2 days'),
    (101, 102, 4, 'Cool urban style', NOW() - INTERVAL '11 days'),
    (102, 102, 3, 'Average quality', NOW() - INTERVAL '7 days'),
    (103, 102, 4, 'Nice designs', NOW() - INTERVAL '4 days'),
    (104, 102, 3, 'Could be better', NOW() - INTERVAL '2 days'),
    (105, 103, 5, 'Luxury and sustainable!', NOW() - INTERVAL '15 days'),
    (106, 103, 5, 'Worth every penny', NOW() - INTERVAL '10 days'),
    (107, 103, 5, 'Exceptional quality', NOW() - INTERVAL '6 days'),
    (108, 103, 4, 'Excellent but pricey', NOW() - INTERVAL '3 days'),
    (109, 103, 5, 'Best brand ever!', NOW() - INTERVAL '1 day'),
    (100, 104, 4, 'Good for sports', NOW() - INTERVAL '9 days'),
    (101, 104, 4, 'Comfortable and durable', NOW() - INTERVAL '6 days'),
    (102, 104, 4, 'Great performance', NOW() - INTERVAL '3 days'),
    (103, 105, 3, 'Vintage look but poor quality', NOW() - INTERVAL '8 days'),
    (104, 105, 3, 'Not what I expected', NOW() - INTERVAL '5 days'),
    (105, 105, 2, 'Disappointed', NOW() - INTERVAL '2 days'),
    (106, 106, 5, 'Perfect jeans!', NOW() - INTERVAL '12 days'),
    (107, 106, 5, 'Best denim brand', NOW() - INTERVAL '8 days'),
    (108, 106, 5, 'Ethical and stylish', NOW() - INTERVAL '4 days'),
    (109, 106, 4, 'Great quality', NOW() - INTERVAL '1 day'),
    (100, 107, 4, 'Clean minimal design', NOW() - INTERVAL '7 days'),
    (101, 107, 4, 'Simple and elegant', NOW() - INTERVAL '4 days'),
    (102, 107, 3, 'Too plain for me', NOW() - INTERVAL '2 days'),
    (103, 108, 5, 'Beautiful handmade pieces', NOW() - INTERVAL '14 days'),
    (104, 108, 5, 'True artisan quality', NOW() - INTERVAL '9 days'),
    (105, 108, 4, 'Unique designs', NOW() - INTERVAL '5 days'),
    (106, 108, 5, 'Worth the price', NOW() - INTERVAL '2 days'),
    (107, 109, 5, 'Safe for my kids!', NOW() - INTERVAL '11 days'),
    (108, 109, 4, 'Good quality children wear', NOW() - INTERVAL '7 days'),
    (109, 109, 4, 'Eco-friendly and durable', NOW() - INTERVAL '3 days');

-- ============================================
-- 14. BRAND ETHIC TAGS
-- ============================================

INSERT INTO "BrandEthicTags" ("BrandId", "Category", "TagKey")
VALUES
    (100, 'MaterialsManufacturing', 'local'), (100, 'MaterialsManufacturing', 'organic'),
    (100, 'MaterialsManufacturing', 'fair-trade'), (100, 'MaterialsManufacturing', 'eco-friendly'),
    (101, 'MaterialsManufacturing', 'organic'), (101, 'MaterialsManufacturing', 'sustainable'),
    (101, 'MaterialsManufacturing', 'eco-friendly'),
    (102, 'MaterialsManufacturing', 'casual'), (102, 'MaterialsManufacturing', 'vegan'),
    (103, 'MaterialsManufacturing', 'premium'), (103, 'MaterialsManufacturing', 'local'),
    (103, 'MaterialsManufacturing', 'organic'), (103, 'MaterialsManufacturing', 'sustainable'),
    (104, 'MaterialsManufacturing', 'sporty'), (104, 'MaterialsManufacturing', 'sustainable'),
    (104, 'MaterialsManufacturing', 'eco-friendly'),
    (105, 'MaterialsManufacturing', 'vintage'),
    (106, 'MaterialsManufacturing', 'ethical'), (106, 'MaterialsManufacturing', 'fair-trade'),
    (106, 'MaterialsManufacturing', 'local'), (106, 'MaterialsManufacturing', 'organic'),
    (107, 'MaterialsManufacturing', 'sustainable'), (107, 'MaterialsManufacturing', 'elegant'),
    (108, 'MaterialsManufacturing', 'premium'), (108, 'MaterialsManufacturing', 'local'),
    (108, 'MaterialsManufacturing', 'ethical'),
    (109, 'MaterialsManufacturing', 'organic'), (109, 'MaterialsManufacturing', 'eco-friendly'),
    (109, 'MaterialsManufacturing', 'sustainable'),
    (100, 'Transport', 'carbon-offset'), (100, 'Transport', 'local-production'),
    (101, 'Transport', 'local-production'),
    (103, 'Transport', 'carbon-offset'),
    (106, 'Transport', 'local-production'),
    (108, 'Transport', 'local-production');

-- ============================================
-- 15. BRAND SHIPPING METHODS
-- ============================================

INSERT INTO "BrandShippingMethods" ("Id", "BrandId", "ProviderName", "MethodType", "DisplayName", "Price", "EstimatedMinDays", "EstimatedMaxDays", "IsEnabled")
VALUES
    (100, 100, 'BPost', 'HomeDelivery', 'BPost - Livraison à domicile', 0.00, 3, 5, true),
    (101, 100, 'BPost', 'PickupPoint', 'BPost - Point relais', 0.00, 2, 4, true),
    (102, 100, 'Colruyt', 'StorePickup', 'Retrait en magasin Colruyt', 0.00, 1, 2, true),
    (103, 101, 'DHL', 'HomeDelivery', 'DHL - Livraison express', 7.50, 1, 2, true),
    (104, 101, 'BPost', 'Locker', 'BPost - Casier automatique', 3.99, 2, 3, true),
    (105, 101, 'NaturalStyle', 'StorePickup', 'Retrait en boutique NaturalStyle', 0.00, 1, 1, true),
    (106, 102, 'BPost', 'HomeDelivery', 'BPost - Livraison standard', 4.99, 3, 5, true),
    (107, 102, 'UPS', 'HomeDelivery', 'UPS - Livraison rapide', 9.99, 1, 3, true),
    (108, 102, 'UrbanFit', 'StorePickup', 'Retrait en magasin UrbanFit', 0.00, 1, 2, true),
    (109, 103, 'DHL Premium', 'HomeDelivery', 'DHL Premium - Livraison VIP', 15.00, 1, 2, true),
    (110, 103, 'LuxeBrand', 'StorePickup', 'Retrait en boutique Luxe', 0.00, 1, 1, true),
    (111, 104, 'BPost', 'HomeDelivery', 'BPost - Livraison standard', 0.00, 3, 5, true),
    (112, 104, 'DPD', 'HomeDelivery', 'DPD - Livraison express', 6.99, 1, 2, true),
    (113, 104, 'BPost', 'Locker', 'BPost - Point relais', 2.99, 2, 4, true),
    (114, 105, 'BPost', 'HomeDelivery', 'BPost - Livraison standard', 5.99, 4, 6, true),
    (115, 105, 'VintageTales', 'StorePickup', 'Retrait en boutique Vintage', 0.00, 1, 3, true),
    (116, 106, 'BPost', 'HomeDelivery', 'BPost - Livraison à domicile', 0.00, 3, 5, true),
    (117, 106, 'BPost', 'PickupPoint', 'BPost - Point de collecte', 0.00, 2, 4, true),
    (118, 106, 'DHL', 'HomeDelivery', 'DHL - Livraison express', 8.99, 1, 2, true),
    (119, 107, 'BPost', 'HomeDelivery', 'BPost - Livraison standard', 4.50, 3, 5, true),
    (120, 107, 'MinimalStyle', 'StorePickup', 'Retrait en boutique Minimal', 0.00, 1, 2, true),
    (121, 108, 'BPost', 'HomeDelivery', 'BPost - Livraison soignée', 6.99, 4, 6, true),
    (122, 108, 'ArtisanCraft', 'StorePickup', 'Retrait atelier Artisan', 0.00, 1, 1, true),
    (123, 109, 'BPost', 'HomeDelivery', 'BPost - Livraison à domicile', 0.00, 3, 5, true),
    (124, 109, 'BPost', 'Locker', 'BPost - Casier automatique', 3.50, 2, 4, true),
    (125, 109, 'EcoKids', 'StorePickup', 'Retrait en magasin EcoKids', 0.00, 1, 2, true),
    (126, 100, 'ProutLand Express', 'HomeDelivery', 'ProutLand Express - Livraison éclair ⚡', 12.99, 1, 1, true);

-- ============================================
-- RÉSUMÉ FINAL
-- ============================================-- ============================================
-- -- TRANSLATIONS (FR / EN / NL / DE)
-- -- ============================================
-- 
-- DELETE FROM "brand_translations";
-- DELETE FROM "product_translations";
-- DELETE FROM "category_translations";
-- DELETE FROM "color_translations";
-- DELETE FROM "size_translations";
-- 
-- -- ============================================
-- -- CATEGORIES
-- -- ============================================



INSERT INTO "category_translations" ("CategoryId", "LanguageCode", "Name") VALUES
-- 100 T-Shirts
(100, 'en', 'T-Shirts'),
(100, 'fr', 'T-shirts'),
(100, 'nl', 'T-shirts'),
(100, 'de', 'T-Shirts'),

-- 101 Jeans
(101, 'en', 'Jeans'),
(101, 'fr', 'Jeans'),
(101, 'nl', 'Jeans'),
(101, 'de', 'Jeans'),

-- 102 Shoes
(102, 'en', 'Shoes'),
(102, 'fr', 'Chaussures'),
(102, 'nl', 'Schoenen'),
(102, 'de', 'Schuhe'),

-- 103 Accessories
(103, 'en', 'Accessories'),
(103, 'fr', 'Accessoires'),
(103, 'nl', 'Accessoires'),
(103, 'de', 'Accessoires'),

-- 104 Dresses
(104, 'en', 'Dresses'),
(104, 'fr', 'Robes'),
(104, 'nl', 'Jurken'),
(104, 'de', 'Kleider'),

-- 105 Jackets
(105, 'en', 'Jackets'),
(105, 'fr', 'Vestes'),
(105, 'nl', 'Jassen'),
(105, 'de', 'Jacken'),

-- 106 Hoodies
(106, 'en', 'Hoodies'),
(106, 'fr', 'Sweats à capuche'),
(106, 'nl', 'Hoodies'),
(106, 'de', 'Kapuzenpullover'),

-- 107 Pants
(107, 'en', 'Pants'),
(107, 'fr', 'Pantalons'),
(107, 'nl', 'Broeken'),
(107, 'de', 'Hosen'),

-- 108 Skirts
(108, 'en', 'Skirts'),
(108, 'fr', 'Jupes'),
(108, 'nl', 'Rokken'),
(108, 'de', 'Röcke'),

-- 109 Swimwear
(109, 'en', 'Swimwear'),
(109, 'fr', 'Maillots de bain'),
(109, 'nl', 'Zwemkleding'),
(109, 'de', 'Bademode');

-- ============================================
-- COLORS
-- ============================================

INSERT INTO "color_translations" ("ColorId", "LanguageCode", "Name") VALUES
                                                                         (100, 'en', 'Red'),     (100, 'fr', 'Rouge'),        (100, 'nl', 'Rood'),        (100, 'de', 'Rot'),
                                                                         (101, 'en', 'Blue'),    (101, 'fr', 'Bleu'),         (101, 'nl', 'Blauw'),       (101, 'de', 'Blau'),
                                                                         (102, 'en', 'Black'),   (102, 'fr', 'Noir'),         (102, 'nl', 'Zwart'),       (102, 'de', 'Schwarz'),
                                                                         (103, 'en', 'White'),   (103, 'fr', 'Blanc'),        (103, 'nl', 'Wit'),         (103, 'de', 'Weiß'),
                                                                         (104, 'en', 'Green'),   (104, 'fr', 'Vert'),         (104, 'nl', 'Groen'),       (104, 'de', 'Grün'),
                                                                         (105, 'en', 'Yellow'),  (105, 'fr', 'Jaune'),        (105, 'nl', 'Geel'),        (105, 'de', 'Gelb'),
                                                                         (106, 'en', 'Navy'),    (106, 'fr', 'Bleu marine'),  (106, 'nl', 'Marineblauw'), (106, 'de', 'Navy'),
                                                                         (107, 'en', 'Gray'),    (107, 'fr', 'Gris'),         (107, 'nl', 'Grijs'),       (107, 'de', 'Grau'),
                                                                         (108, 'en', 'Purple'),  (108, 'fr', 'Violet'),       (108, 'nl', 'Paars'),       (108, 'de', 'Lila'),
                                                                         (109, 'en', 'Orange'),  (109, 'fr', 'Orange'),       (109, 'nl', 'Oranje'),      (109, 'de', 'Orange');

-- ============================================
-- SIZES (4 langues)
-- Note: tailles = identiques dans toutes les langues
-- ============================================

INSERT INTO "size_translations" ("SizeId", "LanguageCode", "Name")
SELECT "Id", 'en', "Name" FROM "Sizes"
UNION ALL
SELECT "Id", 'fr', "Name" FROM "Sizes"
UNION ALL
SELECT "Id", 'nl', "Name" FROM "Sizes"
UNION ALL
SELECT "Id", 'de', "Name" FROM "Sizes";

-- ============================================
-- BRANDS (4 langues)
-- ============================================

INSERT INTO "brand_translations"
("BrandId", "LanguageCode", "Name", "Description", "AboutUs", "WhereAreWe", "OtherInfo")
VALUES
-- 100 EcoWear
(100, 'en', 'EcoWear', 'Eco-responsible clothing', 'Founded in 2015 with sustainability at heart', 'Paris, France', 'Certified organic materials'),
(100, 'fr', 'EcoWear', 'Vêtements éco-responsables', 'Fondée en 2015 avec une mission durable', 'Paris, France', 'Matériaux certifiés bio'),
(100, 'nl', 'EcoWear', 'Duurzame kleding', 'Opgericht in 2015 met duurzaamheid als kern', 'Parijs, Frankrijk', 'Gecertificeerde biologische materialen'),
(100, 'de', 'EcoWear', 'Nachhaltige Kleidung', 'Gegründet 2015 mit Nachhaltigkeit im Fokus', 'Paris, Frankreich', 'Zertifizierte Bio-Materialien'),

-- 101 NaturalStyle
(101, 'en', 'NaturalStyle', '100% organic clothing', 'Fully certified organic brand', 'Lyon, France', 'Low carbon footprint'),
(101, 'fr', 'NaturalStyle', 'Vêtements 100% bio', 'Marque certifiée biologique', 'Lyon, France', 'Faible empreinte carbone'),
(101, 'nl', 'NaturalStyle', '100% biologische kleding', 'Volledig biologisch gecertificeerd merk', 'Lyon, Frankrijk', 'Lage CO₂-voetafdruk'),
(101, 'de', 'NaturalStyle', '100% Bio-Kleidung', 'Vollständig biozertifizierte Marke', 'Lyon, Frankreich', 'Geringer CO₂-Fußabdruck'),

-- 102 UrbanFit
(102, 'en', 'UrbanFit', 'Streetwear with an urban edge', 'Designed for everyday city life', 'Marseille, France', 'Modern cuts and durable fabrics'),
(102, 'fr', 'UrbanFit', 'Streetwear urbain', 'Pensé pour la vie de tous les jours', 'Marseille, France', 'Coupes modernes et tissus résistants'),
(102, 'nl', 'UrbanFit', 'Urban streetwear', 'Ontworpen voor het dagelijkse stadsleven', 'Marseille, Frankrijk', 'Moderne pasvormen en stevige stoffen'),
(102, 'de', 'UrbanFit', 'Urbaner Streetwear-Style', 'Gemacht für den Alltag in der Stadt', 'Marseille, Frankreich', 'Moderne Schnitte und robuste Stoffe'),

-- 103 LuxeBrand
(103, 'en', 'LuxeBrand', 'Sustainable luxury fashion', 'Premium quality with responsible choices', 'Milan, Italy', 'High-end materials and craftsmanship'),
(103, 'fr', 'LuxeBrand', 'Luxe durable', 'Qualité premium et choix responsables', 'Milan, Italie', 'Matériaux haut de gamme et savoir-faire'),
(103, 'nl', 'LuxeBrand', 'Duurzame luxe', 'Premium kwaliteit met verantwoorde keuzes', 'Milaan, Italië', 'Hoogwaardige materialen en vakmanschap'),
(103, 'de', 'LuxeBrand', 'Nachhaltiger Luxus', 'Premiumqualität mit verantwortungsvollen Entscheidungen', 'Mailand, Italien', 'Hochwertige Materialien und Handwerk'),

-- 104 SportWear Pro
(104, 'en', 'SportWear Pro', 'Performance sportswear', 'Built for comfort and movement', 'Amsterdam, Netherlands', 'Technical fabrics and durable design'),
(104, 'fr', 'SportWear Pro', 'Vêtements de sport', 'Conçu pour la performance', 'Amsterdam, Pays-Bas', 'Textiles techniques et durables'),
(104, 'nl', 'SportWear Pro', 'Sportkleding voor prestaties', 'Gemaakt voor comfort en beweging', 'Amsterdam, Nederland', 'Technische stoffen en duurzaam ontwerp'),
(104, 'de', 'SportWear Pro', 'Sportbekleidung für Leistung', 'Für Komfort und Bewegung entwickelt', 'Amsterdam, Niederlande', 'Technische Stoffe und langlebiges Design'),

-- 105 VintageTales
(105, 'en', 'VintageTales', 'Vintage-inspired pieces', 'Retro style with a unique touch', 'Berlin, Germany', 'Limited runs and timeless aesthetics'),
(105, 'fr', 'VintageTales', 'Style vintage', 'Rétro et unique', 'Berlin, Allemagne', 'Séries limitées et esthétique intemporelle'),
(105, 'nl', 'VintageTales', 'Vintage-stijl', 'Retro met een unieke touch', 'Berlijn, Duitsland', 'Beperkte oplages en tijdloze look'),
(105, 'de', 'VintageTales', 'Vintage-Stil', 'Retro mit einzigartigem Flair', 'Berlin, Deutschland', 'Limitierte Stückzahlen und zeitlose Ästhetik'),

-- 106 EthicalDenim
(106, 'en', 'EthicalDenim', 'Ethical denim essentials', 'Responsible jeans, made to last', 'Barcelona, Spain', 'Fair practices and durable denim'),
(106, 'fr', 'EthicalDenim', 'Denim éthique', 'Jeans responsables, faits pour durer', 'Barcelone, Espagne', 'Pratiques équitables et denim robuste'),
(106, 'nl', 'EthicalDenim', 'Ethische denim', 'Verantwoorde jeans, gemaakt om lang mee te gaan', 'Barcelona, Spanje', 'Eerlijke praktijken en stevig denim'),
(106, 'de', 'EthicalDenim', 'Ethischer Denim', 'Verantwortungsvolle Jeans, gemacht für Langlebigkeit', 'Barcelona, Spanien', 'Faire Praktiken und robuster Denim'),

-- 107 MinimalStyle
(107, 'en', 'MinimalStyle', 'Minimalist wardrobe staples', 'Timeless design, everyday comfort', 'Stockholm, Sweden', 'Clean lines and neutral tones'),
(107, 'fr', 'MinimalStyle', 'Minimalisme', 'Design intemporel et confort', 'Stockholm, Suède', 'Lignes épurées et tons neutres'),
(107, 'nl', 'MinimalStyle', 'Minimalistische essentials', 'Tijdloos design en dagelijks comfort', 'Stockholm, Zweden', 'Strakke lijnen en neutrale tinten'),
(107, 'de', 'MinimalStyle', 'Minimalistische Basics', 'Zeitloses Design, Alltagkomfort', 'Stockholm, Schweden', 'Klare Linien und neutrale Farben'),

-- 108 ArtisanCraft
(108, 'en', 'ArtisanCraft', 'Handmade artisan pieces', 'Crafted with care and tradition', 'Athens, Greece', 'Small-batch production and unique details'),
(108, 'fr', 'ArtisanCraft', 'Artisanal', 'Fabriqué à la main avec soin', 'Athènes, Grèce', 'Petites séries et détails uniques'),
(108, 'nl', 'ArtisanCraft', 'Ambachtelijk', 'Met zorg en traditie handgemaakt', 'Athene, Griekenland', 'Kleine oplages en unieke details'),
(108, 'de', 'ArtisanCraft', 'Handwerklich', 'Mit Sorgfalt und Tradition handgefertigt', 'Athen, Griechenland', 'Kleinserien und einzigartige Details'),

-- 109 EcoKids
(109, 'en', 'EcoKids', 'Eco-friendly kidswear', 'Safe materials for little ones', 'Copenhagen, Denmark', 'Soft fabrics and responsible sourcing'),
(109, 'fr', 'EcoKids', 'Vêtements enfants', 'Matériaux sûrs pour les petits', 'Copenhague, Danemark', 'Textiles doux et sourcing responsable'),
(109, 'nl', 'EcoKids', 'Duurzame kinderkleding', 'Veilige materialen voor kleintjes', 'Kopenhagen, Denemarken', 'Zachte stoffen en verantwoord ingekocht'),
(109, 'de', 'EcoKids', 'Nachhaltige Kinderkleidung', 'Sichere Materialien für die Kleinsten', 'Kopenhagen, Dänemark', 'Weiche Stoffe und verantwortungsvolle Herkunft');

-- ============================================
-- PRODUCTS (4 langues)
-- ============================================

INSERT INTO "product_translations"
("ProductId", "LanguageCode", "Name", "Description")
VALUES
-- 100
(100, 'en', 'Organic Cotton Tee - Red', 'Soft organic cotton t-shirt in bright red'),
(100, 'fr', 'T-shirt coton bio - Rouge', 'T-shirt en coton biologique rouge vif'),
(100, 'nl', 'Biologisch katoenen T-shirt - Rood', 'Zacht biologisch katoenen T-shirt in felrood'),
(100, 'de', 'Bio-Baumwoll-T-Shirt - Rot', 'Weiches Bio-Baumwoll-T-Shirt in kräftigem Rot'),

-- 101
(101, 'en', 'Organic Cotton Tee - Blue', 'Organic cotton t-shirt in ocean blue'),
(101, 'fr', 'T-shirt coton bio - Bleu', 'T-shirt en coton biologique bleu océan'),
(101, 'nl', 'Biologisch katoenen T-shirt - Blauw', 'Biologisch katoenen T-shirt in oceaanblauw'),
(101, 'de', 'Bio-Baumwoll-T-Shirt - Blau', 'Bio-Baumwoll-T-Shirt in Ozeanblau'),

-- 102
(102, 'en', 'Organic Cotton Tee - White', 'Organic cotton t-shirt in pure white'),
(102, 'fr', 'T-shirt coton bio - Blanc', 'T-shirt en coton biologique blanc pur'),
(102, 'nl', 'Biologisch katoenen T-shirt - Wit', 'Biologisch katoenen T-shirt in zuiver wit'),
(102, 'de', 'Bio-Baumwoll-T-Shirt - Weiß', 'Bio-Baumwoll-T-Shirt in reinem Weiß'),

-- 103
(103, 'en', 'Eco Hoodie - Black', 'Classic black eco hoodie'),
(103, 'fr', 'Hoodie éco - Noir', 'Hoodie écologique noir classique'),
(103, 'nl', 'Eco hoodie - Zwart', 'Klassieke zwarte eco-hoodie'),
(103, 'de', 'Eco-Hoodie - Schwarz', 'Klassischer schwarzer Eco-Hoodie'),

-- 104
(104, 'en', 'Eco Hoodie - Gray', 'Eco hoodie in heather gray'),
(104, 'fr', 'Hoodie éco - Gris', 'Hoodie écologique gris chiné'),
(104, 'nl', 'Eco hoodie - Grijs', 'Eco-hoodie in gemêleerd grijs'),
(104, 'de', 'Eco-Hoodie - Grau', 'Eco-Hoodie in meliertem Grau'),

-- 105
(105, 'en', 'Sustainable Jeans - Blue', 'Durable sustainable denim in blue'),
(105, 'fr', 'Jean durable - Bleu', 'Jean durable bleu denim'),
(105, 'nl', 'Duurzame jeans - Blauw', 'Duurzame denim jeans in blauw'),
(105, 'de', 'Nachhaltige Jeans - Blau', 'Langlebige nachhaltige Denim-Jeans in Blau'),

-- 106
(106, 'en', 'Sustainable Jeans - Black', 'Durable sustainable denim in deep black'),
(106, 'fr', 'Jean durable - Noir', 'Jean durable noir intense'),
(106, 'nl', 'Duurzame jeans - Zwart', 'Duurzame denim jeans in diepzwart'),
(106, 'de', 'Nachhaltige Jeans - Schwarz', 'Langlebige nachhaltige Denim-Jeans in tiefem Schwarz'),

-- 107
(107, 'en', 'Premium Jacket - Black', 'Elegant premium jacket in black'),
(107, 'fr', 'Veste premium - Noir', 'Veste premium noire élégante'),
(107, 'nl', 'Premium jas - Zwart', 'Elegante premium jas in zwart'),
(107, 'de', 'Premium-Jacke - Schwarz', 'Elegante Premium-Jacke in Schwarz'),

-- 108
(108, 'en', 'Premium Jacket - Navy', 'Premium jacket in navy blue'),
(108, 'fr', 'Veste premium - Bleu marine', 'Veste premium bleu marine'),
(108, 'nl', 'Premium jas - Marineblauw', 'Premium jas in marineblauw'),
(108, 'de', 'Premium-Jacke - Navy', 'Premium-Jacke in Navy'),

-- 109
(109, 'en', 'Premium Jacket - Gray', 'Sophisticated premium jacket in gray'),
(109, 'fr', 'Veste premium - Gris', 'Veste premium grise sophistiquée'),
(109, 'nl', 'Premium jas - Grijs', 'Verfijnde premium jas in grijs'),
(109, 'de', 'Premium-Jacke - Grau', 'Edle Premium-Jacke in Grau'),

-- 110
(110, 'en', 'Sport Leggings - Black', 'Technical black sports leggings'),
(110, 'fr', 'Leggings sport - Noir', 'Leggings sport noir technique'),
(110, 'nl', 'Sportlegging - Zwart', 'Technische zwarte sportlegging'),
(110, 'de', 'Sportleggings - Schwarz', 'Technische schwarze Sportleggings'),

-- 111
(111, 'en', 'Sport Leggings - Purple', 'Sport leggings in dynamic purple'),
(111, 'fr', 'Leggings sport - Violet', 'Leggings sport violet dynamique'),
(111, 'nl', 'Sportlegging - Paars', 'Sportlegging in dynamisch paars'),
(111, 'de', 'Sportleggings - Lila', 'Sportleggings in dynamischem Lila'),

-- 112
(112, 'en', 'Vintage Dress', 'One-of-a-kind vintage dress'),
(112, 'fr', 'Robe vintage', 'Robe vintage unique'),
(112, 'nl', 'Vintage jurk', 'Unieke vintage jurk'),
(112, 'de', 'Vintage-Kleid', 'Einzigartiges Vintage-Kleid'),

-- 113
(113, 'en', 'Ethical Denim Blue', 'Ethical blue denim jeans'),
(113, 'fr', 'Denim éthique - Bleu', 'Jean éthique bleu'),
(113, 'nl', 'Ethische denim - Blauw', 'Ethische blauwe denim jeans'),
(113, 'de', 'Ethischer Denim - Blau', 'Ethische blaue Denim-Jeans'),

-- 114
(114, 'en', 'Minimal Tee White', 'Minimal white t-shirt'),
(114, 'fr', 'T-shirt minimal - Blanc', 'T-shirt minimal blanc'),
(114, 'nl', 'Minimal T-shirt - Wit', 'Minimalistisch wit T-shirt'),
(114, 'de', 'Minimal-T-Shirt - Weiß', 'Minimalistisches weißes T-Shirt'),

-- 115
(115, 'en', 'Artisan Bag Brown', 'Handcrafted brown artisan bag'),
(115, 'fr', 'Sac artisanal - Marron', 'Sac artisanal marron'),
(115, 'nl', 'Ambachtelijke tas - Bruin', 'Handgemaakte ambachtelijke tas in bruin'),
(115, 'de', 'Handwerkstas - Braun', 'Handgefertigte handwerkliche Tasche in Braun'),

-- 116
(116, 'en', 'Kids Organic Tee', 'Organic t-shirt for kids'),
(116, 'fr', 'T-shirt enfant bio', 'T-shirt enfant bio'),
(116, 'nl', 'Biologisch kinder T-shirt', 'Biologisch T-shirt voor kinderen'),
(116, 'de', 'Bio-Kinder-T-Shirt', 'Bio-T-Shirt für Kinder');