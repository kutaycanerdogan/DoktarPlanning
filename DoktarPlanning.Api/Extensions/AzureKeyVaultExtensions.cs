using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace DoktarPlanning.Infrastructure.Extensions
{
    public static class AzureKeyVaultExtensions
    {
        public static IConfigurationBuilder AddAzureKeyVaultIfConfigured(this IConfigurationBuilder configurationBuilder, IHostEnvironment env)
        {
            var tempConfig = configurationBuilder.Build();
            var vaultUri = tempConfig["KeyVault:VaultUri"];
            if (string.IsNullOrWhiteSpace(vaultUri))
                return configurationBuilder;

            var credential = new DefaultAzureCredential();

            var client = new SecretClient(new Uri(vaultUri), credential);
            configurationBuilder.AddAzureKeyVault(client, new KeyVaultSecretManager());

            return configurationBuilder;
        }
    }
}