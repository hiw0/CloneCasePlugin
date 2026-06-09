# CloneCasePlugin

A Dynamics 365 / Dataverse plugin that clones a Case (Incident) record. Triggered from a Custom Action, it copies all fields from the source incident except IDs and audit/status fields, then returns the new record's GUID.

## What it does

- Receives an `EntityReference` to an existing `incident` as `Target`.
- Retrieves the full record.
- Creates a new `incident` copying every attribute except: `incidentid`, `createdon`, `modifiedon`, `createdby`, `modifiedby`, `ownerid`, `statecode`, `statuscode`, `ticketnumber`.
- Sets the new record to active (`statecode=0`, `statuscode=1`).
- Returns the new GUID as `ClonedCaseId` (output parameter).

## Build

Open `CloneCasePlugin.csproj` in Visual Studio. Targets .NET Framework 4.6.2 and references the Dataverse SDK v9 via `packages.config`.

```
msbuild CloneCasePlugin.csproj /p:Configuration=Release
```

## Register

1. Open the **Plugin Registration Tool** (part of the Power Platform SDK).
2. Connect to your environment and register the built assembly (`bin\Release\CloneCasePlugin.dll`).
3. Create a **Custom Action** on the `incident` entity with an input parameter `Target` (`EntityReference`) and an output parameter `ClonedCaseId` (`Guid`).
4. Bind a new step on the custom action to `CloneCasePlugin.CloneCasePlugin`.

## Calling it

From a form ribbon button, JavaScript:

```javascript
Xrm.WebApi.execute({
  getMetadata: () => ({
    boundParameter: null,
    operationType: 0,
    operationName: "new_CloneIncident",
    parameterTypes: {
      Target: { typeName: "mscrm.incident", structuralProperty: 5 },
    },
  }),
  Target: { entityType: "incident", id: incidentId },
});
```

## Limitations

- Fields are copied via `ColumnSet(true)` with a fixed denylist, so any new custom column gets copied automatically. For production use, prefer an explicit allowlist or a configurable field mapping.
- Only the `incident` row itself is cloned. Related records (notes/attachments, activities, connections) are not.

## Notes

- `Pluginkey.snk` is the strong-name signing key. Replace it for any forked or production use.
