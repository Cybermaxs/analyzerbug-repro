using AnalyzerBug.Models;
using System;

namespace AnalyzerBug.Providers.Remote
{
    class ScriptParameters
    {
        public string Key { get; private set; }
        public long LocalTtl { get; private set; }
        public bool LocalSliding { get; private set; }
        public double LocalRatio { get; private set; }
        public long RemoteTtl { get; private set; }
        public bool RemoteSliding { get; private set; }
        public double RemoteRatio { get; private set; }
        public byte[] Data { get; private set; }
        public long Ticks { get; private set; }


        public ScriptParameters(string key, ItemPolicy policy)
        {
            if (string.IsNullOrEmpty(key) || policy == null)
                return;

            Key = key;
            LocalTtl = policy.LocalTtl.HasValue ? (long)policy.LocalTtl.Value.TotalSeconds : Constants.Redis.NotPresent;
            LocalSliding = policy.LocalIsSliding;
            LocalRatio = policy.LocalStaleRatio;
            RemoteTtl = policy.RemoteTtl.HasValue ? (long)policy.RemoteTtl.Value.TotalSeconds : Constants.Redis.NotPresent;
            RemoteSliding = policy.RemoteIsSliding;
            RemoteRatio = policy.RemoteStaleRatio;
            Ticks = DateTime.UtcNow.Ticks;
        }

        public ScriptParameters(string key, ItemPolicy policy, DateTime added, byte[] data)
        {
            if (string.IsNullOrEmpty(key) || policy == null)
                return;

            Key = key;
            LocalTtl = policy.LocalTtl.HasValue ? (long)policy.LocalTtl.Value.TotalSeconds : Constants.Redis.NotPresent;
            LocalSliding = policy.LocalIsSliding;
            LocalRatio = policy.LocalStaleRatio;
            RemoteTtl = policy.RemoteTtl.HasValue ? (long)policy.RemoteTtl.Value.TotalSeconds : Constants.Redis.NotPresent;
            RemoteSliding = policy.RemoteIsSliding;
            RemoteRatio = policy.RemoteStaleRatio;
            Ticks = added.Ticks;
            Data = data;
        }
    }
}
