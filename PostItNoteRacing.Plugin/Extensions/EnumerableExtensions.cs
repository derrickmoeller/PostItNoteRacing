using GameReaderCommon;
using PostItNoteRacing.Plugin.Models;
using System.Collections.Generic;
using System.Linq;

namespace PostItNoteRacing.Plugin.Extensions
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<Team> GetAbsent(this IEnumerable<Team> list, IEnumerable<Opponent> opponents, Game game)
        {
            switch (game.Name)
            {
                case "AssettoCorsaCompetizione":
                case "IRacing":
                    return list.Where(x => opponents.Select(y => y.CarNumber).Contains(x.CarNumber) == false);
                default:
                    return list.Where(x => opponents.Select(y => y.Name).Contains(x.ActiveDriver.Name) == false);
            }
        }

        public static Opponent GetUnique(this IEnumerable<Opponent> list, Team team, Game game)
        {
            switch (game.Name)
            {
                case "AssettoCorsaCompetizione":
                case "IRacing":
                    return list.SingleOrDefault(x => x.CarNumber == team.CarNumber);
                default:
                    return list.SingleOrDefault(x => x.Name == team.ActiveDriver.Name);
            }
        }

        public static Team GetUnique(this IEnumerable<Team> list, Opponent opponent, Game game)
        {
            switch (game.Name)
            {
                case "AssettoCorsaCompetizione":
                case "IRacing":
                    return list.SingleOrDefault(x => x.CarNumber == opponent.CarNumber);
                default:
                    return list.SingleOrDefault(x => x.ActiveDriver.Name == opponent.Name);
            }
        }
    }
}
