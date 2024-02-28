using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageContentBase_Size : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentLength",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "ContentLength",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropColumn(
                name: "ContentLength",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.RenameColumn(
                name: "ContentLength",
                table: "Chat_Message_Template_FileContent",
                newName: "Size");

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "Chat_Message_Template_VideoContent",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "Chat_Message_Template_ImageContent",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Size",
                table: "Chat_Message_Template_FileContent",
                newName: "ContentLength");

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "Chat_Message_Template_VideoContent",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ContentLength",
                table: "Chat_Message_Template_VideoContent",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ContentLength",
                table: "Chat_Message_Template_SoundContent",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "Chat_Message_Template_ImageContent",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ContentLength",
                table: "Chat_Message_Template_ImageContent",
                type: "bigint",
                nullable: true);
        }
    }
}
