

namespace CBT3_Application.Messaging.Commands;

public class SubmitSubscribeToOperationalTextsCommand : BaseCommandBundle, IRequest<Result<SubscribeToOperationalTexts>>
{
    public SubscribeToOperationalTexts SubscribeToOperationalTextsOption { get; set; }


}