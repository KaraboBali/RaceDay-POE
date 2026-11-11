using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Please enter your name."), StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address."),
     EmailAddress(ErrorMessage = "That email address does not look right.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a password."),
     MinLength(6, ErrorMessage = "Your password must be at least 6 characters."),
     DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please repeat your password."),
     Compare(nameof(Password), ErrorMessage = "The two passwords do not match."),
     DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose how you will use RaceDay.")]
    public string Role { get; set; } = "Participant";
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Please enter your email address."),
     EmailAddress(ErrorMessage = "That email address does not look right.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password."), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}