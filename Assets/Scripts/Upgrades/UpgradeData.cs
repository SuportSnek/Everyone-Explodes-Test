using UnityEngine;

public enum UpgradeType {
    RunFast,
    JumpHigh,
    
    ThrowStrength,
    ExplRadius,
    FarExplRadius,
    noGrav,

    theSniper,
    explStrength,
    crazedBomber,
    hugger,
    fastBallJuice,
    combo,
    KzarosRing,

    cooldown,
    slot1Cooldown,
    slot4TsarBoomba,
    MasochismUp,
    indecisive,

    angryBuildings,
    engineeringDegree,

    buildingLord,
    midAir,
    Storm,
    PowerOfWill,

    SpeedScaler,
    SizeUp,
    SizeDown,
    StickyBomb,
    SpiralNades,
    Luck,
    DoubleJump,
    FiftyFifty,
}

//An enum (short for enumeration) is a special C# type that lets you define a set of named constant values.
//This defines a new type called UpgradeType.
//Now variables can be of that type: UpgradeType myUpgrade = UpgradeType.RunFast;
//Enums are extremely useful whenever you have a list of predefined options.

//struct, interface, enum and delagate are all allowed outside of the class. You can do:
//public enum Fruit { Apple, Orange, Banana }
//public class Tree { }
//public struct Stats { }
//public interface IAnimal { }

[CreateAssetMenu(menuName = "Upgrades/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public UpgradeType type;
    public string UpgradeName;
    public string Description;
    public Sprite icon;
    public GameObject UpgradeMenuUIPrefab;

    //[Header("Spawning Upgrades")]
    //public float Strength;
    //public float explosionRadius;
    //public float visualRadiusMultiplier;
}
