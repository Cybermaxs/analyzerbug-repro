namespace AnalyzerBug
{
    internal class Constants
    {
        public const string TokenSeparator = "¤";
        public const char ChannelSeparator = '.';
        public const string SyncLockObjectCacheKeyPrefix = "¤lock¤";

        public static class Redis
        {
            public const int DefaultRedisDb = 0;
            public const string InvalidationChannel = "dcinvalidate.*";
            public const string FlushChannel = "dcflush.*";
            public const string PushChannel = "dcpush.*";
            public const string KeyEventChannel = "__keyevent*__:*";

            public const string LocalTtlKey = "l_ttl";
            public const string LocalSlidingKey = "l_sld";
            public const string LocalStaleRatioKey = "l_rat";
            public const string RemoteTtlKey = "r_ttl";
            public const string RemoteSlidingKey = "r_sld";
            public const string RemoteStaleRatioKey = "r_rat";
            public const string AddedKey = "added";
            public const string DataKey = "data";
            public const long NotPresent = -1;
        }
    }
}
