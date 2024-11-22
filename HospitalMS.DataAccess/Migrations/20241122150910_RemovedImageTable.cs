using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemovedImageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Images_ImageId",
                table: "Doctors");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_ImageId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Doctors");

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

            migrationBuilder.AddColumn<string>(
                name: "ImageFilePath",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileExtension",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ImageFilePath",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ImageFileSizeInBytes",
                table: "Doctors");

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "Doctors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: true),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSizeInBytes = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_ImageId",
                table: "Doctors",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_DoctorId",
                table: "Images",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Images_ImageId",
                table: "Doctors",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id");
        }
    }
}
