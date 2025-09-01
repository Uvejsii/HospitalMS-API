using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsApprovedAtDoctorVacation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "DoctorVacations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "DoctorVacations");
        }
    }
}
