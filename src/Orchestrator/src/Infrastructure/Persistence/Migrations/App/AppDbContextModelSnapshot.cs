﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("truquest")
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Aggregates.AcceptancePollVote", b =>
                {
                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<string>("VoterId")
                        .HasColumnType("text");

                    b.Property<long>("CastedAtMs")
                        .HasColumnType("bigint");

                    b.Property<int>("Decision")
                        .HasColumnType("integer");

                    b.Property<string>("IpfsCid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.Property<string>("VoterSignature")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ThingId", "VoterId");

                    b.ToTable("AcceptancePollVotes", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.AssessmentPollVote", b =>
                {
                    b.Property<Guid>("SettlementProposalId")
                        .HasColumnType("uuid");

                    b.Property<string>("VoterId")
                        .HasColumnType("text");

                    b.Property<long>("CastedAtMs")
                        .HasColumnType("bigint");

                    b.Property<int>("Decision")
                        .HasColumnType("integer");

                    b.Property<string>("IpfsCid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.Property<string>("VoterSignature")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("SettlementProposalId", "VoterId");

                    b.ToTable("AssessmentPollVotes", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.DeferredTask", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<IReadOnlyDictionary<string, object>>("Payload")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<long>("ScheduledBlockNumber")
                        .HasColumnType("bigint");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Tasks", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.Evidence", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("IpfsCid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OriginUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PreviewImageIpfsCid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ThingId");

                    b.ToTable("Evidence", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.SettlementProposal", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long?>("AssessmentPronouncedAt")
                        .HasColumnType("bigint");

                    b.Property<string>("CroppedImageIpfsCid")
                        .HasColumnType("text");

                    b.Property<string>("Details")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ImageIpfsCid")
                        .HasColumnType("text");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<long?>("SubmittedAt")
                        .HasColumnType("bigint");

                    b.Property<string>("SubmitterId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Verdict")
                        .HasColumnType("integer");

                    b.Property<string>("VoteAggIpfsCid")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SubmitterId");

                    b.HasIndex("ThingId");

                    b.ToTable("SettlementProposals", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.SettlementProposalUpdate", b =>
                {
                    b.Property<Guid>("SettlementProposalId")
                        .HasColumnType("uuid");

                    b.Property<int>("Category")
                        .HasColumnType("integer");

                    b.Property<string>("Details")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("UpdateTimestamp")
                        .HasColumnType("bigint");

                    b.HasKey("SettlementProposalId", "Category");

                    b.ToTable("SettlementProposalUpdates", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.SettlementProposalVerifier", b =>
                {
                    b.Property<Guid?>("SettlementProposalId")
                        .HasColumnType("uuid");

                    b.Property<string>("VerifierId")
                        .HasColumnType("text");

                    b.HasKey("SettlementProposalId", "VerifierId");

                    b.HasIndex("VerifierId");

                    b.ToTable("SettlementProposalVerifiers", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.Subject", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<float>("AvgScore")
                        .HasColumnType("real");

                    b.Property<string>("CroppedImageIpfsCid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Details")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ImageIpfsCid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("SettledThingsCount")
                        .HasColumnType("integer");

                    b.Property<long>("SubmittedAt")
                        .HasColumnType("bigint");

                    b.Property<string>("SubmitterId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SubmitterId");

                    b.ToTable("Subjects", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.SubjectAttachedTag", b =>
                {
                    b.Property<Guid?>("SubjectId")
                        .HasColumnType("uuid");

                    b.Property<int>("TagId")
                        .HasColumnType("integer");

                    b.HasKey("SubjectId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("SubjectAttachedTags", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.SubjectUpdate", b =>
                {
                    b.Property<Guid>("SubjectId")
                        .HasColumnType("uuid");

                    b.Property<int>("Category")
                        .HasColumnType("integer");

                    b.Property<string>("Details")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("UpdateTimestamp")
                        .HasColumnType("bigint");

                    b.HasKey("SubjectId", "Category");

                    b.ToTable("SubjectUpdates", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.SupportingEvidence", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("IpfsCid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OriginUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PreviewImageIpfsCid")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ProposalId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ProposalId");

                    b.ToTable("SupportingEvidence", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.Tag", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int?>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Tags", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.Thing", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("AcceptedSettlementProposalId")
                        .HasColumnType("uuid");

                    b.Property<string>("CroppedImageIpfsCid")
                        .HasColumnType("text");

                    b.Property<string>("Details")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ImageIpfsCid")
                        .HasColumnType("text");

                    b.Property<Guid?>("RelatedThingId")
                        .HasColumnType("uuid");

                    b.Property<long?>("SettledAt")
                        .HasColumnType("bigint");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<Guid>("SubjectId")
                        .HasColumnType("uuid");

                    b.Property<long?>("SubmittedAt")
                        .HasColumnType("bigint");

                    b.Property<string>("SubmitterId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("VoteAggIpfsCid")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SubjectId");

                    b.HasIndex("SubmitterId");

                    b.ToTable("Things", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.ThingAttachedTag", b =>
                {
                    b.Property<Guid?>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<int>("TagId")
                        .HasColumnType("integer");

                    b.HasKey("ThingId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("ThingAttachedTags", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.ThingUpdate", b =>
                {
                    b.Property<Guid>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<int>("Category")
                        .HasColumnType("integer");

                    b.Property<string>("Details")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("UpdateTimestamp")
                        .HasColumnType("bigint");

                    b.HasKey("ThingId", "Category");

                    b.ToTable("ThingUpdates", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.ThingVerifier", b =>
                {
                    b.Property<Guid?>("ThingId")
                        .HasColumnType("uuid");

                    b.Property<string>("VerifierId")
                        .HasColumnType("text");

                    b.HasKey("ThingId", "VerifierId");

                    b.HasIndex("VerifierId");

                    b.ToTable("ThingVerifiers", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.WatchedItem", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<int>("ItemType")
                        .HasColumnType("integer");

                    b.Property<Guid>("ItemId")
                        .HasColumnType("uuid");

                    b.Property<int>("ItemUpdateCategory")
                        .HasColumnType("integer");

                    b.Property<long>("LastSeenUpdateTimestamp")
                        .HasColumnType("bigint");

                    b.HasKey("UserId", "ItemType", "ItemId", "ItemUpdateCategory");

                    b.ToTable("WatchList", "truquest");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", "truquest");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", "truquest");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", "truquest");
                });

            modelBuilder.Entity("Domain.Aggregates.AcceptancePollVote", b =>
                {
                    b.HasOne("Domain.Aggregates.ThingVerifier", null)
                        .WithOne()
                        .HasForeignKey("Domain.Aggregates.AcceptancePollVote", "ThingId", "VoterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Aggregates.AssessmentPollVote", b =>
                {
                    b.HasOne("Domain.Aggregates.SettlementProposalVerifier", null)
                        .WithOne()
                        .HasForeignKey("Domain.Aggregates.AssessmentPollVote", "SettlementProposalId", "VoterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Aggregates.Evidence", b =>
                {
                    b.HasOne("Domain.Aggregates.Thing", null)
                        .WithMany("Evidence")
                        .HasForeignKey("ThingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Aggregates.SettlementProposal", b =>
                {
                    b.HasOne("Domain.Aggregates.User", null)
                        .WithMany()
                        .HasForeignKey("SubmitterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Aggregates.Thing", null)
                        .WithMany()
                        .HasForeignKey("ThingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Aggregates.SettlementProposalVerifier", b =>
                {
                    b.HasOne("Domain.Aggregates.SettlementProposal", null)
                        .WithMany("Verifiers")
                        .HasForeignKey("SettlementProposalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Aggregates.User", null)
                        .WithMany()
                        .HasForeignKey("VerifierId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Aggregates.Subject", b =>
                {
                    b.HasOne("Domain.Aggregates.User", null)
                        .WithMany()
                        .HasForeignKey("SubmitterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Aggregates.SubjectAttachedTag", b =>
                {
                    b.HasOne("Domain.Aggregates.Subject", null)
                        .WithMany("Tags")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Aggregates.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Aggregates.SupportingEvidence", b =>
                {
                    b.HasOne("Domain.Aggregates.SettlementProposal", null)
                        .WithMany("Evidence")
                        .HasForeignKey("ProposalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Aggregates.Thing", b =>
                {
                    b.HasOne("Domain.Aggregates.Subject", null)
                        .WithMany()
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Aggregates.User", null)
                        .WithMany()
                        .HasForeignKey("SubmitterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Aggregates.ThingAttachedTag", b =>
                {
                    b.HasOne("Domain.Aggregates.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Aggregates.Thing", null)
                        .WithMany("Tags")
                        .HasForeignKey("ThingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Aggregates.ThingVerifier", b =>
                {
                    b.HasOne("Domain.Aggregates.Thing", null)
                        .WithMany("Verifiers")
                        .HasForeignKey("ThingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Aggregates.User", null)
                        .WithMany()
                        .HasForeignKey("VerifierId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Domain.Aggregates.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Domain.Aggregates.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Domain.Aggregates.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Aggregates.SettlementProposal", b =>
                {
                    b.Navigation("Evidence");

                    b.Navigation("Verifiers");
                });

            modelBuilder.Entity("Domain.Aggregates.Subject", b =>
                {
                    b.Navigation("Tags");
                });

            modelBuilder.Entity("Domain.Aggregates.Thing", b =>
                {
                    b.Navigation("Evidence");

                    b.Navigation("Tags");

                    b.Navigation("Verifiers");
                });
#pragma warning restore 612, 618
        }
    }
}
