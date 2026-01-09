using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using FluentValidation.Validators;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace Whisparr.Api.V3.Config
{
    public static class CertificateValidation
    {
        public static IRuleBuilderOptions<T, string> IsValidCertificate<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new CertificateValidator());
        }
    }

    public class CertificateValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Invalid SSL certificate file or password. {message}";

        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(CertificateValidator));

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            if (context.InstanceToValidate is not HostConfigResource resource)
            {
                return true;
            }

            try
            {
                var certBytes = File.ReadAllBytes(resource.SslCertPath);

                // Use SecureString for password
                SecureString securePassword = null;
                if (!string.IsNullOrEmpty(resource.SslCertPassword))
                {
                    securePassword = new SecureString();
                    foreach (var c in resource.SslCertPassword)
                    {
                        securePassword.AppendChar(c);
                    }

                    securePassword.MakeReadOnly();
                }

                #pragma warning disable SYSLIB0057 // Suppress obsolete warning for X509Certificate2 constructor
                var certificate = new X509Certificate2(
                    certBytes,
                    securePassword,
                    X509KeyStorageFlags.DefaultKeySet);
                #pragma warning restore SYSLIB0057
                return true;
            }
            catch (CryptographicException ex)
            {
                Logger.Debug(ex, "Invalid SSL certificate file or password. {0}", ex.Message);
                context.MessageFormatter.AppendArgument("message", ex.Message);
                return false;
            }
        }
    }
}
