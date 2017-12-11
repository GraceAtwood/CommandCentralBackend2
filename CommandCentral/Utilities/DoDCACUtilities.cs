using System.Security.Cryptography.X509Certificates;

namespace CommandCentral.Utilities
{
    public static class DoDCACUtilities
    {
        public static string GetDoDIdFromCAC(this X509Certificate2 cert)
        {
            var temp = cert.Subject.Substring(0, cert.Subject.Length - cert.Subject.IndexOf(','));
            return temp.Substring(temp.LastIndexOf('.') + 1, temp.Length - temp.IndexOf(',') - 1);
        }
    }
}