using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageStat_Fix_Index_SessionId_MessageType_IsUnique_False : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_AppVersionDevice_Chat_AppVersion_DeviceId",
                table: "Chat_AppVersionDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_AppVersionDevice_Chat_Device_DeviceId",
                table: "Chat_AppVersionDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_AppVersionDeviceGroup_Chat_AppVersion_AppVersionId",
                table: "Chat_AppVersionDeviceGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_AppVersionDeviceGroup_Chat_DeviceGroup_DeviceGroupId",
                table: "Chat_AppVersionDeviceGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_BlobContent_Chat_Blob_BlobId",
                table: "Chat_BlobContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ChatObjectEntryValue_Chat_ChatObject_OwnerId",
                table: "Chat_ChatObjectEntryValue");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ConnectionChatObject_Chat_ChatObject_ChatObjectId",
                table: "Chat_ConnectionChatObject");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ConnectionChatObject_Chat_Connection_ConnectionId",
                table: "Chat_ConnectionChatObject");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_DeletedRecorder_Chat_Message_MessageId",
                table: "Chat_DeletedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_DeletedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_DeletedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Developer_Chat_ChatObject_OwnerId",
                table: "Chat_Developer");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_DeviceGroupMap_Chat_DeviceGroup_DeviceGroupId",
                table: "Chat_DeviceGroupMap");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_DeviceGroupMap_Chat_Device_DeviceId",
                table: "Chat_DeviceGroupMap");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_FavoritedRecorder_Chat_Message_MessageId",
                table: "Chat_FavoritedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_FavoritedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_FavoritedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId",
                table: "Chat_HttpResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_MessageFollower_Chat_Message_MessageId",
                table: "Chat_MessageFollower");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_MessageFollower_Chat_SessionUnit_SessionUnitId",
                table: "Chat_MessageFollower");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_MessageReminder_Chat_Message_MessageId",
                table: "Chat_MessageReminder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_MessageReminder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_MessageReminder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_Message_MessageId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_Message_MessageId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Scoped_Chat_Message_MessageId",
                table: "Chat_Scoped");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Scoped_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Scoped");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_TextContentWord_Chat_Message_Template_TextContent_TextContentId",
                table: "Chat_TextContentWord");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_UserDevice_Chat_Device_DeviceId",
                table: "Chat_UserDevice");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageStat_SessionId_MessageType",
                table: "Chat_MessageStat");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_SessionId_MessageType",
                table: "Chat_MessageStat",
                columns: new[] { "SessionId", "MessageType" });

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_AppVersionDevice_Chat_AppVersion_AppVersionId",
                table: "Chat_AppVersionDevice",
                column: "AppVersionId",
                principalTable: "Chat_AppVersion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_AppVersionDevice_Chat_Device_DeviceId",
                table: "Chat_AppVersionDevice",
                column: "DeviceId",
                principalTable: "Chat_Device",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_AppVersionDeviceGroup_Chat_AppVersion_AppVersionId",
                table: "Chat_AppVersionDeviceGroup",
                column: "AppVersionId",
                principalTable: "Chat_AppVersion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_AppVersionDeviceGroup_Chat_DeviceGroup_DeviceGroupId",
                table: "Chat_AppVersionDeviceGroup",
                column: "DeviceGroupId",
                principalTable: "Chat_DeviceGroup",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_BlobContent_Chat_Blob_BlobId",
                table: "Chat_BlobContent",
                column: "BlobId",
                principalTable: "Chat_Blob",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ChatObjectEntryValue_Chat_ChatObject_OwnerId",
                table: "Chat_ChatObjectEntryValue",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ConnectionChatObject_Chat_ChatObject_ChatObjectId",
                table: "Chat_ConnectionChatObject",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ConnectionChatObject_Chat_Connection_ConnectionId",
                table: "Chat_ConnectionChatObject",
                column: "ConnectionId",
                principalTable: "Chat_Connection",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_DeletedRecorder_Chat_Message_MessageId",
                table: "Chat_DeletedRecorder",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_DeletedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_DeletedRecorder",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Developer_Chat_ChatObject_OwnerId",
                table: "Chat_Developer",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_DeviceGroupMap_Chat_DeviceGroup_DeviceGroupId",
                table: "Chat_DeviceGroupMap",
                column: "DeviceGroupId",
                principalTable: "Chat_DeviceGroup",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_DeviceGroupMap_Chat_Device_DeviceId",
                table: "Chat_DeviceGroupMap",
                column: "DeviceId",
                principalTable: "Chat_Device",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_FavoritedRecorder_Chat_Message_MessageId",
                table: "Chat_FavoritedRecorder",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_FavoritedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_FavoritedRecorder",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId",
                table: "Chat_HttpResponse",
                column: "HttpRequestId",
                principalTable: "Chat_HttpRequest",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_MessageFollower_Chat_Message_MessageId",
                table: "Chat_MessageFollower",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_MessageFollower_Chat_SessionUnit_SessionUnitId",
                table: "Chat_MessageFollower",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_MessageReminder_Chat_Message_MessageId",
                table: "Chat_MessageReminder",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_MessageReminder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_MessageReminder",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_Message_MessageId",
                table: "Chat_OpenedRecorder",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_OpenedRecorder",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_Message_MessageId",
                table: "Chat_ReadedRecorder",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_ReadedRecorder",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Scoped_Chat_Message_MessageId",
                table: "Chat_Scoped",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Scoped_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Scoped",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_TextContentWord_Chat_Message_Template_TextContent_TextContentId",
                table: "Chat_TextContentWord",
                column: "TextContentId",
                principalTable: "Chat_Message_Template_TextContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_UserDevice_Chat_Device_DeviceId",
                table: "Chat_UserDevice",
                column: "DeviceId",
                principalTable: "Chat_Device",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_AppVersionDevice_Chat_AppVersion_AppVersionId",
                table: "Chat_AppVersionDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_AppVersionDevice_Chat_Device_DeviceId",
                table: "Chat_AppVersionDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_AppVersionDeviceGroup_Chat_AppVersion_AppVersionId",
                table: "Chat_AppVersionDeviceGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_AppVersionDeviceGroup_Chat_DeviceGroup_DeviceGroupId",
                table: "Chat_AppVersionDeviceGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_BlobContent_Chat_Blob_BlobId",
                table: "Chat_BlobContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ChatObjectEntryValue_Chat_ChatObject_OwnerId",
                table: "Chat_ChatObjectEntryValue");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ConnectionChatObject_Chat_ChatObject_ChatObjectId",
                table: "Chat_ConnectionChatObject");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ConnectionChatObject_Chat_Connection_ConnectionId",
                table: "Chat_ConnectionChatObject");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_DeletedRecorder_Chat_Message_MessageId",
                table: "Chat_DeletedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_DeletedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_DeletedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Developer_Chat_ChatObject_OwnerId",
                table: "Chat_Developer");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_DeviceGroupMap_Chat_DeviceGroup_DeviceGroupId",
                table: "Chat_DeviceGroupMap");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_DeviceGroupMap_Chat_Device_DeviceId",
                table: "Chat_DeviceGroupMap");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_FavoritedRecorder_Chat_Message_MessageId",
                table: "Chat_FavoritedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_FavoritedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_FavoritedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId",
                table: "Chat_HttpResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_MessageFollower_Chat_Message_MessageId",
                table: "Chat_MessageFollower");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_MessageFollower_Chat_SessionUnit_SessionUnitId",
                table: "Chat_MessageFollower");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_MessageReminder_Chat_Message_MessageId",
                table: "Chat_MessageReminder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_MessageReminder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_MessageReminder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_Message_MessageId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_Message_MessageId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Scoped_Chat_Message_MessageId",
                table: "Chat_Scoped");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Scoped_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Scoped");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_TextContentWord_Chat_Message_Template_TextContent_TextContentId",
                table: "Chat_TextContentWord");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_UserDevice_Chat_Device_DeviceId",
                table: "Chat_UserDevice");

            migrationBuilder.DropIndex(
                name: "IX_Chat_MessageStat_SessionId_MessageType",
                table: "Chat_MessageStat");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_MessageStat_SessionId_MessageType",
                table: "Chat_MessageStat",
                columns: new[] { "SessionId", "MessageType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_AppVersionDevice_Chat_AppVersion_DeviceId",
                table: "Chat_AppVersionDevice",
                column: "DeviceId",
                principalTable: "Chat_AppVersion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_AppVersionDevice_Chat_Device_DeviceId",
                table: "Chat_AppVersionDevice",
                column: "DeviceId",
                principalTable: "Chat_Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_AppVersionDeviceGroup_Chat_AppVersion_AppVersionId",
                table: "Chat_AppVersionDeviceGroup",
                column: "AppVersionId",
                principalTable: "Chat_AppVersion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_AppVersionDeviceGroup_Chat_DeviceGroup_DeviceGroupId",
                table: "Chat_AppVersionDeviceGroup",
                column: "DeviceGroupId",
                principalTable: "Chat_DeviceGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_BlobContent_Chat_Blob_BlobId",
                table: "Chat_BlobContent",
                column: "BlobId",
                principalTable: "Chat_Blob",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ChatObjectEntryValue_Chat_ChatObject_OwnerId",
                table: "Chat_ChatObjectEntryValue",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ConnectionChatObject_Chat_ChatObject_ChatObjectId",
                table: "Chat_ConnectionChatObject",
                column: "ChatObjectId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ConnectionChatObject_Chat_Connection_ConnectionId",
                table: "Chat_ConnectionChatObject",
                column: "ConnectionId",
                principalTable: "Chat_Connection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_DeletedRecorder_Chat_Message_MessageId",
                table: "Chat_DeletedRecorder",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_DeletedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_DeletedRecorder",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Developer_Chat_ChatObject_OwnerId",
                table: "Chat_Developer",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_DeviceGroupMap_Chat_DeviceGroup_DeviceGroupId",
                table: "Chat_DeviceGroupMap",
                column: "DeviceGroupId",
                principalTable: "Chat_DeviceGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_DeviceGroupMap_Chat_Device_DeviceId",
                table: "Chat_DeviceGroupMap",
                column: "DeviceId",
                principalTable: "Chat_Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_FavoritedRecorder_Chat_Message_MessageId",
                table: "Chat_FavoritedRecorder",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_FavoritedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_FavoritedRecorder",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_HttpResponse_Chat_HttpRequest_HttpRequestId",
                table: "Chat_HttpResponse",
                column: "HttpRequestId",
                principalTable: "Chat_HttpRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_MessageFollower_Chat_Message_MessageId",
                table: "Chat_MessageFollower",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_MessageFollower_Chat_SessionUnit_SessionUnitId",
                table: "Chat_MessageFollower",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_MessageReminder_Chat_Message_MessageId",
                table: "Chat_MessageReminder",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_MessageReminder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_MessageReminder",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_Message_MessageId",
                table: "Chat_OpenedRecorder",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_OpenedRecorder",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_Message_MessageId",
                table: "Chat_ReadedRecorder",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_ReadedRecorder",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Scoped_Chat_Message_MessageId",
                table: "Chat_Scoped",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Scoped_Chat_SessionUnit_SessionUnitId",
                table: "Chat_Scoped",
                column: "SessionUnitId",
                principalTable: "Chat_SessionUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_TextContentWord_Chat_Message_Template_TextContent_TextContentId",
                table: "Chat_TextContentWord",
                column: "TextContentId",
                principalTable: "Chat_Message_Template_TextContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_UserDevice_Chat_Device_DeviceId",
                table: "Chat_UserDevice",
                column: "DeviceId",
                principalTable: "Chat_Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
