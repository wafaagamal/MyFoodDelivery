using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using OrderingSvc.Application.Contracts.Payments.Dtos;

namespace OrderingSvc.Application.Contracts.Payments;

public interface IPaymentAppService : IApplicationService
{
    Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentIntentDto input);
    Task<PaymentResultDto> ConfirmPaymentAsync(ConfirmPaymentDto input);
    Task<RefundResultDto> ProcessRefundAsync(ProcessRefundDto input);
    Task HandleWebhookAsync(string payload, string signature);
}
