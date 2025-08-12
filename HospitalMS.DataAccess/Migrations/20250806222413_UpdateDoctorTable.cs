using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDoctorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileExtension",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ImageFileSizeInBytes",
                table: "Doctors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageFileExtension",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "ImageFileSizeInBytes",
                table: "Doctors",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
