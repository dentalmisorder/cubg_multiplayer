using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName="New Skill", menuName="Skills")]
public class Skill : ScriptableObject
{
    [Tooltip("Название умения")] public string skillName;
    [Tooltip("Описание умения")] public string skillDescription;
    [Tooltip("Прирост в размерах который дает активация")] public float skillScale;
    [Tooltip("Прирост в скорости который дает активация")] public float skillSpeed;
    [Tooltip("Умножение в размерах в виде стрика")] public float skillScaleMultiplayer = 1;
    [Tooltip("Активириуется когда сьел количество игроков")] public int skillKilledTask = 0; 
    [Tooltip("КД скилла, время после применения которого нужно ждать")] public int skillCooldown;
    [Space, Tooltip("Если же у скилла нет никакого бафа-стойки, он просто кастуется сразу и идет в откат, если же есть то КД начнется в конце стойки")] public bool immediateCast;
    [Tooltip("Cколько длится состояние бафа/на сколько времени даются статы (если 0 = навсегда)")] public float buffStateTime;
    [Space, Tooltip("Активируемый ли скилл или нет")] public bool isPassive;
    [Tooltip("Иконка умения")] public Sprite skillIcon;

    [HideInInspector] public bool isBuffTime = false;
    [HideInInspector] public bool isPassiveActive = true;
    [HideInInspector] public Button button;
}
