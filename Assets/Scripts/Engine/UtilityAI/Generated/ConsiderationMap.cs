// Automatically generated, do not modify by hand

using System;namespace UtilityAI{
public static class ConsiderationMap {
public enum Types : byte{
Boolean,
Cooldown,
Property,
Repeats,
Target_Distance,
Target_Score,
Target_AccessTag_All,
Target_Health,
Target_IsAlive,
Stamina,
HasAssignment,
IsEngaged,
}

public static ConsiderationScoreDelegate Boolean = CustomConsiderations.BooleanScore;
public static ConsiderationScoreDelegate Cooldown = UtilityAI.DefaultConsiderations.CooldownScore;
public static ConsiderationScoreDelegate Property = CustomConsiderations.PropertyScore;
public static ConsiderationScoreDelegate Repeats = UtilityAI.DefaultConsiderations.RepeatsScore;
public static ConsiderationScoreDelegate Target_Distance = UtilityAI.DefaultConsiderations.TargetDistanceScore;
public static ConsiderationScoreDelegate Target_Score = UtilityAI.DefaultConsiderations.TargetScore;
public static ConsiderationScoreDelegate Target_AccessTag_All = CustomConsiderations.TargetAccessTag;
public static ConsiderationScoreDelegate Target_Health = UtilityAI.DefaultConsiderations.TargetHealthScore;
public static ConsiderationScoreDelegate Target_IsAlive = UtilityAI.DefaultConsiderations.TargetHealthAliveScore;
public static ConsiderationScoreDelegate Stamina = UtilityAI.DefaultConsiderations.StaminaScore;
public static ConsiderationScoreDelegate HasAssignment = UtilityAI.DefaultConsiderations.AssignmentScore;
public static ConsiderationScoreDelegate IsEngaged = UtilityAI.DefaultConsiderations.EngagedScore;


public static bool IsCachable(Types name){
return false;
}

public static ConsiderationScoreDelegate Get(Types name){
switch (name){
case Types.Boolean: return Boolean;
case Types.Cooldown: return Cooldown;
case Types.Property: return Property;
case Types.Repeats: return Repeats;
case Types.Target_Distance: return Target_Distance;
case Types.Target_Score: return Target_Score;
case Types.Target_AccessTag_All: return Target_AccessTag_All;
case Types.Target_Health: return Target_Health;
case Types.Target_IsAlive: return Target_IsAlive;
case Types.Stamina: return Stamina;
case Types.HasAssignment: return HasAssignment;
case Types.IsEngaged: return IsEngaged;
}
return null;
}

public static ParametersType GetParametersType(Types name){
switch (name){
case Types.Boolean: return ParametersType.Property | ParametersType.Boolean;
case Types.Cooldown: return ParametersType.Value;
case Types.Property: return ParametersType.Property;
case Types.Repeats: return ParametersType.Range;
case Types.Target_Distance: return ParametersType.Range;
case Types.Target_Score: return ParametersType.None;
case Types.Target_AccessTag_All: return ParametersType.Boolean;
case Types.Target_Health: return ParametersType.None;
case Types.Target_IsAlive: return ParametersType.Boolean;
case Types.Stamina: return ParametersType.None;
case Types.HasAssignment: return ParametersType.Ability;
case Types.IsEngaged: return ParametersType.Boolean;
}
return ParametersType.None;
}
}}