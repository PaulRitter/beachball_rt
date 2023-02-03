namespace Content.Client.UserInterface.States;

public interface IBeachBallState
{
    void SubscribeToEvents(BeachballSystem system);
    void UnsubscribeFromEvents(BeachballSystem system);
    void UpdateData(BeachballSystem system);
}