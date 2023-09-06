using System.Diagnostics;
using ProtoBuf;

namespace PilotIfcBmService
{
    public interface IBimOperationsManager
    {
        Task<string> GetVersion();
        Task<IReadOnlyList<ServerOperation>> GetRecentServerOperations(int limit, int offset);
        Task<ServerOperation> GetOperation(Guid operationId);
        Task RequestCancelProcessing(Guid operationId); 
        Task RequestRepeatProcessing(Guid operationId);
    }

    public interface IProvideDto<T>
    {
        T Dto { get; }
    }

    [ProtoContract]
    public class ServerOperation : IProvideDto<ServerOperation>
    {
        [ProtoMember(1)]
        public Guid Id;
        [ProtoMember(2)]
        public ServerOperationStatus Status;
        [ProtoMember(3)]
        public ServerOperationKind Kind;
        [ProtoMember(4)]
        public string FileName;
        [ProtoMember(5)]
        public Guid IfcObjectId;
        [ProtoMember(6)]
        public DateTime TimeStampUtc;
        [ProtoMember(7)]
        public int UserId;
        [ProtoMember(8)]
        public int ProgressValue;
        [ProtoMember(9)]
        public string Error;
        [ProtoMember(10)]
        public Guid SettingsSource;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ServerOperation IProvideDto<ServerOperation>.Dto => this;
    }

    [ProtoContract]
    public enum ServerOperationKind
    {
        Undefined = 0,
        Creation = 1,
        Update = 2,
        Rebuild = 3,
        Conversion = 4,
        CollisionsDetection = 5,
    }

    [ProtoContract]
    public enum ServerOperationStatus
    {
        Undefined = 0,
        Started = 1,
        ReadingFile = 2,
        BuildingStructure = 3,
        BuildingBodies = 4,
        BuildingTessellations = 5,
        Uploading = 6,
        Done = 7,
        Canceled = 8,
        Error = 9,
        CancellationRequested = 10,
        Downloading = 11,
        Converting = 12,
        Queued = 13,
        CloudPointCounting = 14,
        CloudPointDistributing = 15,
        CloudPointIndexing = 16,
        ModelUpdating = 17
    }
}
