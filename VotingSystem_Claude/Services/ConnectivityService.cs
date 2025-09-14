using VotingSystem_Claude.Services.Interfaces;
using System.Net.NetworkInformation;

namespace VotingSystem_Claude.Services
{
    public class ConnectivityService : IConnectivityService
    {
        private readonly ILogger<ConnectivityService> _logger;
        private bool _lastConnectionState = true;

        public event EventHandler<bool>? ConnectivityChanged;

        public ConnectivityService(ILogger<ConnectivityService> logger)
        {
            _logger = logger;
            
            // Monitor network changes
            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
        }

        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync("8.8.8.8", 3000); // Google DNS
                var isConnected = reply.Status == IPStatus.Success;
                
                if (isConnected != _lastConnectionState)
                {
                    _lastConnectionState = isConnected;
                    ConnectivityChanged?.Invoke(this, isConnected);
                }
                
                return isConnected;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check network connectivity");
                return false;
            }
        }

        private async void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
        {
            var isConnected = await IsConnectedAsync();
            ConnectivityChanged?.Invoke(this, isConnected);
        }
    }
}