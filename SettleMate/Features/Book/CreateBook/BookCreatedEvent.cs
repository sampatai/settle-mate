using SettleMate.Abstractions;

namespace SettleMate.Features.Book.CreateBook;

public sealed record BookCreatedEvent(Guid BookId, string Title, string Author) : IDomainEvent;
