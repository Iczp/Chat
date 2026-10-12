# Chat AutoMapper → Mapperly migration

## Current state (2026-10-10)

The `mapperly` branch uses ABP Mapperly in Chat Domain, Application,
ManagementApplication, the API Host, and the standalone BackgroundWorker. The
old AutoMapper profiles and direct package references are removed. The API and
Worker images were rebuilt and deployed to the local Docker Compose stack.

The nested `SessionUnit.Owner` mapping now composes the existing custom
`ChatObject → ChatObjectInfo` Mapperly mapper using `[UseMapper]`, preserving
URL normalization and connection-derived device types for direct sender,
message, cache, and quote output paths. A focused test covers those paths.
`MessageInfo<dynamic>` and `MessageInfo<object>` are the same CLR type, so only
the object mapper is registered. Active closed `MessageInfo<T>` and TreeManager
uses are checked by `scripts/check-mapperly-coverage.py`; the old
`MessageWithQuoteInfo<>` open-generic declaration has no production caller,
and the checker rejects a newly introduced caller. The computed
`Message.Content` property is read once in the message output mappers.

Full solution build and test completed: Domain 75 passed / 14 skipped / 0
failed; Application 18 passed / 2 skipped / 0 failed; EF Core 3 passed;
MongoDB 1 passed. The two skipped Application tests are obsolete template
`SampleAppService` tests with no implementation. One pre-existing xUnit2031
analyzer warning remains in `MessageReadCache_Tests.cs`. The structural
coverage checker exits 0: Domain 33/33, Application 192/192, Management 77/77,
six active TreeManager pairs, and 13 active message output pairs.

Local Docker smoke tests through the gateway passed: token login, Chat object
and session queries, message history, text send, quoted-message history,
file upload and download via MinIO, and authenticated SignalR negotiation.
Chat API and Worker are up;
the Worker logs show Quartz scheduler startup, `MessageReportFlushWorker`, and
AI dispatcher startup. The Chat containers join both the application and
infrastructure networks; MinIO is reached as `minio:9000` via
`CHAT_MINIO_ENDPOINT`. The earlier `host.docker.internal:9000` setting returned
a non-MinIO response from inside Docker Desktop. No migration was generated or
database schema update executed. Test messages, one small test blob, and test
chat objects/requests remain in the development database/storage. New-session
notifications also passed automatic acceptance, pending request, manual
acceptance, and decline regression. The remaining Quartz/outbox reliability
limit is described in the deployment guide.

Remaining caution for production sign-off: the custom ChatObject mapper
retains the old synchronous wait on `GetDeviceTypesAsync`, which may block
threads under load. The former `MaxDepth(3)` rule has no direct Mapperly
equivalent; the exercised message/history paths did not recurse, but deeper
quote/forward graph and sustained-load testing remain advisable. These are
separate performance and graph-depth investigations, not part of this
behavior-preserving migration.

## Earlier migration plan (historical; not the current runtime state)

This migration is intentionally staged. Do not replace `AddAutoMapperObjectMapper`
in the Chat modules or hosts until every active map has an equivalent and the
message/content regression tests pass. The currently generated Mapperly classes
are compile-checked candidates; the existing profiles remain the runtime source.

## Historical inventory (superseded)

| Project | Active `CreateMap<T, U>` declarations | Compile-checked Mapperly candidates |
| --- | ---: | ---: |
| Domain | 34 unique (35 declarations) | 19 |
| Application | 192 | 108 |
| ManagementApplication | 77 | 0 |

Run `python scripts/check-mapperly-coverage.py` to list uncovered
`CreateMap<T, U>` pairs. It currently exits with code 1: 15 Domain pairs, 84
Application pairs, 77 ManagementApplication pairs, plus three Domain
open-generic declarations remain. The active API Host does not load the
ManagementApplication module, but the independent Chat API Host and Chat
BackgroundWorker both load the Domain and Application modules. The checker is
only a structural gate: a listed Mapperly class still requires behavioral
parity tests, and `ReverseMap`/runtime-dispatched mappings require separate
review. Do not interpret a zero exit code alone as permission to switch the
provider.

The candidate classes cover direct one-way mappings and uncustomized
`ReverseMap()` declarations. Regenerate them with
`python scripts/generate-simple-mapperly.py`. The script deliberately skips
maps with custom resolvers, actions, input-to-entity inaccessible setters,
or other configuration. Successful compilation alone is not parity evidence.

An initial parity check found that the existing AutoMapper map from
`ChatObjectCreateInput` to `ChatObject` sets the protected `Name` property,
while a generated Mapperly map-to-existing instance left it null. The same
class also contains protected `VerificationMethod`, `ChatObjectTypeId`, and
`ObjectType` setters. The two ChatObject input maps now use a dedicated Mapperly
implementation with cached reflection metadata for those protected setters;
ordinary writable fields are generated by Mapperly. Create and update parity
tests pass. This preserves the old mapper's exact state, including its lack of
`SetName` side effects; changing that domain behavior would be a separate task.
The two Menu input maps use the same approach for the inherited protected
`Name` setter, and their create/update parity test passes. The previous
automatic input-to-entity candidates remain excluded.
The ChatObjectType input maps use the public `Id` constructor for creation and
retain the protected parameterless constructor for the map-to-new update case;
their create/update parity test also passes.
The persisted `TextContent` to `TextContentInfo` output direction has its own
Mapperly candidate and parity test. The opposite input direction remains the
existing `TextContentInfoMapper` because it validates text; it must not be
replaced by a generated property-copy mapper.
The concrete `Message` to `MessageInfo<object>` job output mapping now has a
candidate and an old-vs-new parity test for the no-content case; content and
open-generic message mappings remain uncovered. `CmdContent`, `HtmlContent`,
`ContactsContent`, `LinkContent`, and `LocationContent` now have two-way
Mapperly candidates with create/update/output parity tests. Their update tests
also confirm that the existing concurrency stamp is preserved. URL-resolved
content templates are intentionally not included in this plain-content group.

## Required before provider switch

1. Convert all remaining maps, preserving URL normalization, `AfterMap`,
   auditing/identity fields, and input-to-entity update behavior.
2. Replace the open-generic `TreeEntity<,>` and `MessageInfo<>` maps with
   explicit supported mappings or a tested generic mapper.
3. Preserve `Message.GetContentDto()`'s runtime content-type dispatch and
   register every content entity → output DTO pair. `TextContentInfo` →
   `TextContent` already has a custom `IObjectMapper` with validation and must
   not be silently overwritten.
4. Review `ChatObjectServiceStutusResolver` and `UrlResolver` before replacing
   them. The former synchronously waits for async calls; a straight `AfterMap`
   conversion would preserve that latency and thread-blocking behavior.
5. Repair the pre-existing `ManagementApplication.Contracts` compile errors
   (missing types/namespaces) so management mappings can be validated.
6. Add mapping equivalence tests for representative messages, content types,
   update DTOs, collections, and null values. Then change module registrations
   to Mapperly and remove the old profiles/package references in one verified
   step. Set the default provider explicitly in each host while external ABP
   modules still use AutoMapper.

The `MapperlyEquivalence_Tests` cover an ordinary output map plus ChatObject,
Menu, and ChatObjectType create/update input maps. They pass, but this is not
sufficient to approve the provider switch; the remaining behavior tests and
mappings are still needed.

No database migration or deployment is part of this mapping change.

## Historical verification snapshot (superseded)

- Chat API Host and Chat BackgroundWorker build with no warnings or errors.
- Domain Mapperly parity tests: 7 passed. Application parity tests: 5 passed.
- The full Domain test run has 60 passed, 14 skipped, and 3 failures.
  Those three fail during the existing integrated test Host initialization:
  `DynamicPermissionDefinitionStore` cannot resolve
  `IPermissionGroupDefinitionRecordRepository`. They are not mapping
  assertions, but the test Host must be repaired before a full green run.
- `Chat.ManagementApplication.Contracts` currently has 49 compile errors,
  primarily stale `ChatObjectCategorys` / `SessionSections` namespaces and
  missing interface implementations. The active Chat API Host does not load
  this module; migrate it before claiming *all* Chat profiles removed.

## Local AbpTrees during development

Chat's direct `IczpNet.AbpTrees.*` NuGet references are commented out and
replaced by project references to the sibling `IczpNet.AbpTrees/src` checkout.
This includes the shared, domain, application, contracts, management, admin,
and installer projects. The local checkout must therefore exist beside Chat
when building or publishing Chat from source.

At that earlier stage Chat Domain needed an explicit `Volo.Abp.AutoMapper`
reference while its profiles were active. Those profiles and the direct
package reference have since been removed.

Before switching back to NuGet, publish the changed AbpTrees projects under
new package versions, update *all* commented versions consistently, then
replace each project reference with its corresponding package reference. Do
not publish Chat packages that depend on the changed local AbpTrees source
using the unchanged `10.0.0.1061` AbpTrees version: NuGet consumers would
restore the old published implementation instead.
