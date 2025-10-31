namespace Application.DTOs.RentalManagement
{
    public class ESignPayloadDto
    {
        public string? SignerIp { get; set; }
        public string? UserAgent { get; set; }
        public string? ProviderSignatureId { get; set; }
        public string? SignatureImageUrl { get; set; }
        public string? CertSubject { get; set; }
        public string? CertIssuer { get; set; }
        public string? CertSerial { get; set; }
        public string? CertFingerprintSha256 { get; set; }
        public string? SignatureHash { get; set; }
        public string? EvidenceUrl { get; set; }
    }
}