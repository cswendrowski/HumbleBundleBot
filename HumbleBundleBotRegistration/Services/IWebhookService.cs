using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HumbleBundleBotRegistration.Models;

namespace HumbleBundleBotRegistration.Services
{
    public interface IWebhookService
    {

        Task<bool> RegisterWebhook(RegistrationWebhook registrationWebhook);

        Task<bool> DeregisterWebhook(DeregistrationWebhook deregistrationWebhook);

    }
}
