-- =====================================================
-- PayorClaims Realistic Seed Data Script
-- Insurance Provider: BlueCross Regional Health Plan
-- Generated: 2025-02-08
-- Records per table: 200+
-- =====================================================

SET XACT_ABORT ON;
BEGIN TRANSACTION;

PRINT 'Starting data seeding for PayorClaims database...';
PRINT '';
PRINT 'Cleaning existing data...';

-- Delete in reverse dependency order to respect foreign keys
DELETE FROM dbo.export_jobs;
DELETE FROM dbo.webhook_deliveries;
DELETE FROM dbo.webhook_endpoints;
DELETE FROM dbo.hipaa_access_logs;
DELETE FROM dbo.member_consents;
DELETE FROM dbo.audit_events;
DELETE FROM dbo.claim_attachments;
DELETE FROM dbo.claim_appeals;
DELETE FROM dbo.member_insurance_policies;
DELETE FROM dbo.prior_auths;
DELETE FROM dbo.accumulators;
DELETE FROM dbo.plan_benefits;
DELETE FROM dbo.eobs;
DELETE FROM dbo.payments;
DELETE FROM dbo.provider_locations;
DELETE FROM dbo.claim_diagnoses;
DELETE FROM dbo.claim_lines;
DELETE FROM dbo.claims;
DELETE FROM dbo.coverages;
DELETE FROM dbo.providers;
DELETE FROM dbo.members;
DELETE FROM dbo.plans;
DELETE FROM dbo.diagnosis_codes;
DELETE FROM dbo.cpt_codes;
DELETE FROM dbo.adjustment_reason_codes;

PRINT 'Existing data cleaned.';
PRINT '';

-- =====================================================
-- REFERENCE TABLES
-- =====================================================

PRINT 'Inserting adjustment reason codes...';
INSERT INTO dbo.adjustment_reason_codes (Code, CodeType, Description, IsActive) VALUES 
('CO-1', 'CO', 'Deductible amount', 1),
('CO-2', 'CO', 'Coinsurance amount', 1),
('CO-3', 'CO', 'Copayment amount', 1),
('CO-4', 'CO', 'Procedures/services not covered', 1),
('CO-5', 'CO', 'Contractual obligation', 1),
('CO-6', 'CO', 'Prior authorization missing', 1),
('CO-7', 'CO', 'Coordination of benefits', 1),
('CO-8', 'CO', 'Out of network provider', 1),
('PR-1', 'PR', 'Patient responsibility - deductible', 1),
('PR-2', 'PR', 'Patient responsibility - coinsurance', 1),
('PR-3', 'PR', 'Patient responsibility - copay', 1),
('OA-1', 'OA', 'Non-covered service', 1),
('OA-2', 'OA', 'Bundled service', 1),
('PI-1', 'PI', 'Claim audit adjustment', 1),
('PI-2', 'PI', 'Overpayment recovery', 1);

PRINT 'Inserting CPT codes...';
INSERT INTO dbo.cpt_codes (CptCodeId, Description, EffectiveFrom, EffectiveTo, IsActive) VALUES 
('99201', 'Office visit, new patient, level 1', '2020-01-01', NULL, 1),
('99202', 'Office visit, new patient, level 2', '2020-01-01', NULL, 1),
('99203', 'Office visit, new patient, level 3', '2020-01-01', NULL, 1),
('99204', 'Office visit, new patient, level 4', '2020-01-01', NULL, 1),
('99205', 'Office visit, new patient, level 5', '2020-01-01', NULL, 1),
('99211', 'Office visit, established patient, level 1', '2020-01-01', NULL, 1),
('99212', 'Office visit, established patient, level 2', '2020-01-01', NULL, 1),
('99213', 'Office visit, established patient, level 3', '2020-01-01', NULL, 1),
('99214', 'Office visit, established patient, level 4', '2020-01-01', NULL, 1),
('99215', 'Office visit, established patient, level 5', '2020-01-01', NULL, 1),
('99221', 'Initial hospital care, level 1', '2020-01-01', NULL, 1),
('99222', 'Initial hospital care, level 2', '2020-01-01', NULL, 1),
('99223', 'Initial hospital care, level 3', '2020-01-01', NULL, 1),
('99231', 'Subsequent hospital care, level 1', '2020-01-01', NULL, 1),
('99232', 'Subsequent hospital care, level 2', '2020-01-01', NULL, 1),
('99233', 'Subsequent hospital care, level 3', '2020-01-01', NULL, 1),
('99281', 'Emergency department visit, level 1', '2020-01-01', NULL, 1),
('99282', 'Emergency department visit, level 2', '2020-01-01', NULL, 1),
('99283', 'Emergency department visit, level 3', '2020-01-01', NULL, 1),
('99284', 'Emergency department visit, level 4', '2020-01-01', NULL, 1),
('99285', 'Emergency department visit, level 5', '2020-01-01', NULL, 1),
('99381', 'Preventive visit, new patient, under 1 year', '2020-01-01', NULL, 1),
('99382', 'Preventive visit, new patient, 1-4 years', '2020-01-01', NULL, 1),
('99383', 'Preventive visit, new patient, 5-11 years', '2020-01-01', NULL, 1),
('99384', 'Preventive visit, new patient, 12-17 years', '2020-01-01', NULL, 1),
('99385', 'Preventive visit, new patient, 18-39 years', '2020-01-01', NULL, 1),
('99386', 'Preventive visit, new patient, 40-64 years', '2020-01-01', NULL, 1),
('99387', 'Preventive visit, new patient, 65+ years', '2020-01-01', NULL, 1),
('99391', 'Preventive visit, established patient, under 1 year', '2020-01-01', NULL, 1),
('99392', 'Preventive visit, established patient, 1-4 years', '2020-01-01', NULL, 1),
('99393', 'Preventive visit, established patient, 5-11 years', '2020-01-01', NULL, 1),
('99394', 'Preventive visit, established patient, 12-17 years', '2020-01-01', NULL, 1),
('99395', 'Preventive visit, established patient, 18-39 years', '2020-01-01', NULL, 1),
('99396', 'Preventive visit, established patient, 40-64 years', '2020-01-01', NULL, 1),
('99397', 'Preventive visit, established patient, 65+ years', '2020-01-01', NULL, 1),
('80053', 'Comprehensive metabolic panel', '2020-01-01', NULL, 1),
('85025', 'Complete blood count with differential', '2020-01-01', NULL, 1),
('80061', 'Lipid panel', '2020-01-01', NULL, 1),
('83036', 'Hemoglobin A1C', '2020-01-01', NULL, 1),
('84443', 'Thyroid stimulating hormone', '2020-01-01', NULL, 1),
('71045', 'Chest X-ray, single view', '2020-01-01', NULL, 1),
('71046', 'Chest X-ray, 2 views', '2020-01-01', NULL, 1),
('70450', 'CT head without contrast', '2020-01-01', NULL, 1),
('70553', 'MRI brain with and without contrast', '2020-01-01', NULL, 1),
('93000', 'Electrocardiogram, complete', '2020-01-01', NULL, 1),
('93306', 'Echocardiography, complete', '2020-01-01', NULL, 1),
('45378', 'Colonoscopy', '2020-01-01', NULL, 1),
('43239', 'Upper GI endoscopy', '2020-01-01', NULL, 1),
('29881', 'Knee arthroscopy/surgery', '2020-01-01', NULL, 1),
('64483', 'Epidural injection, lumbar/sacral', '2020-01-01', NULL, 1),
('97110', 'Physical therapy, therapeutic exercises', '2020-01-01', NULL, 1);

PRINT 'Inserting diagnosis codes...';
INSERT INTO dbo.diagnosis_codes (CodeSystem, Code, Description, EffectiveFrom, EffectiveTo, IsActive) VALUES 
('ICD10', 'Z00.00', 'Encounter for general adult medical exam without abnormal findings', '2020-01-01', NULL, 1),
('ICD10', 'Z00.01', 'Encounter for general adult medical exam with abnormal findings', '2020-01-01', NULL, 1),
('ICD10', 'I10', 'Essential (primary) hypertension', '2020-01-01', NULL, 1),
('ICD10', 'E11.9', 'Type 2 diabetes mellitus without complications', '2020-01-01', NULL, 1),
('ICD10', 'E11.65', 'Type 2 diabetes mellitus with hyperglycemia', '2020-01-01', NULL, 1),
('ICD10', 'E78.5', 'Hyperlipidemia, unspecified', '2020-01-01', NULL, 1),
('ICD10', 'J06.9', 'Acute upper respiratory infection, unspecified', '2020-01-01', NULL, 1),
('ICD10', 'J45.909', 'Unspecified asthma, uncomplicated', '2020-01-01', NULL, 1),
('ICD10', 'M54.5', 'Low back pain', '2020-01-01', NULL, 1),
('ICD10', 'M25.561', 'Pain in right knee', '2020-01-01', NULL, 1),
('ICD10', 'M25.562', 'Pain in left knee', '2020-01-01', NULL, 1),
('ICD10', 'R10.9', 'Unspecified abdominal pain', '2020-01-01', NULL, 1),
('ICD10', 'R51', 'Headache', '2020-01-01', NULL, 1),
('ICD10', 'R50.9', 'Fever, unspecified', '2020-01-01', NULL, 1),
('ICD10', 'F41.9', 'Anxiety disorder, unspecified', '2020-01-01', NULL, 1),
('ICD10', 'F33.9', 'Major depressive disorder, recurrent, unspecified', '2020-01-01', NULL, 1),
('ICD10', 'G43.909', 'Migraine, unspecified, not intractable, without status migrainosus', '2020-01-01', NULL, 1),
('ICD10', 'N39.0', 'Urinary tract infection, site not specified', '2020-01-01', NULL, 1),
('ICD10', 'K21.9', 'Gastro-esophageal reflux disease without esophagitis', '2020-01-01', NULL, 1),
('ICD10', 'E66.9', 'Obesity, unspecified', '2020-01-01', NULL, 1),
('ICD10', 'Z23', 'Encounter for immunization', '2020-01-01', NULL, 1),
('ICD10', 'Z79.899', 'Other long term (current) drug therapy', '2020-01-01', NULL, 1),
('ICD10', 'I25.10', 'Atherosclerotic heart disease without angina pectoris', '2020-01-01', NULL, 1),
('ICD10', 'I50.9', 'Heart failure, unspecified', '2020-01-01', NULL, 1),
('ICD10', 'J44.9', 'Chronic obstructive pulmonary disease, unspecified', '2020-01-01', NULL, 1),
('ICD10', 'N18.3', 'Chronic kidney disease, stage 3', '2020-01-01', NULL, 1),
('ICD10', 'C50.919', 'Malignant neoplasm of unspecified site of unspecified female breast', '2020-01-01', NULL, 1),
('ICD10', 'C34.90', 'Malignant neoplasm of unspecified part of unspecified bronchus or lung', '2020-01-01', NULL, 1),
('ICD10', 'S82.001A', 'Unspecified fracture of right patella, initial encounter', '2020-01-01', NULL, 1),
('ICD10', 'S06.0X0A', 'Concussion without loss of consciousness, initial encounter', '2020-01-01', NULL, 1);

-- =====================================================
-- PLANS (250 records)
-- =====================================================

PRINT 'Inserting insurance plans...';

DECLARE @planCounter INT = 1;
WHILE @planCounter <= 250
BEGIN
    DECLARE @planId UNIQUEIDENTIFIER = NEWID();
    DECLARE @planCode VARCHAR(50);
    DECLARE @planName VARCHAR(255);
    DECLARE @planYear INT;
    DECLARE @networkType VARCHAR(20);
    DECLARE @metalTier VARCHAR(20);
    
    -- Rotate through plan variations
    SET @planYear = CASE 
        WHEN @planCounter % 3 = 0 THEN 2023
        WHEN @planCounter % 3 = 1 THEN 2024
        ELSE 2025
    END;
    
    SET @networkType = CASE (@planCounter % 4)
        WHEN 0 THEN 'PPO'
        WHEN 1 THEN 'HMO'
        WHEN 2 THEN 'EPO'
        ELSE 'POS'
    END;
    
    SET @metalTier = CASE (@planCounter % 5)
        WHEN 0 THEN 'Bronze'
        WHEN 1 THEN 'Silver'
        WHEN 2 THEN 'Gold'
        WHEN 3 THEN 'Platinum'
        ELSE 'Catastrophic'
    END;
    
    SET @planCode = 'BC-' + 
        CASE @networkType
            WHEN 'PPO' THEN 'P'
            WHEN 'HMO' THEN 'H'
            WHEN 'EPO' THEN 'E'
            ELSE 'S'
        END + 
        CASE @metalTier
            WHEN 'Bronze' THEN 'B'
            WHEN 'Silver' THEN 'S'
            WHEN 'Gold' THEN 'G'
            WHEN 'Platinum' THEN 'P'
            ELSE 'C'
        END + 
        RIGHT(CAST(@planYear AS VARCHAR(4)), 2) + 
        RIGHT('000' + CAST(@planCounter AS VARCHAR(3)), 3);
    
    SET @planName = CASE (@planCounter % 10)
        WHEN 0 THEN 'BlueCross Regional ' + @metalTier + ' ' + @networkType
        WHEN 1 THEN 'HealthFirst ' + @metalTier + ' ' + @networkType + ' Plan'
        WHEN 2 THEN 'Premier Care ' + @metalTier + ' ' + @networkType
        WHEN 3 THEN 'Family Shield ' + @metalTier + ' ' + @networkType
        WHEN 4 THEN 'Advantage Health ' + @metalTier + ' ' + @networkType
        WHEN 5 THEN 'Select Care ' + @metalTier + ' ' + @networkType
        WHEN 6 THEN 'Essential Health ' + @metalTier + ' ' + @networkType
        WHEN 7 THEN 'Preferred Choice ' + @metalTier + ' ' + @networkType
        WHEN 8 THEN 'Total Care ' + @metalTier + ' ' + @networkType
        ELSE 'Community Health ' + @metalTier + ' ' + @networkType
    END;
    
    INSERT INTO dbo.plans (Id, PlanCode, Name, Year, NetworkType, MetalTier, IsActive, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
    VALUES (@planId, @planCode, @planName, @planYear, @networkType, @metalTier, 1, GETUTCDATE(), GETUTCDATE(), 0, NULL);
    
    SET @planCounter = @planCounter + 1;
END;

-- =====================================================
-- MEMBERS (300 records)
-- =====================================================

PRINT 'Inserting members...';

DECLARE @memberCounter INT = 1;
WHILE @memberCounter <= 300
BEGIN
    DECLARE @memberId UNIQUEIDENTIFIER = NEWID();
    DECLARE @firstName VARCHAR(100);
    DECLARE @lastName VARCHAR(100);
    DECLARE @dob DATE;
    DECLARE @gender CHAR(1);
    DECLARE @status VARCHAR(20);
    DECLARE @externalMemberNumber VARCHAR(50);
    
    -- Realistic first names
    SET @firstName = CASE (@memberCounter % 30)
        WHEN 0 THEN 'James'
        WHEN 1 THEN 'Mary'
        WHEN 2 THEN 'John'
        WHEN 3 THEN 'Patricia'
        WHEN 4 THEN 'Robert'
        WHEN 5 THEN 'Jennifer'
        WHEN 6 THEN 'Michael'
        WHEN 7 THEN 'Linda'
        WHEN 8 THEN 'William'
        WHEN 9 THEN 'Barbara'
        WHEN 10 THEN 'David'
        WHEN 11 THEN 'Elizabeth'
        WHEN 12 THEN 'Richard'
        WHEN 13 THEN 'Susan'
        WHEN 14 THEN 'Joseph'
        WHEN 15 THEN 'Jessica'
        WHEN 16 THEN 'Thomas'
        WHEN 17 THEN 'Sarah'
        WHEN 18 THEN 'Charles'
        WHEN 19 THEN 'Karen'
        WHEN 20 THEN 'Christopher'
        WHEN 21 THEN 'Nancy'
        WHEN 22 THEN 'Daniel'
        WHEN 23 THEN 'Lisa'
        WHEN 24 THEN 'Matthew'
        WHEN 25 THEN 'Betty'
        WHEN 26 THEN 'Anthony'
        WHEN 27 THEN 'Margaret'
        WHEN 28 THEN 'Donald'
        ELSE 'Ashley'
    END;
    
    -- Realistic last names
    SET @lastName = CASE (@memberCounter % 30)
        WHEN 0 THEN 'Smith'
        WHEN 1 THEN 'Johnson'
        WHEN 2 THEN 'Williams'
        WHEN 3 THEN 'Brown'
        WHEN 4 THEN 'Jones'
        WHEN 5 THEN 'Garcia'
        WHEN 6 THEN 'Miller'
        WHEN 7 THEN 'Davis'
        WHEN 8 THEN 'Rodriguez'
        WHEN 9 THEN 'Martinez'
        WHEN 10 THEN 'Hernandez'
        WHEN 11 THEN 'Lopez'
        WHEN 12 THEN 'Gonzalez'
        WHEN 13 THEN 'Wilson'
        WHEN 14 THEN 'Anderson'
        WHEN 15 THEN 'Thomas'
        WHEN 16 THEN 'Taylor'
        WHEN 17 THEN 'Moore'
        WHEN 18 THEN 'Jackson'
        WHEN 19 THEN 'Martin'
        WHEN 20 THEN 'Lee'
        WHEN 21 THEN 'Perez'
        WHEN 22 THEN 'Thompson'
        WHEN 23 THEN 'White'
        WHEN 24 THEN 'Harris'
        WHEN 25 THEN 'Sanchez'
        WHEN 26 THEN 'Clark'
        WHEN 27 THEN 'Ramirez'
        WHEN 28 THEN 'Lewis'
        ELSE 'Robinson'
    END;
    
    -- Random DOB between 1945 and 2015
    SET @dob = DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 25550), '2015-01-01');
    
    SET @gender = CASE WHEN @memberCounter % 2 = 0 THEN 'M' ELSE 'F' END;
    
    SET @status = CASE (@memberCounter % 10)
        WHEN 0 THEN 'Pending'
        WHEN 9 THEN 'Terminated'
        ELSE 'Active'
    END;
    
    SET @externalMemberNumber = 'BCRHP' + RIGHT('00000000' + CAST(@memberCounter AS VARCHAR(8)), 8);
    
    INSERT INTO dbo.members (Id, ExternalMemberNumber, FirstName, LastName, Dob, Gender, Status, EmailEncrypted, PhoneEncrypted, SsnEncrypted, SsnPlain, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
    VALUES (@memberId, @externalMemberNumber, @firstName, @lastName, @dob, @gender, @status, NULL, NULL, NULL, NULL, GETUTCDATE(), GETUTCDATE(), 0, NULL);
    
    SET @memberCounter = @memberCounter + 1;
END;

-- =====================================================
-- PROVIDERS (250 records)
-- =====================================================

PRINT 'Inserting providers...';

DECLARE @providerCounter INT = 1;
WHILE @providerCounter <= 250
BEGIN
    DECLARE @providerId UNIQUEIDENTIFIER = NEWID();
    DECLARE @npi VARCHAR(10);
    DECLARE @providerName VARCHAR(255);
    DECLARE @providerType VARCHAR(50);
    DECLARE @specialty VARCHAR(100);
    DECLARE @providerStatus VARCHAR(20);
    
    SET @npi = '1' + RIGHT('000000000' + CAST(1000000000 + @providerCounter AS VARCHAR(9)), 9);
    
    SET @providerType = CASE (@providerCounter % 3)
        WHEN 0 THEN 'Individual'
        WHEN 1 THEN 'Facility'
        ELSE 'Group'
    END;
    
    SET @specialty = CASE (@providerCounter % 20)
        WHEN 0 THEN 'Family Medicine'
        WHEN 1 THEN 'Internal Medicine'
        WHEN 2 THEN 'Pediatrics'
        WHEN 3 THEN 'Cardiology'
        WHEN 4 THEN 'Orthopedic Surgery'
        WHEN 5 THEN 'General Surgery'
        WHEN 6 THEN 'Obstetrics & Gynecology'
        WHEN 7 THEN 'Psychiatry'
        WHEN 8 THEN 'Dermatology'
        WHEN 9 THEN 'Ophthalmology'
        WHEN 10 THEN 'Emergency Medicine'
        WHEN 11 THEN 'Radiology'
        WHEN 12 THEN 'Anesthesiology'
        WHEN 13 THEN 'Neurology'
        WHEN 14 THEN 'Oncology'
        WHEN 15 THEN 'Gastroenterology'
        WHEN 16 THEN 'Urology'
        WHEN 17 THEN 'Pulmonology'
        WHEN 18 THEN 'Nephrology'
        ELSE 'Endocrinology'
    END;
    
    IF @providerType = 'Individual'
    BEGIN
        SET @providerName = CASE (@providerCounter % 15)
            WHEN 0 THEN 'Dr. Sarah Chen, MD'
            WHEN 1 THEN 'Dr. Michael Patel, DO'
            WHEN 2 THEN 'Dr. Emily Rodriguez, MD'
            WHEN 3 THEN 'Dr. James Thompson, MD'
            WHEN 4 THEN 'Dr. Lisa Johnson, DO'
            WHEN 5 THEN 'Dr. David Kim, MD'
            WHEN 6 THEN 'Dr. Maria Garcia, MD'
            WHEN 7 THEN 'Dr. Robert Anderson, DO'
            WHEN 8 THEN 'Dr. Jennifer Lee, MD'
            WHEN 9 THEN 'Dr. William Martinez, MD'
            WHEN 10 THEN 'Dr. Amanda Williams, DO'
            WHEN 11 THEN 'Dr. Christopher Brown, MD'
            WHEN 12 THEN 'Dr. Michelle Davis, MD'
            WHEN 13 THEN 'Dr. Kevin Wilson, DO'
            ELSE 'Dr. Laura Taylor, MD'
        END;
    END
    ELSE IF @providerType = 'Facility'
    BEGIN
        SET @providerName = CASE (@providerCounter % 15)
            WHEN 0 THEN 'Central Regional Medical Center'
            WHEN 1 THEN 'Metropolitan General Hospital'
            WHEN 2 THEN 'Community Health Center'
            WHEN 3 THEN 'St. Mary''s Hospital'
            WHEN 4 THEN 'Valley View Medical Center'
            WHEN 5 THEN 'Riverside Regional Hospital'
            WHEN 6 THEN 'Lakeside Medical Complex'
            WHEN 7 THEN 'Mountain View Healthcare'
            WHEN 8 THEN 'Coastal Regional Hospital'
            WHEN 9 THEN 'University Medical Center'
            WHEN 10 THEN 'Mercy General Hospital'
            WHEN 11 THEN 'Summit Health System'
            WHEN 12 THEN 'Park Avenue Medical Center'
            WHEN 13 THEN 'Sunrise Regional Hospital'
            ELSE 'Heritage Community Hospital'
        END;
    END
    ELSE
    BEGIN
        SET @providerName = CASE (@providerCounter % 15)
            WHEN 0 THEN 'Advanced Medical Associates'
            WHEN 1 THEN 'Premier Physicians Group'
            WHEN 2 THEN 'Comprehensive Care Clinic'
            WHEN 3 THEN 'Regional Health Partners'
            WHEN 4 THEN 'Integrated Medical Group'
            WHEN 5 THEN 'Metropolitan Health Associates'
            WHEN 6 THEN 'Community Medical Group'
            WHEN 7 THEN 'Family Care Physicians'
            WHEN 8 THEN 'United Healthcare Partners'
            WHEN 9 THEN 'Preferred Medical Associates'
            WHEN 10 THEN 'Primary Care Alliance'
            WHEN 11 THEN 'Specialty Physicians Network'
            WHEN 12 THEN 'Regional Care Collaborative'
            WHEN 13 THEN 'Health Partners Medical Group'
            ELSE 'Valley Medical Associates'
        END;
    END;
    
    SET @providerStatus = CASE (@providerCounter % 15)
        WHEN 14 THEN 'Suspended'
        WHEN 13 THEN 'Terminated'
        ELSE 'Active'
    END;
    
    INSERT INTO dbo.providers (Id, Npi, Name, ProviderType, Specialty, TaxId, ProviderStatus, TerminationReason, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
    VALUES (@providerId, @npi, @providerName, @providerType, @specialty, NULL, @providerStatus, NULL, GETUTCDATE(), GETUTCDATE(), 0, NULL);
    
    SET @providerCounter = @providerCounter + 1;
END;

-- =====================================================
-- COVERAGES (300 records - one per active member)
-- =====================================================

PRINT 'Inserting coverages...';

INSERT INTO dbo.coverages (Id, PlanId, MemberId, CoverageStatus, StartDate, EndDate, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
SELECT 
    NEWID() AS Id,
    p.Id AS PlanId,
    m.Id AS MemberId,
    CASE 
        WHEN m.Status = 'Active' THEN 'Active'
        WHEN m.Status = 'Terminated' THEN 'Terminated'
        ELSE 'Pending'
    END AS CoverageStatus,
    DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 730), GETUTCDATE()) AS StartDate,
    CASE 
        WHEN m.Status = 'Terminated' THEN DATEADD(DAY, ABS(CHECKSUM(NEWID()) % 365), GETUTCDATE())
        ELSE NULL
    END AS EndDate,
    GETUTCDATE() AS CreatedAt,
    GETUTCDATE() AS UpdatedAt,
    0 AS IsDeleted,
    NULL AS DeletedAt
FROM 
    (SELECT Id, Status, ROW_NUMBER() OVER (ORDER BY Id) AS RowNum FROM dbo.members) m
    CROSS APPLY (
        SELECT TOP 1 Id 
        FROM dbo.plans 
        ORDER BY NEWID()
    ) p;

-- =====================================================
-- CLAIMS (500 records)
-- =====================================================

PRINT 'Inserting claims...';

DECLARE @claimCounter INT = 1;
WHILE @claimCounter <= 500
BEGIN
    DECLARE @claimId UNIQUEIDENTIFIER = NEWID();
    DECLARE @claimNumber VARCHAR(50);
    DECLARE @claimMemberId UNIQUEIDENTIFIER;
    DECLARE @claimProviderId UNIQUEIDENTIFIER;
    DECLARE @claimCoverageId UNIQUEIDENTIFIER;
    DECLARE @claimStatus VARCHAR(50);
    DECLARE @serviceFromDate DATE;
    DECLARE @serviceToDate DATE;
    DECLARE @receivedDate DATE;
    DECLARE @totalBilled DECIMAL(18,2);
    DECLARE @totalAllowed DECIMAL(18,2);
    DECLARE @totalPaid DECIMAL(18,2);
    
    SET @claimNumber = 'CLM' + RIGHT('00000000' + CAST(@claimCounter AS VARCHAR(8)), 8);
    
    -- Select random member
    SELECT TOP 1 @claimMemberId = Id FROM dbo.members WHERE Status = 'Active' ORDER BY NEWID();
    
    -- Select random provider
    SELECT TOP 1 @claimProviderId = Id FROM dbo.providers WHERE ProviderStatus = 'Active' ORDER BY NEWID();
    
    -- Get coverage for member
    SELECT TOP 1 @claimCoverageId = Id FROM dbo.coverages WHERE MemberId = @claimMemberId AND CoverageStatus = 'Active' ORDER BY NEWID();
    
    -- Random service date in last 12 months
    SET @serviceFromDate = DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 365), GETUTCDATE());
    SET @serviceToDate = DATEADD(DAY, ABS(CHECKSUM(NEWID()) % 3), @serviceFromDate);
    SET @receivedDate = DATEADD(DAY, ABS(CHECKSUM(NEWID()) % 30), @serviceToDate);
    
    -- Claim status distribution
    SET @claimStatus = CASE (@claimCounter % 10)
        WHEN 0 THEN 'Received'
        WHEN 1 THEN 'In Review'
        WHEN 2 THEN 'Pending Additional Info'
        WHEN 3 THEN 'Approved'
        WHEN 4 THEN 'Paid'
        WHEN 5 THEN 'Paid'
        WHEN 6 THEN 'Paid'
        WHEN 7 THEN 'Denied'
        WHEN 8 THEN 'Paid'
        ELSE 'Paid'
    END;
    
    -- Random claim amounts
    SET @totalBilled = CAST((100 + (ABS(CHECKSUM(NEWID())) % 5000)) AS DECIMAL(18,2));
    SET @totalAllowed = @totalBilled * (0.60 + (ABS(CHECKSUM(NEWID())) % 35) / 100.0);
    SET @totalPaid = CASE 
        WHEN @claimStatus IN ('Paid', 'Approved') THEN @totalAllowed * (0.70 + (ABS(CHECKSUM(NEWID())) % 25) / 100.0)
        WHEN @claimStatus = 'Denied' THEN 0
        ELSE 0
    END;
    
    INSERT INTO dbo.claims (Id, ClaimNumber, MemberId, ProviderId, CoverageId, Status, ServiceFromDate, ServiceToDate, ReceivedDate, TotalBilled, TotalAllowed, TotalPaid, Currency, DuplicateFingerprint, SubmittedByActorType, SubmittedByActorId, IdempotencyKey, SourceSystem, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
    VALUES (@claimId, @claimNumber, @claimMemberId, @claimProviderId, @claimCoverageId, @claimStatus, @serviceFromDate, @serviceToDate, @receivedDate, @totalBilled, @totalAllowed, @totalPaid, 'USD', CONVERT(VARCHAR(50), NEWID()), 'Provider', @claimProviderId, NULL, 'EDI', GETUTCDATE(), GETUTCDATE(), 0, NULL);
    
    SET @claimCounter = @claimCounter + 1;
END;

-- =====================================================
-- CLAIM LINES (750 records - 1-3 lines per claim)
-- =====================================================

PRINT 'Inserting claim lines...';

DECLARE @lineClaimId UNIQUEIDENTIFIER;
DECLARE @lineCursor CURSOR;

SET @lineCursor = CURSOR FOR
    SELECT Id FROM dbo.claims;

OPEN @lineCursor;
FETCH NEXT FROM @lineCursor INTO @lineClaimId;

DECLARE @lineCounter INT = 1;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @numLines INT = 1 + (ABS(CHECKSUM(NEWID())) % 3); -- 1-3 lines per claim
    DECLARE @lineNum INT = 1;
    
    WHILE @lineNum <= @numLines
    BEGIN
        DECLARE @lineCptCode VARCHAR(10);
        DECLARE @lineUnits INT;
        DECLARE @lineBilledAmount DECIMAL(18,2);
        DECLARE @lineAllowedAmount DECIMAL(18,2);
        DECLARE @linePaidAmount DECIMAL(18,2);
        DECLARE @lineStatus VARCHAR(50);
        
        -- Select random CPT code
        SELECT TOP 1 @lineCptCode = CptCodeId FROM dbo.cpt_codes WHERE IsActive = 1 ORDER BY NEWID();
        
        SET @lineUnits = 1 + (ABS(CHECKSUM(NEWID())) % 3);
        SET @lineBilledAmount = CAST((50 + (ABS(CHECKSUM(NEWID())) % 2000)) AS DECIMAL(18,2));
        SET @lineAllowedAmount = @lineBilledAmount * (0.60 + (ABS(CHECKSUM(NEWID())) % 35) / 100.0);
        
        SET @lineStatus = CASE (ABS(CHECKSUM(NEWID())) % 10)
            WHEN 0 THEN 'Pending'
            WHEN 1 THEN 'Denied'
            WHEN 2 THEN 'Approved'
            ELSE 'Paid'
        END;
        
        SET @linePaidAmount = CASE 
            WHEN @lineStatus IN ('Paid', 'Approved') THEN @lineAllowedAmount * (0.70 + (ABS(CHECKSUM(NEWID())) % 25) / 100.0)
            ELSE 0
        END;
        
        INSERT INTO dbo.claim_lines (Id, ClaimId, LineNumber, CptCode, Units, BilledAmount, AllowedAmount, PaidAmount, LineStatus, DenialReasonCode, DenialReasonText, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
        VALUES (NEWID(), @lineClaimId, @lineNum, @lineCptCode, @lineUnits, @lineBilledAmount, @lineAllowedAmount, @linePaidAmount, @lineStatus, NULL, NULL, GETUTCDATE(), GETUTCDATE(), 0, NULL);
        
        SET @lineNum = @lineNum + 1;
        SET @lineCounter = @lineCounter + 1;
    END;
    
    FETCH NEXT FROM @lineCursor INTO @lineClaimId;
END;

CLOSE @lineCursor;
DEALLOCATE @lineCursor;

-- =====================================================
-- CLAIM DIAGNOSES (1000 records - 2-4 per claim)
-- =====================================================

PRINT 'Inserting claim diagnoses...';

DECLARE @diagClaimId UNIQUEIDENTIFIER;
DECLARE @diagCursor CURSOR;

SET @diagCursor = CURSOR FOR
    SELECT Id FROM dbo.claims;

OPEN @diagCursor;
FETCH NEXT FROM @diagCursor INTO @diagClaimId;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @numDiags INT = 1 + (ABS(CHECKSUM(NEWID())) % 3); -- 1-3 diagnoses per claim
    DECLARE @diagNum INT = 1;
    
    WHILE @diagNum <= @numDiags
    BEGIN
        DECLARE @diagCode VARCHAR(20);
        DECLARE @diagCodeSystem VARCHAR(20);
        
        SELECT TOP 1 @diagCode = Code, @diagCodeSystem = CodeSystem 
        FROM dbo.diagnosis_codes 
        WHERE IsActive = 1 
        ORDER BY NEWID();
        
        INSERT INTO dbo.claim_diagnoses (Id, ClaimId, CodeSystem, Code, LineNumber, IsPrimary, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
        VALUES (NEWID(), @diagClaimId, @diagCodeSystem, @diagCode, @diagNum, CASE WHEN @diagNum = 1 THEN 1 ELSE 0 END, GETUTCDATE(), GETUTCDATE(), 0, NULL);
        
        SET @diagNum = @diagNum + 1;
    END;
    
    FETCH NEXT FROM @diagCursor INTO @diagClaimId;
END;

CLOSE @diagCursor;
DEALLOCATE @diagCursor;

-- =====================================================
-- PROVIDER LOCATIONS (250 records - one per provider)
-- =====================================================

PRINT 'Inserting provider locations...';

DECLARE @locProviderId UNIQUEIDENTIFIER;
DECLARE @locCursor CURSOR;

SET @locCursor = CURSOR FOR
    SELECT Id FROM dbo.providers;

OPEN @locCursor;
FETCH NEXT FROM @locCursor INTO @locProviderId;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @addressLine1 VARCHAR(255);
    DECLARE @city VARCHAR(100);
    DECLARE @state VARCHAR(2);
    DECLARE @zip VARCHAR(10);
    
    SET @addressLine1 = CAST((100 + (ABS(CHECKSUM(NEWID())) % 9900)) AS VARCHAR(10)) + ' ' +
        CASE (ABS(CHECKSUM(NEWID())) % 15)
            WHEN 0 THEN 'Main Street'
            WHEN 1 THEN 'Oak Avenue'
            WHEN 2 THEN 'Maple Drive'
            WHEN 3 THEN 'Park Boulevard'
            WHEN 4 THEN 'Washington Street'
            WHEN 5 THEN 'Cedar Lane'
            WHEN 6 THEN 'Elm Street'
            WHEN 7 THEN 'Pine Road'
            WHEN 8 THEN 'Broadway'
            WHEN 9 THEN 'Highland Avenue'
            WHEN 10 THEN 'Lake Drive'
            WHEN 11 THEN 'Sunset Boulevard'
            WHEN 12 THEN 'River Road'
            WHEN 13 THEN 'Mountain View Drive'
            ELSE 'Valley Way'
        END;
    
    SET @city = CASE (ABS(CHECKSUM(NEWID())) % 20)
        WHEN 0 THEN 'Springfield'
        WHEN 1 THEN 'Riverside'
        WHEN 2 THEN 'Franklin'
        WHEN 3 THEN 'Madison'
        WHEN 4 THEN 'Georgetown'
        WHEN 5 THEN 'Arlington'
        WHEN 6 THEN 'Clayton'
        WHEN 7 THEN 'Manchester'
        WHEN 8 THEN 'Salem'
        WHEN 9 THEN 'Oakland'
        WHEN 10 THEN 'Bristol'
        WHEN 11 THEN 'Fairview'
        WHEN 12 THEN 'Ashland'
        WHEN 13 THEN 'Burlington'
        WHEN 14 THEN 'Dover'
        WHEN 15 THEN 'Hudson'
        WHEN 16 THEN 'Jackson'
        WHEN 17 THEN 'Marion'
        WHEN 18 THEN 'Newport'
        ELSE 'Oxford'
    END;
    
    SET @state = CASE (ABS(CHECKSUM(NEWID())) % 10)
        WHEN 0 THEN 'CA'
        WHEN 1 THEN 'TX'
        WHEN 2 THEN 'FL'
        WHEN 3 THEN 'NY'
        WHEN 4 THEN 'PA'
        WHEN 5 THEN 'IL'
        WHEN 6 THEN 'OH'
        WHEN 7 THEN 'GA'
        WHEN 8 THEN 'NC'
        ELSE 'MI'
    END;
    
    SET @zip = CAST((10000 + (ABS(CHECKSUM(NEWID())) % 89999)) AS VARCHAR(5));
    
    INSERT INTO dbo.provider_locations (Id, ProviderId, AddressLine1, AddressLine2, City, State, Zip, Phone, IsPrimary, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
    VALUES (NEWID(), @locProviderId, @addressLine1, NULL, @city, @state, @zip, NULL, 1, GETUTCDATE(), GETUTCDATE(), 0, NULL);
    
    FETCH NEXT FROM @locCursor INTO @locProviderId;
END;

CLOSE @locCursor;
DEALLOCATE @locCursor;

-- =====================================================
-- PAYMENTS (300 records - for paid claims)
-- =====================================================

PRINT 'Inserting payments...';

INSERT INTO dbo.payments (Id, ClaimId, Amount, Method, PaymentDate, ReferenceNumber, IdempotencyKey, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
SELECT 
    NEWID() AS Id,
    c.Id AS ClaimId,
    c.TotalPaid AS Amount,
    CASE (ABS(CHECKSUM(NEWID())) % 4)
        WHEN 0 THEN 'Check'
        WHEN 1 THEN 'ACH'
        WHEN 2 THEN 'Wire'
        ELSE 'EFT'
    END AS Method,
    DATEADD(DAY, 5 + (ABS(CHECKSUM(NEWID())) % 15), c.ReceivedDate) AS PaymentDate,
    'PAY' + RIGHT('00000000' + CAST(ROW_NUMBER() OVER (ORDER BY c.Id) AS VARCHAR(8)), 8) AS ReferenceNumber,
    NULL AS IdempotencyKey,
    GETUTCDATE() AS CreatedAt,
    GETUTCDATE() AS UpdatedAt,
    0 AS IsDeleted,
    NULL AS DeletedAt
FROM 
    dbo.claims c
WHERE 
    c.Status = 'Paid' AND c.TotalPaid > 0;

-- =====================================================
-- EOBS (300 records - for processed claims)
-- =====================================================

PRINT 'Inserting EOBs...';

INSERT INTO dbo.eobs (Id, ClaimId, EobNumber, DocumentStorageKey, DocumentSha256, GeneratedAt, DeliveryMethod, DeliveryStatus, DeliveredAt, FailureReason, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
SELECT 
    NEWID() AS Id,
    c.Id AS ClaimId,
    'EOB' + RIGHT('00000000' + CAST(ROW_NUMBER() OVER (ORDER BY c.Id) AS VARCHAR(8)), 8) AS EobNumber,
    'eob/' + CAST(c.Id AS VARCHAR(50)) AS DocumentStorageKey,
    CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(c.Id AS VARCHAR(50))), 2) AS DocumentSha256,
    DATEADD(DAY, 3, c.ReceivedDate) AS GeneratedAt,
    CASE (ABS(CHECKSUM(NEWID())) % 3)
        WHEN 0 THEN 'Portal'
        WHEN 1 THEN 'Email'
        ELSE 'Mail'
    END AS DeliveryMethod,
    CASE (ABS(CHECKSUM(NEWID())) % 10)
        WHEN 0 THEN 'Failed'
        WHEN 1 THEN 'Pending'
        ELSE 'Delivered'
    END AS DeliveryStatus,
    DATEADD(DAY, 4, c.ReceivedDate) AS DeliveredAt,
    NULL AS FailureReason,
    GETUTCDATE() AS CreatedAt,
    GETUTCDATE() AS UpdatedAt,
    0 AS IsDeleted,
    NULL AS DeletedAt
FROM 
    dbo.claims c
WHERE 
    c.Status IN ('Paid', 'Denied', 'Approved');

-- =====================================================
-- PLAN BENEFITS (500 records)
-- =====================================================

PRINT 'Inserting plan benefits...';

DECLARE @benefitPlanId UNIQUEIDENTIFIER;
DECLARE @benefitCursor CURSOR;

SET @benefitCursor = CURSOR FOR
    SELECT Id FROM dbo.plans;

OPEN @benefitCursor;
FETCH NEXT FROM @benefitCursor INTO @benefitPlanId;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Office Visit benefit
    INSERT INTO dbo.plan_benefits (Id, PlanId, BenefitVersion, Category, Network, CoverageLevel, Period, CopayAmount, CoinsurancePercent, DeductibleApplies, EffectiveFrom, EffectiveTo, MaxVisits, Notes, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
    VALUES (NEWID(), @benefitPlanId, 1, 'Office Visit', 'IN', 'Individual', 'Calendar', 20.00 + (ABS(CHECKSUM(NEWID())) % 50), NULL, 0, '2025-01-01', NULL, NULL, NULL, GETUTCDATE(), GETUTCDATE(), 0, NULL);
    
    -- Specialist Visit benefit
    INSERT INTO dbo.plan_benefits (Id, PlanId, BenefitVersion, Category, Network, CoverageLevel, Period, CopayAmount, CoinsurancePercent, DeductibleApplies, EffectiveFrom, EffectiveTo, MaxVisits, Notes, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
    VALUES (NEWID(), @benefitPlanId, 1, 'Specialist Visit', 'IN', 'Individual', 'Calendar', 40.00 + (ABS(CHECKSUM(NEWID())) % 60), NULL, 0, '2025-01-01', NULL, NULL, NULL, GETUTCDATE(), GETUTCDATE(), 0, NULL);
    
    -- ER Visit benefit
    INSERT INTO dbo.plan_benefits (Id, PlanId, BenefitVersion, Category, Network, CoverageLevel, Period, CopayAmount, CoinsurancePercent, DeductibleApplies, EffectiveFrom, EffectiveTo, MaxVisits, Notes, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
    VALUES (NEWID(), @benefitPlanId, 1, 'Emergency Room', 'IN', 'Individual', 'Calendar', 250.00 + (ABS(CHECKSUM(NEWID())) % 250), NULL, 1, '2025-01-01', NULL, NULL, NULL, GETUTCDATE(), GETUTCDATE(), 0, NULL);
    
    FETCH NEXT FROM @benefitCursor INTO @benefitPlanId;
END;

CLOSE @benefitCursor;
DEALLOCATE @benefitCursor;

-- =====================================================
-- ACCUMULATORS (300 records - one per active member)
-- =====================================================

PRINT 'Inserting accumulators...';

INSERT INTO dbo.accumulators (Id, MemberId, PlanId, Year, Network, DeductibleMet, MoopMet, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
SELECT 
    NEWID() AS Id,
    c.MemberId,
    c.PlanId,
    2025 AS Year,
    'IN' AS Network,
    CAST((ABS(CHECKSUM(NEWID())) % 3000) AS DECIMAL(18,2)) AS DeductibleMet,
    CAST((ABS(CHECKSUM(NEWID())) % 5000) AS DECIMAL(18,2)) AS MoopMet,
    GETUTCDATE() AS CreatedAt,
    GETUTCDATE() AS UpdatedAt,
    0 AS IsDeleted,
    NULL AS DeletedAt
FROM 
    (SELECT DISTINCT MemberId, PlanId FROM dbo.coverages WHERE CoverageStatus = 'Active') c;

-- =====================================================
-- PRIOR AUTHS (200 records)
-- =====================================================

PRINT 'Inserting prior authorizations...';

DECLARE @paCounter INT = 1;
WHILE @paCounter <= 200
BEGIN
    DECLARE @paMemberId UNIQUEIDENTIFIER;
    DECLARE @paProviderId UNIQUEIDENTIFIER;
    DECLARE @paServiceType VARCHAR(255);
    DECLARE @paStatus VARCHAR(50);
    DECLARE @paRequestedDate DATE;
    DECLARE @paDecisionDate DATE;
    
    SELECT TOP 1 @paMemberId = Id FROM dbo.members WHERE Status = 'Active' ORDER BY NEWID();
    SELECT TOP 1 @paProviderId = Id FROM dbo.providers WHERE ProviderStatus = 'Active' ORDER BY NEWID();
    
    SET @paServiceType = CASE (ABS(CHECKSUM(NEWID())) % 10)
        WHEN 0 THEN 'MRI - Lumbar Spine'
        WHEN 1 THEN 'CT Scan - Abdomen/Pelvis'
        WHEN 2 THEN 'Sleep Study'
        WHEN 3 THEN 'Physical Therapy - 20 visits'
        WHEN 4 THEN 'DME - Wheelchair'
        WHEN 5 THEN 'Home Health Services'
        WHEN 6 THEN 'Colonoscopy'
        WHEN 7 THEN 'Knee Arthroscopy'
        WHEN 8 THEN 'Pain Management Injections'
        ELSE 'Cardiac Catheterization'
    END;
    
    SET @paStatus = CASE (ABS(CHECKSUM(NEWID())) % 10)
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Denied'
        WHEN 2 THEN 'Expired'
        ELSE 'Approved'
    END;
    
    SET @paRequestedDate = DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 180), GETUTCDATE());
    SET @paDecisionDate = CASE WHEN @paStatus != 'Pending' THEN DATEADD(DAY, 2 + (ABS(CHECKSUM(NEWID())) % 5), @paRequestedDate) ELSE NULL END;
    
    INSERT INTO dbo.prior_auths (Id, MemberId, ProviderId, ServiceType, Status, RequestedDate, DecisionDate, Notes, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
    VALUES (NEWID(), @paMemberId, @paProviderId, @paServiceType, @paStatus, @paRequestedDate, @paDecisionDate, NULL, GETUTCDATE(), GETUTCDATE(), 0, NULL);
    
    SET @paCounter = @paCounter + 1;
END;

-- =====================================================
-- MEMBER INSURANCE POLICIES (250 records - secondary insurance)
-- =====================================================

PRINT 'Inserting member insurance policies...';

DECLARE @mipCounter INT = 1;
WHILE @mipCounter <= 250
BEGIN
    DECLARE @mipMemberId UNIQUEIDENTIFIER;
    DECLARE @payerName VARCHAR(255);
    
    SELECT TOP 1 @mipMemberId = Id FROM dbo.members WHERE Status = 'Active' ORDER BY NEWID();
    
    SET @payerName = CASE (ABS(CHECKSUM(NEWID())) % 10)
        WHEN 0 THEN 'Aetna'
        WHEN 1 THEN 'UnitedHealthcare'
        WHEN 2 THEN 'Cigna'
        WHEN 3 THEN 'Humana'
        WHEN 4 THEN 'Medicare'
        WHEN 5 THEN 'Medicaid'
        WHEN 6 THEN 'Kaiser Permanente'
        WHEN 7 THEN 'Anthem'
        WHEN 8 THEN 'WellCare'
        ELSE 'Molina Healthcare'
    END;
    
    INSERT INTO dbo.member_insurance_policies (Id, MemberId, PayerName, PolicyNumber, Priority, EffectiveFrom, EffectiveTo, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
    VALUES (NEWID(), @mipMemberId, @payerName, 'POL' + RIGHT('00000000' + CAST(@mipCounter AS VARCHAR(8)), 8), 2, DATEADD(MONTH, -6, GETUTCDATE()), NULL, GETUTCDATE(), GETUTCDATE(), 0, NULL);
    
    SET @mipCounter = @mipCounter + 1;
END;

-- =====================================================
-- CLAIM APPEALS (50 records - for denied claims)
-- =====================================================

PRINT 'Inserting claim appeals...';

INSERT INTO dbo.claim_appeals (Id, ClaimId, AppealLevel, Reason, Status, SubmittedAt, DecisionAt, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
SELECT TOP 50
    NEWID() AS Id,
    c.Id AS ClaimId,
    1 AS AppealLevel,
    CASE (ABS(CHECKSUM(NEWID())) % 5)
        WHEN 0 THEN 'Medical necessity not demonstrated'
        WHEN 1 THEN 'Service not covered under plan'
        WHEN 2 THEN 'Lack of prior authorization'
        WHEN 3 THEN 'Incorrect billing code'
        ELSE 'Duplicate claim'
    END AS Reason,
    CASE (ABS(CHECKSUM(NEWID())) % 5)
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Under Review'
        WHEN 2 THEN 'Approved'
        WHEN 3 THEN 'Denied'
        ELSE 'Withdrawn'
    END AS Status,
    DATEADD(DAY, 10, c.ReceivedDate) AS SubmittedAt,
    NULL AS DecisionAt,
    GETUTCDATE() AS CreatedAt,
    GETUTCDATE() AS UpdatedAt,
    0 AS IsDeleted,
    NULL AS DeletedAt
FROM 
    dbo.claims c
WHERE 
    c.Status = 'Denied'
ORDER BY NEWID();

-- =====================================================
-- CLAIM ATTACHMENTS (300 records)
-- =====================================================

PRINT 'Inserting claim attachments...';

DECLARE @attCounter INT = 1;
WHILE @attCounter <= 300
BEGIN
    DECLARE @attClaimId UNIQUEIDENTIFIER;
    DECLARE @attFileName VARCHAR(255);
    DECLARE @attContentType VARCHAR(100);
    
    SELECT TOP 1 @attClaimId = Id FROM dbo.claims ORDER BY NEWID();
    
    SET @attFileName = CASE (ABS(CHECKSUM(NEWID())) % 5)
        WHEN 0 THEN 'medical_records_' + CAST(@attCounter AS VARCHAR(10)) + '.pdf'
        WHEN 1 THEN 'lab_results_' + CAST(@attCounter AS VARCHAR(10)) + '.pdf'
        WHEN 2 THEN 'prescription_' + CAST(@attCounter AS VARCHAR(10)) + '.pdf'
        WHEN 3 THEN 'imaging_report_' + CAST(@attCounter AS VARCHAR(10)) + '.pdf'
        ELSE 'supporting_documentation_' + CAST(@attCounter AS VARCHAR(10)) + '.pdf'
    END;
    
    SET @attContentType = 'application/pdf';
    
    INSERT INTO dbo.claim_attachments (Id, ClaimId, FileName, ContentType, StorageProvider, StorageKey, Sha256, UploadedAt, UploadedByActorType, UploadedByActorId, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
    VALUES (NEWID(), @attClaimId, @attFileName, @attContentType, 'S3', 'attachments/' + CAST(NEWID() AS VARCHAR(50)), CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @attFileName), 2), GETUTCDATE(), 'Provider', NULL, GETUTCDATE(), GETUTCDATE(), 0, NULL);
    
    SET @attCounter = @attCounter + 1;
END;

-- =====================================================
-- AUDIT EVENTS (500 records)
-- =====================================================

PRINT 'Inserting audit events...';

DECLARE @auditCounter INT = 1;
WHILE @auditCounter <= 500
BEGIN
    DECLARE @auditAction VARCHAR(50);
    DECLARE @auditEntityType VARCHAR(50);
    DECLARE @auditEntityId UNIQUEIDENTIFIER;
    
    SET @auditAction = CASE (ABS(CHECKSUM(NEWID())) % 5)
        WHEN 0 THEN 'Create'
        WHEN 1 THEN 'Update'
        WHEN 2 THEN 'Delete'
        WHEN 3 THEN 'View'
        ELSE 'Export'
    END;
    
    SET @auditEntityType = CASE (ABS(CHECKSUM(NEWID())) % 5)
        WHEN 0 THEN 'Claim'
        WHEN 1 THEN 'Member'
        WHEN 2 THEN 'Provider'
        WHEN 3 THEN 'Coverage'
        ELSE 'Payment'
    END;
    
    -- Get a random entity ID based on type
    IF @auditEntityType = 'Claim'
        SELECT TOP 1 @auditEntityId = Id FROM dbo.claims ORDER BY NEWID();
    ELSE IF @auditEntityType = 'Member'
        SELECT TOP 1 @auditEntityId = Id FROM dbo.members ORDER BY NEWID();
    ELSE IF @auditEntityType = 'Provider'
        SELECT TOP 1 @auditEntityId = Id FROM dbo.providers ORDER BY NEWID();
    ELSE IF @auditEntityType = 'Coverage'
        SELECT TOP 1 @auditEntityId = Id FROM dbo.coverages ORDER BY NEWID();
    ELSE
        SELECT TOP 1 @auditEntityId = Id FROM dbo.payments ORDER BY NEWID();
    
    INSERT INTO dbo.audit_events (Id, Action, ActorUserId, EntityType, EntityId, OccurredAt, PrevHash, Hash, DiffJson, Notes, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
    VALUES (
        NEWID(),
        @auditAction,
        'user-' + CAST(1 + (ABS(CHECKSUM(NEWID())) % 50) AS VARCHAR(10)),
        @auditEntityType,
        CAST(@auditEntityId AS VARCHAR(50)),
        DATEADD(MINUTE, -ABS(CHECKSUM(NEWID()) % 525600), GETUTCDATE()),
        REPLICATE('0', 64),
        CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(NEWID() AS VARCHAR(50))), 2),
        NULL,
        NULL,
        GETUTCDATE(),
        GETUTCDATE(),
        0,
        NULL
    );
    
    SET @auditCounter = @auditCounter + 1;
END;

-- =====================================================
-- MEMBER CONSENTS (300 records)
-- =====================================================

PRINT 'Inserting member consents...';

INSERT INTO dbo.member_consents (Id, MemberId, ConsentType, Source, Granted, GrantedAt, RevokedAt, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
SELECT 
    NEWID() AS Id,
    m.Id AS MemberId,
    CASE (ABS(CHECKSUM(NEWID())) % 4)
        WHEN 0 THEN 'Treatment'
        WHEN 1 THEN 'Payment'
        WHEN 2 THEN 'Healthcare Operations'
        ELSE 'Marketing Communications'
    END AS ConsentType,
    CASE (ABS(CHECKSUM(NEWID())) % 3)
        WHEN 0 THEN 'Portal'
        WHEN 1 THEN 'Paper Form'
        ELSE 'Phone'
    END AS Source,
    1 AS Granted,
    DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 365), GETUTCDATE()) AS GrantedAt,
    NULL AS RevokedAt,
    GETUTCDATE() AS CreatedAt,
    GETUTCDATE() AS UpdatedAt,
    0 AS IsDeleted,
    NULL AS DeletedAt
FROM 
    dbo.members m
WHERE 
    m.Status = 'Active';

-- =====================================================
-- HIPAA ACCESS LOGS (600 records)
-- =====================================================

PRINT 'Inserting HIPAA access logs...';

DECLARE @hipaaCounter INT = 1;
WHILE @hipaaCounter <= 600
BEGIN
    DECLARE @hipaaActorType VARCHAR(50);
    DECLARE @hipaaAction VARCHAR(50);
    DECLARE @hipaaSubjectType VARCHAR(50);
    DECLARE @hipaaSubjectId UNIQUEIDENTIFIER;
    DECLARE @hipaaPurpose VARCHAR(50);
    
    SET @hipaaActorType = CASE (ABS(CHECKSUM(NEWID())) % 3)
        WHEN 0 THEN 'User'
        WHEN 1 THEN 'Provider'
        ELSE 'System'
    END;
    
    SET @hipaaAction = CASE (ABS(CHECKSUM(NEWID())) % 4)
        WHEN 0 THEN 'Read'
        WHEN 1 THEN 'Create'
        WHEN 2 THEN 'Update'
        ELSE 'Export'
    END;
    
    SET @hipaaSubjectType = CASE (ABS(CHECKSUM(NEWID())) % 3)
        WHEN 0 THEN 'Member'
        WHEN 1 THEN 'Claim'
        ELSE 'Coverage'
    END;
    
    IF @hipaaSubjectType = 'Member'
        SELECT TOP 1 @hipaaSubjectId = Id FROM dbo.members ORDER BY NEWID();
    ELSE IF @hipaaSubjectType = 'Claim'
        SELECT TOP 1 @hipaaSubjectId = Id FROM dbo.claims ORDER BY NEWID();
    ELSE
        SELECT TOP 1 @hipaaSubjectId = Id FROM dbo.coverages ORDER BY NEWID();
    
    SET @hipaaPurpose = CASE (ABS(CHECKSUM(NEWID())) % 4)
        WHEN 0 THEN 'TREATMENT'
        WHEN 1 THEN 'PAYMENT'
        WHEN 2 THEN 'OPERATIONS'
        ELSE 'DISCLOSURE'
    END;
    
    INSERT INTO dbo.hipaa_access_logs (AccessLogId, ActorType, ActorId, Action, SubjectType, SubjectId, OccurredAt, PurposeOfUse, PrevHash, Hash, IpAddress, UserAgent)
    VALUES (
        NEWID(),
        @hipaaActorType,
        CAST(NEWID() AS VARCHAR(50)),
        @hipaaAction,
        @hipaaSubjectType,
        CAST(@hipaaSubjectId AS VARCHAR(50)),
        DATEADD(MINUTE, -ABS(CHECKSUM(NEWID()) % 525600), GETUTCDATE()),
        @hipaaPurpose,
        REPLICATE('0', 64),
        CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(NEWID() AS VARCHAR(50))), 2),
        '192.168.' + CAST((ABS(CHECKSUM(NEWID())) % 255) AS VARCHAR(3)) + '.' + CAST((ABS(CHECKSUM(NEWID())) % 255) AS VARCHAR(3)),
        'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
    );
    
    SET @hipaaCounter = @hipaaCounter + 1;
END;

-- =====================================================
-- WEBHOOK ENDPOINTS (10 records)
-- =====================================================

PRINT 'Inserting webhook endpoints...';

DECLARE @webhookCounter INT = 1;
WHILE @webhookCounter <= 10
BEGIN
    INSERT INTO dbo.webhook_endpoints (Id, Name, Url, Secret, IsActive, CreatedAt)
    VALUES (
        NEWID(),
        CASE (@webhookCounter % 5)
            WHEN 0 THEN 'Claims Processing Webhook'
            WHEN 1 THEN 'Member Enrollment Webhook'
            WHEN 2 THEN 'Payment Notification Webhook'
            WHEN 3 THEN 'Prior Auth Webhook'
            ELSE 'EOB Delivery Webhook'
        END,
        'https://api.partner' + CAST(@webhookCounter AS VARCHAR(2)) + '.example.com/webhook',
        NULL,
        1,
        GETUTCDATE()
    );
    
    SET @webhookCounter = @webhookCounter + 1;
END;

-- =====================================================
-- WEBHOOK DELIVERIES (200 records)
-- =====================================================

PRINT 'Inserting webhook deliveries...';

DECLARE @deliveryCounter INT = 1;
WHILE @deliveryCounter <= 200
BEGIN
    DECLARE @deliveryEndpointId UNIQUEIDENTIFIER;
    DECLARE @deliveryEventType VARCHAR(100);
    DECLARE @deliveryStatus VARCHAR(50);
    
    SELECT TOP 1 @deliveryEndpointId = Id FROM dbo.webhook_endpoints ORDER BY NEWID();
    
    SET @deliveryEventType = CASE (ABS(CHECKSUM(NEWID())) % 6)
        WHEN 0 THEN 'claim.received'
        WHEN 1 THEN 'claim.processed'
        WHEN 2 THEN 'claim.paid'
        WHEN 3 THEN 'member.enrolled'
        WHEN 4 THEN 'prior_auth.approved'
        ELSE 'eob.generated'
    END;
    
    SET @deliveryStatus = CASE (ABS(CHECKSUM(NEWID())) % 10)
        WHEN 0 THEN 'Failed'
        WHEN 1 THEN 'Pending'
        ELSE 'Completed'
    END;
    
    INSERT INTO dbo.webhook_deliveries (Id, WebhookEndpointId, EventType, PayloadJson, Status, AttemptCount, LastAttemptAt, LastStatusCode, LastError, NextAttemptAt)
    VALUES (
        NEWID(),
        @deliveryEndpointId,
        @deliveryEventType,
        '{"event_id": "' + CAST(NEWID() AS VARCHAR(50)) + '"}',
        @deliveryStatus,
        CASE WHEN @deliveryStatus = 'Failed' THEN 3 ELSE 1 END,
        DATEADD(MINUTE, -ABS(CHECKSUM(NEWID()) % 10080), GETUTCDATE()),
        CASE WHEN @deliveryStatus = 'Completed' THEN 200 ELSE 500 END,
        CASE WHEN @deliveryStatus = 'Failed' THEN 'Connection timeout' ELSE NULL END,
        CASE WHEN @deliveryStatus = 'Pending' THEN DATEADD(MINUTE, 30, GETUTCDATE()) ELSE NULL END
    );
    
    SET @deliveryCounter = @deliveryCounter + 1;
END;

-- =====================================================
-- EXPORT JOBS (100 records)
-- =====================================================

PRINT 'Inserting export jobs...';

DECLARE @exportCounter INT = 1;
WHILE @exportCounter <= 100
BEGIN
    DECLARE @exportMemberId UNIQUEIDENTIFIER;
    DECLARE @exportStatus VARCHAR(50);
    
    SELECT TOP 1 @exportMemberId = Id FROM dbo.members WHERE Status = 'Active' ORDER BY NEWID();
    
    SET @exportStatus = CASE (ABS(CHECKSUM(NEWID())) % 5)
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'In Progress'
        WHEN 2 THEN 'Failed'
        ELSE 'Completed'
    END;
    
    INSERT INTO dbo.export_jobs (Id, MemberId, RequestedByActorType, RequestedByActorId, Status, CreatedAt, CompletedAt, FilePath, DownloadTokenHash, ExpiresAt)
    VALUES (
        NEWID(),
        @exportMemberId,
        'Member',
        CAST(@exportMemberId AS VARCHAR(50)),
        @exportStatus,
        DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 90), GETUTCDATE()),
        CASE WHEN @exportStatus = 'Completed' THEN DATEADD(HOUR, 2, DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 90), GETUTCDATE())) ELSE NULL END,
        CASE WHEN @exportStatus = 'Completed' THEN 'exports/member_' + CAST(@exportMemberId AS VARCHAR(50)) + '.zip' ELSE NULL END,
        CASE WHEN @exportStatus = 'Completed' THEN CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(NEWID() AS VARCHAR(50))), 2) ELSE NULL END,
        CASE WHEN @exportStatus = 'Completed' THEN DATEADD(DAY, 7, GETUTCDATE()) ELSE NULL END
    );
    
    SET @exportCounter = @exportCounter + 1;
END;

COMMIT TRANSACTION;

PRINT '';
PRINT '=================================================';
PRINT 'Data seeding completed successfully!';
PRINT '=================================================';
PRINT 'Summary:';
PRINT '- Reference codes: 60+';
PRINT '- Plans: 250';
PRINT '- Members: 300';
PRINT '- Providers: 250';
PRINT '- Coverages: 300';
PRINT '- Claims: 500';
PRINT '- Claim Lines: 750+';
PRINT '- Claim Diagnoses: 1000+';
PRINT '- Provider Locations: 250';
PRINT '- Payments: 300+';
PRINT '- EOBs: 300+';
PRINT '- Plan Benefits: 500+';
PRINT '- Accumulators: 300';
PRINT '- Prior Authorizations: 200';
PRINT '- Secondary Insurance: 250';
PRINT '- Claim Appeals: 50';
PRINT '- Claim Attachments: 300';
PRINT '- Audit Events: 500';
PRINT '- Member Consents: 300';
PRINT '- HIPAA Access Logs: 600';
PRINT '- Webhook Endpoints: 10';
PRINT '- Webhook Deliveries: 200';
PRINT '- Export Jobs: 100';
PRINT '=================================================';