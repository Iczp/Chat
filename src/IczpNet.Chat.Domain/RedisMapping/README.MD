下面是完整、可直接放进项目的 `RedisMapper.cs` 文件，满足你最新要求：

https://chatgpt.com/c/6928f90a-1448-832d-b258-d08283c9a2d1

- 支持全部常见类型（string, bool, int/long/float/double/decimal, Guid, DateTime(ISO8601), TimeSpan（ms），enum，nullable 等）。
- 支持嵌套对象扁平化，字段名使用 `Parent.Child` 格式。
- 支持集合：`List<T>` 会展开为 `Deps[0].Name`、`Deps[1].Name`；原始元素会写为 `Deps[0]`。
- 支持字典：`Dictionary<string,T>` 会展开为 `Dict[Key].Name` 或原始 `Dict[Key]`。
- `null` 写为空字符串 `""`（RedisValue.EmptyString）。
- 数字类型写为 Redis 的数字（不加引号）：long / double 等以数值形式写入。
- `HashSetField` 有 `IDatabase` 与 `IBatch` 两个重载（支持 batch）。
- 使用反射属性元数据缓存以提高性能（type -> PropertyInfo[] & helper）。
- 提供 `ToHashEntries(object)` / `ToObject<T>(HashEntry[])` / `HashSetField` / `HashSetFields` 等常用方法。
- DateTime 使用 ISO8601 (`"o"`) 存/读；TimeSpan 存为总毫秒数（number）。

把下列内容保存为 `RedisMapper.cs`，并放入你的项目（命名空间可按需修改，当前用 `Rctea.IM.RedisMapping`）。

使用说明（简短）：

- 将文件放入项目 `Rctea.IM.RedisMapping` 命名空间下（或改命名空间以匹配项目）。
- 写入对象到 Redis（示例）：

```c#
var item = new SessionUnitStoreItem {
    PublicBadge = 5,
    Settings = new SessionUnitSettingCacheItem {
        HistoryLastTime = DateTime.UtcNow,
        MemberName = "Tom"
    },
    Deps = new List<Dep> {
        new Dep { Name = "A" },
        new Dep { Name = "B" }
    },
    Tags = new List<string> { "x", "y" },
    Map = new Dictionary<string, Sub> {
        ["k1"] = new Sub { Name = "S1" }
    }
};

var entries = RedisMapper.ToHashEntries_V2(item); // or ToHashEntries_V2 is provided as robust flatten (see file)
await db.HashSetAsync(key, entries);
```

- 读取对象：

```c#
var entries = await db.HashGetAllAsync(key);
var item2 = RedisMapper.ToObject<SessionUnitStoreItem>(entries);
```

- 更新某个字段（支持 batch）：

```c#
// single
await RedisMapper.HashSetFieldAsync(db, key, "Settings.HistoryLastTime", DateTime.UtcNow);

// batch
var batch = db.CreateBatch();
RedisMapper.HashSetField(batch, key, "Settings.MemberName", "Alice");
RedisMapper.HashSetField(batch, key, "PublicBadge", 10);
batch.Execute();
```

备注与建议：

- 我把 `DateTime` 存为 ISO8601（UTC），TimeSpan 存为总毫秒数（numeric）。
- 集合与字典按照你要求展开成 `Deps[0].Name` 或 `Dict[key].Name`；对于原始集合元素写 `Deps[0]`。
- 反序列化到集合/字典会尝试还原（基于存在的字段）；对于非常复杂/稀疏的场景，你可能需要后处理或提供辅助信息（例如预先知道集合元素类型）。
- 我在实现中启用了属性反射缓存（`_propsCache`）来提升性能。

如果你要，我可以继续：

- 把 `RedisMapper` 拆成两个文件（`RedisMapper.cs` + `RedisMapper.Extensions.cs`），并把 `ToHashEntries_V2` 替换为 `ToHashEntries` 的稳定实现（当前示例包含两个版本以便参考）。
- 根据你的项目类型（.NET 6/7/8）调整 `Activator.CreateInstance` 等行为（比如 DI-friendly).
- 为 `SessionUnitStoreItem` 与 `SessionUnitSettingCacheItem` 自动生成映射测试用例（xUnit）。

要我现在把这个文件写入你项目的具体路径（如 `/mnt/data/RedisMapper.cs`）或把 `ToHashEntries_V2` 名称替换为 `ToHashEntries` 的稳定实现并清理冗余版本吗？