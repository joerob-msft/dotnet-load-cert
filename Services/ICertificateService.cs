using AzureCertInventory.Models;

namespace AzureCertInventory.Services
{
    public interface ICertificateService
    {
        IEnumerable<CertificateInfo> GetPublicCertificates();
        IEnumerable<CertificateInfo> GetPrivateCertificates();
    }
}