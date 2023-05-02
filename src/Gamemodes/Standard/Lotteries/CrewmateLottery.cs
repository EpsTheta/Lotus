using System.Linq;
using TOHTOR.Factions.Crew;
using TOHTOR.Managers;
using TOHTOR.Roles;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Gamemodes.Standard.Lotteries;

public class CrewmateLottery: RoleLottery
{
    public CrewmateLottery() : base(CustomRoleManager.Static.Crewmate)
    {
        CustomRoleManager.AllRoles.Where(r => r.Faction is Crewmates).ForEach(r => AddRole(r));
    }
}