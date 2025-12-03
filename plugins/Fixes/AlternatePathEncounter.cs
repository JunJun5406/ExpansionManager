namespace ExpansionManager.Fixes;

// Combat director overrides will 'fallback' to normal spawns anyway if the selected monster is unavailable
// but we choose a more suitable replacement for the shrine-spawned Halcyonite monster and others
public static class AlternatePathEncounter
{
    [SystemInitializer]
    private static void Init()
    {
        On.RoR2.CombatDirector.OverrideCurrentMonsterCard += CombatDirector_OverrideCurrentMonsterCard;
        On.RoR2.SolusFight.OverrideBossFight += SolusFight_OverrideBossFight;
    }

    private static void CombatDirector_OverrideCurrentMonsterCard(On.RoR2.CombatDirector.orig_OverrideCurrentMonsterCard orig, CombatDirector self, DirectorCard overrideMonsterCard)
    {
        if (overrideMonsterCard != null && !overrideMonsterCard.IsAvailable() && overrideMonsterCard.spawnCard && self.finalMonsterCardsSelection != null)
        {
            WeightedSelection<DirectorCard> suitableReplacementsSelection = new WeightedSelection<DirectorCard>();
            for (int i = 0; i < self.finalMonsterCardsSelection.Count; i++)
            {
                var choice = self.finalMonsterCardsSelection.GetChoice(i);
                if (choice.value != null && choice.value.IsAvailable() && choice.value.cost <= overrideMonsterCard.cost && choice.value.cost * 5 >= overrideMonsterCard.cost)
                {
                    choice.weight *= choice.value.cost;
                    suitableReplacementsSelection.AddChoice(choice);
                }
            }
            if (suitableReplacementsSelection.Count > 0)
            {
                overrideMonsterCard = suitableReplacementsSelection.Evaluate(self.rng.nextNormalizedFloat);
            }
        }
        orig(self, overrideMonsterCard);
    }

    private static void SolusFight_OverrideBossFight(On.RoR2.SolusFight.orig_OverrideBossFight orig, SolusFight self)
    {
        orig(self);
        CombatDirector_OverrideCurrentMonsterCard((combatDirector, overrideMonsterCard) =>
        {
            combatDirector.OverrideNextBossCard(overrideMonsterCard, false);
        },
        TeleporterInteraction.instance.companionBoss, self.ForcedBossFight);
    }
}
