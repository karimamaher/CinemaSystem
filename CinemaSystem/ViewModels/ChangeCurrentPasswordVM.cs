using System.ComponentModel.DataAnnotations;
namespace CinemaSystem.ViewModels
{
    public class ChangeCurrentPasswordVM
    {
     
        [Required, DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }
        
        [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; }
    }
}
