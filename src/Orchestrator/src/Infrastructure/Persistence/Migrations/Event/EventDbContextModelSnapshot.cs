﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    [DbContext(typeof(EventDbContext))]
    partial class EventDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("truquest_events")
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Aggregates.Events.ActionableThingRelatedEvent", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<IReadOnlyDictionary<string, object>>("Payload")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("ThingIdHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("ActionableThingRelatedEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.CastedAcceptancePollVoteEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<int>("Decision")
                        .HasColumnType("integer");

                    b.Property<string>("ThingIdHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("CastedAcceptancePollVoteEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.JoinedVerifierLotteryEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<decimal>("Nonce")
                        .HasColumnType("numeric");

                    b.Property<string>("ThingIdHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("JoinedVerifierLotteryEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.Aggregates.Events.PreJoinedVerifierLotteryEvent", b =>
                {
                    b.Property<long?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long?>("Id"));

                    b.Property<long>("BlockNumber")
                        .HasColumnType("bigint");

                    b.Property<string>("DataHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ThingIdHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TxnIndex")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("PreJoinedVerifierLotteryEvents", "truquest_events");
                });

            modelBuilder.Entity("Domain.QM.VerifierLotteryWinnerQm", b =>
                {
                    b.Property<decimal>("Index")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.ToTable((string)null);

                    b.ToView("VerifierLotteryWinners", "truquest_events");
                });
#pragma warning restore 612, 618
        }
    }
}
