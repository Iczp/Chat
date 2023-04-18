using IczpNet.Chat.Localization;
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

        context.Add(
            new SettingDefinition(ChatSettings.SessionRequestExpirationHours, "72", L(ChatSettings.SessionRequestExpirationHours))
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatResource>(name);
    }
}
