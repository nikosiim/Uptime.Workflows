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
                .HasAnnotation("ProductVersion", "9.0.1")
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
                            CreatedBy = "Emma Carter",
                            Description = "First document",
                            LibraryId = 1,
                            Title = "QuickGuide"
                        },
                        new
                        {
                            Id = 2,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Sophia Patel",
                            Description = "Second document",
                            LibraryId = 1,
                            Title = "PlanDraft"
                        },
                        new
                        {
                            Id = 3,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Liam Rodriguez",
                            Description = "Third document",
                            LibraryId = 2,
                            Title = "Notes2025"
                        },
                        new
                        {
                            Id = 4,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Isabella Nguyen",
                            Description = "Fourth document",
                            LibraryId = 1,
                            Title = "TaskList"
                        },
                        new
                        {
                            Id = 5,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Emma Carter",
                            Description = "Fifth document",
                            LibraryId = 2,
                            Title = "IdeaLog"
                        },
                        new
                        {
                            Id = 6,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Liam Rodriguez",
                            Description = "Sixth document",
                            LibraryId = 1,
                            Title = "MiniReport"
                        },
                        new
                        {
                            Id = 7,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "User7",
                            Description = "Seventh document",
                            LibraryId = 2,
                            Title = "FastSummary"
                        },
                        new
                        {
                            Id = 8,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Isabella Nguyen",
                            Description = "Eighth document",
                            LibraryId = 1,
                            Title = "BriefMemo"
                        },
                        new
                        {
                            Id = 9,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Noah Kim",
                            Description = "Ninth document",
                            LibraryId = 2,
                            Title = "Snapshot"
                        },
                        new
                        {
                            Id = 10,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            CreatedBy = "Sophia Patel",
                            Description = "Tenth document",
                            LibraryId = 2,
                            Title = "OutlineDoc"
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
                            Name = "Contracts"
                        },
                        new
                        {
                            Id = 2,
                            Created = new DateTime(2015, 5, 15, 13, 45, 0, 0, DateTimeKind.Unspecified),
                            Name = "Letters"
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

                    b.Property<string>("InstanceDataJson")
                        .HasMaxLength(4096)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Originator")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

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

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Actor")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Comments")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

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

                    b.Property<DateTime?>("DueDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("StorageJson")
                        .HasMaxLength(4096)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TaskDescription")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

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
