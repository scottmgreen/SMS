using System;
using System.Text.RegularExpressions;

namespace CBT3_Domain.ValueObjects
{
    /// <summary>
    /// Represents the first name value object.
    /// </summary>
    using System.Collections.Generic;

    public sealed class SubscribeToTextNewsletter : BaseValueObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscribeToTextNewsletter"/> class.
        /// </summary>
        /// <param name="value">The SMS Notification value.</param>
        private SubscribeToTextNewsletter(bool value) => Value = value;

        /// <summary>
        /// Gets the SMS notification value.
        /// </summary>
        public bool Value { get; }

        public static implicit operator bool(SubscribeToTextNewsletter smsNotification) => smsNotification.Value;

        /// <summary>
        /// Creates a new <see cref="SMSNotification"/> instance based on the specified value.
        /// </summary>
        /// <param name="smsNotification">The SMS notification value.</param>
        /// <returns>The result of the SMS notification creation process containing the SMS notification or an error.</returns>
        //public static Result<SMSNotification> Create(bool smsNotification) =>
        //    Result.Create(smsNotification, DomainErrors.SMSNotificationError.InValid)
        //        .Map(f => new SMSNotification(f));

        public static Result<SubscribeToTextNewsletter> Create(bool? smsNotification)
        {
            // Default to false if smsNotification is null
            var value = smsNotification ?? false;

            return Result.Success(new SubscribeToTextNewsletter(value));
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
