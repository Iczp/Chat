using System.Collections.Generic;

namespace IczpNet.Chat.TextTemplates
{
    public class TextTemplate
    {
        public virtual IDictionary<string, object> Data { get; protected set; } = new Dictionary<string, object>();

        public virtual string Text { get; protected set; }

        public TextTemplate() { }

        public TextTemplate(string text)
        {
            Text = text;
        }

        public TextTemplate(Dictionary<string, object> data)
        {
            Data = data;
        }

        public TextTemplate(string text, IDictionary<string, object> data)
        {
            Text = text;
            Data = data;
        }

        public TextTemplate WithData(string key, object value)
        {
            Data[key] = value;
            return this;
        }

        protected virtual string Rendering()
        {
            var result = Text;

            foreach (var item in Data)
            {
                result = result.Replace("{" + item.Key + "}", $"{item.Value}");
            }
            return result;
        }

        public override string ToString()
        {
            return Rendering();
        }
    }
}
