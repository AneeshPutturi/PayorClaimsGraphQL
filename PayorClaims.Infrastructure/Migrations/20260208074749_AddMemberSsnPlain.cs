using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayorClaims.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberSsnPlain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SsnPlain",
                table: "members",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SsnPlain",
                table: "members");
        }
    }
}
