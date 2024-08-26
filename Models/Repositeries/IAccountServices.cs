public interface IAccountService{
    Task<LoginResponse> Login(LoginDto loginDto);
    Task<Response> Register(User user);
}
public class AccountService(IAccountRepo account) : IAccountService
{
    public async Task<LoginResponse> Login(LoginDto loginDto)
    {
        return await account.Login(loginDto);
    }

    public async Task<Response> Register(User user)
    {
        return await account.Register(user);
    }
}
