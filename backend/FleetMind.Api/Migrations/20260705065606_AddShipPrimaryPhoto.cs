using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetMind.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddShipPrimaryPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryPhotoAttachmentId",
                table: "Ships",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ships_PrimaryPhotoAttachmentId",
                table: "Ships",
                column: "PrimaryPhotoAttachmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ships_Attachments_PrimaryPhotoAttachmentId",
                table: "Ships",
                column: "PrimaryPhotoAttachmentId",
                principalTable: "Attachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ships_Attachments_PrimaryPhotoAttachmentId",
                table: "Ships");

            migrationBuilder.DropIndex(
                name: "IX_Ships_PrimaryPhotoAttachmentId",
                table: "Ships");

            migrationBuilder.DropColumn(
                name: "PrimaryPhotoAttachmentId",
                table: "Ships");
        }
    }
}
