

using CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Entities;
using IdentityModel;
using k8s;
using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.ObjectPool;
using System.Linq.Expressions;
using System.Reflection;
using static CommunAxiom.Commons.Client.Hosting.Operator.KubernetesClientExtensions;

namespace CommunAxiom.Commons.Client.Hosting.Operator
{
    public static class KubernetesClientExtensions
    {
        public interface IGenWrapper
        {
            Task<IKubernetesObject<V1ObjectMeta>> Save(IKubernetesClient cl, IKubernetesObject<V1ObjectMeta> obj);
            Task<IKubernetesObject<V1ObjectMeta>> Create(IKubernetesClient cl, IKubernetesObject<V1ObjectMeta> obj);
            Task<IKubernetesObject<V1ObjectMeta>> Update(IKubernetesClient cl, IKubernetesObject<V1ObjectMeta> obj);
        }
        public class ExecuteWrapper<T> : IGenWrapper where T : class, IKubernetesObject<V1ObjectMeta>
        {
            public async Task<IKubernetesObject<V1ObjectMeta>> Save(IKubernetesClient cl, IKubernetesObject<V1ObjectMeta> obj)
            {
                var o = await cl.Get<T>(obj.Metadata.Name, obj.Namespace());
                if (o == null)
                    return (IKubernetesObject<V1ObjectMeta>)await cl.Create<T>((T)obj);
                else
                {
                    if(obj is IAssignableSpec)
                    {
                        ((IAssignableSpec)o).Assign(obj as IAssignableSpec);
                        return (IKubernetesObject<V1ObjectMeta>)await cl.Update<T>(o);
                    }
                    else
                    {
                        ((dynamic)o).Spec = ((dynamic)obj).Spec;
                        return (IKubernetesObject<V1ObjectMeta>)await cl.Update<T>(o);
                    }
                    throw new InvalidOperationException("Cannot assign spec");
                }
            }

            public async Task<IKubernetesObject<V1ObjectMeta>> Create(IKubernetesClient cl, IKubernetesObject<V1ObjectMeta> obj)
            {
                return (IKubernetesObject<V1ObjectMeta>)await cl.Create<T>((T)obj);
            }

            public async Task<IKubernetesObject<V1ObjectMeta>> Update(IKubernetesClient cl, IKubernetesObject<V1ObjectMeta> obj)
            {
                return (IKubernetesObject<V1ObjectMeta>)await cl.Update<T>((T)obj);
            }
        }

        public static async Task<IKubernetesObject<V1ObjectMeta>> SaveObject(this IKubernetesClient cl, IKubernetesObject<V1ObjectMeta> obj)
        {
            var tpWrapper = typeof(ExecuteWrapper<>);
            var tp = obj.GetType();
            var genWrapperTp = tpWrapper.MakeGenericType(tp);
            var exeWrapper = (IGenWrapper)Activator.CreateInstance(genWrapperTp);

            return await exeWrapper.Save(cl, obj);
        }

        public static async Task<IKubernetesObject<V1ObjectMeta>> CreateObject(this IKubernetesClient cl, IKubernetesObject<V1ObjectMeta> obj)
        {
            var tpWrapper = typeof(ExecuteWrapper<>);
            var tp = obj.GetType();
            var genWrapperTp = tpWrapper.MakeGenericType(tp);
            var exeWrapper = (IGenWrapper)Activator.CreateInstance(genWrapperTp);
            return await exeWrapper.Create(cl, obj);
        }

        public static async Task<IKubernetesObject<V1ObjectMeta>> UpdateObject(this IKubernetesClient cl, IKubernetesObject<V1ObjectMeta> obj)
        {
            var tpWrapper = typeof(ExecuteWrapper<>);
            var tp = obj.GetType();
            var genWrapperTp = tpWrapper.MakeGenericType(tp);
            var exeWrapper = (IGenWrapper)Activator.CreateInstance(genWrapperTp);
            return await exeWrapper.Update(cl, obj);
        }

        public static async Task DeleteObject<TObj>(this IKubernetesClient cl, ILogger logger, string nameSpace, string name) where TObj : class, IKubernetesObject<V1ObjectMeta>
        {
            var objRef = await cl.Get<TObj>(
                name,
                nameSpace
            );

            if (objRef == null)
            {
                logger.LogError(
                    "Failed to find agent referee for resource {Name} in namespace {Namespace}",
                    name,
                    nameSpace
                );
            }
            else
            {
                await cl.Delete(objRef);
                logger.LogInformation(
                    "Removed Agent Referee for {Name} in namespace {Namespace}",
                    name,
                    nameSpace
                );
            }
        }
    }
}
