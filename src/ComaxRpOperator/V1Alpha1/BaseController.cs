using CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Builder;
using CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Entities;
using k8s;
using k8s.Models;
using KubeOps.KubernetesClient;
using KubeOps.Operator.Controller;
using KubeOps.Operator.Entities;
using KubeOps.Operator.Finalizer;

namespace CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1
{
    public abstract class BaseController<TEntity,TSpec,TState> : IResourceController<TEntity> where TEntity: CustomKubernetesEntity<TSpec, TState>, new() where TSpec: new() where TState: IComaxState, new()
    {
        protected readonly IKubernetesClient _client;
        protected readonly ILogger _logger;
        protected readonly IFinalizerManager<TEntity> _finalizeManager;

        public BaseController(ILogger logger, IFinalizerManager<TEntity> finalizeManager, IKubernetesClient client)
        {
            _logger = logger;
            _finalizeManager = finalizeManager;
            _client = client;
        }

        protected abstract IEnumerable<IKubernetesObject<V1ObjectMeta>> GetWorkload(TEntity entity);

        public abstract Task DeletedAsync(TEntity entity);


        // Summary:
        //     Called for KubeOps.Operator.Kubernetes.ResourceEventType.StatusUpdated events
        //     for a given entity.
        //
        // Parameters:
        //   entity:
        //     The entity that fired the status-modified event.
        //
        // Returns:
        //     A task that completes, when the reconciliation is done.
        protected virtual Task StatusModifiedAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task DeleteBrokenAsync(TEntity entity)
        {
            _logger.LogInformation(
                "Removing deployment for {Name} in namespace {Namespace}",
                entity.Name(),
                entity.Namespace()
            );

            await DeletedAsync(entity);

            _logger.LogInformation(
                "Removed deployment for {Name} in namespace {Namespace}",
                entity.Name(),
                entity.Namespace()
            );
        }

        protected virtual async Task<TEntity> Create(TEntity entity)
        {
            var deployment = GetWorkload(entity);

            foreach (var item in deployment)
            {
                try
                {
                    await _client.SaveObject(item);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return entity;
        }

        protected async Task<TEntity> Update(TEntity entity)
        {
            var deployments = GetWorkload(entity);
            foreach (var item in deployments)
                await _client.SaveObject(item);
            return entity;
        }

        protected async Task<TEntity> UpdateStatus(TEntity entity)
        {
            int cnt = 0;
            bool retry = true;
            while (retry && cnt < 2)
            {
                try
                {
                    cnt++;
                    entity.Status.StateTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    entity.Status.StateTsMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    await _client.UpdateStatus(entity);
                    retry = false;
                }
                catch (k8s.Autorest.HttpOperationException exc)
                {
                    if (exc.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        var state = entity.Status.CurrentState;
                        entity = await _client.Get<TEntity>(entity.Metadata.Name, entity.Namespace());
                        entity.Status.CurrentState = state;
                    }
                    else
                        throw;
                }
            }
            return await _client.Get<TEntity>(entity.Metadata.Name, entity.Namespace());
        }
    }
}
