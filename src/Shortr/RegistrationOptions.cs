namespace Shortr
{
    public readonly struct RegistrationOptions
    {
        public RegistrationOptions(string key, string url, int? ttl)
        {
            Key = key;
            Url = url;
            Ttl = ttl;
        }

        public string Key { get; }
        public string Url { get; }
        public int? Ttl { get; }
    }
}
