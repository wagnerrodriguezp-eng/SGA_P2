namespace SGA.Shared.Messaging.Templates;

public static class PasswordResetTemplate
{
    public static (string Subject, string HtmlBody) Build(string recipientFirstName, string resetLink) => (
        "Reset your SGA-ITLA password",
        $"""
         <p>Hello {recipientFirstName},</p>
         <p>We received a request to reset your password. Click the link below to choose a new one:</p>
         <p><a href="{resetLink}">Reset my password</a></p>
         <p>If you did not request this, you can safely ignore this email — your password will not change.</p>
         """);
}
