﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IczpNet.Chat.Migrations
{
    public partial class Message_FixProp_SessionId_Unrequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Chat_Session_SessionId",
                table: "Chat_Message");

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionId",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Chat_Session_SessionId",
                table: "Chat_Message",
                column: "SessionId",
                principalTable: "Chat_Session",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_Chat_Session_SessionId",
                table: "Chat_Message");

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionId",
                table: "Chat_Message",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_Chat_Session_SessionId",
                table: "Chat_Message",
                column: "SessionId",
                principalTable: "Chat_Session",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
