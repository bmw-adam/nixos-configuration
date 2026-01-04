using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TpvVyber.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tpv_schema");

            migrationBuilder.CreateTable(
                name: "Courses",
                schema: "tpv_schema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PdfUrl = table.Column<string>(type: "text", nullable: false),
                    ForClasses = table.Column<string>(type: "text", nullable: false),
                    MinPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Capacity = table.Column<long>(type: "bigint", nullable: false),
                    MinCapacity = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoggingEndings",
                schema: "tpv_schema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeEnding = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoggingEndings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                schema: "tpv_schema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.UniqueConstraint("AK_Students_Email", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "HistoryStudentCourses",
                schema: "tpv_schema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryStudentCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryStudentCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "tpv_schema",
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoryStudentCourses_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "tpv_schema",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderCourses",
                schema: "tpv_schema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "tpv_schema",
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderCourses_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "tpv_schema",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryStudentCourses_CourseId",
                schema: "tpv_schema",
                table: "HistoryStudentCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryStudentCourses_StudentId",
                schema: "tpv_schema",
                table: "HistoryStudentCourses",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCourses_CourseId",
                schema: "tpv_schema",
                table: "OrderCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCourses_StudentId",
                schema: "tpv_schema",
                table: "OrderCourses",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryStudentCourses",
                schema: "tpv_schema");

            migrationBuilder.DropTable(
                name: "LoggingEndings",
                schema: "tpv_schema");

            migrationBuilder.DropTable(
                name: "OrderCourses",
                schema: "tpv_schema");

            migrationBuilder.DropTable(
                name: "Courses",
                schema: "tpv_schema");

            migrationBuilder.DropTable(
                name: "Students",
                schema: "tpv_schema");
        }
    }
}
