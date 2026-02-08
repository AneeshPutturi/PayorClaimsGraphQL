using GraphQL.Types;
using PayorClaims.Application.Abstractions;

namespace PayorClaims.Schema.Types;

public class ExportRequestResultType : ObjectGraphType<ExportRequestResult>
{
    public ExportRequestResultType()
    {
        Name = "ExportRequestResult";
        Field<NonNullGraphType<IdGraphType>>("jobId").Resolve(c => c.Source!.JobId);
        Field<NonNullGraphType<StringGraphType>>("status").Resolve(c => c.Source!.Status);
        Field<StringGraphType>("downloadTokenOnce").Resolve(c => c.Source!.DownloadTokenOnce);
        Field<DateTimeOffsetGraphType>("expiresAt").Resolve(c => c.Source!.ExpiresAt);
    }
}

public class ExportJobStatusResultType : ObjectGraphType<ExportJobStatusResult>
{
    public ExportJobStatusResultType()
    {
        Name = "ExportJobStatusResult";
        Field<NonNullGraphType<StringGraphType>>("status").Resolve(c => c.Source!.Status);
        Field<DateTimeOffsetGraphType>("readyAt").Resolve(c => c.Source!.ReadyAt);
        Field<StringGraphType>("downloadTokenOnce").Resolve(c => c.Source!.DownloadTokenOnce);
        Field<DateTimeOffsetGraphType>("expiresAt").Resolve(c => c.Source!.ExpiresAt);
    }
}
