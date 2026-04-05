using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InsertDataToMoviesModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into Movies (name, description, price, status, dateTime, mainImg, categoryId, cinemaId) values ('Interstellar', 'Fusce posuere felis sed lacus. Morbi sem mauris, laoreet ut, rhoncus aliquet, pulvinar sed, nisl. Nunc rhoncus dui vel sem.', 27685, 1, '3/18/2026', '1.jpg', 3, 3);   insert into Movies (name, description, price, status, dateTime, mainImg, categoryId, cinemaId) values ('Titanic', 'Quisque porta volutpat erat. Quisque erat eros, viverra eget, congue eget, semper rutrum, nulla. Nunc purus.', 28356, 1, '11/28/2027', '2.jpg', 1, 2);   insert into Movies (name, description, price, status, dateTime, mainImg, categoryId, cinemaId) values ('Avatar', 'Pellentesque at nulla. Suspendisse potenti. Cras in purus eu magna vulputate luctus.', 46918, 0, '7/12/2026', '3.jpg', 1, 5);   insert into Movies (name, description, price, status, dateTime, mainImg, categoryId, cinemaId) values ('The Matrix', 'Phasellus sit amet erat. Nulla tempus. Vivamus in felis eu sapien cursus vestibulum.', 47208, 1, '8/13/2025', '4.jpg', 2, 4);   insert into Movies (name, description, price, status, dateTime, mainImg, categoryId, cinemaId) values ('Joker', 'Suspendisse potenti. In eleifend quam a odio. In hac habitasse platea dictumst.', 37017, 1, '2/9/2027', '5.jpg', 1, 2);   ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Truncate TABLE Movies");
        }
    }
}
