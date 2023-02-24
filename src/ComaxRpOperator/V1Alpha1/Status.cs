namespace CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1
{
    public enum Status
    {
        Unknown = 0,

        Creating,
        Updating,
        Starting,

        Stable,
        Broken,
        Stopped
    }
}
