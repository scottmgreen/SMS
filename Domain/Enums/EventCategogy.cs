using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_Domain.Enums;

public abstract class EventCategory : BaseEnum<EventCategory>
{
    protected EventCategory(int id, string value, string name) : base(value, name)
    {
        Id = id;
    }

    public int Id { get; }

    public static EventCategory? FromId(int id)
    {
        return id switch
        {
            0 => DomainEvent,
            1 => IntegrationEvent,
            2 => UIEvent,
            _ => null
        };
    }

    public static explicit operator int(EventCategory category)
    {
        return category.Id;
    }

    public static explicit operator EventCategory(int id)
    {
        return FromId(id) ?? throw new InvalidCastException($"Unknown EventCategory id '{id}'.");
    }


    // Leading Indicators (Proactive)
    public static readonly EventCategory DomainEvent = new DomainEventCategory();
    public static readonly EventCategory IntegrationEvent = new IntegrationEventCategory();
    public static readonly EventCategory UIEvent = new UIEventCategory();
    
    

   

    // Leading Indicators
    private sealed class DomainEventCategory : EventCategory                
    {
        public DomainEventCategory() : base(0, "DOMAIN_EVENT", "DOMAIN_EVENT")
        { }
    }

    private sealed class IntegrationEventCategory : EventCategory
    {
        public IntegrationEventCategory() : base(1, "INTEGRATION_EVENT", "INTEGRATION_EVENT")
        { }
    }

    private sealed class UIEventCategory : EventCategory
    {
        public UIEventCategory() : base(2, "UI_EVENT", "UI_EVENT")
        { }
    }

   
}
