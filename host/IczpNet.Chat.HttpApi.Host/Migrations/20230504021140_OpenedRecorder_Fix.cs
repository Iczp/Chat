using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class OpenedRecorder_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_ChatObject_OwnerId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_Message_MessageId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_OpenedRecorder",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_OpenedRecorder_MessageId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropColumn(
                name: "MessageAutoId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Chat_OpenedRecorder",
                newName: "SessionUnitId");

            migrationBuilder.AlterColumn<long>(
                name: "OwnerId",
                table: "Chat_OpenedRecorder",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "MessageId",
                table: "Chat_OpenedRecorder",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_OpenedRecorder",
                table: "Chat_OpenedRecorder",
                columns: new[] { "MessageId", "SessionUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OpenedRecorder_SessionUnitId",
                table: "Chat_OpenedRecorder",
                column: "SessionUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_ChatObject_OwnerId",
                table: "Chat_OpenedRecorder",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_ChatObject_OwnerId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_Message_MessageId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_SessionUnit_SessionUnitId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat_OpenedRecorder",
                table: "Chat_OpenedRecorder");

            migrationBuilder.DropIndex(
                name: "IX_Chat_OpenedRecorder_SessionUnitId",
                table: "Chat_OpenedRecorder");

            migrationBuilder.RenameColumn(
                name: "SessionUnitId",
                table: "Chat_OpenedRecorder",
                newName: "Id");

            migrationBuilder.AlterColumn<long>(
                name: "OwnerId",
                table: "Chat_OpenedRecorder",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "MessageId",
                table: "Chat_OpenedRecorder",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Chat_OpenedRecorder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Chat_OpenedRecorder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat_OpenedRecorder",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "MessageAutoId",
                table: "Chat_OpenedRecorder",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Chat_OpenedRecorder",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat_OpenedRecorder",
                table: "Chat_OpenedRecorder",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_OpenedRecorder_MessageId",
                table: "Chat_OpenedRecorder",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_ChatObject_OwnerId",
                table: "Chat_OpenedRecorder",
                column: "OwnerId",
                principalTable: "Chat_ChatObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_OpenedRecorder_Chat_Message_MessageId",
                table: "Chat_OpenedRecorder",
                column: "MessageId",
                principalTable: "Chat_Message",
                principalColumn: "Id");
        }
    }
}
