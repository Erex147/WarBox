using System.Collections.Generic;
using ai.behaviours;
using NeoModLoader.api.attributes;
using UnityEngine;

namespace WarBox.Content;

internal static class WarBoxEWE
{
    public static void Init()
    {
        AddActions();
        AddTerraformOptions();
        AddProjectiles();
        AddGuns();
        AddVehicleWeapons();
    }

    private static void AddActions()
    {
        CombatActionAsset atgm = new CombatActionAsset
        {
            id = "combat_launch_atgm",
            cost_stamina = 50,
            chance = 0.3f,
            cooldown = 20f,
            rate = 1,
            action_actor_target_position = WarBoxActions.LaunchATGM,
            can_do_action = delegate (Actor pSelf, BaseSimObject pAttackTarget)
            {
                float num = Toolbox.SquaredDistVec2Float(pSelf.current_position, pAttackTarget.current_position);
                return num > 10f && num < 1000f;
            },
            pools = AssetLibrary<CombatActionAsset>.a<CombatActionPool>(CombatActionPool.BEFORE_ATTACK_RANGE)
        };
        AssetManager.combat_action_library.add(atgm);

        BehaviourTaskActor idkyet = new BehaviourTaskActor
        {
            id = "artillery_strike"
        };
        AssetManager.tasks_actor.add(idkyet);

        DecisionAsset artillery_strike = new DecisionAsset
        {
            id = "artillery_strike",
            priority = NeuroLayer.Layer_4_Critical,
            unique = true,
            cooldown = 1,
            weight = 1f,
            path_icon = "ui/icons/actors/spg",
            task_id = "artillery_strike",
            action_check_launch = delegate (Actor pActor)
            {
                return WarBoxActions.LaunchArtilleryStrike(pActor, null);
            }
        };
        AssetManager.decisions_library.add(artillery_strike);
    }

    private static void AddGuns()
    {
        if (!AssetManager.items.equipment_by_subtypes.ContainsKey("gun"))
        {
            AssetManager.items.equipment_by_subtypes.Add("gun", new List<EquipmentAsset>());
        }

        if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey("firearm"))
        {
            AssetManager.items.pot_equipment_by_groups_all.Add("firearm", new List<EquipmentAsset>());
        }

        if (!AssetManager.items.pot_equipment_by_groups_unlocked.ContainsKey("firearm"))
        {
            AssetManager.items.pot_equipment_by_groups_unlocked.Add("firearm", new List<EquipmentAsset>());
        }

        // Pistol 
        BaseStats stats_pistol = new BaseStats();
        stats_pistol["projectiles"] = 1;
        stats_pistol["accuracy"] = -10f;
        stats_pistol["range"] = 14f;
        stats_pistol["damage"] = 35f;
        stats_pistol["attack_speed"] = 0f;
        stats_pistol["speed"] = 7.5f;

        EquipmentAsset pistol = CreateGun(
            id: "pistol",
            stats: stats_pistol,
            equipment_value: 60,
            name: "Pistol",
            description: "A very handy semi-automatic firearm",
            goldCost: 0,
            resource1: "common_metals", resource1Cost: 1
        );

        // SMG
        BaseStats stats_smg = new BaseStats();
        stats_smg["projectiles"] = 1;
        stats_smg["accuracy"] = -25f;
        stats_smg["range"] = 15f;
        stats_smg["damage"] = 25f;
        stats_smg["attack_speed"] = 20f;
        stats_smg["speed"] = 5f;

        EquipmentAsset smg = CreateGun(
            id: "smg",
            stats: stats_smg,
            equipment_value: 65,
            name: "SMG",
            description: "A fast firing sub-machine gun, not very accurate",
            goldCost: 0,
            resource1: "common_metals", resource1Cost: 1,
            resource2: "wood", resource2Cost: 1
        );

        // Rifle
        BaseStats stats_rifle = new BaseStats();
        stats_rifle["projectiles"] = 1;
        stats_rifle["accuracy"] = 95f;
        stats_rifle["range"] = 72.5f;
        stats_rifle["damage"] = 75f;
        stats_rifle["attack_speed"] = -5f;

        EquipmentAsset rifle = CreateGun(
            id: "rifle",
            stats: stats_rifle,
            equipment_value: 66,
            name: "Rifle",
            description: "A standard bolt-action rifle",
            goldCost: 0,
            resource1: "common_metals", resource1Cost: 1,
            resource2: "wood", resource2Cost: 2
        );

        // Automatic Rifle
        BaseStats stats_autorifle = new BaseStats();
        stats_autorifle["projectiles"] = 1;
        stats_autorifle["accuracy"] = 10f;
        stats_autorifle["range"] = 20f;
        stats_autorifle["damage"] = 45f;
        stats_autorifle["attack_speed"] = 3.5f;

        EquipmentAsset autorifle = CreateGun(
            id: "autorifle",
            stats: stats_autorifle,
            equipment_value: 68,
            name: "Automatic Rifle",
            description: "A repeating rifle, hits fast and hard",
            goldCost: 0,
            resource1: "common_metals", resource1Cost: 2,
            resource2: "wood", resource2Cost: 2
        );

        // Sniper Rifle
        BaseStats stats_sniperrifle = new BaseStats();
        stats_sniperrifle["projectiles"] = 1;
        stats_sniperrifle["accuracy"] = 100f;
        stats_sniperrifle["critical_chance"] = 0.75f;
        stats_sniperrifle["range"] = 120f;
        stats_sniperrifle["damage"] = 160f;
        stats_sniperrifle["attack_speed"] = -3f;

        EquipmentAsset sniperrifle = CreateGun(
            id: "sniperrifle",
            stats: stats_sniperrifle,
            equipment_value: 65,
            name: "Sniper Rifle",
            description: "A very accurate bolt-action rifle, hits VERY hard",
            goldCost: 0,
            resource1: "common_metals", resource1Cost: 1,
            resource2: "wood", resource2Cost: 3
        );

        // Shotgun (does not actually replace ingame shotgun)
        BaseStats stats_shotgun = new BaseStats();
        stats_shotgun["projectiles"] = 6f;
        stats_shotgun["accuracy"] = -60f;
        stats_shotgun["range"] = 15f;
        stats_shotgun["damage"] = 10f;
        stats_shotgun["attack_speed"] = -5f;

        EquipmentAsset shotgun = CreateGun(
            id: "shotgunreplace",
            stats: stats_shotgun,
            equipment_value: 60,
            name: "Shotgun",
            description: "A shotgun firing many pellets, dangerous up close",
            goldCost: 0,
            resource1: "common_metals", resource1Cost: 1,
            resource2: "wood", resource2Cost: 2
        );

        // RPG
        BaseStats stats_rpg = new BaseStats();
        stats_rpg["projectiles"] = 1;
        stats_rpg["accuracy"] = 0f;
        stats_rpg["range"] = 40f;
        stats_rpg["damage"] = 10f;
        stats_rpg["attack_speed"] = -10f;
        stats_rpg["speed"] = -4.5f;

        EquipmentAsset rpg = CreateGun(
            id: "rpg",
            stats: stats_rpg,
            equipment_value: 55,
            name: "Rocket Launcher",
            projectile: "rpg_projectile",
            description: "A launcher firing rocket propelled grenades",
            goldCost: 1,
            resource1: "common_metals", resource1Cost: 4
        );
    }

    private static void AddVehicleWeapons()
    {
        EquipmentAsset tank_cannon = AssetManager.items.clone("tank_cannon", "$range");
        tank_cannon.has_locales = false;
        tank_cannon.projectile = "tank_shell";
        tank_cannon.base_stats["projectiles"] = 1f;
        tank_cannon.base_stats["recoil"] = 0.5f;
        tank_cannon.path_slash_animation = "effects/gunshots/shot_gun";
        tank_cannon.show_in_meta_editor = false;
        tank_cannon.show_in_knowledge_window = false;

        EquipmentAsset auto_cannon = AssetManager.items.clone("auto_cannon", "tank_cannon");
        auto_cannon.projectile = "autocannon_shell";
        auto_cannon.base_stats["recoil"] = 0f;

        EquipmentAsset machine_gun = AssetManager.items.clone("machine_gun", "auto_cannon");
        machine_gun.projectile = "shotgun_bullet";

        EquipmentAsset artillery_cannon = AssetManager.items.clone("artillery_cannon", "tank_cannon");
        artillery_cannon.projectile = "cannon_shell";
        artillery_cannon.path_slash_animation = "effects/slashes/slash_cannonball";
        artillery_cannon.base_stats["recoil"] = 0.75f;

        EquipmentAsset rocket_pod = AssetManager.items.clone("rocket_pod", "$range");
        rocket_pod.has_locales = false;
        rocket_pod.projectile = "rocket_projectile";
        rocket_pod.base_stats["projectiles"] = 1f;
        rocket_pod.path_slash_animation = "effects/gunshots/shot_gun";
        rocket_pod.show_in_meta_editor = false;
        rocket_pod.show_in_knowledge_window = false;

        EquipmentAsset bomb_bay = AssetManager.items.clone("bomb_bay", "$range");
        bomb_bay.has_locales = false;
        bomb_bay.projectile = "drop_bomb";
        bomb_bay.base_stats["projectiles"] = 1f;
        bomb_bay.path_slash_animation = "event:/SFX/DROPS/DropLaunchGrenade";
        bomb_bay.show_in_meta_editor = false;
        bomb_bay.show_in_knowledge_window = false;
    }

    private static void AddTerraformOptions()
    {
        AssetManager.terraform.add(new TerraformOptions
        {
            id = "rpg",
            flash = true,
            damage_buildings = true,
            damage = 100,
            apply_force = true,
            explode_and_set_random_fire = false,
            explode_tile = false,
            explosion_pixel_effect = true,
            explode_strength = 1,
            shake = false,
            remove_tornado = true,
            remove_frozen = true,
        });

        AssetManager.terraform.add(new TerraformOptions
        {
            id = "rocket",
            flash = true,
            damage_buildings = true,
            damage = 500,
            apply_force = true,
            explode_and_set_random_fire = true,
            explode_tile = false,
            explosion_pixel_effect = true,
            explode_strength = 4,
            shake = false,
            remove_tornado = true,
            remove_frozen = true,
        });

        TerraformOptions soft_grenade = AssetManager.terraform.clone("soft_grenade", "grenade");
        soft_grenade.shake = false;
        soft_grenade.explode_and_set_random_fire = false;
    }

    private static void AddProjectiles()
    {
        AssetManager.projectiles.add(new ProjectileAsset
        {
            id = "rpg_projectile",
            speed = 35f,
            texture = "pr_rpg",
            look_at_target = true,
            texture_shadow = "shadows/projectiles/shadow_arrow",
            end_effect = "fx_fireball_explosion",
            hit_shake = false,
            scale_start = 0.075f,
            scale_target = 0.075f,
            draw_light_area = true,
            draw_light_size = 0.1f,
            sound_launch = "event:/SFX/WEAPONS/WeaponShotgunStart",
            sound_impact = "event:/SFX/WEAPONS/WeaponShotgunLand",
            terraform_option = "rocket",
            terraform_range = 4,
            can_be_blocked = false,
            can_be_collided = false,
        });

        AssetManager.projectiles.add(new ProjectileAsset
        {
            id = "rocket_projectile",
            speed = 45f,
            texture = "pr_rocket",
            look_at_target = true,
            texture_shadow = "shadows/projectiles/shadow_arrow",
            end_effect = "fx_fireball_explosion",
            hit_shake = false,
            scale_start = 0.025f,
            scale_target = 0.025f,
            draw_light_area = true,
            draw_light_size = 0.2f,
            sound_launch = "event:/SFX/WEAPONS/WeaponShotgunStart",
            sound_impact = "event:/SFX/WEAPONS/WeaponShotgunLand",
            terraform_option = "rpg",
            terraform_range = 4,
            can_be_blocked = false,
            can_be_collided = false,
        });

         AssetManager.projectiles.add(new ProjectileAsset
        {
            id = "drop_bomb",
            speed = 10f,
            texture = "pr_bomb",
            look_at_target = true,
            texture_shadow = "shadows/projectiles/shadow_arrow",
            end_effect = "fx_fireball_explosion",
            hit_shake = false,
            scale_start = 0.08f,
            scale_target = 0.08f,
            draw_light_area = false,
            sound_launch = "event:/SFX/EXPLOSIONS/WeaponShotgunStart",
            sound_impact = "event:/SFX/WEAPONS/WeaponShotgunLand",
            terraform_option = "soft_grenade",
            terraform_range = 5,
            can_be_blocked = false,
            can_be_collided = false,
        });

        AssetManager.projectiles.add(new ProjectileAsset
        {
            id = "cannon_shell",
            speed = 50f,
            texture = "pr_shell",
            look_at_target = true,
            texture_shadow = "shadows/projectiles/shadow_arrow",
            end_effect = "fx_fireball_explosion",
            hit_shake = false,
            scale_start = 0.08f,
            scale_target = 0.08f,
            draw_light_area = false,
            sound_launch = "event:/SFX/EXPLOSIONS/WeaponShotgunStart",
            sound_impact = "event:/SFX/WEAPONS/WeaponShotgunLand",
            terraform_option = "soft_grenade",
            terraform_range = 4,
            can_be_blocked = false,
            can_be_collided = false,
        });

        AssetManager.projectiles.add(new ProjectileAsset
        {
            id = "tank_shell",
            speed = 44f,
            texture = "pr_shell",
            look_at_target = true,
            texture_shadow = "shadows/projectiles/shadow_arrow",
            end_effect = "fx_fireball_explosion",
            hit_shake = false,
            scale_start = 0.04f,
            scale_target = 0.04f,
            draw_light_area = false,
            sound_launch = "event:/SFX/EXPLOSIONS/WeaponShotgunStart",
            sound_impact = "event:/SFX/WEAPONS/WeaponShotgunLand",
            terraform_option = "soft_grenade",
            terraform_range = 3,
            can_be_blocked = false,
            can_be_collided = false,
        });

        AssetManager.projectiles.add(new ProjectileAsset
        {
            id = "autocannon_shell",
            speed = 49f,
            texture = "pr_shell",
            look_at_target = true,
            texture_shadow = "shadows/projectiles/shadow_arrow",
            hit_shake = false,
            scale_start = 0.03f,
            scale_target = 0.03f,
            draw_light_area = false,
            sound_launch = "event:/SFX/WEAPONS/WeaponShotgunStart",
            sound_impact = "event:/SFX/WEAPONS/WeaponShotgunLand",
            can_be_blocked = false,
            can_be_collided = false,
        });

        AssetManager.projectiles.get("shotgun_bullet").can_be_collided = false;
    }

    private static EquipmentAsset CreateGun(string id, BaseStats stats, string projectile = "shotgun_bullet", int equipment_value = 100, string name = "", string description = "", int goldCost = 0, string resource1 = "none", int resource1Cost = 0, string resource2 = "none", int resource2Cost = 0)
    {
        EquipmentAsset gun = AssetManager.items.clone(id, "$range");
        gun.material = "steel";
        gun.path_slash_animation = "effects/gunshots/shot_gun";
        gun.name_class = "item_class_weapon";
        gun.equipment_subtype = "gun";
        gun.group_id = "firearm";
        gun.name_templates = new List<string>() { "gun_name" };
        gun.base_stats = stats;
        gun.translation_key = gun.id;
        gun.path_icon = "ui/icons/items/icon_" + gun.id;

        gun.equipment_type = EquipmentType.Weapon;
        gun.projectile = projectile;
        gun.metallic = true;

        gun.equipment_value = equipment_value;
        gun.minimum_city_storage_resource_1 = 10;
        gun.rigidity_rating = 1;

        gun.setCost(goldCost, resource1, resource1Cost, resource2, resource2Cost);
        gun.gameplay_sprites = new Sprite[] { SpriteTextureLoader.getSprite("weapons/" + id) };

        AssetManager.items.equipment_by_subtypes["gun"].Add(gun);
        AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(gun);
        AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(gun);

        return gun;
    }
}