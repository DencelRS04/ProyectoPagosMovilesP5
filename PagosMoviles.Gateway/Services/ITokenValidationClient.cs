namespace PagosMoviles.Gateway.Services
{
    public interface ITokenValidationClient
    {
        Task<bool> ValidateAsync(string token, CancellationToken cancellationToken = default);
    }
}