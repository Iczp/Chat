using IczpNet.Chat.Enums;
using IczpNet.Chat.Sessions;
using System;
using Xunit;

namespace IczpNet.Chat.RoomSections;

public class RoomPolicies_Tests
{
    [Fact]
    public void Session_SetRoomPolicies_Updates_All_Flags_And_Defaults()
    {
        var session = new Session(Guid.NewGuid(), "room:100", Channels.RoomChannel);

        Assert.False(session.IsAllMuted);
        Assert.Equal(RoomInvitePolicies.AllMembers, session.InvitePolicy);
        Assert.Equal(RoomJoinChannels.All, session.AllowJoinChannels);

        session.SetRoomPolicies(
            isAllMuted: true,
            invitePolicy: RoomInvitePolicies.AdminsOnly,
            allowJoinChannels: RoomJoinChannels.Invitation | RoomJoinChannels.QrCode);

        Assert.True(session.IsAllMuted);
        Assert.Equal(RoomInvitePolicies.AdminsOnly, session.InvitePolicy);
        Assert.True(session.AllowJoinChannels.HasFlag(RoomJoinChannels.Invitation));
        Assert.True(session.AllowJoinChannels.HasFlag(RoomJoinChannels.QrCode));
        Assert.False(session.AllowJoinChannels.HasFlag(RoomJoinChannels.Search));
    }
}
