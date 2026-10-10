"""Generate first-pass Mapperly classes for conventional CreateMap declarations.

Only plain maps and plain ReverseMap declarations are converted. Input maps
with inaccessible setters need manual migration: AutoMapper can populate
protected setters that Mapperly leaves untouched.
The generated classes are compiled alongside the original profiles until the
entire module can switch mapping providers atomically.
"""

from pathlib import Path
import re


ROOT = Path(__file__).resolve().parents[1]
PROJECTS = (
    "IczpNet.Chat.Domain",
    "IczpNet.Chat.Application",
    "IczpNet.Chat.ManagementApplication",
)
MANUAL_MAPPINGS = {
    ("SessionUnit", "SessionUnitDestinationDetailDto"),
    ("Developer", "DeveloperDto"),
    ("GenerateInfo", "GeneratedDto"),
}


def parse_pair(line: str):
    declaration = line.split("//", 1)[0].strip()
    match = re.fullmatch(
        r"CreateMap<(.+)>\((MemberList\.(?:Source|None))?\)"
        r"((?:\.(?:ReverseMap|IgnoreAllSourcePropertiesWithAnInaccessibleSetter|IgnoreAllPropertiesWithAnInaccessibleSetter)\(\))*)?;",
        declaration,
    )
    if match is None:
        return None

    arguments = match.group(1)
    reverse = ".ReverseMap()" in (match.group(3) or "")
    relaxed = match.group(2) is not None
    if relaxed:
        return None
    depth = 0
    for index, char in enumerate(arguments):
        if char == "<":
            depth += 1
        elif char == ">":
            depth -= 1
        elif char == "," and depth == 0:
            return arguments[:index].strip(), arguments[index + 1 :].strip(), reverse
    raise ValueError(f"Cannot split CreateMap type arguments: {declaration}")


for project in PROJECTS:
    mapper_directory = ROOT / "src" / project / "AutoMappers"
    for profile in mapper_directory.glob("*AutoMapperProfile.cs"):
        source = profile.read_text(encoding="utf-8-sig")
        pairs = []
        seen_pairs = set()
        for line in source.splitlines():
            pair = parse_pair(line)
            if pair and pair[:2] not in MANUAL_MAPPINGS and pair[:2] not in seen_pairs:
                pairs.append(pair)
                seen_pairs.add(pair[:2])
        if not pairs:
            continue

        namespace = re.search(r"^namespace\s+([^;{]+);", source, re.MULTILINE)
        if namespace is None:
            raise ValueError(f"File-scoped namespace not found in {profile}")

        usings = [
            line
            for line in source.splitlines()
            if line.startswith("using ")
            and line not in ("using AutoMapper;", "using Volo.Abp.AutoMapper;")
        ]
        usings.extend(("using Riok.Mapperly.Abstractions;", "using Volo.Abp.Mapperly;"))
        lines = ["// Generated from conventional CreateMap declarations. Keep complex maps in the profile until migrated."]
        lines.extend(dict.fromkeys(usings))
        lines.extend(("", f"namespace {namespace.group(1).strip()};", ""))

        prefix = profile.stem.removesuffix("AutoMapperProfile")
        names = {}
        for source_type, destination_type, reverse in pairs:
            source_name = re.sub(r"[^A-Za-z0-9_]", "", source_type)
            destination_name = re.sub(r"[^A-Za-z0-9_]", "", destination_type)
            base_name = f"{prefix}{source_name}To{destination_name}Mapper"
            names[base_name] = names.get(base_name, 0) + 1
            name = base_name if names[base_name] == 1 else f"{base_name}{names[base_name]}"
            base = "TwoWayMapperBase" if reverse else "MapperBase"
            lines.extend(
                (
                    "[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]",
                    f"public partial class {name} : {base}<{source_type}, {destination_type}>",
                    "{",
                )
            )
            lines.append(f"    public override partial {destination_type} Map({source_type} source);")
            lines.extend(("", f"    public override partial void Map({source_type} source, {destination_type} destination);"))
            if reverse:
                lines.extend(
                    (
                        "",
                        f"    public override partial {source_type} ReverseMap({destination_type} source);",
                        "",
                        f"    public override partial void ReverseMap({destination_type} source, {source_type} destination);",
                    )
                )
            lines.extend(("}", ""))

        output = profile.with_name(f"{prefix}SimpleMapperlyMappings.cs")
        output.write_text("\n".join(lines), encoding="utf-8")
        print(f"{output.relative_to(ROOT)}: {len(pairs)} mappings")
