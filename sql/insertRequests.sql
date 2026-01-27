USE FitPassDb;
GO

INSERT INTO [Requests] (
    [Id],
    [CreatedOn],
    [CreatedBy],
    [LastModifiedOn],
    [LastModifiedBy],
    [Title],
    [Description],
    [PriorityLevel],
    [Type],
    [Status],
    [HandlerRationale],
    [Error],
    [Payload]
)
VALUES
-- ======================================================================================
-- GYM CREATION
-- ======================================================================================

-- 1) Register Titan Fitness (Submitted, High)
(
    NEWID(),
    SYSDATETIMEOFFSET(),
    'PendingGymEmployeeId',
    SYSDATETIMEOFFSET(),
    'PendingGymEmployeeId',
    'Register Titan Fitness',
    'Opening a new flagship location in the business district.',
    3, -- High
    0, -- GymCreation
    0, -- Submitted
    NULL,
    NULL,
    '{"Name":"Titan Fitness","Address":{"Line1":"100 Wall St","Line2":"Floor 1","City":"New York","State":"NY","PostalCode":"10005","CountryAlpha2":"US"},"Status":0,"Tier":3,"SupervisorEmail":"ceo@titanfit.com"}'
),

-- 2) Register Joe's Garage (Rejected, Medium)
(
    NEWID(),
    DATEADD(DAY, -3, SYSDATETIMEOFFSET()),
    'PendingGymEmployeeId',
    DATEADD(DAY, -1, SYSDATETIMEOFFSET()),
    'AppAdminLocalhostId',
    'Register Joe''s Garage',
    'Small local gym setup.',
    2, -- Medium
    0, -- GymCreation
    3, -- Rejected
    'Address validation failed. "Garage" is not a valid commercial zone.',
    NULL,
    '{"Name":"Joe''s Garage","Address":{"Line1":"12 Back Alley","Line2":null,"City":"Chicago","State":"IL","PostalCode":"60601","CountryAlpha2":"US"},"Status":0,"Tier":0,"SupervisorEmail":"joe@garage.com"}'
),

-- 3) Register SeaSide Wellness (Error, High)
(
    NEWID(),
    DATEADD(HOUR, -5, SYSDATETIMEOFFSET()),
    'PendingGymEmployeeId',
    SYSDATETIMEOFFSET(),
    'PendingGymEmployeeId',
    'Register SeaSide Wellness',
    'Luxury wellness center onboarding.',
    3, -- High
    0, -- GymCreation
    4, -- Error
    NULL,
    'System.Data.DbUpdateException: Unique constraint violation on Index_GymName.',
    '{"Name":"SeaSide Wellness","Address":{"Line1":"1 Ocean Dr","Line2":null,"City":"Miami","State":"FL","PostalCode":"33101","CountryAlpha2":"US"},"Status":0,"Tier":2,"SupervisorEmail":"admin@seaside.com"}'
),

-- 4) Register Metro Flex (Approved, Medium)
(
    NEWID(),
    DATEADD(DAY, -10, SYSDATETIMEOFFSET()),
    'PendingGymEmployeeId',
    DATEADD(DAY, -9, SYSDATETIMEOFFSET()),
    'AppAdminLocalhostId',
    'Register Metro Flex',
    'Mid-range city gym.',
    2, -- Medium
    0, -- GymCreation
    1, -- Approved
    'Gym successfully provisioned and admin assigned.',
    NULL,
    '{"Name":"Metro Flex","Address":{"Line1":"55 Main St","Line2":null,"City":"Seattle","State":"WA","PostalCode":"98101","CountryAlpha2":"US"},"Status":0,"Tier":1,"SupervisorEmail":"contact@metroflex.com"}'
),

-- ======================================================================================
-- GYM ADMIN PROMOTION
-- ======================================================================================

-- 5) Promote Sarah Connor (Submitted, Medium)
(
    NEWID(),
    SYSDATETIMEOFFSET(),
    'GymAdminLocalhostId',
    SYSDATETIMEOFFSET(),
    'GymAdminLocalhostId',
    'Promote Sarah Connor',
    'Promoting lead trainer to assistant manager role.',
    2, -- Medium
    1, -- GymAdminPromotion
    0, -- Submitted
    NULL,
    NULL,
    '{"GymId":"TestGymId","PendingGymEmployeeEmail":"pendinggymemployee@localhost.com","SupervisorEmail":"sarah.c@gym.com"}'
),

-- 6) Promote John Smith (Submitted, Low)
(
    NEWID(),
    DATEADD(DAY, -1, SYSDATETIMEOFFSET()),
    'GymAdminLocalhostId',
    SYSDATETIMEOFFSET(),
    'GymAdminLocalhostId',
    'Promote John Smith',
    'Nomination for admin rights.',
    1, -- Low
    1, -- GymAdminPromotion
    0, -- Submitted
    NULL,
    NULL,
    '{"GymId":"gym-001-guid","PendingGymEmployeeEmail":"invalid@localhost.com","SupervisorEmail":"john.s@gym.com"}'
),

-- ======================================================================================
-- OTHER
-- ======================================================================================

-- 7) App Dark Mode Request (Submitted, Low)
(
    NEWID(),
    SYSDATETIMEOFFSET(),
    'UserId',
    SYSDATETIMEOFFSET(),
    'UserId',
    'App Dark Mode Request',
    'Can we please get a dark mode for the mobile app?',
    1, -- Low
    2, -- Other
    0, -- Submitted
    NULL,
    NULL,
    NULL
),

-- 8) Missing Report Data (Approved, Medium)
(
    NEWID(),
    DATEADD(DAY, -4, SYSDATETIMEOFFSET()),
    'GymStaffLocalhostId',
    DATEADD(DAY, -2, SYSDATETIMEOFFSET()),
    'AppAdminLocalhostId',
    'Missing Report Data',
    'The weekly attendance report for last Tuesday is empty.',
    2, -- Medium
    2, -- Other
    1, -- Approved
    'Data was stuck in cache. Refreshed and report is now available.',
    NULL,
    NULL
),

-- 9) Double Charge on Credit Card (Error, High)
(
    NEWID(),
    DATEADD(HOUR, -2, SYSDATETIMEOFFSET()),
    'UserId',
    SYSDATETIMEOFFSET(),
    'UserId',
    'Double Charge on Credit Card',
    'I was charged twice for my monthly subscription.',
    3, -- High
    2, -- Other
    4, -- Error
    NULL,
    'PaymentGatewayException: Connection timed out while verifying transaction ID.',
    NULL
),

-- 10) Free T-Shirt (Rejected, Low)
(
    NEWID(),
    DATEADD(DAY, -6, SYSDATETIMEOFFSET()),
    'UserId',
    DATEADD(DAY, -5, SYSDATETIMEOFFSET()),
    'AppAdminLocalhostId',
    'Free T-Shirt',
    'Send me a free t-shirt please.',
    1, -- Low
    2, -- Other
    3, -- Rejected
    'Not a valid support request.',
    NULL,
    NULL
),

-- 11) Suspicious Activity Detected (Submitted, High)
(
    NEWID(),
    SYSDATETIMEOFFSET(),
    'GymStaffLocalhostId',
    SYSDATETIMEOFFSET(),
    'GymStaffLocalhostId',
    'Suspicious Activity Detected',
    'User with ID 5555 tried to scan into the gym 40 times in 1 minute.',
    3, -- High
    2, -- Other
    0, -- Submitted
    NULL,
    NULL,
    NULL
);
GO