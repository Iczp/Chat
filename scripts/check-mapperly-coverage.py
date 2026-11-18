"""Report legacy Chat mapping pairs without a Mapperly/custom replacement.

Deleted profiles are read from the Git baseline so removing them cannot make
the migration gate report a false success.
"""

from pathlib import Path
import re
import subprocess


ROOT = Path(__file__).resolve().parents[1]
PROJECTS = (
    "IczpNet.Chat.Domain",
    "IczpNet.Chat.Application",
    "IczpNet.Chat.ManagementApplication",
)

# Mapperly/ABP registers closed pairs; these are all TreeManager and
# MessageSender instantiations used by Chat at present. Add a pair here and a
# runtime registration test when introducing a new tree or message type.
ACTIVE_TREE_PAIRS = {
    ("ChatObject", "ChatObjectInfo"),
    ("ChatObjectCategory", "ChatObjectCategoryInfo"),
    ("SessionOrganization", "SessionOrganizationInfo"),
    ("SessionPermissionGroup", "SessionPermissionGroupInfo"),
    ("Menu", "MenuInfo"),
    ("EntryName", "TreeInfo<Guid>"),
}
ACTIVE_MESSAGE_PAIRS = {
    ("Message", f"MessageInfo<{content}>") for content in (
        "object", "TextContentInfo", "CmdContentInfo", "LinkContentInfo",
        "HtmlContentInfo", "ImageContentInfo", "SoundContentInfo",
        "VideoContentInfo", "FileContentInfo", "LocationContentInfo",
        "ContactsContentInfo", "HistoryContentOutput",
        "RedEnvelopeContentOutput",
    )
}


def baseline_profiles(project: str):
    commits = subprocess.run(
        ["git", "rev-list", "HEAD", "--", f"src/{project}"],
        cwd=ROOT, capture_output=True, text=True, check=True,
    ).stdout.splitlines()
    baseline = None
    paths = []
    for commit in commits:
        result = subprocess.run(
            ["git", "ls-tree", "-r", "--name-only", commit, "--", f"src/{project}"],
            cwd=ROOT, capture_output=True, text=True, check=True,
        )
        paths = [path for path in result.stdout.splitlines()
                 if path.endswith("AutoMapperProfile.cs")]
        if paths:
            baseline = commit
            break
    if baseline is None:
        raise RuntimeError(f"No historic AutoMapper profiles found for {project}")
    for path in paths:
        current = ROOT / path
        if current.is_file():
            yield current.name, current.read_text(encoding="utf-8-sig", errors="replace")
        else:
            original = subprocess.run(
                ["git", "show", f"{baseline}:{path}"], cwd=ROOT,
                capture_output=True, check=True,
            )
            yield current.name, original.stdout.decode("utf-8-sig", errors="replace")


def type_arguments(text: str, start: int):
    depth = 1
    for index in range(start, len(text)):
        if text[index] == "<":
            depth += 1
        elif text[index] == ">":
            depth -= 1
            if depth == 0:
                return text[start:index], index + 1
    raise ValueError("Unclosed generic type argument list")


def pair(arguments: str):
    # C# dynamic is System.Object in a closed generic type at runtime.
    normalize = lambda value: re.sub(r"\bdynamic\b", "object", value.strip())
    depth = 0
    for index, char in enumerate(arguments):
        if char == "<":
            depth += 1
        elif char == ">":
            depth -= 1
        elif char == "," and depth == 0:
            return normalize(arguments[:index]), normalize(arguments[index + 1 :])
    raise ValueError(f"Expected source and destination types: {arguments}")


for project in PROJECTS:
    project_dir = ROOT / "src" / project
    declared = set()
    open_generic = []
    for profile_name, source in baseline_profiles(project):
        for line in source.splitlines():
            if not re.match(r"^\s*CreateMap", line):
                continue
            if "CreateMap(typeof(" in line:
                open_generic.append((profile_name, line.strip()))
                continue
            match = re.search(r"CreateMap<", line)
            if match:
                arguments, _ = type_arguments(line, match.end())
                declared.add(pair(arguments))

    provided = set()
    for mapper in project_dir.rglob("*.cs"):
        if "obj" in mapper.parts or "bin" in mapper.parts:
            continue
        if mapper.name.endswith("AutoMapperProfile.cs"):
            continue
        source = mapper.read_text(encoding="utf-8-sig", errors="replace")
        for match in re.finditer(r"\b(TwoWayMapperBase|MapperBase|IObjectMapper)<", source):
            if source.rfind("//", 0, match.start()) > source.rfind("\n", 0, match.start()):
                continue
            arguments, _ = type_arguments(source, match.end())
            if not arguments.strip() or "," not in arguments:
                continue
            types = pair(arguments)
            provided.add(types)
            if match.group(1) == "TwoWayMapperBase":
                provided.add((types[1], types[0]))

    missing = sorted(declared - provided)
    print(f"{project}: {len(declared)} declared; {len(declared) - len(missing)} provided; {len(missing)} missing")
    for source_type, destination_type in missing:
        print(f"  {source_type} -> {destination_type}")
    unresolved_generics = []
    for profile, declaration in open_generic:
        if "TreeEntity<,>" in declaration:
            missing_closed = ACTIVE_TREE_PAIRS - provided
            status = f"{len(ACTIVE_TREE_PAIRS) - len(missing_closed)}/{len(ACTIVE_TREE_PAIRS)} active closed tree pairs"
        elif "MessageInfo<>" in declaration:
            missing_closed = ACTIVE_MESSAGE_PAIRS - provided
            status = f"{len(ACTIVE_MESSAGE_PAIRS) - len(missing_closed)}/{len(ACTIVE_MESSAGE_PAIRS)} active closed message pairs"
        elif "MessageWithQuoteInfo<>" in declaration:
            # The non-generic MessageWithQuoteInfo mapping remains registered.
            # Fail if a production caller starts using the old open generic.
            callers = []
            for source_dir in (ROOT / "src", ROOT / "host"):
                for candidate in source_dir.rglob("*.cs"):
                    if "obj" in candidate.parts or "bin" in candidate.parts:
                        continue
                    if candidate.name == "MessageInfo.cs":
                        continue
                    if "MessageWithQuoteInfo<" in candidate.read_text(
                        encoding="utf-8-sig", errors="replace"
                    ):
                        callers.append(str(candidate.relative_to(ROOT)))
            missing_closed = {("Message", caller) for caller in callers}
            status = "retired: no active generic call site" if not callers else "new generic caller found"
        else:
            missing_closed = {("unknown", declaration)}
            status = "unclassified"
        print(f"  OPEN GENERIC {profile}: {declaration} -> {status}")
        for source_type, destination_type in sorted(missing_closed):
            print(f"    MISSING CLOSED {source_type} -> {destination_type}")
        unresolved_generics.extend(missing_closed)
    if missing or unresolved_generics:
        print()

    if missing or unresolved_generics:
        exit_code = 1

raise SystemExit(globals().get("exit_code", 0))
