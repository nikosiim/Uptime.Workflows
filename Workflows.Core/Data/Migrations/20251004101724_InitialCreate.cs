using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Workflows.Core.Data.Migrations
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
                name: "OutboundNotifications",
                schema: "UptimeAPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: false),
                    ResponseBody = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboundNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowPrincipals",
                schema: "UptimeAPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LoginName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SyncedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeactivatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowPrincipals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTemplates",
                schema: "UptimeAPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LibraryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    WorkflowName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    WorkflowBaseId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SiteUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    AssociationDataJson = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByPrincipalId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedByPrincipalId = table.Column<int>(type: "int", nullable: true),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByPrincipalId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                schema: "UptimeAPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Phase = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    StorageJson = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    InitiatedById = table.Column<int>(type: "int", nullable: false),
                    WorkflowTemplateId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByPrincipalId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedByPrincipalId = table.Column<int>(type: "int", nullable: true),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByPrincipalId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workflows_WorkflowPrincipals_InitiatedById",
                        column: x => x.InitiatedById,
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
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    PerformedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowHistories_WorkflowPrincipals_PerformedById",
                        column: x => x.PerformedById,
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
                    DueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    StorageJson = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    PhaseId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    AssignedToId = table.Column<int>(type: "int", nullable: false),
                    AssignedById = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByPrincipalId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedByPrincipalId = table.Column<int>(type: "int", nullable: true),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByPrincipalId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowPrincipals_AssignedById",
                        column: x => x.AssignedById,
                        principalSchema: "UptimeAPI",
                        principalTable: "WorkflowPrincipals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowPrincipals_AssignedToId",
                        column: x => x.AssignedToId,
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

            migrationBuilder.InsertData(
                schema: "UptimeAPI",
                table: "WorkflowPrincipals",
                columns: new[] { "Id", "CreatedAtUtc", "DeactivatedAtUtc", "Email", "ExternalId", "LoginName", "Name", "Source", "SyncedAtUtc", "Type" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "klient1@example.com", "S-1-5-21-10001", null, "Klient Üks", "Windows", null, 0 },
                    { 2, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "klient2@example.com", "S-1-5-21-10002", null, "Klient Kaks", "Windows", null, 0 },
                    { 3, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "klient3@example.com", "S-1-5-21-10003", null, "Klient Kolm", "Windows", null, 0 },
                    { 4, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "klient4@example.com", "S-1-5-21-10004", null, "Klient Neli", "Windows", null, 0 },
                    { 5, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "klient5@example.com", "S-1-5-21-10005", null, "Klient Viis", "Windows", null, 0 },
                    { 6, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "marika.oja@example.com", "S-1-5-21-10006", null, "Marika Oja", "Windows", null, 0 },
                    { 7, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "jana.parn@example.com", "S-1-5-21-10007", null, "Jana Pärn", "Windows", null, 0 },
                    { 8, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "piia.saar@example.com", "S-1-5-21-10008", null, "Piia Saar", "Windows", null, 0 },
                    { 9, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "urve.oja@example.com", "S-1-5-21-10009", null, "Urve Oja", "Windows", null, 0 },
                    { 10, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "peeter.sepp@example.com", "S-1-5-21-10010", null, "Peeter Sepp", "Windows", null, 0 },
                    { 11, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "markus.lepik@example.com", "S-1-5-21-10011", null, "Markus Lepik", "Windows", null, 0 },
                    { 12, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "marta.laine@example.com", "S-1-5-21-10012", null, "Marta Laine", "Windows", null, 0 },
                    { 13, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "anton.rebane@example.com", "S-1-5-21-10013", null, "Anton Rebane", "Windows", null, 0 },
                    { 14, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "signe.kask@example.com", "S-1-5-21-10014", null, "Signe Kask", "Windows", null, 0 },
                    { 15, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "riin.koppel@example.com", "S-1-5-21-10015", null, "Riin Koppel", "Windows", null, 0 },
                    { 16, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "lauri.saar@example.com", "S-1-5-21-10016", null, "Lauri Saar", "Windows", null, 0 },
                    { 17, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "viljar.laine@example.com", "S-1-5-21-10017", null, "Viljar Laine", "Windows", null, 0 },
                    { 18, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "kristina.kroon@example.com", "S-1-5-21-10018", null, "Kristina Kroon", "Windows", null, 0 },
                    { 19, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "system@example.srv", "S-1-5-21-10000", null, "System", "Windows", null, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboundNotifications_CreatedAtUtc",
                schema: "UptimeAPI",
                table: "OutboundNotifications",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboundNotifications_EventName_Status",
                schema: "UptimeAPI",
                table: "OutboundNotifications",
                columns: new[] { "EventName", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboundNotifications_UniqueKey",
                schema: "UptimeAPI",
                table: "OutboundNotifications",
                column: "UniqueKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_PerformedById",
                schema: "UptimeAPI",
                table: "WorkflowHistories",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHistories_WorkflowId",
                schema: "UptimeAPI",
                table: "WorkflowHistories",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_InitiatedById",
                schema: "UptimeAPI",
                table: "Workflows",
                column: "InitiatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_WorkflowTemplateId",
                schema: "UptimeAPI",
                table: "Workflows",
                column: "WorkflowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_AssignedById",
                schema: "UptimeAPI",
                table: "WorkflowTasks",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_AssignedToId_InternalStatus",
                schema: "UptimeAPI",
                table: "WorkflowTasks",
                columns: new[] { "AssignedToId", "InternalStatus" });

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
                name: "WorkflowPrincipals",
                schema: "UptimeAPI");

            migrationBuilder.DropTable(
                name: "WorkflowTemplates",
                schema: "UptimeAPI");
        }
    }
}
