using System;

namespace AHS.SharedKernel;

public abstract class Entity
{
    public Guid Id { get; protected set; }
}