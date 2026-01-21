use FitPassDb

-- SQL Server T-SQL Script

BEGIN TRANSACTION;

INSERT INTO dbo.Gyms (
    Id, 
    Name, 
    -- Address Value Object
    Address_Line1, 
    Address_Line2, 
    Address_City, 
    Address_State, 
    Address_PostalCode, 
    Address_CountryAlpha2,
    Status, 
    Tier, 
    -- Auditable Fields
    CreatedOn,
    CreatedBy,
    LastModifiedOn,
    LastModifiedBy
)
VALUES
-- 1. Active Elite Gym in NY
(NEWID(), 'Iron Olympus', '100 Broadway', 'Suite 500', 'New York', 'NY', '10005', 'US', 0, 3, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 2. Local Gym in London
(NEWID(), 'The Local Pump', '15 Baker Street', NULL, 'London', NULL, 'NW1 6XE', 'GB', 0, 0, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 3. MidRange in Toronto
(NEWID(), 'Maple Leaf Fitness', '450 Yonge St', NULL, 'Toronto', 'ON', 'M4Y 1W9', 'CA', 0, 1, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 4. Premium in Berlin
(NEWID(), 'Kraftwerk Berlin', 'Alexanderplatz 1', NULL, 'Berlin', NULL, '10178', 'DE', 0, 2, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 5. Suspended Local Gym
(NEWID(), 'Basement Barbell', '88 Industrial Way', NULL, 'Chicago', 'IL', '60601', 'US', 2, 0, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 6. Active MidRange in Paris
(NEWID(), 'Le Gym Parisien', '12 Rue de Rivoli', NULL, 'Paris', NULL, '75001', 'FR', 0, 1, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 7. Active Elite in Los Angeles
(NEWID(), 'Gold Coast Athletics', '500 Santa Monica Blvd', NULL, 'Santa Monica', 'CA', '90401', 'US', 0, 3, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 8. Inactive Local Gym
(NEWID(), 'Old School Iron', '22 Rust Bucket Rd', NULL, 'Detroit', 'MI', '48201', 'US', 1, 0, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 9. Active Premium in Sydney
(NEWID(), 'Harbour Fit', '200 George St', 'Level 4', 'Sydney', 'NSW', '2000', 'AU', 0, 2, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 10. Active MidRange in Austin
(NEWID(), 'Lone Star Lifts', '1200 Congress Ave', NULL, 'Austin', 'TX', '78701', 'US', 0, 1, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 11. Active Elite in Tokyo
(NEWID(), 'Tokyo Strength Club', '1-1 Chiyoda', NULL, 'Tokyo', NULL, '100-8111', 'JP', 0, 3, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 12. Active Local in Dublin
(NEWID(), 'Clover Crossfit', '5 O''Connell Street', NULL, 'Dublin', NULL, 'D01', 'IE', 0, 0, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 13. Active Premium in Miami
(NEWID(), 'Ocean Drive Wellness', '10 Ocean Dr', NULL, 'Miami Beach', 'FL', '33139', 'US', 0, 2, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 14. Suspended MidRange
(NEWID(), 'Fraud Fitness', '99 Sketchy Lane', NULL, 'Las Vegas', 'NV', '89109', 'US', 2, 1, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 15. Active Local in Rome
(NEWID(), 'Colosseum Gym', 'Via del Corso 10', NULL, 'Rome', NULL, '00186', 'IT', 0, 0, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 16. Active Elite in Dubai
(NEWID(), 'Burj Fitness', 'Sheikh Zayed Rd', 'Floor 45', 'Dubai', NULL, '00000', 'AE', 0, 3, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 17. Inactive Premium
(NEWID(), 'Closed Circuit Gym', '404 Not Found St', NULL, 'San Francisco', 'CA', '94105', 'US', 1, 2, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 18. Active MidRange in Barcelona
(NEWID(), 'Sol y Acero', 'La Rambla 50', NULL, 'Barcelona', NULL, '08002', 'ES', 0, 1, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 19. Active Local in Denver
(NEWID(), 'Mile High Muscle', '1600 Broadway', NULL, 'Denver', 'CO', '80202', 'US', 0, 0, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL),

-- 20. Active Elite in Singapore
(NEWID(), 'Marina Bay Fitness', '10 Bayfront Ave', NULL, 'Singapore', NULL, '018956', 'SG', 0, 3, SYSDATETIMEOFFSET(), 'Seed_Script', SYSDATETIMEOFFSET(), NULL);

COMMIT;