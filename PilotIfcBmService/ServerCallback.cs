using System.Collections.Concurrent;
using Ascon.Pilot.DataClasses;
using Ascon.Pilot.Server.Api.Contracts;
using Ascon.Pilot.Transport;

namespace PilotIfcBmService
{
    class ServerCallback : IServerCallback
    {
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<(byte[] data, ServerCommandResult result)>> _commandCompletions = new();
        
        public Task<(byte[] data, ServerCommandResult result)> WaitCommandResult(Guid requestId)
        {
            return GetCommandCompletion(requestId).Task;
        }

        public void NotifyCommandResult(Guid requestId, byte[] data, ServerCommandResult result)
        {
            GetCommandCompletion(requestId).SetResult((data, result));
        }

        private TaskCompletionSource<(byte[] data, ServerCommandResult result)> GetCommandCompletion(Guid requestId) => _commandCompletions.GetOrAdd(requestId, _ => new TaskCompletionSource<(byte[] data, ServerCommandResult result)>());

        public void NotifyChangeset(DChangeset changeset)
        {
        }

        public void NotifyChangeAsyncCompleted(DChangeset changeset)
        {
        }

        public void NotifyChangeAsyncError(Guid identity, ProtoExceptionInfo exceptionInfo)
        {
        }

        public void NotifyOrganisationUnitChangeset(OrganisationUnitChangeset changeset)
        {
        }

        public void NotifyPersonChangeset(PersonChangeset changeset)
        {
        }

        public void NotifyDMetadataChangeset(DMetadataChangeset changeset)
        {
        }

        public void NotifySearchResult(DSearchResult searchResult)
        {
        }

        public void NotifyGeometrySearchResult(DGeometrySearchResult searchResult)
        {
        }

        public void NotifyDNotificationChangeset(DNotificationChangeset changeset)
        {
        }

        public void NotifyCustomNotification(string name, byte[] data)
        {
        }

        public void NotifyAccessChangeset(Guid objectId)
        {
        }
    }
}
