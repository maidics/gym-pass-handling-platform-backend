use FitPassDb

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
-- TYPE: GYM CREATION (Created by PendingGymEmployeeId ONLY)
-- ======================================================================================

-- 1. Submitted / High Priority (New Premium Gym)
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
    '{"Name": "Titan Fitness", "Address": {"Line1": "100 Wall St", "Line2": "Floor 1", "City": "New York", "State": "NY", "PostalCode": "10005", "CountryAlpha2": "US"}, "Status": 0, "Tier": 3, "EscalationEmail": "ceo@titanfit.com"}'
),

-- 2. Rejected / Medium Priority (Missing Details)
(
    NEWID(), 
    DATEADD(day, -3, SYSDATETIMEOFFSET()), 
    'PendingGymEmployeeId', 
    DATEADD(day, -1, SYSDATETIMEOFFSET()), 
    'AppAdminLocalhostId', 
    'Register Joe''s Garage', 
    'Small local gym setup.', 
    2, -- Medium
    0, -- GymCreation
    3, -- Rejected
    'Address validation failed. "Garage" is not a valid commercial zone.', 
    NULL, 
    '{"Name": "Joe''s Garage", "Address": {"Line1": "12 Back Alley", "Line2": null, "City": "Chicago", "State": "IL", "PostalCode": "60601", "CountryAlpha2": "US"}, "Status": 0, "Tier": 0, "EscalationEmail": "joe@garage.com"}'
),

-- 3. Error / High Priority (System Failure)
(
    NEWID(), 
    DATEADD(hour, -5, SYSDATETIMEOFFSET()), 
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
    '{"Name": "SeaSide Wellness", "Address": {"Line1": "1 Ocean Dr", "Line2": null, "City": "Miami", "State": "FL", "PostalCode": "33101", "CountryAlpha2": "US"}, "Status": 0, "Tier": 2, "EscalationEmail": "admin@seaside.com"}'
),

-- 4. Completed / Medium Priority (Successful Onboarding)
(
    NEWID(), 
    DATEADD(day, -10, SYSDATETIMEOFFSET()), 
    'PendingGymEmployeeId', 
    DATEADD(day, -9, SYSDATETIMEOFFSET()), 
    'AppAdminLocalhostId', 
    'Register Metro Flex', 
    'Mid-range city gym.', 
    2, -- Medium
    0, -- GymCreation
    1, -- Completed
    'Gym successfully provisioned and admin assigned.', 
    NULL, 
    '{"Name": "Metro Flex", "Address": {"Line1": "55 Main St", "Line2": null, "City": "Seattle", "State": "WA", "PostalCode": "98101", "CountryAlpha2": "US"}, "Status": 0, "Tier": 1, "EscalationEmail": "contact@metroflex.com"}'
),


-- ======================================================================================
-- TYPE: GYM ADMIN PROMOTION (Created by GymAdminLocalhostId ONLY)
-- ======================================================================================

-- 5. Submitted / Medium Priority (Promoting Staff)
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
    '{"GymId": "gym-001-guid", "UserIdToNominate": "GymStaffLocalhostId", "EscalationEmail": "sarah.c@gym.com"}'
),

-- 6. Cancelled / Low Priority (Mistake)
(
    NEWID(), 
    DATEADD(day, -1, SYSDATETIMEOFFSET()), 
    'GymAdminLocalhostId', 
    SYSDATETIMEOFFSET(), 
    'GymAdminLocalhostId', 
    'Promote John Smith', 
    'Nomination for admin rights.', 
    1, -- Low
    1, -- GymAdminPromotion
    2, -- Cancelled
    'Employee resigned before promotion could be processed.', 
    NULL, 
    '{"GymId": "gym-001-guid", "UserIdToNominate": "user-guid-temp", "EscalationEmail": "john.s@gym.com"}'
),

-- 7. Rejected / High Priority (Security Risk)
(
    NEWID(), 
    DATEADD(day, -2, SYSDATETIMEOFFSET()), 
    'GymAdminLocalhostId', 
    DATEADD(day, -1, SYSDATETIMEOFFSET()), 
    'AppAdminLocalhostId', 
    'Promote External Contractor', 
    'Granting admin access to IT contractor.', 
    3, -- High
    1, -- GymAdminPromotion
    3, -- Rejected
    'External contractors cannot hold Admin roles per company policy Section 4.2.', 
    NULL, 
    '{"GymId": "gym-002-guid", "UserIdToNominate": "contractor-guid", "EscalationEmail": "it@contractor.com"}'
),

-- 8. Completed / Low Priority (Routine Promotion)
(
    NEWID(), 
    DATEADD(day, -20, SYSDATETIMEOFFSET()), 
    'GymAdminLocalhostId', 
    DATEADD(day, -19, SYSDATETIMEOFFSET()), 
    'AppAdminLocalhostId', 
    'Promote Shift Lead', 
    'Night shift lead needs dashboard access.', 
    1, -- Low
    1, -- GymAdminPromotion
    1, -- Completed
    'Access granted.', 
    NULL, 
    '{"GymId": "gym-001-guid", "UserIdToNominate": "user-shift-lead", "EscalationEmail": "nightops@gym.com"}'
),


-- ======================================================================================
-- TYPE: OTHER (Created by UserId or GymStaffLocalhostId ONLY)
-- ======================================================================================

-- 9. Submitted / Low Priority (User Feedback)
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
    NULL -- Payload is null for Other
),

-- 10. Completed / Medium Priority (Gym Staff Inquiry)
(
    NEWID(), 
    DATEADD(day, -4, SYSDATETIMEOFFSET()), 
    'GymStaffLocalhostId', 
    DATEADD(day, -2, SYSDATETIMEOFFSET()), 
    'AppAdminLocalhostId', 
    'Missing Report Data', 
    'The weekly attendance report for last Tuesday is empty.', 
    2, -- Medium
    2, -- Other
    1, -- Completed
    'Data was stuck in cache. Refreshed and report is now available.', 
    NULL, 
    NULL
),

-- 11. Error / High Priority (User Payment Issue)
(
    NEWID(), 
    DATEADD(hour, -2, SYSDATETIMEOFFSET()), 
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

-- 12. Rejected / Low Priority (User Spam)
(
    NEWID(), 
    DATEADD(day, -6, SYSDATETIMEOFFSET()), 
    'UserId', 
    DATEADD(day, -5, SYSDATETIMEOFFSET()), 
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

-- 13. Submitted / High Priority (Staff Security)
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