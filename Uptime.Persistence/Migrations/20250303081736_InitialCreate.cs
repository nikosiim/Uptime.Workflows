using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Uptime.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "UptimeAPI");

            migrationBuilder.CreateTable(
                name: "Libraries",
                schema: "UptimeAPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                schema: "UptimeAPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LibraryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalSchema: "UptimeAPI",
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTemplates",
                schema: "UptimeAPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    WorkflowName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    WorkflowBaseId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AssociationDataJson = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LibraryId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTemplates_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalSchema: "UptimeAPI",
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                schema: "UptimeAPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Phase = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Originator = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StorageJson = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    WorkflowTemplateId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workflows_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "UptimeAPI",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Workflows_WorkflowTemplates_WorkflowTemplateId",
                        column: x => x.WorkflowTemplateId,
                        principalSchema: "UptimeAPI",
                        principalTable: "WorkflowTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowHistories",
                schema: "UptimeAPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Event = table.Column<int>(type: "int", nullable: false),
                    User = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Occurred = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    WorkflowId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowHistories_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "UptimeAPI",
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                schema: "UptimeAPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InternalStatus = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StorageJson = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    PhaseId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    WorkflowId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "UptimeAPI",
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "UptimeAPI",
                table: "Libraries",
                columns: new[] { "Id", "Created", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), false, "Lepingud" },
                    { 2, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), false, "Kirjavahetus" }
                });

            migrationBuilder.InsertData(
                schema: "UptimeAPI",
                table: "Documents",
                columns: new[] { "Id", "Created", "CreatedBy", "Description", "IsDeleted", "LibraryId", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Lauri Saar", "Sofia Kuperštein", false, 1, "Teabenõue" },
                    { 2, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Riin Koppel", "Vello Lauri", false, 1, "LISA_13.01.2025_7-4.2_277-3" },
                    { 3, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Jana Pärn", "SK_25.02.2025_9-11_25_59-4", false, 2, "Pöördumine" },
                    { 4, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Markus Lepik", "AS GoTravel", false, 1, "LEPING_AS GoTravel_18.12.2024_7-4.2_281" },
                    { 5, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Emma Carter", "Fifth document", false, 2, "IdeaLog" },
                    { 6, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Marta Laine", "Rethinkers OÜ", false, 1, "LEPING_14.02.2025_7-4.2_293" },
                    { 7, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Klient Kaks", "Rethinkers OÜ", false, 2, "FastSummary" },
                    { 8, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Viljar Laine", "PZU Kindlustus", false, 1, "2024 inventuuri lõppakt" },
                    { 9, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Signe Kask", "Riigi IKT Keskus", false, 2, "Intervjuu tervisekassaga" },
                    { 10, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Anton Rebane", "Kaitseministeerium", false, 2, "Juurdepääsupiirangu muutumine" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_LibraryId",
                schema: "UptimeAPI",
                table: "Documents",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_WorkflowId",
                schema: "UptimeAPI",
                table: "WorkflowHistories",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_DocumentId",
                schema: "UptimeAPI",
                table: "Workflows",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_WorkflowTemplateId",
                schema: "UptimeAPI",
                table: "Workflows",
                column: "WorkflowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowId",
                schema: "UptimeAPI",
                table: "WorkflowTasks",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplates_LibraryId",
                schema: "UptimeAPI",
                table: "WorkflowTemplates",
                column: "LibraryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowHistories",
                schema: "UptimeAPI");

            migrationBuilder.DropTable(
                name: "WorkflowTasks",
                schema: "UptimeAPI");

            migrationBuilder.DropTable(
                name: "Workflows",
                schema: "UptimeAPI");

            migrationBuilder.DropTable(
                name: "Documents",
                schema: "UptimeAPI");

            migrationBuilder.DropTable(
                name: "WorkflowTemplates",
                schema: "UptimeAPI");

            migrationBuilder.DropTable(
                name: "Libraries",
                schema: "UptimeAPI");
        }
    }
}
