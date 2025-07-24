using System;

namespace CBT3_Domain.ValueObjects
{
    public sealed class SubscribeToEmailNewsletter : BaseValueObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscribeToEmailNewsletter"/> class.
        /// </summary>
        /// <param name="value">The Email Notification value.</param>
        private SubscribeToEmailNewsletter(bool value) => Value = value;

        /// <summary>
        /// Gets the Email notification value.
        /// </summary>
        public bool Value { get; }

        public static implicit operator bool(SubscribeToEmailNewsletter emailNotification) => emailNotification.Value;

        /// <summary>
        /// Creates a new <see cref="EmailNotification"/> instance based on the specified value.
        /// </summary>
        /// <param name="smsNotification">The Email notification value.</param>
        /// <returns>The result of the Email notification creation process containing the Email notification or an error.</returns>
        //public static Result<EmailNotification> Create(bool emailNotification) =>
        //    Result.Create(emailNotification, DomainErrors.EmailNotificationError.InValid)
        //        .Map(f => new EmailNotification(f));

        public static Result<SubscribeToEmailNewsletter> Create(bool? emailNotification)
        {
            // Default to false if smsNotification is null
            var value = emailNotification ?? false;

            return Result.Success(new SubscribeToEmailNewsletter(value));
        }



        /// <inheritdoc />
        public override string ToString() => Value.ToString();

        /// <inheritdoc />
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }
    }
}
