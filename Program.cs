using Azure.Identity;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Microsoft.Graph;
using System.Data;

namespace theDeprecat0r
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please provide the ADX cluster as an argument.");
                Console.WriteLine("Usage: deprecat0r.exe help.kusto.windows.net");
                return;
            }

            var cluster = args[0];
            Console.WriteLine($"Searching for Distribution group usage on cluster {cluster}");
            Console.WriteLine();

            var result = Search(cluster);

            PrintResult(result);

            Console.WriteLine();
            Console.WriteLine("Done, Press enter to QUIT");
            Console.ReadLine();
        }

        private static void PrintResult(DistributionGroupSearchResult result)
        {
            Console.WriteLine($"Based on your identity, the tenant {result.TenantChecked} was used");

            Console.WriteLine();
            Console.WriteLine("RESULT");

            if(result != null)
            {
                if(result.TenantsNotChecked.Count > 0 || result.DistributionGroups.Count > 0)
                {
                    if(result.DistributionGroups.Count >0)
                    {
                        Console.WriteLine($"{result.DistributionGroups.Count} Distribution groups were detected");
                        foreach(var group in result.DistributionGroups)
                        {
                            Console.WriteLine(group);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No distribution group principals were found on this cluster.");
                    }

                    if(result.TenantsNotChecked.Count > 0)
                    {
                        Console.WriteLine("Please execute this command again using an identity of the following tenants:");
                        foreach(var aTenant in result.TenantsNotChecked) 
                        { 
                            Console.WriteLine(aTenant); 
                        }
                    }

                    return;
                }
            }
                
            Console.WriteLine("Nothing to do for you, you are all set. No usage of distribution groups on the cluster you specified.");
        }

        private static DistributionGroupSearchResult Search(string cluster)
        {
            var browserCredentials = new InteractiveBrowserCredential();

            KustoConnectionStringBuilder connectionStringBuilder = new KustoConnectionStringBuilder(cluster).WithAadAzureTokenCredentialsAuthentication(browserCredentials);
            var clientKusto = KustoClientFactory.CreateCslQueryProvider(connectionStringBuilder);

            var clientGraph = new GraphServiceClient(browserCredentials);

            var orgRequestResult = clientGraph.Organization.GetAsync().Result;
            if (orgRequestResult != null)
            {
                var org = orgRequestResult.Value?.Single();
                if (org != null)
                {
                    var myTenantId = org.Id;

                    var tenantsNotChecked = new HashSet<string>();
                    var distributionGroups = new HashSet<DistributionGroup>();
                    DistributionGroupSearchResult result = new DistributionGroupSearchResult() 
                    { 
                        TenantsNotChecked = tenantsNotChecked, DistributionGroups = distributionGroups,
                        TenantChecked = myTenantId
                    };

                    var databasesResult = clientKusto.ExecuteQuery(".show databases | project DatabaseName");

                    string queryForPrincipals = "";

                    while (databasesResult.Read())
                    {
                        var database = databasesResult.GetString(0);
                        queryForPrincipals = $".show database {database} principals | project PrincipalFQN, PrincipalDisplayName | parse-where PrincipalFQN with principalType:string \"=\" principalIdentity:string \";\" principalTenant:string | project-away PrincipalFQN | where principalType == \"aadgroup\" | project-away principalType";
                        CheckPrincipals(clientKusto, clientGraph, myTenantId, tenantsNotChecked, distributionGroups, queryForPrincipals);
                    }

                    queryForPrincipals = $".show cluster principals \r\n| project PrincipalFQN, PrincipalDisplayName\r\n| parse-where PrincipalFQN with principalType:string \"=\" principalIdentity:string \";\" principalTenant:string \r\n| project-away PrincipalFQN\r\n| where principalType == \"aadgroup\"\r\n| project-away principalType";
                    CheckPrincipals(clientKusto, clientGraph, myTenantId, tenantsNotChecked, distributionGroups, queryForPrincipals);

                    return result;
                }
            }

            return null;
        }

        private static void CheckPrincipals(ICslQueryProvider clientKusto, GraphServiceClient clientGraph, string? myTenantId, HashSet<string> tenantsNotChecked, HashSet<DistributionGroup> distributionGroups, string queryForPrincipals)
        {
            IDataReader principalsResult = clientKusto.ExecuteQuery(queryForPrincipals);
            while (principalsResult.Read())
            {
                var displayName = principalsResult.GetString(0);
                var groupId = principalsResult.GetString(1);
                var tenantId = principalsResult.GetString(2);
                CheckGroup(clientGraph, myTenantId, tenantsNotChecked, distributionGroups, displayName, groupId, tenantId);
            }
        }

        private static void CheckGroup(GraphServiceClient clientGraph, string? myTenantId, HashSet<string> tenantsNotChecked, HashSet<DistributionGroup> distributionGroups, string displayName, string groupId, string tenantId)
        {
            if (tenantId.Equals(myTenantId))
            {
                try
                {
                    var groupRequest = clientGraph.Groups[groupId].GetAsync().Result;
                    if (groupRequest != null && groupRequest.SecurityEnabled.HasValue)
                    {
                        if (!groupRequest.SecurityEnabled.Value)
                        {
                            DistributionGroup dg = new DistributionGroup() { DisplayName = displayName, GroupId = groupId, TenantId = tenantId };
                            distributionGroups.Add(dg);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            else
            {
                tenantsNotChecked.Add(tenantId);
            }
        }
    }
}