namespace InventoryPlatform.Application.Interfaces.Communication;

public interface IEmailService
{
    Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}