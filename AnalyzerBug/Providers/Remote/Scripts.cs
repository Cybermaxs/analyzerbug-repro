namespace AnalyzerBug.Providers.Remote
{
    class Scripts
    {
        public const string Refresh = @" 
                redis.call('HMSET', @Key, 'added', @Ticks)
                if @RemoteTtl ~= '-1' then
                    if @RemoteRatio ~= '-1' then
                        redis.call('EXPIRE', @Key, @RemoteTtl+@RemoteTtl*@RemoteRatio)
                    else
                        redis.call('EXPIRE', @Key, @RemoteTtl)
                    end
                end
                return 1";

        public const string Set = @" 
                redis.call('HMSET', @Key, 'l_ttl', @LocalTtl, 'l_sld', @LocalSliding, 'l_rat', @LocalRatio, 'r_ttl', @RemoteTtl, 'r_sld', @RemoteSliding, 'r_rat', @RemoteRatio, 'added', @Ticks, 'data', @Data)
                if @RemoteTtl ~= '-1' then
                    if @RemoteRatio ~= '-1' then
                        redis.call('EXPIRE', @Key, @RemoteTtl+@RemoteTtl*@RemoteRatio)
                    else
                        redis.call('EXPIRE', @Key, @RemoteTtl)
                    end
                end
                return 1";
    }
}
