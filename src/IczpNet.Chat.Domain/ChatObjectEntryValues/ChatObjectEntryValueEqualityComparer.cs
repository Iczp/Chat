using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace IczpNet.Chat.ChatObjectEntryValues
{
    public class ChatObjectEntryValueEqualityComparer : IEqualityComparer<ChatObjectEntryValue>
    {
        public bool Equals(ChatObjectEntryValue x, ChatObjectEntryValue y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return x.OwnerId == y.OwnerId && x.EntryValueId == y.EntryValueId;
        }

        public int GetHashCode([DisallowNull] ChatObjectEntryValue obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            return obj.GetHashCode();
        }
    }
}
