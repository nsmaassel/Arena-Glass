using ArenaGlass.Core.Models;
using ArenaGlass.Core.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ArenaGlass.Tests;

public class CardTrackingServiceTests
{
    private readonly ILogger<CardTrackingService> _logger;
    private readonly CardTrackingService _service;

    public CardTrackingServiceTests()
    {
        _logger = new LoggerFactory().CreateLogger<CardTrackingService>();
        _service = new CardTrackingService(_logger);
    }

    [Fact]
    public void InitialState_ShouldHaveCardsInLibrary()
    {
        // Act
        var gameState = _service.CurrentGameState;

        // Assert
        Assert.NotNull(gameState);
        Assert.True(gameState.Library.Count > 0);
        Assert.True(gameState.TotalCardsInLibrary > 0);
        Assert.False(gameState.IsGameActive);
    }

    [Fact]
    public void ProcessDrawEvent_ShouldReduceCardCount()
    {
        // Arrange
        var initialCount = _service.CurrentGameState.TotalCardsInLibrary;
        var drawEvent = new GameEvent
        {
            Timestamp = DateTime.Now,
            EventType = "DrawEvent",
            Data = "Test draw event"
        };

        // Act
        _service.ProcessGameEvent(drawEvent);

        // Assert
        Assert.Equal(initialCount - 1, _service.CurrentGameState.TotalCardsInLibrary);
    }

    [Fact]
    public void Reset_ShouldRestoreInitialState()
    {
        // Arrange - draw a card first
        var drawEvent = new GameEvent
        {
            Timestamp = DateTime.Now,
            EventType = "DrawEvent",
            Data = "Test draw event"
        };
        _service.ProcessGameEvent(drawEvent);
        
        // Act
        _service.Reset();

        // Assert
        Assert.True(_service.CurrentGameState.TotalCardsInLibrary > 0);
        Assert.False(_service.CurrentGameState.IsGameActive);
    }

    [Fact]
    public void GameStateUpdated_ShouldTriggerEvent()
    {
        // Arrange
        var eventTriggered = false;
        GameState? receivedState = null;
        
        _service.GameStateUpdated += (sender, state) =>
        {
            eventTriggered = true;
            receivedState = state;
        };

        var drawEvent = new GameEvent
        {
            Timestamp = DateTime.Now,
            EventType = "DrawEvent",
            Data = "Test draw event"
        };

        // Act
        _service.ProcessGameEvent(drawEvent);

        // Assert
        Assert.True(eventTriggered);
        Assert.NotNull(receivedState);
    }
}