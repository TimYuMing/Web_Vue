using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web_Vue.Server.Models.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleInUserAccounts",
                columns: table => new
                {
                    UserAccountID = table.Column<int>(type: "integer", nullable: false, comment: "使用者ID"),
                    RoleID = table.Column<int>(type: "integer", nullable: false, comment: "角色ID")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleInUserAccounts", x => new { x.UserAccountID, x.RoleID });
                },
                comment: "角色使用者關聯表(多對多)");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false, comment: "主鍵")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "角色名稱"),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "角色代碼"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "備註"),
                    IsEnable = table.Column<bool>(type: "boolean", nullable: false, comment: "啟用狀態"),
                    IsMaintaining = table.Column<bool>(type: "boolean", nullable: false, comment: "是否系統維護中"),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false, comment: "是否為系統管理者"),
                    CreateUser = table.Column<string>(type: "text", nullable: true, comment: "建立人員"),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "建立時間"),
                    UpdateUser = table.Column<string>(type: "text", nullable: true, comment: "更新人員"),
                    UpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "更新時間"),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, comment: "是否已刪除")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.ID);
                },
                comment: "角色資料表");

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false, comment: "主鍵")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Account = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false, comment: "帳號"),
                    Password = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false, comment: "密碼"),
                    LatestLoginDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "最後登入日期"),
                    ValidPasswordExpireTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "驗證密碼有效期限"),
                    IsTemplatePassword = table.Column<bool>(type: "boolean", nullable: false, comment: "為暫時密碼")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.ID);
                },
                comment: "使用者帳號");

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "姓名"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "是否啟用 (1:啟用, 2:停用)"),
                    CreateUser = table.Column<string>(type: "text", nullable: true, comment: "建立人員"),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "建立時間"),
                    UpdateUser = table.Column<string>(type: "text", nullable: true, comment: "更新人員"),
                    UpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "更新時間"),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, comment: "是否已刪除")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.ID);
                },
                comment: "使用者資訊資料表");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleInUserAccounts");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "UserAccounts");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}

