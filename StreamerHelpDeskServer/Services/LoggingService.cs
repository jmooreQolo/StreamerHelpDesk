using StreamerHelpDeskServer.Models;
using System.IO;

namespace StreamerHelpDeskServer.Services
{
    public class LoggingService : IDisposable
    {
        private readonly string _logDirectory;
        private StreamWriter? _serverLogWriter;
        private readonly Dictionary<string, StreamWriter> _chatLogWriters = new();
        private readonly object _lockObject = new();
        private readonly string _serverLogFileName;

        public LoggingService()
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            Directory.CreateDirectory(_logDirectory);

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _serverLogFileName = $"ServerLog_{timestamp}.txt";
            var serverLogPath = Path.Combine(_logDirectory, _serverLogFileName);
            
            _serverLogWriter = new StreamWriter(serverLogPath, append: true)
            {
                AutoFlush = true
            };

            LogServerEvent("Server log started");
        }

        public void LogServerEvent(string message)
        {
            lock (_lockObject)
            {
                if (_serverLogWriter != null)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    _serverLogWriter.WriteLine($"[{timestamp}] {message}");
                }
            }
        }

        public void LogHelpRequest(HelpDeskMessage message)
        {
            var logMessage = $"HELP REQUEST - Client: {message.ClientName}, Category: {message.Category}, Message: {message.MessageText}";
            LogServerEvent(logMessage);
        }

        public void LogClientConnection(string clientName, string connectionId)
        {
            LogServerEvent($"CLIENT CONNECTED - Name: {clientName}, ConnectionId: {connectionId}");
        }

        public void LogClientDisconnection(string clientName, string connectionId)
        {
            LogServerEvent($"CLIENT DISCONNECTED - Name: {clientName}, ConnectionId: {connectionId}");
            
            CloseChatLog(connectionId);
        }

        public void LogChatMessage(string clientName, string connectionId, ChatMessage message)
        {
            lock (_lockObject)
            {
                if (!_chatLogWriters.ContainsKey(connectionId))
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    var chatLogFileName = $"Chat_{clientName}_{timestamp}.txt";
                    var chatLogPath = Path.Combine(_logDirectory, chatLogFileName);
                    
                    _chatLogWriters[connectionId] = new StreamWriter(chatLogPath, append: true)
                    {
                        AutoFlush = true
                    };

                    _chatLogWriters[connectionId].WriteLine($"=== Chat Log with {clientName} ===");
                    _chatLogWriters[connectionId].WriteLine($"Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    _chatLogWriters[connectionId].WriteLine("==========================================");
                    _chatLogWriters[connectionId].WriteLine();
                }

                var chatWriter = _chatLogWriters[connectionId];
                var msgTimestamp = message.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                chatWriter.WriteLine($"[{msgTimestamp}] {message.SenderName}: {message.MessageText}");
            }
        }

        private void CloseChatLog(string connectionId)
        {
            lock (_lockObject)
            {
                if (_chatLogWriters.ContainsKey(connectionId))
                {
                    _chatLogWriters[connectionId].WriteLine();
                    _chatLogWriters[connectionId].WriteLine($"Chat ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    _chatLogWriters[connectionId].Close();
                    _chatLogWriters[connectionId].Dispose();
                    _chatLogWriters.Remove(connectionId);
                }
            }
        }

        public void Dispose()
        {
            lock (_lockObject)
            {
                LogServerEvent("Server log ended");

                _serverLogWriter?.Close();
                _serverLogWriter?.Dispose();
                _serverLogWriter = null;

                foreach (var writer in _chatLogWriters.Values)
                {
                    writer.WriteLine();
                    writer.WriteLine($"Chat ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.Close();
                    writer.Dispose();
                }
                _chatLogWriters.Clear();
            }
        }
    }
}
