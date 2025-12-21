-- ============================================
-- SCRIPT MASSIF COMPLET - NOUVEAU MODÈLE AVEC ETHICS V2 (CORRIGÉ)
-- ============================================

-- 1. NETTOYER TOUT
DELETE FROM "BrandQuestionResponseOptions";
DELETE FROM "BrandQuestionResponses";
DELETE FROM "BrandQuestionnaires";
DELETE FROM "UserReviews";
DELETE FROM "Deposits";
DELETE FROM "ProductReviews";
DELETE FROM "ProductMedia";
DELETE FROM "ProductVariants";
DELETE FROM "Products";
DELETE FROM "ProductGroups";
DELETE FROM "Brands";
DELETE FROM "EthicsOptions";
DELETE FROM "EthicsQuestions";
DELETE FROM "Keywords";
DELETE FROM "Categories";
DELETE FROM "Colors";
DELETE FROM "Users" WHERE "Id" BETWEEN 100 AND 200;

-- Note: Les Sizes sont déjà créées par la migration (IDs 1-19, 99)
-- On ajoute seulement les tailles de jeans manquantes
INSERT INTO "Sizes" ("Id", "Name")
VALUES
    (20, '28'), (21, '30'), (22, '32'), (23, '34')
    ON CONFLICT ("Id") DO NOTHING;

-- 2. CRÉER DES UTILISATEURS DE TEST

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

-- 3. INSÉRER LES DONNÉES DE BASE
INSERT INTO "Colors" ("Id", "Name", "Hexa")
VALUES
    (100, 'Red', '#FF0000'), (101, 'Blue', '#0000FF'), (102, 'Black', '#000000'),
    (103, 'White', '#FFFFFF'), (104, 'Green', '#00AA00'), (105, 'Yellow', '#FFFF00'),
    (106, 'Navy', '#000080'), (107, 'Gray', '#808080'), (108, 'Purple', '#800080'),
    (109, 'Orange', '#FFA500');

INSERT INTO "Categories" ("Id", "Name")
VALUES
    (100, 'T-Shirts'), (101, 'Jeans'), (102, 'Shoes'), (103, 'Accessories'),
    (104, 'Dresses'), (105, 'Jackets'), (106, 'Hoodies'), (107, 'Pants'),
    (108, 'Skirts'), (109, 'Swimwear');

INSERT INTO "Keywords" ("Id", "Name")
VALUES
    (100, 'eco-friendly'), (101, 'organic'), (102, 'sustainable'), (103, 'ethical'),
    (104, 'fair-trade'), (105, 'vegan'), (106, 'premium'), (107, 'casual'),
    (108, 'elegant'), (109, 'sporty');

-- ============================================
-- NOUVELLE STRUCTURE ETHICS V2 (CORRIGÉE)
-- ============================================

-- 3.0 CRÉER UN CATALOG VERSION (OBLIGATOIRE)
INSERT INTO "CatalogVersions" ("Id", "VersionNumber", "CreatedAt", "PublishedAt", "IsActive", "IsDraft")
VALUES
    (1, 'v1.0', NOW(), NOW(), true, false);

-- 3.2 CRÉER LES QUESTIONS D'ÉTHIQUE (AVEC CatalogVersionId ET Category en INT)
-- Category : 0=MaterialsManufacturing, 1=Transport, 2=SocialImpact, 3=Environmental
-- AnswerType : 0=SingleChoice, 1=MultipleChoice, 2=Text, 3=Number
INSERT INTO "EthicsQuestions" ("Id", "CatalogVersionId", "Category", "Key", "Label", "Order", "AnswerType", "IsActive")
VALUES
    -- MaterialsManufacturing (Category=0)
    (100, 1, 0, 'material_origin', 'Où proviennent vos matériaux ?', 1, 0, true),
    (101, 1, 0, 'manufacturing_conditions', 'Conditions de travail ?', 2, 0, true),
    (102, 1, 0, 'organic_certified', 'Certifiés bio ?', 3, 0, true),

    -- Transport (Category=1)
    (103, 1, 1, 'transport_method', 'Mode de transport ?', 1, 0, true),
    (104, 1, 1, 'carbon_offset', 'Compensation carbone ?', 2, 0, true),
    (105, 1, 1, 'local_production', 'Production locale ?', 3, 0, true);

-- 3.3 CRÉER LES OPTIONS DE RÉPONSE (AVEC Order ET IsActive)
INSERT INTO "EthicsOptions" ("Id", "QuestionId", "Key", "Label", "Score", "Order", "IsActive")
VALUES
    -- material_origin (QuestionId=100)
    (100, 100, 'local', 'Local', 100.0, 1, true),
    (101, 100, 'regional', 'Régional', 75.0, 2, true),
    (102, 100, 'imported', 'Importé', 40.0, 3, true),
    (103, 100, 'unknown', 'Inconnu', 0.0, 4, true),

    -- manufacturing_conditions (QuestionId=101)
    (104, 101, 'excellent', 'Excellent', 100.0, 1, true),
    (105, 101, 'good', 'Bon', 80.0, 2, true),
    (106, 101, 'fair', 'Correct', 50.0, 3, true),
    (107, 101, 'poor', 'Mauvais', 10.0, 4, true),

    -- organic_certified (QuestionId=102)
    (108, 102, 'fully', 'Entièrement certifié', 100.0, 1, true),
    (109, 102, 'mostly', 'Majoritaire (80%+)', 70.0, 2, true),
    (110, 102, 'partial', 'Partiel', 40.0, 3, true),
    (111, 102, 'none', 'Aucun', 0.0, 4, true),

    -- transport_method (QuestionId=103)
    (112, 103, 'sea', 'Maritime', 90.0, 1, true),
    (113, 103, 'train', 'Train', 85.0, 2, true),
    (114, 103, 'truck', 'Camion', 60.0, 3, true),
    (115, 103, 'air', 'Avion', 10.0, 4, true),

    -- carbon_offset (QuestionId=104)
    (116, 104, 'yes_cert', 'Oui certifié', 100.0, 1, true),
    (117, 104, 'partial_off', 'Partiellement', 60.0, 2, true),
    (118, 104, 'planned', 'En projet', 20.0, 3, true),
    (119, 104, 'no', 'Non', 0.0, 4, true),

    -- local_production (QuestionId=105)
    (120, 105, 'local_100', '100% local', 100.0, 1, true),
    (121, 105, 'local_70', '70%+ local', 75.0, 2, true),
    (122, 105, 'partial_local', 'Partiel', 40.0, 3, true),
    (123, 105, 'no_local', 'Non local', 0.0, 4, true);

-- 4. CRÉER LES MARQUES
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


-- 5. CRÉER LES ADRESSES DE LIVRAISON DE TEST POUR L'UTILISATEUR PRINCIPAL (Hugo - ID 97)
INSERT INTO "ShippingAddresses" ("Id", "UserId", "Street", "Number", "PostalCode", "City", "Country", "IsDefault")
VALUES
    (2000, 97, 'Avenue Louise', '100', '1050', 'Bruxelles', 'BE', true),
    (2001, 97, 'Rue Léopold', '25', '4000', 'Liège', 'BE', false),
    (2002, 97, 'Rue de Rivoli', '50', '75001', 'Paris', 'FR', false);

-- 6. CRÉER LES DÉPÔTS EN BELGIQUE
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

-- 7. CRÉER LES PRODUCTGROUPS
INSERT INTO "ProductGroups" ("Id", "Name", "BaseDescription", "BrandId", "CategoryId")
VALUES
    (100, 'Organic Cotton Tee', 'T-shirt en coton bio ultra-doux', 100, 100),
    (101, 'Eco Hoodie', 'Hoodie confortable et éco-responsable', 100, 106),
    (102, 'Sustainable Jeans', 'Jean durable et éthique', 101, 101),
    (103, 'Premium Jacket', 'Veste premium haut de gamme', 103, 105),
    (104, 'Sport Leggings', 'Leggings haute performance', 104, 107);

-- 8. CRÉER LES PRODUCTS
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

-- 9. CRÉER LES PRODUCTMEDIA
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
    (116, 113, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219569/kids-tee-1_rmwbfh.jpg', 'Image', 1, true),
    (117, 114, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219591/tee-white-2_emlexh.jpg', 'Image', 1, true),
    (118, 115, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219592/bag-brown-1_yawuge.jpg', 'Image', 1, true),
    (119, 116, 'https://res.cloudinary.com/db82qv38a/image/upload/v1765219569/kids-tee-1_rmwbfh.jpg', 'Image', 1, true);

-- 10. CRÉER LES PRODUCTVARIANTS
INSERT INTO "ProductVariants" ("Id", "ProductId", "SizeId", "SKU", "StockCount")
VALUES
    (100, 100, 2, 'ECO-TEE-RED-S', 20), (101, 100, 3, 'ECO-TEE-RED-M', 50), (102, 100, 4, 'ECO-TEE-RED-L', 30),
    (103, 101, 2, 'ECO-TEE-BLUE-S', 25), (104, 101, 3, 'ECO-TEE-BLUE-M', 60), (105, 101, 4, 'ECO-TEE-BLUE-L', 35),
    (106, 102, 2, 'ECO-TEE-WHITE-S', 15), (107, 102, 3, 'ECO-TEE-WHITE-M', 40), (108, 102, 4, 'ECO-TEE-WHITE-L', 25),
    (109, 103, 2, 'ECO-HOOD-BLACK-S', 10), (110, 103, 3, 'ECO-HOOD-BLACK-M', 30), (111, 103, 4, 'ECO-HOOD-BLACK-L', 25), (112, 103, 5, 'ECO-HOOD-BLACK-XL', 15),
    (113, 104, 2, 'ECO-HOOD-GRAY-S', 12), (114, 104, 3, 'ECO-HOOD-GRAY-M', 28), (115, 104, 4, 'ECO-HOOD-GRAY-L', 20), (116, 104, 5, 'ECO-HOOD-GRAY-XL', 10),
    (117, 105, 20, 'SUST-JEAN-BLUE-28', 15), (118, 105, 21, 'SUST-JEAN-BLUE-30', 40), (119, 105, 22, 'SUST-JEAN-BLUE-32', 35), (120, 105, 23, 'SUST-JEAN-BLUE-34', 25),
    (121, 106, 20, 'SUST-JEAN-BLACK-28', 10), (122, 106, 21, 'SUST-JEAN-BLACK-30', 30), (123, 106, 22, 'SUST-JEAN-BLACK-32', 28), (124, 106, 23, 'SUST-JEAN-BLACK-34', 20),
    (125, 107, 3, 'PREM-JACK-BLACK-M', 15), (126, 107, 4, 'PREM-JACK-BLACK-L', 20), (127, 107, 5, 'PREM-JACK-BLACK-XL', 10),
    (128, 108, 3, 'PREM-JACK-NAVY-M', 12), (129, 108, 4, 'PREM-JACK-NAVY-L', 18), (130, 108, 5, 'PREM-JACK-NAVY-XL', 8),
    (131, 109, 3, 'PREM-JACK-GRAY-M', 10), (132, 109, 4, 'PREM-JACK-GRAY-L', 15), (133, 109, 5, 'PREM-JACK-GRAY-XL', 5),
    (134, 110, 2, 'SPORT-LEG-BLACK-S', 30), (135, 110, 3, 'SPORT-LEG-BLACK-M', 45), (136, 110, 4, 'SPORT-LEG-BLACK-L', 25),
    (137, 111, 2, 'SPORT-LEG-PURPLE-S', 25), (138, 111, 3, 'SPORT-LEG-PURPLE-M', 40), (139, 111, 4, 'SPORT-LEG-PURPLE-L', 20),
    (140, 112, 3, 'VINT-DRESS-M', 10), (141, 113, 21, 'ETH-DENIM-30', 20), (142, 114, 3, 'MIN-TEE-M', 30),
    (143, 115, 99, 'ART-BAG-ONESIZE', 15), (144, 116, 2, 'KIDS-TEE-S', 40);

-- ============================================
-- 11. QUESTIONNAIRES ET RÉPONSES ÉTHIQUES (NOUVELLE STRUCTURE)
-- ============================================

-- 11.1 Créer les questionnaires (avec Status enum au lieu de IsApproved)
-- Status : 0=Draft, 1=Submitted, 2=UnderReview, 3=Approved, 4=Rejected

INSERT INTO "BrandQuestionnaires" ("Id", "BrandId", "CatalogVersionId", "Status", "CreatedAt", "SubmittedAt", "ReviewedAt")
VALUES
    (100, 100, 1, 3, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (101, 101, 1, 3, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (102, 102, 1, 3, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (103, 103, 1, 3, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (104, 104, 1, 3, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (105, 105, 1, 3, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (106, 106, 1, 3, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (107, 107, 1, 3, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (108, 108, 1, 3, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days'),
    (109, 109, 1, 3, NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '20 days');

-- 11.2 Créer les réponses (SANS OptionId direct)
INSERT INTO "BrandQuestionResponses" ("Id", "QuestionnaireId", "QuestionId", "CalculatedScore")
VALUES
    -- EcoWear (QuestionnaireId=100)
    (100, 100, 100, 100.0), (101, 100, 101, 100.0), (102, 100, 102, 100.0),
    (103, 100, 103, 90.0), (104, 100, 104, 100.0), (105, 100, 105, 100.0),

    -- NaturalStyle (QuestionnaireId=101)
    (106, 101, 100, 75.0), (107, 101, 101, 80.0), (108, 101, 102, 70.0),
    (109, 101, 103, 85.0), (110, 101, 104, 60.0), (111, 101, 105, 75.0),

    -- UrbanFit (QuestionnaireId=102)
    (112, 102, 100, 40.0), (113, 102, 101, 50.0), (114, 102, 102, 40.0),
    (115, 102, 103, 60.0), (116, 102, 104, 20.0), (117, 102, 105, 40.0),

    -- LuxeBrand (QuestionnaireId=103)
    (118, 103, 100, 100.0), (119, 103, 101, 100.0), (120, 103, 102, 70.0),
    (121, 103, 103, 90.0), (122, 103, 104, 100.0), (123, 103, 105, 75.0),

    -- SportWear Pro (QuestionnaireId=104)
    (124, 104, 100, 75.0), (125, 104, 101, 80.0), (126, 104, 102, 40.0),
    (127, 104, 103, 60.0), (128, 104, 104, 60.0), (129, 104, 105, 40.0),

    -- VintageTales (QuestionnaireId=105)
    (130, 105, 100, 40.0), (131, 105, 101, 50.0), (132, 105, 102, 0.0),
    (133, 105, 103, 10.0), (134, 105, 104, 0.0), (135, 105, 105, 0.0),

    -- EthicalDenim (QuestionnaireId=106)
    (136, 106, 100, 100.0), (137, 106, 101, 100.0), (138, 106, 102, 100.0),
    (139, 106, 103, 85.0), (140, 106, 104, 100.0), (141, 106, 105, 100.0),

    -- MinimalStyle (QuestionnaireId=107)
    (142, 107, 100, 75.0), (143, 107, 101, 50.0), (144, 107, 102, 40.0),
    (145, 107, 103, 10.0), (146, 107, 104, 20.0), (147, 107, 105, 40.0),

    -- ArtisanCraft (QuestionnaireId=108)
    (148, 108, 100, 100.0), (149, 108, 101, 100.0), (150, 108, 102, 100.0),
    (151, 108, 103, 90.0), (152, 108, 104, 100.0), (153, 108, 105, 100.0),

    -- EcoKids (QuestionnaireId=109)
    (154, 109, 100, 75.0), (155, 109, 101, 50.0), (156, 109, 102, 70.0),
    (157, 109, 103, 85.0), (158, 109, 104, 60.0), (159, 109, 105, 75.0);

-- 11.3 Créer les liens Response-Options (NOUVELLE TABLE)
INSERT INTO "BrandQuestionResponseOptions" ("ResponseId", "OptionId")
VALUES
    -- EcoWear
    (100, 100), (101, 104), (102, 108), (103, 112), (104, 116), (105, 120),
    -- NaturalStyle
    (106, 101), (107, 105), (108, 109), (109, 113), (110, 117), (111, 121),
    -- UrbanFit
    (112, 102), (113, 106), (114, 110), (115, 114), (116, 118), (117, 122),
    -- LuxeBrand
    (118, 100), (119, 104), (120, 109), (121, 112), (122, 116), (123, 121),
    -- SportWear Prox
    (124, 101), (125, 105), (126, 110), (127, 114), (128, 117), (129, 122),
    -- VintageTales
    (130, 102), (131, 107), (132, 111), (133, 115), (134, 119), (135, 123),
    -- EthicalDenim
    (136, 100), (137, 104), (138, 108), (139, 113), (140, 116), (141, 120),
    -- MinimalStyle
    (142, 101), (143, 106), (144, 110), (145, 115), (146, 118), (147, 122),
    -- ArtisanCraft
    (148, 100), (149, 104), (150, 108), (151, 112), (152, 116), (153, 120),
    -- EcoKids
    (154, 101), (155, 106), (156, 109), (157, 113), (158, 117), (159, 121);

-- ============================================
-- 11.4 CRÉER LES SCORES ÉTHIQUES OFFICIELS (NOUVELLE TABLE CRITIQUE)
-- ============================================
-- Cette table stocke les scores calculés et approuvés pour chaque marque par catégorie

INSERT INTO "BrandEthicScores" ("BrandId", "CategoryId", "QuestionnaireId", "RawScore", "FinalScore", "IsOfficial", "CreatedAt")
VALUES
    -- EcoWear (BrandId=100, QuestionnaireId=100) - Scores excellents
    (100, 1, 100, 100.0, 100.0, true, NOW() - INTERVAL '20 days'),  -- MaterialsManufacturing
    (100, 2, 100, 96.67, 96.67, true, NOW() - INTERVAL '20 days'),  -- Transport (avg: 90+100+100)/3

    -- NaturalStyle (BrandId=101, QuestionnaireId=101) - Scores bons
    (101, 1, 101, 75.0, 75.0, true, NOW() - INTERVAL '20 days'),    -- MaterialsManufacturing
    (101, 2, 101, 73.33, 73.33, true, NOW() - INTERVAL '20 days'),  -- Transport (avg: 85+60+75)/3

    -- UrbanFit (BrandId=102, QuestionnaireId=102) - Scores moyens
    (102, 1, 102, 43.33, 43.33, true, NOW() - INTERVAL '20 days'),  -- MaterialsManufacturing
    (102, 2, 102, 40.0, 40.0, true, NOW() - INTERVAL '20 days'),    -- Transport (avg: 60+20+40)/3

    -- LuxeBrand (BrandId=103, QuestionnaireId=103) - Scores excellents
    (103, 1, 103, 90.0, 90.0, true, NOW() - INTERVAL '20 days'),    -- MaterialsManufacturing
    (103, 2, 103, 88.33, 88.33, true, NOW() - INTERVAL '20 days'),  -- Transport (avg: 90+100+75)/3

    -- SportWear Pro (BrandId=104, QuestionnaireId=104) - Scores moyens-bons
    (104, 1, 104, 65.0, 65.0, true, NOW() - INTERVAL '20 days'),    -- MaterialsManufacturing
    (104, 2, 104, 53.33, 53.33, true, NOW() - INTERVAL '20 days'),  -- Transport (avg: 60+60+40)/3

    -- VintageTales (BrandId=105, QuestionnaireId=105) - Scores faibles
    (105, 1, 105, 30.0, 30.0, true, NOW() - INTERVAL '20 days'),    -- MaterialsManufacturing
    (105, 2, 105, 3.33, 3.33, true, NOW() - INTERVAL '20 days'),    -- Transport (avg: 10+0+0)/3

    -- EthicalDenim (BrandId=106, QuestionnaireId=106) - Scores excellents
    (106, 1, 106, 100.0, 100.0, true, NOW() - INTERVAL '20 days'),  -- MaterialsManufacturing
    (106, 2, 106, 95.0, 95.0, true, NOW() - INTERVAL '20 days'),    -- Transport (avg: 85+100+100)/3

    -- MinimalStyle (BrandId=107, QuestionnaireId=107) - Scores moyens
    (107, 1, 107, 55.0, 55.0, true, NOW() - INTERVAL '20 days'),    -- MaterialsManufacturing
    (107, 2, 107, 23.33, 23.33, true, NOW() - INTERVAL '20 days'),  -- Transport (avg: 10+20+40)/3

    -- ArtisanCraft (BrandId=108, QuestionnaireId=108) - Scores excellents
    (108, 1, 108, 100.0, 100.0, true, NOW() - INTERVAL '20 days'),  -- MaterialsManufacturing
    (108, 2, 108, 96.67, 96.67, true, NOW() - INTERVAL '20 days'),  -- Transport (avg: 90+100+100)/3

    -- EcoKids (BrandId=109, QuestionnaireId=109) - Scores moyens-bons
    (109, 1, 109, 65.0, 65.0, true, NOW() - INTERVAL '20 days'),    -- MaterialsManufacturing
    (109, 2, 109, 73.33, 73.33, true, NOW() - INTERVAL '20 days');  -- Transport (avg: 85+60+75)/3

-- Vérifier que les scores sont bien insérés
SELECT 'BrandEthicScores : ' || COUNT(*) as info FROM "BrandEthicScores";

-- Afficher un aperçu des scores par marque
SELECT
    b."Name" as brand_name,
    ec."Label" as category,
    bes."FinalScore" as score,
    bes."IsOfficial" as is_official
FROM "BrandEthicScores" bes
         JOIN "Brands" b ON bes."BrandId" = b."Id"
         JOIN "EthicsCategories" ec ON bes."CategoryId" = ec."Id"
ORDER BY b."Name", ec."Order";

-- 12. AVIS UTILISATEURS
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

-- 13. TAGS ÉTHIQUES
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
    (109, 'MaterialsManufacturing', 'sustainable');

INSERT INTO "BrandEthicTags" ("BrandId", "Category", "TagKey")
VALUES
    (100, 'Transport', 'carbon-offset'), (100, 'Transport', 'local-production'),
    (101, 'Transport', 'local-production'),
    (103, 'Transport', 'carbon-offset'),
    (106, 'Transport', 'local-production'),
    (108, 'Transport', 'local-production');

-- 14. CRÉER LES MÉTHODES DE LIVRAISON PAR MARQUE
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

-- RÉSUMÉ FINAL
SELECT '✅ TOUTES LES DONNÉES INSÉRÉES AVEC NOUVEAU MODÈLE ETHICS V2 !' as status;
SELECT 'Users : ' || COUNT(*) as info FROM "Users";
SELECT 'Brands : ' || COUNT(*) as info FROM "Brands";
SELECT 'Deposits : ' || COUNT(*) as info FROM "Deposits";
SELECT 'EthicsQuestions : ' || COUNT(*) as info FROM "EthicsQuestions";
SELECT 'EthicsOptions : ' || COUNT(*) as info FROM "EthicsOptions";
SELECT 'ProductGroups : ' || COUNT(*) as info FROM "ProductGroups";
SELECT 'Products : ' || COUNT(*) as info FROM "Products";
SELECT 'ProductMedia : ' || COUNT(*) as info FROM "ProductMedia";
SELECT 'ProductVariants : ' || COUNT(*) as info FROM "ProductVariants";
SELECT 'Questionnaires : ' || COUNT(*) as info FROM "BrandQuestionnaires";
SELECT 'QuestionResponses : ' || COUNT(*) as info FROM "BrandQuestionResponses";
SELECT 'ResponseOptions : ' || COUNT(*) as info FROM "BrandQuestionResponseOptions";
SELECT 'Reviews : ' || COUNT(*) as info FROM "UserReviews";
SELECT 'ShippingMethods : ' || COUNT(*) as info FROM "BrandShippingMethods";
