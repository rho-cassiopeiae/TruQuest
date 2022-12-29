namespace Application.Vote.Commands.CastAcceptancePollVote;

public enum DecisionIm
{
    SoftDecline,
    HardDecline,
    Accept,
}

public static class DecisionImExtension
{
    public static string GetString(this DecisionIm decision)
    {
        switch (decision)
        {
            case DecisionIm.SoftDecline:
                return "Soft decline";
            case DecisionIm.HardDecline:
                return "Hard decline";
            case DecisionIm.Accept:
                return "Accept";
        }

        throw new InvalidOperationException();
    }
}