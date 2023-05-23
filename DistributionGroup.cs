namespace theDeprecat0r
{
    public class DistributionGroup
    {
        public string DisplayName { get; internal set; }
        public string GroupId { get; internal set; }
        public string TenantId { get; internal set; }

        public override bool Equals(object? obj)
        {
            return obj is DistributionGroup group &&
                   DisplayName == group.DisplayName &&
                   GroupId == group.GroupId &&
                   TenantId == group.TenantId;
        }

        public override int GetHashCode()
        {
            return GroupId.GetHashCode();
        }

        public override string? ToString()
        {
            return $"TenantId: {TenantId}, GroupId: {GroupId}, DisplayName: {DisplayName}";
        }
    }
}