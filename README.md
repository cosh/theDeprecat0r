# theDeprecat0r

The tool will output a list of all the distribution groups that are being used on a given ADX cluster.

## How to get theDeprecat0r

You can either download the pre built executables from the GitHub releases or build it yourself.

## How to use the tool

```
deprecat0r.exe <your cluster>
```

Example:
```
Usage: deprecat0r.exe help.kusto.windows.net
```

### Example Output 1:
No distribution groups but the tool needs to be reexecuted with a different identity.

```
Searching for Distribution group usage on cluster help.kusto.windows.net

Based on your identity, the tenant XXX-XXX-XXX-XXX-XXX was used

RESULT
No distribution group principals were found on this cluster.

Groups of other tenants were found. Please execute this command again using an identity of the following tenants:
YYY-YYY-YYY-YYY-YYY
ZZZ-ZZZ-ZZZ-ZZZ-ZZZ

Done, Press enter to QUIT
```

### Example Output 2:
Two distribution groups were found.

```
Searching for Distribution group usage on cluster help.kusto.windows.net

Based on your identity, the tenant XXX-XXX-XXX-XXX-XXX was used

RESULT
2 Distribution groups were detected
TenantId: XXX-XXX-XXX-XXX-XXX, GroupId: 123, DisplayName: Some Distribution Group
TenantId: XXX-XXX-XXX-XXX-XXX, GroupId: 456, DisplayName: Some Other Distribution Group

No distribution group principals were found on this cluster.

Done, Press enter to QUIT
```

### Example Output 3:
All good, nothing was found.

```
Searching for Distribution group usage on cluster help.kusto.windows.net

Based on your identity, the tenant XXX-XXX-XXX-XXX-XXX was used

RESULT
Nothing to do for you, you are all set. No usage of distribution groups on the cluster you specified.

Done, Press enter to QUIT
```
