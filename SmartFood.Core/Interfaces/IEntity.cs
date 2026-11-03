using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFood.Core.Interfaces
{
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
    }
}
