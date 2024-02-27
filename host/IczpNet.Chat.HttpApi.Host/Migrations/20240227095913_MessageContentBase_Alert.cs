using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    /// <inheritdoc />
    public partial class MessageContentBase_Alert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BlobId",
                table: "Chat_Message_Template_VideoContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ContentLength",
                table: "Chat_Message_Template_VideoContent",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Chat_Message_Template_VideoContent",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Suffix",
                table: "Chat_Message_Template_VideoContent",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BlobId",
                table: "Chat_Message_Template_SoundContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ContentLength",
                table: "Chat_Message_Template_SoundContent",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Chat_Message_Template_SoundContent",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Suffix",
                table: "Chat_Message_Template_SoundContent",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BlobId",
                table: "Chat_Message_Template_ImageContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ContentLength",
                table: "Chat_Message_Template_ImageContent",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Chat_Message_Template_ImageContent",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Suffix",
                table: "Chat_Message_Template_ImageContent",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BlobId",
                table: "Chat_Message_Template_FileContent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_VideoContent_BlobId",
                table: "Chat_Message_Template_VideoContent",
                column: "BlobId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_SoundContent_BlobId",
                table: "Chat_Message_Template_SoundContent",
                column: "BlobId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_ImageContent_BlobId",
                table: "Chat_Message_Template_ImageContent",
                column: "BlobId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Message_Template_FileContent_BlobId",
                table: "Chat_Message_Template_FileContent",
                column: "BlobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_FileContent_Chat_Blob_BlobId",
                table: "Chat_Message_Template_FileContent",
                column: "BlobId",
                principalTable: "Chat_Blob",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_ImageContent_Chat_Blob_BlobId",
                table: "Chat_Message_Template_ImageContent",
                column: "BlobId",
                principalTable: "Chat_Blob",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_SoundContent_Chat_Blob_BlobId",
                table: "Chat_Message_Template_SoundContent",
                column: "BlobId",
                principalTable: "Chat_Blob",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Template_VideoContent_Chat_Blob_BlobId",
                table: "Chat_Message_Template_VideoContent",
                column: "BlobId",
                principalTable: "Chat_Blob",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_FileContent_Chat_Blob_BlobId",
                table: "Chat_Message_Template_FileContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_ImageContent_Chat_Blob_BlobId",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_SoundContent_Chat_Blob_BlobId",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Template_VideoContent_Chat_Blob_BlobId",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_VideoContent_BlobId",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_SoundContent_BlobId",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_ImageContent_BlobId",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Message_Template_FileContent_BlobId",
                table: "Chat_Message_Template_FileContent");

            migrationBuilder.DropColumn(
                name: "BlobId",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "ContentLength",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "Suffix",
                table: "Chat_Message_Template_VideoContent");

            migrationBuilder.DropColumn(
                name: "BlobId",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropColumn(
                name: "ContentLength",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropColumn(
                name: "Suffix",
                table: "Chat_Message_Template_SoundContent");

            migrationBuilder.DropColumn(
                name: "BlobId",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropColumn(
                name: "ContentLength",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropColumn(
                name: "Suffix",
                table: "Chat_Message_Template_ImageContent");

            migrationBuilder.DropColumn(
                name: "BlobId",
                table: "Chat_Message_Template_FileContent");
        }
    }
}
