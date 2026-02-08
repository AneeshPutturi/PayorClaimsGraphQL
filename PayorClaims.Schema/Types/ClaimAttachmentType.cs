using GraphQL.Types;
using PayorClaims.Domain.Entities;
using PayorClaims.Schema.Interfaces;

namespace PayorClaims.Schema.Types;

public class ClaimAttachmentType : ObjectGraphType<ClaimAttachment>
{
    public ClaimAttachmentType()
    {
        Name = "ClaimAttachment";
        Interface<AuditableInterface>();
        Field<NonNullGraphType<IdGraphType>>("id").Resolve(c => c.Source.Id);
        Field<NonNullGraphType<DateTimeGraphType>>("createdAt").Resolve(c => c.Source.CreatedAt);
        Field<NonNullGraphType<DateTimeGraphType>>("updatedAt").Resolve(c => c.Source.UpdatedAt);
        Field<NonNullGraphType<IdGraphType>>("claimId").Resolve(c => c.Source.ClaimId);
        Field<NonNullGraphType<StringGraphType>>("fileName").Resolve(c => c.Source.FileName);
        Field<NonNullGraphType<StringGraphType>>("contentType").Resolve(c => c.Source.ContentType);
        Field<NonNullGraphType<StringGraphType>>("storageProvider").Resolve(c => c.Source.StorageProvider);
        Field<NonNullGraphType<StringGraphType>>("storageKey").Resolve(c => c.Source.StorageKey);
        Field<NonNullGraphType<StringGraphType>>("sha256").Resolve(c => c.Source.Sha256);
        Field<NonNullGraphType<DateTimeGraphType>>("uploadedAt").Resolve(c => c.Source.UploadedAt);
    }
}
