using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace delivery_rental_system.Migrations
{
    /// <inheritdoc />
    public partial class first_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "delivery_drivers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CnhNumero = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    CnhEnum = table.Column<int>(type: "integer", nullable: false),
                    CnhImagemUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_drivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "motorcycle_year2024_notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Plate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    MotorcycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_motorcycle_year2024_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "motorcycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Plate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_motorcycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rentals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DeliveryManId = table.Column<int>(type: "integer", nullable: false),
                    MotorcycleId = table.Column<int>(type: "integer", nullable: false),
                    Plan = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PredictedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RentalValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rentals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rentals_delivery_drivers_DeliveryManId",
                        column: x => x.DeliveryManId,
                        principalTable: "delivery_drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rentals_motorcycles_MotorcycleId",
                        column: x => x.MotorcycleId,
                        principalTable: "motorcycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_drivers_CnhNumero",
                table: "delivery_drivers",
                column: "CnhNumero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_delivery_drivers_Cnpj",
                table: "delivery_drivers",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_motorcycles_Plate",
                table: "motorcycles",
                column: "Plate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rentals_DeliveryManId_StartDate",
                table: "rentals",
                columns: new[] { "DeliveryManId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_rentals_MotorcycleId_StartDate_PredictedEndDate",
                table: "rentals",
                columns: new[] { "MotorcycleId", "StartDate", "PredictedEndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "motorcycle_year2024_notifications");

            migrationBuilder.DropTable(
                name: "rentals");

            migrationBuilder.DropTable(
                name: "delivery_drivers");

            migrationBuilder.DropTable(
                name: "motorcycles");
        }
    }
}
