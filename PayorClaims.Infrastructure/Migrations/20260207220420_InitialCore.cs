using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayorClaims.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accumulators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Network = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeductibleMet = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MoopMet = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accumulators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "adjustment_reason_codes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CodeType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adjustment_reason_codes", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiffJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cpt_codes",
                columns: table => new
                {
                    CptCodeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cpt_codes", x => x.CptCodeId);
                });

            migrationBuilder.CreateTable(
                name: "diagnosis_codes",
                columns: table => new
                {
                    CodeSystem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diagnosis_codes", x => new { x.CodeSystem, x.Code });
                });

            migrationBuilder.CreateTable(
                name: "hipaa_access_logs",
                columns: table => new
                {
                    AccessLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SubjectType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PurposeOfUse = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrevHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hipaa_access_logs", x => x.AccessLogId);
                });

            migrationBuilder.CreateTable(
                name: "member_consents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Granted = table.Column<bool>(type: "bit", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_member_consents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "member_insurance_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_member_insurance_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalMemberNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Dob = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmailEncrypted = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PhoneEncrypted = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    SsnEncrypted = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    NetworkType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MetalTier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prior_auths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prior_auths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "providers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Npi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProviderType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ProviderStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CredentialedFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    CredentialedTo = table.Column<DateOnly>(type: "date", nullable: true),
                    TerminationReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "coverages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CoverageStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coverages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_coverages_members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_coverages_plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plan_benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BenefitVersion = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Network = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CoverageLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Period = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CopayAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CoinsurancePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    DeductibleApplies = table.Column<bool>(type: "bit", nullable: false),
                    MaxVisits = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_benefits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_plan_benefits_plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "provider_locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Zip = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_provider_locations_providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoverageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceivedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ServiceFromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ServiceToDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TotalBilled = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAllowed = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    DuplicateFingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SubmittedByActorType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubmittedByActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_claims_coverages_CoverageId",
                        column: x => x.CoverageId,
                        principalTable: "coverages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claims_members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claims_providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "claim_appeals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppealLevel = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecisionAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_appeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_claim_appeals_claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "claim_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StorageProvider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UploadedByActorType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UploadedByActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_claim_attachments_claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "claim_diagnoses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: true),
                    CodeSystem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_diagnoses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_claim_diagnoses_claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "claim_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    CptCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Units = table.Column<int>(type: "int", nullable: false),
                    BilledAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AllowedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DenialReasonCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DenialReasonText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_claim_lines_adjustment_reason_codes_DenialReasonCode",
                        column: x => x.DenialReasonCode,
                        principalTable: "adjustment_reason_codes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_lines_claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "eobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EobNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentStorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DocumentSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DeliveryMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_eobs_claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accumulators_MemberId_PlanId_Year_Network",
                table: "accumulators",
                columns: new[] { "MemberId", "PlanId", "Year", "Network" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_EntityType_EntityId",
                table: "audit_events",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_OccurredAt",
                table: "audit_events",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_claim_appeals_ClaimId",
                table: "claim_appeals",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_claim_attachments_ClaimId",
                table: "claim_attachments",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_claim_attachments_Sha256",
                table: "claim_attachments",
                column: "Sha256");

            migrationBuilder.CreateIndex(
                name: "IX_claim_diagnoses_ClaimId_LineNumber",
                table: "claim_diagnoses",
                columns: new[] { "ClaimId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_claim_lines_ClaimId",
                table: "claim_lines",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_claim_lines_ClaimId_LineNumber",
                table: "claim_lines",
                columns: new[] { "ClaimId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_claim_lines_DenialReasonCode",
                table: "claim_lines",
                column: "DenialReasonCode");

            migrationBuilder.CreateIndex(
                name: "IX_claims_ClaimNumber",
                table: "claims",
                column: "ClaimNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_claims_CoverageId",
                table: "claims",
                column: "CoverageId");

            migrationBuilder.CreateIndex(
                name: "IX_claims_IdempotencyKey",
                table: "claims",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_claims_MemberId",
                table: "claims",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_claims_ProviderId",
                table: "claims",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_claims_Status",
                table: "claims",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_coverages_MemberId_StartDate",
                table: "coverages",
                columns: new[] { "MemberId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_coverages_PlanId",
                table: "coverages",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_eobs_ClaimId",
                table: "eobs",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_eobs_EobNumber",
                table: "eobs",
                column: "EobNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hipaa_access_logs_ActorType_ActorId",
                table: "hipaa_access_logs",
                columns: new[] { "ActorType", "ActorId" });

            migrationBuilder.CreateIndex(
                name: "IX_hipaa_access_logs_OccurredAt",
                table: "hipaa_access_logs",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_hipaa_access_logs_SubjectType_SubjectId",
                table: "hipaa_access_logs",
                columns: new[] { "SubjectType", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_member_consents_MemberId_ConsentType",
                table: "member_consents",
                columns: new[] { "MemberId", "ConsentType" });

            migrationBuilder.CreateIndex(
                name: "IX_member_insurance_policies_MemberId_Priority",
                table: "member_insurance_policies",
                columns: new[] { "MemberId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_payments_ClaimId_PaymentDate",
                table: "payments",
                columns: new[] { "ClaimId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_payments_IdempotencyKey",
                table: "payments",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_plan_benefits_PlanId_Category_Network",
                table: "plan_benefits",
                columns: new[] { "PlanId", "Category", "Network" });

            migrationBuilder.CreateIndex(
                name: "IX_prior_auths_MemberId_Status",
                table: "prior_auths",
                columns: new[] { "MemberId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_provider_locations_ProviderId",
                table: "provider_locations",
                column: "ProviderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accumulators");

            migrationBuilder.DropTable(
                name: "audit_events");

            migrationBuilder.DropTable(
                name: "claim_appeals");

            migrationBuilder.DropTable(
                name: "claim_attachments");

            migrationBuilder.DropTable(
                name: "claim_diagnoses");

            migrationBuilder.DropTable(
                name: "claim_lines");

            migrationBuilder.DropTable(
                name: "cpt_codes");

            migrationBuilder.DropTable(
                name: "diagnosis_codes");

            migrationBuilder.DropTable(
                name: "eobs");

            migrationBuilder.DropTable(
                name: "hipaa_access_logs");

            migrationBuilder.DropTable(
                name: "member_consents");

            migrationBuilder.DropTable(
                name: "member_insurance_policies");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "plan_benefits");

            migrationBuilder.DropTable(
                name: "prior_auths");

            migrationBuilder.DropTable(
                name: "provider_locations");

            migrationBuilder.DropTable(
                name: "adjustment_reason_codes");

            migrationBuilder.DropTable(
                name: "claims");

            migrationBuilder.DropTable(
                name: "coverages");

            migrationBuilder.DropTable(
                name: "providers");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "plans");
        }
    }
}
