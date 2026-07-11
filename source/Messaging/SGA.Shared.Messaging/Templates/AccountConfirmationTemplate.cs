namespace SGA.Shared.Messaging.Templates;

public static class AccountConfirmationTemplate
{
    public static (string Subject, string HtmlBody) Build(string recipientFirstName, string confirmationLink) => (
        "Confirm your SGA-ITLA account",
        $"""
         <p>Hello {recipientFirstName},</p>
         <p>Thanks for registering with the ITLA Bus Transport System. Please confirm your account
         by clicking the link below:</p>
         <p><a href="{confirmationLink}">Confirm my account</a></p>
         <p>If you did not request this account, you can safely ignore this email.</p>
         """);
}
