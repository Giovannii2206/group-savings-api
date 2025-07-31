using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupSavingsApi.Migrations
{
    /// <inheritdoc />
    public partial class SavingsGoalGroupSessionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "SavingsGoals",
                newName: "GroupSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsGoals_GroupSessionId",
                table: "SavingsGoals",
                column: "GroupSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SavingsGoals_GroupSessions_GroupSessionId",
                table: "SavingsGoals",
                column: "GroupSessionId",
                principalTable: "GroupSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavingsGoals_GroupSessions_GroupSessionId",
                table: "SavingsGoals");

            migrationBuilder.DropIndex(
                name: "IX_SavingsGoals_GroupSessionId",
                table: "SavingsGoals");

            migrationBuilder.RenameColumn(
                name: "GroupSessionId",
                table: "SavingsGoals",
                newName: "GroupId");
        }
    }
}
