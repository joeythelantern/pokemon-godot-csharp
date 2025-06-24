using System;
using Game.Core;
using Game.Gameplay;

namespace Game.Utilities
{
    public static class Stats
    {
        public static PlayerPokemonStats GetAllStats(PokemonResource pokemonResource, int level)
        {
            int[] ivs = GenerateIVs();

            return new()
            {
                CurrentHP = HpCalculator(pokemonResource.BaseHP, ivs[0], 0, level),
                Attack = HpCalculator(pokemonResource.BaseHP, ivs[1], 0, level),
                Defence = HpCalculator(pokemonResource.BaseHP, ivs[2], 0, level),
                SpecialAttack = HpCalculator(pokemonResource.BaseHP, ivs[3], 0, level),
                SpecialDefence = HpCalculator(pokemonResource.BaseHP, ivs[4], 0, level),
                Speed = HpCalculator(pokemonResource.BaseHP, ivs[5], 0, level),
            };
        }

        public static int HpCalculator(int baseStat, int ivStat, int evStat, int level)
        {
            return (int)(((2 * baseStat + ivStat + (evStat / 4.0)) * level / 100.0) + level + 10);
        }

        public static int StatCalculator(int baseStat, int ivStat, int evStat, int level, Nature nature, Stat stat)
        {
            return (int)((((2 * baseStat + ivStat + (evStat / 4.0)) * level / 100.0) + 5) * GetNatureMultiplier(nature, stat));
        }

        public static double GetNatureMultiplier(Nature nature, Stat stat)
        {
            switch (nature)
            {
                case Nature.Lonely:
                    if (stat == Stat.Attack) return 1.1;
                    if (stat == Stat.Defence) return 0.9;
                    break;
                case Nature.Brave:
                    if (stat == Stat.Attack) return 1.1;
                    if (stat == Stat.Speed) return 0.9;
                    break;
                case Nature.Adamant:
                    if (stat == Stat.Attack) return 1.1;
                    if (stat == Stat.SpecialAttack) return 0.9;
                    break;
                case Nature.Naughty:
                    if (stat == Stat.Attack) return 1.1;
                    if (stat == Stat.SpecialDefence) return 0.9;
                    break;

                case Nature.Bold:
                    if (stat == Stat.Defence) return 1.1;
                    if (stat == Stat.Attack) return 0.9;
                    break;
                case Nature.Relaxed:
                    if (stat == Stat.Defence) return 1.1;
                    if (stat == Stat.Speed) return 0.9;
                    break;
                case Nature.Impish:
                    if (stat == Stat.Defence) return 1.1;
                    if (stat == Stat.SpecialAttack) return 0.9;
                    break;
                case Nature.Lax:
                    if (stat == Stat.Defence) return 1.1;
                    if (stat == Stat.SpecialDefence) return 0.9;
                    break;

                case Nature.Timid:
                    if (stat == Stat.Speed) return 1.1;
                    if (stat == Stat.Attack) return 0.9;
                    break;
                case Nature.Hasty:
                    if (stat == Stat.Speed) return 1.1;
                    if (stat == Stat.Defence) return 0.9;
                    break;
                case Nature.Jolly:
                    if (stat == Stat.Speed) return 1.1;
                    if (stat == Stat.SpecialAttack) return 0.9;
                    break;
                case Nature.Naive:
                    if (stat == Stat.Speed) return 1.1;
                    if (stat == Stat.SpecialDefence) return 0.9;
                    break;

                case Nature.Modest:
                    if (stat == Stat.SpecialAttack) return 1.1;
                    if (stat == Stat.Attack) return 0.9;
                    break;
                case Nature.Mild:
                    if (stat == Stat.SpecialAttack) return 1.1;
                    if (stat == Stat.Defence) return 0.9;
                    break;
                case Nature.Quiet:
                    if (stat == Stat.SpecialAttack) return 1.1;
                    if (stat == Stat.Speed) return 0.9;
                    break;
                case Nature.Rash:
                    if (stat == Stat.SpecialAttack) return 1.1;
                    if (stat == Stat.SpecialDefence) return 0.9;
                    break;

                case Nature.Calm:
                    if (stat == Stat.SpecialDefence) return 1.1;
                    if (stat == Stat.Attack) return 0.9;
                    break;
                case Nature.Gentle:
                    if (stat == Stat.SpecialDefence) return 1.1;
                    if (stat == Stat.Defence) return 0.9;
                    break;
                case Nature.Sassy:
                    if (stat == Stat.SpecialDefence) return 1.1;
                    if (stat == Stat.Speed) return 0.9;
                    break;
                case Nature.Careful:
                    if (stat == Stat.SpecialDefence) return 1.1;
                    if (stat == Stat.SpecialAttack) return 0.9;
                    break;

                case Nature.Hardy:
                case Nature.Docile:
                case Nature.Bashful:
                case Nature.Quirky:
                    return 1.0;

                default:
                    return 1.0;
            }

            return 1.0;
        }

        public static int[] GenerateIVs()
        {
            Random random = new Random();
            int[] ivs = new int[6];

            for (int i = 0; i < ivs.Length; i++)
            {
                ivs[i] = random.Next(0, 32); // Generates a random number between 0 and 31 inclusive
            }

            return ivs;
        }

        public static int GetHighestIVIndex(int[] ivs)
        {
            int maxIndex = 0;
            for (int i = 1; i < ivs.Length; i++)
            {
                if (ivs[i] > ivs[maxIndex])
                    maxIndex = i;
            }
            return maxIndex;
        }

        public static string GetCharacteristic(int iv, Stat stat)
        {
            string[][] characteristics =
            [
            ["Loves to eat", "Proud of its power", "Sturdy body", "Likes to run", "Highly curious", "Strong willed"],
            ["Takes plenty of siestas", "Likes to thrash about", "Capable of taking hits", "Alert to sounds", "Mischievous", "Somewhat vain"],
            ["Nods off a lot", "A little quick tempered", "Highly persistent", "Impetuous and silly", "Thoroughly cunning", "Strongly defiant"],
            ["Scatters things often", "Likes to fight", "Good endurance", "Somewhat of a clown", "Often lost in thought", "Hates to lose"],
            ["Likes to relax", "Quick tempered", "Good perseverance", "Quick to flee", "Very finicky", "Somewhat stubborn"]
            ];

            int characteristicRow = iv % 5;

            return characteristics[characteristicRow][(int)stat];
        }

        public static int GetExperienceRequiredForLevel(ExperienceGroup experienceGroup, int level)
        {
            switch (experienceGroup)
            {
                case ExperienceGroup.MediumFast:
                    return (int)Math.Pow(level, 3);
                case ExperienceGroup.Eratic:
                    return level switch
                    {
                        < 50 => (int)(Math.Pow(level, 3) * (100 - level) / 50.0),
                        >= 50 and < 68 => (int)(Math.Pow(level, 3) * (150 - level) / 100.0),
                        >= 68 and < 98 => (int)(Math.Pow(level, 3) * Math.Floor((1911 - 10 * level) / 3.0) / 500.0),
                        _ => (int)(Math.Pow(level, 3) * (160 - level) / 100.0)
                    };
                case ExperienceGroup.Fluctuating:
                    return level switch
                    {
                        < 15 => (int)(Math.Pow(level, 3) * (Math.Floor((level + 1) / 3.0) + 24) / 50.0),
                        >= 15 and < 36 => (int)(Math.Pow(level, 3) * (level + 14) / 50.0),
                        _ => (int)(Math.Pow(level, 3) * (Math.Floor(level / 2.0) + 32) / 50.0)
                    };
                case ExperienceGroup.MediumSlow:
                    return (int)(6.0 / 5.0 * Math.Pow(level, 3) - 15 * Math.Pow(level, 2) + 100 * level - 140.0);
                case ExperienceGroup.Fast:
                    return (int)(4.0 * Math.Pow(level, 3) / 5.0);
                case ExperienceGroup.Slow:
                    return (int)(5.0 * Math.Pow(level, 3) / 4.0);
            }

            return 0;
        }

        public static int GetExperienceForNextLevel(ExperienceGroup experienceGroup, int currentLevel, int nextLevel)
        {
            return GetExperienceRequiredForLevel(experienceGroup, nextLevel) - GetExperienceRequiredForLevel(experienceGroup, currentLevel);
        }
    }
}