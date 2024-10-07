using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visualizador_de_Facturas_XML.Migrations
{
    /// <inheritdoc />
    public partial class Inicializacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Perfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    UUID = table.Column<string>(type: "TEXT", nullable: false),
                    XmlContent = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PerfilID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.UUID);
                    table.ForeignKey(
                        name: "FK_Facturas_Perfiles_PerfilID",
                        column: x => x.PerfilID,
                        principalTable: "Perfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_PerfilID",
                table: "Facturas",
                column: "PerfilID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropTable(
                name: "Perfiles");
        }
    }
}
