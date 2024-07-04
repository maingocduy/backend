using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs.Account
{
    public class UpdatePasswordRequestDTO
    {
        private string? _password;

        public string OldPassword {  get; set; }
        public string? Password
        {
            get => _password;
            set => _password = replaceEmptyWithNull(value);
        }
        public string username { get; set; }

        // helpers

        private string? replaceEmptyWithNull(string? value)
        {
            // replace empty string with null to make field optional
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
