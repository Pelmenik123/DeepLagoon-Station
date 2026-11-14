using System;
using System.Linq;
using Content.Shared._DeepLagoon.InteractionPanel;
using NUnit.Framework;

namespace Content.Tests.Shared._DeepLagoon;

[TestFixture]
public sealed class InteractionPanelConsentTests
{
    [Test]
    public void LegacyValuesKeepTheirDatabaseNumbers()
    {
        Assert.Multiple(() =>
        {
            Assert.That((byte) InteractionPanelConsent.No, Is.EqualTo(0));
            Assert.That((byte) InteractionPanelConsent.Ask, Is.EqualTo(1));
            Assert.That((byte) InteractionPanelConsent.Yes, Is.EqualTo(2));
        });
    }

    [Test]
    public void NonNoConsentAllowsInteraction()
    {
        foreach (var consent in Enum.GetValues<InteractionPanelConsent>())
        {
            Assert.That(consent.Allows(), Is.EqualTo(consent != InteractionPanelConsent.No), consent.ToString());
        }
    }

    [Test]
    public void AskAndYesFamiliesDoNotOverlap()
    {
        foreach (var consent in Enum.GetValues<InteractionPanelConsent>())
        {
            Assert.That(consent.IsAsk() && consent.IsYes(), Is.False, consent.ToString());
            if (consent == InteractionPanelConsent.No)
                continue;

            Assert.That(consent.IsAsk() || consent.IsYes(), Is.True, consent.ToString());
        }
    }

    [TestCase(InteractionPanelCategory.Erotic)]
    [TestCase(InteractionPanelCategory.NonCon)]
    public void EroticCategoriesExposeRoleOptions(InteractionPanelCategory category)
    {
        Assert.That(InteractionPanelConsentRules.Options(category), Is.EqualTo(new[]
        {
            InteractionPanelConsent.No,
            InteractionPanelConsent.AskSub,
            InteractionPanelConsent.AskTop,
            InteractionPanelConsent.AskSwitch,
            InteractionPanelConsent.Ask,
            InteractionPanelConsent.Yes,
            InteractionPanelConsent.YesSub,
            InteractionPanelConsent.YesTop,
            InteractionPanelConsent.YesSwitch,
        }));
    }

    [Test]
    public void VoreCategoryExposesPreyAndPredatorOptions()
    {
        Assert.That(InteractionPanelConsentRules.Options(InteractionPanelCategory.Vore), Is.EqualTo(new[]
        {
            InteractionPanelConsent.No,
            InteractionPanelConsent.AskPrey,
            InteractionPanelConsent.AskPred,
            InteractionPanelConsent.Ask,
            InteractionPanelConsent.AskSwitch,
            InteractionPanelConsent.Yes,
            InteractionPanelConsent.YesPrey,
            InteractionPanelConsent.YesPred,
            InteractionPanelConsent.YesSwitch,
        }));
    }

    [Test]
    public void EveryOptionHasALocalizationKey()
    {
        foreach (var category in Enum.GetValues<InteractionPanelCategory>())
        foreach (var consent in InteractionPanelConsentRules.Options(category))
        {
            Assert.That(InteractionPanelConsentRules.LabelKey(consent), Is.Not.Empty, $"{category}/{consent}");
        }
    }
}
