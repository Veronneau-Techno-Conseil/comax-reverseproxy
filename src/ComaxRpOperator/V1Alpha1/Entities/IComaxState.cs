using System.Text.Json.Serialization;

namespace CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Entities
{
    public interface IComaxState
    {
        string CurrentState { get; set; }
        long StateTsMs { get; set; }
        long StateTs { get; set; }
    }
}
