using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLL_ConstruccionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposFacturaEstadoPagoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Notas",
                table: "PagosCliente",
                newName: "Observaciones");

            migrationBuilder.AddColumn<string>(
                name: "ActividadStatus",
                table: "PagosCliente",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "PagosCliente",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Pendiente");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCotizacion",
                table: "PagosCliente",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Iva",
                table: "PagosCliente",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NumeroFactura",
                table: "PagosCliente",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "PagosCliente",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "PagosCliente",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActividadStatus",
                table: "PagosCliente");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "PagosCliente");

            migrationBuilder.DropColumn(
                name: "FechaCotizacion",
                table: "PagosCliente");

            migrationBuilder.DropColumn(
                name: "Iva",
                table: "PagosCliente");

            migrationBuilder.DropColumn(
                name: "NumeroFactura",
                table: "PagosCliente");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "PagosCliente");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "PagosCliente");

            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "PagosCliente",
                newName: "Notas");
        }
    }
}
