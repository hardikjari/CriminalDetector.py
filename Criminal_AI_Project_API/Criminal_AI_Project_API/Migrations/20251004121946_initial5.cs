using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Criminal_AI_Project_API.Migrations
{
    /// <inheritdoc />
    public partial class initial5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Details",
                table: "tbl_CriminalEvents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "tbl_CriminalEvents",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
