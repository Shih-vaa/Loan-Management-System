public class MemberLeadStatsViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string Role { get; set; }
    public int LeadsGenerated { get; set; }
    public int LeadsAssigned { get; set; }
    public int LeadsApproved { get; set; }
    public int DocumentsUploaded { get; set; }
    public int DocumentsVerified { get; set; }
}

public class TeamDetailsViewModel
{
    public string TeamName { get; set; }
    public List<MemberLeadStatsViewModel> Members { get; set; }
}
