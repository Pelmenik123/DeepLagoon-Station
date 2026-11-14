using Robust.Shared.Serialization;

namespace Content.Shared._DeepLagoon.InteractionPanel;

[Serializable, NetSerializable]
public enum InteractionPanelConsent : byte
{
    No = 0,
    Ask = 1,
    Yes = 2,

    AskSub = 3,
    AskTop = 4,
    AskSwitch = 5,
    YesSub = 6,
    YesTop = 7,
    YesSwitch = 8,

    AskPrey = 9,
    AskPred = 10,
    YesPrey = 11,
    YesPred = 12,
}

public static class InteractionPanelConsentRules
{
    public static readonly InteractionPanelConsent[] EroticOptions =
    [
        InteractionPanelConsent.No,
        InteractionPanelConsent.AskSub,
        InteractionPanelConsent.AskTop,
        InteractionPanelConsent.AskSwitch,
        InteractionPanelConsent.Ask,
        InteractionPanelConsent.Yes,
        InteractionPanelConsent.YesSub,
        InteractionPanelConsent.YesTop,
        InteractionPanelConsent.YesSwitch,
    ];

    public static readonly InteractionPanelConsent[] VoreOptions =
    [
        InteractionPanelConsent.No,
        InteractionPanelConsent.AskPrey,
        InteractionPanelConsent.AskPred,
        InteractionPanelConsent.Ask,
        InteractionPanelConsent.AskSwitch,
        InteractionPanelConsent.Yes,
        InteractionPanelConsent.YesPrey,
        InteractionPanelConsent.YesPred,
        InteractionPanelConsent.YesSwitch,
    ];

    private static readonly InteractionPanelConsent[] NeutralOptions =
    [
        InteractionPanelConsent.Yes,
    ];

    public static InteractionPanelConsent[] Options(InteractionPanelCategory category) => category switch
    {
        InteractionPanelCategory.Erotic or InteractionPanelCategory.NonCon => EroticOptions,
        InteractionPanelCategory.Vore => VoreOptions,
        _ => NeutralOptions,
    };

    public static bool Allows(this InteractionPanelConsent consent) => consent != InteractionPanelConsent.No;

    public static bool IsAsk(this InteractionPanelConsent consent) => consent is
        InteractionPanelConsent.Ask or
        InteractionPanelConsent.AskSub or
        InteractionPanelConsent.AskTop or
        InteractionPanelConsent.AskSwitch or
        InteractionPanelConsent.AskPrey or
        InteractionPanelConsent.AskPred;

    public static bool IsYes(this InteractionPanelConsent consent) => consent is
        InteractionPanelConsent.Yes or
        InteractionPanelConsent.YesSub or
        InteractionPanelConsent.YesTop or
        InteractionPanelConsent.YesSwitch or
        InteractionPanelConsent.YesPrey or
        InteractionPanelConsent.YesPred;

    public static string LabelKey(InteractionPanelConsent consent) => consent switch
    {
        InteractionPanelConsent.No => "dl-interaction-panel-consent-no",
        InteractionPanelConsent.Ask => "dl-interaction-panel-consent-ask",
        InteractionPanelConsent.Yes => "dl-interaction-panel-consent-yes",
        InteractionPanelConsent.AskSub => "dl-interaction-panel-consent-ask-sub",
        InteractionPanelConsent.AskTop => "dl-interaction-panel-consent-ask-top",
        InteractionPanelConsent.AskSwitch => "dl-interaction-panel-consent-ask-switch",
        InteractionPanelConsent.AskPrey => "dl-interaction-panel-consent-ask-prey",
        InteractionPanelConsent.AskPred => "dl-interaction-panel-consent-ask-pred",
        InteractionPanelConsent.YesSub => "dl-interaction-panel-consent-yes-sub",
        InteractionPanelConsent.YesTop => "dl-interaction-panel-consent-yes-top",
        InteractionPanelConsent.YesSwitch => "dl-interaction-panel-consent-yes-switch",
        InteractionPanelConsent.YesPrey => "dl-interaction-panel-consent-yes-prey",
        InteractionPanelConsent.YesPred => "dl-interaction-panel-consent-yes-pred",
        _ => "dl-interaction-panel-consent-no",
    };
}
