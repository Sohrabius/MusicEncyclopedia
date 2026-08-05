using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicEncyclopedia.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlbumCategory",
                columns: table => new
                {
                    AlbumCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumCategory", x => x.AlbumCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "AlbumRelationType",
                columns: table => new
                {
                    AlbumRelationTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumRelationType", x => x.AlbumRelationTypeId);
                });

            migrationBuilder.CreateTable(
                name: "AliasType",
                columns: table => new
                {
                    AliasTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AliasType", x => x.AliasTypeId);
                });

            migrationBuilder.CreateTable(
                name: "AttributeDefinition",
                columns: table => new
                {
                    AttributeDefinitionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DataTypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeDefinition", x => x.AttributeDefinitionId);
                });

            migrationBuilder.CreateTable(
                name: "AwardResultType",
                columns: table => new
                {
                    AwardResultTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AwardResultType", x => x.AwardResultTypeId);
                });

            migrationBuilder.CreateTable(
                name: "CompanyRoleType",
                columns: table => new
                {
                    CompanyRoleTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyRoleType", x => x.CompanyRoleTypeId);
                });

            migrationBuilder.CreateTable(
                name: "CompanyType",
                columns: table => new
                {
                    CompanyTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyType", x => x.CompanyTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "CountryRoleType",
                columns: table => new
                {
                    CountryRoleTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryRoleType", x => x.CountryRoleTypeId);
                });

            migrationBuilder.CreateTable(
                name: "EntityType",
                columns: table => new
                {
                    EntityTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityType", x => x.EntityTypeId);
                });

            migrationBuilder.CreateTable(
                name: "EventType",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventType", x => x.EventTypeId);
                });

            migrationBuilder.CreateTable(
                name: "IdentifierType",
                columns: table => new
                {
                    IdentifierTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentifierType", x => x.IdentifierTypeId);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentFamily",
                columns: table => new
                {
                    InstrumentFamilyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentFamily", x => x.InstrumentFamilyId);
                });

            migrationBuilder.CreateTable(
                name: "Language",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.LanguageId);
                });

            migrationBuilder.CreateTable(
                name: "LinkType",
                columns: table => new
                {
                    LinkTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkType", x => x.LinkTypeId);
                });

            migrationBuilder.CreateTable(
                name: "LocationType",
                columns: table => new
                {
                    LocationTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationType", x => x.LocationTypeId);
                });

            migrationBuilder.CreateTable(
                name: "LyricsAvailabilityType",
                columns: table => new
                {
                    LyricsAvailabilityTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LyricsAvailabilityType", x => x.LyricsAvailabilityTypeId);
                });

            migrationBuilder.CreateTable(
                name: "MediaRoleType",
                columns: table => new
                {
                    MediaRoleTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaRoleType", x => x.MediaRoleTypeId);
                });

            migrationBuilder.CreateTable(
                name: "MediaType",
                columns: table => new
                {
                    MediaTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaType", x => x.MediaTypeId);
                });

            migrationBuilder.CreateTable(
                name: "MusicalKey",
                columns: table => new
                {
                    MusicalKeyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicalKey", x => x.MusicalKeyId);
                });

            migrationBuilder.CreateTable(
                name: "PersonKind",
                columns: table => new
                {
                    PersonKindId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonKind", x => x.PersonKindId);
                });

            migrationBuilder.CreateTable(
                name: "PersonType",
                columns: table => new
                {
                    PersonTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonType", x => x.PersonTypeId);
                });

            migrationBuilder.CreateTable(
                name: "PublicationType",
                columns: table => new
                {
                    PublicationTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationType", x => x.PublicationTypeId);
                });

            migrationBuilder.CreateTable(
                name: "RoleScopeType",
                columns: table => new
                {
                    RoleScopeTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleScopeType", x => x.RoleScopeTypeId);
                });

            migrationBuilder.CreateTable(
                name: "SessionType",
                columns: table => new
                {
                    SessionTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionType", x => x.SessionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "SourceType",
                columns: table => new
                {
                    SourceTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceType", x => x.SourceTypeId);
                });

            migrationBuilder.CreateTable(
                name: "TrackRelationType",
                columns: table => new
                {
                    TrackRelationTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackRelationType", x => x.TrackRelationTypeId);
                });

            migrationBuilder.CreateTable(
                name: "TrackVersionType",
                columns: table => new
                {
                    TrackVersionTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackVersionType", x => x.TrackVersionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "VocalStyle",
                columns: table => new
                {
                    VocalStyleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocalStyle", x => x.VocalStyleId);
                });

            migrationBuilder.CreateTable(
                name: "Entity",
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_Entity_EntityType_EntityTypeId",
                        column: x => x.EntityTypeId,
                        principalTable: "EntityType",
                        principalColumn: "EntityTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alias",
                columns: table => new
                {
                    AliasId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    AliasTypeId = table.Column<int>(type: "int", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    AliasName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alias", x => x.AliasId);
                    table.ForeignKey(
                        name: "FK_Alias_AliasType_AliasTypeId",
                        column: x => x.AliasTypeId,
                        principalTable: "AliasType",
                        principalColumn: "AliasTypeId");
                    table.ForeignKey(
                        name: "FK_Alias_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "LanguageId");
                });

            migrationBuilder.CreateTable(
                name: "AttributeValue",
                columns: table => new
                {
                    AttributeValueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    AttributeDefinitionId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ValueString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueInt = table.Column<int>(type: "int", nullable: true),
                    ValueDecimal = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    ValueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ValueDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValueBit = table.Column<bool>(type: "bit", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeValue", x => x.AttributeValueId);
                    table.ForeignKey(
                        name: "FK_AttributeValue_AttributeDefinition_AttributeDefinitionId",
                        column: x => x.AttributeDefinitionId,
                        principalTable: "AttributeDefinition",
                        principalColumn: "AttributeDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttributeValue_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "LanguageId");
                });

            migrationBuilder.CreateTable(
                name: "Localization",
                columns: table => new
                {
                    LocalizationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LocalizedText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localization", x => x.LocalizationId);
                    table.ForeignKey(
                        name: "FK_Localization_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "LanguageId");
                });

            migrationBuilder.CreateTable(
                name: "EntityLink",
                columns: table => new
                {
                    EntityLinkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    LinkTypeId = table.Column<int>(type: "int", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityLink", x => x.EntityLinkId);
                    table.ForeignKey(
                        name: "FK_EntityLink_LinkType_LinkTypeId",
                        column: x => x.LinkTypeId,
                        principalTable: "LinkType",
                        principalColumn: "LinkTypeId");
                });

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    MediaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MediaTypeId = table.Column<int>(type: "int", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ThumbnailUrl150 = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ThumbnailUrl300 = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ThumbnailUrl600 = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ThumbnailUrl1200 = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.MediaId);
                    table.ForeignKey(
                        name: "FK_Media_MediaType_MediaTypeId",
                        column: x => x.MediaTypeId,
                        principalTable: "MediaType",
                        principalColumn: "MediaTypeId");
                });

            migrationBuilder.CreateTable(
                name: "CreditRole",
                columns: table => new
                {
                    CreditRoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    RoleScopeTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditRole", x => x.CreditRoleId);
                    table.ForeignKey(
                        name: "FK_CreditRole_RoleScopeType_RoleScopeTypeId",
                        column: x => x.RoleScopeTypeId,
                        principalTable: "RoleScopeType",
                        principalColumn: "RoleScopeTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Award",
                columns: table => new
                {
                    AwardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Organization = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Award", x => x.AwardId);
                    table.ForeignKey(
                        name: "FK_Award_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_Award_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Certification",
                columns: table => new
                {
                    CertificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Organization = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certification", x => x.CertificationId);
                    table.ForeignKey(
                        name: "FK_Certification_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_Certification_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chart",
                columns: table => new
                {
                    ChartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Publisher = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chart", x => x.ChartId);
                    table.ForeignKey(
                        name: "FK_Chart_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_Chart_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NameSort = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnglishName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompanyTypeId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    History = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.CompanyId);
                    table.ForeignKey(
                        name: "FK_Company_CompanyType_CompanyTypeId",
                        column: x => x.CompanyTypeId,
                        principalTable: "CompanyType",
                        principalColumn: "CompanyTypeId");
                    table.ForeignKey(
                        name: "FK_Company_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_Company_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Genre",
                columns: table => new
                {
                    GenreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NameSort = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentGenreId = table.Column<int>(type: "int", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genre", x => x.GenreId);
                    table.ForeignKey(
                        name: "FK_Genre_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Genre_Genre_ParentGenreId",
                        column: x => x.ParentGenreId,
                        principalTable: "Genre",
                        principalColumn: "GenreId");
                });

            migrationBuilder.CreateTable(
                name: "Instrument",
                columns: table => new
                {
                    InstrumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstrumentFamilyId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    HistoricalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrument", x => x.InstrumentId);
                    table.ForeignKey(
                        name: "FK_Instrument_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_Instrument_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Instrument_InstrumentFamily_InstrumentFamilyId",
                        column: x => x.InstrumentFamilyId,
                        principalTable: "InstrumentFamily",
                        principalColumn: "InstrumentFamilyId");
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    LocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LocationTypeId = table.Column<int>(type: "int", nullable: true),
                    ParentLocationId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.LocationId);
                    table.ForeignKey(
                        name: "FK_Location_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_Location_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Location_LocationType_LocationTypeId",
                        column: x => x.LocationTypeId,
                        principalTable: "LocationType",
                        principalColumn: "LocationTypeId");
                    table.ForeignKey(
                        name: "FK_Location_Location_ParentLocationId",
                        column: x => x.ParentLocationId,
                        principalTable: "Location",
                        principalColumn: "LocationId");
                });

            migrationBuilder.CreateTable(
                name: "Mood",
                columns: table => new
                {
                    MoodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mood", x => x.MoodId);
                    table.ForeignKey(
                        name: "FK_Mood_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    TagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.TagId);
                    table.ForeignKey(
                        name: "FK_Tag_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Track",
                columns: table => new
                {
                    TrackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TitleSort = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnglishTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    RecordingStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RecordingEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RecordingDatePrecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReleaseDatePrecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LyricsAvailabilityTypeId = table.Column<int>(type: "int", nullable: true),
                    VocalStyleId = table.Column<int>(type: "int", nullable: true),
                    MusicalKeyId = table.Column<int>(type: "int", nullable: true),
                    BPM = table.Column<short>(type: "smallint", nullable: true),
                    ISRC = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    IsInstrumental = table.Column<bool>(type: "bit", nullable: false),
                    IsExplicit = table.Column<bool>(type: "bit", nullable: false),
                    CopyrightNotice = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Track", x => x.TrackId);
                    table.ForeignKey(
                        name: "FK_Track_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Track_LyricsAvailabilityType_LyricsAvailabilityTypeId",
                        column: x => x.LyricsAvailabilityTypeId,
                        principalTable: "LyricsAvailabilityType",
                        principalColumn: "LyricsAvailabilityTypeId");
                    table.ForeignKey(
                        name: "FK_Track_MusicalKey_MusicalKeyId",
                        column: x => x.MusicalKeyId,
                        principalTable: "MusicalKey",
                        principalColumn: "MusicalKeyId");
                    table.ForeignKey(
                        name: "FK_Track_VocalStyle_VocalStyleId",
                        column: x => x.VocalStyleId,
                        principalTable: "VocalStyle",
                        principalColumn: "VocalStyleId");
                });

            migrationBuilder.CreateTable(
                name: "Album",
                columns: table => new
                {
                    AlbumId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TitleSort = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnglishTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AlbumCategoryId = table.Column<int>(type: "int", nullable: false),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReleaseDatePrecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecordingStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RecordingEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RecordingDatePrecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverMediaId = table.Column<int>(type: "int", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    CopyrightNotice = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsOfficial = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Album", x => x.AlbumId);
                    table.ForeignKey(
                        name: "FK_Album_AlbumCategory_AlbumCategoryId",
                        column: x => x.AlbumCategoryId,
                        principalTable: "AlbumCategory",
                        principalColumn: "AlbumCategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Album_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Album_Media_CoverMediaId",
                        column: x => x.CoverMediaId,
                        principalTable: "Media",
                        principalColumn: "MediaId");
                });

            migrationBuilder.CreateTable(
                name: "MediaAssignment",
                columns: table => new
                {
                    MediaAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MediaId = table.Column<int>(type: "int", nullable: false),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    MediaRoleTypeId = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssignment", x => x.MediaAssignmentId);
                    table.ForeignKey(
                        name: "FK_MediaAssignment_MediaRoleType_MediaRoleTypeId",
                        column: x => x.MediaRoleTypeId,
                        principalTable: "MediaRoleType",
                        principalColumn: "MediaRoleTypeId");
                    table.ForeignKey(
                        name: "FK_MediaAssignment_Media_MediaId",
                        column: x => x.MediaId,
                        principalTable: "Media",
                        principalColumn: "MediaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditRoleEntityType",
                columns: table => new
                {
                    CreditRoleEntityTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreditRoleId = table.Column<int>(type: "int", nullable: false),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditRoleEntityType", x => x.CreditRoleEntityTypeId);
                    table.ForeignKey(
                        name: "FK_CreditRoleEntityType_CreditRole_CreditRoleId",
                        column: x => x.CreditRoleId,
                        principalTable: "CreditRole",
                        principalColumn: "CreditRoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditRoleEntityType_EntityType_EntityTypeId",
                        column: x => x.EntityTypeId,
                        principalTable: "EntityType",
                        principalColumn: "EntityTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AwardAssignment",
                columns: table => new
                {
                    AwardAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AwardId = table.Column<int>(type: "int", nullable: false),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    AwardResultTypeId = table.Column<int>(type: "int", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AwardAssignment", x => x.AwardAssignmentId);
                    table.ForeignKey(
                        name: "FK_AwardAssignment_AwardResultType_AwardResultTypeId",
                        column: x => x.AwardResultTypeId,
                        principalTable: "AwardResultType",
                        principalColumn: "AwardResultTypeId");
                    table.ForeignKey(
                        name: "FK_AwardAssignment_Award_AwardId",
                        column: x => x.AwardId,
                        principalTable: "Award",
                        principalColumn: "AwardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CertificationAssignment",
                columns: table => new
                {
                    CertificationAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CertificationId = table.Column<int>(type: "int", nullable: false),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    CertificationLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificationAssignment", x => x.CertificationAssignmentId);
                    table.ForeignKey(
                        name: "FK_CertificationAssignment_Certification_CertificationId",
                        column: x => x.CertificationId,
                        principalTable: "Certification",
                        principalColumn: "CertificationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChartEntry",
                columns: table => new
                {
                    ChartEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChartId = table.Column<int>(type: "int", nullable: false),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    PreviousPosition = table.Column<int>(type: "int", nullable: true),
                    WeeksOnChart = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartEntry", x => x.ChartEntryId);
                    table.ForeignKey(
                        name: "FK_ChartEntry_Chart_ChartId",
                        column: x => x.ChartId,
                        principalTable: "Chart",
                        principalColumn: "ChartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Source",
                columns: table => new
                {
                    SourceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    SourceTypeId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PublisherId = table.Column<int>(type: "int", nullable: true),
                    PublicationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Source", x => x.SourceId);
                    table.ForeignKey(
                        name: "FK_Source_Company_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Company",
                        principalColumn: "CompanyId");
                    table.ForeignKey(
                        name: "FK_Source_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Source_SourceType_SourceTypeId",
                        column: x => x.SourceTypeId,
                        principalTable: "SourceType",
                        principalColumn: "SourceTypeId");
                });

            migrationBuilder.CreateTable(
                name: "PerformanceEvent",
                columns: table => new
                {
                    PerformanceEventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EventTypeId = table.Column<int>(type: "int", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AudienceInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformanceNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImprovisationNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceEvent", x => x.PerformanceEventId);
                    table.ForeignKey(
                        name: "FK_PerformanceEvent_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerformanceEvent_EventType_EventTypeId",
                        column: x => x.EventTypeId,
                        principalTable: "EventType",
                        principalColumn: "EventTypeId");
                    table.ForeignKey(
                        name: "FK_PerformanceEvent_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "LocationId");
                });

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FullNameSort = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnglishName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PersonKindId = table.Column<int>(type: "int", nullable: true),
                    Biography = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BirthDatePrecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BirthLocationId = table.Column<int>(type: "int", nullable: true),
                    DeathDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DeathDatePrecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeathLocationId = table.Column<int>(type: "int", nullable: true),
                    NationalityCountryId = table.Column<int>(type: "int", nullable: true),
                    ImageMediaId = table.Column<int>(type: "int", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.PersonId);
                    table.ForeignKey(
                        name: "FK_Person_Country_NationalityCountryId",
                        column: x => x.NationalityCountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_Person_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Person_Location_BirthLocationId",
                        column: x => x.BirthLocationId,
                        principalTable: "Location",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Person_Location_DeathLocationId",
                        column: x => x.DeathLocationId,
                        principalTable: "Location",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Person_Media_ImageMediaId",
                        column: x => x.ImageMediaId,
                        principalTable: "Media",
                        principalColumn: "MediaId");
                    table.ForeignKey(
                        name: "FK_Person_PersonKind_PersonKindId",
                        column: x => x.PersonKindId,
                        principalTable: "PersonKind",
                        principalColumn: "PersonKindId");
                });

            migrationBuilder.CreateTable(
                name: "RecordingSession",
                columns: table => new
                {
                    RecordingSessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    SessionTypeId = table.Column<int>(type: "int", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DatePrecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordingSession", x => x.RecordingSessionId);
                    table.ForeignKey(
                        name: "FK_RecordingSession_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecordingSession_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "LocationId");
                    table.ForeignKey(
                        name: "FK_RecordingSession_SessionType_SessionTypeId",
                        column: x => x.SessionTypeId,
                        principalTable: "SessionType",
                        principalColumn: "SessionTypeId");
                });

            migrationBuilder.CreateTable(
                name: "TagAssignment",
                columns: table => new
                {
                    TagAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagAssignment", x => x.TagAssignmentId);
                    table.ForeignKey(
                        name: "FK_TagAssignment_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "Tag",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackGenre",
                columns: table => new
                {
                    TrackGenreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackGenre", x => x.TrackGenreId);
                    table.ForeignKey(
                        name: "FK_TrackGenre_Genre_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genre",
                        principalColumn: "GenreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackGenre_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackInstrument",
                columns: table => new
                {
                    TrackInstrumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    InstrumentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackInstrument", x => x.TrackInstrumentId);
                    table.ForeignKey(
                        name: "FK_TrackInstrument_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackInstrument_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackMood",
                columns: table => new
                {
                    TrackMoodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    MoodId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackMood", x => x.TrackMoodId);
                    table.ForeignKey(
                        name: "FK_TrackMood_Mood_MoodId",
                        column: x => x.MoodId,
                        principalTable: "Mood",
                        principalColumn: "MoodId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackMood_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackRelation",
                columns: table => new
                {
                    TrackRelationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    RelatedTrackId = table.Column<int>(type: "int", nullable: false),
                    TrackRelationTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackRelation", x => x.TrackRelationId);
                    table.ForeignKey(
                        name: "FK_TrackRelation_TrackRelationType_TrackRelationTypeId",
                        column: x => x.TrackRelationTypeId,
                        principalTable: "TrackRelationType",
                        principalColumn: "TrackRelationTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackRelation_Track_RelatedTrackId",
                        column: x => x.RelatedTrackId,
                        principalTable: "Track",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrackRelation_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrackVersionTypeAssignment",
                columns: table => new
                {
                    TrackVersionTypeAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    TrackVersionTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackVersionTypeAssignment", x => x.TrackVersionTypeAssignmentId);
                    table.ForeignKey(
                        name: "FK_TrackVersionTypeAssignment_TrackVersionType_TrackVersionTypeId",
                        column: x => x.TrackVersionTypeId,
                        principalTable: "TrackVersionType",
                        principalColumn: "TrackVersionTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackVersionTypeAssignment_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumCompany",
                columns: table => new
                {
                    AlbumCompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CompanyRoleTypeId = table.Column<int>(type: "int", nullable: true),
                    CatalogNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumCompany", x => x.AlbumCompanyId);
                    table.ForeignKey(
                        name: "FK_AlbumCompany_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumCompany_CompanyRoleType_CompanyRoleTypeId",
                        column: x => x.CompanyRoleTypeId,
                        principalTable: "CompanyRoleType",
                        principalColumn: "CompanyRoleTypeId");
                    table.ForeignKey(
                        name: "FK_AlbumCompany_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumCountry",
                columns: table => new
                {
                    AlbumCountryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    CountryRoleTypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumCountry", x => x.AlbumCountryId);
                    table.ForeignKey(
                        name: "FK_AlbumCountry_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumCountry_CountryRoleType_CountryRoleTypeId",
                        column: x => x.CountryRoleTypeId,
                        principalTable: "CountryRoleType",
                        principalColumn: "CountryRoleTypeId");
                    table.ForeignKey(
                        name: "FK_AlbumCountry_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId");
                });

            migrationBuilder.CreateTable(
                name: "AlbumGenre",
                columns: table => new
                {
                    AlbumGenreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumGenre", x => x.AlbumGenreId);
                    table.ForeignKey(
                        name: "FK_AlbumGenre_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumGenre_Genre_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genre",
                        principalColumn: "GenreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumIdentifier",
                columns: table => new
                {
                    AlbumIdentifierId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    IdentifierTypeId = table.Column<int>(type: "int", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumIdentifier", x => x.AlbumIdentifierId);
                    table.ForeignKey(
                        name: "FK_AlbumIdentifier_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumIdentifier_IdentifierType_IdentifierTypeId",
                        column: x => x.IdentifierTypeId,
                        principalTable: "IdentifierType",
                        principalColumn: "IdentifierTypeId");
                });

            migrationBuilder.CreateTable(
                name: "AlbumLanguage",
                columns: table => new
                {
                    AlbumLanguageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumLanguage", x => x.AlbumLanguageId);
                    table.ForeignKey(
                        name: "FK_AlbumLanguage_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumLanguage_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "LanguageId");
                });

            migrationBuilder.CreateTable(
                name: "AlbumMood",
                columns: table => new
                {
                    AlbumMoodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    MoodId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumMood", x => x.AlbumMoodId);
                    table.ForeignKey(
                        name: "FK_AlbumMood_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumMood_Mood_MoodId",
                        column: x => x.MoodId,
                        principalTable: "Mood",
                        principalColumn: "MoodId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumRelation",
                columns: table => new
                {
                    AlbumRelationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    RelatedAlbumId = table.Column<int>(type: "int", nullable: false),
                    AlbumRelationTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumRelation", x => x.AlbumRelationId);
                    table.ForeignKey(
                        name: "FK_AlbumRelation_AlbumRelationType_AlbumRelationTypeId",
                        column: x => x.AlbumRelationTypeId,
                        principalTable: "AlbumRelationType",
                        principalColumn: "AlbumRelationTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumRelation_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlbumRelation_Album_RelatedAlbumId",
                        column: x => x.RelatedAlbumId,
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlbumTrack",
                columns: table => new
                {
                    AlbumTrackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    DiscNumber = table.Column<int>(type: "int", nullable: false),
                    TrackNumber = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    TrackTitleOverride = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DurationSecondsOverride = table.Column<int>(type: "int", nullable: true),
                    IsBonus = table.Column<bool>(type: "bit", nullable: false),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumTrack", x => x.AlbumTrackId);
                    table.ForeignKey(
                        name: "FK_AlbumTrack_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumTrack_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Citation",
                columns: table => new
                {
                    CitationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Quote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PageNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AccessedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citation", x => x.CitationId);
                    table.ForeignKey(
                        name: "FK_Citation_Source_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Source",
                        principalColumn: "SourceId");
                });

            migrationBuilder.CreateTable(
                name: "PerformanceEventAlbum",
                columns: table => new
                {
                    PerformanceEventAlbumId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerformanceEventId = table.Column<int>(type: "int", nullable: false),
                    AlbumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceEventAlbum", x => x.PerformanceEventAlbumId);
                    table.ForeignKey(
                        name: "FK_PerformanceEventAlbum_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerformanceEventAlbum_PerformanceEvent_PerformanceEventId",
                        column: x => x.PerformanceEventId,
                        principalTable: "PerformanceEvent",
                        principalColumn: "PerformanceEventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceEventTrack",
                columns: table => new
                {
                    PerformanceEventTrackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerformanceEventId = table.Column<int>(type: "int", nullable: false),
                    TrackId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceEventTrack", x => x.PerformanceEventTrackId);
                    table.ForeignKey(
                        name: "FK_PerformanceEventTrack_PerformanceEvent_PerformanceEventId",
                        column: x => x.PerformanceEventId,
                        principalTable: "PerformanceEvent",
                        principalColumn: "PerformanceEventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerformanceEventTrack_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Credit",
                columns: table => new
                {
                    CreditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityTypeId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    CreditRoleId = table.Column<int>(type: "int", nullable: false),
                    RoleScopeTypeId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    InstrumentId = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credit", x => x.CreditId);
                    table.ForeignKey(
                        name: "FK_Credit_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId");
                    table.ForeignKey(
                        name: "FK_Credit_CreditRole_CreditRoleId",
                        column: x => x.CreditRoleId,
                        principalTable: "CreditRole",
                        principalColumn: "CreditRoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Credit_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId");
                    table.ForeignKey(
                        name: "FK_Credit_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "PersonId");
                    table.ForeignKey(
                        name: "FK_Credit_RoleScopeType_RoleScopeTypeId",
                        column: x => x.RoleScopeTypeId,
                        principalTable: "RoleScopeType",
                        principalColumn: "RoleScopeTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusicianInstrument",
                columns: table => new
                {
                    MusicianInstrumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicianInstrument", x => x.MusicianInstrumentId);
                    table.ForeignKey(
                        name: "FK_MusicianInstrument_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusicianInstrument_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonTypeAssignment",
                columns: table => new
                {
                    PersonTypeAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    PersonTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonTypeAssignment", x => x.PersonTypeAssignmentId);
                    table.ForeignKey(
                        name: "FK_PersonTypeAssignment_PersonType_PersonTypeId",
                        column: x => x.PersonTypeId,
                        principalTable: "PersonType",
                        principalColumn: "PersonTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonTypeAssignment_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Publication",
                columns: table => new
                {
                    PublicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PublicationTypeId = table.Column<int>(type: "int", nullable: true),
                    PublisherId = table.Column<int>(type: "int", nullable: true),
                    PublicationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PublicationDatePrecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ISBN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publication", x => x.PublicationId);
                    table.ForeignKey(
                        name: "FK_Publication_Company_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Company",
                        principalColumn: "CompanyId");
                    table.ForeignKey(
                        name: "FK_Publication_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Publication_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Publication_PublicationType_PublicationTypeId",
                        column: x => x.PublicationTypeId,
                        principalTable: "PublicationType",
                        principalColumn: "PublicationTypeId");
                });

            migrationBuilder.CreateTable(
                name: "RecordingSessionAlbum",
                columns: table => new
                {
                    RecordingSessionAlbumId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecordingSessionId = table.Column<int>(type: "int", nullable: false),
                    AlbumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordingSessionAlbum", x => x.RecordingSessionAlbumId);
                    table.ForeignKey(
                        name: "FK_RecordingSessionAlbum_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecordingSessionAlbum_RecordingSession_RecordingSessionId",
                        column: x => x.RecordingSessionId,
                        principalTable: "RecordingSession",
                        principalColumn: "RecordingSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecordingSessionTrack",
                columns: table => new
                {
                    RecordingSessionTrackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecordingSessionId = table.Column<int>(type: "int", nullable: false),
                    TrackId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordingSessionTrack", x => x.RecordingSessionTrackId);
                    table.ForeignKey(
                        name: "FK_RecordingSessionTrack_RecordingSession_RecordingSessionId",
                        column: x => x.RecordingSessionId,
                        principalTable: "RecordingSession",
                        principalColumn: "RecordingSessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecordingSessionTrack_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Poem",
                columns: table => new
                {
                    PoemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnglishTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PublicationId = table.Column<int>(type: "int", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Book = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalPublicationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OriginalPublicationDatePrecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExternalReferenceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Copyright = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CanonicalText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poem", x => x.PoemId);
                    table.ForeignKey(
                        name: "FK_Poem_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Poem_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "PersonId");
                    table.ForeignKey(
                        name: "FK_Poem_Publication_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publication",
                        principalColumn: "PublicationId");
                });

            migrationBuilder.CreateTable(
                name: "SungVersion",
                columns: table => new
                {
                    SungVersionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    PoemId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VocalStyleId = table.Column<int>(type: "int", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCanonical = table.Column<bool>(type: "bit", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SungVersion", x => x.SungVersionId);
                    table.ForeignKey(
                        name: "FK_SungVersion_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SungVersion_Poem_PoemId",
                        column: x => x.PoemId,
                        principalTable: "Poem",
                        principalColumn: "PoemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SungVersion_VocalStyle_VocalStyleId",
                        column: x => x.VocalStyleId,
                        principalTable: "VocalStyle",
                        principalColumn: "VocalStyleId");
                });

            migrationBuilder.CreateTable(
                name: "TrackSungVersion",
                columns: table => new
                {
                    TrackSungVersionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    SungVersionId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackSungVersion", x => x.TrackSungVersionId);
                    table.ForeignKey(
                        name: "FK_TrackSungVersion_SungVersion_SungVersionId",
                        column: x => x.SungVersionId,
                        principalTable: "SungVersion",
                        principalColumn: "SungVersionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackSungVersion_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Album_AlbumCategoryId",
                table: "Album",
                column: "AlbumCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Album_CoverMediaId",
                table: "Album",
                column: "CoverMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_Album_EntityId",
                table: "Album",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Album_Slug",
                table: "Album",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumCategory_Code",
                table: "AlbumCategory",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumCompany_AlbumId",
                table: "AlbumCompany",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumCompany_CompanyId",
                table: "AlbumCompany",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumCompany_CompanyRoleTypeId",
                table: "AlbumCompany",
                column: "CompanyRoleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumCountry_AlbumId",
                table: "AlbumCountry",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumCountry_CountryId",
                table: "AlbumCountry",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumCountry_CountryRoleTypeId",
                table: "AlbumCountry",
                column: "CountryRoleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumCountry_Unique",
                table: "AlbumCountry",
                columns: new[] { "AlbumId", "CountryId" },
                unique: true,
                filter: "[CountryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumGenre_AlbumId",
                table: "AlbumGenre",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumGenre_GenreId",
                table: "AlbumGenre",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumGenre_Unique",
                table: "AlbumGenre",
                columns: new[] { "AlbumId", "GenreId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumIdentifier_AlbumId",
                table: "AlbumIdentifier",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumIdentifier_IdentifierTypeId",
                table: "AlbumIdentifier",
                column: "IdentifierTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumIdentifier_Unique",
                table: "AlbumIdentifier",
                columns: new[] { "AlbumId", "IdentifierTypeId", "Value" },
                unique: true,
                filter: "[IdentifierTypeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumLanguage_AlbumId",
                table: "AlbumLanguage",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumLanguage_LanguageId",
                table: "AlbumLanguage",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumLanguage_Unique",
                table: "AlbumLanguage",
                columns: new[] { "AlbumId", "LanguageId" },
                unique: true,
                filter: "[LanguageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumMood_AlbumId",
                table: "AlbumMood",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumMood_MoodId",
                table: "AlbumMood",
                column: "MoodId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumMood_Unique",
                table: "AlbumMood",
                columns: new[] { "AlbumId", "MoodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumRelation_AlbumId",
                table: "AlbumRelation",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumRelation_AlbumRelationTypeId",
                table: "AlbumRelation",
                column: "AlbumRelationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumRelation_RelatedAlbumId",
                table: "AlbumRelation",
                column: "RelatedAlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumRelation_Unique",
                table: "AlbumRelation",
                columns: new[] { "AlbumId", "RelatedAlbumId", "AlbumRelationTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumRelationType_Code",
                table: "AlbumRelationType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTrack_AlbumId",
                table: "AlbumTrack",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTrack_AlbumId_TrackId",
                table: "AlbumTrack",
                columns: new[] { "AlbumId", "TrackId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTrack_TrackId",
                table: "AlbumTrack",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Alias_AliasTypeId",
                table: "Alias",
                column: "AliasTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Alias_EntityTypeId_EntityId",
                table: "Alias",
                columns: new[] { "EntityTypeId", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Alias_LanguageId",
                table: "Alias",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_AliasType_Code",
                table: "AliasType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttributeValue_AttributeDefinitionId",
                table: "AttributeValue",
                column: "AttributeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeValue_EntityTypeId_EntityId",
                table: "AttributeValue",
                columns: new[] { "EntityTypeId", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AttributeValue_LanguageId",
                table: "AttributeValue",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Award_CountryId",
                table: "Award",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Award_EntityId",
                table: "Award",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Award_Slug",
                table: "Award",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AwardAssignment_AwardId",
                table: "AwardAssignment",
                column: "AwardId");

            migrationBuilder.CreateIndex(
                name: "IX_AwardAssignment_AwardResultTypeId",
                table: "AwardAssignment",
                column: "AwardResultTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AwardAssignment_EntityTypeId_EntityId",
                table: "AwardAssignment",
                columns: new[] { "EntityTypeId", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AwardResultType_Code",
                table: "AwardResultType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certification_CountryId",
                table: "Certification",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Certification_EntityId",
                table: "Certification",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certification_Slug",
                table: "Certification",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CertificationAssignment_CertificationId",
                table: "CertificationAssignment",
                column: "CertificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationAssignment_EntityTypeId_EntityId",
                table: "CertificationAssignment",
                columns: new[] { "EntityTypeId", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chart_CountryId",
                table: "Chart",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Chart_EntityId",
                table: "Chart",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chart_Slug",
                table: "Chart",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartEntry_ChartId",
                table: "ChartEntry",
                column: "ChartId");

            migrationBuilder.CreateIndex(
                name: "IX_ChartEntry_EntityTypeId_EntityId",
                table: "ChartEntry",
                columns: new[] { "EntityTypeId", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChartEntry_Unique",
                table: "ChartEntry",
                columns: new[] { "ChartId", "Date", "EntityTypeId", "EntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Citation_EntityTypeId_EntityId",
                table: "Citation",
                columns: new[] { "EntityTypeId", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Citation_SourceId",
                table: "Citation",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_CompanyTypeId",
                table: "Company",
                column: "CompanyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_CountryId",
                table: "Company",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_EntityId",
                table: "Company",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_Slug",
                table: "Company",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRoleType_Code",
                table: "CompanyRoleType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyType_Code",
                table: "CompanyType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Country_Code",
                table: "Country",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CountryRoleType_Code",
                table: "CountryRoleType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Credit_CompanyId",
                table: "Credit",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Credit_CreditRoleId",
                table: "Credit",
                column: "CreditRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Credit_EntityTypeId_EntityId",
                table: "Credit",
                columns: new[] { "EntityTypeId", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Credit_InstrumentId",
                table: "Credit",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Credit_PersonId",
                table: "Credit",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Credit_RoleScopeTypeId",
                table: "Credit",
                column: "RoleScopeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditRole_Code",
                table: "CreditRole",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditRole_RoleScopeTypeId",
                table: "CreditRole",
                column: "RoleScopeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditRoleEntityType_CreditRoleId",
                table: "CreditRoleEntityType",
                column: "CreditRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditRoleEntityType_CreditRoleId_EntityTypeId",
                table: "CreditRoleEntityType",
                columns: new[] { "CreditRoleId", "EntityTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditRoleEntityType_EntityTypeId",
                table: "CreditRoleEntityType",
                column: "EntityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Entity_EntityTypeId",
                table: "Entity",
                column: "EntityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Entity_Slug",
                table: "Entity",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_EntityLink_EntityTypeId_EntityId",
                table: "EntityLink",
                columns: new[] { "EntityTypeId", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityLink_LinkTypeId",
                table: "EntityLink",
                column: "LinkTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityType_Code",
                table: "EntityType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventType_Code",
                table: "EventType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genre_EntityId",
                table: "Genre",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genre_ParentGenreId",
                table: "Genre",
                column: "ParentGenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Genre_Slug",
                table: "Genre",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentifierType_Code",
                table: "IdentifierType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_CountryId",
                table: "Instrument",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_EntityId",
                table: "Instrument",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_InstrumentFamilyId",
                table: "Instrument",
                column: "InstrumentFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_Slug",
                table: "Instrument",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentFamily_Code",
                table: "InstrumentFamily",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Language_Code",
                table: "Language",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LinkType_Code",
                table: "LinkType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Localization_EntityTypeId_EntityId",
                table: "Localization",
                columns: new[] { "EntityTypeId", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Localization_LanguageId",
                table: "Localization",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Localization_Unique",
                table: "Localization",
                columns: new[] { "EntityTypeId", "EntityId", "LanguageId", "FieldName" },
                unique: true,
                filter: "[LanguageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Location_CountryId",
                table: "Location",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_EntityId",
                table: "Location",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_LocationTypeId",
                table: "Location",
                column: "LocationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_ParentLocationId",
                table: "Location",
                column: "ParentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_Slug",
                table: "Location",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationType_Code",
                table: "LocationType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LyricsAvailabilityType_Code",
                table: "LyricsAvailabilityType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_MediaTypeId",
                table: "Media",
                column: "MediaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssignment_EntityTypeId_EntityId",
                table: "MediaAssignment",
                columns: new[] { "EntityTypeId", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssignment_MediaId",
                table: "MediaAssignment",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssignment_MediaRoleTypeId",
                table: "MediaAssignment",
                column: "MediaRoleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaRoleType_Code",
                table: "MediaRoleType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaType_Code",
                table: "MediaType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mood_EntityId",
                table: "Mood",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mood_Slug",
                table: "Mood",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusicalKey_Code",
                table: "MusicalKey",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusicianInstrument_InstrumentId",
                table: "MusicianInstrument",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicianInstrument_PersonId",
                table: "MusicianInstrument",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicianInstrument_Unique",
                table: "MusicianInstrument",
                columns: new[] { "PersonId", "InstrumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceEvent_EntityId",
                table: "PerformanceEvent",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceEvent_EventTypeId",
                table: "PerformanceEvent",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceEvent_LocationId",
                table: "PerformanceEvent",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceEvent_Slug",
                table: "PerformanceEvent",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceEventAlbum_AlbumId",
                table: "PerformanceEventAlbum",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceEventAlbum_PerformanceEventId",
                table: "PerformanceEventAlbum",
                column: "PerformanceEventId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceEventAlbum_Unique",
                table: "PerformanceEventAlbum",
                columns: new[] { "PerformanceEventId", "AlbumId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceEventTrack_PerformanceEventId",
                table: "PerformanceEventTrack",
                column: "PerformanceEventId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceEventTrack_TrackId",
                table: "PerformanceEventTrack",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceEventTrack_Unique",
                table: "PerformanceEventTrack",
                columns: new[] { "PerformanceEventId", "TrackId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Person_BirthLocationId",
                table: "Person",
                column: "BirthLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_DeathLocationId",
                table: "Person",
                column: "DeathLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_EntityId",
                table: "Person",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Person_ImageMediaId",
                table: "Person",
                column: "ImageMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_NationalityCountryId",
                table: "Person",
                column: "NationalityCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_PersonKindId",
                table: "Person",
                column: "PersonKindId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_Slug",
                table: "Person",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonKind_Code",
                table: "PersonKind",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonType_Code",
                table: "PersonType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonTypeAssignment_PersonId",
                table: "PersonTypeAssignment",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonTypeAssignment_PersonTypeId",
                table: "PersonTypeAssignment",
                column: "PersonTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonTypeAssignment_Unique",
                table: "PersonTypeAssignment",
                columns: new[] { "PersonId", "PersonTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Poem_EntityId",
                table: "Poem",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Poem_PersonId",
                table: "Poem",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Poem_PublicationId",
                table: "Poem",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Poem_Slug",
                table: "Poem",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Publication_EntityId",
                table: "Publication",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Publication_PersonId",
                table: "Publication",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Publication_PublicationTypeId",
                table: "Publication",
                column: "PublicationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Publication_PublisherId",
                table: "Publication",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_Publication_Slug",
                table: "Publication",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicationType_Code",
                table: "PublicationType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSession_EntityId",
                table: "RecordingSession",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSession_LocationId",
                table: "RecordingSession",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSession_SessionTypeId",
                table: "RecordingSession",
                column: "SessionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSession_Slug",
                table: "RecordingSession",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSessionAlbum_AlbumId",
                table: "RecordingSessionAlbum",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSessionAlbum_RecordingSessionId",
                table: "RecordingSessionAlbum",
                column: "RecordingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSessionAlbum_Unique",
                table: "RecordingSessionAlbum",
                columns: new[] { "RecordingSessionId", "AlbumId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSessionTrack_RecordingSessionId",
                table: "RecordingSessionTrack",
                column: "RecordingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSessionTrack_TrackId",
                table: "RecordingSessionTrack",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSessionTrack_Unique",
                table: "RecordingSessionTrack",
                columns: new[] { "RecordingSessionId", "TrackId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleScopeType_Code",
                table: "RoleScopeType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionType_Code",
                table: "SessionType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Source_EntityId",
                table: "Source",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Source_PublisherId",
                table: "Source",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_Source_Slug",
                table: "Source",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Source_SourceTypeId",
                table: "Source",
                column: "SourceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SourceType_Code",
                table: "SourceType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SungVersion_EntityId",
                table: "SungVersion",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SungVersion_PoemId",
                table: "SungVersion",
                column: "PoemId");

            migrationBuilder.CreateIndex(
                name: "IX_SungVersion_Slug",
                table: "SungVersion",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SungVersion_VocalStyleId",
                table: "SungVersion",
                column: "VocalStyleId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_EntityId",
                table: "Tag",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tag_Slug",
                table: "Tag",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagAssignment_EntityTypeId_EntityId",
                table: "TagAssignment",
                columns: new[] { "EntityTypeId", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_TagAssignment_TagId",
                table: "TagAssignment",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_TagAssignment_Unique",
                table: "TagAssignment",
                columns: new[] { "EntityTypeId", "EntityId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Track_EntityId",
                table: "Track",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Track_LyricsAvailabilityTypeId",
                table: "Track",
                column: "LyricsAvailabilityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Track_MusicalKeyId",
                table: "Track",
                column: "MusicalKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_Track_Slug",
                table: "Track",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Track_VocalStyleId",
                table: "Track",
                column: "VocalStyleId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackGenre_GenreId",
                table: "TrackGenre",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackGenre_TrackId",
                table: "TrackGenre",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackGenre_Unique",
                table: "TrackGenre",
                columns: new[] { "TrackId", "GenreId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackInstrument_InstrumentId",
                table: "TrackInstrument",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackInstrument_TrackId",
                table: "TrackInstrument",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackInstrument_Unique",
                table: "TrackInstrument",
                columns: new[] { "TrackId", "InstrumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackMood_MoodId",
                table: "TrackMood",
                column: "MoodId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackMood_TrackId",
                table: "TrackMood",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackMood_Unique",
                table: "TrackMood",
                columns: new[] { "TrackId", "MoodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackRelation_RelatedTrackId",
                table: "TrackRelation",
                column: "RelatedTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackRelation_TrackId",
                table: "TrackRelation",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackRelation_TrackRelationTypeId",
                table: "TrackRelation",
                column: "TrackRelationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackRelation_Unique",
                table: "TrackRelation",
                columns: new[] { "TrackId", "RelatedTrackId", "TrackRelationTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackRelationType_Code",
                table: "TrackRelationType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackSungVersion_SungVersionId",
                table: "TrackSungVersion",
                column: "SungVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackSungVersion_TrackId",
                table: "TrackSungVersion",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackSungVersion_Unique",
                table: "TrackSungVersion",
                columns: new[] { "TrackId", "SungVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackVersionType_Code",
                table: "TrackVersionType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackVersionTypeAssignment_TrackId",
                table: "TrackVersionTypeAssignment",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackVersionTypeAssignment_TrackVersionTypeId",
                table: "TrackVersionTypeAssignment",
                column: "TrackVersionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackVersionTypeAssignment_Unique",
                table: "TrackVersionTypeAssignment",
                columns: new[] { "TrackId", "TrackVersionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VocalStyle_Code",
                table: "VocalStyle",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumCompany");

            migrationBuilder.DropTable(
                name: "AlbumCountry");

            migrationBuilder.DropTable(
                name: "AlbumGenre");

            migrationBuilder.DropTable(
                name: "AlbumIdentifier");

            migrationBuilder.DropTable(
                name: "AlbumLanguage");

            migrationBuilder.DropTable(
                name: "AlbumMood");

            migrationBuilder.DropTable(
                name: "AlbumRelation");

            migrationBuilder.DropTable(
                name: "AlbumTrack");

            migrationBuilder.DropTable(
                name: "Alias");

            migrationBuilder.DropTable(
                name: "AttributeValue");

            migrationBuilder.DropTable(
                name: "AwardAssignment");

            migrationBuilder.DropTable(
                name: "CertificationAssignment");

            migrationBuilder.DropTable(
                name: "ChartEntry");

            migrationBuilder.DropTable(
                name: "Citation");

            migrationBuilder.DropTable(
                name: "Credit");

            migrationBuilder.DropTable(
                name: "CreditRoleEntityType");

            migrationBuilder.DropTable(
                name: "EntityLink");

            migrationBuilder.DropTable(
                name: "Localization");

            migrationBuilder.DropTable(
                name: "MediaAssignment");

            migrationBuilder.DropTable(
                name: "MusicianInstrument");

            migrationBuilder.DropTable(
                name: "PerformanceEventAlbum");

            migrationBuilder.DropTable(
                name: "PerformanceEventTrack");

            migrationBuilder.DropTable(
                name: "PersonTypeAssignment");

            migrationBuilder.DropTable(
                name: "RecordingSessionAlbum");

            migrationBuilder.DropTable(
                name: "RecordingSessionTrack");

            migrationBuilder.DropTable(
                name: "TagAssignment");

            migrationBuilder.DropTable(
                name: "TrackGenre");

            migrationBuilder.DropTable(
                name: "TrackInstrument");

            migrationBuilder.DropTable(
                name: "TrackMood");

            migrationBuilder.DropTable(
                name: "TrackRelation");

            migrationBuilder.DropTable(
                name: "TrackSungVersion");

            migrationBuilder.DropTable(
                name: "TrackVersionTypeAssignment");

            migrationBuilder.DropTable(
                name: "CompanyRoleType");

            migrationBuilder.DropTable(
                name: "CountryRoleType");

            migrationBuilder.DropTable(
                name: "IdentifierType");

            migrationBuilder.DropTable(
                name: "AlbumRelationType");

            migrationBuilder.DropTable(
                name: "AliasType");

            migrationBuilder.DropTable(
                name: "AttributeDefinition");

            migrationBuilder.DropTable(
                name: "AwardResultType");

            migrationBuilder.DropTable(
                name: "Award");

            migrationBuilder.DropTable(
                name: "Certification");

            migrationBuilder.DropTable(
                name: "Chart");

            migrationBuilder.DropTable(
                name: "Source");

            migrationBuilder.DropTable(
                name: "CreditRole");

            migrationBuilder.DropTable(
                name: "LinkType");

            migrationBuilder.DropTable(
                name: "Language");

            migrationBuilder.DropTable(
                name: "MediaRoleType");

            migrationBuilder.DropTable(
                name: "PerformanceEvent");

            migrationBuilder.DropTable(
                name: "PersonType");

            migrationBuilder.DropTable(
                name: "Album");

            migrationBuilder.DropTable(
                name: "RecordingSession");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "Genre");

            migrationBuilder.DropTable(
                name: "Instrument");

            migrationBuilder.DropTable(
                name: "Mood");

            migrationBuilder.DropTable(
                name: "TrackRelationType");

            migrationBuilder.DropTable(
                name: "SungVersion");

            migrationBuilder.DropTable(
                name: "TrackVersionType");

            migrationBuilder.DropTable(
                name: "Track");

            migrationBuilder.DropTable(
                name: "SourceType");

            migrationBuilder.DropTable(
                name: "RoleScopeType");

            migrationBuilder.DropTable(
                name: "EventType");

            migrationBuilder.DropTable(
                name: "AlbumCategory");

            migrationBuilder.DropTable(
                name: "SessionType");

            migrationBuilder.DropTable(
                name: "InstrumentFamily");

            migrationBuilder.DropTable(
                name: "Poem");

            migrationBuilder.DropTable(
                name: "LyricsAvailabilityType");

            migrationBuilder.DropTable(
                name: "MusicalKey");

            migrationBuilder.DropTable(
                name: "VocalStyle");

            migrationBuilder.DropTable(
                name: "Publication");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropTable(
                name: "PublicationType");

            migrationBuilder.DropTable(
                name: "CompanyType");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropTable(
                name: "PersonKind");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropTable(
                name: "Entity");

            migrationBuilder.DropTable(
                name: "LocationType");

            migrationBuilder.DropTable(
                name: "MediaType");

            migrationBuilder.DropTable(
                name: "EntityType");
        }
    }
}
