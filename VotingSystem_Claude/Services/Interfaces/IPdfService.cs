namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerateVotingReceipt(string voterName, string electionTitle, string studentId, string classInfo);
    }
}
