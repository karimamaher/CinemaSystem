using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InsertDataToActorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into Actors (name, Img, MovieId ) values ('Andie Bickerton', 'Actor1.jpg', 1);    insert into Actors (name, Img, MovieId ) values ('Beryl Lumber', 'Actor2.jpg', 3);    insert into Actors (name, Img, MovieId ) values ('Kermy Rottgers', 'Actor3.jpg', 2);    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Truncate TABLE Actors");
        }
    }
}
