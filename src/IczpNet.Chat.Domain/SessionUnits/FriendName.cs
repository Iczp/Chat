using System.Linq;
namespace IczpNet.Chat.SessionUnits;

public readonly struct FriendName(string index, string abbr, string name)
{
    public string Index { get; } = NormalizeIndex(index);
    public string Abbreviation { get; } = NormalizeAbbr(abbr);
    public string Name { get; } = name ?? string.Empty;

    // Parse:  Z:ZS:张三
    public static FriendName Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new FriendName("#", "", "");

        int p1 = input.IndexOf(':');

        // 没有 :
        if (p1 < 0)
        {
            var contact = input;
            var abbr = GetAbbr(contact);
            return new FriendName(GetIndex(abbr), abbr, contact);
        }

        int p2 = input.IndexOf(':', p1 + 1);

        // 只有一个 :
        if (p2 < 0)
        {
            var index = input[..p1];
            var contact = input[(p1 + 1)..];
            var abbr = GetAbbr(contact);
            return new FriendName(index, abbr, contact);
        }

        // 标准三段
        var idx = input[..p1];
        var ab = input[(p1 + 1)..p2];
        var name = input[(p2 + 1)..];

        return new FriendName(idx, ab, name);
    }

    // TryParse
    public static bool TryParse(string input, out FriendName result)
    {
        try
        {
            result = Parse(input);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    // 解构
    public void Deconstruct(out string index, out string abbr, out string name)
    {
        index = Index;
        abbr = Abbreviation;
        name = Name;
    }

    // ToString（反解构）
    public override string ToString()
        => $"{Index}:{Abbreviation}:{Name}";

    // 隐式转换
    public static implicit operator FriendName(string input)
        => Parse(input);

    public static implicit operator string(FriendName f)
        => f.ToString();

    // ------------------------
    // 内部工具方法
    // ------------------------

    private static string NormalizeIndex(string index)
    {
        if (string.IsNullOrWhiteSpace(index))
            return "#";

        char c = index[0];
        return char.IsLetter(c) ? char.ToUpper(c).ToString() : "#";
    }

    /// <summary>
    /// 简写：只允许 A-Z，其它全部清空
    /// </summary>
    private static string NormalizeAbbr(string abbr)
    {
        if (string.IsNullOrWhiteSpace(abbr))
            return "";

        var chars = abbr.ToUpper().Where(c => c >= 'A' && c <= 'Z').ToArray();
        return new string(chars);
    }

    /// <summary>
    /// 从 Contact 自动生成简写（可接拼音库）
    /// </summary>
    private static string GetAbbr(string contact)
    {
        if (string.IsNullOrWhiteSpace(contact))
            return "";

        // 默认简单版（英文）
        if (char.IsLetter(contact[0]))
            return char.ToUpper(contact[0]).ToString();

        return "";
    }

    private static string GetIndex(string abbr)
    {
        if (string.IsNullOrWhiteSpace(abbr))
            return "#";

        return abbr[0].ToString();
    }
}