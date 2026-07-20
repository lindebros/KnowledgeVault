using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KnowledgeVault.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTagIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add a new Guid column to Tags and populate it
            migrationBuilder.AddColumn<Guid>(
                name: "NewId",
                table: "Tags",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            // Ensure existing rows have a Guid value
            migrationBuilder.Sql("UPDATE \"Tags\" SET \"NewId\" = gen_random_uuid() WHERE \"NewId\" IS NULL;");

            // Add temporary Guid column to NoteTags to hold migrated TagId values
            migrationBuilder.AddColumn<Guid>(
                name: "NewTagId",
                table: "NoteTags",
                type: "uuid",
                nullable: true);

            // Populate NewTagId by joining with Tags.NewId
            migrationBuilder.Sql(
                "UPDATE \"NoteTags\" nt SET \"NewTagId\" = t.\"NewId\" FROM \"Tags\" t WHERE t.\"Id\" = nt.\"TagId\";");

            // Drop foreign key from NoteTags to old Tags.Id
            migrationBuilder.DropForeignKey(
                name: "FK_NoteTags_Tags_TagId",
                table: "NoteTags");

            // Drop old primary keys to allow column changes
            migrationBuilder.DropPrimaryKey(
                name: "PK_NoteTags",
                table: "NoteTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            // Drop index on TagId
            migrationBuilder.DropIndex(
                name: "IX_NoteTags_TagId",
                table: "NoteTags");

            // Remove old integer TagId column and replace with migrated Guid column
            migrationBuilder.DropColumn(
                name: "TagId",
                table: "NoteTags");

            migrationBuilder.RenameColumn(
                name: "NewTagId",
                table: "NoteTags",
                newName: "TagId");

            // Make migrated TagId non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "TagId",
                table: "NoteTags",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            // Recreate primary key for NoteTags on (NoteId, TagId)
            migrationBuilder.AddPrimaryKey(
                name: "PK_NoteTags",
                table: "NoteTags",
                columns: new[] { "NoteId", "TagId" });

            // Replace Tags primary key: drop old Id column and make NewId the primary key
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Tags");

            migrationBuilder.RenameColumn(
                name: "NewId",
                table: "Tags",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "Id");

            // Recreate index and foreign key from NoteTags.TagId to Tags.Id
            migrationBuilder.CreateIndex(
                name: "IX_NoteTags_TagId",
                table: "NoteTags",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteTags_Tags_TagId",
                table: "NoteTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new InvalidOperationException("Downgrading this migration is not supported because it changes Tag primary key type and migrating back may cause data loss.");
        }
    }
}
