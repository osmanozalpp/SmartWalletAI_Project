using MediatR;
using SmartWalletAI.Application.Features.Auth.Queries.GetLastFailedLogin;
using System.Text.Json.Serialization;

public class GetLastFailedLoginQuery : IRequest<LastFailedLoginDto>
{
    [JsonIgnore]
    public Guid UserId { get; set; }
}