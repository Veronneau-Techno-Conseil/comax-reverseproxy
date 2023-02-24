using k8s.Models;
using k8s;

namespace CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Entities
{
    public interface IAssignableSpec
    {
        void Assign(IAssignableSpec other);
    }

    public interface IAssignableSpec<T> : ISpec<T>, IAssignableSpec where T: IAssignable<T>
    {
        void Assign(IAssignableSpec<T> other);
    }

    public interface IAssignable<TSpec>
    {
        void Assign(TSpec other);
    }
}
