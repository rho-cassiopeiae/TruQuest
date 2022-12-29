﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class SelectWinnerIndicesFuncsV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlResourceUp("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents.SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents.v1.sql");
            migrationBuilder.SqlResourceUp("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.SelectWinnerIndicesAccordingToPreJoinedThingAssessmentVerifierLotteryEvents.SelectWinnerIndicesAccordingToPreJoinedThingAssessmentVerifierLotteryEvents.v1.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlResourceDown("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents.SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents.v1.sql");
            migrationBuilder.SqlResourceDown("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.SelectWinnerIndicesAccordingToPreJoinedThingAssessmentVerifierLotteryEvents.SelectWinnerIndicesAccordingToPreJoinedThingAssessmentVerifierLotteryEvents.v1.sql");
        }
    }
}