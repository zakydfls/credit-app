using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditApplication.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pengajuan_kredit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plafon = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    bunga = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    tenor = table.Column<int>(type: "integer", nullable: false),
                    angsuran = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pengajuan_kredit", x => x.id);
                    table.CheckConstraint("ck_pengajuan_kredit_angsuran_positive", "angsuran > 0");
                    table.CheckConstraint("ck_pengajuan_kredit_bunga_range", "bunga > 0 AND bunga <= 100");
                    table.CheckConstraint("ck_pengajuan_kredit_plafon_positive", "plafon > 0");
                    table.CheckConstraint("ck_pengajuan_kredit_tenor_positive", "tenor > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_pengajuan_kredit_plafon",
                table: "pengajuan_kredit",
                column: "plafon");

            migrationBuilder.CreateIndex(
                name: "ix_pengajuan_kredit_tenor",
                table: "pengajuan_kredit",
                column: "tenor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pengajuan_kredit");
        }
    }
}
