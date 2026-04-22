namespace MediatorAuthService.Domain.Core.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class NotSortableAttribute : Attribute { }