

namespace CBT3_Application.Messaging.Commands;

public class SubmitSubscribeToTextNewsletterCommand : BaseCommandBundle, IRequest<Result<SubscribeToTextNewsletter>>
{
    public SubscribeToTextNewsletter SubscribeToTextNewsletterOption { get; set; }


}
