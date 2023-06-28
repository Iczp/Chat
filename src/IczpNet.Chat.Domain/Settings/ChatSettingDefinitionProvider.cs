using Castle.Core.Internal;
using IczpNet.Chat.Localization;
using System.ComponentModel;
using System.Linq;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace IczpNet.Chat.Settings;

public class ChatSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        /* Define module settings here.
         * Use names from ChatSettings class.
         */

        var fields = typeof(ChatSettings).GetFields().Where(x => x.IsStatic && x.Name != nameof(ChatSettings.GroupName));

        foreach (var field in fields)
        {
            var defaultValueAttribute = field.GetAttribute<DefaultValueAttribute>();

            if (defaultValueAttribute != null)
            {
                var value = field.GetValue(null)?.ToString();

                context.Add(new SettingDefinition(value, defaultValueAttribute.Value?.ToString(), L(value)));
            }
        }

        //context.Add(new SettingDefinition(ChatSettings.IsMessageSenderEnabled, "True", L(ChatSettings.IsMessageSenderEnabled)));
        //context.Add(new SettingDefinition(ChatSettings.SessionRequestExpirationHours, "72", L(ChatSettings.SessionRequestExpirationHours)));
        //context.Add(new SettingDefinition(ChatSettings.MaxFollowingCount, "10", L(ChatSettings.MaxFollowingCount)));
        //context.Add(new SettingDefinition(ChatSettings.AllowRollbackHours, "24", L(ChatSettings.AllowRollbackHours)));
        //context.Add(new SettingDefinition(ChatSettings.MaxFavoriteSize, long.MaxValue.ToString(), L(ChatSettings.MaxFavoriteSize)));
        //context.Add(new SettingDefinition(ChatSettings.MaxFavoriteCount, long.MaxValue.ToString(), L(ChatSettings.MaxFavoriteCount)));
        //context.Add(new SettingDefinition(ChatSettings.UseBackgroundJobSenderMinSessionUnitCount, "500", L(ChatSettings.UseBackgroundJobSenderMinSessionUnitCount)));
        //context.Add(new SettingDefinition(ChatSettings.MaxSessionUnitCount, "5000", L(ChatSettings.UseBackgroundJobSenderMinSessionUnitCount)));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatResource>(name);
    }
}
