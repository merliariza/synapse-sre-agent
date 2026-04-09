public interface INotificationService
{
    Task SendTeamAlertAsync(string incidentTitle, int severity, Guid incidentId, string aiSummary);
    Task SendReporterNotificationAsync(string reporterEmail, string incidentTitle, Guid incidentId);
}