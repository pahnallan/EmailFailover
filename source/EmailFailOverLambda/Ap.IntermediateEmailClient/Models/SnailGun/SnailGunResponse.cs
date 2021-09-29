namespace Ap.IntermediateEmailService.Models.SnailGun
{
    public class SnailGunResponse
    {
        public string Id { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string ToEmail { get; set; }
        public string ToName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Status { get; set; }
        public string Created { get; set; }
    }
}
