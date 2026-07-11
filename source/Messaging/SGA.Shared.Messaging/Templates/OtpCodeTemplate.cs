namespace SGA.Shared.Messaging.Templates;

public static class OtpCodeTemplate
{
    public static (string Subject, string HtmlBody) Build(string recipientFirstName, string otpCode) => (
        "Your SGA-ITLA verification code",
        $"""
         <p>Hello {recipientFirstName},</p>
         <p>Your one-time verification code is:</p>
         <p style="font-size:1.5em;font-weight:bold;letter-spacing:0.2em;">{otpCode}</p>
         <p>This code expires shortly. If you did not request it, you can safely ignore this email.</p>
         """);
}
