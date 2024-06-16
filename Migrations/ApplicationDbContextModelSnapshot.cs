﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SidusAPI.Data;

#nullable disable

namespace SidusAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("SidusAPI.ServerModels.GameMatch", b =>
                {
                    b.Property<Guid>("GameGuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("GameSettings")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("HealthCheck")
                        .HasColumnType("datetime2");

                    b.Property<int>("MaxPlayers")
                        .HasColumnType("int");

                    b.Property<int>("NumberOfModules")
                        .HasColumnType("int");

                    b.Property<int>("Winner")
                        .HasColumnType("int");

                    b.HasKey("GameGuid");

                    b.ToTable("GameMatches");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.GamePlayer", b =>
                {
                    b.Property<Guid>("GameGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("TurnNumber")
                        .HasColumnType("int");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.Property<int>("BonusExplosive")
                        .HasColumnType("int");

                    b.Property<int>("BonusHP")
                        .HasColumnType("int");

                    b.Property<int>("BonusKinetic")
                        .HasColumnType("int");

                    b.Property<int>("BonusMining")
                        .HasColumnType("int");

                    b.Property<int>("BonusThermal")
                        .HasColumnType("int");

                    b.Property<int>("Credits")
                        .HasColumnType("int");

                    b.Property<int>("FleetCount")
                        .HasColumnType("int");

                    b.Property<int>("MaxActions")
                        .HasColumnType("int");

                    b.Property<string>("ModulesGuids")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("PlayerGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Score")
                        .HasColumnType("int");

                    b.HasKey("GameGuid", "TurnNumber", "PlayerId");

                    b.ToTable("GamePlayers");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.GameTurn", b =>
                {
                    b.Property<Guid>("GameGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("TurnNumber")
                        .HasColumnType("int");

                    b.Property<string>("AllModules")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ModulesForMarket")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TurnIsOver")
                        .HasColumnType("bit");

                    b.HasKey("GameGuid", "TurnNumber");

                    b.ToTable("GameTurns");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.ServerAction", b =>
                {
                    b.Property<Guid>("GameGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("TurnNumber")
                        .HasColumnType("int");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.Property<int>("ActionOrder")
                        .HasColumnType("int");

                    b.Property<int?>("ActionTypeId")
                        .HasColumnType("int");

                    b.Property<Guid?>("GeneratedGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("SelectedModuleGameGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("SelectedModuleModuleGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("SelectedModuleTurnNumber")
                        .HasColumnType("int");

                    b.Property<Guid?>("SelectedUnitGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("XList")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("YList")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("GameGuid", "TurnNumber", "PlayerId", "ActionOrder");

                    b.HasIndex("SelectedModuleGameGuid", "SelectedModuleTurnNumber", "SelectedModuleModuleGuid");

                    b.ToTable("ServerActions");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.ServerModule", b =>
                {
                    b.Property<Guid>("GameGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("TurnNumber")
                        .HasColumnType("int");

                    b.Property<Guid?>("ModuleGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("MidBid")
                        .HasColumnType("int");

                    b.Property<int>("ModuleId")
                        .HasColumnType("int");

                    b.Property<int>("PlayerBid")
                        .HasColumnType("int");

                    b.Property<int>("TurnsLeft")
                        .HasColumnType("int");

                    b.HasKey("GameGuid", "TurnNumber", "ModuleGuid");

                    b.ToTable("ServerModules");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.ServerNode", b =>
                {
                    b.Property<Guid>("GameGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("TurnNumber")
                        .HasColumnType("int");

                    b.Property<int?>("X")
                        .HasColumnType("int");

                    b.Property<int?>("Y")
                        .HasColumnType("int");

                    b.Property<int>("CreditRegin")
                        .HasColumnType("int");

                    b.Property<Guid?>("GameTurnGameGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("GameTurnTurnNumber")
                        .HasColumnType("int");

                    b.Property<bool>("IsRift")
                        .HasColumnType("bit");

                    b.Property<int>("MaxCredits")
                        .HasColumnType("int");

                    b.Property<int>("Minerals")
                        .HasColumnType("int");

                    b.Property<int>("OwnedById")
                        .HasColumnType("int");

                    b.HasKey("GameGuid", "TurnNumber", "X", "Y");

                    b.HasIndex("GameTurnGameGuid", "GameTurnTurnNumber");

                    b.ToTable("ServerNodes");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.ServerTechnology", b =>
                {
                    b.Property<Guid>("GameGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("TurnNumber")
                        .HasColumnType("int");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.Property<int>("TechnologyId")
                        .HasColumnType("int");

                    b.Property<int>("CurrentAmount")
                        .HasColumnType("int");

                    b.Property<string>("CurrentEffectText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EffectText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("GamePlayerGameGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("GamePlayerPlayerId")
                        .HasColumnType("int");

                    b.Property<int?>("GamePlayerTurnNumber")
                        .HasColumnType("int");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.Property<int>("NeededAmount")
                        .HasColumnType("int");

                    b.Property<string>("RequirementText")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("GameGuid", "TurnNumber", "PlayerId", "TechnologyId");

                    b.HasIndex("GamePlayerGameGuid", "GamePlayerTurnNumber", "GamePlayerPlayerId");

                    b.ToTable("ServerTechnologies");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.ServerUnit", b =>
                {
                    b.Property<Guid>("GameGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("TurnNumber")
                        .HasColumnType("int");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.Property<Guid>("UnitGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AttachedModules")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Color")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ExplosiveDamageModifier")
                        .HasColumnType("int");

                    b.Property<int>("ExplosivePower")
                        .HasColumnType("int");

                    b.Property<int>("Facing")
                        .HasColumnType("int");

                    b.Property<Guid?>("GamePlayerGameGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("GamePlayerPlayerId")
                        .HasColumnType("int");

                    b.Property<int?>("GamePlayerTurnNumber")
                        .HasColumnType("int");

                    b.Property<int>("GlobalCreditGain")
                        .HasColumnType("int");

                    b.Property<int>("HP")
                        .HasColumnType("int");

                    b.Property<bool>("IsStation")
                        .HasColumnType("bit");

                    b.Property<int>("KineticDamageModifier")
                        .HasColumnType("int");

                    b.Property<int>("KineticPower")
                        .HasColumnType("int");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.Property<int>("MaxAttachedModules")
                        .HasColumnType("int");

                    b.Property<int>("MaxHP")
                        .HasColumnType("int");

                    b.Property<int>("MaxMining")
                        .HasColumnType("int");

                    b.Property<int>("MaxMovement")
                        .HasColumnType("int");

                    b.Property<int>("MiningLeft")
                        .HasColumnType("int");

                    b.Property<string>("ModuleEffects")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MovementLeft")
                        .HasColumnType("int");

                    b.Property<double>("SupportValue")
                        .HasColumnType("float");

                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.Property<int>("ThermalDamageModifier")
                        .HasColumnType("int");

                    b.Property<int>("ThermalPower")
                        .HasColumnType("int");

                    b.Property<string>("UnitName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("X")
                        .HasColumnType("int");

                    b.Property<int?>("Y")
                        .HasColumnType("int");

                    b.HasKey("GameGuid", "TurnNumber", "PlayerId", "UnitGuid");

                    b.HasIndex("GamePlayerGameGuid", "GamePlayerTurnNumber", "GamePlayerPlayerId");

                    b.ToTable("ServerUnits");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.GamePlayer", b =>
                {
                    b.HasOne("SidusAPI.ServerModels.GameTurn", null)
                        .WithMany("Players")
                        .HasForeignKey("GameGuid", "TurnNumber")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SidusAPI.ServerModels.GameTurn", b =>
                {
                    b.HasOne("SidusAPI.ServerModels.GameMatch", null)
                        .WithMany("GameTurns")
                        .HasForeignKey("GameGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SidusAPI.ServerModels.ServerAction", b =>
                {
                    b.HasOne("SidusAPI.ServerModels.GamePlayer", null)
                        .WithMany("Actions")
                        .HasForeignKey("GameGuid", "TurnNumber", "PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SidusAPI.ServerModels.ServerModule", "SelectedModule")
                        .WithMany()
                        .HasForeignKey("SelectedModuleGameGuid", "SelectedModuleTurnNumber", "SelectedModuleModuleGuid");

                    b.Navigation("SelectedModule");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.ServerModule", b =>
                {
                    b.HasOne("SidusAPI.ServerModels.GameTurn", null)
                        .WithMany("MarketModules")
                        .HasForeignKey("GameGuid", "TurnNumber")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SidusAPI.ServerModels.ServerNode", b =>
                {
                    b.HasOne("SidusAPI.ServerModels.GameTurn", null)
                        .WithMany("AllNodes")
                        .HasForeignKey("GameTurnGameGuid", "GameTurnTurnNumber");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.ServerTechnology", b =>
                {
                    b.HasOne("SidusAPI.ServerModels.GamePlayer", null)
                        .WithMany("Technology")
                        .HasForeignKey("GamePlayerGameGuid", "GamePlayerTurnNumber", "GamePlayerPlayerId");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.ServerUnit", b =>
                {
                    b.HasOne("SidusAPI.ServerModels.GamePlayer", null)
                        .WithMany("Units")
                        .HasForeignKey("GamePlayerGameGuid", "GamePlayerTurnNumber", "GamePlayerPlayerId");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.GameMatch", b =>
                {
                    b.Navigation("GameTurns");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.GamePlayer", b =>
                {
                    b.Navigation("Actions");

                    b.Navigation("Technology");

                    b.Navigation("Units");
                });

            modelBuilder.Entity("SidusAPI.ServerModels.GameTurn", b =>
                {
                    b.Navigation("AllNodes");

                    b.Navigation("MarketModules");

                    b.Navigation("Players");
                });
#pragma warning restore 612, 618
        }
    }
}
