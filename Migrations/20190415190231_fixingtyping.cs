using Microsoft.EntityFrameworkCore.Migrations;

namespace timebot.Migrations
{
    public partial class fixingtyping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "serverid",
                table: "BotCommands",
                nullable: false,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "serverid",
                table: "BotCommands",
                nullable: false,
                oldClrType: typeof(decimal));
        }
    }
}
