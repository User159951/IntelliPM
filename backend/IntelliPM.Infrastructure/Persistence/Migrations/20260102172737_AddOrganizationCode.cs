using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliPM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Keep Name with maxLength 200 as per OrganizationConfiguration
            // No need to alter if it's already correct

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Organizations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Set Code for existing organizations based on Name (slugify)
            // For "Default Organization", set code to "default"
            // For others, create a slug from the name
            migrationBuilder.Sql(@"
                UPDATE Organizations 
                SET Code = CASE 
                    WHEN Name = 'Default Organization' THEN 'default'
                    ELSE LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                        Name, ' ', '-'), '.', ''), ',', ''), '(', ''), ')', ''), '[', ''), ']', ''), '{', ''), '}', ''), '''', ''))
                END
                WHERE Code = '' OR Code IS NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Code",
                table: "Organizations",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organizations_Code",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Organizations");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organizations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name");
        }
    }
}
