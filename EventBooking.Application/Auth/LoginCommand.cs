using MediatR;

namespace EventBooking.Application.Auth
{
    public record LoginCommand(string UserName, string Password) : IRequest<LoginResponse>;
}
