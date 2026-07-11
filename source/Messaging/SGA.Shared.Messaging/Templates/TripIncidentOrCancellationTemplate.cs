namespace SGA.Shared.Messaging.Templates;

public static class TripIncidentOrCancellationTemplate
{
    public static (string Subject, string HtmlBody) BuildCancellation(string recipientFirstName, string tripDescription, string reason) => (
        "Your trip was cancelled",
        $"""
         <p>Hello {recipientFirstName},</p>
         <p>Unfortunately, the following trip has been cancelled:</p>
         <p><strong>{tripDescription}</strong></p>
         <p>Reason: {reason}</p>
         <p>Please check the schedule for the next available trip.</p>
         """);

    public static (string Subject, string HtmlBody) BuildIncidentNotice(string tripDescription, string incidentDescription) => (
        "Incident reported on a trip",
        $"""
         <p>An incident was reported on the following trip:</p>
         <p><strong>{tripDescription}</strong></p>
         <p>Details: {incidentDescription}</p>
         <p>Please review it in the administration console.</p>
         """);
}
