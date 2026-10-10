"""Report legacy Chat mapping pairs that lack a source-generated or custom mapper.

This is a migration gate, not an automatic converter. Run it before removing
the AutoMapper profiles; it exits nonzero while declared mappings are missing.
"""

from pathlib import Path
import re


ROOT = Path(__file__).resolve().parents[1]
PROJECTS = (
    "IczpNet.Chat.Domain",
    "IczpNet.Chat.Application",
    "IczpNet.Chat.ManagementApplication",
)


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
    depth = 0
    for index, char in enumerate(arguments):
        if char == "<":
            depth += 1
        elif char == ">":
            depth -= 1
        elif char == "," and depth == 0:
            return arguments[:index].strip(), arguments[index + 1 :].strip()
    raise ValueError(f"Expected source and destination types: {arguments}")


for project in PROJECTS:
    project_dir = ROOT / "src" / project
    declared = set()
    open_generic = []
    for profile in project_dir.rglob("*AutoMapperProfile.cs"):
        if "obj" in profile.parts or "bin" in profile.parts:
            continue
        source = profile.read_text(encoding="utf-8-sig", errors="replace")
        for line in source.splitlines():
            if not re.match(r"^\s*CreateMap", line):
                continue
            if "CreateMap(typeof(" in line:
                open_generic.append((profile.name, line.strip()))
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
    for profile, declaration in open_generic:
        print(f"  OPEN GENERIC {profile}: {declaration}")
    if missing or open_generic:
        print()

    if missing or open_generic:
        exit_code = 1

raise SystemExit(globals().get("exit_code", 0))
