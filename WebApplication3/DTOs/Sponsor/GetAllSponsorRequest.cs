namespace WebApplication3.DTOs.Sponsor
{
    public class GetAllSponsorRequest
    {
        public int? projectId {get;set;}
        public int pageNumber { get; set; } = 1;
    }
}
