﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Uptime.Persistence;

#nullable disable

namespace Uptime.Persistence.Migrations
{
    [DbContext(typeof(WorkflowDbContext))]
    partial class WorkflowDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Uptime.Domain.Entities.Document", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Description")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("LibraryId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("LibraryId");

                    b.ToTable("Documents");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Lauri Saar",
                            Description = "Sofia Kuperštein",
                            IsDeleted = false,
                            LibraryId = 1,
                            Title = "Teabenõue"
                        },
                        new
                        {
                            Id = 2,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Riin Koppel",
                            Description = "Vello Lauri",
                            IsDeleted = false,
                            LibraryId = 1,
                            Title = "LISA_13.01.2025_7-4.2_277-3"
                        },
                        new
                        {
                            Id = 3,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Jana Pärn",
                            Description = "SK_25.02.2025_9-11_25_59-4",
                            IsDeleted = false,
                            LibraryId = 2,
                            Title = "Pöördumine"
                        },
                        new
                        {
                            Id = 4,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Markus Lepik",
                            Description = "AS GoTravel",
                            IsDeleted = false,
                            LibraryId = 1,
                            Title = "LEPING_AS GoTravel_18.12.2024_7-4.2_281"
                        },
                        new
                        {
                            Id = 5,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Emma Carter",
                            Description = "Fifth document",
                            IsDeleted = false,
                            LibraryId = 2,
                            Title = "IdeaLog"
                        },
                        new
                        {
                            Id = 6,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Marta Laine",
                            Description = "Rethinkers OÜ",
                            IsDeleted = false,
                            LibraryId = 1,
                            Title = "LEPING_14.02.2025_7-4.2_293"
                        },
                        new
                        {
                            Id = 7,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Klient Kaks",
                            Description = "Rethinkers OÜ",
                            IsDeleted = false,
                            LibraryId = 2,
                            Title = "FastSummary"
                        },
                        new
                        {
                            Id = 8,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Viljar Laine",
                            Description = "PZU Kindlustus",
                            IsDeleted = false,
                            LibraryId = 1,
                            Title = "2024 inventuuri lõppakt"
                        },
                        new
                        {
                            Id = 9,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Signe Kask",
                            Description = "Riigi IKT Keskus",
                            IsDeleted = false,
                            LibraryId = 2,
                            Title = "Intervjuu tervisekassaga"
                        },
                        new
                        {
                            Id = 10,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Anton Rebane",
                            Description = "Kaitseministeerium",
                            IsDeleted = false,
                            LibraryId = 2,
                            Title = "Juurdepääsupiirangu muutumine"
                        });
                });

            modelBuilder.Entity("Uptime.Domain.Entities.Library", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.ToTable("Libraries");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            IsDeleted = false,
                            Name = "Lepingud"
                        },
                        new
                        {
                            Id = 2,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            IsDeleted = false,
                            Name = "Kirjavahetus"
                        });
                });

            modelBuilder.Entity("Uptime.Domain.Entities.Workflow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("DocumentId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Originator")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Outcome")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Phase")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("StorageJson")
                        .HasMaxLength(4096)
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("WorkflowTemplateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DocumentId");

                    b.HasIndex("WorkflowTemplateId");

                    b.ToTable("Workflows");
                });

            modelBuilder.Entity("Uptime.Domain.Entities.WorkflowHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Comment")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<string>("Description")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<int>("Event")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Occurred")
                        .HasColumnType("datetime2");

                    b.Property<string>("User")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("WorkflowId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("WorkflowId");

                    b.ToTable("WorkflowHistories");
                });

            modelBuilder.Entity("Uptime.Domain.Entities.WorkflowTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AssignedBy")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("AssignedTo")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Description")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<DateTime?>("DueDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("InternalStatus")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("PhaseId")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StorageJson")
                        .HasMaxLength(4096)
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("TaskGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("WorkflowId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("WorkflowId");

                    b.ToTable("WorkflowTasks");
                });

            modelBuilder.Entity("Uptime.Domain.Entities.WorkflowTemplate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AssociationDataJson")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("LibraryId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime2");

                    b.Property<string>("TemplateName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("WorkflowBaseId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("WorkflowName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("LibraryId");

                    b.ToTable("WorkflowTemplates");
                });

            modelBuilder.Entity("Uptime.Domain.Entities.Document", b =>
                {
                    b.HasOne("Uptime.Domain.Entities.Library", "Library")
                        .WithMany("Documents")
                        .HasForeignKey("LibraryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Library");
                });

            modelBuilder.Entity("Uptime.Domain.Entities.Workflow", b =>
                {
                    b.HasOne("Uptime.Domain.Entities.Document", "Document")
                        .WithMany("Workflows")
                        .HasForeignKey("DocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Uptime.Domain.Entities.WorkflowTemplate", "WorkflowTemplate")
                        .WithMany("Workflows")
                        .HasForeignKey("WorkflowTemplateId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Document");

                    b.Navigation("WorkflowTemplate");
                });

            modelBuilder.Entity("Uptime.Domain.Entities.WorkflowHistory", b =>
                {
                    b.HasOne("Uptime.Domain.Entities.Workflow", "Workflow")
                        .WithMany("WorkflowHistories")
                        .HasForeignKey("WorkflowId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Workflow");
                });

            modelBuilder.Entity("Uptime.Domain.Entities.WorkflowTask", b =>
                {
                    b.HasOne("Uptime.Domain.Entities.Workflow", "Workflow")
                        .WithMany("WorkflowTasks")
                        .HasForeignKey("WorkflowId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Workflow");
                });

            modelBuilder.Entity("Uptime.Domain.Entities.WorkflowTemplate", b =>
                {
                    b.HasOne("Uptime.Domain.Entities.Library", "Library")
                        .WithMany("WorkflowTemplates")
                        .HasForeignKey("LibraryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Library");
                });

            modelBuilder.Entity("Uptime.Domain.Entities.Document", b =>
                {
                    b.Navigation("Workflows");
                });

            modelBuilder.Entity("Uptime.Domain.Entities.Library", b =>
                {
                    b.Navigation("Documents");

                    b.Navigation("WorkflowTemplates");
                });

            modelBuilder.Entity("Uptime.Domain.Entities.Workflow", b =>
                {
                    b.Navigation("WorkflowHistories");

                    b.Navigation("WorkflowTasks");
                });

            modelBuilder.Entity("Uptime.Domain.Entities.WorkflowTemplate", b =>
                {
                    b.Navigation("Workflows");
                });
#pragma warning restore 612, 618
        }
    }
}
