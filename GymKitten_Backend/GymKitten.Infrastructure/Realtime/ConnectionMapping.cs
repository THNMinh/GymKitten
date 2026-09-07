using System.Collections.Concurrent;

namespace GymKitten.Infrastructure.Realtime;

public class ConnectionMapping
{
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _connections = new();

    public void Add(Guid userId, string connectionId)
    {
        _connections.AddOrUpdate(
            userId,
            new HashSet<string> { connectionId },
            (_, existing) =>
            {
                lock (existing) { existing.Add(connectionId); }
                return existing;
            });
    }

    public void Remove(Guid userId, string connectionId)
    {
        if (_connections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _connections.TryRemove(userId, out _);
                }
            }
        }
    }

    public IEnumerable<string> GetConnections(Guid userId)
    {
        return _connections.TryGetValue(userId, out var connections)
            ? connections.ToList()
            : Enumerable.Empty<string>();
    }

    public bool IsOnline(Guid userId)
    {
        return _connections.TryGetValue(userId, out var connections) && connections.Count > 0;
    }

    public IEnumerable<Guid> GetOnlineUsers()
    {
        return _connections.Keys.ToList();
    }
}
