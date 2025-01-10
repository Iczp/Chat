using IczpNet.Chat.ChatObjects;

namespace IczpNet.Chat.TextTemplates;

public class ChatObjectTextTemplate : TextTemplate
{
    /// <summary>
    /// defaultValue:<![CDATA[<a oid="{ChatObjectId}">{ChatObjectName}</a>]]>
    /// </summary>
    public static string Template { get; set; } = "<a oid=\"{ChatObjectId}\">{ChatObjectName}</a>";

    public override string Text { get; protected set; } = Template;

    public long ChatObjectId { get; set; }

    public string ChatObjectName { get; set; }

    public ChatObjectTextTemplate(long chatObjectId, string chatObjectName)
    {
        ChatObjectId = chatObjectId;
        ChatObjectName = chatObjectName;
        SetData();
    }

    public ChatObjectTextTemplate(IChatObject chatObject)
    {
        ChatObjectId = chatObject.Id;
        ChatObjectName = chatObject.Name;
        SetData();
    }

    private void SetData()
    {
        Data[nameof(ChatObjectId)] = ChatObjectId;
        Data[nameof(ChatObjectName)] = ChatObjectName;
    }

    public override string ToString()
    {
        SetData();
        return base.ToString();
    }
}
