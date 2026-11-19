-- Liv'In Paris, database schema (MySQL 8)
-- Usage: mysql -u root -p < database/schema.sql

DROP DATABASE IF EXISTS livinparis;
CREATE DATABASE livinparis CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE livinparis;

-- Every person on the platform. Adresse holds the name of the nearest metro station.
CREATE TABLE identite (
    ID          INT AUTO_INCREMENT PRIMARY KEY,
    Nom         VARCHAR(100) NOT NULL,
    Prenom      VARCHAR(100) NOT NULL,
    Adresse     VARCHAR(255) NOT NULL,
    Telephone   VARCHAR(20)  NOT NULL,
    Email       VARCHAR(255) NOT NULL UNIQUE,
    MotDePasse  VARCHAR(255) NOT NULL COMMENT 'PBKDF2 hash, see PasswordHasher'
);

-- Customer role (a person can be customer, cook, or both).
CREATE TABLE client (
    ID            INT PRIMARY KEY,
    TypeClient    VARCHAR(20) NOT NULL DEFAULT 'Particulier' COMMENT 'Particulier or Entreprise',
    NomEntreprise VARCHAR(150) NULL,
    Referent      VARCHAR(150) NULL,
    FOREIGN KEY (ID) REFERENCES identite(ID) ON DELETE CASCADE
);

-- Cook role.
CREATE TABLE cuisinier (
    ID INT PRIMARY KEY,
    FOREIGN KEY (ID) REFERENCES identite(ID) ON DELETE CASCADE
);

CREATE TABLE commande (
    CommandeID       INT AUTO_INCREMENT PRIMARY KEY,
    DateCommande     DATETIME      NOT NULL,
    AdresseLivraison VARCHAR(255)  NOT NULL,
    PrixTotal        DECIMAL(10,2) NOT NULL,
    Statut           VARCHAR(50)   NOT NULL DEFAULT 'En cours',
    ModePaiement     VARCHAR(50)   NOT NULL DEFAULT 'En attente',
    ClientID         INT           NOT NULL,
    FOREIGN KEY (ClientID) REFERENCES identite(ID) ON DELETE CASCADE
);

-- A dish on offer (CommandeID NULL) or an ordered portion (CommandeID set).
CREATE TABLE plat (
    PlatID              INT AUTO_INCREMENT PRIMARY KEY,
    Nom                 VARCHAR(150)  NOT NULL,
    Quantite            INT           NOT NULL,
    date_de_fabrication DATE          NOT NULL,
    Date_de_peremption  DATE          NOT NULL,
    Prix                DECIMAL(10,2) NOT NULL,
    Regime              VARCHAR(50)   NOT NULL DEFAULT 'Standard',
    CuisinierID         INT           NOT NULL,
    CommandeID          INT           NULL,
    FOREIGN KEY (CuisinierID) REFERENCES identite(ID) ON DELETE CASCADE,
    FOREIGN KEY (CommandeID)  REFERENCES commande(CommandeID) ON DELETE CASCADE,
    INDEX idx_plat_commande (CommandeID),
    INDEX idx_plat_cuisinier (CuisinierID)
);

-- Paris metro network: one row per (station, line).
-- Precedent / Suivant hold the StationId of the neighbours on the same line.
-- TempsEntre2Stations is the time in minutes from the previous station,
-- TempsChangement the time to change to another line at this station.
CREATE TABLE metroParis (
    StationId           INT PRIMARY KEY,
    Station             VARCHAR(150) NOT NULL,
    Ligne               VARCHAR(10)  NOT NULL,
    Precedent           VARCHAR(10)  NULL,
    Suivant             VARCHAR(10)  NULL,
    TempsEntre2Stations VARCHAR(10)  NULL,
    TempsChangement     VARCHAR(10)  NULL,
    Longitude           DOUBLE       NULL,
    Latitude            DOUBLE       NULL
);
