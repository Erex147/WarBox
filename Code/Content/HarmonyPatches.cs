using NeoModLoader.General.Event.Handlers;
using NeoModLoader.General.Event.Listeners;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using ai.behaviours;
using System;
using UnityEngine.UI;
using UnityEngine;

namespace WarBox.Content;

internal static class WarBoxPatches
{
    internal static Harmony harmony;
    public static void Init()
    {
        harmony = new Harmony("com.Erex147.WarBox");
        Assembly assembly = Assembly.GetExecutingAssembly();

        try
        {
            harmony.PatchAll(assembly);
        }
        catch (Exception e)
        {
            WarBox.LogError(e.ToString());
        }

        AssetManager.combat_action_library.get("combat_attack_range").action = attackRangeAction;
        AssetManager.decisions_library.get("city_idle_walking").action_check_launch = delegate (Actor pActor)
        {
            if (!pActor.hasCity())
            {
                return false;
            }
            return pActor.city.hasZones();
        };
    }

    private static bool attackRangeAction(AttackData pData) // replaces default range action
    {
        Actor actor = pData.initiator.a;
        BaseSimObject target = pData.target;
        string projectile_id = pData.projectile_id;
        _ = actor.actor_scale;
        float scaleMod = actor.getScaleMod();
        float num = actor.stats["size"];
        int num2 = (int)actor.stats["projectiles"];
        Vector2 vector;
        if (target == null)
        {
            vector = pData.hit_position;
        }
        else
        {
            vector = AssetManager.combat_action_library.getAttackTargetPosition(pData);
            vector.y += 0.2f * scaleMod;
        }
        float num3 = actor.stats["accuracy"];
        float pMaxExclusive = Toolbox.DistVec2Float(actor.current_position, vector) / num3 * 0.25f;
        pMaxExclusive = Randy.randomFloat(0f, pMaxExclusive);
        pMaxExclusive = Mathf.Clamp(pMaxExclusive, 0f, 2f);
        float pStartPosZ = (actor.getActorAsset().very_high_flyer == true) ? 3f : 0.6f * scaleMod;
        float pTargetZ = 0f;
        float value = 0f;
        for (int i = 0; i < num2; i++)
        {
            Vector2 vector2 = new Vector2(vector.x, vector.y);
            if (num3 < 10f)
            {
                Vector2 innacuracyVector = AssetManager.combat_action_library.getInnacuracyVector(num3);
                innacuracyVector *= pMaxExclusive;
                vector2 += innacuracyVector;
            }
            Vector3 newPoint = Toolbox.getNewPoint(actor.current_position.x, actor.current_position.y, vector2.x, vector2.y, num * scaleMod);
            newPoint.y += actor.getHeight();
            if (target != null && target.isInAir())
            {
                pTargetZ = target.getHeight();
            }
            value = World.world.projectiles.spawn(actor, target, projectile_id, newPoint, vector2, pTargetZ, pStartPosZ, pData.kill_action, pData.kingdom).getLaunchAngle();
        }
        actor.spawnSlash(vector, null, 2f, pTargetZ, (actor.getActorAsset().very_high_flyer == true) ? pStartPosZ : 0f, value);
        return true;
    }

    private static bool city_idle_walking(Actor pActor)
    {
        if (!pActor.hasCity())
        {
            return false;
        }
        return pActor.city.hasZones();
    }
}

[HarmonyPatch(typeof(City), "update")]
public static class Patch_CityUpdate //adding units according to population
{
    static readonly Dictionary<long, float> times = new Dictionary<long, float>();

    static void Prefix(City __instance, float pElapsed)
    {
        if (!WarBox.warbox_factories) return;
        if (__instance == null) return;
        if (!times.ContainsKey(__instance.id)) times[__instance.id] = 20f;
        if (__instance.isAlive() == false) return;

        if (times[__instance.id] > 0)
        {
            times[__instance.id] -= pElapsed;
            return;
        }

        times[__instance.id] = 20f;

        Dictionary<WorldTile, string> can_produce = new Dictionary<WorldTile, string> { };

        foreach (Building building in __instance.buildings)
        {
            BuildingAsset buildingAsset = building.asset;
            if (buildingAsset == null) continue;

            if (buildingAsset.type == "type_heavyfactory") // heavy vehicles
            {
                if (Randy.randomChance(0.55f)) can_produce.Add(building.current_tile, "warbox_ifv");
                else can_produce.Add(building.current_tile, "warbox_tank");
            }
            else if (buildingAsset.type == "type_lightfactory") // light vehicles
            {
                if (Randy.randomChance(0.6f)) can_produce.Add(building.current_tile, "warbox_spg");
                else can_produce.Add(building.current_tile, "warbox_apc");
            }
            else if (buildingAsset.type == "type_lightaircraftfactory") // light helicopters
            {
                can_produce.Add(building.current_tile, "warbox_helicopter");
            }
            else if (buildingAsset.type == "type_heavyaircraftfactory") // heavy planes
            {
                if (Randy.randomChance(0.6f)) can_produce.Add(building.current_tile, "warbox_bomber");
                else can_produce.Add(building.current_tile, "warbox_fighter");
            }
        }

        if (can_produce.Count == 0) return;

        int people = __instance.getPopulationPeople();
        int total_vehicles = 0;

        int vehicle_count = Math.Max(people / WarBox.warbox_army_per_pop, 0);
        foreach (Actor actor in __instance.units) if (actor.hasTrait("warbox_unit")) total_vehicles++;


        if (total_vehicles >= vehicle_count) return;

        foreach (KeyValuePair<WorldTile, string> pair in can_produce)
        {
            if (total_vehicles > vehicle_count) break;

            WorldTile tile = pair.Key;
            if (tile == null) continue;

            ActorAsset asset = AssetManager.actor_library.get(pair.Value);
            if (asset == null) continue;
            if (__instance.getResourcesAmount("common_metals") < asset.cost.common_metals) continue;
            __instance.takeResource("common_metals", asset.cost.common_metals);

            Actor unit = World.world.units.createNewUnit(
                pair.Value,
                tile,
                pMiracleSpawn: false,
                0f,
                null,
                null,
                pSpawnWithItems: false
            );

            unit.makeWait(1f);
            unit.setKingdom(__instance.kingdom);
            unit.setCity(__instance);
        }
    }
}

[HarmonyPatch(typeof(City), "checkIfWarriorStillOk")]
public static class Patch_CityCheckWarrior
{
    static bool Prefix()
    {
        if (WarBox.warbox_army_limits == true) return true;
        return false;
    }
}

[HarmonyPatch(typeof(ActorAnimationLoader), nameof(ActorAnimationLoader.loadAnimationBoat))]
public static class Patch_ActorAnimationLoader_Fix
{
    static bool Prefix(string pTexturePath)
    {
        if (SpriteTextureLoader.getSpriteList("actors/boats/" + pTexturePath).Length == 0)
            return false;
        return true;
    }
}

[HarmonyPatch(typeof(Actor), "setFamily")]
public static class Patch_Actor_Exclude_WarBoxUnit_Family
{
    static bool Prefix(Actor __instance, Family pObject)
    {
        if (__instance.hasTrait("warbox_unit"))
            return false;
        return true;
    }
}

[HarmonyPatch(typeof(Actor), "addStatusEffect")]
public static class Patch_Actor_AddStatusEffect_DontGetAngry // idiots shoot each other and then keep shoot each other
{
    static bool Prefix(Actor __instance, StatusAsset pStatusAsset, float pOverrideTimer = 0f, bool pColorEffect = true)
    {
        return !(__instance.hasTrait("warbox_unit") && pStatusAsset.id == "angry");
    }
}

[HarmonyPatch(typeof(Kingdom), "setKing")]
public static class Patch_Kingdom_Exclude_WarBoxUnit_King
{
    static bool Prefix(Kingdom __instance, Actor pActor, bool pFromLoad)
    {
        if (pActor.hasTrait("warbox_unit"))
            return false;
        return true;
    }
}

[HarmonyPatch(typeof(City), "setLeader")]
public static class Patch_City_Exclude_WarBoxUnit_Leader
{
    static bool Prefix(City __instance, Actor pActor, bool pNew)
    {
        if (pActor.hasTrait("warbox_unit"))
            return false;
        return true;
    }
}

[HarmonyPatch(typeof(TileZone), nameof(TileZone.canBeClaimedByCity))]
public static class Patch_TileZone_CanBeClaimedByCity_WarBoxUnit
{
    static bool Prefix(TileZone __instance, City pCity, ref bool __result)
    {
        if (pCity != null && pCity.leader != null && pCity.leader.hasTrait("warbox_unit"))
        {
            __result = false;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(TileZone), "isGoodForNewCity", new[] { typeof(Actor) })]
public static class Patch_TileZone_IsGoodForNewCity_WarBoxUnit
{
    static bool Prefix(TileZone __instance, Actor pActor, ref bool __result)
    {
        if (pActor != null && pActor.hasTrait("warbox_unit"))
        {
            __result = false;
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Clan), "newClan")]
public static class Patch_Clan_NewClan
{
    static bool Prefix(Actor pFounder, bool pAddDefaultTraits)
    {
        return pFounder != null && !pFounder.hasTrait("warbox_unit");
    }
}

[HarmonyPatch(typeof(ai.behaviours.BehFightCheckEnemyIsOk), "execute")]
public static class BehFightCheckEnemyIsOk_Patch
{
    static bool Prefix(Actor pActor, ref BehResult __result)
    {
        if (!pActor.has_attack_target || !pActor.isEnemyTargetAlive())
        {
            __result = BehResult.Stop;
            return false;
        }

        Actor tTarget = pActor.attack_target as Actor;
        if (tTarget == null)
        {
            __result = BehResult.Stop;
            return false;
        }

        bool isValidMilitaryTarget = tTarget.isWarrior()
            || (tTarget.profession_asset != null && (
                tTarget.profession_asset.profession_id == UnitProfession.King ||
                tTarget.profession_asset.profession_id == UnitProfession.Leader))
            || tTarget.hasTrait("warbox_unit")
            || tTarget.hasTrait("boat")
            || tTarget.asset.is_boat;

        if (pActor.hasTrait("warbox_unit") && !isValidMilitaryTarget)
        {
            pActor.ignoreTarget(tTarget);
            pActor.clearAttackTarget();
            __result = BehResult.Stop;
            return false;
        }

        if (tTarget.isKingdomCiv() && tTarget.hasCity() && !isValidMilitaryTarget)
        {
            pActor.ignoreTarget(tTarget);
            pActor.clearAttackTarget();
            __result = BehResult.Stop;
            return false;
        }

        Kingdom actorKingdom = pActor.kingdom;
        if (pActor.hasTrait("warbox_unit") && pActor.city != null)
        {
            actorKingdom = pActor.city.kingdom;
        }

        if (!actorKingdom.isEnemy(tTarget.kingdom))
        {
            pActor.clearAttackTarget();
            __result = BehResult.Stop;
            return false;
        }

        if (!pActor.canAttackTarget(tTarget))
        {
            pActor.ignoreTarget(tTarget);
            pActor.clearAttackTarget();
            __result = BehResult.Stop;
            return false;
        }

        if (!pActor.isInAttackRange(tTarget))
        {
            if ((pActor.isWaterCreature() && (!tTarget.isInLiquid() && !pActor.asset.force_land_creature)) || tTarget.isFlying() ||
                (!pActor.isWaterCreature() && tTarget.isInLiquid()))
            {
                pActor.ignoreTarget(tTarget);
                pActor.clearAttackTarget();
                __result = BehResult.Stop;
                return false;
            }
        }

        if (Toolbox.Dist(pActor.chunk.x, pActor.chunk.y, tTarget.chunk.x, tTarget.chunk.y) >= SimGlobals.m.unit_chunk_sight_range + 1f)
        {
            pActor.clearAttackTarget();
            __result = BehResult.Stop;
            return false;
        }

        pActor.beh_actor_target = tTarget;
        __result = BehResult.Continue;
        return false;
    }
}

[HarmonyPatch(typeof(UtilityBasedDecisionSystem), "registerBasicDecisionLists")]
public static class Patch_UtilityBasedDecisionSystem_RegisterBasicDecisionLists
{
    static bool Prefix(Actor pActor, bool pGameplay)
    {
        if (pActor.asset.is_boat || pActor.hasTrait("warbox_unit"))
        {
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(ItemCrafting), nameof(ItemCrafting.tryToCraftRandomWeapon))]
public class Patch_TryToCraftRandomWeapon
{
    static bool Prefix(Actor pActor, City pCity)
    {
        if (pActor == null)
        {
            return false;
        }
        if (pActor.hasTrait("warbox_unit"))
        {
            return false;
        }

        if (pActor.equipment?.getSlot(EquipmentType.Weapon) == null)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch]
public static class Patch_ItemCrafting_ExcludeWarBoxUnit
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemCrafting), nameof(ItemCrafting.tryToCraftRandomWeapon))]
    public static bool Prefix_Weapon(Actor pActor, City pCity)
    {
        return isSafeToCraft(pActor, EquipmentType.Weapon);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemCrafting), nameof(ItemCrafting.tryToCraftRandomArmor))]
    public static bool Prefix_Armor(Actor pActor, City pCity)
    {
        return isSafeToCraft(pActor, EquipmentType.Armor);
    }

    private static bool isSafeToCraft(Actor pActor, EquipmentType type)
    {
        if (pActor == null || pActor.hasTrait("warbox_unit"))
            return false;

        if (pActor.equipment == null)
            return false;

        if (pActor.equipment.getSlot(type) == null)
            return false;

        return true;
    }
}
