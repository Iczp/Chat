# Chat AutoMapper → Mapperly migration

## Current state (2026-10-10)

The `mapperly` branch now registers ABP Mapperly in Chat Domain, Application,
ManagementApplication, the API Host, and the standalone BackgroundWorker.
The old AutoMapper profiles and direct package references are removed from the
working tree. `dotnet build IczpNet.Chat.sln --no-restore` succeeds. This is a
**compile milestone, not a behavior-parity or production-release sign-off**.

`python scripts/check-mapperly-coverage.py` compares the current replacement
classes against the most recent Git revision retaining each old profile. It
reports 34/34 Domain, 192/192 Application, and 77/77 Management closed pairs,
but still exits nonzero for the three old open-generic declarations. The
closed-pair count is structural and does not verify custom actions, resolvers,
null behavior, or nested mapping behavior.

Known runtime parity issues to resolve before release:

1. Generated `Message → MessageInfo<T>` mappings originally evaluated the
   computed `Message.Content` property twice when non-null. The cache,
   concrete `MessageInfo<object>`, `MessageAnyInfo`, and currently registered
   closed `MessageInfo<T>` mappings now read it once and are regression-tested.
   Future message output mappers must preserve this rule.
2. Generated nested `SessionUnit.Owner → ChatObjectInfo` mappings directly copy
   `Thumbnail`, `Portrait`, and `DeviceTypes`. The old configured ChatObject
   mapping normalized URLs and obtained device types through
   `IConnectionPoolManager`. The direct `SessionUnit → SessionUnitSenderInfo`
   mapping and message mappings that include a sender unit need a behavioral
   equivalent, preferably covered by a focused regression test.
3. The old message profiles used `MaxDepth(3)`. The generated nested mappings
   have no equivalent depth policy; verify quote and sender-unit graphs do not
   recurse or overfetch in the actual API paths.
4. The new `ChatObject → ChatObjectInfo` mapper still waits synchronously on
   `GetDeviceTypesAsync` (`.Result`), preserving the old behavior but also its
   high-concurrency thread-blocking risk. Any redesign should be explicit and
   benchmarked, not hidden in a mapper substitution.
5. Several old-vs-new parity tests were changed to Mapperly-only assertions.
   Keep representative golden-output tests for custom URL, nested content,
   create/update, null, and open-generic cases.
6. `MessageInfo<dynamic>` and `MessageInfo<object>` are the same CLR closed
   generic type. Separate mapper classes for both create duplicate provider
   registrations; consolidate them after confirming all callers, rather than
   relying on registration order.

The BackgroundWorker now calls `UseStaticObjectMapper()` during initialization;
`Message.GetContentDto()` requires that static bridge for runtime content-type
dispatch. The API Host already initialized it.

Current full test runs: Domain 68 passed / 14 skipped / 3 failed; Application
18 passed / 2 failed. The five failures all occur while initializing the
existing test Host because `DynamicPermissionDefinitionStore` cannot resolve
`IPermissionGroupDefinitionRecordRepository`; they are not mapper assertions.
The targeted Mapperly tests pass (Domain 15/15, Application 6/6). No migration
or database update was generated or executed.

## Earlier migration plan (historical; not the current runtime state)

This migration is intentionally staged. Do not replace `AddAutoMapperObjectMapper`
in the Chat modules or hosts until every active map has an equivalent and the
message/content regression tests pass. The currently generated Mapperly classes
are compile-checked candidates; the existing profiles remain the runtime source.

## Current inventory

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

## Verification snapshot

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

The Chat Domain still needs an explicit `Volo.Abp.AutoMapper` reference while
its existing profiles are active; the old AbpTrees NuGet package previously
provided that dependency transitively. Do not remove it until the remaining
Chat profiles are converted and tested.

Before switching back to NuGet, publish the changed AbpTrees projects under
new package versions, update *all* commented versions consistently, then
replace each project reference with its corresponding package reference. Do
not publish Chat packages that depend on the changed local AbpTrees source
using the unchanged `10.0.0.1061` AbpTrees version: NuGet consumers would
restore the old published implementation instead.
