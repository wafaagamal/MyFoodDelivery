using System.Threading.Tasks;
using AuthSvc.Application.Contracts.Account.Dtos;

namespace AuthSvc.Application.Contracts.Account;

public interface IAccountAppService
{
    Task<RegisterResultDto> RegisterAsync(RegisterRequestDto input);
}
