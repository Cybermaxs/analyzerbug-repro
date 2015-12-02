using StackExchange.Redis;
using System;

namespace AnalyzerBug.Extensions
{
    internal static class ConnectionMultiplexerHelperExtensions
    {
        public static IServer GetServer(this IConnectionMultiplexer muxer)
        {
            var endpoints = muxer.GetEndPoints();
            IServer result = null;
            foreach (var endpoint in endpoints)
            {
                var server = muxer.GetServer(endpoint);
                if (server.IsSlave || !server.IsConnected) continue;
                if (result != null) throw new InvalidOperationException(string.Format("Requires exactly one master endpoint (found {0} and {1})",server.EndPoint,result.EndPoint));
                result = server;
            }
            if (result == null) throw new InvalidOperationException("Requires exactly one master endpoint (found none)");
            return result;
        }
    }
}
