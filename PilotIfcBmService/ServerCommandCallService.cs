using Ascon.Pilot.Server.Api.Contracts;
using Ascon.Pilot.Transport;

namespace PilotIfcBmService;

class ServerCommandCallService<T> : ICallService
{
    private readonly IServerApi _serverApi;
    private readonly ServerCallback _callback;
    private readonly string _commandName;

    public ServerCommandCallService(IServerApi serverApi, ServerCallback callback, string? processorName = null)
    {
        _serverApi = serverApi;
        _callback = callback;
        _commandName = CommandNameParser.GetCommandName(typeof(T).Name, processorName);
    }

    public byte[] Get(string data)
    {
        throw new NotSupportedException();
    }

    public byte[] Call(ICallData data)
    {
        return CallAsync(data).GetAwaiter().GetResult();
    }

    public async Task<byte[]> CallAsync(ICallData data)
    {
        var requestId = Guid.NewGuid();
        _serverApi.InvokeCommand(_commandName, requestId, data.GetBytes());
        var result = await _callback.WaitCommandResult(requestId);
        switch (result.result)
        {
            case ServerCommandResult.Success:
                return result.data;
            case ServerCommandResult.Error:
                throw TransportClient.ReadException(result.data);
            default:
                throw new InvalidOperationException("Unable to deserialize server command response");
        }
    }
}