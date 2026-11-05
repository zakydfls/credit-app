using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditApplication.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "pengajuan_kredit",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_pengajuan_kredit_user_id",
                table: "pengajuan_kredit",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_pengajuan_kredit_users_user_id",
                table: "pengajuan_kredit",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pengajuan_kredit_users_user_id",
                table: "pengajuan_kredit");

            migrationBuilder.DropIndex(
                name: "ix_pengajuan_kredit_user_id",
                table: "pengajuan_kredit");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "pengajuan_kredit");
        }
    }
}
