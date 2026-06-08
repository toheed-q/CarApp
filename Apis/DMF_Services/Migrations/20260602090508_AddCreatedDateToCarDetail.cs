using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMF_Services.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedDateToCarDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "CarDetail",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "CarDetail");
        }
    }
}
