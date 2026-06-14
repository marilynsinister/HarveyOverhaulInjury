namespace HarveyOverhaul.InjuryCare.Api
{
    /// <summary>
    /// Зеркало контракта HarveyStressMeter.Api.IHarveyPanelHostApi для вызова общего окна «План Харви».
    /// </summary>
    public interface IHarveyPanelHostApi
    {
        void OpenPanel(string tabId);

        bool IsPanelOpen();
    }
}
