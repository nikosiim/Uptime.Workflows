using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Uptime.Workflows.Core.Data.Migrations
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
                name: "WorkflowPrincipals",
                schema: "UptimeAPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowPrincipals", x => x.Id);
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
                    SiteUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
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
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StorageJson = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    InitiatedByPrincipalId = table.Column<int>(type: "int", nullable: false),
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
                        name: "FK_Workflows_WorkflowPrincipals_InitiatedByPrincipalId",
                        column: x => x.InitiatedByPrincipalId,
                        principalSchema: "UptimeAPI",
                        principalTable: "WorkflowPrincipals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    Occurred = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    PerformedByPrincipalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowHistories_WorkflowPrincipals_PerformedByPrincipalId",
                        column: x => x.PerformedByPrincipalId,
                        principalSchema: "UptimeAPI",
                        principalTable: "WorkflowPrincipals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    TaskGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    InternalStatus = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    StorageJson = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    PhaseId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    AssignedToPrincipalId = table.Column<int>(type: "int", nullable: false),
                    AssignedByPrincipalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowPrincipals_AssignedByPrincipalId",
                        column: x => x.AssignedByPrincipalId,
                        principalSchema: "UptimeAPI",
                        principalTable: "WorkflowPrincipals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowPrincipals_AssignedToPrincipalId",
                        column: x => x.AssignedToPrincipalId,
                        principalSchema: "UptimeAPI",
                        principalTable: "WorkflowPrincipals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "UptimeAPI",
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutboundNotifications",
                schema: "UptimeAPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    WorkflowTaskId = table.Column<int>(type: "int", nullable: true),
                    TaskGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhaseId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EndpointPath = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SourceSiteUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseBody = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UniqueKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboundNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboundNotifications_WorkflowTasks_WorkflowTaskId",
                        column: x => x.WorkflowTaskId,
                        principalSchema: "UptimeAPI",
                        principalTable: "WorkflowTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OutboundNotifications_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "UptimeAPI",
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                table: "WorkflowPrincipals",
                columns: new[] { "Id", "Email", "ExternalId", "Name", "Source", "Type" },
                values: new object[,]
                {
                    { 1, "klient1@example.com", "S-1-5-21-10001", "Klient Üks", "Windows", 0 },
                    { 2, "klient2@example.com", "S-1-5-21-10002", "Klient Kaks", "Windows", 0 },
                    { 3, "klient3@example.com", "S-1-5-21-10003", "Klient Kolm", "Windows", 0 },
                    { 4, "klient4@example.com", "S-1-5-21-10004", "Klient Neli", "Windows", 0 },
                    { 5, "klient5@example.com", "S-1-5-21-10005", "Klient Viis", "Windows", 0 },
                    { 6, "marika.oja@example.com", "S-1-5-21-10006", "Marika Oja", "Windows", 0 },
                    { 7, "jana.parn@example.com", "S-1-5-21-10007", "Jana Pärn", "Windows", 0 },
                    { 8, "piia.saar@example.com", "S-1-5-21-10008", "Piia Saar", "Windows", 0 },
                    { 9, "urve.oja@example.com", "S-1-5-21-10009", "Urve Oja", "Windows", 0 },
                    { 10, "peeter.sepp@example.com", "S-1-5-21-10010", "Peeter Sepp", "Windows", 0 },
                    { 11, "markus.lepik@example.com", "S-1-5-21-10011", "Markus Lepik", "Windows", 0 },
                    { 12, "marta.laine@example.com", "S-1-5-21-10012", "Marta Laine", "Windows", 0 },
                    { 13, "anton.rebane@example.com", "S-1-5-21-10013", "Anton Rebane", "Windows", 0 },
                    { 14, "signe.kask@example.com", "S-1-5-21-10014", "Signe Kask", "Windows", 0 },
                    { 15, "riin.koppel@example.com", "S-1-5-21-10015", "Riin Koppel", "Windows", 0 },
                    { 16, "lauri.saar@example.com", "S-1-5-21-10016", "Lauri Saar", "Windows", 0 },
                    { 17, "viljar.laine@example.com", "S-1-5-21-10017", "Viljar Laine", "Windows", 0 },
                    { 18, "kristina.kroon@example.com", "S-1-5-21-10018", "Kristina Kroon", "Windows", 0 },
                    { 19, "system@example.srv", "S-1-5-21-10000", "System", "Windows", 0 }
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
                name: "IX_OutboundNotifications_CreatedAtUtc",
                schema: "UptimeAPI",
                table: "OutboundNotifications",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboundNotifications_PhaseId",
                schema: "UptimeAPI",
                table: "OutboundNotifications",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboundNotifications_TaskGuid",
                schema: "UptimeAPI",
                table: "OutboundNotifications",
                column: "TaskGuid");

            migrationBuilder.CreateIndex(
                name: "IX_OutboundNotifications_UniqueKey",
                schema: "UptimeAPI",
                table: "OutboundNotifications",
                column: "UniqueKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboundNotifications_WorkflowId_EventType_Status",
                schema: "UptimeAPI",
                table: "OutboundNotifications",
                columns: new[] { "WorkflowId", "EventType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboundNotifications_WorkflowTaskId",
                schema: "UptimeAPI",
                table: "OutboundNotifications",
                column: "WorkflowTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_PerformedByPrincipalId",
                schema: "UptimeAPI",
                table: "WorkflowHistories",
                column: "PerformedByPrincipalId");

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
                name: "IX_Workflows_InitiatedByPrincipalId",
                schema: "UptimeAPI",
                table: "Workflows",
                column: "InitiatedByPrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_WorkflowTemplateId",
                schema: "UptimeAPI",
                table: "Workflows",
                column: "WorkflowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_AssignedByPrincipalId",
                schema: "UptimeAPI",
                table: "WorkflowTasks",
                column: "AssignedByPrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_AssignedToPrincipalId_InternalStatus",
                schema: "UptimeAPI",
                table: "WorkflowTasks",
                columns: new[] { "AssignedToPrincipalId", "InternalStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_PhaseId",
                schema: "UptimeAPI",
                table: "WorkflowTasks",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_TaskGuid",
                schema: "UptimeAPI",
                table: "WorkflowTasks",
                column: "TaskGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowId_InternalStatus",
                schema: "UptimeAPI",
                table: "WorkflowTasks",
                columns: new[] { "WorkflowId", "InternalStatus" });

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
                name: "OutboundNotifications",
                schema: "UptimeAPI");

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
                name: "WorkflowPrincipals",
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
