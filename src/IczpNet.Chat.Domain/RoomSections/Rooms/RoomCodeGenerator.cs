using IczpNet.Pusher.ShortIds;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.RoomSections.Rooms;

public class RoomCodeGenerator(IShortIdGenerator shortIdGenerator) : IRoomCodeGenerator, ITransientDependency
{
    public const string StaticCode = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public static int Length { get; set; } = 12;
    public static string Prefix { get; set; } = "G_";
    public IShortIdGenerator ShortIdGenerator { get; } = shortIdGenerator;

    public string Make()
    {
        return $"{Prefix}{GenerateCode(Length - Prefix.Length)}";
    }

    public async Task<string> MakeAsync()
    {
        await Task.Yield();
        return Make();
    }

    protected virtual string GenerateCode(int length)
    {
        var shortId = ShortIdGenerator.Create() + ShortIdGenerator.Create();

        return shortId.Substring(0, Math.Min(length, shortId.Length));
        //var random = new Random();
        //return new string(Enumerable.Repeat(StaticCode, length)
        //    .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
