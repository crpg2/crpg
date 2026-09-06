// VC_EventGen.js
// Generates all voice command events for cRPG
// Run via Scripts menu in FMOD Studio: cRPG > Generate Voice Command Events

var EVENT_GROUPS = {
    "voice/crpg/attack/female/archers": ["voice/attack/female/vc_1_attack_archersF.ogg", "voice/attack/female/vc_1_attack_archersF1.ogg", "voice/attack/female/vc_1_attack_archersF2.ogg", "voice/attack/female/vc_1_attack_archersF3.ogg"],
    "voice/crpg/attack/female/attack": ["voice/attack/female/vc_1_attack_attackF.ogg", "voice/attack/female/vc_1_attack_attack_F1.ogg", "voice/attack/female/vc_1_attack_attack_F2.ogg", "voice/attack/female/vc_1_attack_attack_F3.ogg"],
    "voice/crpg/attack/female/base": ["voice/attack/female/vc_1_attack_baseF.ogg", "voice/attack/female/vc_1_attack_baseF1.ogg", "voice/attack/female/vc_1_attack_baseF2.ogg", "voice/attack/female/vc_1_attack_baseF3.ogg"],
    "voice/crpg/attack/female/catapult": ["voice/attack/female/vc_1_attack_catapultF.ogg", "voice/attack/female/vc_1_attack_catapultF1.ogg", "voice/attack/female/vc_1_attack_catapultF2.ogg", "voice/attack/female/vc_1_attack_catapultF3.ogg"],
    "voice/crpg/attack/female/cavalry": ["voice/attack/female/vc_1_attack_cavalryF.ogg", "voice/attack/female/vc_1_attack_cavalryF1.ogg", "voice/attack/female/vc_1_attack_cavalryF2.ogg", "voice/attack/female/vc_1_attack_cavalryF3.ogg"],
    "voice/crpg/attack/female/charge": ["voice/attack/female/vc_1_attack_chargeF.ogg", "voice/attack/female/vc_1_attack_chargeF1.ogg", "voice/attack/female/vc_1_attack_chargeF2.ogg"],
    "voice/crpg/attack/female/equipment": ["voice/attack/female/vc_1_attack_equipmentF.ogg", "voice/attack/female/vc_1_attack_equipmentF1.ogg", "voice/attack/female/vc_1_attack_equipmentF2.ogg"],
    "voice/crpg/attack/female/gate": ["voice/attack/female/vc_1_attack_gateF.ogg", "voice/attack/female/vc_1_attack_gateF1.ogg", "voice/attack/female/vc_1_attack_gateF2.ogg", "voice/attack/female/vc_1_attack_gateF3.ogg"],
    "voice/crpg/attack/female/infantry": ["voice/attack/female/vc_1_attack_infantryF.ogg", "voice/attack/female/vc_1_attack_infantryF1.ogg", "voice/attack/female/vc_1_attack_infantryF2.ogg", "voice/attack/female/vc_1_attack_infantryF3.ogg"],
    "voice/crpg/attack/female/left_flank": ["voice/attack/female/vc_1_attack_left_flankF.ogg", "voice/attack/female/vc_1_attack_left_flankF1.ogg", "voice/attack/female/vc_1_attack_left_flankF2.ogg", "voice/attack/female/vc_1_attack_left_flankF3.ogg"],
    "voice/crpg/attack/female/right_flank": ["voice/attack/female/vc_1_attack_right_flankF.ogg", "voice/attack/female/vc_1_attack_right_flankF1.ogg", "voice/attack/female/vc_1_attack_right_flankF2.ogg", "voice/attack/female/vc_1_attack_right_flankF3.ogg"],
    "voice/crpg/attack/female/siege_tower": ["voice/attack/female/vc_1_attack_siege_towerF.ogg", "voice/attack/female/vc_1_attack_siege_towerF1.ogg", "voice/attack/female/vc_1_attack_siege_towerF2.ogg"],
    "voice/crpg/attack/female/tower": ["voice/attack/female/vc_1_attack_towerF.ogg", "voice/attack/female/vc_1_attack_towerF1.ogg", "voice/attack/female/vc_1_attack_towerF2.ogg", "voice/attack/female/vc_1_attack_towerF3.ogg"],
    "voice/crpg/attack/male/archers": ["voice/attack/male/vc_1_attack_archers.ogg", "voice/attack/male/vc_1_attack_archers1.ogg", "voice/attack/male/vc_1_attack_archers2.ogg", "voice/attack/male/vc_1_attack_archers3.ogg"],
    "voice/crpg/attack/male/attack": ["voice/attack/male/vc_1_attack_1.ogg", "voice/attack/male/vc_1_attack_2.ogg", "voice/attack/male/vc_1_attack_3.ogg", "voice/attack/male/vc_1_attack_attack.ogg", "voice/attack/male/vc_1_attack_attack_1.ogg", "voice/attack/male/vc_1_attack_attack_2.ogg", "voice/attack/male/vc_1_attack_attack_3.ogg"],
    "voice/crpg/attack/male/base": ["voice/attack/male/vc_1_attack_base.ogg", "voice/attack/male/vc_1_attack_base1.ogg", "voice/attack/male/vc_1_attack_base2.ogg", "voice/attack/male/vc_1_attack_base3.ogg"],
    "voice/crpg/attack/male/catapult": ["voice/attack/male/vc_1_attack_catapult.ogg", "voice/attack/male/vc_1_attack_catapult1.ogg", "voice/attack/male/vc_1_attack_catapult2.ogg", "voice/attack/male/vc_1_attack_catapult3.ogg"],
    "voice/crpg/attack/male/cavalry": ["voice/attack/male/vc_1_attack_cavalry.ogg", "voice/attack/male/vc_1_attack_cavalry1.ogg", "voice/attack/male/vc_1_attack_cavalry2.ogg", "voice/attack/male/vc_1_attack_cavalry3.ogg"],
    "voice/crpg/attack/male/charge": ["voice/attack/male/vc_1_attack_charge.ogg", "voice/attack/male/vc_1_attack_charge1.ogg", "voice/attack/male/vc_1_attack_charge2.ogg", "voice/attack/male/vc_1_attack_charge3.ogg"],
    "voice/crpg/attack/male/equipment": ["voice/attack/male/vc_1_attack_equipment.ogg", "voice/attack/male/vc_1_attack_equipment1.ogg", "voice/attack/male/vc_1_attack_equipment2.ogg", "voice/attack/male/vc_1_attack_equipment3.ogg"],
    "voice/crpg/attack/male/gate": ["voice/attack/male/vc_1_attack_gate.ogg", "voice/attack/male/vc_1_attack_gate1.ogg", "voice/attack/male/vc_1_attack_gate2.ogg", "voice/attack/male/vc_1_attack_gate3.ogg"],
    "voice/crpg/attack/male/infantry": ["voice/attack/male/vc_1_attack_infantry.ogg", "voice/attack/male/vc_1_attack_infantry1.ogg", "voice/attack/male/vc_1_attack_infantry2.ogg", "voice/attack/male/vc_1_attack_infantry3.ogg"],
    "voice/crpg/attack/male/left_flank": ["voice/attack/male/vc_1_attack_left_flank.ogg", "voice/attack/male/vc_1_attack_left_flank1.ogg", "voice/attack/male/vc_1_attack_left_flank2.ogg", "voice/attack/male/vc_1_attack_left_flank3.ogg"],
    "voice/crpg/attack/male/right_flank": ["voice/attack/male/vc_1_attack_right_flank.ogg", "voice/attack/male/vc_1_attack_right_flank1.ogg", "voice/attack/male/vc_1_attack_right_flank2.ogg", "voice/attack/male/vc_1_attack_right_flank3.ogg"],
    "voice/crpg/attack/male/siege_tower": ["voice/attack/male/vc_1_attack_siege_tower.ogg", "voice/attack/male/vc_1_attack_siege_tower1.ogg", "voice/attack/male/vc_1_attack_siege_tower2.ogg", "voice/attack/male/vc_1_attack_siege_tower3.ogg"],
    "voice/crpg/attack/male/tower": ["voice/attack/male/vc_1_attack_tower.ogg", "voice/attack/male/vc_1_attack_tower1.ogg", "voice/attack/male/vc_1_attack_tower2.ogg", "voice/attack/male/vc_1_attack_tower3.ogg"],
    "voice/crpg/defend/female/catapult": ["voice/defend/female/vc_1_defend_catapultF.ogg", "voice/defend/female/vc_1_defend_catapultF1.ogg", "voice/defend/female/vc_1_defend_catapultF2.ogg", "voice/defend/female/vc_1_defend_catapultF3.ogg"],
    "voice/crpg/defend/female/close_gate": ["voice/defend/female/vc_1_defend_close_gateF.ogg", "voice/defend/female/vc_1_defend_close_gateF1.ogg", "voice/defend/female/vc_1_defend_close_gateF2.ogg", "voice/defend/female/vc_1_defend_close_gateF3.ogg"],
    "voice/crpg/defend/female/defend": ["voice/defend/female/vc_1_defend_defendF.ogg", "voice/defend/female/vc_1_defend_defendF1.ogg", "voice/defend/female/vc_1_defend_defendF2.ogg", "voice/defend/female/vc_1_defend_defendF3.ogg"],
    "voice/crpg/defend/female/flag": ["voice/defend/female/vc_1_defend_flagF.ogg", "voice/defend/female/vc_1_defend_flagF1.ogg", "voice/defend/female/vc_1_defend_flagF2.ogg", "voice/defend/female/vc_1_defend_flagF3.ogg"],
    "voice/crpg/defend/female/flag_carrier": ["voice/defend/female/vc_1_defend_flag_carrierF.ogg", "voice/defend/female/vc_1_defend_flag_carrierF1.ogg", "voice/defend/female/vc_1_defend_flag_carrierF2.ogg", "voice/defend/female/vc_1_defend_flag_carrierF3.ogg"],
    "voice/crpg/defend/female/gate": ["voice/defend/female/vc_1_defend_gateF.ogg", "voice/defend/female/vc_1_defend_gateF1.ogg", "voice/defend/female/vc_1_defend_gateF2.ogg", "voice/defend/female/vc_1_defend_gateF3.ogg"],
    "voice/crpg/defend/female/gatehouse": ["voice/defend/female/vc_1_defend_gatehouseF.ogg", "voice/defend/female/vc_1_defend_gatehouseF1.ogg", "voice/defend/female/vc_1_defend_gatehouseF2.ogg", "voice/defend/female/vc_1_defend_gatehouseF3.ogg"],
    "voice/crpg/defend/female/ladders": ["voice/defend/female/vc_1_defend_laddersF.ogg", "voice/defend/female/vc_1_defend_laddersF1.ogg", "voice/defend/female/vc_1_defend_laddersF2.ogg", "voice/defend/female/vc_1_defend_laddersF3.ogg"],
    "voice/crpg/defend/female/siege_tower": ["voice/defend/female/vc_1_defend_siege_towerF.ogg", "voice/defend/female/vc_1_defend_siege_towerF1.ogg", "voice/defend/female/vc_1_defend_siege_towerF2.ogg", "voice/defend/female/vc_1_defend_siege_towerF3.ogg"],
    "voice/crpg/defend/female/stairs": ["voice/defend/female/vc_1_defend_stairsF.ogg", "voice/defend/female/vc_1_defend_stairsF1.ogg", "voice/defend/female/vc_1_defend_stairsF2.ogg", "voice/defend/female/vc_1_defend_stairsF3.ogg"],
    "voice/crpg/defend/female/tower": ["voice/defend/female/vc_1_defend_towerF.ogg", "voice/defend/female/vc_1_defend_towerF1.ogg", "voice/defend/female/vc_1_defend_towerF2.ogg", "voice/defend/female/vc_1_defend_towerF3.ogg"],
    "voice/crpg/defend/female/walls": ["voice/defend/female/vc_1_defend_wallsF.ogg", "voice/defend/female/vc_1_defend_wallsF1.ogg", "voice/defend/female/vc_1_defend_wallsF2.ogg", "voice/defend/female/vc_1_defend_wallsF3.ogg"],
    "voice/crpg/defend/male/catapult": ["voice/defend/male/vc_1_defend_catapult.ogg", "voice/defend/male/vc_1_defend_catapult1.ogg", "voice/defend/male/vc_1_defend_catapult2.ogg", "voice/defend/male/vc_1_defend_catapult3.ogg"],
    "voice/crpg/defend/male/close_gate": ["voice/defend/male/vc_1_defend_close_gate.ogg", "voice/defend/male/vc_1_defend_close_gate1.ogg", "voice/defend/male/vc_1_defend_close_gate2.ogg", "voice/defend/male/vc_1_defend_close_gate3.ogg"],
    "voice/crpg/defend/male/defend": ["voice/defend/male/vc_1_defend_defend.ogg", "voice/defend/male/vc_1_defend_defend1.ogg", "voice/defend/male/vc_1_defend_defend2.ogg", "voice/defend/male/vc_1_defend_defend3.ogg"],
    "voice/crpg/defend/male/flag": ["voice/defend/male/vc_1_defend_flag.ogg", "voice/defend/male/vc_1_defend_flag1.ogg", "voice/defend/male/vc_1_defend_flag2.ogg", "voice/defend/male/vc_1_defend_flag3.ogg"],
    "voice/crpg/defend/male/flag_carrier": ["voice/defend/male/vc_1_defend_flag_carrier.ogg", "voice/defend/male/vc_1_defend_flag_carrier1.ogg", "voice/defend/male/vc_1_defend_flag_carrier2.ogg", "voice/defend/male/vc_1_defend_flag_carrier3.ogg"],
    "voice/crpg/defend/male/gate": ["voice/defend/male/vc_1_defend_gate.ogg", "voice/defend/male/vc_1_defend_gate1.ogg", "voice/defend/male/vc_1_defend_gate2.ogg", "voice/defend/male/vc_1_defend_gate3.ogg"],
    "voice/crpg/defend/male/gatehouse": ["voice/defend/male/vc_1_defend_gatehouse.ogg", "voice/defend/male/vc_1_defend_gatehouse1.ogg", "voice/defend/male/vc_1_defend_gatehouse2.ogg", "voice/defend/male/vc_1_defend_gatehouse3.ogg"],
    "voice/crpg/defend/male/ladders": ["voice/defend/male/vc_1_defend_ladders.ogg", "voice/defend/male/vc_1_defend_ladders1.ogg", "voice/defend/male/vc_1_defend_ladders2.ogg", "voice/defend/male/vc_1_defend_ladders3.ogg"],
    "voice/crpg/defend/male/siege_tower": ["voice/defend/male/vc_1_defend_siege_tower.ogg", "voice/defend/male/vc_1_defend_siege_tower1.ogg", "voice/defend/male/vc_1_defend_siege_tower2.ogg", "voice/defend/male/vc_1_defend_siege_tower3.ogg"],
    "voice/crpg/defend/male/stairs": ["voice/defend/male/vc_1_defend_stairs.ogg", "voice/defend/male/vc_1_defend_stairs1.ogg", "voice/defend/male/vc_1_defend_stairs2.ogg", "voice/defend/male/vc_1_defend_stairs3.ogg"],
    "voice/crpg/defend/male/tower": ["voice/defend/male/vc_1_defend_tower.ogg", "voice/defend/male/vc_1_defend_tower1.ogg", "voice/defend/male/vc_1_defend_tower2.ogg", "voice/defend/male/vc_1_defend_tower3.ogg"],
    "voice/crpg/defend/male/walls": ["voice/defend/male/vc_1_defend_walls.ogg", "voice/defend/male/vc_1_defend_walls1.ogg", "voice/defend/male/vc_1_defend_walls2.ogg", "voice/defend/male/vc_1_defend_walls3.ogg"],
    "voice/crpg/equipment/female/build_siege_tower": ["voice/equipment/female/vc_1_equipment_build_siege_towerF.ogg", "voice/equipment/female/vc_1_equipment_build_siege_towerF1.ogg", "voice/equipment/female/vc_1_equipment_build_siege_towerF2.ogg", "voice/equipment/female/vc_1_equipment_build_siege_towerF3.ogg"],
    "voice/crpg/equipment/female/deploy_catapult": ["voice/equipment/female/vc_1_equipment_deploy_catapultF.ogg", "voice/equipment/female/vc_1_equipment_deploy_catapultF1.ogg", "voice/equipment/female/vc_1_equipment_deploy_catapultF2.ogg"],
    "voice/crpg/equipment/female/equipment": ["voice/equipment/female/vc_1_equipment_equipmentF.ogg", "voice/equipment/female/vc_1_equipment_equipmentF1.ogg", "voice/equipment/female/vc_1_equipment_equipmentF2.ogg", "voice/equipment/female/vc_1_equipment_equipmentF3.ogg"],
    "voice/crpg/equipment/female/ladders": ["voice/equipment/female/vc_1_equipment_laddersF.ogg", "voice/equipment/female/vc_1_equipment_laddersF1.ogg", "voice/equipment/female/vc_1_equipment_laddersF2.ogg", "voice/equipment/female/vc_1_equipment_laddersF3.ogg"],
    "voice/crpg/equipment/female/man_catapult": ["voice/equipment/female/vc_1_equipment_man_catapultF.ogg", "voice/equipment/female/vc_1_equipment_man_catapultF1.ogg", "voice/equipment/female/vc_1_equipment_man_catapultF2.ogg", "voice/equipment/female/vc_1_equipment_man_catapultF3.ogg"],
    "voice/crpg/equipment/female/man_siege_tower": ["voice/equipment/female/vc_1_equipment_man_siege_towerF.ogg", "voice/equipment/female/vc_1_equipment_man_siege_towerF1.ogg", "voice/equipment/female/vc_1_equipment_man_siege_towerF2.ogg"],
    "voice/crpg/equipment/female/shields": ["voice/equipment/female/vc_1_equipment_shieldsF.ogg", "voice/equipment/female/vc_1_equipment_shieldsF1.ogg", "voice/equipment/female/vc_1_equipment_shieldsF2.ogg", "voice/equipment/female/vc_1_equipment_shieldsF3.ogg"],
    "voice/crpg/equipment/male/build_siege_tower": ["voice/equipment/male/vc_1_equipment_build_siege_tower.ogg", "voice/equipment/male/vc_1_equipment_build_siege_tower1.ogg", "voice/equipment/male/vc_1_equipment_build_siege_tower2.ogg", "voice/equipment/male/vc_1_equipment_build_siege_tower3.ogg"],
    "voice/crpg/equipment/male/deploy_catapult": ["voice/equipment/male/vc_1_equipment_deploy_catapult.ogg", "voice/equipment/male/vc_1_equipment_deploy_catapult1.ogg", "voice/equipment/male/vc_1_equipment_deploy_catapult2.ogg", "voice/equipment/male/vc_1_equipment_deploy_catapult3.ogg"],
    "voice/crpg/equipment/male/equipment": ["voice/equipment/male/vc_1_equipment_equipment.ogg", "voice/equipment/male/vc_1_equipment_equipment1.ogg", "voice/equipment/male/vc_1_equipment_equipment2.ogg", "voice/equipment/male/vc_1_equipment_equipment3.ogg"],
    "voice/crpg/equipment/male/ladders": ["voice/equipment/male/vc_1_equipment_ladders.ogg", "voice/equipment/male/vc_1_equipment_ladders1.ogg", "voice/equipment/male/vc_1_equipment_ladders2.ogg", "voice/equipment/male/vc_1_equipment_ladders3.ogg"],
    "voice/crpg/equipment/male/man_catapult": ["voice/equipment/male/vc_1_equipment_man_catapult.ogg", "voice/equipment/male/vc_1_equipment_man_catapult1.ogg", "voice/equipment/male/vc_1_equipment_man_catapult2.ogg", "voice/equipment/male/vc_1_equipment_man_catapult3.ogg"],
    "voice/crpg/equipment/male/man_siege_tower": ["voice/equipment/male/vc_1_equipment_man_siege_tower.ogg", "voice/equipment/male/vc_1_equipment_man_siege_tower1.ogg", "voice/equipment/male/vc_1_equipment_man_siege_tower2.ogg", "voice/equipment/male/vc_1_equipment_man_siege_tower3.ogg"],
    "voice/crpg/equipment/male/shields": ["voice/equipment/male/vc_1_equipment_shields.ogg", "voice/equipment/male/vc_1_equipment_shields1.ogg", "voice/equipment/male/vc_1_equipment_shields2.ogg", "voice/equipment/male/vc_1_equipment_shields3.ogg"],
    "voice/crpg/formation/female/archers": ["voice/formation/female/vc_1_formation_archersF.ogg", "voice/formation/female/vc_1_formation_archersF1.ogg", "voice/formation/female/vc_1_formation_archersF2.ogg", "voice/formation/female/vc_1_formation_archersF3.ogg"],
    "voice/crpg/formation/female/cavalry": ["voice/formation/female/vc_1_formation_cavalryF.ogg", "voice/formation/female/vc_1_formation_cavalryF1.ogg", "voice/formation/female/vc_1_formation_cavalryF2.ogg", "voice/formation/female/vc_1_formation_cavalryF3.ogg"],
    "voice/crpg/formation/female/fall_back": ["voice/formation/female/vc_1_formation_fall_backF.ogg", "voice/formation/female/vc_1_formation_fall_backF1.ogg", "voice/formation/female/vc_1_formation_fall_backF2.ogg"],
    "voice/crpg/formation/female/follow_me": ["voice/formation/female/vc_1_formation_follow_meF.ogg", "voice/formation/female/vc_1_formation_follow_meF1.ogg", "voice/formation/female/vc_1_formation_follow_meF2.ogg", "voice/formation/female/vc_1_formation_follow_meF3.ogg"],
    "voice/crpg/formation/female/hold": ["voice/formation/female/vc_1_formation_holdF.ogg", "voice/formation/female/vc_1_formation_holdF1.ogg", "voice/formation/female/vc_1_formation_holdF2.ogg", "voice/formation/female/vc_1_formation_holdF3.ogg"],
    "voice/crpg/formation/female/infantry": ["voice/formation/female/vc_1_formation_infantryF.ogg", "voice/formation/female/vc_1_formation_infantryF1.ogg", "voice/formation/female/vc_1_formation_infantryF2.ogg", "voice/formation/female/vc_1_formation_infantryF3.ogg"],
    "voice/crpg/formation/female/keep": ["voice/formation/female/vc_1_formation_keepF.ogg", "voice/formation/female/vc_1_formation_keepF1.ogg", "voice/formation/female/vc_1_formation_keepF2.ogg", "voice/formation/female/vc_1_formation_keepF3.ogg"],
    "voice/crpg/formation/female/release_arrows": ["voice/formation/female/vc_1_formation_release_arrowsF.ogg", "voice/formation/female/vc_1_formation_release_arrowsF1.ogg", "voice/formation/female/vc_1_formation_release_arrowsF2.ogg"],
    "voice/crpg/formation/female/retreat": ["voice/formation/female/vc_1_formation_retreatF.ogg", "voice/formation/female/vc_1_formation_retreatF1.ogg", "voice/formation/female/vc_1_formation_retreatF2.ogg", "voice/formation/female/vc_1_formation_retreatF3.ogg"],
    "voice/crpg/formation/female/shieldwall": ["voice/formation/female/vc_1_formation_shieldwallF.ogg", "voice/formation/female/vc_1_formation_shieldwallF1.ogg", "voice/formation/female/vc_1_formation_shieldwallF2.ogg", "voice/formation/female/vc_1_formation_shieldwallF3.ogg"],
    "voice/crpg/formation/female/shoot": ["voice/formation/female/vc_1_formation_shootF.ogg", "voice/formation/female/vc_1_formation_shootF1.ogg", "voice/formation/female/vc_1_formation_shootF2.ogg"],
    "voice/crpg/formation/female/spread_out": ["voice/formation/female/vc_1_formation_spread_outF.ogg", "voice/formation/female/vc_1_formation_spread_outF1.ogg"],
    "voice/crpg/formation/female/stand_closer": ["voice/formation/female/vc_1_formation_stand_closerF.ogg", "voice/formation/female/vc_1_formation_stand_closerF1.ogg"],
    "voice/crpg/formation/female/take_cover": ["voice/formation/female/vc_1_formation_take_coverF.ogg", "voice/formation/female/vc_1_formation_take_coverF1.ogg", "voice/formation/female/vc_1_formation_take_coverF2.ogg", "voice/formation/female/vc_1_formation_take_coverF3.ogg"],
    "voice/crpg/formation/male/archers": ["voice/formation/male/vc_1_formation_archers.ogg", "voice/formation/male/vc_1_formation_archers1.ogg", "voice/formation/male/vc_1_formation_archers2.ogg", "voice/formation/male/vc_1_formation_archers3.ogg"],
    "voice/crpg/formation/male/cavalry": ["voice/formation/male/vc_1_formation_cavalry.ogg", "voice/formation/male/vc_1_formation_cavalry1.ogg", "voice/formation/male/vc_1_formation_cavalry2.ogg", "voice/formation/male/vc_1_formation_cavalry3.ogg"],
    "voice/crpg/formation/male/fall_back": ["voice/formation/male/vc_1_formation_fall_back.ogg", "voice/formation/male/vc_1_formation_fall_back1.ogg", "voice/formation/male/vc_1_formation_fall_back2.ogg", "voice/formation/male/vc_1_formation_fall_back3.ogg"],
    "voice/crpg/formation/male/follow_me": ["voice/formation/male/vc_1_formation_follow_me.ogg", "voice/formation/male/vc_1_formation_follow_me1.ogg", "voice/formation/male/vc_1_formation_follow_me2.ogg", "voice/formation/male/vc_1_formation_follow_me3.ogg"],
    "voice/crpg/formation/male/hold": ["voice/formation/male/vc_1_formation_hold.ogg", "voice/formation/male/vc_1_formation_hold1.ogg", "voice/formation/male/vc_1_formation_hold2.ogg", "voice/formation/male/vc_1_formation_hold3.ogg"],
    "voice/crpg/formation/male/infantry": ["voice/formation/male/vc_1_formation_infantry.ogg", "voice/formation/male/vc_1_formation_infantry1.ogg", "voice/formation/male/vc_1_formation_infantry2.ogg", "voice/formation/male/vc_1_formation_infantry3.ogg"],
    "voice/crpg/formation/male/keep": ["voice/formation/male/vc_1_formation_keep.ogg", "voice/formation/male/vc_1_formation_keep1.ogg", "voice/formation/male/vc_1_formation_keep2.ogg", "voice/formation/male/vc_1_formation_keep3.ogg"],
    "voice/crpg/formation/male/release_arrows": ["voice/formation/male/vc_1_formation_release_arrows.ogg", "voice/formation/male/vc_1_formation_release_arrows1.ogg", "voice/formation/male/vc_1_formation_release_arrows2.ogg", "voice/formation/male/vc_1_formation_release_arrows3.ogg"],
    "voice/crpg/formation/male/retreat": ["voice/formation/male/vc_1_formation_retreat.ogg", "voice/formation/male/vc_1_formation_retreat1.ogg", "voice/formation/male/vc_1_formation_retreat2.ogg", "voice/formation/male/vc_1_formation_retreat3.ogg"],
    "voice/crpg/formation/male/shieldwall": ["voice/formation/male/vc_1_formation_shieldwall.ogg", "voice/formation/male/vc_1_formation_shieldwall1.ogg", "voice/formation/male/vc_1_formation_shieldwall2.ogg", "voice/formation/male/vc_1_formation_shieldwall3.ogg"],
    "voice/crpg/formation/male/shoot": ["voice/formation/male/vc_1_formation_shoot.ogg", "voice/formation/male/vc_1_formation_shoot1.ogg", "voice/formation/male/vc_1_formation_shoot2.ogg", "voice/formation/male/vc_1_formation_shoot3.ogg"],
    "voice/crpg/formation/male/spread_out": ["voice/formation/male/vc_1_formation_spread_out.ogg", "voice/formation/male/vc_1_formation_spread_out1.ogg", "voice/formation/male/vc_1_formation_spread_out2.ogg", "voice/formation/male/vc_1_formation_spread_out3.ogg"],
    "voice/crpg/formation/male/stand_closer": ["voice/formation/male/vc_1_formation_stand_closer.ogg", "voice/formation/male/vc_1_formation_stand_closer1.ogg", "voice/formation/male/vc_1_formation_stand_closer2.ogg", "voice/formation/male/vc_1_formation_stand_closer3.ogg"],
    "voice/crpg/formation/male/take_cover": ["voice/formation/male/vc_1_formation_take_cover.ogg", "voice/formation/male/vc_1_formation_take_cover1.ogg", "voice/formation/male/vc_1_formation_take_cover2.ogg", "voice/formation/male/vc_1_formation_take_cover3.ogg"],
    "voice/crpg/incoming/female/arrows": ["voice/incoming/female/vc_1_incoming_arrowsF.ogg", "voice/incoming/female/vc_1_incoming_arrowsF1.ogg", "voice/incoming/female/vc_1_incoming_arrowsF2.ogg"],
    "voice/crpg/incoming/female/base": ["voice/incoming/female/vc_1_incoming_baseF.ogg", "voice/incoming/female/vc_1_incoming_baseF1.ogg", "voice/incoming/female/vc_1_incoming_baseF2.ogg", "voice/incoming/female/vc_1_incoming_baseF3.ogg"],
    "voice/crpg/incoming/female/catapult": ["voice/incoming/female/vc_1_incoming_catapultF.ogg", "voice/incoming/female/vc_1_incoming_catapultF1.ogg"],
    "voice/crpg/incoming/female/cavalry": ["voice/incoming/female/vc_1_incoming_cavalryF1.ogg"],
    "voice/crpg/incoming/female/cavalry_behind": ["voice/incoming/female/vc_1_incoming_cavalry_behindF.ogg", "voice/incoming/female/vc_1_incoming_cavalry_behindF1.ogg", "voice/incoming/female/vc_1_incoming_cavalry_behindF2.ogg"],
    "voice/crpg/incoming/female/cavalry_cavalry": ["voice/incoming/female/vc_1_incoming_cavalry_cavalryF.ogg"],
    "voice/crpg/incoming/female/cavalry_left": ["voice/incoming/female/vc_1_incoming_cavalry_leftF.ogg", "voice/incoming/female/vc_1_incoming_cavalry_leftF1.ogg"],
    "voice/crpg/incoming/female/cavalry_right": ["voice/incoming/female/vc_1_incoming_cavalry_rightF.ogg", "voice/incoming/female/vc_1_incoming_cavalry_rightF1.ogg"],
    "voice/crpg/incoming/female/infantry": ["voice/incoming/female/vc_1_incoming_infantryF1.ogg", "voice/incoming/female/vc_1_incoming_infantryF2.ogg", "voice/incoming/female/vc_1_incoming_infantryF3.ogg"],
    "voice/crpg/incoming/female/infantry_behind": ["voice/incoming/female/vc_1_incoming_infantry_behindF.ogg", "voice/incoming/female/vc_1_incoming_infantry_behindF1.ogg", "voice/incoming/female/vc_1_incoming_infantry_behindF2.ogg"],
    "voice/crpg/incoming/female/infantry_infantry": ["voice/incoming/female/vc_1_incoming_infantry_infantryF.ogg"],
    "voice/crpg/incoming/female/infantry_left": ["voice/incoming/female/vc_1_incoming_infantry_leftF.ogg", "voice/incoming/female/vc_1_incoming_infantry_leftF1.ogg", "voice/incoming/female/vc_1_incoming_infantry_leftF2.ogg"],
    "voice/crpg/incoming/female/infantry_right": ["voice/incoming/female/vc_1_incoming_infantry_rightF.ogg", "voice/incoming/female/vc_1_incoming_infantry_rightF1.ogg", "voice/incoming/female/vc_1_incoming_infantry_rightF2.ogg"],
    "voice/crpg/incoming/female/siege_tower": ["voice/incoming/female/vc_1_incoming_siege_towerF.ogg", "voice/incoming/female/vc_1_incoming_siege_towerF1.ogg"],
    "voice/crpg/incoming/male/arrows": ["voice/incoming/male/vc_1_incoming_arrows.ogg", "voice/incoming/male/vc_1_incoming_arrows1.ogg", "voice/incoming/male/vc_1_incoming_arrows2.ogg", "voice/incoming/male/vc_1_incoming_arrows3.ogg"],
    "voice/crpg/incoming/male/base": ["voice/incoming/male/vc_1_incoming_base.ogg", "voice/incoming/male/vc_1_incoming_base1.ogg", "voice/incoming/male/vc_1_incoming_base2.ogg", "voice/incoming/male/vc_1_incoming_base3.ogg"],
    "voice/crpg/incoming/male/catapult": ["voice/incoming/male/vc_1_incoming_catapult.ogg", "voice/incoming/male/vc_1_incoming_catapult1.ogg", "voice/incoming/male/vc_1_incoming_catapult2.ogg", "voice/incoming/male/vc_1_incoming_catapult3.ogg"],
    "voice/crpg/incoming/male/cavalry": ["voice/incoming/male/vc_1_incoming_cavalry1.ogg", "voice/incoming/male/vc_1_incoming_cavalry2.ogg", "voice/incoming/male/vc_1_incoming_cavalry3.ogg"],
    "voice/crpg/incoming/male/cavalry_behind": ["voice/incoming/male/vc_1_incoming_cavalry_behind.ogg", "voice/incoming/male/vc_1_incoming_cavalry_behind1.ogg", "voice/incoming/male/vc_1_incoming_cavalry_behind2.ogg", "voice/incoming/male/vc_1_incoming_cavalry_behind3.ogg"],
    "voice/crpg/incoming/male/cavalry_cavalry": ["voice/incoming/male/vc_1_incoming_cavalry_cavalry.ogg"],
    "voice/crpg/incoming/male/cavalry_left": ["voice/incoming/male/vc_1_incoming_cavalry_left.ogg", "voice/incoming/male/vc_1_incoming_cavalry_left1.ogg", "voice/incoming/male/vc_1_incoming_cavalry_left2.ogg", "voice/incoming/male/vc_1_incoming_cavalry_left3.ogg"],
    "voice/crpg/incoming/male/cavalry_right": ["voice/incoming/male/vc_1_incoming_cavalry_right.ogg", "voice/incoming/male/vc_1_incoming_cavalry_right1.ogg", "voice/incoming/male/vc_1_incoming_cavalry_right2.ogg", "voice/incoming/male/vc_1_incoming_cavalry_right3.ogg"],
    "voice/crpg/incoming/male/infantry": ["voice/incoming/male/vc_1_incoming_infantry1.ogg", "voice/incoming/male/vc_1_incoming_infantry2.ogg", "voice/incoming/male/vc_1_incoming_infantry3.ogg"],
    "voice/crpg/incoming/male/infantry_behind": ["voice/incoming/male/vc_1_incoming_infantry_behind.ogg", "voice/incoming/male/vc_1_incoming_infantry_behind1.ogg", "voice/incoming/male/vc_1_incoming_infantry_behind2.ogg", "voice/incoming/male/vc_1_incoming_infantry_behind3.ogg"],
    "voice/crpg/incoming/male/infantry_infantry": ["voice/incoming/male/vc_1_incoming_infantry_infantry.ogg"],
    "voice/crpg/incoming/male/infantry_left": ["voice/incoming/male/vc_1_incoming_infantry_left.ogg", "voice/incoming/male/vc_1_incoming_infantry_left1.ogg", "voice/incoming/male/vc_1_incoming_infantry_left2.ogg", "voice/incoming/male/vc_1_incoming_infantry_left3.ogg"],
    "voice/crpg/incoming/male/infantry_right": ["voice/incoming/male/vc_1_incoming_infantry_right.ogg", "voice/incoming/male/vc_1_incoming_infantry_right1.ogg", "voice/incoming/male/vc_1_incoming_infantry_right2.ogg"],
    "voice/crpg/incoming/male/siege_tower": ["voice/incoming/male/vc_1_incoming_siege_tower.ogg", "voice/incoming/male/vc_1_incoming_siege_tower1.ogg", "voice/incoming/male/vc_1_incoming_siege_tower2.ogg", "voice/incoming/male/vc_1_incoming_siege_tower3.ogg"],
    "voice/crpg/quick/female/hello": ["voice/quick/female/vc_1_quickmenu_helloF.ogg", "voice/quick/female/vc_1_quickmenu_helloF1.ogg", "voice/quick/female/vc_1_quickmenu_helloF2.ogg"],
    "voice/crpg/quick/female/help": ["voice/quick/female/vc_1_quickmenu_helpF.ogg"],
    "voice/crpg/quick/female/move": ["voice/quick/female/vc_1_quickmenu_moveF.ogg", "voice/quick/female/vc_1_quickmenu_moveF1.ogg", "voice/quick/female/vc_1_quickmenu_moveF2.ogg", "voice/quick/female/vc_1_quickmenu_moveF3.ogg"],
    "voice/crpg/quick/female/no": ["voice/quick/female/vc_1_quickmenu_noF1.ogg", "voice/quick/female/vc_1_quickmenu_noF2.ogg", "voice/quick/female/vc_1_quickmenu_noF3.ogg"],
    "voice/crpg/quick/female/silence": ["voice/quick/female/vc_1_quickmenu_silenceF.ogg", "voice/quick/female/vc_1_quickmenu_silenceF1.ogg", "voice/quick/female/vc_1_quickmenu_silenceF2.ogg", "voice/quick/female/vc_1_quickmenu_silenceF3.ogg"],
    "voice/crpg/quick/female/sorry": ["voice/quick/female/vc_1_quickmenu_sorryF.ogg", "voice/quick/female/vc_1_quickmenu_sorryF1.ogg", "voice/quick/female/vc_1_quickmenu_sorryF2.ogg", "voice/quick/female/vc_1_quickmenu_sorryF3.ogg"],
    "voice/crpg/quick/female/stop": ["voice/quick/female/vc_1_quickmenu_stopF.ogg", "voice/quick/female/vc_1_quickmenu_stopF1.ogg", "voice/quick/female/vc_1_quickmenu_stopF2.ogg", "voice/quick/female/vc_1_quickmenu_stopF3.ogg"],
    "voice/crpg/quick/female/thanks": ["voice/quick/female/vc_1_quickmenu_thanksF.ogg", "voice/quick/female/vc_1_quickmenu_thanksF1.ogg", "voice/quick/female/vc_1_quickmenu_thanksF2.ogg", "voice/quick/female/vc_1_quickmenu_thanksF3.ogg"],
    "voice/crpg/quick/female/watch_aim": ["voice/quick/female/vc_1_quickmenu_watch_aimF.ogg", "voice/quick/female/vc_1_quickmenu_watch_aimF1.ogg", "voice/quick/female/vc_1_quickmenu_watch_aimF2.ogg", "voice/quick/female/vc_1_quickmenu_watch_aimF3.ogg"],
    "voice/crpg/quick/female/yell": ["voice/quick/female/vc_1_quickmenu_yellF.ogg", "voice/quick/female/vc_1_quickmenu_yellF1.ogg", "voice/quick/female/vc_1_quickmenu_yellF10.ogg", "voice/quick/female/vc_1_quickmenu_yellF2.ogg", "voice/quick/female/vc_1_quickmenu_yellF3.ogg", "voice/quick/female/vc_1_quickmenu_yellF4.ogg", "voice/quick/female/vc_1_quickmenu_yellF5.ogg", "voice/quick/female/vc_1_quickmenu_yellF6.ogg", "voice/quick/female/vc_1_quickmenu_yellF7.ogg", "voice/quick/female/vc_1_quickmenu_yellF8.ogg", "voice/quick/female/vc_1_quickmenu_yellF9.ogg"],
    "voice/crpg/quick/female/yes": ["voice/quick/female/vc_1_quickmenu_yesF.ogg", "voice/quick/female/vc_1_quickmenu_yesF1.ogg", "voice/quick/female/vc_1_quickmenu_yesF2.ogg", "voice/quick/female/vc_1_quickmenu_yesF3.ogg"],
    "voice/crpg/quick/male/hello": ["voice/quick/male/vc_1_quickmenu_hello.ogg", "voice/quick/male/vc_1_quickmenu_hello1.ogg", "voice/quick/male/vc_1_quickmenu_hello2.ogg", "voice/quick/male/vc_1_quickmenu_hello3.ogg"],
    "voice/crpg/quick/male/help": ["voice/quick/male/vc_1_quickmenu_help.ogg", "voice/quick/male/vc_1_quickmenu_help1.ogg", "voice/quick/male/vc_1_quickmenu_help2.ogg", "voice/quick/male/vc_1_quickmenu_help3.ogg"],
    "voice/crpg/quick/male/move": ["voice/quick/male/vc_1_quickmenu_move.ogg", "voice/quick/male/vc_1_quickmenu_move1.ogg", "voice/quick/male/vc_1_quickmenu_move2.ogg", "voice/quick/male/vc_1_quickmenu_move3.ogg"],
    "voice/crpg/quick/male/no": ["voice/quick/male/vc_1_quickmenu_no.ogg", "voice/quick/male/vc_1_quickmenu_no1.ogg", "voice/quick/male/vc_1_quickmenu_no2.ogg", "voice/quick/male/vc_1_quickmenu_no3.ogg"],
    "voice/crpg/quick/male/silence": ["voice/quick/male/vc_1_quickmenu_silence.ogg", "voice/quick/male/vc_1_quickmenu_silence1.ogg", "voice/quick/male/vc_1_quickmenu_silence2.ogg", "voice/quick/male/vc_1_quickmenu_silence3.ogg"],
    "voice/crpg/quick/male/sorry": ["voice/quick/male/vc_1_quickmenu_sorry.ogg", "voice/quick/male/vc_1_quickmenu_sorry1.ogg", "voice/quick/male/vc_1_quickmenu_sorry2.ogg", "voice/quick/male/vc_1_quickmenu_sorry3.ogg"],
    "voice/crpg/quick/male/stop": ["voice/quick/male/vc_1_quickmenu_stop.ogg", "voice/quick/male/vc_1_quickmenu_stop1.ogg", "voice/quick/male/vc_1_quickmenu_stop2.ogg", "voice/quick/male/vc_1_quickmenu_stop3.ogg"],
    "voice/crpg/quick/male/thanks": ["voice/quick/male/vc_1_quickmenu_thanks.ogg", "voice/quick/male/vc_1_quickmenu_thanks1.ogg", "voice/quick/male/vc_1_quickmenu_thanks2.ogg", "voice/quick/male/vc_1_quickmenu_thanks3.ogg"],
    "voice/crpg/quick/male/watch_aim": ["voice/quick/male/vc_1_quickmenu_watch_aim.ogg", "voice/quick/male/vc_1_quickmenu_watch_aim1.ogg", "voice/quick/male/vc_1_quickmenu_watch_aim2.ogg", "voice/quick/male/vc_1_quickmenu_watch_aim3.ogg"],
    "voice/crpg/quick/male/yell": ["voice/quick/male/vc_1_quickmenu_yell.ogg", "voice/quick/male/vc_1_quickmenu_yell1.ogg", "voice/quick/male/vc_1_quickmenu_yell10.ogg", "voice/quick/male/vc_1_quickmenu_yell2.ogg", "voice/quick/male/vc_1_quickmenu_yell3.ogg", "voice/quick/male/vc_1_quickmenu_yell4.ogg", "voice/quick/male/vc_1_quickmenu_yell5.ogg", "voice/quick/male/vc_1_quickmenu_yell6.ogg", "voice/quick/male/vc_1_quickmenu_yell7.ogg", "voice/quick/male/vc_1_quickmenu_yell8.ogg", "voice/quick/male/vc_1_quickmenu_yell9.ogg"],
    "voice/crpg/quick/male/yes": ["voice/quick/male/vc_1_quickmenu_yes.ogg", "voice/quick/male/vc_1_quickmenu_yes1.ogg", "voice/quick/male/vc_1_quickmenu_yes2.ogg", "voice/quick/male/vc_1_quickmenu_yes3.ogg"],
    "voice/crpg/self/attack/female/attacking": ["voice/self/female/vc_1_self_attacking_attackingF.ogg", "voice/self/female/vc_1_self_attacking_attackingF1.ogg", "voice/self/female/vc_1_self_attacking_attackingF2.ogg", "voice/self/female/vc_1_self_attacking_attackingF3.ogg"],
    "voice/crpg/self/attack/female/base": ["voice/self/female/vc_1_self_attacking_baseF.ogg", "voice/self/female/vc_1_self_attacking_baseF1.ogg", "voice/self/female/vc_1_self_attacking_baseF2.ogg"],
    "voice/crpg/self/attack/female/catapult": ["voice/self/female/vc_1_self_attacking_catapultF.ogg", "voice/self/female/vc_1_self_attacking_catapultF1.ogg"],
    "voice/crpg/self/attack/female/equipment": ["voice/self/female/vc_1_self_attacking_equipmentF.ogg", "voice/self/female/vc_1_self_attacking_equipmentF1.ogg"],
    "voice/crpg/self/attack/female/flag": ["voice/self/female/vc_1_self_attacking_flagF.ogg", "voice/self/female/vc_1_self_attacking_flagF1.ogg", "voice/self/female/vc_1_self_attacking_flagF2.ogg"],
    "voice/crpg/self/attack/female/gate": ["voice/self/female/vc_1_self_attacking_gateF.ogg", "voice/self/female/vc_1_self_attacking_gateF1.ogg", "voice/self/female/vc_1_self_attacking_gateF2.ogg"],
    "voice/crpg/self/attack/female/left": ["voice/self/female/vc_1_self_attacking_leftF.ogg", "voice/self/female/vc_1_self_attacking_leftF1.ogg", "voice/self/female/vc_1_self_attacking_leftF2.ogg"],
    "voice/crpg/self/attack/female/right": ["voice/self/female/vc_1_self_attacking_rightF.ogg", "voice/self/female/vc_1_self_attacking_rightF1.ogg", "voice/self/female/vc_1_self_attacking_rightF2.ogg"],
    "voice/crpg/self/attack/female/siege_tower": ["voice/self/female/vc_1_self_attacking_siege_towerF.ogg", "voice/self/female/vc_1_self_attacking_siege_towerF1.ogg"],
    "voice/crpg/self/attack/female/tower": ["voice/self/female/vc_1_self_attacking_towerF.ogg", "voice/self/female/vc_1_self_attacking_towerF1.ogg", "voice/self/female/vc_1_self_attacking_towerF2.ogg"],
    "voice/crpg/self/attack/male/attacking": ["voice/self/male/vc_1_self_attacking_attacking.ogg", "voice/self/male/vc_1_self_attacking_attacking1.ogg", "voice/self/male/vc_1_self_attacking_attacking2.ogg", "voice/self/male/vc_1_self_attacking_attacking3.ogg"],
    "voice/crpg/self/attack/male/base": ["voice/self/male/vc_1_self_attacking_base.ogg", "voice/self/male/vc_1_self_attacking_base1.ogg", "voice/self/male/vc_1_self_attacking_base2.ogg", "voice/self/male/vc_1_self_attacking_base3.ogg"],
    "voice/crpg/self/attack/male/catapult": ["voice/self/male/vc_1_self_attacking_catapult.ogg", "voice/self/male/vc_1_self_attacking_catapult1.ogg", "voice/self/male/vc_1_self_attacking_catapult2.ogg", "voice/self/male/vc_1_self_attacking_catapult3.ogg"],
    "voice/crpg/self/attack/male/equipment": ["voice/self/male/vc_1_self_attacking_equipment.ogg", "voice/self/male/vc_1_self_attacking_equipment1.ogg", "voice/self/male/vc_1_self_attacking_equipment2.ogg", "voice/self/male/vc_1_self_attacking_equipment3.ogg"],
    "voice/crpg/self/attack/male/flag": ["voice/self/male/vc_1_self_attacking_flag.ogg", "voice/self/male/vc_1_self_attacking_flag1.ogg", "voice/self/male/vc_1_self_attacking_flag2.ogg", "voice/self/male/vc_1_self_attacking_flag3.ogg"],
    "voice/crpg/self/attack/male/gate": ["voice/self/male/vc_1_self_attacking_gate.ogg", "voice/self/male/vc_1_self_attacking_gate1.ogg", "voice/self/male/vc_1_self_attacking_gate2.ogg", "voice/self/male/vc_1_self_attacking_gate3.ogg"],
    "voice/crpg/self/attack/male/left": ["voice/self/male/vc_1_self_attacking_left.ogg", "voice/self/male/vc_1_self_attacking_left1.ogg", "voice/self/male/vc_1_self_attacking_left2.ogg", "voice/self/male/vc_1_self_attacking_left3.ogg"],
    "voice/crpg/self/attack/male/right": ["voice/self/male/vc_1_self_attacking_right.ogg", "voice/self/male/vc_1_self_attacking_right1.ogg", "voice/self/male/vc_1_self_attacking_right2.ogg", "voice/self/male/vc_1_self_attacking_right3.ogg"],
    "voice/crpg/self/attack/male/siege_tower": ["voice/self/male/vc_1_self_attacking_siege_tower.ogg", "voice/self/male/vc_1_self_attacking_siege_tower1.ogg", "voice/self/male/vc_1_self_attacking_siege_tower2.ogg", "voice/self/male/vc_1_self_attacking_siege_tower3.ogg"],
    "voice/crpg/self/attack/male/tower": ["voice/self/male/vc_1_self_attacking_tower.ogg", "voice/self/male/vc_1_self_attacking_tower1.ogg", "voice/self/male/vc_1_self_attacking_tower2.ogg", "voice/self/male/vc_1_self_attacking_tower3.ogg"],
    "voice/crpg/self/defend/female/catapult": ["voice/self/female/vc_1_self_defending_catapultF.ogg", "voice/self/female/vc_1_self_defending_catapultF1.ogg", "voice/self/female/vc_1_self_defending_catapultF2.ogg"],
    "voice/crpg/self/defend/female/close_gate": ["voice/self/female/vc_1_self_defending_close_gateF.ogg", "voice/self/female/vc_1_self_defending_close_gateF1.ogg", "voice/self/female/vc_1_self_defending_close_gateF2.ogg"],
    "voice/crpg/self/defend/female/flag": ["voice/self/female/vc_1_self_defending_flagF.ogg", "voice/self/female/vc_1_self_defending_flagF1.ogg", "voice/self/female/vc_1_self_defending_flagF2.ogg"],
    "voice/crpg/self/defend/female/gate": ["voice/self/female/vc_1_self_defending_gateF.ogg", "voice/self/female/vc_1_self_defending_gateF1.ogg", "voice/self/female/vc_1_self_defending_gateF2.ogg"],
    "voice/crpg/self/defend/female/gatehouse": ["voice/self/female/vc_1_self_defending_gatehouseF.ogg", "voice/self/female/vc_1_self_defending_gatehouseF1.ogg", "voice/self/female/vc_1_self_defending_gatehouseF2.ogg", "voice/self/female/vc_1_self_defending_gatehouseF3.ogg"],
    "voice/crpg/self/defend/female/ladders": ["voice/self/female/vc_1_self_defending_laddersF.ogg", "voice/self/female/vc_1_self_defending_laddersF1.ogg", "voice/self/female/vc_1_self_defending_laddersF2.ogg", "voice/self/female/vc_1_self_defending_laddersF3.ogg"],
    "voice/crpg/self/defend/female/siege_tower": ["voice/self/female/vc_1_self_defending_siege_towerF.ogg", "voice/self/female/vc_1_self_defending_siege_towerF1.ogg", "voice/self/female/vc_1_self_defending_siege_towerF2.ogg"],
    "voice/crpg/self/defend/female/stairs": ["voice/self/female/vc_1_self_defending_stairsF.ogg", "voice/self/female/vc_1_self_defending_stairsF1.ogg", "voice/self/female/vc_1_self_defending_stairsF2.ogg"],
    "voice/crpg/self/defend/female/tower": ["voice/self/female/vc_1_self_defending_towerF.ogg", "voice/self/female/vc_1_self_defending_towerF1.ogg", "voice/self/female/vc_1_self_defending_towerF2.ogg", "voice/self/female/vc_1_self_defending_towerF3.ogg"],
    "voice/crpg/self/defend/female/walls": ["voice/self/female/vc_1_self_defending_wallsF.ogg", "voice/self/female/vc_1_self_defending_wallsF1.ogg", "voice/self/female/vc_1_self_defending_wallsF2.ogg", "voice/self/female/vc_1_self_defending_wallsF3.ogg"],
    "voice/crpg/self/defend/male/catapult": ["voice/self/male/vc_1_self_defending_catapult.ogg", "voice/self/male/vc_1_self_defending_catapult1.ogg", "voice/self/male/vc_1_self_defending_catapult2.ogg"],
    "voice/crpg/self/defend/male/close_gate": ["voice/self/male/vc_1_self_defending_close_gate.ogg", "voice/self/male/vc_1_self_defending_close_gate1.ogg", "voice/self/male/vc_1_self_defending_close_gate2.ogg", "voice/self/male/vc_1_self_defending_close_gate3.ogg"],
    "voice/crpg/self/defend/male/flag": ["voice/self/male/vc_1_self_defending_flag.ogg", "voice/self/male/vc_1_self_defending_flag1.ogg", "voice/self/male/vc_1_self_defending_flag2.ogg", "voice/self/male/vc_1_self_defending_flag3.ogg"],
    "voice/crpg/self/defend/male/gate": ["voice/self/male/vc_1_self_defending_gate.ogg", "voice/self/male/vc_1_self_defending_gate1.ogg", "voice/self/male/vc_1_self_defending_gate2.ogg", "voice/self/male/vc_1_self_defending_gate3.ogg"],
    "voice/crpg/self/defend/male/gatehouse": ["voice/self/male/vc_1_self_defending_gatehouse.ogg", "voice/self/male/vc_1_self_defending_gatehouse1.ogg", "voice/self/male/vc_1_self_defending_gatehouse2.ogg", "voice/self/male/vc_1_self_defending_gatehouse3.ogg"],
    "voice/crpg/self/defend/male/ladders": ["voice/self/male/vc_1_self_defending_ladders.ogg", "voice/self/male/vc_1_self_defending_ladders1.ogg", "voice/self/male/vc_1_self_defending_ladders2.ogg", "voice/self/male/vc_1_self_defending_ladders3.ogg"],
    "voice/crpg/self/defend/male/siege_tower": ["voice/self/male/vc_1_self_defending_siege_tower.ogg", "voice/self/male/vc_1_self_defending_siege_tower1.ogg", "voice/self/male/vc_1_self_defending_siege_tower2.ogg", "voice/self/male/vc_1_self_defending_siege_tower3.ogg"],
    "voice/crpg/self/defend/male/stairs": ["voice/self/male/vc_1_self_defending_stairs.ogg", "voice/self/male/vc_1_self_defending_stairs1.ogg", "voice/self/male/vc_1_self_defending_stairs2.ogg", "voice/self/male/vc_1_self_defending_stairs3.ogg"],
    "voice/crpg/self/defend/male/tower": ["voice/self/male/vc_1_self_defending_tower.ogg", "voice/self/male/vc_1_self_defending_tower1.ogg", "voice/self/male/vc_1_self_defending_tower2.ogg", "voice/self/male/vc_1_self_defending_tower3.ogg"],
    "voice/crpg/self/defend/male/walls": ["voice/self/male/vc_1_self_defending_walls.ogg", "voice/self/male/vc_1_self_defending_walls1.ogg", "voice/self/male/vc_1_self_defending_walls2.ogg", "voice/self/male/vc_1_self_defending_walls3.ogg"],
    "voice/crpg/self/equipment/female/build_catapult": ["voice/self/female/vc_1_self_equipment_build_catapultF.ogg", "voice/self/female/vc_1_self_equipment_build_catapultF1.ogg", "voice/self/female/vc_1_self_equipment_build_catapultF2.ogg"],
    "voice/crpg/self/equipment/female/build_siege_tower": ["voice/self/female/vc_1_self_equipment_build_siege_towerF.ogg", "voice/self/female/vc_1_self_equipment_build_siege_towerF1.ogg"],
    "voice/crpg/self/equipment/female/equipment": ["voice/self/female/vc_1_self_equipment_equipmentF.ogg", "voice/self/female/vc_1_self_equipment_equipmentF1.ogg", "voice/self/female/vc_1_self_equipment_equipmentF2.ogg"],
    "voice/crpg/self/equipment/female/help_catapult": ["voice/self/female/vc_1_self_equipment_help_catapultF.ogg", "voice/self/female/vc_1_self_equipment_help_catapultF1.ogg", "voice/self/female/vc_1_self_equipment_help_catapultF2.ogg"],
    "voice/crpg/self/equipment/female/help_siege_tower": ["voice/self/female/vc_1_self_equipment_help_siege_towerF.ogg", "voice/self/female/vc_1_self_equipment_help_siege_towerF1.ogg"],
    "voice/crpg/self/equipment/female/ladders": ["voice/self/female/vc_1_self_equipment_laddersF.ogg", "voice/self/female/vc_1_self_equipment_laddersF1.ogg", "voice/self/female/vc_1_self_equipment_laddersF2.ogg"],
    "voice/crpg/self/equipment/female/man_catapult": ["voice/self/female/vc_1_self_equipment_man_catapultF.ogg", "voice/self/female/vc_1_self_equipment_man_catapultF1.ogg", "voice/self/female/vc_1_self_equipment_man_catapultF2.ogg"],
    "voice/crpg/self/equipment/female/man_siege_tower": ["voice/self/female/vc_1_self_equipment_man_siege_towerF.ogg", "voice/self/female/vc_1_self_equipment_man_siege_towerF1.ogg"],
    "voice/crpg/self/equipment/female/shields": ["voice/self/female/vc_1_self_equipment_shieldsF.ogg", "voice/self/female/vc_1_self_equipment_shieldsF1.ogg", "voice/self/female/vc_1_self_equipment_shieldsF2.ogg"],
    "voice/crpg/self/equipment/male/build_catapult": ["voice/self/male/vc_1_self_equipment_build_catapult.ogg", "voice/self/male/vc_1_self_equipment_build_catapult1.ogg", "voice/self/male/vc_1_self_equipment_build_catapult2.ogg", "voice/self/male/vc_1_self_equipment_build_catapult3.ogg"],
    "voice/crpg/self/equipment/male/build_siege_tower": ["voice/self/male/vc_1_self_equipment_build_siege_tower.ogg", "voice/self/male/vc_1_self_equipment_build_siege_tower1.ogg", "voice/self/male/vc_1_self_equipment_build_siege_tower2.ogg", "voice/self/male/vc_1_self_equipment_build_siege_tower3.ogg"],
    "voice/crpg/self/equipment/male/equipment": ["voice/self/male/vc_1_self_equipment_equipment.ogg", "voice/self/male/vc_1_self_equipment_equipment1.ogg", "voice/self/male/vc_1_self_equipment_equipment2.ogg", "voice/self/male/vc_1_self_equipment_equipment3.ogg"],
    "voice/crpg/self/equipment/male/help_catapult": ["voice/self/male/vc_1_self_equipment_help_catapult.ogg", "voice/self/male/vc_1_self_equipment_help_catapult1.ogg", "voice/self/male/vc_1_self_equipment_help_catapult2.ogg", "voice/self/male/vc_1_self_equipment_help_catapult3.ogg"],
    "voice/crpg/self/equipment/male/help_siege_tower": ["voice/self/male/vc_1_self_equipment_help_siege_tower.ogg", "voice/self/male/vc_1_self_equipment_help_siege_tower1.ogg", "voice/self/male/vc_1_self_equipment_help_siege_tower2.ogg", "voice/self/male/vc_1_self_equipment_help_siege_tower3.ogg"],
    "voice/crpg/self/equipment/male/ladders": ["voice/self/male/vc_1_self_equipment_ladders.ogg", "voice/self/male/vc_1_self_equipment_ladders1.ogg", "voice/self/male/vc_1_self_equipment_ladders2.ogg", "voice/self/male/vc_1_self_equipment_ladders3.ogg"],
    "voice/crpg/self/equipment/male/man_catapult": ["voice/self/male/vc_1_self_equipment_man_catapult.ogg", "voice/self/male/vc_1_self_equipment_man_catapult1.ogg", "voice/self/male/vc_1_self_equipment_man_catapult2.ogg", "voice/self/male/vc_1_self_equipment_man_catapult3.ogg"],
    "voice/crpg/self/equipment/male/man_siege_tower": ["voice/self/male/vc_1_self_equipment_man_siege_tower.ogg", "voice/self/male/vc_1_self_equipment_man_siege_tower1.ogg", "voice/self/male/vc_1_self_equipment_man_siege_tower2.ogg", "voice/self/male/vc_1_self_equipment_man_siege_tower3.ogg"],
    "voice/crpg/self/equipment/male/shields": ["voice/self/male/vc_1_self_equipment_shields.ogg", "voice/self/male/vc_1_self_equipment_shields1.ogg", "voice/self/male/vc_1_self_equipment_shields2.ogg", "voice/self/male/vc_1_self_equipment_shields3.ogg"]
};

studio.menu.addMenuItem({
    name: "cRPG\\Generate Voice Command Events",
    execute: function() {
        var stats = generateVCEvents();
        studio.ui.showModalDialog({
            windowTitle: "VC Event Generator",
            windowWidth: 400,
            windowHeight: 200,
            widgetType: studio.ui.widgetType.Layout,
            layout: studio.ui.layoutType.VBoxLayout,
            spacing: 8,
            items: [
                { widgetType: studio.ui.widgetType.Label, text: "Done!" },
                { widgetType: studio.ui.widgetType.Label, text: "Created: " + stats.created },
                { widgetType: studio.ui.widgetType.Label, text: "Skipped: " + stats.skipped },
                { widgetType: studio.ui.widgetType.Label, text: "Errors: " + stats.errors.length },
                { widgetType: studio.ui.widgetType.Label, text: stats.errors.length > 0 ? stats.errors.slice(0,5).join(", ") : "" },
                {
                    widgetType: studio.ui.widgetType.PushButton,
                    text: "OK",
                    onClicked: function() { this.closeDialog(); }
                }
            ]
        });
    }
});

function generateVCEvents() {
    var stats = { created: 0, skipped: 0, errors: [] };

    // --- Build audio file map: assetPath -> AudioFile ---
    var audioFileMap = {};
    studio.project.model.AudioFile.findInstances().forEach(function(af) {
        if (af.assetPath) audioFileMap[af.assetPath] = af;
    });

    // --- Look up shared constants ---
    var shared = {};

    // Parameter presets -> their internal parameter objects
    studio.project.model.ParameterPreset.findInstances().forEach(function(pp) {
        if (pp.name === 'Distance Voice Long') shared.paramDVL = pp.parameter;
        else if (pp.name === 'Occlusion')       shared.paramOcclusion = pp.parameter;
        else if (pp.name === 'isPlayer')         shared.paramIsPlayer = pp.parameter;
    });

    // Effect presets
    studio.project.model.EffectPreset.findInstances().forEach(function(ep) {
        if (ep.name === 'Voice Long Spatializer')    shared.fxSpatializer = ep;
        else if (ep.name === 'Voice Long Distance Send') shared.fxDistSend = ep;
        else if (ep.name === 'Voice Long Clean Send')    shared.fxCleanSend = ep;
    });

    // Mixer returns
    studio.project.model.MixerReturn.findInstances().forEach(function(mr) {
        if (mr.name === 'SlowMotionClean') shared.retSlowClean = mr;
        else if (mr.name === 'SlowMotion') shared.retSlow = mr;
    });

    // Bank and tags
    studio.project.model.Bank.findInstances().forEach(function(b) {
        if (b.id === '{7d4941b2-09af-4406-8ebb-1cd2d87d250e}') shared.bank = b;
    });
    var allTags = studio.project.model.Tag.findInstances();
    shared.tags = allTags.filter(function(t) {
        return t.id === '{97dcd2c4-d869-445a-8b51-99206300c848}' ||
               t.id === '{f2c9ecb2-e085-4ed8-8605-6cad262776ad}';
    });

    // Master bus output
    shared.masterOutput = studio.project.lookup('{e905d906-e187-494b-a49f-fa97761544ea}');

    // --- Process each event group ---
    for (var groupKey in EVENT_GROUPS) {
        var parts = groupKey.split('/');
        var eventName = parts[parts.length - 1];
        var folderPath = parts.slice(0, parts.length - 1).join('/');
        var filePaths = EVENT_GROUPS[groupKey];

        // Skip the existing example event
        if (groupKey === 'voice/crpg/self/attack/male/attack') {
            stats.skipped++;
            continue;
        }

        // Check if event already exists
        var eventLookupPath = 'event:/' + groupKey;
        if (studio.project.lookup(eventLookupPath)) {
            stats.skipped++;
            continue;
        }

        try {
            // Get or create the parent folder
            var folder = getOrCreateFolder(folderPath);
            if (!folder) {
                stats.errors.push('No folder: ' + folderPath);
                continue;
            }

            // Get/create audio file objects
            var audioFiles = [];
            filePaths.forEach(function(p) {
                var af = audioFileMap[p];
                if (!af) {
                    af = studio.project.create('AudioFile');
                    af.assetPath = p;
                    audioFileMap[p] = af;
                }
                audioFiles.push(af);
            });

            createVCEvent(eventName, folder, audioFiles, shared);
            stats.created++;
        } catch (e) {
            stats.errors.push(eventName + ': ' + e);
        }
    }

    return stats;
}

function getOrCreateFolder(folderPath) {
    var existing = studio.project.lookup('event:/' + folderPath);
    if (existing) return existing;

    var parts = folderPath.split('/');
    var folderName = parts[parts.length - 1];
    var parentPath = parts.slice(0, parts.length - 1).join('/');

    var parent = parentPath.length > 0 ? getOrCreateFolder(parentPath) : studio.project.workspace.masterEventFolder;
    if (!parent) return null;

    var folder = studio.project.create('EventFolder');
    folder.name = folderName;
    folder.folder = parent;
    return folder;
}

function createVCEvent(eventName, folder, audioFiles, shared) {
    var event = studio.project.create('Event');
    event.name = eventName;
    event.folder = folder;
    event.outputFormat = 2;

    // Automatable properties
    if (event.automatableProperties) {
        event.automatableProperties.voiceStealing = 2;
        event.automatableProperties.priority = 4;
        event.automatableProperties.maximumDistance = 450;
    }

    // User property: priorityMultiplier
    var userProp = studio.project.create('UserProperty');
    userProp.key = 'priorityMultiplier';
    userProp.value = 1.3;
    event.userProperties.add(userProp);

    // ParameterProxy: Distance Voice Long
    if (shared.paramDVL) {
        var proxyDVL = studio.project.create('ParameterProxy');
        proxyDVL.preset = shared.paramDVL;
        event.parameters.add(proxyDVL);
    }

    // ParameterProxy: Occlusion
    if (shared.paramOcclusion) {
        var proxyOcc = studio.project.create('ParameterProxy');
        proxyOcc.preset = shared.paramOcclusion;
        event.parameters.add(proxyOcc);
    }

    // Bank assignment
    if (shared.bank) event.banks.add(shared.bank);

    // Tags
    if (shared.tags) {
        shared.tags.forEach(function(tag) { event.tags.add(tag); });
    }

    // EventMixerMaster: volume
    if (event.mixer && event.mixer.masterBus) {
        event.mixer.masterBus.volume = -80;

        // Effects chain
        addEffectsChain(event, shared);
    }

    // MultiSound on masterTrack
    var maxLen = 0;
    audioFiles.forEach(function(af) {
        if (af.length && af.length > maxLen) maxLen = af.length;
    });

    var multiSound = studio.project.create('MultiSound');
    multiSound.length = maxLen > 0 ? maxLen : 2.0;
    event.masterTrack.modules.add(multiSound);

    audioFiles.forEach(function(af) {
        var singleSound = studio.project.create('SingleSound');
        singleSound.audioFile = af;
        multiSound.sounds.add(singleSound);
    });
}

function addEffectsChain(event, shared) {
    var bus = event.mixer.masterBus;

    // 1. Spatializer (no automation)
    if (shared.fxSpatializer) {
        var fx1 = studio.project.create('ProxyEffect');
        fx1.preset = shared.fxSpatializer;
        fx1.inputFormat = 1;
        bus.effectChain.effects.add(fx1);
    }

    // 2. Distance Send (automator: level driven by Distance Voice Long)
    if (shared.fxDistSend && shared.paramDVL) {
        var fx2 = studio.project.create('ProxyEffect');
        fx2.preset = shared.fxDistSend;
        fx2.inputFormat = 2;
        bus.effectChain.effects.add(fx2);
        addAutomator(fx2, shared.paramDVL, [
            { position: 0,   value: -80, curveShape: -0.794435918 },
            { position: 500, value: -80 },
            { position: 100, value: -24, curveShape: -0.100460663 }
        ]);
    }

    // 3. Clean Send (automator: level driven by Distance Voice Long)
    if (shared.fxCleanSend && shared.paramDVL) {
        var fx3 = studio.project.create('ProxyEffect');
        fx3.preset = shared.fxCleanSend;
        fx3.inputFormat = 2;
        bus.effectChain.effects.add(fx3);
        addAutomator(fx3, shared.paramDVL, [
            { position: 0,   value: 0 },
            { position: 2,   value: -6,  curveShape: -0.0862192065 },
            { position: 150, value: -80, curveShape: 0.265698314 }
        ]);
    }

    // 4. MixerSend -> SlowMotionClean (automator: level driven by isPlayer)
    if (shared.retSlowClean && shared.paramIsPlayer) {
        var ms1 = studio.project.create('MixerSend');
        ms1.inputFormat = 2;
        ms1.mixerReturn = shared.retSlowClean;
        bus.effectChain.effects.add(ms1);
        addAutomator(ms1, shared.paramIsPlayer, [
            { position: 0, value: -80 },
            { position: 1, value: 0 }
        ]);
    }

    // 5. MixerSend -> SlowMotion (automator: level driven by isPlayer)
    if (shared.retSlow && shared.paramIsPlayer) {
        var ms2 = studio.project.create('MixerSend');
        ms2.inputFormat = 2;
        ms2.mixerReturn = shared.retSlow;
        bus.effectChain.effects.add(ms2);
        addAutomator(ms2, shared.paramIsPlayer, [
            { position: 0, value: 0 },
            { position: 1, value: -80 }
        ]);
    }

    // 6. Fader
    var fader = studio.project.create('MixerBusFader');
    bus.effectChain.effects.add(fader);
}

function addAutomator(owner, paramObj, pointDefs) {
    var automator = studio.project.create('Automator');
    automator.nameOfPropertyBeingAutomated = 'level';
    owner.automators.add(automator);

    var curve = studio.project.create('AutomationCurve');
    curve.parameter = paramObj;
    automator.automationCurves.add(curve);

    pointDefs.forEach(function(def) {
        var pt = studio.project.create('AutomationPoint');
        pt.position = def.position;
        pt.value = def.value;
        if (def.curveShape !== undefined) pt.curveShape = def.curveShape;
        curve.automationPoints.add(pt);
    });
}
