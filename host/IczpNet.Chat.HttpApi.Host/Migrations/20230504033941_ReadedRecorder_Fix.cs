using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class ReadedRecorder_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_ChatObject_OwnerId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_Message_MessageId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_ReadedRecorder",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ReadedRecorder_MessageId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropColumn(
                name: "MessageAutoId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Chat_ReadedRecorder",
                newName: "SessionUnitId");

            migrationBuilder.AlterColumn<long>(
                name: "OwnerId",
                table: "Chat_ReadedRecorder",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "MessageId",
                table: "Chat_ReadedRecorder",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_ReadedRecorder",
                table: "Chat_ReadedRecorder",
                columns: new[] { "MessageId", "SessionUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ReadedRecorder_SessionUnitId",
                table: "Chat_ReadedRecorder",
                column: "SessionUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_ChatObject_OwnerId",
                table: "Chat_ReadedRecorder",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_ChatObject_OwnerId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_Message_MessageId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_ReadedRecorder",
                table: "Chat_ReadedRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_ReadedRecorder_SessionUnitId",
                table: "Chat_ReadedRecorder");

            migrationBuilder.RenameColumn(
                name: "SessionUnitId",
                table: "Chat_ReadedRecorder",
                newName: "Id");

            migrationBuilder.AlterColumn<long>(
                name: "OwnerId",
                table: "Chat_ReadedRecorder",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "MessageId",
                table: "Chat_ReadedRecorder",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Chat_ReadedRecorder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Chat_ReadedRecorder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat_ReadedRecorder",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "MessageAutoId",
                table: "Chat_ReadedRecorder",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "Chat_ReadedRecorder",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_ReadedRecorder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_ReadedRecorder",
                table: "Chat_ReadedRecorder",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ReadedRecorder_MessageId",
                table: "Chat_ReadedRecorder",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_ChatObject_OwnerId",
                table: "Chat_ReadedRecorder",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_ReadedRecorder_Chat_Message_MessageId",
                table: "Chat_ReadedRecorder",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");
        }
    }
}
