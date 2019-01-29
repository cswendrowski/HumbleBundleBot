using System;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;

namespace HumbleBundleBotRegistration.Shared.Components
{
    public abstract class ImFormComponentBase<T> : BlazorComponent
    {

        [Parameter] protected string Id { get; set; }

        [Parameter] protected string Title { get; set; }

        [Parameter] protected T Value { get; set; }

        [Parameter] private Action<T> ValueChanged { get; set; }

        protected void ValueChange(UIChangeEventArgs e)
        {
            if (typeof(T).IsEnum)
            {
                Value = (T)Enum.Parse(typeof(T), e.Value.ToString());
            }
            else if (typeof(T).IsPrimitive)
            {
                Value = (T) Convert.ChangeType(e.Value, typeof(T));
            }
            else
            {
                Value = (T) e.Value;
            }

            ValueChanged(Value);
        }

    }
}
