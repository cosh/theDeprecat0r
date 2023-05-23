namespace theDeprecat0r
{
    public class DistributionGroupSearchResult
    {
        public HashSet<string> TenantsNotChecked { get; internal set; }
        public HashSet<DistributionGroup> DistributionGroups { get; internal set; }
        public string? TenantChecked { get; internal set; }
    }
}