using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IczpNet.Chat.QrLogins;

public class StringTemplateBuilder(string template)
{
    public string Template => template ?? throw new ArgumentNullException(nameof(template));

    private readonly Dictionary<string, string> _values = new();      // 构造替换用
    private readonly Dictionary<string, string> _patterns = new();    // 自定义正则规则
    private Dictionary<string, string> _parsedValues;                // 解析结果缓存
    private string _cachedRegexPattern;

    #region 构造与正则设置

    public StringTemplateBuilder Set(string key, object value)
    {
        _values[key] = value?.ToString() ?? string.Empty;
        return this;
    }

    public StringTemplateBuilder Match(string key, string pattern)
    {
        _patterns[key] = pattern ?? throw new ArgumentNullException(nameof(pattern));
        return this;
    }

    public override string ToString()
    {
        string result = Template;
        foreach (var kv in _values)
        {
            result = result.Replace($"{{{kv.Key}}}", kv.Value);
        }
        return result;
    }

    public string BuildRegex()
    {
        if (_cachedRegexPattern != null)
            return _cachedRegexPattern;

        string pattern = Regex.Escape(Template)
            .Replace(@"\{", "{")
            .Replace(@"\}", "}");

        // 把 {xxx} 替换为命名捕获组
        pattern = Regex.Replace(pattern, @"\{([^}]+)\}", m =>
        {
            var key = m.Groups[1].Value;
            var regex = _patterns.TryGetValue(key, out var defined)
                ? defined
                : @"[^/?&#]+"; // 默认宽松匹配
            return $"(?<{key}>{regex})";
        });

        _cachedRegexPattern = $"^{pattern}$";
        return _cachedRegexPattern;
    }

    #endregion

    #region 解析

    public StringTemplateBuilder ToValues(string input)
    {
        if (!TryToValues(input, out _))
        {
            throw new InvalidOperationException($"输入字符串与模板不匹配：{Template}");
        }
        return this;
    }

    public bool TryToValues(string input, out IReadOnlyDictionary<string, string> values)
    {
        values = null;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var pattern = BuildRegex();
        var regex = new Regex(pattern);
        var match = regex.Match(input);

        if (!match.Success)
            return false;

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in regex.GetGroupNames())
        {
            if (name != "0")
                dict[name] = match.Groups[name].Value;
        }

        _parsedValues = dict;
        values = dict;
        return true;
    }

    /// <summary>
    /// 链式解析方法（等价于 ToValues）
    /// </summary>
    public StringTemplateBuilder Parse(string input)
    {
        return ToValues(input);
    }

    #endregion

    #region 获取值（带缓存与默认值）

    private void EnsureParsed()
    {
        if (_parsedValues != null)
            return;

        // 自动创建空值字典，防止异常
        _parsedValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match m in Regex.Matches(Template, @"\{([^}]+)\}"))
        {
            _parsedValues[m.Groups[1].Value] = string.Empty;
        }
    }

    public bool HasKey(string key)
    {
        EnsureParsed();
        return _parsedValues!.ContainsKey(key);
    }

    public string Get(string key, string defaultValue = null)
    {
        EnsureParsed();
        return _parsedValues!.TryGetValue(key, out var val)
            ? val
            : defaultValue ?? throw new KeyNotFoundException($"未找到键：{key}");
    }

    public bool TryGet(string key, out string value)
    {
        EnsureParsed();
        return _parsedValues!.TryGetValue(key, out value);
    }

    public IReadOnlyDictionary<string, string> GetAll()
    {
        EnsureParsed();
        return new Dictionary<string, string>(_parsedValues!);
    }

    #endregion
}
