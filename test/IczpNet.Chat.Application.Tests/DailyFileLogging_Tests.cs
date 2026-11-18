using IczpNet.AbpCommons;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace IczpNet.Chat;

public class DailyFileLogging_Tests
{
    [Fact]
    public void Files_include_all_levels_and_separate_each_level_by_date()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), $"chat-log-test-{Guid.NewGuid():N}");
        try
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["LogFiles:RootPath"] = rootPath,
                ["LogFiles:MinimumLevel"] = "Verbose",
            }).Build();

            using (var logger = new LoggerConfiguration().WithDailyLogFiles(configuration).CreateLogger())
            {
                foreach (var level in Enum.GetValues<LogEventLevel>())
                {
                    logger.ForContext("SourceContext", "LoggingProbe")
                        .Write(level, new InvalidOperationException($"exception-{level}"), "{Probe:l}", $"probe-{level}");
                }
            }

            var allFile = Directory.GetFiles(Path.Combine(rootPath, "All")).Single();
            Path.GetFileName(allFile).ShouldEndWith($"-{DateTime.Now:yyyyMMdd}.log");
            var allText = File.ReadAllText(allFile);
            foreach (var level in Enum.GetValues<LogEventLevel>())
            {
                allText.ShouldContain($"probe-{level}");
                var levelText = File.ReadAllText(Directory.GetFiles(Path.Combine(rootPath, level.ToString())).Single());
                levelText.ShouldContain($"probe-{level}");
                levelText.ShouldContain($"exception-{level}");
                levelText.ShouldContain("LoggingProbe");
                foreach (var otherLevel in Enum.GetValues<LogEventLevel>().Where(other => other != level))
                    levelText.ShouldNotContain($"probe-{otherLevel}");
            }
        }
        finally
        {
            if (Directory.Exists(rootPath)) Directory.Delete(rootPath, true);
        }
    }

    [Fact]
    public void Files_roll_by_size_and_honor_retention()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), $"chat-log-test-{Guid.NewGuid():N}");
        try
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["LogFiles:RootPath"] = rootPath,
                ["LogFiles:FileSizeLimitBytes"] = "512",
                ["LogFiles:RetainedFileCountLimit"] = "2",
            }).Build();
            using (var logger = new LoggerConfiguration().WithDailyLogFiles(configuration).CreateLogger())
            {
                for (var index = 0; index < 8; index++)
                    logger.Error("probe-{Index}: {Payload}", index, new string('x', 512));
            }

            foreach (var category in new[] { "All", "Error" })
            {
                var files = Directory.GetFiles(Path.Combine(rootPath, category));
                files.Length.ShouldBe(2);
                string.Concat(files.Select(File.ReadAllText)).ShouldContain("probe-7:");
            }
        }
        finally
        {
            if (Directory.Exists(rootPath)) Directory.Delete(rootPath, true);
        }
    }
}
