using Content.Shared._DeepLagoon.InteractionPanel;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Localization;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private void InitializeInteractionPanelPreferences()
    {
        SetupInteractionPanelOption(ERPConsentButton, InteractionPanelCategory.Erotic);
        SetupInteractionPanelOption(NonConConsentButton, InteractionPanelCategory.NonCon);
        SetupInteractionPanelOption(VoreConsentButton, InteractionPanelCategory.Vore);
    }

    private void SetupInteractionPanelOption(OptionButton button, InteractionPanelCategory category)
    {
        foreach (var consent in InteractionPanelConsentRules.Options(category))
            button.AddItem(Loc.GetString(InteractionPanelConsentRules.LabelKey(consent)), (int)consent);
        button.OnItemSelected += args =>
        {
            if (Profile == null) return;
            button.SelectId(args.Id);
            Profile = Profile.WithInteractionPanelConsent(category, (InteractionPanelConsent)args.Id);
            SetDirty();
        };
    }

    private void UpdateInteractionPanelPreferences()
    {
        ERPConsentButton.Disabled = NonConConsentButton.Disabled = VoreConsentButton.Disabled = Profile == null;
        ERPConsentButton.SelectId((int)(Profile?.ERPConsent ?? InteractionPanelConsent.Ask));
        NonConConsentButton.SelectId((int)(Profile?.NonConConsent ?? InteractionPanelConsent.Ask));
        VoreConsentButton.SelectId((int)(Profile?.VoreConsent ?? InteractionPanelConsent.Ask));
    }
}
