using FluentValidation;
using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Application.Abstractions;
using PayorClaims.Application.Dtos.Claims;
using PayorClaims.Application.Exceptions;
using PayorClaims.Application.Security;
using PayorClaims.Domain.Entities;
using PayorClaims.Infrastructure.Persistence;
using PayorClaims.Schema.Inputs;
using PayorClaims.Schema.Types;

namespace PayorClaims.Schema.Schema;

public class AppMutation : ObjectGraphType
{
    public AppMutation()
    {
        Name = "Mutation";

        Field<NonNullGraphType<SubmitClaimPayloadType>>("submitClaim")
            .Argument<NonNullGraphType<ClaimSubmissionInputGraphType>>("input")
            .ResolveAsync(async c =>
            {
                var input = c.GetArgument<ClaimSubmissionInput>("input")!;
                var dto = MapToDto(input);
                var validator = c.RequestServices!.GetRequiredService<IValidator<ClaimSubmissionInputDto>>();
                await validator.ValidateAndThrowAsync(dto, c.CancellationToken);

                var service = c.RequestServices!.GetRequiredService<IClaimService>();
                try
                {
                    var result = await service.SubmitClaimAsync(dto, "Provider", null, c.CancellationToken);
                    return new SubmitClaimPayload { Claim = result.Claim, AlreadyExisted = result.AlreadyExisted };
                }
                catch (AppValidationException ex)
                {
                    var err = new ExecutionError("Validation failed") { Code = ex.Code ?? "VALIDATION_FAILED" };
                    (err.Extensions ??= new Dictionary<string, object?>())["errors"] = ex.Errors;
                    throw err;
                }
            });

        Field<NonNullGraphType<AdjudicateClaimPayloadType>>("adjudicateClaim")
            .Argument<NonNullGraphType<IdGraphType>>("claimId")
            .Argument<NonNullGraphType<StringGraphType>>("rowVersion")
            .Argument<NonNullGraphType<ListGraphType<NonNullGraphType<AdjudicateLineInputGraphType>>>>("lines")
            .ResolveAsync(async c =>
            {
                var claimId = c.GetArgument<Guid>("claimId");
                var rowVersionBase64 = c.GetArgument<string>("rowVersion")!;
                byte[] rowVersion;
                try
                {
                    rowVersion = Convert.FromBase64String(rowVersionBase64);
                }
                catch
                {
                    throw new ExecutionError("Invalid rowVersion: must be base64.");
                }

                var lineInputs = c.GetArgument<List<AdjudicateLineInput>>("lines")!;
                var lineDecisions = lineInputs.Select(l => new AdjudicateLineDto
                {
                    LineNumber = l.LineNumber,
                    Status = l.Status,
                    DenialReasonCode = l.DenialReasonCode,
                    AllowedAmount = l.AllowedAmount,
                    PaidAmount = l.PaidAmount
                }).ToList();

                var service = c.RequestServices!.GetRequiredService<IClaimService>();
                try
                {
                    var claim = await service.AdjudicateClaimAsync(claimId, rowVersion, lineDecisions, c.CancellationToken);
                    return new AdjudicateClaimPayload { Claim = claim };
                }
                catch (AppValidationException ex)
                {
                    var err = new ExecutionError("Validation failed") { Code = ex.Code ?? "VALIDATION_FAILED" };
                    (err.Extensions ??= new Dictionary<string, object?>())["errors"] = ex.Errors;
                    throw err;
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new ExecutionError("CONCURRENCY_CONFLICT") { Code = "CONCURRENCY_CONFLICT" };
                }
            });

        Field<NonNullGraphType<ClaimAppealType>>("submitAppeal")
            .Argument<NonNullGraphType<IdGraphType>>("claimId")
            .Argument<NonNullGraphType<IntGraphType>>("level")
            .Argument<NonNullGraphType<StringGraphType>>("reason")
            .ResolveAsync(async c =>
            {
                var claimId = c.GetArgument<Guid>("claimId");
                var level = c.GetArgument<int>("level");
                var reason = c.GetArgument<string>("reason")!;
                var service = c.RequestServices!.GetRequiredService<IClaimService>();
                return await service.SubmitAppealAsync(claimId, level, reason, c.CancellationToken);
            });

        Field<NonNullGraphType<ClaimAppealType>>("decideAppeal")
            .Argument<NonNullGraphType<IdGraphType>>("appealId")
            .Argument<NonNullGraphType<StringGraphType>>("status")
            .ResolveAsync(async c =>
            {
                var appealId = c.GetArgument<Guid>("appealId");
                var status = c.GetArgument<string>("status")!;
                var service = c.RequestServices!.GetRequiredService<IClaimService>();
                return await service.DecideAppealAsync(appealId, status, c.CancellationToken);
            });

        Field<NonNullGraphType<ClaimAttachmentType>>("uploadClaimAttachment")
            .Argument<NonNullGraphType<IdGraphType>>("claimId")
            .Argument<NonNullGraphType<StringGraphType>>("fileName")
            .Argument<NonNullGraphType<StringGraphType>>("contentType")
            .Argument<NonNullGraphType<StringGraphType>>("base64")
            .ResolveAsync(async c =>
            {
                var claimId = c.GetArgument<Guid>("claimId");
                var fileName = c.GetArgument<string>("fileName")!;
                var contentType = c.GetArgument<string>("contentType")!;
                var base64 = c.GetArgument<string>("base64")!;
                byte[] data;
                try
                {
                    data = Convert.FromBase64String(base64);
                }
                catch
                {
                    throw new ExecutionError("Invalid base64 content.");
                }

                var attachmentService = c.RequestServices!.GetRequiredService<IClaimAttachmentService>();
                try
                {
                    return await attachmentService.UploadAsync(claimId, fileName, contentType, data, "Provider", null, c.CancellationToken);
                }
                catch (AppValidationException ex)
                {
                    var err = new ExecutionError(ex.Message) { Code = ex.Code ?? "VALIDATION_FAILED" };
                    (err.Extensions ??= new Dictionary<string, object?>())["errors"] = ex.Errors;
                    throw err;
                }
            });

        Field<NonNullGraphType<WebhookEndpointType>>("registerWebhook")
            .Argument<NonNullGraphType<StringGraphType>>("name")
            .Argument<NonNullGraphType<StringGraphType>>("url")
            .Argument<StringGraphType>("secret")
            .ResolveAsync(async c =>
            {
                var actorProvider = c.RequestServices!.GetRequiredService<IActorContextProvider>();
                var actor = actorProvider.GetActorContext();
                if (actor == null || !actor.IsAdmin)
                    throw new ExecutionError("Admin role required") { Code = "FORBIDDEN" };
                var name = c.GetArgument<string>("name")!;
                var url = c.GetArgument<string>("url")!;
                var secret = c.GetArgument<string>("secret");
                var service = c.RequestServices!.GetRequiredService<IWebhookService>();
                return await service.RegisterEndpointAsync(name, url, secret, c.CancellationToken);
            });

        Field<WebhookEndpointType>("deactivateWebhook")
            .Argument<NonNullGraphType<IdGraphType>>("id")
            .ResolveAsync(async c =>
            {
                var actorProvider = c.RequestServices!.GetRequiredService<IActorContextProvider>();
                var actor = actorProvider.GetActorContext();
                if (actor == null || !actor.IsAdmin)
                    throw new ExecutionError("Admin role required") { Code = "FORBIDDEN" };
                var id = c.GetArgument<Guid>("id");
                var service = c.RequestServices!.GetRequiredService<IWebhookService>();
                return await service.DeactivateEndpointAsync(id, c.CancellationToken);
            });

        Field<NonNullGraphType<ExportRequestResultType>>("requestMemberClaimsExport")
            .Argument<NonNullGraphType<IdGraphType>>("memberId")
            .ResolveAsync(async c =>
            {
                var actorProvider = c.RequestServices!.GetRequiredService<IActorContextProvider>();
                var actor = actorProvider.GetActorContext();
                if (actor == null)
                    throw new ExecutionError("Authentication required") { Code = "UNAUTHORIZED" };
                var memberId = c.GetArgument<Guid>("memberId");
                var service = c.RequestServices!.GetRequiredService<IExportService>();
                return await service.RequestMemberClaimsExportAsync(memberId, actor, c.CancellationToken);
            });
    }

    private static ClaimSubmissionInputDto MapToDto(ClaimSubmissionInput input)
    {
        return new ClaimSubmissionInputDto
        {
            MemberId = input.MemberId,
            ProviderId = input.ProviderId,
            ServiceFrom = input.ServiceFrom,
            ServiceTo = input.ServiceTo,
            ReceivedDate = input.ReceivedDate,
            IdempotencyKey = input.IdempotencyKey,
            Diagnoses = input.Diagnoses?.Select(d => new ClaimDiagnosisInputDto { CodeSystem = d.CodeSystem, Code = d.Code, IsPrimary = d.IsPrimary, LineNumber = d.LineNumber }).ToList(),
            Lines = input.Lines.Select(l => new ClaimLineInputDto
            {
                LineNumber = l.LineNumber,
                CptCode = l.CptCode,
                Units = l.Units,
                BilledAmount = l.BilledAmount,
                Diagnoses = l.Diagnoses?.Select(d => new ClaimDiagnosisInputDto { CodeSystem = d.CodeSystem, Code = d.Code, IsPrimary = d.IsPrimary, LineNumber = d.LineNumber }).ToList()
            }).ToList()
        };
    }
}
