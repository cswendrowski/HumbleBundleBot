using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor;

namespace HumbleBundleBotRegistration.Shared.Components
{
    public interface IImFormComponent<T>
    {

        string Id { get; set; }

        string Title { get; set; }

        T Value { get; set; }        

        Action<T> ValueChanged { get; set; }

        void ValueChange(UIChangeEventArgs e);

    }
}
