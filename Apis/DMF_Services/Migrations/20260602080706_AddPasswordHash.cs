using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMF_Services.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // All other columns already exist in the database (created via SQL scripts).
            // Only add the new PasswordHash column.
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "UserDetail",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "UserDetail");
        }
    }
}
