using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynapseSRE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResolvedAtToIncident : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "incidents",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "incidents");
        }
    }
}
