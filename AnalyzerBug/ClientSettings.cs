using AnalyzerBug.Serialization;
using System.Runtime.Caching;

namespace AnalyzerBug.Client
{
    public class ClientSettings
    {
        public ClientSettings()
        {
            Namespace = string.Empty;
        }

        /// <summary>
        /// Allow to use a local cache.
        /// </summary>
        public bool AllowLocal { get; set; }

        public MemoryCache TargetCache { get; set; }

        /// <summary>
        /// Allow to use a remote cache server.
        /// </summary>
        public bool AllowRemote { get; set; }

        public string RedisConnectionInfo { get; set; }

        public int DefaultRedisDb { get; set; }

        public IMessageSerializer MessageSerializer { get; set; }

        public string Namespace { get; set; }

        public bool TryValidate(out string error)
        {
            error=string.Empty;

            if (!AllowLocal && !AllowRemote)
            {
                error = "Local & Remote Cache are disabled. At least one provider should be activated.";
                return false;
            }

            if (AllowRemote && RedisConnectionInfo==null)
            {
                error = "Remote is enabled but without config.";
                return false;
            }

            if (AllowRemote && MessageSerializer == null)
            {
                error = "A Message Serializer is required with a remote cache.";
                return false;
            }

            return true;
        }
    }
}
