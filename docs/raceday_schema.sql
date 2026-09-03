/* =========================================================
   RaceDay Database Schema
   PROG6212 POE — Part 1, Section C
   Run in SQL Server Management Studio (SSMS) on a clean database.
   ========================================================= */

-- Drop tables if re-running on the same database (safe for dev/testing)
IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID('dbo.EventEnrolments', 'U') IS NOT NULL DROP TABLE dbo.EventEnrolments;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL DROP TABLE dbo.Routes;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- =========================================================
-- Table: Users
-- =========================================================
CREATE TABLE dbo.Users (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Name          VARCHAR(100)  NOT NULL,
    Email         VARCHAR(150)  NOT NULL UNIQUE,
    PasswordHash  VARCHAR(255)  NOT NULL,
    Role          VARCHAR(20)   NOT NULL DEFAULT 'Participant'
                  CHECK (Role IN ('Organiser', 'Participant'))
);
GO

-- =========================================================
-- Table: Events
-- =========================================================
CREATE TABLE dbo.Events (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId   INT NOT NULL,
    Name          VARCHAR(150) NOT NULL,
    [Date]        DATETIME NOT NULL,
    Location      VARCHAR(150) NOT NULL,
    Description   VARCHAR(500) NULL,
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserId) REFERENCES dbo.Users(Id)
);
GO

-- =========================================================
-- Table: Routes  (1-to-1 with Events)
-- =========================================================
CREATE TABLE dbo.Routes (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    EventId       INT NOT NULL UNIQUE,
    Description   VARCHAR(300) NULL,
    DistanceKm    DECIMAL(5,2) NOT NULL,
    MapUrl        VARCHAR(255) NULL,
    CONSTRAINT FK_Routes_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(Id)
);
GO

-- =========================================================
-- Table: Categories  (many-to-1 with Events)
-- =========================================================
CREATE TABLE dbo.Categories (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    EventId       INT NOT NULL,
    Name          VARCHAR(50) NOT NULL,
    DistanceKm    DECIMAL(5,2) NOT NULL,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(Id)
);
GO

-- =========================================================
-- Table: EventEnrolments  (Participant enrols in a Category)
-- =========================================================
CREATE TABLE dbo.EventEnrolments (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId  INT NOT NULL,
    CategoryId     INT NOT NULL,
    EnrolmentDate  DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id),
    CONSTRAINT UQ_Participant_Category UNIQUE (ParticipantId, CategoryId)
);
GO

-- =========================================================
-- Table: Results  (1-to-1 with EventEnrolments)
-- =========================================================
CREATE TABLE dbo.Results (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId   INT NOT NULL UNIQUE,
    FinishTime    TIME NOT NULL,
    Position      INT NOT NULL,
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId) REFERENCES dbo.EventEnrolments(Id)
);
GO

/* =========================================================
   Sample Data
   2 Organisers, 2 Participants, 3 Events, categories per
   event, and sample enrolments — as required by the brief.
   ========================================================= */

-- Users: 2 Organisers, 2 Participants
INSERT INTO dbo.Users (Name, Email, PasswordHash, Role) VALUES
('Thabo Mokoena',  'thabo.mokoena@raceday.co.za',  'HASHED_PASSWORD_1', 'Organiser'),
('Lindiwe Dlamini', 'lindiwe.dlamini@raceday.co.za', 'HASHED_PASSWORD_2', 'Organiser'),
('Sipho Nkosi',     'sipho.nkosi@example.com',       'HASHED_PASSWORD_3', 'Participant'),
('Aisha Khan',      'aisha.khan@example.com',        'HASHED_PASSWORD_4', 'Participant');
GO

-- Events: 3 events, owned by the two Organisers
INSERT INTO dbo.Events (OrganiserId, Name, [Date], Location, Description) VALUES
(1, 'Pretoria Park Run Challenge', '2026-10-10 07:00:00', 'Pretoria, Gauteng', 'A community park run through the Union Buildings gardens.'),
(1, 'Soweto Heritage Marathon',    '2026-11-15 06:00:00', 'Soweto, Gauteng',   'A marathon celebrating Soweto''s history and culture.'),
(2, 'Cape Town Coastal Cycle Tour','2026-12-05 06:30:00', 'Cape Town, Western Cape', 'A scenic cycling event along the Atlantic coastline.');
GO

-- Routes: one per event (1-to-1)
INSERT INTO dbo.Routes (EventId, Description, DistanceKm, MapUrl) VALUES
(1, 'Loop through the park and surrounding gardens.', 5.00,  'https://maps.example.com/route1'),
(2, 'Point-to-point route through Soweto''s main streets.', 42.20, 'https://maps.example.com/route2'),
(3, 'Coastal road route with sea views.', 90.00, 'https://maps.example.com/route3');
GO

-- Categories: at least one set per event
INSERT INTO dbo.Categories (EventId, Name, DistanceKm) VALUES
(1, '5km Fun Run', 5.00),
(1, '10km Challenge', 10.00),
(2, '21km Half Marathon', 21.10),
(2, '42km Full Marathon', 42.20),
(3, '45km Half Route', 45.00),
(3, '90km Full Route', 90.00);
GO

-- Enrolments: sample participants enrolling in categories
INSERT INTO dbo.EventEnrolments (ParticipantId, CategoryId, EnrolmentDate) VALUES
(3, 1, '2026-09-01 08:00:00'),  -- Sipho -> 5km Fun Run
(3, 3, '2026-09-02 09:15:00'),  -- Sipho -> 21km Half Marathon
(4, 2, '2026-09-03 10:30:00'),  -- Aisha -> 10km Challenge
(4, 6, '2026-09-04 11:45:00');  -- Aisha -> 90km Full Route
GO

-- Results: sample results for two of the enrolments
INSERT INTO dbo.Results (EnrolmentId, FinishTime, Position) VALUES
(1, '00:28:45', 3),
(3, '00:52:10', 7);
GO
