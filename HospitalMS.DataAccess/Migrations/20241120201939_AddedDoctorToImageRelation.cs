using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedDoctorToImageRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageIrl",
                table: "Doctors");

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "Doctors",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: true)
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
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "ImageIrl",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
