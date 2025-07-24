
namespace CBT3_Application.Messaging.Commands;

public class SubmitSubscribeToEmailNewsletterCommand : BaseCommandBundle, IRequest<Result<SubscribeToEmailNewsletter>>
{
    public SubscribeToEmailNewsletter SubscribeToEmailNewsletterOption { get; set; }


}