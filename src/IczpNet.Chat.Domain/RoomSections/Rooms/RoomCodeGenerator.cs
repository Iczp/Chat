using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.RoomSections.Rooms;

public class RoomCodeGenerator : IRoomCodeGenerator, ITransientDependency
{
    public const string StaticCode = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    protected virtual int Length { get; set; } = 8;
    protected virtual string Prefix { get; set; } = "G_";

    public string Make()
    {
        return $"{Prefix}{GenerateCode(Length - Prefix.Length)}";
    }

    public async Task<string> MakeAsync()
    {
        await Task.Yield();
        return Make();
    }

    protected static string GenerateCode(int length)
    {
        var random = new Random();
        return new string(Enumerable.Repeat(StaticCode, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
