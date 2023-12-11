using Application.Common.Monitoring;
using Application.Subject.Commands.AddNewSubject;

namespace Application.Common.Messages.Responses;

public class ArchiveSubjectAttachmentsSuccessResult : BaseResponse
{
    public required string SubmitterId { get; init; }
    public required NewSubjectIm Input { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.UserId, SubmitterId)
        };
    }
}
