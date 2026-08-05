using Microsoft.Extensions.Logging;
using SettleMate.Abstractions;

namespace SettleMate.Features.Book.CreateBook;

public sealed class BookCreatedEventHandler(ILogger<BookCreatedEventHandler> logger) : IEventHandler<BookCreatedEvent>
{
    public Task HandleAsync(BookCreatedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Book created: {BookId} - '{Title}' by {Author}", @event.BookId, @event.Title, @event.Author);
        return Task.CompletedTask;
    }
}
