using System;

namespace HanwhaClient.Model.Dto
{
    public class ExposeApiUserDto
    {
        public string? Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
