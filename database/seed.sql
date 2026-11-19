-- Liv'In Paris, demo data (fictional people, small extract of the metro network).
-- Usage: mysql -u root -p < database/seed.sql   (after schema.sql)
-- Every demo account uses the password: demo1234

USE livinparis;

-- Metro sample: part of lines 1 and 4, connected at Châtelet.
-- Coordinates are approximate. Import the full network for real use (see README).
INSERT INTO metroParis (StationId, Station, Ligne, Precedent, Suivant, TempsEntre2Stations, TempsChangement, Longitude, Latitude) VALUES
 (101, 'Concorde',                    '1', NULL,  '102', '0', '0', 2.3212, 48.8656),
 (102, 'Tuileries',                   '1', '101', '103', '1', '0', 2.3297, 48.8645),
 (103, 'Palais Royal - Musée du Louvre','1','102','104', '1', '0', 2.3363, 48.8624),
 (104, 'Louvre - Rivoli',             '1', '103', '105', '1', '0', 2.3410, 48.8609),
 (105, 'Châtelet',                    '1', '104', '106', '1', '4', 2.3470, 48.8584),
 (106, 'Hôtel de Ville',              '1', '105', NULL,  '1', '0', 2.3519, 48.8574),
 (401, 'Les Halles',                  '4', NULL,  '402', '0', '0', 2.3460, 48.8625),
 (402, 'Châtelet',                    '4', '401', '403', '1', '4', 2.3470, 48.8584),
 (403, 'Cité',                        '4', '402', '404', '2', '0', 2.3470, 48.8550),
 (404, 'Saint-Michel',                '4', '403', '405', '1', '0', 2.3440, 48.8536),
 (405, 'Odéon',                       '4', '404', NULL,  '1', '0', 2.3389, 48.8521);

INSERT INTO identite (ID, Nom, Prenom, Adresse, Telephone, Email, MotDePasse) VALUES
 (1, 'Martin',  'Camille', 'Concorde',       '0600000001', 'camille.martin@example.com',  'pbkdf2$100000$hsLw0WrU0GhNhJBcV0vlhQ==$xRDp5X1DRzGFiyP4jwLRP3IKiYAkvoMahrdAMjOP4PE='),
 (2, 'Bernard', 'Lucas',   'Odéon',          '0600000002', 'lucas.bernard@example.com',   'pbkdf2$100000$hsLw0WrU0GhNhJBcV0vlhQ==$xRDp5X1DRzGFiyP4jwLRP3IKiYAkvoMahrdAMjOP4PE='),
 (3, 'Petit',   'Inès',    'Hôtel de Ville', '0600000003', 'ines.petit@example.com',      'pbkdf2$100000$hsLw0WrU0GhNhJBcV0vlhQ==$xRDp5X1DRzGFiyP4jwLRP3IKiYAkvoMahrdAMjOP4PE='),
 (4, 'Durand',  'Hugo',    'Les Halles',     '0600000004', 'hugo.durand@example.com',     'pbkdf2$100000$hsLw0WrU0GhNhJBcV0vlhQ==$xRDp5X1DRzGFiyP4jwLRP3IKiYAkvoMahrdAMjOP4PE=');

INSERT INTO client (ID, TypeClient, NomEntreprise, Referent) VALUES
 (1, 'Particulier', NULL, NULL),
 (3, 'Entreprise', 'Studio Petit', 'Inès Petit'),
 (4, 'Particulier', NULL, NULL);

-- Hugo (4) is both a customer and a cook.
INSERT INTO cuisinier (ID) VALUES (2), (4);

-- Dishes on offer (no order yet).
INSERT INTO plat (Nom, Quantite, date_de_fabrication, Date_de_peremption, Prix, Regime, CuisinierID, CommandeID) VALUES
 ('Poulet basquaise',   4, CURDATE(), DATE_ADD(CURDATE(), INTERVAL 3 DAY), 12.50, 'Standard',   2, NULL),
 ('Curry de légumes',   6, CURDATE(), DATE_ADD(CURDATE(), INTERVAL 4 DAY),  9.00, 'Végétarien', 2, NULL),
 ('Lasagnes maison',    3, CURDATE(), DATE_ADD(CURDATE(), INTERVAL 2 DAY), 11.00, 'Standard',   4, NULL);

-- Past orders (one ordered portion per row in plat).
INSERT INTO commande (CommandeID, DateCommande, AdresseLivraison, PrixTotal, Statut, ModePaiement, ClientID) VALUES
 (1, NOW() - INTERVAL 5 DAY, 'Concorde',       12.50, 'Livrée',   'Carte',      1),
 (2, NOW() - INTERVAL 2 DAY, 'Hôtel de Ville',  9.00, 'Livrée',   'Carte',      3),
 (3, NOW() - INTERVAL 1 DAY, 'Concorde',       11.00, 'En cours', 'En attente', 1);

INSERT INTO plat (Nom, Quantite, date_de_fabrication, Date_de_peremption, Prix, Regime, CuisinierID, CommandeID) VALUES
 ('Poulet basquaise', 1, CURDATE() - INTERVAL 5 DAY, CURDATE() - INTERVAL 2 DAY, 12.50, 'Standard',   2, 1),
 ('Curry de légumes', 1, CURDATE() - INTERVAL 2 DAY, CURDATE() + INTERVAL 1 DAY,  9.00, 'Végétarien', 2, 2),
 ('Lasagnes maison',  1, CURDATE() - INTERVAL 1 DAY, CURDATE() + INTERVAL 1 DAY, 11.00, 'Standard',   4, 3);
