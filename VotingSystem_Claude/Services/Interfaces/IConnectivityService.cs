namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IConnectivityService
    {
        Task<bool> IsConnectedAsync();
        event EventHandler<bool> ConnectivityChanged;
    }
}