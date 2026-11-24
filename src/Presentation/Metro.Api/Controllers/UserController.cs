using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Metro.Application.Users.Commands;
using Metro.Application.Users.Queries;
using Metro.Application.Users.ViewModels;
using Metro.Application.Results;
using Metro.Domain.Users;

namespace Metro.Api.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [ApiExplorerSettings(GroupName = "ApiV1")]
    public class UserController : ApiController
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("me")]
        [Authorize("Authenticated")]
        public async Task<ObjectResult> Me()
        {
            var idValue = User.FindFirstValue(ClaimTypes.Name);
            if (!Guid.TryParse(idValue, out var id))
            {
                return FromResult(ApiResult<GetUserViewModel>.Unauthorized());
            }

            return FromResult(await _mediator.Send(new GetUserQuery
            {
                Id = id
            }));
        }

        [HttpGet]
        [Authorize("Staff")]
        public async Task<ObjectResult> List([FromQuery] ListUserQuery query)
            => FromResult(await _mediator.Send(query));

        [HttpGet("{id}")]
        [Authorize("Staff")]
        public async Task<ObjectResult> Get(Guid id)
            => FromResult(await _mediator.Send(new GetUserQuery
            {
                Id = id
            }));

        [HttpPost]
        [EnableRateLimiting("create-user")]
        public async Task<ObjectResult> Create([FromBody] CreateUserCommand command)
        {
            if (!User.IsInRole(UserRoles.Staff))
            {
                command.Role = UserRoles.Passenger;
            }

            return FromResult(await _mediator.Send(command));
        }

        [HttpPut("{id}")]
        [Authorize("Staff")]
        public async Task<ObjectResult> Update(Guid id, [FromBody] UpdateUserCommand command)
        {
            command.Id = id;
            return FromResult(await _mediator.Send(command));
        }

        [HttpDelete("{id}")]
        [Authorize("Staff")]
        public async Task<ObjectResult> Delete(Guid id)
            => FromResult(await _mediator.Send(new DeleteUserCommand
            {
                Id = id
            }));
    }
}
