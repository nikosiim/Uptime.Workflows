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
            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LibraryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTemplates",
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
                    LibraryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTemplates_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
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
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Workflows_WorkflowTemplates_WorkflowTemplateId",
                        column: x => x.WorkflowTemplateId,
                        principalTable: "WorkflowTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Actor = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    WorkflowId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowHistories_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StorageJson = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    WorkflowId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "Created", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Contracts" },
                    { 2, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Letters" }
                });

            migrationBuilder.InsertData(
                table: "Documents",
                columns: new[] { "Id", "Created", "CreatedBy", "Description", "LibraryId", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Emma Carter", "First document", 1, "QuickGuide" },
                    { 2, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Sophia Patel", "Second document", 1, "PlanDraft" },
                    { 3, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Liam Rodriguez", "Third document", 2, "Notes2025" },
                    { 4, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Isabella Nguyen", "Fourth document", 1, "TaskList" },
                    { 5, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Emma Carter", "Fifth document", 2, "IdeaLog" },
                    { 6, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Liam Rodriguez", "Sixth document", 1, "MiniReport" },
                    { 7, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "User7", "Seventh document", 2, "FastSummary" },
                    { 8, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Isabella Nguyen", "Eighth document", 1, "BriefMemo" },
                    { 9, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Noah Kim", "Ninth document", 2, "Snapshot" },
                    { 10, new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified), "Sophia Patel", "Tenth document", 2, "OutlineDoc" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_LibraryId",
                table: "Documents",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_WorkflowId",
                table: "WorkflowHistories",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_DocumentId",
                table: "Workflows",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_WorkflowTemplateId",
                table: "Workflows",
                column: "WorkflowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowId",
                table: "WorkflowTasks",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplates_LibraryId",
                table: "WorkflowTemplates",
                column: "LibraryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowHistories");

            migrationBuilder.DropTable(
                name: "WorkflowTasks");

            migrationBuilder.DropTable(
                name: "Workflows");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "WorkflowTemplates");

            migrationBuilder.DropTable(
                name: "Libraries");
        }
    }
}
