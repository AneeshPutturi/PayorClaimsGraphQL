using FluentValidation;
using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using PayorClaims.Application.Abstractions;
using PayorClaims.Application.Dtos.Claims;
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
                var (claim, alreadyExisted) = await service.SubmitClaimAsync(dto, "Provider", null, c.CancellationToken);
                return new SubmitClaimPayload { Claim = claim, AlreadyExisted = alreadyExisted };
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
                return await attachmentService.UploadAsync(claimId, fileName, contentType, data, "Provider", null, c.CancellationToken);
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
