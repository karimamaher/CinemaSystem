using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InsertDataToCategoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into Categories (Name, Status) values ('Action', 1); insert into Categories (Name, Status) values ('Comedy', 0); insert into Categories (Name, Status) values ('Horror', 1); insert into Categories (Name, Status) values ('Romance', 1); insert into Categories (Name, Status) values ('Kids', 0); insert into Categories (Name, Status) values ('History', 1); insert into Categories (Name, Status) values ('Action', 1); insert into Categories (Name, Status) values ('Comedy', 0); ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Truncate TABLE Categories");

        }
    }
}
