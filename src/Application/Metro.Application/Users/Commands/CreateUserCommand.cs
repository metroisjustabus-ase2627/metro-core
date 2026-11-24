using Metro.Application.Messaging;
using Metro.Application.Results;
using Metro.Domain.Users;

namespace Metro.Application.Users.Commands
{
    public class CreateUserCommand : ICommand<ApiResult<CreatedId>>
    {
        public string Name { get; set; } = string.Empty;

        required public string Email { get; set; }

        public string Document { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        required public string Password { get; set; }

        public string Role { get; set; } = UserRoles.Passenger;
    }
}
