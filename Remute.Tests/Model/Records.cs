using System;

namespace Remutable.Tests.Model
{
    public record Record1(Guid Id, string Title);
    public record Record2(Record1 Record1, bool Enabled);
}