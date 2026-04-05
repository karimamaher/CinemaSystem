using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InsertDataToCinemasModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into Cinemas (Name, Logo , Status) values ('Galaxy Cinema', 'cinema1.jpg', 0);  insert into Cinemas (Name, Logo , Status) values ('Royal Cinema', 'cinema2.jpg', 1);  insert into Cinemas (Name, Logo , Status) values ('Grand Cinema', 'cinema3.jpg', 1);  insert into Cinemas (Name, Logo , Status) values ('Star Cinema', 'cinema4.jpg', 1);  insert into Cinemas (Name, Logo , Status) values ('Sunset Cinema', 'cinema5.jpg', 0);  ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Truncate TABLE Cinemas");
        }
    }
}
